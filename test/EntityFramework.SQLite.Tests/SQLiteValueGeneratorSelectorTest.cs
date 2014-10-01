﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Identity;
using Microsoft.Data.Entity.Metadata;
using Xunit;

namespace Microsoft.Data.Entity.SQLite.Tests
{
    public class SQLiteValueGeneratorSelectorTest
    {
        [Fact]
        public void Select_returns_tempFactory_when_single_long_key()
        {
            var entityType = new EntityType("Entity");
            var property = entityType.GetOrAddProperty("Id", typeof(long), shadowProperty: true);
            property.ValueGeneration = ValueGeneration.OnAdd;
            entityType.GetOrSetPrimaryKey(property);

            var result = CreateSelector().Select(property);

            Assert.IsType<SimpleValueGeneratorFactory<TemporaryValueGenerator>>(result);
        }

        [Fact]
        public void Select_returns_tempFactory_when_single_integer_column_key()
        {
            var entityType = new EntityType("Entity");
            var property = entityType.GetOrAddProperty("Id", typeof(int), shadowProperty: true);
            property.ValueGeneration = ValueGeneration.OnAdd;
            property[MetadataExtensions.Annotations.StorageTypeName] = "INTEGER";
            entityType.GetOrSetPrimaryKey(property);

            var result = CreateSelector().Select(property);

            Assert.IsType<SimpleValueGeneratorFactory<TemporaryValueGenerator>>(result);
        }

        [Fact]
        public void Select_returns_null_when_ValueGeneration_is_not_set()
        {
            var entityType = new EntityType("Entity");
            var property = entityType.GetOrAddProperty("Id", typeof(long), shadowProperty: true);
            entityType.GetOrSetPrimaryKey(property);

            var result = CreateSelector().Select(property);

            Assert.Null(result);
        }

        [Fact]
        public void Select_throws_when_composite_key()
        {
            var selector = CreateSelector();
            var entityType = new EntityType("Entity");
            var property = entityType.GetOrAddProperty("Id1", typeof(long), shadowProperty: true);
            property.ValueGeneration = ValueGeneration.OnAdd;
            entityType.GetOrSetPrimaryKey(new[] { property, entityType.GetOrAddProperty("Id2", typeof(long), shadowProperty: true) });

            Assert.Throws<NotSupportedException>(() => selector.Select(property));
        }

        [Fact]
        public void Select_throws_when_non_integer_column_key()
        {
            var selector = CreateSelector();
            var entityType = new EntityType("Entity");
            var property = entityType.GetOrAddProperty("Id", typeof(int), shadowProperty: true);
            property.ValueGeneration = ValueGeneration.OnAdd;
            entityType.GetOrSetPrimaryKey(property);

            Assert.Throws<NotSupportedException>(() => selector.Select(property));
        }

        private SQLiteValueGeneratorSelector CreateSelector()
        {
            return new SQLiteValueGeneratorSelector(
                new SimpleValueGeneratorFactory<GuidValueGenerator>(),
                new SimpleValueGeneratorFactory<TemporaryValueGenerator>());
        }
    }
}
