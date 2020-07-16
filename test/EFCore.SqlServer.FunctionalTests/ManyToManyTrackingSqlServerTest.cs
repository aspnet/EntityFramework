﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestModels.ManyToManyModel;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore
{
    public class ManyToManyTrackingSqlServerTest
        : ManyToManyTrackingTestBase<ManyToManyTrackingSqlServerTest.ManyToManyTrackingSqlServerFixture>
    {
        public ManyToManyTrackingSqlServerTest(ManyToManyTrackingSqlServerFixture fixture)
            : base(fixture)
        {
        }

        protected override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
            => facade.UseTransaction(transaction.GetDbTransaction());

        public class ManyToManyTrackingSqlServerFixture : ManyToManyTrackingFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory => SqlServerTestStoreFactory.Instance;

            protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
            {
                base.OnModelCreating(modelBuilder, context);

                modelBuilder
                    .Entity<JoinOneSelfPayload>()
                    .Property(e => e.Payload)
                    .HasDefaultValueSql("GETUTCDATE()");

                modelBuilder
                    .Entity<JoinOneToThreePayloadFullShared>()
                    .Property(e => e.Payload)
                    .HasDefaultValue("Generated");

                modelBuilder
                    .Entity<JoinOneToThreePayloadFull>()
                    .Property(e => e.Payload)
                    .HasDefaultValue("Generated");
            }
        }
    }
}
