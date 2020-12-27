// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Xunit;
using Microsoft.EntityFrameworkCore.TestModels.ConcurrencyModel;
using System.Linq;

namespace Microsoft.EntityFrameworkCore
{
    public class OptimisticConcurrencyULongInMemoryTest : OptimisticConcurrencyInMemoryTestBase<F1ULongInMemoryFixture, ulong>
    {
        public OptimisticConcurrencyULongInMemoryTest(F1ULongInMemoryFixture fixture)
            : base(fixture)
        {
        }
    }

    public class OptimisticConcurrencyInMemoryTest : OptimisticConcurrencyInMemoryTestBase<F1InMemoryFixture, byte[]>
    {
        public OptimisticConcurrencyInMemoryTest(F1InMemoryFixture fixture)
            : base(fixture)
        {
        }
    }

    public abstract class OptimisticConcurrencyInMemoryTestBase<TFixture, TRowVersion>
        : OptimisticConcurrencyTestBase<TFixture, TRowVersion>
        where TFixture : F1FixtureBase<TRowVersion>, new()
    {
        protected OptimisticConcurrencyInMemoryTestBase(TFixture fixture)
            : base(fixture)
        {
        }
       
        [ConditionalFact(Skip = "Optimistic Offline Lock #2195")]
        public override Task Simple_concurrency_exception_can_be_resolved_with_store_values()
            => Task.FromResult(true);

        [ConditionalFact(Skip = "Optimistic Offline Lock #2195")]
        public override Task Simple_concurrency_exception_can_be_resolved_with_client_values()
            => Task.FromResult(true);

        [ConditionalFact(Skip = "Optimistic Offline Lock #2195")]
        public override Task Simple_concurrency_exception_can_be_resolved_with_new_values()
            => Task.FromResult(true);

        [ConditionalFact(Skip = "Optimistic Offline Lock #2195")]
        public override Task Simple_concurrency_exception_can_be_resolved_with_store_values_using_equivalent_of_accept_changes()
            => Task.FromResult(true);

        [ConditionalFact(Skip = "Optimistic Offline Lock #2195")]
        public override Task Simple_concurrency_exception_can_be_resolved_with_store_values_using_Reload()
            => Task.FromResult(true);

        [ConditionalFact(Skip = "Optimistic Offline Lock #2195")]
        public override Task Updating_then_deleting_the_same_entity_results_in_DbUpdateConcurrencyException()
            => Task.FromResult(true);

        [ConditionalFact(Skip = "Optimistic Offline Lock #2195")]
        public override Task
            Updating_then_deleting_the_same_entity_results_in_DbUpdateConcurrencyException_which_can_be_resolved_with_store_values()
            => Task.FromResult(true);

        [ConditionalFact(Skip = "Optimistic Offline Lock #2195")]
        public override Task
            Change_in_independent_association_after_change_in_different_concurrency_token_results_in_independent_association_exception()
            => Task.FromResult(true);

        [ConditionalFact(Skip = "Optimistic Offline Lock #2195")]
        public override Task Change_in_independent_association_results_in_independent_association_exception()
            => Task.FromResult(true);

        [ConditionalFact(Skip = "Optimistic Offline Lock #2195")]
        public override Task Two_concurrency_issues_in_one_to_many_related_entities_can_be_handled_by_dealing_with_dependent_first()
            => Task.FromResult(true);

        [ConditionalFact(Skip = "Optimistic Offline Lock #2195")]
        public override Task Two_concurrency_issues_in_one_to_one_related_entities_can_be_handled_by_dealing_with_dependent_first()
            => Task.FromResult(true);

        [ConditionalFact]
        public override async Task Adding_the_same_entity_twice_results_in_DbUpdateException()
        {
            using var c = CreateF1Context();
            await c.Database.CreateExecutionStrategy().ExecuteAsync(
                c, async context =>
                {
                    using var transaction = BeginTransaction(context.Database);
                    context.Teams.Add(
                        new Team
                        {
                            Id = -1,
                            Name = "Wubbsy Racing",
                            Chassis = new Chassis { TeamId = -1, Name = "Wubbsy" }
                        });

                    using var innerContext = CreateF1Context();
                    UseTransaction(innerContext.Database, transaction);
                    innerContext.Teams.Add(
                        new Team
                        {
                            Id = -1,
                            Name = "Wubbsy Racing",
                            Chassis = new Chassis { TeamId = -1, Name = "Wubbsy" }
                        });

                    await innerContext.SaveChangesAsync();

                    await Assert.ThrowsAnyAsync<DbUpdateConcurrencyException>(() => context.SaveChangesAsync());
                });
        }
        [ConditionalFact]
        public override async Task Deleting_the_same_entity_twice_results_in_DbUpdateConcurrencyException()
        {
            const string DRIVER="Fernando Alonso";
            using var c = CreateF1Context();
            await c.Database.CreateExecutionStrategy().ExecuteAsync(
                c, async context =>
                {
                    using var prepareTransaction = BeginTransaction(context.Database);
                    context.Drivers.Add(
                        new Driver(){ Name=DRIVER}
                    );
                    await context.SaveChangesAsync();
                    using var transaction = BeginTransaction(context.Database);
                    context.Drivers.Remove(context.Drivers.Single(d => d.Name == DRIVER));
                        

                    using var innerContext = CreateF1Context();
                    UseTransaction(innerContext.Database, transaction);
                    innerContext.Drivers.Remove(innerContext.Drivers.Single(d => d.Name == DRIVER));
                    
                    await innerContext.SaveChangesAsync();
                    
                    await Assert.ThrowsAnyAsync<DbUpdateConcurrencyException>(() => context.SaveChangesAsync());
                });
        }

        [ConditionalFact]
        public override async Task Deleting_then_updating_the_same_entity_results_in_DbUpdateConcurrencyException()
        {
            const string DRIVER="Fernando Alonso";
            using var c = CreateF1Context();
            await c.Database.CreateExecutionStrategy().ExecuteAsync(
                c, async context =>
                {
                    using var prepareTransaction = BeginTransaction(context.Database);
                    context.Drivers.Add(
                        new Driver(){ Name=DRIVER}
                    );
                    await context.SaveChangesAsync();

                    using var transaction = BeginTransaction(context.Database);
                    context.Drivers.Remove(context.Drivers.Single(d => d.Name == DRIVER));
                        

                    using var innerContext = CreateF1Context();
                    UseTransaction(innerContext.Database, transaction);
                    var driver = innerContext.Drivers.Single(d => d.Name == DRIVER);
                    driver.Name="Felix";
                    await innerContext.SaveChangesAsync();

                    await Assert.ThrowsAnyAsync<DbUpdateConcurrencyException>(() => context.SaveChangesAsync());
                });
        }
    }
}
