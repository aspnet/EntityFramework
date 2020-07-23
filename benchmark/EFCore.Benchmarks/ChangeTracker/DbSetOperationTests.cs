// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using Microsoft.EntityFrameworkCore.Benchmarks.Models.Orders;

#pragma warning disable CA1034 // Nested types should not be visible

namespace Microsoft.EntityFrameworkCore.Benchmarks.ChangeTracker
{
#pragma warning disable CA1052 // Static holder types should be Static or NotInheritable
    public class DbSetOperationTests
#pragma warning restore CA1052 // Static holder types should be Static or NotInheritable
    {
        public abstract class DbSetOperationBase
        {
            public const int OperationsPerInvoke = 20000;

            private OrdersFixtureBase _fixture;

            protected List<Customer> _customersWithoutPk;
            protected List<Customer> _customersWithPk;
            protected OrdersContextBase _context;

            public abstract OrdersFixtureBase CreateFixture();

            [Params(true, false)]
            public virtual bool AutoDetectChanges { get; set; }

            [GlobalSetup]
            public virtual void CreateCustomers()
            {
                _fixture = CreateFixture();
                _fixture.Initialize(0, 0, 0, 0);
                _customersWithoutPk = _fixture.CreateCustomers(OperationsPerInvoke, setPrimaryKeys: false);
                _customersWithPk = _fixture.CreateCustomers(OperationsPerInvoke, setPrimaryKeys: true);
            }

            public virtual void InitializeContext()
            {
                _context = _fixture.CreateContext();
                _context.ChangeTracker.AutoDetectChangesEnabled = AutoDetectChanges;
            }

            [IterationCleanup]
            public virtual void CleanupContext()
            {
                _context.Dispose();
            }
        }

        public abstract class AddDataVariationsBase : DbSetOperationBase
        {
            [IterationSetup]
            public override void InitializeContext()
            {
                base.InitializeContext();
            }

            [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
            public virtual void Add()
            {
                foreach (var customer in _customersWithoutPk)
                {
                    _context.Customers.Add(customer);
                }
            }

            [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
            public virtual void AddRange()
            {
                _context.Customers.AddRange(_customersWithoutPk);
            }

            [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
            public virtual void Attach()
            {
                foreach (var customer in _customersWithPk)
                {
                    _context.Customers.Attach(customer);
                }
            }

            [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
            public virtual void AttachRange()
            {
                _context.Customers.AttachRange(_customersWithPk);
            }
        }

        public abstract class ExistingDataVariationsBase : DbSetOperationBase
        {
            [IterationSetup]
            public override void InitializeContext()
            {
                base.InitializeContext();
                _context.Customers.AttachRange(_customersWithPk);
            }

            [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
            public virtual void Remove()
            {
                foreach (var customer in _customersWithPk)
                {
                    _context.Customers.Remove(customer);
                }
            }

            [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
            public virtual void RemoveRange()
            {
                _context.Customers.RemoveRange(_customersWithPk);
            }

            [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
            public virtual void Update()
            {
                foreach (var customer in _customersWithPk)
                {
                    _context.Customers.Update(customer);
                }
            }

            [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
            public virtual void UpdateRange()
            {
                _context.Customers.UpdateRange(_customersWithPk);
            }
        }
    }
}
