﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Northwind;
using Xunit;

namespace Microsoft.Data.Entity.FunctionalTests
{
    public abstract class AsNoTrackingTestBase
    {
        [Fact]
        public virtual void Entity_not_added_to_state_manager()
        {
            using (var context = CreateContext())
            {
                var customers = context.Set<Customer>().AsNoTracking().ToList();

                Assert.Equal(91, customers.Count);
                Assert.Equal(0, context.ChangeTracker.Entries().Count());
            }
        }

        [Fact]
        public virtual void Applied_to_body_clause()
        {
            using (var context = CreateContext())
            {
                var customers
                    = (from c in context.Set<Customer>()
                        join o in context.Set<Order>().AsNoTracking()
                            on c.CustomerID equals o.CustomerID
                        where c.CustomerID == "ALFKI"
                        select o)
                        .ToList();

                Assert.Equal(6, customers.Count);
                Assert.Equal(0, context.ChangeTracker.Entries().Count());
            }
        }

        [Fact]
        public virtual void Applied_to_body_clause_with_projection()
        {
            using (var context = CreateContext())
            {
                var customers
                    = (from c in context.Set<Customer>()
                        join o in context.Set<Order>().AsNoTracking()
                            on c.CustomerID equals o.CustomerID
                        where c.CustomerID == "ALFKI"
                        select new { c.CustomerID, c, ocid = o.CustomerID, o })
                        .ToList();

                Assert.Equal(6, customers.Count);
                Assert.Equal(1, context.ChangeTracker.Entries().Count());
            }
        }

        [Fact]
        public virtual void Applied_to_projection()
        {
            using (var context = CreateContext())
            {
                var customers
                    = (from c in context.Set<Customer>()
                       join o in context.Set<Order>().AsNoTracking()
                           on c.CustomerID equals o.CustomerID
                       where c.CustomerID == "ALFKI"
                       select new { c, o })
                        .AsNoTracking()
                        .ToList();

                Assert.Equal(6, customers.Count);
                Assert.Equal(0, context.ChangeTracker.Entries().Count());
            }
        }

        protected abstract DbContext CreateContext();
    }
}
