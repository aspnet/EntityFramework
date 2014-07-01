// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Metadata;
using Microsoft.WindowsAzure.Storage.Table;

namespace Microsoft.Data.Entity.AzureTableStorage.Tests.Helpers
{
    public class TestStateEntry : StateEntry
    {
        private object _entity;
        private IEntityType _entityType;
        private Dictionary<string,object> _propertyBag = new Dictionary<string, object>();

        public override object Entity
        {
            get { return _entity; }
        }

        public override IEntityType EntityType
        {
            get { return _entityType; }
        }

        public override EntityState EntityState { get; set; }

        protected override object ReadPropertyValue(IPropertyBase property)
        {
            return _propertyBag[property.ColumnName()];
        }

        protected override void WritePropertyValue(IPropertyBase property, object value)
        {
            _propertyBag[property.ColumnName()] = value;
        }

        public override object this[IPropertyBase property]
        {
            get { return ReadPropertyValue(property); }
            set { WritePropertyValue(property,value); }
        }

        public static TestStateEntry Mock()
        {
            var entry = new TestStateEntry
                {
                    _entity = new TableEntity { ETag = "*"}
                }
                .WithType(typeof(TableEntity));

            return entry;
        }

        public TestStateEntry WithState(EntityState state)
        {
            EntityState = state;
            return this;
        }

        public TestStateEntry WithType(string name)
        {
            var e = new EntityType(name);
            e.AddProperty("PartitionKey", typeof(string), true, false);
            e.AddProperty("RowKey", typeof(string), true, false);
            e.AddProperty("ETag", typeof(string), true, false);
            e.AddProperty("Timestamp", typeof(DateTime), true, false);
            _entityType = e;
            return this;
        }

        public TestStateEntry WithType(Type type)
        {
            var e = new EntityType(type);
            e.AddProperty("PartitionKey", typeof(string));
            e.AddProperty("RowKey", typeof(string));
            e.AddProperty("ETag", typeof(string));
            e.AddProperty("Timestamp", typeof(DateTimeOffset));
            _entityType = e;
            return this;
        }

        public TestStateEntry WithEntityType(IEntityType entityType)
        {
            _entityType = entityType;
            return this;
        }

        public TestStateEntry WithProperty(string property, string value)
        {
            _propertyBag[property] = value;
            return this;
        }

        public TestStateEntry WithEntity(object entity)
        {
            _entity = entity;
            return this;
        }
    }
}
