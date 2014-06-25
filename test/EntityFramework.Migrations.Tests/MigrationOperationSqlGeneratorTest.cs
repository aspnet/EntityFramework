// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.Data.Entity.Migrations.Model;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.Relational.Model;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Migrations.Tests
{
    public class MigrationOperationSqlGeneratorTest
    {
        [Fact]
        public void Generate_when_create_database_operation()
        {
            Assert.Equal(
                @"CREATE DATABASE ""MyDatabase""",
                Generate(new CreateDatabaseOperation("MyDatabase")).Sql);
        }

        [Fact]
        public void Generate_when_drop_database_operation()
        {
            Assert.Equal(
                @"DROP DATABASE ""MyDatabase""",
                Generate(new DropDatabaseOperation("MyDatabase")).Sql);
        }

        [Fact]
        public void Generate_when_create_sequence_operation()
        {
            Assert.Equal(
                @"CREATE SEQUENCE ""dbo"".""MySequence"" AS BIGINT START WITH 0 INCREMENT BY 1",
                Generate(
                    new CreateSequenceOperation(new Sequence("dbo.MySequence"))).Sql);
        }

        [Fact]
        public void Generate_when_drop_sequence_operation()
        {
            Assert.Equal(
                @"DROP SEQUENCE ""dbo"".""MySequence""",
                Generate(new DropSequenceOperation("dbo.MySequence")).Sql);
        }

        [Fact]
        public void Generate_when_create_table_operation()
        {
            Column foo, bar;
            var table = new Table(
                "dbo.MyTable",
                new[]
                    {
                        foo = new Column("Foo", "int") { IsNullable = false, DefaultValue = 5 },
                        bar = new Column("Bar", "int") { IsNullable = true }
                    })
                {
                    PrimaryKey = new PrimaryKey("MyPK", new[] { foo, bar }, isClustered: false)
                };

            Assert.Equal(
                @"CREATE TABLE ""dbo"".""MyTable"" (
    ""Foo"" int NOT NULL DEFAULT 5,
    ""Bar"" int,
    CONSTRAINT ""MyPK"" PRIMARY KEY (""Foo"", ""Bar"")
)",
                Generate(
                    new CreateTableOperation(table)).Sql);
        }

        [Fact]
        public void Generate_when_drop_table_operation()
        {
            Assert.Equal(
                @"DROP TABLE ""dbo"".""MyTable""",
                Generate(new DropTableOperation("dbo.MyTable")).Sql);
        }

        [Fact]
        public void Generate_when_rename_table_operation()
        {
            Assert.Throws<NotImplementedException>(() => Generate(
                new RenameTableOperation("dbo.MyTable", "MyTable2")).Sql);
        }

        [Fact]
        public void Generate_when_move_table_operation()
        {
            Assert.Equal(
                @"ALTER SCHEMA ""dbo2"" TRANSFER ""dbo"".""MyTable""",
                Generate(
                    new MoveTableOperation("dbo.MyTable", "dbo2")).Sql);
        }

        [Fact]
        public void Generate_when_add_column_operation()
        {
            var column = new Column("Bar", "int") { IsNullable = false, DefaultValue = 5 };

            Assert.Equal(
                @"ALTER TABLE ""dbo"".""MyTable"" ADD ""Bar"" int NOT NULL DEFAULT 5",
                Generate(
                    new AddColumnOperation("dbo.MyTable", column)).Sql);
        }

        [Fact]
        public void Generate_when_drop_column_operation()
        {
            Assert.Equal(
                @"ALTER TABLE ""dbo"".""MyTable"" DROP COLUMN ""Foo""",
                Generate(
                    new DropColumnOperation("dbo.MyTable", "Foo")).Sql);
        }

        [Fact]
        public void Generate_when_alter_column_operation_with_nullable()
        {
            Assert.Equal(
                @"ALTER TABLE ""dbo"".""MyTable"" ALTER COLUMN ""Foo"" int NULL",
                Generate(
                    new AlterColumnOperation("dbo.MyTable",
                        new Column("Foo", "int") { IsNullable = true }, isDestructiveChange: false)).Sql);
        }

        [Fact]
        public void Generate_when_alter_column_operation_with_not_nullable()
        {
            Assert.Equal(
                @"ALTER TABLE ""dbo"".""MyTable"" ALTER COLUMN ""Foo"" int NOT NULL",
                Generate(
                    new AlterColumnOperation("dbo.MyTable",
                        new Column("Foo", "int") { IsNullable = false }, isDestructiveChange: false)).Sql);
        }

        [Fact]
        public void Generate_when_add_default_constraint_operation_with_default_value()
        {
            Assert.Equal(
                @"ALTER TABLE ""dbo"".""MyTable"" ALTER COLUMN ""Foo"" SET DEFAULT 'MyDefault'",
                Generate(
                    new AddDefaultConstraintOperation("dbo.MyTable", "Foo", "MyDefault", null)).Sql);
        }

        [Fact]
        public void Generate_when_add_default_constraint_operation_with_default_sql()
        {
            Assert.Equal(
                @"ALTER TABLE ""dbo"".""MyTable"" ALTER COLUMN ""Foo"" SET DEFAULT GETDATE()",
                Generate(
                    new AddDefaultConstraintOperation("dbo.MyTable", "Foo", null, "GETDATE()")).Sql);
        }

        [Fact]
        public void Generate_when_drop_default_constraint_operation()
        {
            Assert.Equal(
                @"ALTER TABLE ""dbo"".""MyTable"" ALTER COLUMN ""Foo"" DROP DEFAULT",
                Generate(
                    new DropDefaultConstraintOperation("dbo.MyTable", "Foo")).Sql);
        }

        [Fact]
        public void Generate_when_rename_column_operation()
        {
            Assert.Throws<NotImplementedException>(() => Generate(
                new RenameColumnOperation("dbo.MyTable", "Foo", "Bar")).Sql);
        }

        [Fact]
        public void Generate_when_add_primary_key_operation()
        {
            Assert.Equal(
                @"ALTER TABLE ""dbo"".""MyTable"" ADD CONSTRAINT ""MyPK"" PRIMARY KEY (""Foo"", ""Bar"")",
                Generate(
                    new AddPrimaryKeyOperation("dbo.MyTable", "MyPK", new[] { "Foo", "Bar" }, isClustered: false)).Sql);
        }

        [Fact]
        public void Generate_when_drop_primary_key_operation()
        {
            Assert.Equal(
                @"ALTER TABLE ""dbo"".""MyTable"" DROP CONSTRAINT ""MyPK""",
                Generate(new DropPrimaryKeyOperation("dbo.MyTable", "MyPK")).Sql);
        }

        [Fact]
        public void Generate_when_add_foreign_key_operation()
        {
            Assert.Equal(
                @"ALTER TABLE ""dbo"".""MyTable"" ADD CONSTRAINT ""MyFK"" FOREIGN KEY (""Foo"", ""Bar"") REFERENCES ""dbo"".""MyTable2"" (""Foo2"", ""Bar2"") ON DELETE CASCADE",
                Generate(
                    new AddForeignKeyOperation("dbo.MyTable", "MyFK", new[] { "Foo", "Bar" },
                        "dbo.MyTable2", new[] { "Foo2", "Bar2" }, cascadeDelete: true)).Sql);
        }

        [Fact]
        public void Generate_when_drop_foreign_key_operation()
        {
            Assert.Equal(
                @"ALTER TABLE ""dbo"".""MyTable"" DROP CONSTRAINT ""MyFK""",
                Generate(new DropForeignKeyOperation("dbo.MyTable", "MyFK")).Sql);
        }

        [Fact]
        public void Generate_when_create_index_operation()
        {
            Assert.Equal(
                @"CREATE UNIQUE CLUSTERED INDEX ""MyIndex"" ON ""dbo"".""MyTable"" (""Foo"", ""Bar"")",
                Generate(
                    new CreateIndexOperation("dbo.MyTable", "MyIndex", new[] { "Foo", "Bar" },
                        isUnique: true, isClustered: true)).Sql);
        }

        [Fact]
        public void Generate_when_drop_index_operation()
        {
            Assert.Equal(
                @"DROP INDEX ""MyIndex""",
                Generate(new DropIndexOperation("dbo.MyTable", "MyIndex")).Sql);
        }

        [Fact]
        public void Generate_when_rename_index_operation()
        {
            Assert.Throws<NotImplementedException>(() => Generate(
                new RenameIndexOperation("dbo.MyTable", "MyIndex", "MyIndex2")).Sql);
        }

        [Fact]
        public void Delimit_identifier()
        {
            var sqlGenerator = new Mock<MigrationOperationSqlGenerator>(new RelationalTypeMapper()) { CallBase = true };

            Assert.Equal("\"foo\"\"bar\"", sqlGenerator.Object.DelimitIdentifier("foo\"bar"));
        }

        [Fact]
        public void Delimit_identifier_when_schema_qualified()
        {
            var sqlGenerator = new Mock<MigrationOperationSqlGenerator>(new RelationalTypeMapper()) { CallBase = true };

            Assert.Equal("\"foo\".\"bar\"", sqlGenerator.Object.DelimitIdentifier(SchemaQualifiedName.Parse("foo.bar")));
        }

        [Fact]
        public void Escape_identifier()
        {
            var sqlGenerator = new Mock<MigrationOperationSqlGenerator>(new RelationalTypeMapper()) { CallBase = true };

            Assert.Equal("foo\"\"bar", sqlGenerator.Object.EscapeIdentifier("foo\"bar"));
        }

        [Fact]
        public void Delimit_literal()
        {
            var sqlGenerator = new Mock<MigrationOperationSqlGenerator>(new RelationalTypeMapper()) { CallBase = true };

            Assert.Equal("'foo''bar'", sqlGenerator.Object.DelimitLiteral("foo'bar"));
        }

        [Fact]
        public void Escape_literal()
        {
            var sqlGenerator = new Mock<MigrationOperationSqlGenerator>(new RelationalTypeMapper()) { CallBase = true };

            Assert.Equal("foo''bar", sqlGenerator.Object.EscapeLiteral("foo'bar"));
        }

        private static SqlStatement Generate(MigrationOperation migrationOperation)
        {
            var sqlGenerator = new Mock<MigrationOperationSqlGenerator>(new RelationalTypeMapper()) { CallBase = true };

            return sqlGenerator.Object.Generate(new[] { migrationOperation }).Single();
        }
    }
}
