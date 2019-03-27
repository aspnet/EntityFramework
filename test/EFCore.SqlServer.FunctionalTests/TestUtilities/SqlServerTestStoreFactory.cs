// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    public class SqlServerTestStoreFactory : RelationalTestStoreFactory
    {
        public static SqlServerTestStoreFactory Instance { get; } = new SqlServerTestStoreFactory();

        static SqlServerTestStoreFactory()
        {
            System.Globalization.CultureInfo.DefaultThreadCurrentCulture = new System.Globalization.CultureInfo("cs-CZ");
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("cs-CZ");
        }

        protected SqlServerTestStoreFactory()
        {
        }

        public override TestStore Create(string storeName)
            => SqlServerTestStore.Create(storeName);

        public override TestStore GetOrCreate(string storeName)
            => SqlServerTestStore.GetOrCreate(storeName);

        public override IServiceCollection AddProviderServices(IServiceCollection serviceCollection)
            => serviceCollection.AddEntityFrameworkSqlServer();
    }
}
