﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.AspNet.DependencyInjection;
using Microsoft.Data.Entity.Identity;
using Xunit;

namespace Microsoft.Data.InMemory
{
    public class InMemoryServicesTest
    {
        [Fact]
        public void CanGetDefaultServices()
        {
            var services = InMemoryServices.GetDefaultServices().ToList();

            Assert.True(services.Any(sd => sd.ServiceType == typeof(IIdentityGenerator<long>)));
        }

        [Fact]
        public void ServicesWireUpCorrectly()
        {
            var serviceProvider = new ServiceProvider().Add(InMemoryServices.GetDefaultServices());

            Assert.NotNull(serviceProvider.GetService<IIdentityGenerator<long>>());
        }
    }
}
