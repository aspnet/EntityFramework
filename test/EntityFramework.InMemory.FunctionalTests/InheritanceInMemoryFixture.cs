// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.FunctionalTests;
using Microsoft.Data.Entity.FunctionalTests.TestModels.Inheritance;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.Data.Entity.InMemory.FunctionalTests
{
    public class InheritanceInMemoryFixture : InheritanceFixtureBase
    {
        private readonly EntityOptionsBuilder _optionsBuilder = new EntityOptionsBuilder();
        private readonly IServiceProvider _serviceProvider;

        public InheritanceInMemoryFixture()
        {
            _serviceProvider
                = new ServiceCollection()
                    .AddEntityFramework()
                    .AddInMemoryStore()
                    .ServiceCollection()
                    .AddSingleton(TestInMemoryModelSource.GetFactory(OnModelCreating))
                    .BuildServiceProvider();

            _optionsBuilder.UseInMemoryStore();

            using (var context = CreateContext())
            {
                SeedData(context);
            }
        }

        public override AnimalContext CreateContext()
        {
            return new AnimalContext(_serviceProvider, _optionsBuilder.Options);
        }
    }
}
