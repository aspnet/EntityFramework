// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Migrations.History;
using Microsoft.Data.Entity.Migrations.Operations;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Sqlite.Migrations
{
    public class SqliteHistoryRepository : IHistoryRepository
    {
        private readonly IRelationalConnection _connection;
        private readonly string _contextKey;
        private readonly SqliteUpdateSqlGenerator _sql;

        protected string MigrationTableName { get; } = "__migrationHistory";

        public SqliteHistoryRepository(
            [NotNull] IRelationalConnection connection,
            [NotNull] DbContext context,
            [NotNull] SqliteUpdateSqlGenerator sql)
        {
            Check.NotNull(connection, nameof(connection));
            Check.NotNull(context, nameof(context));
            Check.NotNull(sql, nameof(sql));

            _connection = connection;
            _contextKey = context.GetType().FullName;
            _sql = sql;
        }

        public virtual string BeginIfExists(string migrationId)
        {
            throw new NotSupportedException(Strings.MigrationScriptGenerationNotSupported);
        }

        public virtual string BeginIfNotExists(string migrationId)
        {
            throw new NotSupportedException(Strings.MigrationScriptGenerationNotSupported);
        }

        public virtual string Create(bool ifNotExists)
        {
            var builder = new IndentedStringBuilder();

            builder.Append("CREATE TABLE ");
            if (ifNotExists)
            {
                builder.Append("IF NOT EXISTS ");
            }
            builder.Append(_sql.DelimitIdentifier(MigrationTableName))
                .AppendLine(" (");
            using (builder.Indent())
            {
                builder
                    .AppendLine("MigrationId TEXT PRIMARY KEY,")
                    .AppendLine("ContextKey TEXT NOT NULL,")
                    .AppendLine("ProductVersion TEXT NOT NULL");
            }
            builder.Append(");");

            return builder.ToString();
        }

        public virtual string EndIf()
        {
            throw new NotSupportedException(Strings.MigrationScriptGenerationNotSupported);
        }

        public async Task<bool> ExistsAsync()
        {
            using (var connection = _connection.DbConnection)
            {
                if (connection.State != ConnectionState.Open)
                {
                    await connection.OpenAsync();
                }
                using (var command = connection.CreateCommand())
                {
                    PopulateExistsCommandText(command);
                    var result = await command.ExecuteScalarAsync();
                    return result != null && (long)result == 1;
                }
            }
        }

        public virtual bool Exists()
        {
            using (var connection = _connection.DbConnection)
            {
                if (connection.State != ConnectionState.Open)
                {
                    connection.Open();
                }
                using (var command = connection.CreateCommand())
                {
                    PopulateExistsCommandText(command);
                    var result = command.ExecuteScalar();
                    return result != null && (long)result == 1;
                }
            }
        }

        public async Task<IReadOnlyList<IHistoryRow>> GetAppliedMigrationsAsync()
        {
            var migrations = new List<IHistoryRow>();

            if (!await ExistsAsync())
            {
                return migrations;
            }

            using (var connection = _connection.DbConnection)
            {
                if (connection.State != ConnectionState.Open)
                {
                    await connection.OpenAsync();
                }
                var command = connection.CreateCommand();
                PopulateGetAppliedMigrationsCommandText(command);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        migrations.Add(MapHistoryRow(reader));
                    }
                }
            }

            return migrations.AsReadOnly();
        }

        public virtual IReadOnlyList<IHistoryRow> GetAppliedMigrations()
        {
            var migrations = new List<IHistoryRow>();

            if (!Exists())
            {
                return migrations;
            }

            using (var connection = _connection.DbConnection)
            {
                if (connection.State != ConnectionState.Open)
                {
                    connection.Open();
                }
                var command = connection.CreateCommand();
                PopulateGetAppliedMigrationsCommandText(command);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        migrations.Add(MapHistoryRow(reader));
                    }
                }
            }

            return migrations.AsReadOnly();
        }

        public virtual MigrationOperation GetDeleteOperation(string migrationId) => new SqlOperation
        {
            Sql = $"DELETE FROM {_sql.DelimitIdentifier(MigrationTableName)} WHERE {_sql.DelimitIdentifier("MigrationId")} = '{_sql.EscapeLiteral(migrationId)}';"
        };

        public virtual MigrationOperation GetInsertOperation(IHistoryRow row) => new SqlOperation
        {
            Sql = new IndentedStringBuilder().Append("INSERT INTO ")
                .Append(_sql.DelimitIdentifier(MigrationTableName))
                .Append(" (\"MigrationId\", \"ContextKey\", \"ProductVersion\") VALUES (")
                .Append($"'{_sql.EscapeLiteral(row.MigrationId)}', ")
                .Append($"'{_sql.EscapeLiteral(_contextKey)}', ")
                .Append($"'{_sql.EscapeLiteral(row.ProductVersion)}'")
                .Append(");")
                .ToString()
        };

        protected virtual void PopulateExistsCommandText(DbCommand command)
        {
            command.CommandText = $"SELECT 1 FROM sqlite_master WHERE type = 'table'" +
                                  $" AND tbl_name = '{_sql.EscapeLiteral(MigrationTableName)}'" +
                                  $" AND rootpage IS NOT NULL;";
        }

        protected virtual void PopulateGetAppliedMigrationsCommandText(DbCommand command)
        {
            command.CommandText = $"SELECT MigrationId, ProductVersion FROM {_sql.DelimitIdentifier(MigrationTableName)} " +
                                  $"WHERE ContextKey = '{_sql.EscapeLiteral(_contextKey)}' ORDER BY MigrationId;";
        }

        protected virtual HistoryRow MapHistoryRow(DbDataReader reader) => new HistoryRow(reader.GetString(0), reader.GetString(1));
    }
}
