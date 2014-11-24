// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Tests;
using Xunit;

namespace Microsoft.Data.Entity.InMemory.Tests
{
    public class InMemoryValueGeneratorTest
    {
        private static readonly Model _model = TestHelpers.BuildModelFor<AnEntity>();
        
        [Fact]
        public async Task Creates_values()
        {
            var property = _model.GetEntityType(typeof(AnEntity)).GetProperty("Id");

            var generator = new InMemoryValueGenerator();

            var generatedValue = await generator.NextAsync(property, new ContextService<DataStoreServices>(() => null));

            Assert.Equal(1, generatedValue.Value);
            Assert.False(generatedValue.IsTemporary);

            generatedValue = await generator.NextAsync(property, new ContextService<DataStoreServices>(() => null));

            Assert.Equal(2, generatedValue.Value);
            Assert.False(generatedValue.IsTemporary);

            generatedValue = await generator.NextAsync(property, new ContextService<DataStoreServices>(() => null));

            Assert.Equal(3, generatedValue.Value);
            Assert.False(generatedValue.IsTemporary);

            generatedValue = generator.Next(property, new ContextService<DataStoreServices>(() => null));

            Assert.Equal(4, generatedValue.Value);
            Assert.False(generatedValue.IsTemporary);

            generatedValue = generator.Next(property, new ContextService<DataStoreServices>(() => null));

            Assert.Equal(5, generatedValue.Value);
            Assert.False(generatedValue.IsTemporary);

            generatedValue = generator.Next(property, new ContextService<DataStoreServices>(() => null));

            Assert.Equal(6, generatedValue.Value);
            Assert.False(generatedValue.IsTemporary);

            generator = new InMemoryValueGenerator();

            generatedValue = await generator.NextAsync(property, new ContextService<DataStoreServices>(() => null));

            Assert.Equal(1, generatedValue.Value);
            Assert.False(generatedValue.IsTemporary);

            generatedValue = await generator.NextAsync(property, new ContextService<DataStoreServices>(() => null));

            Assert.Equal(2, generatedValue.Value);
            Assert.False(generatedValue.IsTemporary);
        }

        [Fact]
        public async Task Can_create_values_for_all_integer_types()
        {
            var entityType = _model.GetEntityType(typeof(AnEntity));

            var intProperty = entityType.GetProperty("Id");
            var longProperty = entityType.GetProperty("Long");
            var shortProperty = entityType.GetProperty("Short");
            var byteProperty = entityType.GetProperty("Byte");
            var uintProperty = entityType.GetProperty("UnsignedInt");
            var ulongProperty = entityType.GetProperty("UnsignedLong");
            var ushortProperty = entityType.GetProperty("UnsignedShort");
            var sbyteProperty = entityType.GetProperty("SignedByte");

            var generator = new InMemoryValueGenerator();

            var generatedValue = await generator.NextAsync(longProperty, new ContextService<DataStoreServices>(() => null));

            Assert.Equal(1L, generatedValue.Value);
            Assert.False(generatedValue.IsTemporary);

            generatedValue = await generator.NextAsync(intProperty, new ContextService<DataStoreServices>(() => null));

            Assert.Equal(2, generatedValue.Value);
            Assert.False(generatedValue.IsTemporary);

            generatedValue = await generator.NextAsync(shortProperty, new ContextService<DataStoreServices>(() => null));

            Assert.Equal((short)3, generatedValue.Value);
            Assert.False(generatedValue.IsTemporary);

            generatedValue = await generator.NextAsync(byteProperty, new ContextService<DataStoreServices>(() => null));

            Assert.Equal((byte)4, generatedValue.Value);
            Assert.False(generatedValue.IsTemporary);

            generatedValue = await generator.NextAsync(ulongProperty, new ContextService<DataStoreServices>(() => null));

            Assert.Equal((ulong)5, generatedValue.Value);
            Assert.False(generatedValue.IsTemporary);

            generatedValue = await generator.NextAsync(uintProperty, new ContextService<DataStoreServices>(() => null));

            Assert.Equal((uint)6, generatedValue.Value);
            Assert.False(generatedValue.IsTemporary);

            generatedValue = await generator.NextAsync(ushortProperty, new ContextService<DataStoreServices>(() => null));

            Assert.Equal((ushort)7, generatedValue.Value);
            Assert.False(generatedValue.IsTemporary);

            generatedValue = await generator.NextAsync(sbyteProperty, new ContextService<DataStoreServices>(() => null));

            Assert.Equal((sbyte)8, generatedValue.Value);
            Assert.False(generatedValue.IsTemporary);

            generatedValue = generator.Next(longProperty, new ContextService<DataStoreServices>(() => null));

            Assert.Equal(9L, generatedValue.Value);
            Assert.False(generatedValue.IsTemporary);

            generatedValue = generator.Next(intProperty, new ContextService<DataStoreServices>(() => null));

            Assert.Equal(10, generatedValue.Value);
            Assert.False(generatedValue.IsTemporary);

            generatedValue = generator.Next(shortProperty, new ContextService<DataStoreServices>(() => null));

            Assert.Equal((short)11, generatedValue.Value);
            Assert.False(generatedValue.IsTemporary);

            generatedValue = generator.Next(byteProperty, new ContextService<DataStoreServices>(() => null));

            Assert.Equal((byte)12, generatedValue.Value);
            Assert.False(generatedValue.IsTemporary);

            generatedValue = generator.Next(ulongProperty, new ContextService<DataStoreServices>(() => null));

            Assert.Equal((ulong)13, generatedValue.Value);
            Assert.False(generatedValue.IsTemporary);

            generatedValue = generator.Next(uintProperty, new ContextService<DataStoreServices>(() => null));

            Assert.Equal((uint)14, generatedValue.Value);
            Assert.False(generatedValue.IsTemporary);

            generatedValue = generator.Next(ushortProperty, new ContextService<DataStoreServices>(() => null));

            Assert.Equal((ushort)15, generatedValue.Value);
            Assert.False(generatedValue.IsTemporary);

            generatedValue = generator.Next(sbyteProperty, new ContextService<DataStoreServices>(() => null));

            Assert.Equal((sbyte)16, generatedValue.Value);
            Assert.False(generatedValue.IsTemporary);
        }

        [Fact]
        public void Throws_when_type_conversion_would_overflow()
        {
            var generator = new InMemoryValueGenerator();
            var property = CreateProperty(typeof(byte));

            for (var i = 1; i < 256; i++)
            {
                generator.Next(property, new ContextService<DataStoreServices>(() => null));
            }

            Assert.Throws<OverflowException>(() => generator.Next(property, new ContextService<DataStoreServices>(() => null)));
        }

        private static Property CreateProperty(Type propertyType)
        {
            var entityType = new Model().AddEntityType("MyType");
            return entityType.GetOrAddProperty("MyProperty", propertyType, shadowProperty: true);
        }

        private class AnEntity
        {
            public int Id { get; set; }
            public long Long { get; set; }
            public short Short { get; set; }
            public byte Byte { get; set; }
            public uint UnsignedInt { get; set; }
            public ulong UnsignedLong { get; set; }
            public ushort UnsignedShort { get; set; }
            public sbyte SignedByte { get; set; }
        }
    }
}
