﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Migrations.Infrastructure;
using Microsoft.Data.Entity.Relational;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.SqlServer
{
    // TODO: Figure out what needs to be done to avoid having provider specific migrators.
    // From Arthur: Another option is to create one Migrator type that depends on DbContextConfiguration 
    // and then uses the DataStoreSource/DataStoreSelector to get provider-specific Migrations services.
    public class SqlServerMigrator : Migrator
    {
        /// <summary>
        ///     This constructor is intended only for use when creating test doubles that will override members
        ///     with mocked or faked behavior. Use of this constructor for other purposes may result in unexpected
        ///     behavior including but not limited to throwing <see cref="NullReferenceException" />.
        /// </summary>
        protected SqlServerMigrator()
        {
        }

        public SqlServerMigrator(
            [NotNull] DbContextConfiguration contextConfiguration,
            [NotNull] HistoryRepository historyRepository,
            [NotNull] MigrationAssembly migrationAssembly,
            [NotNull] SqlServerModelDiffer modelDiffer,
            [NotNull] SqlServerMigrationOperationSqlGeneratorFactory sqlGeneratorFactory,
            [NotNull] SqlServerSqlGenerator sqlGenerator,
            [NotNull] SqlStatementExecutor sqlStatementExecutor,
            [NotNull] ILoggerFactory loggerFactory)
            : base(
                contextConfiguration,
                historyRepository,
                migrationAssembly,
                modelDiffer,
                sqlGeneratorFactory,
                sqlGenerator,
                sqlStatementExecutor,
                loggerFactory)
        {
        }
    }
}
