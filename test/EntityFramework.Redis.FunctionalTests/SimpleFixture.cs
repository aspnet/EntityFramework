﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Redis.Extensions;

namespace Microsoft.Data.Entity.Redis
{
    public class SimpleFixture : IDisposable
    {
        private DbContext _context;
        public DbContext GetOrCreateContext()
        {
            if (_context == null)
            {
                var options = new DbContextOptions()
                    .UseModel(CreateModel())
                    .UseRedis("127.0.0.1");

                _context = new DbContext(options);
            }

            return _context;
        }

        public IModel CreateModel()
        {
            var model = new Model();
            var builder = new ModelBuilder(model);
            builder.Entity<Customer>()
                .Key(cust => cust.CustomerID)
                .Properties(pb =>
                {
                    pb.Property(cust => cust.CustomerID);
                    pb.Property(cust => cust.Name);
                });

            return model;
        }

        void IDisposable.Dispose()
        {
            _context.Database.EnsureDeleted();
        }
    }

    public class Customer
    {
        public int CustomerID { get; set; }
        public string Name { get; set; }
    }
}
