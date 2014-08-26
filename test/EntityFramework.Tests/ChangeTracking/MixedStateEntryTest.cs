// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Metadata;
using Xunit;

namespace Microsoft.Data.Entity.ChangeTracking
{
    public class MixedStateEntryTest : StateEntryTest
    {
        [Fact]
        public void Can_get_entity()
        {
            var model = BuildModel();
            var configuration = TestHelpers.CreateContextConfiguration(model);

            var entity = new SomeEntity();
            var entry = CreateStateEntry(configuration, model.GetEntityType(typeof(SomeEntity).FullName), entity);

            Assert.Same(entity, entry.Entity);
        }

        [Fact]
        public void Can_set_and_get_property_value_from_CLR_object()
        {
            var model = BuildModel();
            var entityType = model.GetEntityType(typeof(SomeEntity).FullName);
            var keyProperty = entityType.GetProperty("Id");
            var nonKeyProperty = entityType.GetProperty("Name");
            var configuration = TestHelpers.CreateContextConfiguration(model);

            var entity = new SomeEntity { Id = 77, Name = "Magic Tree House" };
            var entry = CreateStateEntry(configuration, entityType, entity);

            Assert.Null(entry[keyProperty]); // In shadow
            Assert.Equal("Magic Tree House", entry[nonKeyProperty]);

            entry[keyProperty] = 78;
            entry[nonKeyProperty] = "Normal Tree House";

            Assert.Equal(77, entity.Id); // In shadow
            Assert.Equal("Normal Tree House", entity.Name);
        }

        [Fact]
        public void Asking_for_entity_instance_causes_it_to_be_materialized()
        {
            var model = BuildModel();
            var entityType = model.GetEntityType(typeof(SomeEntity).FullName);
            var configuration = TestHelpers.CreateContextConfiguration(model);

            var entry = CreateStateEntry(configuration, entityType, new ObjectArrayValueReader(new object[] { 1, "Kool" }));

            var entity = (SomeEntity)entry.Entity;

            Assert.Equal("Kool", entity.Name);
        }

        [Fact]
        public void All_original_values_can_be_accessed_for_entity_that_does_no_notifiction()
        {
            var model = BuildModel();
            var entityType = model.GetEntityType(typeof(SomeEntity).FullName);

            AllOriginalValuesTest(model, entityType);
        }

        [Fact]
        public void All_original_values_can_be_accessed_for_entity_that_does_changed_only_notifictions_if_eager_values_on()
        {
            var model = BuildModel();
            var entityType = model.GetEntityType(typeof(ChangedOnlyEntity).FullName);
            entityType.UseLazyOriginalValues = false;

            AllOriginalValuesTest(model, entityType);
        }

        [Fact]
        public void Setting_CLR_property_with_snapshot_change_tracking_requires_DetectChanges()
        {
            SetPropertyClrTest<SomeEntity>(needsDetectChanges: true);
        }

        [Fact]
        public void Setting_CLR_property_with_changed_only_notifications_does_not_require_DetectChanges()
        {
            SetPropertyClrTest<ChangedOnlyEntity>(needsDetectChanges: false);
        }

        [Fact]
        public void Setting_CLR_property_with_full_notifications_does_not_require_DetectChanges()
        {
            SetPropertyClrTest<FullNotificationEntity>(needsDetectChanges: false);
        }

        [Fact]
        public void Original_values_are_not_tracked_unless_needed_by_default_for_properties_of_full_notifications_entity()
        {
            var model = BuildModel();
            var entityType = model.GetEntityType(typeof(FullNotificationEntity).FullName);
            var idProperty = entityType.GetProperty("Id");
            var configuration = TestHelpers.CreateContextConfiguration(model);

            var entry = CreateStateEntry(configuration, entityType, new ObjectArrayValueReader(new object[] { 1, "Kool" }));

            Assert.Equal(
                Strings.FormatOriginalValueNotTracked("Id", typeof(FullNotificationEntity).FullName),
                Assert.Throws<InvalidOperationException>(() => entry.OriginalValues[idProperty] = 1).Message);

            Assert.Equal(
                Strings.FormatOriginalValueNotTracked("Id", typeof(FullNotificationEntity).FullName),
                Assert.Throws<InvalidOperationException>(() => entry.OriginalValues[idProperty]).Message);
        }

        protected override Model BuildModel()
        {
            var model = new Model();

            var entityType1 = new EntityType(typeof(SomeEntity));
            model.AddEntityType(entityType1);
            var key1 = entityType1.AddProperty("Id", typeof(int), shadowProperty: true, concurrencyToken: false);
            key1.ValueGenerationOnSave = ValueGenerationOnSave.WhenInserting;
            key1.ValueGenerationOnAdd = ValueGenerationOnAdd.Client;
            entityType1.SetKey(key1);
            entityType1.AddProperty("Name", typeof(string));

            var entityType2 = new EntityType(typeof(SomeDependentEntity));
            model.AddEntityType(entityType2);
            var key2a = entityType2.AddProperty("Id1", typeof(int));
            var key2b = entityType2.AddProperty("Id2", typeof(string));
            entityType2.SetKey(key2a, key2b);
            var fk = entityType2.AddProperty("SomeEntityId", typeof(int), shadowProperty: true, concurrencyToken: false);
            entityType2.AddForeignKey(entityType1.GetKey(), new[] { fk });
            var justAProperty = entityType2.AddProperty("JustAProperty", typeof(int));
            justAProperty.ValueGenerationOnSave = ValueGenerationOnSave.WhenInserting;
            justAProperty.ValueGenerationOnAdd = ValueGenerationOnAdd.Client;

            var entityType3 = new EntityType(typeof(FullNotificationEntity));
            model.AddEntityType(entityType3);
            entityType3.SetKey(entityType3.AddProperty("Id", typeof(int), shadowProperty: true, concurrencyToken: false));
            entityType3.AddProperty("Name", typeof(string), shadowProperty: false, concurrencyToken: true);

            var entityType4 = new EntityType(typeof(ChangedOnlyEntity));
            model.AddEntityType(entityType4);
            entityType4.SetKey(entityType4.AddProperty("Id", typeof(int), shadowProperty: true, concurrencyToken: false));
            entityType4.AddProperty("Name", typeof(string), shadowProperty: false, concurrencyToken: true);

            var entityType5 = new EntityType(typeof(SomeMoreDependentEntity));
            model.AddEntityType(entityType5);
            var key5 = entityType5.AddProperty("Id", typeof(int));
            entityType5.SetKey(key5);
            var fk5a = entityType5.AddProperty("Fk1", typeof(int));
            var fk5b = entityType5.AddProperty("Fk2", typeof(string));
            entityType5.AddForeignKey(entityType2.GetKey(), new[] { fk5a, fk5b });

            return model;
        }
    }
}
