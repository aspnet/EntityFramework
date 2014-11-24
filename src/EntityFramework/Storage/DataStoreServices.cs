// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Identity;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.Data.Entity.Storage
{
    public abstract class DataStoreServices
    {
        public abstract DataStore Store { get; }
        public abstract DataStoreCreator Creator { get; }
        public abstract DataStoreConnection Connection { get; }
        public abstract ValueGeneratorCache ValueGeneratorCache { get; }
        public abstract Database Database { get; }
        public abstract IModelBuilderFactory ModelBuilderFactory { get; }

        public static Func<IServiceProvider, ContextService<DataStoreServices>> DataStoreServicesFactory
        {
            get { return p => new ContextService<DataStoreServices>(() => GetStoreServices(p)); }
        }

        public static Func<IServiceProvider, ContextService<DataStore>> DataStoreFactory
        {
            get { return p => new ContextService<DataStore>(() => GetStoreServices(p).Store); }
        }

        public static Func<IServiceProvider, ContextService<Database>> DatabaseFactory
        {
            get { return p => new ContextService<Database>(() => GetStoreServices(p).Database); }
        }

        public static Func<IServiceProvider, ContextService<DataStoreCreator>> DataStoreCreatorFactory
        {
            get { return p => new ContextService<DataStoreCreator>(() => GetStoreServices(p).Creator); }
        }

        public static Func<IServiceProvider, ContextService<ValueGeneratorCache>> ValueGeneratorCacheFactory
        {
            get { return p => new ContextService<ValueGeneratorCache>(() => GetStoreServices(p).ValueGeneratorCache); }
        }

        public static Func<IServiceProvider, ContextService<DataStoreConnection>> ConnectionFactory
        {
            get { return p => new ContextService<DataStoreConnection>(() => GetStoreServices(p).Connection); }
        }

        protected static DataStoreServices GetStoreServices([NotNull] IServiceProvider serviceProvider)
        {
            Check.NotNull(serviceProvider, "serviceProvider");

            return serviceProvider.GetRequiredServiceChecked<DbContextConfiguration>().DataStoreServices;
        }
    }
}
