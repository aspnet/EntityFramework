﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.FunctionalTests;
using Xunit;

namespace Microsoft.Data.Entity.Redis.FunctionalTests
{
    public class AsNoTrackingTest : AsNoTrackingTestBase, IClassFixture<NorthwindQueryFixture>
    {
        public override void Entity_not_added_to_state_manager()
        {
            base.Entity_not_added_to_state_manager();
        }

        private readonly NorthwindQueryFixture _fixture;

        public AsNoTrackingTest(NorthwindQueryFixture fixture)
        {
            _fixture = fixture;
        }

        protected override DbContext CreateContext()
        {
            return _fixture.CreateContext();
        }
    }
}
