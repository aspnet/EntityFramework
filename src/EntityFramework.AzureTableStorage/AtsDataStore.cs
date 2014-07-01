﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.AzureTableStorage.Adapters;
using Microsoft.Data.Entity.AzureTableStorage.Interfaces;
using Microsoft.Data.Entity.AzureTableStorage.Query;
using Microsoft.Data.Entity.AzureTableStorage.Utilities;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Remotion.Linq;

namespace Microsoft.Data.Entity.AzureTableStorage
{
    public class AtsDataStore : DataStore
    {
        private readonly AtsQueryFactory _queryFactory;
        protected readonly AtsConnection Connection;
        internal TableEntityAdapterFactory EntityFactory;
        private const int MaxBatchOperations = 100;

        /// <summary>
        ///     Provided only for testing purposes. Do not use.
        /// </summary>
        protected AtsDataStore(AtsConnection connection, TableEntityAdapterFactory entityFactory)
        {
            Connection = connection;
            EntityFactory = entityFactory;
        }

        public AtsDataStore([NotNull] DbContextConfiguration configuration,
            [NotNull] AtsConnection connection,
            [NotNull] AtsQueryFactory queryFactory,
            [NotNull] TableEntityAdapterFactory tableEntityFactory)
            : base(configuration)
        {
            Check.NotNull(connection, "connection");
            Check.NotNull(queryFactory, "queryFactory");
            Check.NotNull(tableEntityFactory, "tableEntityFactory");

            _queryFactory = queryFactory;
            EntityFactory = tableEntityFactory;
            Connection = connection;
        }

        public override Task<int> SaveChangesAsync(IReadOnlyList<StateEntry> stateEntries, CancellationToken cancellationToken = new CancellationToken())
        {
            if (Connection.Batching)
            {
                return ExecuteBatchedChangesAsync(stateEntries, cancellationToken);
            }
            return ExecuteChangesAsync(stateEntries, cancellationToken);
        }

        public override IEnumerable<TResult> Query<TResult>(QueryModel queryModel, StateManager stateManager)
        {
            Check.NotNull(queryModel, "queryModel");
            Check.NotNull(stateManager, "stateManager");

            var compilationContext = _queryFactory.MakeCompilationContext(Model);
            var queryExecutor = compilationContext.CreateQueryModelVisitor().CreateQueryExecutor<TResult>(queryModel);
            var queryContext = _queryFactory.MakeQueryContext(Model, Logger, stateManager, Connection);
            return queryExecutor(queryContext, null);
        }

        public override IAsyncEnumerable<TResult> AsyncQuery<TResult>(QueryModel queryModel, StateManager stateManager)
        {
            Check.NotNull(queryModel, "queryModel");
            Check.NotNull(stateManager, "stateManager");

            // TODO This should happen properly async
            return Query<TResult>(queryModel, stateManager).ToAsyncEnumerable();
        }

        private async Task<int> ExecuteChangesAsync(IReadOnlyList<StateEntry> stateEntries, CancellationToken cancellationToken = default(CancellationToken))
        {
            Check.NotNull(stateEntries, "stateEntries");

            cancellationToken.ThrowIfCancellationRequested();
            var tableGroups = stateEntries.GroupBy(s => s.EntityType);
            var allTasks = new List<Task<ITableResult>>();
            foreach (var tableGroup in tableGroups)
            {
                var table = Connection.GetTableReference(tableGroup.Key.TableName());
                var tasks = tableGroup.Select(entry => GetOperation(entry, EntityFactory.CreateFromStateEntry(entry)))
                    .TakeWhile(operation => !cancellationToken.IsCancellationRequested)
                    .Select(operation => table.ExecuteAsync(operation, cancellationToken));
                allTasks.AddRange(tasks);

                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
            }
            await Task.WhenAll(allTasks);
            return InspectResults(allTasks);
        }

        protected int InspectResults(IList<Task<ITableResult>> tasks)
        {
            return CountTableResults(tasks, task =>
                {
                    if (task.Result.HttpStatusCode >= HttpStatusCode.BadRequest)
                    {
                        throw new DbUpdateException("Could not add entity: " + task.Result);
                    }
                    return 1;
                });
        }

        private async Task<int> ExecuteBatchedChangesAsync(IReadOnlyList<StateEntry> stateEntries,
            CancellationToken cancellationToken = new CancellationToken())
        {
            cancellationToken.ThrowIfCancellationRequested();
            var tableGroups = stateEntries.GroupBy(s => s.EntityType.TableName());
            var allBatchTasks = new List<Task<IList<ITableResult>>>();

            foreach (var tableGroup in tableGroups)
            {
                var table = Connection.GetTableReference(tableGroup.Key);
                var partitionGroups = tableGroup.GroupBy(s =>
                    {
                        var property = s.EntityType.GetPropertyByStorageName("PartitionKey");
                        return s[property];
                    }
                    );
                foreach (var partitionGroup in partitionGroups)
                {
                    var batch = new TableBatchOperation();
                    foreach (var operation in partitionGroup
                        .Select(entry => GetOperation(entry, EntityFactory.CreateFromStateEntry(entry)))
                        .Where(operation => operation != null)
                        )
                    {
                        // TODO allow user access to config options: Retry Policy, Secondary Storage, Timeout 
                        batch.Add(operation);
                        if (batch.Count >= MaxBatchOperations)
                        {
                            allBatchTasks.Add(table.ExecuteBatchAsync(batch, cancellationToken));
                            batch = new TableBatchOperation();
                        }
                    }
                    if (batch.Count != 0)
                    {
                        allBatchTasks.Add(table.ExecuteBatchAsync(batch, cancellationToken));
                    }
                }
            }
            await Task.WhenAll(allBatchTasks);
            return InspectBatchResults(allBatchTasks);
        }

        protected int InspectBatchResults(IList<Task<IList<ITableResult>>> arg)
        {
            return CountTableResults(arg, task =>
                {
                    var failedResult = task.Result.FirstOrDefault(result => result.HttpStatusCode >= HttpStatusCode.BadRequest);
                    if (failedResult != default(ITableResult))
                    {
                        throw new DbUpdateException("Could not add entity: " + failedResult.Result);
                    }
                    return task.Result.Count;
                });
        }

        protected int CountTableResults<TTask>(IList<Task<TTask>> tasks, Func<Task<TTask>, int> inspect)
        {
            var failedTask = tasks.FirstOrDefault(t => t.Exception != null);
            if (failedTask != null
                && failedTask.Exception != null)
            {
                throw failedTask.Exception;
            }
            //TODO identify failed tasks and their associated identity: return to user.
            return tasks.Aggregate(0, (current, task) => current + inspect(task));
        }

        protected TableOperation GetOperation(StateEntry entry, ITableEntity entity)
        {
            switch (entry.EntityState)
            {
                case EntityState.Added:
                    return TableOperation.Insert(entity);

                case EntityState.Deleted:
                    entity.ETag = entity.ETag ?? "*"; // TODO use ETag for concurrency checks
                    return TableOperation.Delete(entity);

                case EntityState.Modified:
                    entity.ETag = entity.ETag ?? "*";
                    return TableOperation.Replace(entity);

                case EntityState.Unchanged:
                case EntityState.Unknown:
                    return null;

                default:
                    throw new ArgumentOutOfRangeException("entry", "Unknown entity state");
            }
        }
    }
}
