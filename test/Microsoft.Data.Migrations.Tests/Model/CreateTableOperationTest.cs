﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Utilities;
using Microsoft.Data.Migrations.Model;
using Microsoft.Data.Relational.Model;
using Moq;
using Xunit;

namespace Microsoft.Data.Migrations.Tests.Model
{
    public class CreateTableOperationTest
    {
        [Fact]
        public void Create_and_initialize_operation()
        {
            var table = new Table("foo.bar");

            var createTableOperation = new CreateTableOperation(table);

            Assert.Same(table, createTableOperation.Table);
        }

        [Fact]
        public void Is_not_destructive_change()
        {
            var createTableOperation = new CreateTableOperation(new Table("foo.bar"));

            Assert.False(createTableOperation.IsDestructiveChange);
        }

        [Fact]
        public void Dispatches_sql_generation()
        {
            var createTableOperation = new CreateTableOperation(new Table("foo.bar"));
            var mockSqlGenerator = new Mock<MigrationOperationSqlGenerator>();
            var stringBuilder = new IndentedStringBuilder();

            createTableOperation.GenerateOperationSql(mockSqlGenerator.Object, stringBuilder, true);

            mockSqlGenerator.Verify(
                g => g.Generate(createTableOperation, stringBuilder, true), Times.Once());
        }
    }
}
