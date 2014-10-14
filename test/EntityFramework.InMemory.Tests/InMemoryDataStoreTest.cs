// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Services;
using Microsoft.Framework.Logging;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.InMemory.Tests
{
    public class InMemoryDataStoreTest
    {
        [Fact]
        public void Uses_persistent_database_by_default()
        {
            var configuration = CreateConfiguration();
            var persistentDatabase = new InMemoryDatabase(new[] { new NullLoggerFactory() });

            var inMemoryDataStore = new InMemoryDataStore(configuration, persistentDatabase);

            Assert.Same(persistentDatabase, inMemoryDataStore.Database);
        }

        [Fact]
        public void Uses_persistent_database_if_configured_as_persistent()
        {
            var configuration = CreateConfiguration(new DbContextOptions().UseInMemoryStore(persist: true));

            var persistentDatabase = new InMemoryDatabase(new[] { new NullLoggerFactory() });

            var inMemoryDataStore = new InMemoryDataStore(configuration, persistentDatabase);

            Assert.Same(persistentDatabase, inMemoryDataStore.Database);
        }

        [Fact]
        public void Uses_transient_database_if_not_configured_as_persistent()
        {
            var configuration = CreateConfiguration(new DbContextOptions().UseInMemoryStore(persist: false));

            var persistentDatabase = new InMemoryDatabase(new[] { new NullLoggerFactory() });

            var inMemoryDataStore = new InMemoryDataStore(configuration, persistentDatabase);

            Assert.NotNull(inMemoryDataStore.Database);
            Assert.NotSame(persistentDatabase, inMemoryDataStore.Database);
            Assert.Same(inMemoryDataStore.Database, inMemoryDataStore.Database);
        }

        [Fact]
        public void IsDatabaseCreated_returns_true_for_first_use_of_persistent_database_and_false_thereafter()
        {
            var model = CreateModel();
            var configuration = CreateConfiguration(new DbContextOptions().UseInMemoryStore(persist: true));
            var entityType = model.GetEntityType(typeof(Customer));

            var persistentDatabase = new InMemoryDatabase(new[] { new NullLoggerFactory() });

            var inMemoryDataStore = new InMemoryDataStore(configuration, persistentDatabase);

            Assert.True(inMemoryDataStore.IsDatabaseCreated(model));
            Assert.False(inMemoryDataStore.IsDatabaseCreated(model));
            Assert.False(inMemoryDataStore.IsDatabaseCreated(model));
        }

        [Fact]
        public void IsDatabaseCreated_returns_true_for_first_use_of_non_persistent_database_and_false_thereafter()
        {
            var model = CreateModel();
            var configuration = CreateConfiguration(new DbContextOptions().UseInMemoryStore(persist: false));
            var entityType = model.GetEntityType(typeof(Customer));

            var nonPersistentDatabase = new InMemoryDatabase(new[] { new NullLoggerFactory() });

            var inMemoryDataStore = new InMemoryDataStore(configuration, nonPersistentDatabase);

            Assert.True(inMemoryDataStore.IsDatabaseCreated(model));
            Assert.False(inMemoryDataStore.IsDatabaseCreated(model));
            Assert.False(inMemoryDataStore.IsDatabaseCreated(model));
        }

        [Fact]
        public async Task Save_changes_adds_new_objects_to_store()
        {
            var model = CreateModel();
            var configuration = CreateConfiguration();
            var entityType = model.GetEntityType(typeof(Customer));

            var customer = new Customer { Id = 42, Name = "Unikorn" };
            var entityEntry = new ClrStateEntry(configuration, entityType, customer);
            await entityEntry.SetEntityStateAsync(EntityState.Added);

            var inMemoryDataStore = new InMemoryDataStore(configuration, new InMemoryDatabase(new[] { new NullLoggerFactory() }));

            await inMemoryDataStore.SaveChangesAsync(new[] { entityEntry });

            Assert.Equal(1, inMemoryDataStore.Database.SelectMany(t => t).Count());
            Assert.Equal(new object[] { 42, "Unikorn" }, inMemoryDataStore.Database.Single().Single());
        }

        [Fact]
        public async Task Save_changes_updates_changed_objects_in_store()
        {
            var model = CreateModel();
            var configuration = CreateConfiguration();
            var entityType = model.GetEntityType(typeof(Customer));

            var customer = new Customer { Id = 42, Name = "Unikorn" };
            var entityEntry = new ClrStateEntry(configuration, entityType, customer);
            await entityEntry.SetEntityStateAsync(EntityState.Added);

            var inMemoryDataStore = new InMemoryDataStore(configuration, new InMemoryDatabase(new[] { new NullLoggerFactory() }));

            await inMemoryDataStore.SaveChangesAsync(new[] { entityEntry });

            customer.Name = "Unikorn, The Return";
            await entityEntry.SetEntityStateAsync(EntityState.Modified);

            await inMemoryDataStore.SaveChangesAsync(new[] { entityEntry });

            Assert.Equal(1, inMemoryDataStore.Database.SelectMany(t => t).Count());
            Assert.Equal(new object[] { 42, "Unikorn, The Return" }, inMemoryDataStore.Database.Single().Single());
        }

        [Fact]
        public async Task Save_changes_removes_deleted_objects_from_store()
        {
            var model = CreateModel();
            var configuration = CreateConfiguration();
            var entityType = model.GetEntityType(typeof(Customer));

            var customer = new Customer { Id = 42, Name = "Unikorn" };
            var entityEntry = new ClrStateEntry(configuration, entityType, customer);
            await entityEntry.SetEntityStateAsync(EntityState.Added);

            var inMemoryDataStore = new InMemoryDataStore(configuration, new InMemoryDatabase(new[] { new NullLoggerFactory() }));

            await inMemoryDataStore.SaveChangesAsync(new[] { entityEntry });

            // Because the data store is being used directly the entity state must be manually changed after saving.
            await entityEntry.SetEntityStateAsync(EntityState.Unchanged);

            customer.Name = "Unikorn, The Return";
            await entityEntry.SetEntityStateAsync(EntityState.Deleted);

            await inMemoryDataStore.SaveChangesAsync(new[] { entityEntry });

            Assert.Equal(0, inMemoryDataStore.Database.SelectMany(t => t).Count());
        }

        [Fact]
        public async Task Should_log_writes()
        {
            var model = CreateModel();
            var configuration = CreateConfiguration();
            var entityType = model.GetEntityType(typeof(Customer));

            var customer = new Customer { Id = 42, Name = "Unikorn" };
            var entityEntry = new ClrStateEntry(configuration, entityType, customer);
            await entityEntry.SetEntityStateAsync(EntityState.Added);

            var mockLogger = new Mock<ILogger>();
            var mockFactory = new Mock<ILoggerFactory>();
            mockFactory.Setup(m => m.Create(It.IsAny<string>())).Returns(mockLogger.Object);

            var inMemoryDataStore = new InMemoryDataStore(configuration, new InMemoryDatabase(new[] { mockFactory.Object }));

            await inMemoryDataStore.SaveChangesAsync(new[] { entityEntry });

            mockLogger.Verify(
                l => l.WriteCore(
                    TraceType.Information,
                    0,
                    It.IsAny<string>(),
                    null,
                    It.IsAny<Func<object, Exception, string>>()),
                Times.Once);
        }

        private static IModel CreateModel()
        {
            var model = new Model();
            var modelBuilder = new BasicModelBuilder(model);

            modelBuilder.Entity<Customer>(b =>
                {
                    b.Key(c => c.Id);
                    b.Property(c => c.Name);
                });

            return model;
        }

        private static DbContextConfiguration CreateConfiguration()
        {
            return CreateConfiguration(new DbContextOptions().UseInMemoryStore());
        }

        private static DbContextConfiguration CreateConfiguration(DbContextOptions options)
        {
            return new DbContext(options).Configuration;
        }

        private class Customer
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }
    }
}
