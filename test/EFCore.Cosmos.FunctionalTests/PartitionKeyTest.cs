﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Cosmos.TestUtilities;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Cosmos
{
    public class PartitionKeyTest : IClassFixture<PartitionKeyTest.CosmosPartitionKeyFixture>
    {
        private const string DatabaseName = nameof(PartitionKeyTest);

        private void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

        protected CosmosPartitionKeyFixture Fixture { get; }
        
        public PartitionKeyTest(CosmosPartitionKeyFixture fixture)
        {
            Fixture = fixture;
        }

        [ConditionalFact]
        public virtual async Task Can_add_update_delete_end_to_end_with_partition_key()
        {
            var readSql = "SELECT c\r\n"
                + "FROM root c\r\n"
                + "WHERE (c[\"Discriminator\"] = \"Customer\")\r\n"
                + "ORDER BY c[\"PartitionKey\"]\r\n"
                + "OFFSET 0 LIMIT 1";

            await PartitionKeyTestAsync(
                async ctx => await ctx.Customers.OrderBy(c => c.PartitionKey).FirstAsync(),
                readSql,
                async ctx => await ctx.Customers.OrderBy(c => c.PartitionKey).LastAsync(),
                async ctx => await ctx.Customers.OrderBy(c => c.PartitionKey).ToListAsync());
        }

        [ConditionalFact]
        public virtual async Task Can_add_update_delete_end_to_end_with_with_partition_key_extension()
        {
            var readSql = "SELECT c\r\n"
                + "FROM root c\r\n"
                + "WHERE (c[\"Discriminator\"] = \"Customer\")\r\n"
                + "OFFSET 0 LIMIT 1";

            await PartitionKeyTestAsync(
                async ctx => await ctx.Customers.WithPartitionKey("1").FirstAsync(),
                readSql,
                async ctx => await ctx.Customers.WithPartitionKey("2").LastAsync(),
                async ctx => await ctx.Customers.WithPartitionKey("2").ToListAsync());
        }

        protected virtual async Task PartitionKeyTestAsync(
            Func<PartitionKeyContext, Task<Customer>> readSingleTask,
            string readSql,
            Func<PartitionKeyContext, Task<Customer>> readLastTask,
            Func<PartitionKeyContext, Task<List<Customer>>> readListTask)
        {
            await using var outerContext = CreateContext();
            await outerContext.Database.EnsureCreatedAsync();

            var customer1 = new Customer
            {
                Id = 42,
                Name = "Theon",
                PartitionKey = 1
            };

            var customer2 = new Customer
            {
                Id = 42,
                Name = "Theon Twin",
                PartitionKey = 2
            };

            await outerContext.AddAsync(customer1);
            await outerContext.AddAsync(customer2);
            await outerContext.SaveChangesAsync();

            // Read & update
            await using (var innerContext = CreateContext())
            {
                var customerFromStore = await readSingleTask(innerContext);

                AssertSql(readSql);

                Assert.Equal(42, customerFromStore.Id);
                Assert.Equal("Theon", customerFromStore.Name);
                Assert.Equal(1, customerFromStore.PartitionKey);

                customerFromStore.Name = "Theon Greyjoy";

                await innerContext.SaveChangesAsync();
            }

            // Test exception
            await using (var innerContext = CreateContext())
            {
                var customerFromStore = await readSingleTask(innerContext);
                customerFromStore.PartitionKey = 2;

                Assert.Equal(
                    CoreStrings.KeyReadOnly(nameof(Customer.PartitionKey), nameof(Customer)),
                    Assert.Throws<InvalidOperationException>(() => innerContext.SaveChanges()).Message);
            }

            // Read update & delete
            await using (var innerContext = CreateContext())
            {
                var customerFromStore = await readSingleTask(innerContext);

                Assert.Equal(42, customerFromStore.Id);
                Assert.Equal("Theon Greyjoy", customerFromStore.Name);
                Assert.Equal(1, customerFromStore.PartitionKey);

                innerContext.Remove(customerFromStore);

                innerContext.Remove(await readLastTask(innerContext));

                await innerContext.SaveChangesAsync();
            }

            await using (var innerContext = CreateContext())
            {
                Assert.Empty(await readListTask(innerContext));
            }
        }

        protected PartitionKeyContext CreateContext() => Fixture.CreateContext();

        public class CosmosPartitionKeyFixture : SharedStoreFixtureBase<PartitionKeyContext>
        {
            protected override string StoreName => DatabaseName;
            
            protected override ITestStoreFactory TestStoreFactory => CosmosTestStoreFactory.Instance;

            public TestSqlLoggerFactory TestSqlLoggerFactory
                => (TestSqlLoggerFactory)ServiceProvider.GetRequiredService<ILoggerFactory>();
        }

        public class PartitionKeyContext : PoolableDbContext
        {
            public DbSet<Customer> Customers { get; set; }

            public PartitionKeyContext(DbContextOptions dbContextOptions)
                : base(dbContextOptions)
            {
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Customer>(
                    cb =>
                    {
                        cb.HasPartitionKey(c => c.PartitionKey);
                        cb.Property(c => c.PartitionKey).HasConversion<string>();
                        cb.HasKey(c => new { c.Id, c.PartitionKey });
                    });
            }
        }

        public class Customer
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int PartitionKey { get; set; }
        }
    }
}
