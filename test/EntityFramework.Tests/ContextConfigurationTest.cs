// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Storage;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Fallback;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Tests
{
    public class ContextConfigurationTest
    {
        [Fact]
        public void Optional_multi_services_return_empty_list_when_not_registered()
        {
            Assert.Empty(CreateEmptyConfiguration().Services.EntityStateListeners);
        }

        [Fact]
        public void Requesting_a_singleton_always_returns_same_instance()
        {
            var provider = CreateDefaultProvider();
            var configuration1 = TestHelpers.CreateContextConfiguration(provider);
            var configuration2 = TestHelpers.CreateContextConfiguration(provider);

            Assert.Same(configuration1.Services.ModelSource, configuration2.Services.ModelSource);
            Assert.Same(configuration1.Services.EntityKeyFactorySource, configuration2.Services.EntityKeyFactorySource);
            Assert.Same(configuration1.Services.ClrPropertyGetterSource, configuration2.Services.ClrPropertyGetterSource);
            Assert.Same(configuration1.Services.ClrPropertySetterSource, configuration2.Services.ClrPropertySetterSource);
        }

        [Fact]
        public void Requesting_a_scoped_service_always_returns_same_instance_in_scope()
        {
            var provider = CreateDefaultProvider();
            var configuration = TestHelpers.CreateContextConfiguration(provider);

            Assert.Same(configuration.Services.StateManager, configuration.Services.StateManager);
            Assert.Same(configuration.Services.ContextSets, configuration.Services.ContextSets);
            Assert.Same(configuration.Services.StateEntryNotifier, configuration.Services.StateEntryNotifier);
            Assert.Same(configuration.Services.StateEntryFactory, configuration.Services.StateEntryFactory);
        }

        [Fact]
        public void Requesting_a_scoped_service_always_returns_a_different_instance_in_a_different_scope()
        {
            var provider = CreateDefaultProvider();
            var configuration1 = TestHelpers.CreateContextConfiguration(provider);
            var configuration2 = TestHelpers.CreateContextConfiguration(provider);

            Assert.NotSame(configuration1.Services.StateManager, configuration2.Services.StateManager);
            Assert.NotSame(configuration1.Services.ContextSets, configuration2.Services.ContextSets);
            Assert.NotSame(configuration1.Services.StateEntryNotifier, configuration2.Services.StateEntryNotifier);
            Assert.NotSame(configuration1.Services.StateEntryFactory, configuration2.Services.StateEntryFactory);
        }

        [Fact]
        public void Scoped_data_store_services_can_be_obtained_from_configuration()
        {
            var services = new ServiceCollection();
            services.AddEntityFramework().AddInMemoryStore();
            var serviceProvider = services.BuildServiceProvider();

            DataStore store;
            DataStoreCreator creator;
            DataStoreConnection connection;

            using (var context = new DbContext(serviceProvider))
            {
                store = context.Configuration.DataStore;
                creator = context.Configuration.DataStoreCreator;
                connection = context.Configuration.Connection;

                Assert.Same(store, context.Configuration.DataStore);
                Assert.Same(creator, context.Configuration.DataStoreCreator);
                Assert.Same(connection, context.Configuration.Connection);
            }

            using (var context = new DbContext(serviceProvider))
            {
                Assert.NotSame(store, context.Configuration.DataStore);
                Assert.NotSame(creator, context.Configuration.DataStoreCreator);
                Assert.NotSame(connection, context.Configuration.Connection);
            }
        }

        [Fact]
        public void Scoped_data_store_services_can_be_obtained_from_configuration_with_implicit_service_provider()
        {
            DataStore store;
            DataStoreCreator creator;
            DataStoreConnection connection;

            using (var context = new GiddyupContext())
            {
                store = context.Configuration.DataStore;
                creator = context.Configuration.DataStoreCreator;
                connection = context.Configuration.Connection;

                Assert.Same(store, context.Configuration.DataStore);
                Assert.Same(creator, context.Configuration.DataStoreCreator);
                Assert.Same(connection, context.Configuration.Connection);
            }

            using (var context = new GiddyupContext())
            {
                Assert.NotSame(store, context.Configuration.DataStore);
                Assert.NotSame(creator, context.Configuration.DataStoreCreator);
                Assert.NotSame(connection, context.Configuration.Connection);
            }
        }

        private class GiddyupContext : DbContext
        {
            protected internal override void OnConfiguring(DbContextOptions options)
            {
                options.UseInMemoryStore();
            }
        }

        private static IServiceProvider CreateDefaultProvider()
        {
            var services = new ServiceCollection();
            services.AddEntityFramework().AddInMemoryStore();
            return services.BuildServiceProvider();
        }

        private static DbContextConfiguration CreateEmptyConfiguration()
        {
            var provider = new ServiceCollection().BuildServiceProvider();
            return new DbContextConfiguration()
                .Initialize(provider, provider, new DbContextOptions(), Mock.Of<DbContext>(), DbContextConfiguration.ServiceProviderSource.Explicit);
        }
    }
}
