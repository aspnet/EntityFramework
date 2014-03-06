﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Reflection;
using Microsoft.Data.Entity.Metadata;
using Xunit;

namespace Microsoft.Data.Entity.Tests.Metadata
{
    public class PropertyTest
    {
        #region Fixture

        public class Customer
        {
            public static PropertyInfo IdProperty = typeof(Customer).GetProperty("Id");
            public static PropertyInfo NameProperty = typeof(Customer).GetProperty("Name");
            public static PropertyInfo AgeProperty = typeof(Customer).GetProperty("Age");
            public static PropertyInfo HashProperty = typeof(Customer).GetProperty("Hash");

            public int Id { get; set; }
            public string Name { get; set; }
            public byte? Age { get; set; }
            public Guid Hash { get; set; }
        }

        #endregion

        [Fact]
        public void Members_check_arguments()
        {
            Assert.Equal(
                "propertyInfo",
                // ReSharper disable once AssignNullToNotNullAttribute
                Assert.Throws<ArgumentNullException>(() => new Property(null)).ParamName);

            var property = new Property(Customer.NameProperty);

            Assert.Equal(
                Strings.FormatArgumentIsEmpty("value"),
                Assert.Throws<ArgumentException>(() => property.StorageName = "").Message);

            Assert.Equal(
                "instance",
                // ReSharper disable once AssignNullToNotNullAttribute
                Assert.Throws<ArgumentNullException>(() => property.GetValue(null)).ParamName);

            Assert.Equal(
                "instance",
                // ReSharper disable once AssignNullToNotNullAttribute
                Assert.Throws<ArgumentNullException>(() => property.SetValue(null, "Kake")).ParamName);
        }

        [Fact]
        public void Storage_name_defaults_to_name()
        {
            var property = new Property(Customer.NameProperty);

            Assert.Equal("Name", property.StorageName);
        }

        [Fact]
        public void Storage_name_can_be_different_from_name()
        {
            var property = new Property(Customer.NameProperty) { StorageName = "CustomerName" };

            Assert.Equal("CustomerName", property.StorageName);
        }

        [Fact]
        public void Can_create_property_from_property_info()
        {
            var property = new Property(Customer.NameProperty);

            Assert.Equal("Name", property.Name);
            Assert.Same(typeof(string), property.PropertyType);
            Assert.True(property.IsNullable);
        }

        [Fact]
        public void Can_get_and_set_property_value()
        {
            var entityType = new EntityType(typeof(Customer));
            var property = new Property(Customer.NameProperty);
            entityType.AddProperty(property);
            var entity = new Customer();

            Assert.Null(property.GetValue(entity));
            property.SetValue(entity, "There is no kake");
            Assert.Equal("There is no kake", property.GetValue(entity));
        }
    }
}
