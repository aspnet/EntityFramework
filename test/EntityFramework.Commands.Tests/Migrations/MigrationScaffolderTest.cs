﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Commands.Migrations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Migrations;
using Microsoft.Data.Entity.Migrations.Infrastructure;
using Microsoft.Data.Entity.Relational;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Commands.Tests.Migrations
{
    public class MigrationScaffolderTest
    {
        [Fact]
        public void Scaffold_empty_migration()
        {
            using (var context = new Context(new Model()))
            {
                var scaffolder
                    = new MyMigrationScaffolder(
                        context.Configuration,
                        MockMigrationAssembly(context.Configuration),
                        new ModelDiffer(new DatabaseBuilder()),
                        new CSharpMigrationCodeGenerator(
                            new CSharpModelCodeGenerator()),
                        ValidateEmptyMigration,
                        ValidateEmptyModelSnapshot);

                scaffolder.ScaffoldMigration("MyMigration");
            }
        }

        [Fact]
        public void Scaffold_migration()
        {
            using (var context = new Context(CreateModel()))
            {
                var scaffolder
                    = new MyMigrationScaffolder(
                        context.Configuration,
                        MockMigrationAssembly(context.Configuration),
                        new ModelDiffer(new DatabaseBuilder()),
                        new CSharpMigrationCodeGenerator(
                            new CSharpModelCodeGenerator()),
                        ValidateMigration,
                        ValidateModelSnapshot);

                scaffolder.ScaffoldMigration("MyMigration");
            }
        }

        [Fact]
        public void Scaffold_migration_with_foreign_keys()
        {
            using (var context = new Context(CreateModelWithForeignKeys()))
            {
                var scaffolder
                    = new MyMigrationScaffolder(
                        context.Configuration,
                        MockMigrationAssembly(context.Configuration),
                        new ModelDiffer(new DatabaseBuilder()),
                        new CSharpMigrationCodeGenerator(
                            new CSharpModelCodeGenerator()),
                        ValidateMigrationWithForeignKeys,
                        ValidateModelWithForeignKeysSnapshot);

                scaffolder.ScaffoldMigration("MyMigration");
            }
        }

        [Fact]
        public void Scaffold_migration_with_composite_keys()
        {
            using (var context = new Context(CreateModelWithCompositeKeys()))
            {
                var scaffolder
                    = new MyMigrationScaffolder(
                        context.Configuration,
                        MockMigrationAssembly(context.Configuration),
                        new ModelDiffer(new DatabaseBuilder()),
                        new CSharpMigrationCodeGenerator(
                            new CSharpModelCodeGenerator()),
                        ValidateMigrationWithCompositeKeys,
                        ValidateModelWithCompositeKeysSnapshot);

                scaffolder.ScaffoldMigration("MyMigration");
            }
        }

        private static void ValidateEmptyMigration(string className, string migrationClass, string migrationMetadataClass)
        {
            Assert.Equal("MyMigration", className);

            Assert.Equal(
                @"using Microsoft.Data.Entity.Migrations;
using Microsoft.Data.Entity.Migrations.Builders;
using Microsoft.Data.Entity.Relational.Model;
using System;

namespace MyNamespace
{
    public partial class MyMigration : Migration
    {
        public override void Up(MigrationBuilder migrationBuilder)
        {
        }
        
        public override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}",
                migrationClass);

            Assert.Equal(
@"using Microsoft.Data.Entity.Commands.Tests.Migrations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Migrations.Infrastructure;
using System;

namespace MyNamespace
{
    [ContextType(typeof(MigrationScaffolderTest.Context))]
    public partial class MyMigration : IMigrationMetadata
    {
        string IMigrationMetadata.MigrationId
        {
            get
            {
                return ""000000000000001_MyMigration"";
            }
        }
        
        IModel IMigrationMetadata.TargetModel
        {
            get
            {
                var builder = new BasicModelBuilder();
                
                return builder.Model;
            }
        }
    }
}",
                migrationMetadataClass);
        }

        private static void ValidateEmptyModelSnapshot(string className, string modelSnapshotClass)
        {
            Assert.Equal("ContextModelSnapshot", className);

            Assert.Equal(
                @"using Microsoft.Data.Entity.Commands.Tests.Migrations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Migrations.Infrastructure;
using System;

namespace MyNamespace
{
    [ContextType(typeof(MigrationScaffolderTest.Context))]
    public class ContextModelSnapshot : ModelSnapshot
    {
        public override IModel Model
        {
            get
            {
                var builder = new BasicModelBuilder();
                
                return builder.Model;
            }
        }
    }
}",
                modelSnapshotClass);
        }

        private static void ValidateMigration(string className, string migrationClass, string migrationMetadataClass)
        {
            Assert.Equal("MyMigration", className);

            Assert.Equal(@"using Microsoft.Data.Entity.Migrations;
using Microsoft.Data.Entity.Migrations.Builders;
using Microsoft.Data.Entity.Relational.Model;
using System;

namespace MyNamespace
{
    public partial class MyMigration : Migration
    {
        public override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(""dbo.MyTable"",
                c => new
                    {
                        Id = c.Int(nullable: false)
                    })
                .PrimaryKey(""MyPK"", t => t.Id);
        }
        
        public override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(""dbo.MyTable"");
        }
    }
}",
                migrationClass);

            Assert.Equal(
@"using Microsoft.Data.Entity.Commands.Tests.Migrations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Migrations.Infrastructure;
using System;

namespace MyNamespace
{
    [ContextType(typeof(MigrationScaffolderTest.Context))]
    public partial class MyMigration : IMigrationMetadata
    {
        string IMigrationMetadata.MigrationId
        {
            get
            {
                return ""000000000000001_MyMigration"";
            }
        }
        
        IModel IMigrationMetadata.TargetModel
        {
            get
            {
                var builder = new BasicModelBuilder();
                
                builder.Entity(""Microsoft.Data.Entity.Commands.Tests.Migrations.MigrationScaffolderTest+Entity"", b =>
                    {
                        b.Property<int>(""Id"");
                        b.Key(""Id"")
                            .KeyName(""MyPK"");
                        b.TableName(""MyTable"", ""dbo"");
                    });
                
                return builder.Model;
            }
        }
    }
}",
                migrationMetadataClass);
        }

        private static void ValidateModelSnapshot(string className, string modelSnapshotClass)
        {
            Assert.Equal("ContextModelSnapshot", className);

            Assert.Equal(
                @"using Microsoft.Data.Entity.Commands.Tests.Migrations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Migrations.Infrastructure;
using System;

namespace MyNamespace
{
    [ContextType(typeof(MigrationScaffolderTest.Context))]
    public class ContextModelSnapshot : ModelSnapshot
    {
        public override IModel Model
        {
            get
            {
                var builder = new BasicModelBuilder();
                
                builder.Entity(""Microsoft.Data.Entity.Commands.Tests.Migrations.MigrationScaffolderTest+Entity"", b =>
                    {
                        b.Property<int>(""Id"");
                        b.Key(""Id"")
                            .KeyName(""MyPK"");
                        b.TableName(""MyTable"", ""dbo"");
                    });
                
                return builder.Model;
            }
        }
    }
}",
                modelSnapshotClass);
        }

        private static void ValidateMigrationWithForeignKeys(string className, string migrationClass, string migrationMetadataClass)
        {
            Assert.Equal("MyMigration", className);

            Assert.Equal(
                @"using Microsoft.Data.Entity.Migrations;
using Microsoft.Data.Entity.Migrations.Builders;
using Microsoft.Data.Entity.Relational.Model;
using System;

namespace MyNamespace
{
    public partial class MyMigration : Migration
    {
        public override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(""[Ho!use[]]]"",
                c => new
                    {
                        Id = c.Int(nullable: false)
                    })
                .PrimaryKey(""PK_Ho!use[]"", t => t.Id);
            
            migrationBuilder.CreateTable(""dbo.[Cus[\""om.er]]s]"",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        HouseIdColumn = c.Int(name: ""House[\""Id]Column"", nullable: false)
                    })
                .PrimaryKey(""My[\""PK]"", t => t.Id);
            
            migrationBuilder.CreateTable(""dbo.[Ord[\""e.r]]s]"",
                c => new
                    {
                        OrderId = c.Int(nullable: false),
                        CustomerId = c.Int(nullable: false)
                    })
                .PrimaryKey(""PK_dbo.Ord[\""e.r]s"", t => t.OrderId);
            
            migrationBuilder.AddForeignKey(""dbo.[Cus[\""om.er]]s]"", ""My_[\""FK]"", new[] { ""House[\""Id]Column"" }, ""[Ho!use[]]]"", new[] { ""Id"" }, cascadeDelete: false);
            
            migrationBuilder.AddForeignKey(""dbo.[Ord[\""e.r]]s]"", ""FK_dbo.Ord[\""e.r]s_dbo.Cus[\""om.er]s_CustomerId"", new[] { ""CustomerId"" }, ""dbo.[Cus[\""om.er]]s]"", new[] { ""Id"" }, cascadeDelete: false);
        }
        
        public override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(""dbo.[Cus[\""om.er]]s]"", ""My_[\""FK]"");
            
            migrationBuilder.DropForeignKey(""dbo.[Ord[\""e.r]]s]"", ""FK_dbo.Ord[\""e.r]s_dbo.Cus[\""om.er]s_CustomerId"");
            
            migrationBuilder.DropTable(""[Ho!use[]]]"");
            
            migrationBuilder.DropTable(""dbo.[Cus[\""om.er]]s]"");
            
            migrationBuilder.DropTable(""dbo.[Ord[\""e.r]]s]"");
        }
    }
}",
                migrationClass);

            Assert.Equal(
@"using Microsoft.Data.Entity.Commands.Tests.Migrations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Migrations.Infrastructure;
using System;

namespace MyNamespace
{
    [ContextType(typeof(MigrationScaffolderTest.Context))]
    public partial class MyMigration : IMigrationMetadata
    {
        string IMigrationMetadata.MigrationId
        {
            get
            {
                return ""000000000000001_MyMigration"";
            }
        }
        
        IModel IMigrationMetadata.TargetModel
        {
            get
            {
                var builder = new BasicModelBuilder();
                
                builder.Entity(""Ho!use[]"", b =>
                    {
                        b.Property<int>(""Id"");
                        b.Key(""Id"");
                    });
                
                builder.Entity(""Microsoft.Data.Entity.Commands.Tests.Migrations.MigrationScaffolderTest+Customer"", b =>
                    {
                        b.Property<int>(""HouseId"")
                            .ColumnName(""House[\""Id]Column"");
                        b.Property<int>(""Id"");
                        b.Key(""Id"")
                            .KeyName(""My[\""PK]"")
                            .Annotation(""My\""PK\""Annotat!on"", ""\""Foo\"""");
                        b.TableName(""Cus[\""om.er]s"", ""dbo"");
                    });
                
                builder.Entity(""Microsoft.Data.Entity.Commands.Tests.Migrations.MigrationScaffolderTest+Order"", b =>
                    {
                        b.Property<int>(""CustomerId"");
                        b.Property<int>(""OrderId"");
                        b.Key(""OrderId"");
                        b.TableName(""Ord[\""e.r]s"", ""dbo"");
                        b.Annotation(""Random annotation"", ""42"");
                    });
                
                builder.Entity(""Microsoft.Data.Entity.Commands.Tests.Migrations.MigrationScaffolderTest+Customer"", b =>
                    {
                        b.ForeignKey(""Ho!use[]"", ""HouseId"")
                            .KeyName(""My_[\""FK]"")
                            .Annotation(""My\""FK\""Annotation"", ""\""Bar\"""");
                    });
                
                builder.Entity(""Microsoft.Data.Entity.Commands.Tests.Migrations.MigrationScaffolderTest+Order"", b =>
                    {
                        b.ForeignKey(""Microsoft.Data.Entity.Commands.Tests.Migrations.MigrationScaffolderTest+Customer"", ""CustomerId"");
                    });
                
                return builder.Model;
            }
        }
    }
}",
                migrationMetadataClass);
        }

        private static void ValidateModelWithForeignKeysSnapshot(string className, string modelSnapshotClass)
        {
            Assert.Equal("ContextModelSnapshot", className);

            Assert.Equal(
                @"using Microsoft.Data.Entity.Commands.Tests.Migrations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Migrations.Infrastructure;
using System;

namespace MyNamespace
{
    [ContextType(typeof(MigrationScaffolderTest.Context))]
    public class ContextModelSnapshot : ModelSnapshot
    {
        public override IModel Model
        {
            get
            {
                var builder = new BasicModelBuilder();
                
                builder.Entity(""Ho!use[]"", b =>
                    {
                        b.Property<int>(""Id"");
                        b.Key(""Id"");
                    });
                
                builder.Entity(""Microsoft.Data.Entity.Commands.Tests.Migrations.MigrationScaffolderTest+Customer"", b =>
                    {
                        b.Property<int>(""HouseId"")
                            .ColumnName(""House[\""Id]Column"");
                        b.Property<int>(""Id"");
                        b.Key(""Id"")
                            .KeyName(""My[\""PK]"")
                            .Annotation(""My\""PK\""Annotat!on"", ""\""Foo\"""");
                        b.TableName(""Cus[\""om.er]s"", ""dbo"");
                    });
                
                builder.Entity(""Microsoft.Data.Entity.Commands.Tests.Migrations.MigrationScaffolderTest+Order"", b =>
                    {
                        b.Property<int>(""CustomerId"");
                        b.Property<int>(""OrderId"");
                        b.Key(""OrderId"");
                        b.TableName(""Ord[\""e.r]s"", ""dbo"");
                        b.Annotation(""Random annotation"", ""42"");
                    });
                
                builder.Entity(""Microsoft.Data.Entity.Commands.Tests.Migrations.MigrationScaffolderTest+Customer"", b =>
                    {
                        b.ForeignKey(""Ho!use[]"", ""HouseId"")
                            .KeyName(""My_[\""FK]"")
                            .Annotation(""My\""FK\""Annotation"", ""\""Bar\"""");
                    });
                
                builder.Entity(""Microsoft.Data.Entity.Commands.Tests.Migrations.MigrationScaffolderTest+Order"", b =>
                    {
                        b.ForeignKey(""Microsoft.Data.Entity.Commands.Tests.Migrations.MigrationScaffolderTest+Customer"", ""CustomerId"");
                    });
                
                return builder.Model;
            }
        }
    }
}",
                modelSnapshotClass);
        }

        private static void ValidateMigrationWithCompositeKeys(string className, string migrationClass, string migrationMetadataClass)
        {
            Assert.Equal("MyMigration", className);

            Assert.Equal(
                @"using Microsoft.Data.Entity.Migrations;
using Microsoft.Data.Entity.Migrations.Builders;
using Microsoft.Data.Entity.Relational.Model;
using System;

namespace MyNamespace
{
    public partial class MyMigration : Migration
    {
        public override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(""EntityWithNamedKey"",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Foo = c.Int(nullable: false)
                    })
                .PrimaryKey(""MyPK2"", t => new { t.Id, t.Foo });
            
            migrationBuilder.CreateTable(""EntityWithNamedKeyAndAnnotations"",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Foo = c.Int(nullable: false)
                    })
                .PrimaryKey(""MyPK1"", t => new { t.Id, t.Foo });
            
            migrationBuilder.CreateTable(""EntityWithUnnamedKey"",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Foo = c.Int(nullable: false)
                    })
                .PrimaryKey(""PK_EntityWithUnnamedKey"", t => new { t.Id, t.Foo });
            
            migrationBuilder.CreateTable(""EntityWithUnnamedKeyAndAnnotations"",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Foo = c.Int(nullable: false)
                    })
                .PrimaryKey(""PK_EntityWithUnnamedKeyAndAnnotations"", t => new { t.Id, t.Foo });
        }
        
        public override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(""EntityWithNamedKey"");
            
            migrationBuilder.DropTable(""EntityWithNamedKeyAndAnnotations"");
            
            migrationBuilder.DropTable(""EntityWithUnnamedKey"");
            
            migrationBuilder.DropTable(""EntityWithUnnamedKeyAndAnnotations"");
        }
    }
}",
                migrationClass);

            Assert.Equal(
@"using Microsoft.Data.Entity.Commands.Tests.Migrations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Migrations.Infrastructure;
using System;

namespace MyNamespace
{
    [ContextType(typeof(MigrationScaffolderTest.Context))]
    public partial class MyMigration : IMigrationMetadata
    {
        string IMigrationMetadata.MigrationId
        {
            get
            {
                return ""000000000000001_MyMigration"";
            }
        }
        
        IModel IMigrationMetadata.TargetModel
        {
            get
            {
                var builder = new BasicModelBuilder();
                
                builder.Entity(""EntityWithNamedKey"", b =>
                    {
                        b.Property<int>(""Foo"");
                        b.Property<int>(""Id"");
                        b.Key(""Id"", ""Foo"")
                            .KeyName(""MyPK2"");
                    });
                
                builder.Entity(""EntityWithNamedKeyAndAnnotations"", b =>
                    {
                        b.Property<int>(""Foo"")
                            .Annotation(""Foo_Annotation"", ""Foo"");
                        b.Property<int>(""Id"")
                            .Annotation(""Id_Annotation1"", ""Id1"")
                            .Annotation(""Id_Annotation2"", ""Id2"");
                        b.Key(""Id"", ""Foo"")
                            .KeyName(""MyPK1"")
                            .Annotation(""KeyAnnotation1"", ""Key1"")
                            .Annotation(""KeyAnnotation2"", ""Key2"");
                    });
                
                builder.Entity(""EntityWithUnnamedKey"", b =>
                    {
                        b.Property<int>(""Foo"");
                        b.Property<int>(""Id"");
                        b.Key(""Id"", ""Foo"");
                    });
                
                builder.Entity(""EntityWithUnnamedKeyAndAnnotations"", b =>
                    {
                        b.Property<int>(""Foo"")
                            .Annotation(""Foo_Annotation"", ""Foo"");
                        b.Property<int>(""Id"")
                            .Annotation(""Id_Annotation1"", ""Id1"")
                            .Annotation(""Id_Annotation2"", ""Id2"");
                        b.Key(""Id"", ""Foo"")
                            .Annotation(""KeyAnnotation1"", ""Key1"")
                            .Annotation(""KeyAnnotation2"", ""Key2"");
                    });
                
                return builder.Model;
            }
        }
    }
}",
                migrationMetadataClass);
        }

        private static void ValidateModelWithCompositeKeysSnapshot(string className, string modelSnapshotClass)
        {
            Assert.Equal("ContextModelSnapshot", className);

            Assert.Equal(
                @"using Microsoft.Data.Entity.Commands.Tests.Migrations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Migrations.Infrastructure;
using System;

namespace MyNamespace
{
    [ContextType(typeof(MigrationScaffolderTest.Context))]
    public class ContextModelSnapshot : ModelSnapshot
    {
        public override IModel Model
        {
            get
            {
                var builder = new BasicModelBuilder();
                
                builder.Entity(""EntityWithNamedKey"", b =>
                    {
                        b.Property<int>(""Foo"");
                        b.Property<int>(""Id"");
                        b.Key(""Id"", ""Foo"")
                            .KeyName(""MyPK2"");
                    });
                
                builder.Entity(""EntityWithNamedKeyAndAnnotations"", b =>
                    {
                        b.Property<int>(""Foo"")
                            .Annotation(""Foo_Annotation"", ""Foo"");
                        b.Property<int>(""Id"")
                            .Annotation(""Id_Annotation1"", ""Id1"")
                            .Annotation(""Id_Annotation2"", ""Id2"");
                        b.Key(""Id"", ""Foo"")
                            .KeyName(""MyPK1"")
                            .Annotation(""KeyAnnotation1"", ""Key1"")
                            .Annotation(""KeyAnnotation2"", ""Key2"");
                    });
                
                builder.Entity(""EntityWithUnnamedKey"", b =>
                    {
                        b.Property<int>(""Foo"");
                        b.Property<int>(""Id"");
                        b.Key(""Id"", ""Foo"");
                    });
                
                builder.Entity(""EntityWithUnnamedKeyAndAnnotations"", b =>
                    {
                        b.Property<int>(""Foo"")
                            .Annotation(""Foo_Annotation"", ""Foo"");
                        b.Property<int>(""Id"")
                            .Annotation(""Id_Annotation1"", ""Id1"")
                            .Annotation(""Id_Annotation2"", ""Id2"");
                        b.Key(""Id"", ""Foo"")
                            .Annotation(""KeyAnnotation1"", ""Key1"")
                            .Annotation(""KeyAnnotation2"", ""Key2"");
                    });
                
                return builder.Model;
            }
        }
    }
}",
                modelSnapshotClass);
        }

        private static MigrationAssembly MockMigrationAssembly(DbContextConfiguration contextConfiguration)
        {
            var mock = new Mock<MigrationAssembly>(contextConfiguration);

            mock.SetupGet(ma => ma.Migrations).Returns(new IMigrationMetadata[0]);
            mock.SetupGet(ma => ma.Model).Returns((IModel)null);

            return mock.Object;
        }

        private static IModel CreateModel()
        {
            var model = new Model();
            var entityType = new EntityType(typeof(Entity));
            var property = entityType.GetOrAddProperty("Id", typeof(int));

            entityType.SetTableName("MyTable");
            entityType.SetSchema("dbo");
            entityType.GetOrSetPrimaryKey(property);
            entityType.GetPrimaryKey().SetKeyName("MyPK");
            model.AddEntityType(entityType);

            return model;
        }

        private static IModel CreateModelWithForeignKeys()
        {
            var model = new Model();

            var houseType = new EntityType("Ho!use[]");
            var houseId = houseType.GetOrAddProperty("Id", typeof(int), shadowProperty: true);
            houseType.GetOrSetPrimaryKey(houseId);
            model.AddEntityType(houseType);

            var customerType = new EntityType(typeof(Customer));
            var customerId = customerType.GetOrAddProperty("Id", typeof(int));
            var customerFkProperty = customerType.GetOrAddProperty("HouseId", typeof(int));
            customerFkProperty.SetColumnName(@"House[""Id]Column");
            customerType.SetSchema("dbo");
            customerType.SetTableName(@"Cus[""om.er]s");
            customerType.GetOrSetPrimaryKey(customerId);
            customerType.GetPrimaryKey().SetKeyName(@"My[""PK]");
            customerType.GetPrimaryKey().Annotations.Add(new Annotation(@"My""PK""Annotat!on", @"""Foo"""));
            var customerFk = customerType.GetOrAddForeignKey(houseType.GetPrimaryKey(), customerFkProperty);
            customerFk.SetKeyName(@"My_[""FK]");
            customerFk.Annotations.Add(new Annotation(@"My""FK""Annotation", @"""Bar"""));
            model.AddEntityType(customerType);

            var orderType = new EntityType(typeof(Order));
            var orderId = orderType.GetOrAddProperty(@"OrderId", typeof(int));
            var orderFK = orderType.GetOrAddProperty(@"CustomerId", typeof(int));
            orderType.SetSchema("dbo");
            orderType.GetOrSetPrimaryKey(orderId);
            orderType.SetTableName(@"Ord[""e.r]s");
            orderType.GetOrAddForeignKey(customerType.GetPrimaryKey(), orderFK);
            orderType.Annotations.Add(new Annotation("Random annotation", "42"));
            model.AddEntityType(orderType);

            return model;
        }

        private static IModel CreateModelWithCompositeKeys()
        {
            var model = new Model();
            var entity1 = new EntityType("EntityWithNamedKeyAndAnnotations");

            var id1 = entity1.GetOrAddProperty("Id", typeof(int), shadowProperty: true);
            id1.Annotations.Add(new Annotation("Id_Annotation1", "Id1"));
            id1.Annotations.Add(new Annotation("Id_Annotation2", "Id2"));
            var foo1 = entity1.GetOrAddProperty("Foo", typeof(int), shadowProperty: true);
            foo1.Annotations.Add(new Annotation("Foo_Annotation", "Foo"));

            entity1.GetOrSetPrimaryKey(id1, foo1);
            entity1.GetPrimaryKey().SetKeyName("MyPK1");
            entity1.GetPrimaryKey().Annotations.Add(new Annotation("KeyAnnotation1", "Key1"));
            entity1.GetPrimaryKey().Annotations.Add(new Annotation("KeyAnnotation2", "Key2"));
            model.AddEntityType(entity1);

            var entity2 = new EntityType("EntityWithUnnamedKeyAndAnnotations");

            var id2 = entity2.GetOrAddProperty("Id", typeof(int), shadowProperty: true);
            id2.Annotations.Add(new Annotation("Id_Annotation1", "Id1"));
            id2.Annotations.Add(new Annotation("Id_Annotation2", "Id2"));
            var foo2 = entity2.GetOrAddProperty("Foo", typeof(int), shadowProperty: true);
            foo2.Annotations.Add(new Annotation("Foo_Annotation", "Foo"));

            entity2.GetOrSetPrimaryKey(id2, foo2);
            entity2.GetPrimaryKey().Annotations.Add(new Annotation("KeyAnnotation1", "Key1"));
            entity2.GetPrimaryKey().Annotations.Add(new Annotation("KeyAnnotation2", "Key2"));
            model.AddEntityType(entity2);

            var entity3 = new EntityType("EntityWithNamedKey");
            var id3 = entity3.GetOrAddProperty("Id", typeof(int), shadowProperty: true);
            var foo3 = entity3.GetOrAddProperty("Foo", typeof(int), shadowProperty: true);
            entity3.GetOrSetPrimaryKey(id3, foo3);
            entity3.GetPrimaryKey().SetKeyName("MyPK2");
            model.AddEntityType(entity3);

            var entity4 = new EntityType("EntityWithUnnamedKey");
            var id4 = entity4.GetOrAddProperty("Id", typeof(int), shadowProperty: true);
            var foo4 = entity4.GetOrAddProperty("Foo", typeof(int), shadowProperty: true);
            entity4.GetOrSetPrimaryKey(id4, foo4);
            model.AddEntityType(entity4);

            return model;
        }

        private class Entity
        {
            public int Id { get; set; }
        }

        private class Customer
        {
            public int Id { get; set; }
            public int HouseId { get; set; }
        }

        private class Order
        {
            public int OrderId { get; set; }
            public int CustomerId { get; set; }
        }

        public class Context : DbContext
        {
            private readonly IModel _model;

            public Context(IModel model)
            {
                _model = model;
            }

            protected override void OnConfiguring(DbContextOptions builder)
            {
                var contextOptionsExtensions = (IDbContextOptionsExtensions)builder;

                builder.UseModel(_model);
                contextOptionsExtensions.AddOrUpdateExtension<MyRelationalOptionsExtension>(x => x.ConnectionString = "ConnectionString");
                contextOptionsExtensions.AddOrUpdateExtension<MyRelationalOptionsExtension>(x => x.MigrationNamespace = "MyNamespace");
            }
        }

        public class MyRelationalOptionsExtension : RelationalOptionsExtension
        {
            protected override void ApplyServices(EntityServicesBuilder builder)
            {
            }
        }

        public class MyMigrationScaffolder : MigrationScaffolder
        {
            private readonly Action<string, string, string> _migrationValidation;
            private readonly Action<string, string> _modelValidation;

            public MyMigrationScaffolder(
                DbContextConfiguration contextConfiguration,
                MigrationAssembly migrationAssembly,
                ModelDiffer modelDiffer,
                MigrationCodeGenerator migrationCodeGenerator,
                Action<string, string, string> migrationValidation,
                Action<string, string> modelValidation)
                : base(
                    contextConfiguration,
                    migrationAssembly,
                    modelDiffer,
                    migrationCodeGenerator)
            {
                _migrationValidation = migrationValidation;
                _modelValidation = modelValidation;
            }

            protected override string CreateMigrationId(string migrationName)
            {
                return "000000000000001_" + migrationName;
            }

            public override ScaffoldedMigration ScaffoldMigration(string migrationName)
            {
                var scaffoldedMigration = base.ScaffoldMigration(migrationName);

                _migrationValidation(
                    scaffoldedMigration.MigrationClass,
                    scaffoldedMigration.MigrationCode,
                    scaffoldedMigration.MigrationMetadataCode);

                _modelValidation(
                    scaffoldedMigration.SnapshotModelClass,
                    scaffoldedMigration.SnapshotModelCode);

                return scaffoldedMigration;
            }
        }
    }
}
