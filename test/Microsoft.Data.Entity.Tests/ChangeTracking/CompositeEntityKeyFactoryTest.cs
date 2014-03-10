// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Metadata;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Tests.ChangeTracking
{
    public class CompositeEntityKeyFactoryTest
    {
        [Fact]
        public void Creates_a_new_key_for_key_values_in_the_given_entry()
        {
            var keyPart1 = new Mock<IProperty>().Object;
            var keyPart2 = new Mock<IProperty>().Object;
            var keyPart3 = new Mock<IProperty>().Object;

            var typeMock = new Mock<IEntityType>();
            typeMock.Setup(m => m.Key).Returns(new[] { keyPart1, keyPart2, keyPart3 });

            var random = new Random();
            var entryMock = new Mock<StateEntry>();
            entryMock.Setup(m => m.GetPropertyValue(keyPart1)).Returns(7);
            entryMock.Setup(m => m.GetPropertyValue(keyPart2)).Returns("Ate");
            entryMock.Setup(m => m.GetPropertyValue(keyPart3)).Returns(random);
            entryMock.Setup(m => m.EntityType).Returns(typeMock.Object);

            var key = (CompositeEntityKey)new CompositeEntityKeyFactory().Create(entryMock.Object);

            Assert.Equal(new Object[] { 7, "Ate", random }, key.Value);
        }
    }
}
