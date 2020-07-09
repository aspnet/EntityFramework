﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class ManyToManyQuerySqliteTest : ManyToManyQueryRelationalTestBase<ManyToManyQuerySqliteFixture>
    {
        public ManyToManyQuerySqliteTest(ManyToManyQuerySqliteFixture fixture)
            : base(fixture)
        {
        }

        // Sqlite does not support Apply operations

        public override Task Skip_navigation_order_by_single_or_default(bool async)
            => Task.CompletedTask;

        public override Task Filtered_include_skip_navigation_order_by_skip_take_then_include_skip_navigation_where(bool async)
            => Task.CompletedTask;

        [ConditionalTheory(Skip = "Issue#21541")]
        public override Task Left_join_with_skip_navigation(bool async)
            => Task.CompletedTask;

        public override Task Include_skip_navigation_then_include_inverse_throws_in_no_tracking(bool async)
            => Task.CompletedTask;
    }
}
