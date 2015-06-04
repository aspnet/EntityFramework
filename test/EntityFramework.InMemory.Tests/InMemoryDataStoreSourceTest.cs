// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Xunit;

namespace Microsoft.Data.Entity.InMemory.Tests
{
    public class InMemoryDataStoreSourceTest
    {
        [Fact]
        public void Returns_appropriate_name()
        {
            Assert.Equal("In-Memory Data Store", new InMemoryDataStoreSource().Name);
        }

        [Fact]
        public void Is_configured_when_configuration_contains_associated_extension()
        {
            var optionsBuilder = new EntityOptionsBuilder();
            optionsBuilder.UseInMemoryStore();

            Assert.True(new InMemoryDataStoreSource().IsConfigured(optionsBuilder.Options));
        }

        [Fact]
        public void Can_be_auto_configured()
        {
            var optionsBuilder = new EntityOptionsBuilder();

            var dataStoreSource = new InMemoryDataStoreSource();
            dataStoreSource.AutoConfigure(optionsBuilder);

            Assert.True(dataStoreSource.IsConfigured(optionsBuilder.Options));
        }

        [Fact]
        public void Is_not_configured_when_configuration_does_not_contain_associated_extension()
        {
            var optionsBuilder = new EntityOptionsBuilder();

            Assert.False(new InMemoryDataStoreSource().IsConfigured(optionsBuilder.Options));
        }
    }
}
