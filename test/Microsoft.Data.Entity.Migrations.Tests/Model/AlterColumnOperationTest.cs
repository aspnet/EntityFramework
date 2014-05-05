// Copyright (c) Microsoft Open Technologies, Inc.
// All Rights Reserved
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR
// CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING
// WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR CONDITIONS OF
// TITLE, FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR
// NON-INFRINGEMENT.
// See the Apache 2 License for the specific language governing
// permissions and limitations under the License.

using Microsoft.Data.Entity.Utilities;
using Microsoft.Data.Entity.Migrations.Model;
using Microsoft.Data.Entity.Relational.Model;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Migrations.Tests.Model
{
    public class AlterColumnOperationTest
    {
        [Fact]
        public void Create_and_initialize_operation()
        {
            var newColumn = new Column("Foo", "int") { IsNullable = true };
            var alterColumnOperation = new AlterColumnOperation(
                "dbo.MyTable", newColumn, isDestructiveChange: true);

            Assert.Equal("dbo.MyTable", alterColumnOperation.TableName);
            Assert.Same(newColumn, alterColumnOperation.NewColumn);
            Assert.True(alterColumnOperation.IsDestructiveChange);
        }

        [Fact]
        public void Dispatches_visitor()
        {
            var newColumn = new Column("Foo", "int") { IsNullable = true };
            var alterColumnOperation = new AlterColumnOperation(
                "dbo.MyTable", newColumn, isDestructiveChange: true);
            var mockVisitor = new Mock<MigrationOperationSqlGenerator>();
            var builder = new Mock<IndentedStringBuilder>();
            alterColumnOperation.GenerateSql(mockVisitor.Object, builder.Object, false);

            mockVisitor.Verify(g => g.Generate(alterColumnOperation, builder.Object, false), Times.Once());
        }
    }
}
