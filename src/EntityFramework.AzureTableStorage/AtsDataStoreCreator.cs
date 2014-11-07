﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.AzureTableStorage.Requests;
using Microsoft.Data.Entity.AzureTableStorage.Utilities;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Storage;

namespace Microsoft.Data.Entity.AzureTableStorage
{
    public class AtsDataStoreCreator : DataStoreCreator
    {
        private readonly AtsConnection _connection;

        /// <summary>
        ///     This constructor is intended only for use when creating test doubles that will override members
        ///     with mocked or faked behavior. Use of this constructor for other purposes may result in unexpected
        ///     behavior including but not limited to throwing <see cref="NullReferenceException" />.
        /// </summary>
        protected AtsDataStoreCreator()
        {
        }

        public AtsDataStoreCreator([NotNull] AtsConnection connection)
        {
            Check.NotNull(connection, "connection");

            _connection = connection;
        }

        public override bool EnsureDeleted(IModel model)
        {
            Check.NotNull(model, "model");

            var deleted = false;
            foreach (var type in model.EntityTypes)
            {
                var request = new DeleteTableRequest(new AtsTable(type.AzureTableStorage().Table));
                deleted |= _connection.ExecuteRequest(request);
            }

            return deleted;
        }

        public override async Task<bool> EnsureDeletedAsync(IModel model, CancellationToken cancellationToken = default(CancellationToken))
        {
            Check.NotNull(model, "model");

            var tasks = model.EntityTypes
                .Select(type => new DeleteTableRequest(new AtsTable(type.AzureTableStorage().Table)))
                .Select(request => _connection.ExecuteRequestAsync(request, cancellationToken: cancellationToken))
                .ToList();

            var deleted = await Task.WhenAll(tasks).WithCurrentCulture();

            return deleted.Any(d => d);
        }

        public override bool EnsureCreated(IModel model)
        {
            Check.NotNull(model, "model");

            var created = false;
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var type in model.EntityTypes)
            {
                var request = new CreateTableRequest(new AtsTable(type.AzureTableStorage().Table));
                created |= _connection.ExecuteRequest(request);
            }

            return created;
        }

        public override async Task<bool> EnsureCreatedAsync(IModel model, CancellationToken cancellationToken = default(CancellationToken))
        {
            Check.NotNull(model, "model");

            var tasks = model.EntityTypes
                .Select(type => new CreateTableRequest(new AtsTable(type.AzureTableStorage().Table)))
                .Select(request => _connection.ExecuteRequestAsync(request, cancellationToken: cancellationToken))
                .ToList();

            var created = await Task.WhenAll(tasks).WithCurrentCulture();

            return created.Any(c => c);
        }
    }
}
