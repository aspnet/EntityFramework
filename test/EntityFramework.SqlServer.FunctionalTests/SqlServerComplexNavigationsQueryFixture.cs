﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.FunctionalTests;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Fallback;
using Microsoft.Framework.Logging;
using Microsoft.Data.Entity.FunctionalTests.TestModels.ComplexNavigationsModel;

namespace Microsoft.Data.Entity.SqlServer.FunctionalTests
{
    public class SqlServerComplexNavigationsQueryFixture : RelationalComplexNavigationsQueryFixture<SqlServerTestStore>
    {
        public static readonly string DatabaseName = "ComplexNavigations";

        private readonly IServiceProvider _serviceProvider;

        private readonly string _connectionString = SqlServerTestStore.CreateConnectionString(DatabaseName);

        public SqlServerComplexNavigationsQueryFixture()
        {
            _serviceProvider = new ServiceCollection()
                .AddEntityFramework()
                .AddSqlServer()
                .ServiceCollection
                .AddSingleton(typeof(SqlServerModelSource), p => new TestSqlServerModelSource(OnModelCreating))
                .AddInstance<ILoggerFactory>(new TestSqlLoggerFactory())
                .BuildServiceProvider();
        }

        public override SqlServerTestStore CreateTestStore()
        {
            return SqlServerTestStore.GetOrCreateShared(DatabaseName, () =>
            {
                var options = new DbContextOptions();
                options.UseSqlServer(_connectionString);

                using (var context = new ComplexNavigationsContext(_serviceProvider, options))
                {
                    // TODO: Delete DB if model changed

                    if (context.Database.EnsureCreated())
                    {
                        ComplexNavigationsModelInitializer.Seed(context);
                    }

                    TestSqlLoggerFactory.SqlStatements.Clear();
                }
            });
        }

        public override ComplexNavigationsContext CreateContext(SqlServerTestStore testStore)
        {
            var options = new DbContextOptions();
            options.UseSqlServer(testStore.Connection);

            var context = new ComplexNavigationsContext(_serviceProvider, options);
            context.Database.AsRelational().Connection.UseTransaction(testStore.Transaction);
            return context;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ForSqlServer().UseSequence();
        }
    }
}

