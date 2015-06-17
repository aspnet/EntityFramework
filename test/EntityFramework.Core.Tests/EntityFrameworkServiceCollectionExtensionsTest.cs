// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Builders;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Query;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.ValueGeneration;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Logging;
using Xunit;

namespace Microsoft.Data.Entity.Tests
{
    public abstract class EntityFrameworkServiceCollectionExtensionsTest : IDisposable
    {
        [Fact]
        public virtual void Services_wire_up_correctly()
        {
            VerifySingleton<IDbSetFinder>();
            VerifySingleton<IDbSetInitializer>();
            VerifySingleton<IDbSetSource>();
            VerifySingleton<IEntityKeyFactorySource>();
            VerifySingleton<IClrAccessorSource<IClrPropertyGetter>>();
            VerifySingleton<IClrAccessorSource<IClrPropertySetter>>();
            VerifySingleton<IClrCollectionAccessorSource>();
            VerifySingleton<ICollectionTypeFactory>();
            VerifySingleton<IEntityMaterializerSource>();
            VerifySingleton<IModelValidator>();
            VerifySingleton<IMemberMapper>();
            VerifySingleton<IFieldMatcher>();
            VerifySingleton<IOriginalValuesFactory>();
            VerifySingleton<IRelationshipsSnapshotFactory>();
            VerifySingleton<IStoreGeneratedValuesFactory>();
            VerifySingleton<IEntityEntryMetadataServices>();
            VerifySingleton<ICompiledQueryCache>();
            VerifySingleton<ILoggerFactory>();

            VerifyCached<IModelBuilderFactory>();
            VerifyCached<IModel>();

            VerifyScoped<IKeyPropagator>();
            VerifyScoped<INavigationFixer>();
            VerifyScoped<IStateManager>();
            VerifyScoped<IInternalEntityEntryFactory>();
            VerifyScoped<IInternalEntityEntryNotifier>();
            VerifyScoped<IInternalEntityEntrySubscriber>();
            VerifyScoped<IValueGenerationManager>();
            VerifyScoped<IEntityQueryProvider>();
            VerifyScoped<IChangeTrackerFactory>();
            VerifyScoped<IChangeDetector>();
            VerifyScoped<IEntityEntryGraphIterator>();
            VerifyScoped<IDbContextServices>();
            VerifyScoped<DbContext>();
            VerifyScoped<IDbContextOptions>();
            VerifyScoped<IDataStoreSelector>();
            VerifyScoped<IDataStore>();
            VerifyScoped<IDataStoreConnection>();
            VerifyScoped<IDatabaseFactory>();
            VerifyScoped<IValueGeneratorSelector>();
            VerifyScoped<IDataStoreCreator>();

            VerifyScoped<IEntityStateListener>(isExistingReplaced: true);
            VerifyScoped<IForeignKeyListener>(isExistingReplaced: true);
            VerifyScoped<INavigationListener>(isExistingReplaced: true);
            VerifyScoped<IKeyListener>(isExistingReplaced: true);
            VerifyScoped<IPropertyListener>(isExistingReplaced: true);
        }

        [Fact]
        protected void Common_data_store_services_wire_up_correctly()
        {
            VerifySingleton<IDataStoreSource>(isExistingReplaced: true);
            Assert.NotNull(VerifyCached<IModel>());
            Assert.NotNull(VerifyScoped<DbContext>());
            Assert.NotNull(VerifyScoped<IDbContextOptions>());
            Assert.NotNull(VerifyScoped<IDataStore>());
            Assert.NotNull(VerifyScoped<IDataStoreConnection>());
            Assert.NotNull(VerifyScoped<IDatabaseFactory>());
            Assert.NotNull(VerifyScoped<IValueGeneratorSelector>());
            Assert.NotNull(VerifyScoped<IDataStoreCreator>());
            Assert.NotNull(VerifySingleton<IModelBuilderFactory>());
        }

