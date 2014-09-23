﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#if NET45

using Microsoft.Data.Entity.FunctionalTests;
using Xunit;

namespace Microsoft.Data.Entity.InMemory.FunctionalTests
{
    public class DeadlockTest : DeadlockTestBase<InMemoryTestStore>, IClassFixture<NorthwindQueryFixture>
    {
        private readonly NorthwindQueryFixture _fixture;

        public DeadlockTest(NorthwindQueryFixture fixture)
        {
            _fixture = fixture;
        }

        protected override InMemoryTestStore CreateTestDatabase()
        {
            return new InMemoryTestStore();
        }

        protected override DbContext CreateContext(InMemoryTestStore testDatabase)
        {
            return _fixture.CreateContext(persist: false);
        }

        protected override DbContext CreateContext()
        {
            return _fixture.CreateContext();
        }
    }
}

#endif