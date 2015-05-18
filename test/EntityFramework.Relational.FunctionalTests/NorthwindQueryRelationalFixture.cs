// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.FunctionalTests;
using Microsoft.Data.Entity.FunctionalTests.TestModels.Northwind;

namespace Microsoft.Data.Entity.Relational.FunctionalTests
{
    public abstract class NorthwindQueryRelationalFixture : NorthwindQueryFixtureBase
    {
        public override void OnModelCreating(ModelBuilder model)
        {
            base.OnModelCreating(model);

            model.Entity<Customer>().Table("Customers");
            model.Entity<Employee>().Table("Employees");
            model.Entity<Product>().Table("Products");
            model.Entity<Product>().Ignore(p => p.SupplierID);
            model.Entity<Order>().Table("Orders");
            model.Entity<OrderDetail>().Table("Order Details");
        }
    }
}
