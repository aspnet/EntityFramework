// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.Data.Entity.FunctionalTests.TestModels.Northwind;
using Microsoft.Data.Entity.Infrastructure;

namespace Microsoft.Data.Entity.SqlServer.FunctionalTests.TestModels
{
    public class SqlServerNorthwindContext : NorthwindContext
    {
        public static readonly string DatabaseName = StoreName;
        public static readonly string ConnectionString = SqlServerTestStore.CreateConnectionString(DatabaseName);

        public SqlServerNorthwindContext(IServiceProvider serviceProvider, EntityOptions options)
            : base(serviceProvider, options)
        {
        }

        /// <summary>
        ///     A transactional test database, pre-populated with Northwind schema/data
        /// </summary>
        public static Task<SqlServerTestStore> GetSharedStoreAsync()
        {
            return SqlServerTestStore.GetOrCreateSharedAsync(
                DatabaseName,
                () => SqlServerTestStore.CreateDatabaseIfNotExistsAsync(DatabaseName, scriptPath: @"..\..\Northwind.sql")); // relative from bin/<config>
        }

        public static SqlServerTestStore GetSharedStore()
        {
            return SqlServerTestStore.GetOrCreateShared(
                DatabaseName,
                () => SqlServerTestStore.CreateDatabaseIfNotExists(DatabaseName, scriptPath: @"..\..\Northwind.sql")); // relative from bin/<config>
        }
    }
}
