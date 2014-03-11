﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Utilities;
using Microsoft.Data.Migrations.Model;
using Microsoft.Data.Relational.Model;
using Moq;
using Xunit;

namespace Microsoft.Data.Migrations.Tests.Model
{
    public class AddPrimaryKeyOperationTest
    {
        [Fact]
        public void CreateAndInitializeOperation()
        {
            var table = new Table("T");
            var column = new Column("C", "int");
            table.AddColumn(column);
            var primaryKey = new PrimaryKey("PK", new[] { column });

            var addPrimaryKeyOperation = new AddPrimaryKeyOperation(primaryKey, table);

            Assert.Same(primaryKey, addPrimaryKeyOperation.Target);
            Assert.Same(table, addPrimaryKeyOperation.Table);
        }

        [Fact]
        public void ObtainInverse()
        {
            var table = new Table("T");
            var column = new Column("C", "int");
            table.AddColumn(column);
            var primaryKey = new PrimaryKey("PK", new[] { column });
            var addPrimaryKeyOperation = new AddPrimaryKeyOperation(primaryKey, table);

            var inverse = addPrimaryKeyOperation.Inverse;

            Assert.Same(primaryKey, inverse.Target);
            Assert.Same(table, inverse.Table);
        }

        [Fact]
        public void DispatchesSqlGeneration()
        {
            var table = new Table("T");
            var column = new Column("C", "int");
            table.AddColumn(column);
            var primaryKey = new PrimaryKey("PK", new[] { column });

            var addPrimaryKeyOperation = new AddPrimaryKeyOperation(primaryKey, table);
            var mockSqlGenerator = new Mock<MigrationOperationSqlGenerator>();
            var stringBuilder = new IndentedStringBuilder();

            addPrimaryKeyOperation.GenerateOperationSql(mockSqlGenerator.Object, stringBuilder, true);

            mockSqlGenerator.Verify(
                g => g.Generate(addPrimaryKeyOperation, stringBuilder, true), Times.Once());
        }
    }
}
