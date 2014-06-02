﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Migrations;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.SQLite.Utilities;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.SQLite;

namespace Microsoft.Data.Entity.SQLite
{
    public class SQLiteDataStoreCreator : RelationalDataStoreCreator
    {
        private const int SQLITE_CANTOPEN = 14;

        private readonly SQLiteConnectionConnection _connection;
        private readonly SqlStatementExecutor _executor;
        private readonly SQLiteMigrationOperationSqlGenerator _generator;
        private readonly ModelDiffer _modelDiffer;

        public SQLiteDataStoreCreator(
            [NotNull] SQLiteConnectionConnection connection,
            [NotNull] SqlStatementExecutor executor,
            [NotNull] SQLiteMigrationOperationSqlGenerator generator,
            [NotNull] ModelDiffer modelDiffer)
        {
            Check.NotNull(connection, "connection");
            Check.NotNull(executor, "executor");
            Check.NotNull(generator, "generator");
            Check.NotNull(modelDiffer, "modelDiffer");

            _connection = connection;
            _executor = executor;
            _generator = generator;
            _modelDiffer = modelDiffer;
        }

        public override void Create()
        {
            using (var connection = _connection.CreateConnectionReadWriteCreate())
            {
                connection.Open();
            }
        }

        public override Task CreateAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            Create();

            return Task.FromResult(0);
        }

        public override void CreateTables(IModel model)
        {
            Check.NotNull(model, "model");

            var operations = _modelDiffer.DiffSource(model);
            var statements = _generator.Generate(operations, generateIdempotentSql: false);

            // TODO: Delete database on error
            using (var connection = _connection.CreateConnectionReadWrite())
            {
                _executor.ExecuteNonQuery(connection, statements);
            }
        }

        public override Task CreateTablesAsync(IModel model, CancellationToken cancellationToken = default(CancellationToken))
        {
            CreateTables(model);

            return Task.FromResult(0);
        }

        public override bool Exists()
        {
            try
            {
                using (var connection = _connection.CreateConnectionReadOnly())
                {
                    connection.Open();
                    connection.Close();
                }

                return true;
            }
            catch (SQLiteException ex)
            {
                if (ex.ErrorCode != SQLITE_CANTOPEN)
                {
                    throw;
                }
            }

            return false;
        }

        public override Task<bool> ExistsAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.FromResult(Exists());
        }

        public override bool HasTables()
        {
            return (int)_executor.ExecuteScalar(_connection.DbConnection, CreateHasTablesCommand()) != 0;
        }

        public override async Task<bool> HasTablesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            return (int)(await _executor.ExecuteScalarAsync(_connection.DbConnection, CreateHasTablesCommand(), cancellationToken)) != 0;
        }

        private SqlStatement CreateHasTablesCommand()
        {
            return new SqlStatement("SELECT count(*) FROM sqlite_master WHERE type = 'table' AND rootpage IS NOT NULL");
        }

        public override void Delete()
        {
            throw new NotImplementedException();
        }

        public override Task DeleteAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            Delete();

            return Task.FromResult(0);
        }
    }
}
