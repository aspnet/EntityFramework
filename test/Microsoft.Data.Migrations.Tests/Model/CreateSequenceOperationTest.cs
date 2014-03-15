﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Utilities;
using Microsoft.Data.Migrations.Model;
using Microsoft.Data.Relational.Model;
using Moq;
using Xunit;

namespace Microsoft.Data.Migrations.Tests.Model
{
    public class CreateSequenceOperationTest
    {
        [Fact]
        public void Create_and_initialize_operation()
        {
            var sequence = new Sequence("foo.bar");

            var createSequenceOperation = new CreateSequenceOperation(sequence);

            Assert.Same(sequence, createSequenceOperation.Sequence);
        }

        [Fact]
        public void Is_not_destructive_change()
        {
            var createSequenceOperation = new CreateSequenceOperation(new Sequence("foo.bar"));

            Assert.False(createSequenceOperation.IsDestructiveChange);
        }

        [Fact]
        public void Dispatches_sql_generation()
        {
            var createSequenceOperation = new CreateSequenceOperation(new Sequence("foo.bar"));
            var mockSqlGenerator = new Mock<MigrationOperationSqlGenerator>();
            var stringBuilder = new IndentedStringBuilder();

            createSequenceOperation.GenerateOperationSql(mockSqlGenerator.Object, stringBuilder, true);

            mockSqlGenerator.Verify(
                g => g.Generate(createSequenceOperation, stringBuilder, true), Times.Once());
        }
    }
}