        [Fact]
        public virtual void Throws_on_multiple_calls_to_add_service()
        {
            var servicesBuilder = _testHelpers
                .AddProviderServices(new ServiceCollection().AddEntityFramework());

            Assert.Equal(
                Strings.MultipleCallsToAddProvider(AddProviderMethodName),
                Assert.Throws<InvalidOperationException>(() => _testHelpers.AddProviderServices(servicesBuilder)).Message);
        }

        private readonly TestHelpers _testHelpers;
        private readonly IServiceProvider _serviceProvider;
        private readonly DbContext _firstContext;
        private readonly DbContext _secondContext;

        protected abstract string AddProviderMethodName { get; }

        protected EntityFrameworkServiceCollectionExtensionsTest(TestHelpers testHelpers)
        {
            _testHelpers = testHelpers;

            var serviceCollection = new ServiceCollection();
            _testHelpers.AddProviderServices(serviceCollection.AddEntityFramework());
            _serviceProvider = serviceCollection.BuildServiceProvider();
            _firstContext = _testHelpers.CreateContext(_serviceProvider);
            _secondContext = _testHelpers.CreateContext(_serviceProvider);
        }

        protected virtual IServiceCollection GetServices(IServiceCollection services = null)
        {
            return (services ?? new ServiceCollection())
                .AddEntityFramework()
                .AddInMemoryStore()
                .ServiceCollection();
        }

        public void Dispose()
        {
            _firstContext.Dispose();
            _secondContext.Dispose();
        }

        protected TService VerifySingleton<TService>(bool isExistingReplaced = false)
            where TService : class
        {
            return VerifyService<TService>(((IAccessor<IServiceProvider>)_firstContext).Service, isExistingReplaced, isScoped: false, isCached: false);
        }

        protected TService VerifyScoped<TService>(bool isExistingReplaced = false)
            where TService : class
        {
            return VerifyService<TService>(((IAccessor<IServiceProvider>)_firstContext).Service, isExistingReplaced, isScoped: true, isCached: false);
        }

        protected TService VerifyCached<TService>(bool isExistingReplaced = false)
            where TService : class
        {
            return VerifyService<TService>(((IAccessor<IServiceProvider>)_firstContext).Service, isExistingReplaced, isScoped: true, isCached: true);
        }

        private TService VerifyService<TService>(IServiceProvider serviceProvider, bool isExistingReplaced, bool isScoped, bool isCached)
            where TService : class
        {
            var service = serviceProvider.GetRequiredService<TService>();

            Assert.NotNull(service);
            Assert.Same(service, serviceProvider.GetRequiredService<TService>());

            var scopedService = ((IAccessor<IServiceProvider>)_secondContext).Service.GetRequiredService<TService>();

            if (isCached)
            {
                Assert.Same(service, scopedService);
            }
            else if (isScoped)
            {
                Assert.NotSame(service, scopedService);
            }
            else
            {
                Assert.Same(service, scopedService);
            }
            Assert.Equal(1, serviceProvider.GetRequiredService<IEnumerable<TService>>().Count());

            if (typeof(TService) != typeof(IDbContextServices))
            {
                var customServices = isScoped
                    ? new ServiceCollection().AddScoped(p => service)
                    : new ServiceCollection().AddSingleton(p => service);

                var serviceProviderWithCustomService = ((IAccessor<IServiceProvider>)new DbContext(
                    _testHelpers.AddProviderServices(customServices.AddEntityFramework())
                        .ServiceCollection()
                        .BuildServiceProvider(),
                    _testHelpers.CreateOptions())).Service;
                if (isExistingReplaced)
                {
                    Assert.NotSame(service, serviceProviderWithCustomService.GetRequiredService<TService>());
                    Assert.Equal(2, serviceProviderWithCustomService.GetRequiredService<IEnumerable<TService>>().Count());
                }
                else
                {
                    Assert.Same(service, serviceProviderWithCustomService.GetRequiredService<TService>());
                    Assert.Equal(1, serviceProviderWithCustomService.GetRequiredService<IEnumerable<TService>>().Count());
                }
            }

            return service;
        }
    }
}
