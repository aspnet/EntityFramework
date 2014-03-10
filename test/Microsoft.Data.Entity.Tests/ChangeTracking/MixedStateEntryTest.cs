// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Metadata;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Tests.ChangeTracking
{
    public class MixedStateEntryTest : StateEntryTest
    {
        [Fact]
        public void Constructors_check_arguments()
        {
            var entityTypeMock = CreateEntityTypeMock();
            var stateManager = CreateManagerMock(entityTypeMock).Object;

            Assert.Equal(
                "stateManager",
                // ReSharper disable once AssignNullToNotNullAttribute
                Assert.Throws<ArgumentNullException>(() => new MixedStateEntry(null, entityTypeMock.Object, new Random())).ParamName);
            Assert.Equal(
                "entityType",
                // ReSharper disable once AssignNullToNotNullAttribute
                Assert.Throws<ArgumentNullException>(() => new MixedStateEntry(stateManager, null, new Random())).ParamName);
            Assert.Equal(
                "entity",
                // ReSharper disable once AssignNullToNotNullAttribute
                Assert.Throws<ArgumentNullException>(() => new MixedStateEntry(stateManager, entityTypeMock.Object, null)).ParamName);
        }

        [Fact]
        public void Can_get_entity()
        {
            var entityTypeMock = CreateEntityTypeMock();
            var entity = new Random();
            var entry = new MixedStateEntry(CreateManagerMock(entityTypeMock).Object, entityTypeMock.Object, entity);

            Assert.Same(entity, entry.Entity);
        }

        [Fact]
        public void Can_set_and_get_property_value_from_shadow_state()
        {
            var propertyMock = new Mock<IProperty>();
            var entityTypeMock = CreateEntityTypeMock(propertyMock);
            var entry = new MixedStateEntry(CreateManagerMock(entityTypeMock).Object, entityTypeMock.Object, new Random());

            Assert.Equal(null, entry.GetPropertyValue(propertyMock.Object));

            entry.SetPropertyValue(propertyMock.Object, "Magic Tree House");

            Assert.Equal("Magic Tree House", entry.GetPropertyValue(propertyMock.Object));
        }

        [Fact]
        public void Can_set_and_get_property_value_from_CLR_object()
        {
            var propertyMock = new Mock<IProperty>();
            var entityTypeMock = CreateEntityTypeMock(new Mock<IProperty>(), propertyMock);
            var entity = new Random();
            var entry = new MixedStateEntry(CreateManagerMock(entityTypeMock).Object, entityTypeMock.Object, entity);

            Assert.Equal(null, entry.GetPropertyValue(propertyMock.Object));

            propertyMock.Verify(m => m.GetValue(entity));

            entry.SetPropertyValue(propertyMock.Object, "Magic Tree House");

            propertyMock.Verify(m => m.SetValue(entity, "Magic Tree House"));
        }

        [Fact]
        public void Can_get_value_buffer_from_mix_of_shadow_and_CLR_properties()
        {
            var propertyMock1 = new Mock<IProperty>();
            var propertyMock2 = new Mock<IProperty>();
            var entityTypeMock = CreateEntityTypeMock(propertyMock1, propertyMock2);
            var entry = new MixedStateEntry(CreateManagerMock(entityTypeMock).Object, entityTypeMock.Object, new Random());

            entry.SetPropertyValue(propertyMock1.Object, "Magic");
            propertyMock2.Setup(m => m.GetValue(It.IsAny<object>())).Returns("Tree House");

            Assert.Equal(new object[] { "Magic", "Tree House" }, entry.GetValueBuffer());
        }

        protected override Mock<IEntityType> CreateEntityTypeMock(Mock<IProperty> key = null, Mock<IProperty> nonKey = null)
        {
            key = key ?? new Mock<IProperty>();
            key.Setup(m => m.Index).Returns(0);
            key.Setup(m => m.ShadowIndex).Returns(0);
            key.Setup(m => m.HasClrProperty).Returns(false);
            var keys = new[] { key.Object };
            nonKey = nonKey ?? new Mock<IProperty>();
            nonKey.Setup(m => m.Index).Returns(1);
            nonKey.Setup(m => m.ShadowIndex).Returns(-1);
            nonKey.Setup(m => m.HasClrProperty).Returns(true);

            var entityTypeMock = new Mock<IEntityType>();
            entityTypeMock.Setup(m => m.Key).Returns(keys);
            entityTypeMock.Setup(m => m.Properties).Returns(keys.Concat(new[] { nonKey.Object }).ToArray());
            entityTypeMock.Setup(m => m.ShadowPropertyCount).Returns(2);

            return entityTypeMock;
        }
    }
}
