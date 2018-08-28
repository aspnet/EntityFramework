﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestModels.SpatialModel;

namespace Microsoft.EntityFrameworkCore
{
    public abstract class SpatialFixtureBase : SharedStoreFixtureBase<SpatialContext>
    {
        protected override string StoreName
            => "SpatialTest";

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            modelBuilder.Entity<PointEntity>().Property(e => e.Id).ValueGeneratedNever();
            modelBuilder.Entity<LineStringEntity>().Property(e => e.Id).ValueGeneratedNever();
            modelBuilder.Entity<PolygonEntity>().Property(e => e.Id).ValueGeneratedNever();
            modelBuilder.Entity<MultiLineStringEntity>().Property(e => e.Id).ValueGeneratedNever();
        }

        protected override void Seed(SpatialContext context)
            => SpatialContext.Seed(context);
    }
}
