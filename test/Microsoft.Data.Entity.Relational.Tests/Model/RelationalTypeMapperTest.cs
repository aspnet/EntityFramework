// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data;
using Microsoft.Data.Entity.Relational.Model;
using Xunit;

namespace Microsoft.Data.Entity.Relational.Tests.Model
{
    public class RelationalTypeMapperTest
    {
        [Fact]
        public void Does_simple_ANSI_mappings_to_DDL_types()
        {
            Assert.Equal("integer", GetTypeMapping(typeof(int)).StoreTypeName);
            Assert.Equal("timestamp", GetTypeMapping(typeof(DateTime)).StoreTypeName);
            Assert.Equal("boolean", GetTypeMapping(typeof(bool)).StoreTypeName);
            Assert.Equal("double precision", GetTypeMapping(typeof(double)).StoreTypeName);
            Assert.Equal("smallint", GetTypeMapping(typeof(short)).StoreTypeName);
            Assert.Equal("bigint", GetTypeMapping(typeof(long)).StoreTypeName);
            Assert.Equal("real", GetTypeMapping(typeof(float)).StoreTypeName);
            Assert.Equal("timestamp with time zone", GetTypeMapping(typeof(DateTimeOffset)).StoreTypeName);
        }

        [Fact]
        public void Does_simple_ANSI_mappings_to_DbTypes()
        {
            Assert.Equal(DbType.Int32, GetTypeMapping(typeof(int)).StoreType);
            Assert.Equal(DbType.DateTime, GetTypeMapping(typeof(DateTime)).StoreType);
            Assert.Equal(DbType.Boolean, GetTypeMapping(typeof(bool)).StoreType);
            Assert.Equal(DbType.Double, GetTypeMapping(typeof(double)).StoreType);
            Assert.Equal(DbType.Int16, GetTypeMapping(typeof(short)).StoreType);
            Assert.Equal(DbType.Int64, GetTypeMapping(typeof(long)).StoreType);
            Assert.Equal(DbType.Single, GetTypeMapping(typeof(float)).StoreType);
            Assert.Equal(DbType.DateTimeOffset, GetTypeMapping(typeof(DateTimeOffset)).StoreType);
        }

        [Fact]
        public void Does_decimal_mapping()
        {
            var typeMapping = (RelationalDecimalTypeMapping)GetTypeMapping(typeof(decimal));

            Assert.Equal(DbType.Decimal, typeMapping.StoreType);
            Assert.Equal(18, typeMapping.Precision);
            Assert.Equal(2, typeMapping.Scale);
            Assert.Equal("decimal(18, 2)", typeMapping.StoreTypeName);
        }

        [Fact]
        public void Does_non_key_ANSI_string_mapping()
        {
            var typeMapping = (RelationalSizedTypeMapping)new ConcreteTypeMapper()
                .GetTypeMapping(null, "MyColumn", typeof(string), isKey: false, isConcurrencyToken: false);

            Assert.Equal(DbType.AnsiString, typeMapping.StoreType);
            Assert.Equal(4000, typeMapping.Size);
            Assert.Equal("varchar(4000)", typeMapping.StoreTypeName);
        }

        [Fact]
        public void Does_key_ANSI_string_mapping()
        {
            var typeMapping = (RelationalSizedTypeMapping)new ConcreteTypeMapper()
                .GetTypeMapping(null, "MyColumn", typeof(string), isKey: true, isConcurrencyToken: false);

            Assert.Equal(DbType.AnsiString, typeMapping.StoreType);
            Assert.Equal(128, typeMapping.Size);
            Assert.Equal("varchar(128)", typeMapping.StoreTypeName);
        }

        [Fact]
        public void Does_rowversion_mapping()
        {
            var typeMapping = (RelationalSizedTypeMapping)new ConcreteTypeMapper()
                .GetTypeMapping(null, "MyColumn", typeof(byte[]), isKey: false, isConcurrencyToken: true);

            Assert.Equal(DbType.Binary, typeMapping.StoreType);
            Assert.Equal(8, typeMapping.Size);
            Assert.Equal("rowversion", typeMapping.StoreTypeName);
        }

        [Fact]
        public void Throws_for_type_that_has_no_default_mapping()
        {
            Assert.Equal(
                Strings.FormatUnsupportedType("MyColumn", "Byte[]"),
                Assert.Throws<NotSupportedException>(() => GetTypeMapping(typeof(byte[]))).Message);
        }

        private static RelationalTypeMapping GetTypeMapping(Type propertyType)
        {
            return new ConcreteTypeMapper().GetTypeMapping(null, "MyColumn", propertyType, isKey: false, isConcurrencyToken: false);
        }

        private class ConcreteTypeMapper : RelationalTypeMapper
        {
        }
    }
}
