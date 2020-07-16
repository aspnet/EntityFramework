﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestModels.ManyToManyModel;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore
{
    public abstract class ManyToManyTrackingTestBase<TFixture> : IClassFixture<TFixture>
        where TFixture : ManyToManyTrackingTestBase<TFixture>.ManyToManyTrackingFixtureBase
    {
        [ConditionalFact]
        public virtual void Can_insert_many_to_many_self_shared()
        {
            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var leftEntities = new[]
                    {
                        new EntityTwo { Id = 7711, SelfSkipSharedLeft = new List<EntityTwo>() },
                        new EntityTwo { Id = 7712, SelfSkipSharedLeft = new List<EntityTwo>() },
                        new EntityTwo { Id = 7713, SelfSkipSharedLeft = new List<EntityTwo>() }
                    };
                    var rightEntities = new[]
                    {
                        new EntityTwo { Id = 7721, SelfSkipSharedRight = new List<EntityTwo>() },
                        new EntityTwo { Id = 7722, SelfSkipSharedRight = new List<EntityTwo>() },
                        new EntityTwo { Id = 7723, SelfSkipSharedRight = new List<EntityTwo>() }
                    };

                    leftEntities[0].SelfSkipSharedLeft.Add(rightEntities[0]); // 11 - 21
                    leftEntities[0].SelfSkipSharedLeft.Add(rightEntities[1]); // 11 - 22
                    leftEntities[0].SelfSkipSharedLeft.Add(rightEntities[2]); // 11 - 23

                    rightEntities[0].SelfSkipSharedRight.Add(leftEntities[0]); // 21 - 11 (Dupe)
                    rightEntities[0].SelfSkipSharedRight.Add(leftEntities[1]); // 21 - 12
                    rightEntities[0].SelfSkipSharedRight.Add(leftEntities[2]); // 21 - 13

                    context.AddRange(leftEntities[0], leftEntities[1], leftEntities[2]);
                    context.AddRange(rightEntities[0], rightEntities[1], rightEntities[2]);

                    ValidateFixup(context, leftEntities, rightEntities);

                    context.SaveChanges();

                    ValidateFixup(context, leftEntities, rightEntities);
                },
                context =>
                {
                    var results = context.Set<EntityTwo>().Where(e => e.Id > 7700).Include(e => e.SelfSkipSharedLeft).ToList();
                    Assert.Equal(6, results.Count);

                    var leftEntities = context.ChangeTracker.Entries<EntityTwo>()
                        .Select(e => e.Entity)
                        .Where(e => e.Id < 7720)
                        .OrderBy(e => e.Id)
                        .ToList();

                    var rightEntities = context.ChangeTracker.Entries<EntityTwo>()
                        .Select(e => e.Entity)
                        .Where(e => e.Id > 7720)
                        .OrderBy(e => e.Id)
                        .ToList();

                    ValidateFixup(context, leftEntities, rightEntities);
                });

            void ValidateFixup(DbContext context, IList<EntityTwo> leftEntities, IList<EntityTwo> rightEntities)
            {
                Assert.Equal(11, context.ChangeTracker.Entries().Count());
                Assert.Equal(6, context.ChangeTracker.Entries<EntityTwo>().Count());
                Assert.Equal(5, context.ChangeTracker.Entries< /*TODO: Shared*/JoinTwoSelfShared>().Count());

                Assert.Equal(3, leftEntities[0].SelfSkipSharedLeft.Count);
                Assert.Single(leftEntities[1].SelfSkipSharedLeft);
                Assert.Single(leftEntities[2].SelfSkipSharedLeft);

                Assert.Equal(3, rightEntities[0].SelfSkipSharedRight.Count);
                Assert.Single(rightEntities[1].SelfSkipSharedRight);
                Assert.Single(rightEntities[2].SelfSkipSharedRight);
            }
        }

        [ConditionalFact]
        public virtual void Can_update_many_to_many_self()
        {
            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var leftEntities = context.Set<EntityTwo>().Include(e => e.SelfSkipSharedRight).OrderBy(e => e.Id).ToList();
                    var rightEntities = context.Set<EntityTwo>().Include(e => e.SelfSkipSharedLeft).ToList();

                    leftEntities[0].SelfSkipSharedRight.AddRange(new[]
                    {
                        new EntityTwo { Id = 7721 },
                        new EntityTwo { Id = 7722 },
                        new EntityTwo { Id = 7723 }
                    });

                    rightEntities[0].SelfSkipSharedLeft.AddRange(new[]
                    {
                        new EntityTwo { Id = 7711 },
                        new EntityTwo { Id = 7712 },
                        new EntityTwo { Id = 7713 }
                    });

                    leftEntities[2].SelfSkipSharedRight.Remove(leftEntities[2].SelfSkipSharedRight.Single(e => e.Id == 5));
                    rightEntities[1].SelfSkipSharedLeft.Remove(rightEntities[1].SelfSkipSharedLeft.Single(e => e.Id == 8));

                    leftEntities[4].SelfSkipSharedRight.Remove(leftEntities[4].SelfSkipSharedRight.Single(e => e.Id == 8));
                    leftEntities[4].SelfSkipSharedRight.Add(new EntityTwo { Id = 7724 });

                    rightEntities[3].SelfSkipSharedLeft.Remove(rightEntities[3].SelfSkipSharedLeft.Single(e => e.Id == 9));
                    rightEntities[3].SelfSkipSharedLeft.Add(new EntityTwo { Id = 7714 });

                    context.ChangeTracker.DetectChanges();

                    ValidateFixup(context, leftEntities, rightEntities, 28, 42);

                    context.SaveChanges();

                    ValidateFixup(context, leftEntities, rightEntities, 28, 42 - 4);
                },
                context =>
                {
                    var leftEntities = context.Set<EntityTwo>().Include(e => e.SelfSkipSharedRight).OrderBy(e => e.Id).ToList();
                    var rightEntities = context.Set<EntityTwo>().Include(e => e.SelfSkipSharedLeft).ToList();

                    ValidateFixup(context, leftEntities, rightEntities, 28, 42 - 4);
                });

            void ValidateFixup(
                DbContext context,
                List<EntityTwo> leftEntities,
                List<EntityTwo> rightEntities,
                int count,
                int joinCount)
            {
                Assert.Equal(count, context.ChangeTracker.Entries<EntityTwo>().Count());
                Assert.Equal(joinCount, context.ChangeTracker.Entries<JoinTwoSelfShared>().Count());
                Assert.Equal(count + joinCount, context.ChangeTracker.Entries().Count());

                Assert.Contains(leftEntities[0].SelfSkipSharedRight, e => e.Id == 7721);
                Assert.Contains(leftEntities[0].SelfSkipSharedRight, e => e.Id == 7722);
                Assert.Contains(leftEntities[0].SelfSkipSharedRight, e => e.Id == 7723);

                Assert.Contains(rightEntities[0].SelfSkipSharedLeft, e => e.Id == 7711);
                Assert.Contains(rightEntities[0].SelfSkipSharedLeft, e => e.Id == 7712);
                Assert.Contains(rightEntities[0].SelfSkipSharedLeft, e => e.Id == 7713);

                Assert.DoesNotContain(leftEntities[2].SelfSkipSharedRight, e => e.Id == 5);
                Assert.DoesNotContain(rightEntities[1].SelfSkipSharedLeft, e => e.Id == 8);

                Assert.DoesNotContain(leftEntities[4].SelfSkipSharedRight, e => e.Id == 8);
                Assert.Contains(leftEntities[4].SelfSkipSharedRight, e => e.Id == 7724);

                Assert.DoesNotContain(rightEntities[3].SelfSkipSharedLeft, e => e.Id == 9);
                Assert.Contains(rightEntities[3].SelfSkipSharedLeft, e => e.Id == 7714);

                var allLeft = context.ChangeTracker.Entries<EntityTwo>().Select(e => e.Entity).OrderBy(e => e.Id).ToList();
                var allRight = context.ChangeTracker.Entries<EntityTwo>().Select(e => e.Entity).OrderBy(e => e.Id).ToList();

                var joins = 0;
                foreach (var left in allLeft)
                {
                    foreach (var right in allRight)
                    {
                        if (left.SelfSkipSharedRight?.Contains(right) == true)
                        {
                            Assert.Contains(left, right.SelfSkipSharedLeft);
                            joins++;
                        }

                        if (right.SelfSkipSharedLeft?.Contains(left) == true)
                        {
                            Assert.Contains(right, left.SelfSkipSharedRight);
                            joins++;
                        }
                    }
                }

                var deleted = context.ChangeTracker.Entries<JoinTwoSelfShared>().Count(e => e.State == EntityState.Deleted);
                Assert.Equal(joinCount, (joins / 2) + deleted);
            }
        }

        [ConditionalFact]
        public virtual void Can_insert_many_to_many_with_navs()
        {
            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var leftEntities = new[]
                    {
                        new EntityTwo { Id = 7711, ThreeSkipFull = new List<EntityThree>() },
                        new EntityTwo { Id = 7712, ThreeSkipFull = new List<EntityThree>() },
                        new EntityTwo { Id = 7713, ThreeSkipFull = new List<EntityThree>() }
                    };
                    var rightEntities = new[]
                    {
                        new EntityThree { Id = 7721, TwoSkipFull = new List<EntityTwo>() },
                        new EntityThree { Id = 7722, TwoSkipFull = new List<EntityTwo>() },
                        new EntityThree { Id = 7723, TwoSkipFull = new List<EntityTwo>() }
                    };

                    leftEntities[0].ThreeSkipFull.Add(rightEntities[0]); // 11 - 21
                    leftEntities[0].ThreeSkipFull.Add(rightEntities[1]); // 11 - 22
                    leftEntities[0].ThreeSkipFull.Add(rightEntities[2]); // 11 - 23

                    rightEntities[0].TwoSkipFull.Add(leftEntities[0]); // 21 - 11 (Dupe)
                    rightEntities[0].TwoSkipFull.Add(leftEntities[1]); // 21 - 12
                    rightEntities[0].TwoSkipFull.Add(leftEntities[2]); // 21 - 13

                    context.AddRange(leftEntities[0], leftEntities[1], leftEntities[2]);
                    context.AddRange(rightEntities[0], rightEntities[1], rightEntities[2]);

                    ValidateFixup(context, leftEntities, rightEntities);

                    context.SaveChanges();

                    ValidateFixup(context, leftEntities, rightEntities);
                },
                context =>
                {
                    var results = context.Set<EntityTwo>().Where(e => e.Id > 7700).Include(e => e.ThreeSkipFull).ToList();
                    Assert.Equal(3, results.Count);

                    var leftEntities = context.ChangeTracker.Entries<EntityTwo>().Select(e => e.Entity).OrderBy(e => e.Id).ToList();
                    var rightEntities = context.ChangeTracker.Entries<EntityThree>().Select(e => e.Entity).OrderBy(e => e.Id).ToList();

                    ValidateFixup(context, leftEntities, rightEntities);
                });

            void ValidateFixup(DbContext context, IList<EntityTwo> leftEntities, IList<EntityThree> rightEntities)
            {
                Assert.Equal(11, context.ChangeTracker.Entries().Count());
                Assert.Equal(3, context.ChangeTracker.Entries<EntityTwo>().Count());
                Assert.Equal(3, context.ChangeTracker.Entries<EntityThree>().Count());
                Assert.Equal(5, context.ChangeTracker.Entries<JoinTwoToThree>().Count());

                Assert.Equal(3, leftEntities[0].ThreeSkipFull.Count);
                Assert.Single(leftEntities[1].ThreeSkipFull);
                Assert.Single(leftEntities[2].ThreeSkipFull);

                Assert.Equal(3, rightEntities[0].TwoSkipFull.Count);
                Assert.Single(rightEntities[1].TwoSkipFull);
                Assert.Single(rightEntities[2].TwoSkipFull);

                foreach (var joinEntity in context.ChangeTracker.Entries<JoinTwoToThree>().Select(e => e.Entity).ToList())
                {
                    Assert.Equal(joinEntity.Two.Id, joinEntity.TwoId);
                    Assert.Equal(joinEntity.Three.Id, joinEntity.ThreeId);
                    Assert.Contains(joinEntity, joinEntity.Two.JoinThreeFull);
                    Assert.Contains(joinEntity, joinEntity.Three.JoinTwoFull);
                }
            }
        }

        [ConditionalFact]
        public virtual void Can_update_many_to_many_with_navs()
        {
            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var leftEntities = context.Set<EntityTwo>().Include(e => e.ThreeSkipFull).OrderBy(e => e.Id).ToList();
                    var rightEntities = context.Set<EntityThree>().Include(e => e.TwoSkipFull).ToList();

                    leftEntities[0].ThreeSkipFull.AddRange(new[]
                    {
                        new EntityThree { Id = 7721 },
                        new EntityThree { Id = 7722 },
                        new EntityThree { Id = 7723 }
                    });

                    rightEntities[0].TwoSkipFull.AddRange(new[]
                    {
                        new EntityTwo { Id = 7711 },
                        new EntityTwo { Id = 7712 },
                        new EntityTwo { Id = 7713 }
                    });

                    leftEntities[1].ThreeSkipFull.Remove(leftEntities[1].ThreeSkipFull.Single(e => e.Id == 9));
                    rightEntities[1].TwoSkipFull.Remove(rightEntities[1].TwoSkipFull.Single(e => e.Id == 4));

                    leftEntities[2].ThreeSkipFull.Remove(leftEntities[2].ThreeSkipFull.Single(e => e.Id == 11));
                    leftEntities[2].ThreeSkipFull.Add(new EntityThree { Id = 7724 });

                    rightEntities[2].TwoSkipFull.Remove(rightEntities[2].TwoSkipFull.Single(e => e.Id == 6));
                    rightEntities[2].TwoSkipFull.Add(new EntityTwo { Id = 7714 });

                    context.ChangeTracker.DetectChanges();

                    ValidateFixup(context, leftEntities, rightEntities, 24, 24, 60);

                    context.SaveChanges();

                    ValidateFixup(context, leftEntities, rightEntities, 24, 24, 60 - 4);
                },
                context =>
                {
                    var leftEntities = context.Set<EntityTwo>().Include(e => e.ThreeSkipFull).OrderBy(e => e.Id).ToList();
                    var rightEntities = context.Set<EntityThree>().Include(e => e.TwoSkipFull).ToList();

                    ValidateFixup(context, leftEntities, rightEntities, 24, 24, 60 - 4);
                });

            void ValidateFixup(
                DbContext context,
                List<EntityTwo> leftEntities,
                List<EntityThree> rightEntities,
                int leftCount,
                int rightCount,
                int joinCount)
            {
                Assert.Equal(leftCount, context.ChangeTracker.Entries<EntityTwo>().Count());
                Assert.Equal(rightCount, context.ChangeTracker.Entries<EntityThree>().Count());
                Assert.Equal(joinCount, context.ChangeTracker.Entries<JoinTwoToThree>().Count());
                Assert.Equal(leftCount + rightCount + joinCount, context.ChangeTracker.Entries().Count());

                Assert.Contains(leftEntities[0].ThreeSkipFull, e => e.Id == 7721);
                Assert.Contains(leftEntities[0].ThreeSkipFull, e => e.Id == 7722);
                Assert.Contains(leftEntities[0].ThreeSkipFull, e => e.Id == 7723);

                Assert.Contains(rightEntities[0].TwoSkipFull, e => e.Id == 7711);
                Assert.Contains(rightEntities[0].TwoSkipFull, e => e.Id == 7712);
                Assert.Contains(rightEntities[0].TwoSkipFull, e => e.Id == 7713);

                Assert.DoesNotContain(leftEntities[1].ThreeSkipFull, e => e.Id == 9);
                Assert.DoesNotContain(rightEntities[1].TwoSkipFull, e => e.Id == 4);

                Assert.DoesNotContain(leftEntities[2].ThreeSkipFull, e => e.Id == 11);
                Assert.Contains(leftEntities[2].ThreeSkipFull, e => e.Id == 7724);

                Assert.DoesNotContain(rightEntities[2].TwoSkipFull, e => e.Id == 6);
                Assert.Contains(rightEntities[2].TwoSkipFull, e => e.Id == 7714);

                foreach (var joinEntry in context.ChangeTracker.Entries<JoinTwoToThree>().ToList())
                {
                    var joinEntity = joinEntry.Entity;
                    Assert.Equal(joinEntity.Two.Id, joinEntity.TwoId);
                    Assert.Equal(joinEntity.Three.Id, joinEntity.ThreeId);
                    Assert.Contains(joinEntity, joinEntity.Two.JoinThreeFull);
                    Assert.Contains(joinEntity, joinEntity.Three.JoinTwoFull);
                }

                var allLeft = context.ChangeTracker.Entries<EntityTwo>().Select(e => e.Entity).OrderBy(e => e.Id).ToList();
                var allRight = context.ChangeTracker.Entries<EntityThree>().Select(e => e.Entity).OrderBy(e => e.Id).ToList();

                var count = 0;
                foreach (var left in allLeft)
                {
                    foreach (var right in allRight)
                    {
                        if (left.ThreeSkipFull?.Contains(right) == true)
                        {
                            Assert.Contains(left, right.TwoSkipFull);
                            count++;
                        }

                        if (right.TwoSkipFull?.Contains(left) == true)
                        {
                            Assert.Contains(right, left.ThreeSkipFull);
                            count++;
                        }
                    }
                }

                var deleted = context.ChangeTracker.Entries<JoinTwoToThree>().Count(e => e.State == EntityState.Deleted);
                Assert.Equal(joinCount, (count / 2) + deleted);
            }
        }

        [ConditionalFact]
        public virtual void Can_insert_many_to_many_with_inheritance()
        {
            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var leftEntities = new[]
                    {
                        new EntityOne { Id = 7711, BranchSkip = new List<EntityBranch>() },
                        new EntityOne { Id = 7712, BranchSkip = new List<EntityBranch>() },
                        new EntityOne { Id = 7713, BranchSkip = new List<EntityBranch>() }
                    };
                    var rightEntities = new[]
                    {
                        new EntityBranch { Id = 7721, OneSkip = new List<EntityOne>() },
                        new EntityBranch { Id = 7722, OneSkip = new List<EntityOne>() },
                        new EntityBranch { Id = 7723, OneSkip = new List<EntityOne>() }
                    };

                    leftEntities[0].BranchSkip.Add(rightEntities[0]); // 11 - 21
                    leftEntities[0].BranchSkip.Add(rightEntities[1]); // 11 - 22
                    leftEntities[0].BranchSkip.Add(rightEntities[2]); // 11 - 23

                    rightEntities[0].OneSkip.Add(leftEntities[0]); // 21 - 11 (Dupe)
                    rightEntities[0].OneSkip.Add(leftEntities[1]); // 21 - 12
                    rightEntities[0].OneSkip.Add(leftEntities[2]); // 21 - 13

                    context.AddRange(leftEntities[0], leftEntities[1], leftEntities[2]);
                    context.AddRange(rightEntities[0], rightEntities[1], rightEntities[2]);

                    ValidateFixup(context, leftEntities, rightEntities);

                    context.SaveChanges();

                    ValidateFixup(context, leftEntities, rightEntities);
                },
                context =>
                {
                    var results = context.Set<EntityOne>().Where(e => e.Id > 7700).Include(e => e.BranchSkip).ToList();
                    Assert.Equal(3, results.Count);

                    var leftEntities = context.ChangeTracker.Entries<EntityOne>().Select(e => e.Entity).OrderBy(e => e.Id).ToList();
                    var rightEntities = context.ChangeTracker.Entries<EntityBranch>().Select(e => e.Entity).OrderBy(e => e.Id).ToList();

                    ValidateFixup(context, leftEntities, rightEntities);
                });

            void ValidateFixup(DbContext context, IList<EntityOne> leftEntities, IList<EntityBranch> rightEntities)
            {
                Assert.Equal(11, context.ChangeTracker.Entries().Count());
                Assert.Equal(3, context.ChangeTracker.Entries<EntityOne>().Count());
                Assert.Equal(3, context.ChangeTracker.Entries<EntityBranch>().Count());
                Assert.Equal(5, context.ChangeTracker.Entries<JoinOneToBranch>().Count());

                Assert.Equal(3, leftEntities[0].BranchSkip.Count);
                Assert.Single(leftEntities[1].BranchSkip);
                Assert.Single(leftEntities[2].BranchSkip);

                Assert.Equal(3, rightEntities[0].OneSkip.Count);
                Assert.Single(rightEntities[1].OneSkip);
                Assert.Single(rightEntities[2].OneSkip);
            }
        }

        [ConditionalFact]
        public virtual void Can_update_many_to_many_with_inheritance()
        {
            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var leftEntities = context.Set<EntityOne>().Include(e => e.BranchSkip).OrderBy(e => e.Id).ToList();
                    var rightEntities = context.Set<EntityBranch>().Include(e => e.OneSkip).ToList();

                    leftEntities[0].BranchSkip.AddRange(
                        new[] { new EntityBranch { Id = 7721 }, new EntityBranch { Id = 7722 }, new EntityBranch { Id = 7723 } });

                    rightEntities[0].OneSkip.AddRange(
                        new[] { new EntityOne { Id = 7711 }, new EntityOne { Id = 7712 }, new EntityOne { Id = 7713 } });

                    leftEntities[1].BranchSkip.Remove(leftEntities[1].BranchSkip.Single(e => e.Id == 16));
                    rightEntities[1].OneSkip.Remove(rightEntities[1].OneSkip.Single(e => e.Id == 9));

                    leftEntities[2].BranchSkip.Remove(leftEntities[2].BranchSkip.Single(e => e.Id == 14));
                    leftEntities[2].BranchSkip.Add(new EntityBranch { Id = 7724 });

                    rightEntities[2].OneSkip.Remove(rightEntities[2].OneSkip.Single(e => e.Id == 8));
                    rightEntities[2].OneSkip.Add(new EntityOne { Id = 7714 });

                    context.ChangeTracker.DetectChanges();

                    ValidateFixup(context, leftEntities, rightEntities, 24, 14, 55);

                    context.SaveChanges();

                    ValidateFixup(context, leftEntities, rightEntities, 24, 14, 55 - 4);
                },
                context =>
                {
                    var leftEntities = context.Set<EntityOne>().Include(e => e.BranchSkip).OrderBy(e => e.Id).ToList();
                    var rightEntities = context.Set<EntityBranch>().Include(e => e.OneSkip).ToList();

                    ValidateFixup(context, leftEntities, rightEntities, 24, 14, 55 - 4);
                });

            void ValidateFixup(
                DbContext context,
                List<EntityOne> leftEntities,
                List<EntityBranch> rightEntities,
                int leftCount,
                int rightCount,
                int joinCount)
            {
                Assert.Equal(leftCount, context.ChangeTracker.Entries<EntityOne>().Count());
                Assert.Equal(rightCount, context.ChangeTracker.Entries<EntityBranch>().Count());
                Assert.Equal(joinCount, context.ChangeTracker.Entries<JoinOneToBranch>().Count());
                Assert.Equal(leftCount + rightCount + joinCount, context.ChangeTracker.Entries().Count());

                Assert.Contains(leftEntities[0].BranchSkip, e => e.Id == 7721);
                Assert.Contains(leftEntities[0].BranchSkip, e => e.Id == 7722);
                Assert.Contains(leftEntities[0].BranchSkip, e => e.Id == 7723);

                Assert.Contains(rightEntities[0].OneSkip, e => e.Id == 7711);
                Assert.Contains(rightEntities[0].OneSkip, e => e.Id == 7712);
                Assert.Contains(rightEntities[0].OneSkip, e => e.Id == 7713);

                Assert.DoesNotContain(leftEntities[1].BranchSkip, e => e.Id == 1);
                Assert.DoesNotContain(rightEntities[1].OneSkip, e => e.Id == 1);

                Assert.DoesNotContain(leftEntities[2].BranchSkip, e => e.Id == 1);
                Assert.Contains(leftEntities[2].BranchSkip, e => e.Id == 7724);

                Assert.DoesNotContain(rightEntities[2].OneSkip, e => e.Id == 1);
                Assert.Contains(rightEntities[2].OneSkip, e => e.Id == 7714);

                var allLeft = context.ChangeTracker.Entries<EntityOne>().Select(e => e.Entity).OrderBy(e => e.Id).ToList();
                var allRight = context.ChangeTracker.Entries<EntityBranch>().Select(e => e.Entity).OrderBy(e => e.Id).ToList();

                var count = 0;
                foreach (var left in allLeft)
                {
                    foreach (var right in allRight)
                    {
                        if (left.BranchSkip?.Contains(right) == true)
                        {
                            Assert.Contains(left, right.OneSkip);
                            count++;
                        }

                        if (right.OneSkip?.Contains(left) == true)
                        {
                            Assert.Contains(right, left.BranchSkip);
                            count++;
                        }
                    }
                }

                var deleted = context.ChangeTracker.Entries<JoinOneToBranch>().Count(e => e.State == EntityState.Deleted);
                Assert.Equal(joinCount, (count / 2) + deleted);
            }
        }

        [ConditionalFact]
        public virtual void Can_insert_many_to_many_self_with_payload()
        {
            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var leftEntities = new[]
                    {
                        new EntityOne { Id = 7711, SelfSkipPayloadLeft = new List<EntityOne>() },
                        new EntityOne { Id = 7712, SelfSkipPayloadLeft = new List<EntityOne>() },
                        new EntityOne { Id = 7713, SelfSkipPayloadLeft = new List<EntityOne>() }
                    };
                    var rightEntities = new[]
                    {
                        new EntityOne { Id = 7721, SelfSkipPayloadRight = new List<EntityOne>() },
                        new EntityOne { Id = 7722, SelfSkipPayloadRight = new List<EntityOne>() },
                        new EntityOne { Id = 7723, SelfSkipPayloadRight = new List<EntityOne>() }
                    };

                    leftEntities[0].SelfSkipPayloadLeft.Add(rightEntities[0]); // 11 - 21
                    leftEntities[0].SelfSkipPayloadLeft.Add(rightEntities[1]); // 11 - 22
                    leftEntities[0].SelfSkipPayloadLeft.Add(rightEntities[2]); // 11 - 23

                    rightEntities[0].SelfSkipPayloadRight.Add(leftEntities[0]); // 21 - 11 (Dupe)
                    rightEntities[0].SelfSkipPayloadRight.Add(leftEntities[1]); // 21 - 12
                    rightEntities[0].SelfSkipPayloadRight.Add(leftEntities[2]); // 21 - 13

                    context.AddRange(leftEntities[0], leftEntities[1], leftEntities[2]);
                    context.AddRange(rightEntities[0], rightEntities[1], rightEntities[2]);

                    ValidateFixup(context, leftEntities, rightEntities, postSave: false);

                    context.SaveChanges();

                    ValidateFixup(context, leftEntities, rightEntities, postSave: true);
                },
                context =>
                {
                    var results = context.Set<EntityOne>().Where(e => e.Id > 7700).Include(e => e.SelfSkipPayloadLeft).ToList();
                    Assert.Equal(6, results.Count);

                    var leftEntities = context.ChangeTracker.Entries<EntityOne>()
                        .Select(e => e.Entity)
                        .Where(e => e.Id < 7720)
                        .OrderBy(e => e.Id)
                        .ToList();

                    var rightEntities = context.ChangeTracker.Entries<EntityOne>()
                        .Select(e => e.Entity)
                        .Where(e => e.Id > 7720)
                        .OrderBy(e => e.Id)
                        .ToList();

                    ValidateFixup(context, leftEntities, rightEntities, postSave: true);
                });

            void ValidateFixup(DbContext context, IList<EntityOne> leftEntities, IList<EntityOne> rightEntities, bool postSave)
            {
                Assert.Equal(11, context.ChangeTracker.Entries().Count());
                Assert.Equal(6, context.ChangeTracker.Entries<EntityOne>().Count());
                Assert.Equal(5, context.ChangeTracker.Entries<JoinOneSelfPayload>().Count());

                Assert.Equal(3, leftEntities[0].SelfSkipPayloadLeft.Count);
                Assert.Single(leftEntities[1].SelfSkipPayloadLeft);
                Assert.Single(leftEntities[2].SelfSkipPayloadLeft);

                Assert.Equal(3, rightEntities[0].SelfSkipPayloadRight.Count);
                Assert.Single(rightEntities[1].SelfSkipPayloadRight);
                Assert.Single(rightEntities[2].SelfSkipPayloadRight);

                foreach (var joinEntity in context.ChangeTracker.Entries<JoinOneSelfPayload>().Select(e => e.Entity).ToList())
                {
                    Assert.Equal(joinEntity.Left.Id, joinEntity.LeftId);
                    Assert.Equal(joinEntity.Right.Id, joinEntity.RightId);
                    Assert.Contains(joinEntity, joinEntity.Left.JoinSelfPayloadLeft);
                    Assert.Contains(joinEntity, joinEntity.Right.JoinSelfPayloadRight);

                    if (postSave
                        && SupportsDatabaseDefaults)
                    {
                        Assert.True(joinEntity.Payload >= DateTime.Now - new TimeSpan(7, 0, 0, 0));
                    }
                    else
                    {
                        Assert.Equal(default, joinEntity.Payload);
                    }
                }
            }
        }

        [ConditionalFact]
        public virtual void Can_update_many_to_many_self_with_payload()
        {
            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var leftEntities = context.Set<EntityOne>().Include(e => e.SelfSkipPayloadRight).OrderBy(e => e.Id).ToList();
                    var rightEntities = context.Set<EntityOne>().Include(e => e.SelfSkipPayloadLeft).ToList();

                    leftEntities[0].SelfSkipPayloadRight.AddRange(new[]
                    {
                        new EntityOne { Id = 7721 },
                        new EntityOne { Id = 7722 },
                        new EntityOne { Id = 7723 }
                    });

                    rightEntities[0].SelfSkipPayloadLeft.AddRange(new[]
                    {
                        new EntityOne { Id = 7711 },
                        new EntityOne { Id = 7712 },
                        new EntityOne { Id = 7713 }
                    });

                    leftEntities[2].SelfSkipPayloadRight.Remove(leftEntities[2].SelfSkipPayloadRight.Single(e => e.Id == 6));
                    rightEntities[1].SelfSkipPayloadLeft.Remove(rightEntities[1].SelfSkipPayloadLeft.Single(e => e.Id == 13));

                    leftEntities[4].SelfSkipPayloadRight.Remove(leftEntities[4].SelfSkipPayloadRight.Single(e => e.Id == 3));
                    leftEntities[4].SelfSkipPayloadRight.Add(new EntityOne { Id = 7724 });

                    rightEntities[2].SelfSkipPayloadLeft.Remove(rightEntities[2].SelfSkipPayloadLeft.Single(e => e.Id == 5));
                    rightEntities[2].SelfSkipPayloadLeft.Add(new EntityOne { Id = 7714 });

                    context.ChangeTracker.DetectChanges();

                    context.Find<JoinOneSelfPayload>(7712, 1).Payload = new DateTime(1973, 9, 3);
                    context.Find<JoinOneSelfPayload>(20, 16).Payload = new DateTime(1969, 8, 3);

                    ValidateFixup(context, leftEntities, rightEntities, 28, 37, postSave: false);

                    context.SaveChanges();

                    ValidateFixup(context, leftEntities, rightEntities, 28, 37 - 3, postSave: true);
                },
                context =>
                {
                    var leftEntities = context.Set<EntityOne>().Include(e => e.SelfSkipPayloadRight).OrderBy(e => e.Id).ToList();
                    var rightEntities = context.Set<EntityOne>().Include(e => e.SelfSkipPayloadLeft).ToList();

                    ValidateFixup(context, leftEntities, rightEntities, 28, 37 - 3, postSave: true);
                });

            void ValidateFixup(
                DbContext context,
                List<EntityOne> leftEntities,
                List<EntityOne> rightEntities,
                int count,
                int joinCount,
                bool postSave)
            {
                Assert.Equal(count, context.ChangeTracker.Entries<EntityOne>().Count());
                Assert.Equal(joinCount, context.ChangeTracker.Entries<JoinOneSelfPayload>().Count());
                Assert.Equal(count + joinCount, context.ChangeTracker.Entries().Count());

                Assert.Contains(leftEntities[0].SelfSkipPayloadRight, e => e.Id == 7721);
                Assert.Contains(leftEntities[0].SelfSkipPayloadRight, e => e.Id == 7722);
                Assert.Contains(leftEntities[0].SelfSkipPayloadRight, e => e.Id == 7723);

                Assert.Contains(rightEntities[0].SelfSkipPayloadLeft, e => e.Id == 7711);
                Assert.Contains(rightEntities[0].SelfSkipPayloadLeft, e => e.Id == 7712);
                Assert.Contains(rightEntities[0].SelfSkipPayloadLeft, e => e.Id == 7713);

                Assert.DoesNotContain(leftEntities[2].SelfSkipPayloadRight, e => e.Id == 6);
                Assert.DoesNotContain(rightEntities[1].SelfSkipPayloadLeft, e => e.Id == 13);

                Assert.DoesNotContain(leftEntities[4].SelfSkipPayloadRight, e => e.Id == 3);
                Assert.Contains(leftEntities[4].SelfSkipPayloadRight, e => e.Id == 7724);

                Assert.DoesNotContain(rightEntities[2].SelfSkipPayloadLeft, e => e.Id == 5);
                Assert.Contains(rightEntities[2].SelfSkipPayloadLeft, e => e.Id == 7714);

                foreach (var joinEntry in context.ChangeTracker.Entries<JoinOneSelfPayload>().ToList())
                {
                    var joinEntity = joinEntry.Entity;
                    Assert.Equal(joinEntity.Left.Id, joinEntity.LeftId);
                    Assert.Equal(joinEntity.Right.Id, joinEntity.RightId);
                    Assert.Contains(joinEntity, joinEntity.Left.JoinSelfPayloadLeft);
                    Assert.Contains(joinEntity, joinEntity.Right.JoinSelfPayloadRight);

                    if (joinEntity.LeftId == 7712
                        && joinEntity.RightId == 1)
                    {
                        Assert.Equal(postSave ? EntityState.Unchanged : EntityState.Added, joinEntry.State);
                        Assert.Equal(new DateTime(1973, 9, 3), joinEntity.Payload);
                    }
                    else if (joinEntity.LeftId == 20
                        && joinEntity.RightId == 20)
                    {
                        Assert.Equal(postSave ? EntityState.Unchanged : EntityState.Modified, joinEntry.State);
                        Assert.Equal(!postSave, joinEntry.Property(e => e.Payload).IsModified);
                        Assert.Equal(new DateTime(1969, 8, 3), joinEntity.Payload);
                    }
                }

                var allLeft = context.ChangeTracker.Entries<EntityOne>().Select(e => e.Entity).OrderBy(e => e.Id).ToList();
                var allRight = context.ChangeTracker.Entries<EntityOne>().Select(e => e.Entity).OrderBy(e => e.Id).ToList();

                var joins = 0;
                foreach (var left in allLeft)
                {
                    foreach (var right in allRight)
                    {
                        if (left.SelfSkipPayloadRight?.Contains(right) == true)
                        {
                            Assert.Contains(left, right.SelfSkipPayloadLeft);
                            joins++;
                        }

                        if (right.SelfSkipPayloadLeft?.Contains(left) == true)
                        {
                            Assert.Contains(right, left.SelfSkipPayloadRight);
                            joins++;
                        }
                    }
                }

                var deleted = context.ChangeTracker.Entries<JoinOneSelfPayload>().Count(e => e.State == EntityState.Deleted);
                Assert.Equal(joinCount, (joins / 2) + deleted);
            }
        }

        [ConditionalFact]
        public virtual void Can_insert_many_to_many_shared_with_payload()
        {
            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var leftEntities = new[]
                    {
                        new EntityOne { Id = 7711, ThreeSkipPayloadFullShared = new List<EntityThree>() },
                        new EntityOne { Id = 7712, ThreeSkipPayloadFullShared = new List<EntityThree>() },
                        new EntityOne { Id = 7713, ThreeSkipPayloadFullShared = new List<EntityThree>() }
                    };
                    var rightEntities = new[]
                    {
                        new EntityThree { Id = 7721, OneSkipPayloadFullShared = new List<EntityOne>() },
                        new EntityThree { Id = 7722, OneSkipPayloadFullShared = new List<EntityOne>() },
                        new EntityThree { Id = 7723, OneSkipPayloadFullShared = new List<EntityOne>() }
                    };

                    leftEntities[0].ThreeSkipPayloadFullShared.Add(rightEntities[0]); // 11 - 21
                    leftEntities[0].ThreeSkipPayloadFullShared.Add(rightEntities[1]); // 11 - 22
                    leftEntities[0].ThreeSkipPayloadFullShared.Add(rightEntities[2]); // 11 - 23

                    rightEntities[0].OneSkipPayloadFullShared.Add(leftEntities[0]); // 21 - 11 (Dupe)
                    rightEntities[0].OneSkipPayloadFullShared.Add(leftEntities[1]); // 21 - 12
                    rightEntities[0].OneSkipPayloadFullShared.Add(leftEntities[2]); // 21 - 13

                    context.AddRange(leftEntities[0], leftEntities[1], leftEntities[2]);
                    context.AddRange(rightEntities[0], rightEntities[1], rightEntities[2]);

                    ValidateFixup(context, leftEntities, rightEntities, postSave: false);

                    context.SaveChanges();

                    ValidateFixup(context, leftEntities, rightEntities, postSave: true);
                },
                context =>
                {
                    var results = context.Set<EntityOne>().Where(e => e.Id > 7700).Include(e => e.ThreeSkipPayloadFullShared).ToList();
                    Assert.Equal(3, results.Count);

                    var leftEntities = context.ChangeTracker.Entries<EntityOne>().Select(e => e.Entity).OrderBy(e => e.Id).ToList();
                    var rightEntities = context.ChangeTracker.Entries<EntityThree>().Select(e => e.Entity).OrderBy(e => e.Id).ToList();

                    ValidateFixup(context, leftEntities, rightEntities, postSave: true);
                });

            void ValidateFixup(DbContext context, IList<EntityOne> leftEntities, IList<EntityThree> rightEntities, bool postSave)
            {
                Assert.Equal(11, context.ChangeTracker.Entries().Count());
                Assert.Equal(3, context.ChangeTracker.Entries<EntityOne>().Count());
                Assert.Equal(3, context.ChangeTracker.Entries<EntityThree>().Count());
                Assert.Equal(5, context.ChangeTracker.Entries</*TODO: Shared*/JoinOneToThreePayloadFullShared>().Count());

                Assert.Equal(3, leftEntities[0].ThreeSkipPayloadFullShared.Count);
                Assert.Single(leftEntities[1].ThreeSkipPayloadFullShared);
                Assert.Single(leftEntities[2].ThreeSkipPayloadFullShared);

                Assert.Equal(3, rightEntities[0].OneSkipPayloadFullShared.Count);
                Assert.Single(rightEntities[1].OneSkipPayloadFullShared);
                Assert.Single(rightEntities[2].OneSkipPayloadFullShared);

                foreach (var joinEntity in context.ChangeTracker
                    .Entries< /*TODO: Shared*/JoinOneToThreePayloadFullShared>().Select(e => e.Entity).ToList())
                {
                    Assert.Equal(postSave && SupportsDatabaseDefaults ? "Generated" : default, joinEntity.Payload);
                }
            }
        }

        [ConditionalFact]
        public virtual void Can_update_many_to_many_shared_with_payload()
        {
            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var leftEntities = context.Set<EntityOne>().Include(e => e.ThreeSkipPayloadFullShared).OrderBy(e => e.Id).ToList();
                    var rightEntities = context.Set<EntityThree>().Include(e => e.OneSkipPayloadFullShared).ToList();

                    leftEntities[0].ThreeSkipPayloadFullShared.AddRange(new[]
                    {
                        new EntityThree { Id = 7721 },
                        new EntityThree { Id = 7722 },
                        new EntityThree { Id = 7723 }
                    });

                    rightEntities[0].OneSkipPayloadFullShared.AddRange(new[]
                    {
                        new EntityOne { Id = 7711 },
                        new EntityOne { Id = 7712 },
                        new EntityOne { Id = 7713 }
                    });

                    leftEntities[2].ThreeSkipPayloadFullShared.Remove(leftEntities[2].ThreeSkipPayloadFullShared.Single(e => e.Id == 2));
                    rightEntities[1].OneSkipPayloadFullShared.Remove(rightEntities[1].OneSkipPayloadFullShared.Single(e => e.Id == 3));

                    leftEntities[3].ThreeSkipPayloadFullShared.Remove(leftEntities[3].ThreeSkipPayloadFullShared.Single(e => e.Id == 5));
                    leftEntities[3].ThreeSkipPayloadFullShared.Add(new EntityThree { Id = 7724 });

                    rightEntities[2].OneSkipPayloadFullShared.Remove(rightEntities[2].OneSkipPayloadFullShared.Single(e => e.Id == 13));
                    rightEntities[2].OneSkipPayloadFullShared.Add(new EntityOne { Id = 7714 });

                    context.ChangeTracker.DetectChanges();

                    context.Find<JoinOneToThreePayloadFullShared>(7712, 1).Payload = "Set!";
                    context.Find<JoinOneToThreePayloadFullShared>(20, 16).Payload = "Changed!";

                    ValidateFixup(context, leftEntities, rightEntities, 24, 24, 48, postSave: false);

                    context.SaveChanges();

                    ValidateFixup(context, leftEntities, rightEntities, 24, 24, 48 - 3, postSave: true);
                },
                context =>
                {
                    var leftEntities = context.Set<EntityOne>().Include(e => e.ThreeSkipPayloadFullShared).OrderBy(e => e.Id).ToList();
                    var rightEntities = context.Set<EntityThree>().Include(e => e.OneSkipPayloadFullShared).ToList();

                    ValidateFixup(context, leftEntities, rightEntities, 24, 24, 48 - 3, postSave: true);
                });

            void ValidateFixup(
                DbContext context,
                List<EntityOne> leftEntities,
                List<EntityThree> rightEntities,
                int leftCount,
                int rightCount,
                int joinCount,
                bool postSave)
            {
                Assert.Equal(leftCount, context.ChangeTracker.Entries<EntityOne>().Count());
                Assert.Equal(rightCount, context.ChangeTracker.Entries<EntityThree>().Count());
                Assert.Equal(joinCount, context.ChangeTracker.Entries<JoinOneToThreePayloadFullShared>().Count());
                Assert.Equal(leftCount + rightCount + joinCount, context.ChangeTracker.Entries().Count());

                Assert.Contains(leftEntities[0].ThreeSkipPayloadFullShared, e => e.Id == 7721);
                Assert.Contains(leftEntities[0].ThreeSkipPayloadFullShared, e => e.Id == 7722);
                Assert.Contains(leftEntities[0].ThreeSkipPayloadFullShared, e => e.Id == 7723);

                Assert.Contains(rightEntities[0].OneSkipPayloadFullShared, e => e.Id == 7711);
                Assert.Contains(rightEntities[0].OneSkipPayloadFullShared, e => e.Id == 7712);
                Assert.Contains(rightEntities[0].OneSkipPayloadFullShared, e => e.Id == 7713);

                Assert.DoesNotContain(leftEntities[2].ThreeSkipPayloadFullShared, e => e.Id == 2);
                Assert.DoesNotContain(rightEntities[1].OneSkipPayloadFullShared, e => e.Id == 3);

                Assert.DoesNotContain(leftEntities[3].ThreeSkipPayloadFullShared, e => e.Id == 5);
                Assert.Contains(leftEntities[3].ThreeSkipPayloadFullShared, e => e.Id == 7724);

                Assert.DoesNotContain(rightEntities[2].OneSkipPayloadFullShared, e => e.Id == 13);
                Assert.Contains(rightEntities[2].OneSkipPayloadFullShared, e => e.Id == 7714);

                foreach (var joinEntry in context.ChangeTracker.Entries<JoinOneToThreePayloadFullShared>().ToList())
                {
                    var joinEntity = joinEntry.Entity;

                    if (joinEntity.OneId == 7712
                        && joinEntity.ThreeId == 1)
                    {
                        Assert.Equal(postSave ? EntityState.Unchanged : EntityState.Added, joinEntry.State);
                        Assert.Equal("Set!", joinEntity.Payload);
                    }
                    else if (joinEntity.OneId == 20
                        && joinEntity.ThreeId == 20)
                    {
                        Assert.Equal(postSave ? EntityState.Unchanged : EntityState.Modified, joinEntry.State);
                        Assert.Equal(!postSave, joinEntry.Property(e => e.Payload).IsModified);
                        Assert.Equal("Changed!", joinEntity.Payload);
                    }
                }

                var allLeft = context.ChangeTracker.Entries<EntityOne>().Select(e => e.Entity).OrderBy(e => e.Id).ToList();
                var allRight = context.ChangeTracker.Entries<EntityThree>().Select(e => e.Entity).OrderBy(e => e.Id).ToList();

                var count = 0;
                foreach (var left in allLeft)
                {
                    foreach (var right in allRight)
                    {
                        if (left.ThreeSkipPayloadFullShared?.Contains(right) == true)
                        {
                            Assert.Contains(left, right.OneSkipPayloadFullShared);
                            count++;
                        }

                        if (right.OneSkipPayloadFullShared?.Contains(left) == true)
                        {
                            Assert.Contains(right, left.ThreeSkipPayloadFullShared);
                            count++;
                        }
                    }
                }

                var deleted = context.ChangeTracker.Entries<JoinOneToThreePayloadFullShared>().Count(e => e.State == EntityState.Deleted);
                Assert.Equal(joinCount, (count / 2) + deleted);
            }
        }

        [ConditionalFact]
        public virtual void Can_insert_many_to_many_shared()
        {
            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var leftEntities = new[]
                    {
                        new EntityOne { Id = 7711, TwoSkipShared = new List<EntityTwo>() },
                        new EntityOne { Id = 7712, TwoSkipShared = new List<EntityTwo>() },
                        new EntityOne { Id = 7713, TwoSkipShared = new List<EntityTwo>() }
                    };
                    var rightEntities = new[]
                    {
                        new EntityTwo { Id = 7721, OneSkipShared = new List<EntityOne>() },
                        new EntityTwo { Id = 7722, OneSkipShared = new List<EntityOne>() },
                        new EntityTwo { Id = 7723, OneSkipShared = new List<EntityOne>() }
                    };

                    leftEntities[0].TwoSkipShared.Add(rightEntities[0]); // 11 - 21
                    leftEntities[0].TwoSkipShared.Add(rightEntities[1]); // 11 - 22
                    leftEntities[0].TwoSkipShared.Add(rightEntities[2]); // 11 - 23

                    rightEntities[0].OneSkipShared.Add(leftEntities[0]); // 21 - 11 (Dupe)
                    rightEntities[0].OneSkipShared.Add(leftEntities[1]); // 21 - 12
                    rightEntities[0].OneSkipShared.Add(leftEntities[2]); // 21 - 13

                    context.AddRange(leftEntities[0], leftEntities[1], leftEntities[2]);
                    context.AddRange(rightEntities[0], rightEntities[1], rightEntities[2]);

                    ValidateFixup(context, leftEntities, rightEntities);

                    context.SaveChanges();

                    ValidateFixup(context, leftEntities, rightEntities);
                },
                context =>
                {
                    var results = context.Set<EntityOne>().Where(e => e.Id > 7700).Include(e => e.TwoSkipShared).ToList();
                    Assert.Equal(3, results.Count);

                    var leftEntities = context.ChangeTracker.Entries<EntityOne>().Select(e => e.Entity).OrderBy(e => e.Id).ToList();
                    var rightEntities = context.ChangeTracker.Entries<EntityTwo>().Select(e => e.Entity).OrderBy(e => e.Id).ToList();

                    ValidateFixup(context, leftEntities, rightEntities);
                });

            void ValidateFixup(DbContext context, IList<EntityOne> leftEntities, IList<EntityTwo> rightEntities)
            {
                Assert.Equal(11, context.ChangeTracker.Entries().Count());
                Assert.Equal(3, context.ChangeTracker.Entries<EntityOne>().Count());
                Assert.Equal(3, context.ChangeTracker.Entries<EntityTwo>().Count());
                Assert.Equal(5, context.ChangeTracker.Entries< /*TODO: Shared*/JoinOneToTwoShared>().Count());

                Assert.Equal(3, leftEntities[0].TwoSkipShared.Count);
                Assert.Single(leftEntities[1].TwoSkipShared);
                Assert.Single(leftEntities[2].TwoSkipShared);

                Assert.Equal(3, rightEntities[0].OneSkipShared.Count);
                Assert.Single(rightEntities[1].OneSkipShared);
                Assert.Single(rightEntities[2].OneSkipShared);
            }
        }

        [ConditionalFact]
        public virtual void Can_update_many_to_many_shared()
        {
            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var leftEntities = context.Set<EntityOne>().Include(e => e.TwoSkipShared).OrderBy(e => e.Id).ToList();
                    var rightEntities = context.Set<EntityTwo>().Include(e => e.OneSkipShared).ToList();

                    leftEntities[0].TwoSkipShared.AddRange(
                        new[] { new EntityTwo { Id = 7721 }, new EntityTwo { Id = 7722 }, new EntityTwo { Id = 7723 } });

                    rightEntities[0].OneSkipShared.AddRange(
                        new[] { new EntityOne { Id = 7711 }, new EntityOne { Id = 7712 }, new EntityOne { Id = 7713 } });

                    leftEntities[1].TwoSkipShared.Remove(leftEntities[1].TwoSkipShared.Single(e => e.Id == 3));
                    rightEntities[1].OneSkipShared.Remove(rightEntities[1].OneSkipShared.Single(e => e.Id == 5));

                    leftEntities[2].TwoSkipShared.Remove(leftEntities[2].TwoSkipShared.Single(e => e.Id == 10));
                    leftEntities[2].TwoSkipShared.Add(new EntityTwo { Id = 7724 });

                    rightEntities[2].OneSkipShared.Remove(rightEntities[2].OneSkipShared.Single(e => e.Id == 7));
                    rightEntities[2].OneSkipShared.Add(new EntityOne { Id = 7714 });

                    context.ChangeTracker.DetectChanges();

                    ValidateFixup(context, leftEntities, rightEntities, 24, 24, 53);

                    context.SaveChanges();

                    ValidateFixup(context, leftEntities, rightEntities, 24, 24, 49);
                },
                context =>
                {
                    var leftEntities = context.Set<EntityOne>().Include(e => e.TwoSkipShared).OrderBy(e => e.Id).ToList();
                    var rightEntities = context.Set<EntityTwo>().Include(e => e.OneSkipShared).ToList();

                    ValidateFixup(context, leftEntities, rightEntities, 24, 24, 49);
                });

            void ValidateFixup(
                DbContext context,
                List<EntityOne> leftEntities,
                List<EntityTwo> rightEntities,
                int leftCount,
                int rightCount,
                int joinCount)
            {
                Assert.Equal(leftCount, context.ChangeTracker.Entries<EntityOne>().Count());
                Assert.Equal(rightCount, context.ChangeTracker.Entries<EntityTwo>().Count());
                Assert.Equal(joinCount, context.ChangeTracker.Entries<JoinOneToTwoShared>().Count());
                Assert.Equal(leftCount + rightCount + joinCount, context.ChangeTracker.Entries().Count());

                Assert.Contains(leftEntities[0].TwoSkipShared, e => e.Id == 7721);
                Assert.Contains(leftEntities[0].TwoSkipShared, e => e.Id == 7722);
                Assert.Contains(leftEntities[0].TwoSkipShared, e => e.Id == 7723);

                Assert.Contains(rightEntities[0].OneSkipShared, e => e.Id == 7711);
                Assert.Contains(rightEntities[0].OneSkipShared, e => e.Id == 7712);
                Assert.Contains(rightEntities[0].OneSkipShared, e => e.Id == 7713);

                Assert.DoesNotContain(leftEntities[1].TwoSkipShared, e => e.Id == 3);
                Assert.DoesNotContain(rightEntities[1].OneSkipShared, e => e.Id == 6);

                Assert.DoesNotContain(leftEntities[2].TwoSkipShared, e => e.Id == 10);
                Assert.Contains(leftEntities[2].TwoSkipShared, e => e.Id == 7724);

                Assert.DoesNotContain(rightEntities[2].OneSkipShared, e => e.Id == 7);
                Assert.Contains(rightEntities[2].OneSkipShared, e => e.Id == 7714);

                var allLeft = context.ChangeTracker.Entries<EntityOne>().Select(e => e.Entity).OrderBy(e => e.Id).ToList();
                var allRight = context.ChangeTracker.Entries<EntityTwo>().Select(e => e.Entity).OrderBy(e => e.Id).ToList();

                var count = 0;
                foreach (var left in allLeft)
                {
                    foreach (var right in allRight)
                    {
                        if (left.TwoSkipShared?.Contains(right) == true)
                        {
                            Assert.Contains(left, right.OneSkipShared);
                            count++;
                        }

                        if (right.OneSkipShared?.Contains(left) == true)
                        {
                            Assert.Contains(right, left.TwoSkipShared);
                            count++;
                        }
                    }
                }

                var deleted = context.ChangeTracker.Entries<JoinOneToTwoShared>().Count(e => e.State == EntityState.Deleted);
                Assert.Equal(joinCount, (count / 2) + deleted);
            }
        }

        [ConditionalFact]
        public virtual void Can_insert_many_to_many_with_payload()
        {
            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var leftEntities = new[]
                    {
                        new EntityOne { Id = 7711, ThreeSkipPayloadFull = new List<EntityThree>() },
                        new EntityOne { Id = 7712, ThreeSkipPayloadFull = new List<EntityThree>() },
                        new EntityOne { Id = 7713, ThreeSkipPayloadFull = new List<EntityThree>() }
                    };
                    var rightEntities = new[]
                    {
                        new EntityThree { Id = 7721, OneSkipPayloadFull = new List<EntityOne>() },
                        new EntityThree { Id = 7722, OneSkipPayloadFull = new List<EntityOne>() },
                        new EntityThree { Id = 7723, OneSkipPayloadFull = new List<EntityOne>() }
                    };

                    leftEntities[0].ThreeSkipPayloadFull.Add(rightEntities[0]); // 11 - 21
                    leftEntities[0].ThreeSkipPayloadFull.Add(rightEntities[1]); // 11 - 22
                    leftEntities[0].ThreeSkipPayloadFull.Add(rightEntities[2]); // 11 - 23

                    rightEntities[0].OneSkipPayloadFull.Add(leftEntities[0]); // 21 - 11 (Dupe)
                    rightEntities[0].OneSkipPayloadFull.Add(leftEntities[1]); // 21 - 12
                    rightEntities[0].OneSkipPayloadFull.Add(leftEntities[2]); // 21 - 13

                    context.AddRange(leftEntities[0], leftEntities[1], leftEntities[2]);
                    context.AddRange(rightEntities[0], rightEntities[1], rightEntities[2]);

                    ValidateFixup(context, leftEntities, rightEntities, postSave: false);

                    context.SaveChanges();

                    ValidateFixup(context, leftEntities, rightEntities, postSave: true);
                },
                context =>
                {
                    var results = context.Set<EntityOne>().Where(e => e.Id > 7700).Include(e => e.ThreeSkipPayloadFull).ToList();
                    Assert.Equal(3, results.Count);

                    var leftEntities = context.ChangeTracker.Entries<EntityOne>().Select(e => e.Entity).OrderBy(e => e.Id).ToList();
                    var rightEntities = context.ChangeTracker.Entries<EntityThree>().Select(e => e.Entity).OrderBy(e => e.Id).ToList();

                    ValidateFixup(context, leftEntities, rightEntities, postSave: true);
                });

            void ValidateFixup(DbContext context, IList<EntityOne> leftEntities, IList<EntityThree> rightEntities, bool postSave)
            {
                Assert.Equal(11, context.ChangeTracker.Entries().Count());
                Assert.Equal(3, context.ChangeTracker.Entries<EntityOne>().Count());
                Assert.Equal(3, context.ChangeTracker.Entries<EntityThree>().Count());
                Assert.Equal(5, context.ChangeTracker.Entries<JoinOneToThreePayloadFull>().Count());

                Assert.Equal(3, leftEntities[0].ThreeSkipPayloadFull.Count);
                Assert.Single(leftEntities[1].ThreeSkipPayloadFull);
                Assert.Single(leftEntities[2].ThreeSkipPayloadFull);

                Assert.Equal(3, rightEntities[0].OneSkipPayloadFull.Count);
                Assert.Single(rightEntities[1].OneSkipPayloadFull);
                Assert.Single(rightEntities[2].OneSkipPayloadFull);

                foreach (var joinEntity in context.ChangeTracker.Entries<JoinOneToThreePayloadFull>().Select(e => e.Entity).ToList())
                {
                    Assert.Equal(joinEntity.One.Id, joinEntity.OneId);
                    Assert.Equal(joinEntity.Three.Id, joinEntity.ThreeId);
                    Assert.Contains(joinEntity, joinEntity.One.JoinThreePayloadFull);
                    Assert.Contains(joinEntity, joinEntity.Three.JoinOnePayloadFull);

                    Assert.Equal(postSave && SupportsDatabaseDefaults ? "Generated" : default, joinEntity.Payload);
                }
            }
        }

        [ConditionalFact]
        public virtual void Can_update_many_to_many_with_payload()
        {
            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var leftEntities = context.Set<EntityOne>().Include(e => e.ThreeSkipPayloadFull).OrderBy(e => e.Id).ToList();
                    var rightEntities = context.Set<EntityThree>().Include(e => e.OneSkipPayloadFull).ToList();

                    leftEntities[0].ThreeSkipPayloadFull.AddRange(new[]
                    {
                        new EntityThree { Id = 7721 },
                        new EntityThree { Id = 7722 },
                        new EntityThree { Id = 7723 }
                    });

                    rightEntities[0].OneSkipPayloadFull.AddRange(new[]
                    {
                        new EntityOne { Id = 7711 },
                        new EntityOne { Id = 7712 },
                        new EntityOne { Id = 7713 }
                    });

                    leftEntities[1].ThreeSkipPayloadFull.Remove(leftEntities[1].ThreeSkipPayloadFull.Single(e => e.Id == 9));
                    rightEntities[1].OneSkipPayloadFull.Remove(rightEntities[1].OneSkipPayloadFull.Single(e => e.Id == 4));

                    leftEntities[2].ThreeSkipPayloadFull.Remove(leftEntities[2].ThreeSkipPayloadFull.Single(e => e.Id == 5));
                    leftEntities[2].ThreeSkipPayloadFull.Add(new EntityThree { Id = 7724 });

                    rightEntities[2].OneSkipPayloadFull.Remove(rightEntities[2].OneSkipPayloadFull.Single(e => e.Id == 8));
                    rightEntities[2].OneSkipPayloadFull.Add(new EntityOne { Id = 7714 });

                    context.ChangeTracker.DetectChanges();

                    context.Find<JoinOneToThreePayloadFull>(7712, 1).Payload = "Set!";
                    context.Find<JoinOneToThreePayloadFull>(20, 20).Payload = "Changed!";

                    ValidateFixup(context, leftEntities, rightEntities, 24, 24, 123, postSave: false);

                    context.SaveChanges();

                    ValidateFixup(context, leftEntities, rightEntities, 24, 24, 123 - 4, postSave: true);
                },
                context =>
                {
                    var leftEntities = context.Set<EntityOne>().Include(e => e.ThreeSkipPayloadFull).OrderBy(e => e.Id).ToList();
                    var rightEntities = context.Set<EntityThree>().Include(e => e.OneSkipPayloadFull).ToList();

                    ValidateFixup(context, leftEntities, rightEntities, 24, 24, 123 - 4, postSave: true);
                });

            void ValidateFixup(
                DbContext context,
                List<EntityOne> leftEntities,
                List<EntityThree> rightEntities,
                int leftCount,
                int rightCount,
                int joinCount,
                bool postSave)
            {
                Assert.Equal(leftCount, context.ChangeTracker.Entries<EntityOne>().Count());
                Assert.Equal(rightCount, context.ChangeTracker.Entries<EntityThree>().Count());
                Assert.Equal(joinCount, context.ChangeTracker.Entries<JoinOneToThreePayloadFull>().Count());
                Assert.Equal(leftCount + rightCount + joinCount, context.ChangeTracker.Entries().Count());

                Assert.Contains(leftEntities[0].ThreeSkipPayloadFull, e => e.Id == 7721);
                Assert.Contains(leftEntities[0].ThreeSkipPayloadFull, e => e.Id == 7722);
                Assert.Contains(leftEntities[0].ThreeSkipPayloadFull, e => e.Id == 7723);

                Assert.Contains(rightEntities[0].OneSkipPayloadFull, e => e.Id == 7711);
                Assert.Contains(rightEntities[0].OneSkipPayloadFull, e => e.Id == 7712);
                Assert.Contains(rightEntities[0].OneSkipPayloadFull, e => e.Id == 7713);

                Assert.DoesNotContain(leftEntities[1].ThreeSkipPayloadFull, e => e.Id == 9);
                Assert.DoesNotContain(rightEntities[1].OneSkipPayloadFull, e => e.Id == 4);

                Assert.DoesNotContain(leftEntities[2].ThreeSkipPayloadFull, e => e.Id == 5);
                Assert.Contains(leftEntities[2].ThreeSkipPayloadFull, e => e.Id == 7724);

                Assert.DoesNotContain(rightEntities[2].OneSkipPayloadFull, e => e.Id == 8);
                Assert.Contains(rightEntities[2].OneSkipPayloadFull, e => e.Id == 7714);

                foreach (var joinEntry in context.ChangeTracker.Entries<JoinOneToThreePayloadFull>().ToList())
                {
                    var joinEntity = joinEntry.Entity;
                    Assert.Equal(joinEntity.One.Id, joinEntity.OneId);
                    Assert.Equal(joinEntity.Three.Id, joinEntity.ThreeId);
                    Assert.Contains(joinEntity, joinEntity.One.JoinThreePayloadFull);
                    Assert.Contains(joinEntity, joinEntity.Three.JoinOnePayloadFull);

                    if (joinEntity.OneId == 7712
                        && joinEntity.ThreeId == 1)
                    {
                        Assert.Equal(postSave ? EntityState.Unchanged : EntityState.Added, joinEntry.State);
                        Assert.Equal("Set!", joinEntity.Payload);
                    }
                    else if (joinEntity.OneId == 20
                        && joinEntity.ThreeId == 20)
                    {
                        Assert.Equal(postSave ? EntityState.Unchanged : EntityState.Modified, joinEntry.State);
                        Assert.Equal(!postSave, joinEntry.Property(e => e.Payload).IsModified);
                        Assert.Equal("Changed!", joinEntity.Payload);
                    }
                }

                var allLeft = context.ChangeTracker.Entries<EntityOne>().Select(e => e.Entity).OrderBy(e => e.Id).ToList();
                var allRight = context.ChangeTracker.Entries<EntityThree>().Select(e => e.Entity).OrderBy(e => e.Id).ToList();

                var count = 0;
                foreach (var left in allLeft)
                {
                    foreach (var right in allRight)
                    {
                        if (left.ThreeSkipPayloadFull?.Contains(right) == true)
                        {
                            Assert.Contains(left, right.OneSkipPayloadFull);
                            count++;
                        }

                        if (right.OneSkipPayloadFull?.Contains(left) == true)
                        {
                            Assert.Contains(right, left.ThreeSkipPayloadFull);
                            count++;
                        }
                    }
                }

                var deleted = context.ChangeTracker.Entries<JoinOneToThreePayloadFull>().Count(e => e.State == EntityState.Deleted);
                Assert.Equal(joinCount, (count / 2) + deleted);
            }
        }

        [ConditionalFact]
        public virtual void Can_delete_with_many_to_many_with_navs()
        {
            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var ones = context.Set<EntityOne>().Include(e => e.ThreeSkipPayloadFull).OrderBy(e => e.Id).ToList();
                    var threes = context.Set<EntityThree>().Include(e => e.OneSkipPayloadFull).ToList();

                    // Make sure other related entities are loaded for delete fixup
                    context.Set<EntityTwo>().Load();
                    context.Set<JoinOneSelfPayload>().Load();
                    context.Set<JoinOneToTwo>().Load();

                    var toRemoveOne = context.Find<EntityOne>(1);
                    var refCountOnes = threes.SelectMany(e => e.OneSkipPayloadFull).Count(e => e == toRemoveOne);

                    var toRemoveThree = context.Find<EntityThree>(1);
                    var refCountThrees = ones.SelectMany(e => e.ThreeSkipPayloadFull).Count(e => e == toRemoveThree);

                    foreach (var joinEntity in context.ChangeTracker.Entries<JoinOneToThreePayloadFull>().Select(e => e.Entity).ToList())
                    {
                        Assert.Equal(joinEntity.One.Id, joinEntity.OneId);
                        Assert.Equal(joinEntity.Three.Id, joinEntity.ThreeId);
                        Assert.Contains(joinEntity, joinEntity.One.JoinThreePayloadFull);
                        Assert.Contains(joinEntity, joinEntity.Three.JoinOnePayloadFull);
                    }

                    context.Remove(toRemoveOne);
                    context.Remove(toRemoveThree);

                    Assert.Equal(refCountOnes, threes.SelectMany(e => e.OneSkipPayloadFull).Count(e => e == toRemoveOne));
                    Assert.Equal(refCountThrees, ones.SelectMany(e => e.ThreeSkipPayloadFull).Count(e => e == toRemoveThree));

                    ValidateJoinNavigations(context);

                    context.ChangeTracker.DetectChanges();

                    Assert.Equal(refCountOnes, threes.SelectMany(e => e.OneSkipPayloadFull).Count(e => e == toRemoveOne));
                    Assert.Equal(refCountThrees, ones.SelectMany(e => e.ThreeSkipPayloadFull).Count(e => e == toRemoveThree));

                    ValidateJoinNavigations(context);

                    Assert.All(
                        context.ChangeTracker.Entries<JoinOneToThreePayloadFull>(), e => Assert.Equal(
                            e.Entity.OneId == 1
                            || e.Entity.ThreeId == 1
                                ? EntityState.Deleted
                                : EntityState.Unchanged, e.State));

                    context.SaveChanges();

                    Assert.Equal(0, threes.SelectMany(e => e.OneSkipPayloadFull).Count(e => e == toRemoveOne));
                    Assert.Equal(0, ones.SelectMany(e => e.ThreeSkipPayloadFull).Count(e => e == toRemoveThree));

                    ValidateJoinNavigations(context);

                    ones.Remove(toRemoveOne);
                    threes.Remove(toRemoveThree);

                    Assert.Equal(0, threes.SelectMany(e => e.OneSkipPayloadFull).Count(e => e == toRemoveOne));
                    Assert.Equal(0, ones.SelectMany(e => e.ThreeSkipPayloadFull).Count(e => e == toRemoveThree));

                    Assert.DoesNotContain(
                        context.ChangeTracker.Entries<JoinOneToThreePayloadFull>(),
                        e => e.Entity.OneId == 1 || e.Entity.ThreeId == 1);
                },
                context =>
                {
                    var ones = context.Set<EntityOne>().Include(e => e.ThreeSkipPayloadFull).OrderBy(e => e.Id).ToList();
                    var threes = context.Set<EntityThree>().Include(e => e.OneSkipPayloadFull).ToList();

                    ValidateNavigations(ones, threes);

                    Assert.DoesNotContain(
                        context.ChangeTracker.Entries<JoinOneToThreePayloadFull>(),
                        e => e.Entity.OneId == 1 || e.Entity.ThreeId == 1);
                });

            void ValidateNavigations(List<EntityOne> ones, List<EntityThree> threes)
            {
                foreach (var one in ones)
                {
                    if (one.ThreeSkipPayloadFull != null)
                    {
                        Assert.DoesNotContain(one.ThreeSkipPayloadFull, e => e.Id == 1);
                    }

                    if (one.JoinThreePayloadFull != null)
                    {
                        Assert.DoesNotContain(one.JoinThreePayloadFull, e => e.OneId == 1);
                        Assert.DoesNotContain(one.JoinThreePayloadFull, e => e.ThreeId == 1);
                    }
                }

                foreach (var three in threes)
                {
                    if (three.OneSkipPayloadFull != null)
                    {
                        Assert.DoesNotContain(three.OneSkipPayloadFull, e => e.Id == 1);
                    }

                    if (three.JoinOnePayloadFull != null)
                    {
                        Assert.DoesNotContain(three.JoinOnePayloadFull, e => e.OneId == 1);
                        Assert.DoesNotContain(three.JoinOnePayloadFull, e => e.ThreeId == 1);
                    }
                }
            }

            void ValidateJoinNavigations(DbContext context)
            {
                foreach (var joinEntity in context.ChangeTracker.Entries<JoinOneToThreePayloadFull>().Select(e => e.Entity).ToList())
                {
                    Assert.Equal(joinEntity.One.Id, joinEntity.OneId);
                    Assert.Equal(joinEntity.Three.Id, joinEntity.ThreeId);
                    Assert.Contains(joinEntity, joinEntity.One.JoinThreePayloadFull);
                    Assert.Contains(joinEntity, joinEntity.Three.JoinOnePayloadFull);
                }
            }
        }

        [ConditionalFact]
        public virtual void Can_insert_many_to_many()
        {
            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var leftEntities = new[]
                    {
                        new EntityOne { Id = 7711, TwoSkip = new List<EntityTwo>() },
                        new EntityOne { Id = 7712, TwoSkip = new List<EntityTwo>() },
                        new EntityOne { Id = 7713, TwoSkip = new List<EntityTwo>() }
                    };
                    var rightEntities = new[]
                    {
                        new EntityTwo { Id = 7721, OneSkip = new List<EntityOne>() },
                        new EntityTwo { Id = 7722, OneSkip = new List<EntityOne>() },
                        new EntityTwo { Id = 7723, OneSkip = new List<EntityOne>() }
                    };

                    leftEntities[0].TwoSkip.Add(rightEntities[0]); // 11 - 21
                    leftEntities[0].TwoSkip.Add(rightEntities[1]); // 11 - 22
                    leftEntities[0].TwoSkip.Add(rightEntities[2]); // 11 - 23

                    rightEntities[0].OneSkip.Add(leftEntities[0]); // 21 - 11 (Dupe)
                    rightEntities[0].OneSkip.Add(leftEntities[1]); // 21 - 12
                    rightEntities[0].OneSkip.Add(leftEntities[2]); // 21 - 13

                    context.AddRange(leftEntities[0], leftEntities[1], leftEntities[2]);
                    context.AddRange(rightEntities[0], rightEntities[1], rightEntities[2]);

                    ValidateFixup(context, leftEntities, rightEntities);

                    context.SaveChanges();

                    ValidateFixup(context, leftEntities, rightEntities);
                },
                context =>
                {
                    var results = context.Set<EntityOne>().Where(e => e.Id > 7700).Include(e => e.TwoSkip).ToList();
                    Assert.Equal(3, results.Count);

                    var leftEntities = context.ChangeTracker.Entries<EntityOne>().Select(e => e.Entity).OrderBy(e => e.Id).ToList();
                    var rightEntities = context.ChangeTracker.Entries<EntityTwo>().Select(e => e.Entity).OrderBy(e => e.Id).ToList();

                    ValidateFixup(context, leftEntities, rightEntities);
                });

            void ValidateFixup(DbContext context, IList<EntityOne> leftEntities, IList<EntityTwo> rightEntities)
            {
                Assert.Equal(11, context.ChangeTracker.Entries().Count());
                Assert.Equal(3, context.ChangeTracker.Entries<EntityOne>().Count());
                Assert.Equal(3, context.ChangeTracker.Entries<EntityTwo>().Count());
                Assert.Equal(5, context.ChangeTracker.Entries<JoinOneToTwo>().Count());

                Assert.Equal(3, leftEntities[0].TwoSkip.Count);
                Assert.Single(leftEntities[1].TwoSkip);
                Assert.Single(leftEntities[2].TwoSkip);

                Assert.Equal(3, rightEntities[0].OneSkip.Count);
                Assert.Single(rightEntities[1].OneSkip);
                Assert.Single(rightEntities[2].OneSkip);
            }
        }

        [ConditionalFact]
        public virtual void Can_update_many_to_many()
        {
            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var leftEntities = context.Set<EntityOne>().Include(e => e.TwoSkip).OrderBy(e => e.Id).ToList();
                    var rightEntities = context.Set<EntityTwo>().Include(e => e.OneSkip).ToList();

                    leftEntities[0].TwoSkip.AddRange(
                        new[] { new EntityTwo { Id = 7721 }, new EntityTwo { Id = 7722 }, new EntityTwo { Id = 7723 } });

                    rightEntities[0].OneSkip.AddRange(
                        new[] { new EntityOne { Id = 7711 }, new EntityOne { Id = 7712 }, new EntityOne { Id = 7713 } });

                    leftEntities[1].TwoSkip.Remove(leftEntities[1].TwoSkip.Single(e => e.Id == 1));
                    rightEntities[1].OneSkip.Remove(rightEntities[1].OneSkip.Single(e => e.Id == 1));

                    leftEntities[2].TwoSkip.Remove(leftEntities[2].TwoSkip.Single(e => e.Id == 1));
                    leftEntities[2].TwoSkip.Add(new EntityTwo { Id = 7724 });

                    rightEntities[2].OneSkip.Remove(rightEntities[2].OneSkip.Single(e => e.Id == 1));
                    rightEntities[2].OneSkip.Add(new EntityOne { Id = 7714 });

                    context.ChangeTracker.DetectChanges();

                    ValidateFixup(context, leftEntities, rightEntities, 24, 24, 120);

                    context.SaveChanges();

                    ValidateFixup(context, leftEntities, rightEntities, 24, 24, 116);
                },
                context =>
                {
                    var leftEntities = context.Set<EntityOne>().Include(e => e.TwoSkip).OrderBy(e => e.Id).ToList();
                    var rightEntities = context.Set<EntityTwo>().Include(e => e.OneSkip).ToList();

                    ValidateFixup(context, leftEntities, rightEntities, 24, 24, 116);
                });

            void ValidateFixup(
                DbContext context,
                List<EntityOne> leftEntities,
                List<EntityTwo> rightEntities,
                int leftCount,
                int rightCount,
                int joinCount)
            {
                Assert.Equal(leftCount, context.ChangeTracker.Entries<EntityOne>().Count());
                Assert.Equal(rightCount, context.ChangeTracker.Entries<EntityTwo>().Count());
                Assert.Equal(joinCount, context.ChangeTracker.Entries<JoinOneToTwo>().Count());
                Assert.Equal(leftCount + rightCount + joinCount, context.ChangeTracker.Entries().Count());

                Assert.Contains(leftEntities[0].TwoSkip, e => e.Id == 7721);
                Assert.Contains(leftEntities[0].TwoSkip, e => e.Id == 7722);
                Assert.Contains(leftEntities[0].TwoSkip, e => e.Id == 7723);

                Assert.Contains(rightEntities[0].OneSkip, e => e.Id == 7711);
                Assert.Contains(rightEntities[0].OneSkip, e => e.Id == 7712);
                Assert.Contains(rightEntities[0].OneSkip, e => e.Id == 7713);

                Assert.DoesNotContain(leftEntities[1].TwoSkip, e => e.Id == 1);
                Assert.DoesNotContain(rightEntities[1].OneSkip, e => e.Id == 1);

                Assert.DoesNotContain(leftEntities[2].TwoSkip, e => e.Id == 1);
                Assert.Contains(leftEntities[2].TwoSkip, e => e.Id == 7724);

                Assert.DoesNotContain(rightEntities[2].OneSkip, e => e.Id == 1);
                Assert.Contains(rightEntities[2].OneSkip, e => e.Id == 7714);

                var allLeft = context.ChangeTracker.Entries<EntityOne>().Select(e => e.Entity).OrderBy(e => e.Id).ToList();
                var allRight = context.ChangeTracker.Entries<EntityTwo>().Select(e => e.Entity).OrderBy(e => e.Id).ToList();

                var count = 0;
                foreach (var left in allLeft)
                {
                    foreach (var right in allRight)
                    {
                        if (left.TwoSkip?.Contains(right) == true)
                        {
                            Assert.Contains(left, right.OneSkip);
                            count++;
                        }

                        if (right.OneSkip?.Contains(left) == true)
                        {
                            Assert.Contains(right, left.TwoSkip);
                            count++;
                        }
                    }
                }

                var deleted = context.ChangeTracker.Entries<JoinOneToTwo>().Count(e => e.State == EntityState.Deleted);
                Assert.Equal(joinCount, (count / 2) + deleted);
            }
        }

        [ConditionalFact]
        public virtual void Can_delete_with_many_to_many()
        {
            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var ones = context.Set<EntityOne>().Include(e => e.TwoSkip).OrderBy(e => e.Id).ToList();
                    var twos = context.Set<EntityTwo>().Include(e => e.OneSkip).ToList();

                    // Make sure other related entities are loaded for delete fixup
                    context.Set<EntityThree>().Load();
                    context.Set<JoinOneToThreePayloadFull>().Load();
                    context.Set<JoinOneSelfPayload>().Load();

                    var toRemoveOne = context.Find<EntityOne>(1);
                    var refCountOnes = twos.SelectMany(e => e.OneSkip).Count(e => e == toRemoveOne);

                    var toRemoveTwo = context.Find<EntityTwo>(1);
                    var refCountTwos = ones.SelectMany(e => e.TwoSkip).Count(e => e == toRemoveTwo);

                    context.Remove(toRemoveOne);
                    context.Remove(toRemoveTwo);

                    Assert.Equal(refCountOnes, twos.SelectMany(e => e.OneSkip).Count(e => e == toRemoveOne));
                    Assert.Equal(refCountTwos, ones.SelectMany(e => e.TwoSkip).Count(e => e == toRemoveTwo));

                    context.ChangeTracker.DetectChanges();

                    Assert.Equal(refCountOnes, twos.SelectMany(e => e.OneSkip).Count(e => e == toRemoveOne));
                    Assert.Equal(refCountTwos, ones.SelectMany(e => e.TwoSkip).Count(e => e == toRemoveTwo));

                    Assert.All(
                        context.ChangeTracker.Entries<JoinOneToTwo>(), e => Assert.Equal(
                            e.Entity.OneId == 1
                            || e.Entity.TwoId == 1
                                ? EntityState.Deleted
                                : EntityState.Unchanged, e.State));

                    context.SaveChanges();

                    Assert.Equal(1, twos.SelectMany(e => e.OneSkip).Count(e => e == toRemoveOne));
                    Assert.Equal(1, ones.SelectMany(e => e.TwoSkip).Count(e => e == toRemoveTwo));

                    ones.Remove(toRemoveOne);
                    twos.Remove(toRemoveTwo);

                    Assert.Equal(0, twos.SelectMany(e => e.OneSkip).Count(e => e == toRemoveOne));
                    Assert.Equal(0, ones.SelectMany(e => e.TwoSkip).Count(e => e == toRemoveTwo));

                    ValidateNavigations(ones, twos);

                    Assert.DoesNotContain(context.ChangeTracker.Entries<JoinOneToTwo>(), e => e.Entity.OneId == 1 || e.Entity.TwoId == 1);
                },
                context =>
                {
                    var ones = context.Set<EntityOne>().Include(e => e.TwoSkip).OrderBy(e => e.Id).ToList();
                    var twos = context.Set<EntityTwo>().Include(e => e.OneSkip).ToList();

                    ValidateNavigations(ones, twos);
                    Assert.DoesNotContain(context.ChangeTracker.Entries<JoinOneToTwo>(), e => e.Entity.OneId == 1 || e.Entity.TwoId == 1);
                });

            void ValidateNavigations(List<EntityOne> ones, List<EntityTwo> twos)
            {
                foreach (var one in ones)
                {
                    if (one.TwoSkip != null)
                    {
                        Assert.DoesNotContain(one.TwoSkip, e => e.Id == 1);
                    }
                }

                foreach (var two in twos)
                {
                    if (two.OneSkip != null)
                    {
                        Assert.DoesNotContain(two.OneSkip, e => e.Id == 1);
                    }
                }
            }
        }

        [ConditionalFact]
        public virtual void Can_insert_many_to_many_fully_by_convention()
        {
            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var leftEntities = new[]
                    {
                        new ImplicitManyToManyA { Id = 7711, Bs = new List<ImplicitManyToManyB>() },
                        new ImplicitManyToManyA { Id = 7712, Bs = new List<ImplicitManyToManyB>() },
                        new ImplicitManyToManyA { Id = 7713, Bs = new List<ImplicitManyToManyB>() }
                    };
                    var rightEntities = new[]
                    {
                        new ImplicitManyToManyB { Id = 7721, As = new List<ImplicitManyToManyA>() },
                        new ImplicitManyToManyB { Id = 7722, As = new List<ImplicitManyToManyA>() },
                        new ImplicitManyToManyB { Id = 7723, As = new List<ImplicitManyToManyA>() }
                    };

                    leftEntities[0].Bs.Add(rightEntities[0]); // 11 - 21
                    leftEntities[0].Bs.Add(rightEntities[1]); // 11 - 22
                    leftEntities[0].Bs.Add(rightEntities[2]); // 11 - 23

                    rightEntities[0].As.Add(leftEntities[0]); // 21 - 11 (Dupe)
                    rightEntities[0].As.Add(leftEntities[1]); // 21 - 12
                    rightEntities[0].As.Add(leftEntities[2]); // 21 - 13

                    context.AddRange(leftEntities[0], leftEntities[1], leftEntities[2]);
                    context.AddRange(rightEntities[0], rightEntities[1], rightEntities[2]);

                    ValidateFixup(context, leftEntities, rightEntities);

                    context.SaveChanges();

                    ValidateFixup(context, leftEntities, rightEntities);
                },
                context =>
                {
                    // Query fails...
                    //var results = context.Set<ImplicitManyToManyA>().Where(e => e.Id > 7700).Include(e => e.Bs).ToList();
                    //Assert.Equal(3, results.Count);

                    //Assert.Equal(11, context.ChangeTracker.Entries().Count());
                    //Assert.Equal(3, context.ChangeTracker.Entries<ImplicitManyToManyA>().Count());
                    //Assert.Equal(3, context.ChangeTracker.Entries<ImplicitManyToManyB>().Count());
                    //Assert.Equal(5, context.ChangeTracker.Entries<Dictionary<string, object>>().Count());

                    //var leftEntities = context.ChangeTracker.Entries<ImplicitManyToManyA>().Select(e => e.Entity).OrderBy(e => e.Id).ToList();
                    //var rightEntities = context.ChangeTracker.Entries<ImplicitManyToManyB>().Select(e => e.Entity).OrderBy(e => e.Id).ToList();

                    //Assert.Equal(3, leftEntities[0].Bs.Count);
                    //Assert.Single(leftEntities[1].Bs);
                    //Assert.Single(leftEntities[2].Bs);

                    //Assert.Equal(3, rightEntities[0].As.Count);
                    //Assert.Single(rightEntities[1].As);
                    //Assert.Single(rightEntities[2].As);
                });

            void ValidateFixup(DbContext context, IList<ImplicitManyToManyA> leftEntities, IList<ImplicitManyToManyB> rightEntities)
            {
                Assert.Equal(11, context.ChangeTracker.Entries().Count());
                Assert.Equal(3, context.ChangeTracker.Entries<ImplicitManyToManyA>().Count());
                Assert.Equal(3, context.ChangeTracker.Entries<ImplicitManyToManyB>().Count());
                Assert.Equal(5, context.ChangeTracker.Entries<Dictionary<string, object>>().Count());

                Assert.Equal(3, leftEntities[0].Bs.Count);
                Assert.Single(leftEntities[1].Bs);
                Assert.Single(leftEntities[2].Bs);

                Assert.Equal(3, rightEntities[0].As.Count);
                Assert.Single(rightEntities[1].As);
                Assert.Single(rightEntities[2].As);
            }
        }

        [ConditionalTheory]
        [InlineData(new[] { 1, 2, 3 })]
        [InlineData(new[] { 2, 1, 3 })]
        [InlineData(new[] { 3, 1, 2 })]
        [InlineData(new[] { 3, 2, 1 })]
        [InlineData(new[] { 1, 3, 2 })]
        [InlineData(new[] { 2, 3, 1 })]
        public virtual void Can_load_entities_in_any_order(int[] order)
        {
            using var context = CreateContext();

            foreach (var i in order)
            {
                (i switch
                {
                    1 => (IQueryable<object>)context.Set<EntityOne>(),
                    2 => context.Set<EntityTwo>(),
                    3 => context.Set<JoinOneToTwo>(),
                    _ => throw new ArgumentException()
                }).Load();
            }

            ValidateCounts(context, 152, 20, 20, 112);
        }

        private static void ValidateCounts(DbContext context, int total, int ones, int twos, int associations)
        {
            Assert.Equal(total, context.ChangeTracker.Entries().Count());
            Assert.Equal(ones, context.ChangeTracker.Entries<EntityOne>().Count());
            Assert.Equal(twos, context.ChangeTracker.Entries<EntityTwo>().Count());
            Assert.Equal(associations, context.ChangeTracker.Entries<JoinOneToTwo>().Count());

            var leftEntities = context.ChangeTracker.Entries<EntityOne>().Select(e => e.Entity).OrderBy(e => e.Id).ToList();
            var rightEntities = context.ChangeTracker.Entries<EntityTwo>().Select(e => e.Entity).OrderBy(e => e.Id).ToList();

            var joinCount = 0;
            foreach (var left in leftEntities)
            {
                foreach (var right in rightEntities)
                {
                    if (left.TwoSkip?.Contains(right) == true)
                    {
                        Assert.Contains(left, right.OneSkip);
                        joinCount++;
                    }

                    if (right.OneSkip?.Contains(left) == true)
                    {
                        Assert.Contains(right, left.TwoSkip);
                        joinCount++;
                    }
                }
            }

            var deleted = context.ChangeTracker.Entries<JoinOneToTwo>().Count(e => e.State == EntityState.Deleted);
            Assert.Equal(associations, (joinCount / 2) + deleted);
        }

        protected ManyToManyTrackingTestBase(TFixture fixture) => Fixture = fixture;

        protected TFixture Fixture { get; }

        protected virtual void ExecuteWithStrategyInTransaction(
            Action<DbContext> testOperation,
            Action<DbContext> nestedTestOperation1 = null,
            Action<DbContext> nestedTestOperation2 = null,
            Action<DbContext> nestedTestOperation3 = null)
            => TestHelpers.ExecuteWithStrategyInTransaction(
                CreateContext, UseTransaction,
                testOperation, nestedTestOperation1, nestedTestOperation2, nestedTestOperation3);

        protected virtual void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
        {
        }

        protected DbContext CreateContext() => Fixture.CreateContext();

        protected virtual bool SupportsDatabaseDefaults => true;

        public abstract class ManyToManyTrackingFixtureBase : ManyToManyQueryFixtureBase
        {
            protected override string StoreName { get; } = "ManyToManyTracking";
        }
    }
}
