// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Metadata;
using Xunit;

namespace Microsoft.Data.Entity.Tests.Metadata
{
    public class PropertyTest
    {
        [Fact]
        public void Can_create_property_from_property_info()
        {
            var property = new Property("Name", typeof(string), new EntityType(typeof(object)));

            Assert.Equal("Name", property.Name);
            Assert.Same(typeof(string), property.PropertyType);
        }

        [Fact]
        public void Default_nullability_of_property_is_based_on_nullability_of_CLR_type()
        {
            Assert.True(new Property("Name", typeof(string), new EntityType(typeof(object))).IsNullable);
            Assert.True(new Property("Name", typeof(int?), new EntityType(typeof(object))).IsNullable);
            Assert.False(new Property("Name", typeof(int), new EntityType(typeof(object))).IsNullable);
        }

        [Fact]
        public void Property_nullability_can_be_mutated()
        {
            Assert.False(new Property("Name", typeof(string), new EntityType(typeof(object))) { IsNullable = false }.IsNullable);
            Assert.True(new Property("Name", typeof(int), new EntityType(typeof(object))) { IsNullable = true }.IsNullable);
        }

        [Fact]
        public void UnderlyingType_returns_correct_underlying_type()
        {
            Assert.Equal(typeof(int), new Property("Name", typeof(int?), new EntityType(typeof(object))).UnderlyingType);
            Assert.Equal(typeof(int), new Property("Name", typeof(int), new EntityType(typeof(object))).UnderlyingType);
        }

        [Fact]
        public void HasClrProperty_is_set_appropriately()
        {
            Assert.False(new Property("Kake", typeof(int), new EntityType(typeof(object))).IsShadowProperty);
            Assert.False(new Property("Kake", typeof(int), new EntityType(typeof(object))).IsShadowProperty);
            Assert.True(new Property("Kake", typeof(int), new EntityType(typeof(object)), shadowProperty: true).IsShadowProperty);
        }

        [Fact]
        public void Property_is_not_concurrency_token_by_default()
        {
            Assert.False(new Property("Name", typeof(string), new EntityType(typeof(object))).IsConcurrencyToken);
        }

        [Fact]
        public void Can_mark_property_as_concurrency_token()
        {
            var property = new Property("Name", typeof(string), new EntityType(typeof(Entity)));
            Assert.False(property.IsConcurrencyToken);

            property.IsConcurrencyToken = true;
            Assert.True(property.IsConcurrencyToken);
        }

        [Fact]
        public void Property_is_read_write_by_default()
        {
            Assert.False(new Property("Name", typeof(string), new EntityType(typeof(object))).IsReadOnly);
        }

        [Fact]
        public void Property_can_be_marked_as_read_only()
        {
            var property = new Property("Name", typeof(string), new EntityType(typeof(object)));
            Assert.False(property.IsReadOnly);

            property.IsReadOnly = true;
            Assert.True(property.IsReadOnly);

            property.IsReadOnly = false;
            Assert.False(property.IsReadOnly);
        }

        [Fact]
        public void Can_get_and_set_property_index_for_normal_property()
        {
            var property = new Property("Kake", typeof(int), new EntityType(typeof(object)));

            Assert.Equal(0, property.Index);
            Assert.Equal(-1, property.ShadowIndex);

            property.Index = 1;

            Assert.Equal(1, property.Index);
            Assert.Equal(-1, property.ShadowIndex);

            Assert.Equal(
                "value",
                Assert.Throws<ArgumentOutOfRangeException>(() => property.Index = -1).ParamName);

            Assert.Equal(
                "value",
                Assert.Throws<ArgumentOutOfRangeException>(() => property.ShadowIndex = -1).ParamName);

            Assert.Equal(
                "value",
                Assert.Throws<ArgumentOutOfRangeException>(() => property.ShadowIndex = 1).ParamName);
        }

        [Fact]
        public void Can_get_and_set_property_and_shadow_index_for_shadow_property()
        {
            var property = new Property("Kake", typeof(int), new EntityType(typeof(object)), shadowProperty: true);

            Assert.Equal(0, property.Index);
            Assert.Equal(0, property.ShadowIndex);

            property.Index = 1;
            property.ShadowIndex = 2;

            Assert.Equal(1, property.Index);
            Assert.Equal(2, property.ShadowIndex);

            Assert.Equal(
                "value",
                Assert.Throws<ArgumentOutOfRangeException>(() => property.Index = -1).ParamName);

            Assert.Equal(
                "value",
                Assert.Throws<ArgumentOutOfRangeException>(() => property.ShadowIndex = -1).ParamName);
        }

        private class Entity
        {
            public string Name { get; set; }
        }
    }
}
