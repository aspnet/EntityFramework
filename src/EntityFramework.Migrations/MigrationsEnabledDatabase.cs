﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Migrations.Infrastructure;
using Microsoft.Data.Entity.Migrations.Utilities;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.Storage;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.Migrations
{
    public abstract class MigrationsEnabledDatabase : RelationalDatabase, IMigrationsEnabledDatabaseInternals
    {
        private readonly Migrator _migrator;

        protected MigrationsEnabledDatabase(
            [NotNull] ContextService<IModel> model,
            [NotNull] DataStoreCreator dataStoreCreator,
            [NotNull] DataStoreConnection connection,
            [NotNull] Migrator migrator,
            [NotNull] ILoggerFactory loggerFactory)
            : base(model, dataStoreCreator, connection, loggerFactory)
        {
            Check.NotNull(migrator, "migrator");

            _migrator = migrator;
        }

        public virtual void ApplyMigrations()
        {
            Migrator.ApplyMigrations();
        }

        protected virtual Migrator Migrator
        {
            get { return _migrator; }
        }

        Migrator IMigrationsEnabledDatabaseInternals.Migrator
        {
            get { return Migrator; }
        }
    }
}
