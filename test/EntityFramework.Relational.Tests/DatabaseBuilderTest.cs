// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.Data.Entity.Metadata;
using Xunit;

namespace Microsoft.Data.Entity.Relational.Tests
{
    public class DatabaseBuilderTest
    {
        [Fact]
        public void Build_creates_database()
        {
            var database = new DatabaseBuilder().GetDatabase(CreateModel());

            Assert.NotNull(database);
            Assert.Equal(2, database.Tables.Count);

            var table0 = database.Tables[0];
            var table1 = database.Tables[1];

            Assert.Equal("dbo.MyTable0", table0.Name);
            Assert.Equal(1, table0.Columns.Count);
            Assert.Equal("Id", table0.Columns[0].Name);
            Assert.Equal("int", table0.Columns[0].DataType);
            Assert.Equal(ValueGenerationOnSave.None, table0.Columns[0].ValueGenerationStrategy);
            Assert.NotNull(table1.PrimaryKey.Name);
            Assert.Equal("MyPK0", table0.PrimaryKey.Name);
            Assert.Same(table0.Columns[0], table0.PrimaryKey.Columns[0]);
            Assert.Equal(1, table0.ForeignKeys.Count);

            Assert.Equal("dbo.MyTable1", table1.Name);
            Assert.Equal(1, table1.Columns.Count);
            Assert.Equal("Id", table1.Columns[0].Name);
            Assert.Equal("int", table1.Columns[0].DataType);
            Assert.Equal(ValueGenerationOnSave.WhenInserting, table1.Columns[0].ValueGenerationStrategy);
            Assert.NotNull(table1.PrimaryKey.Name);
            Assert.Equal("MyPK1", table1.PrimaryKey.Name);
            Assert.Same(table1.Columns[0], table1.PrimaryKey.Columns[0]);
            Assert.Equal(0, table1.ForeignKeys.Count);

            var foreignKey = table0.ForeignKeys[0];

            Assert.Equal("MyFK", foreignKey.Name);
            Assert.Same(table0, foreignKey.Table);
            Assert.Same(table1, foreignKey.ReferencedTable);
            Assert.Same(table0.Columns[0], foreignKey.Columns[0]);
            Assert.Same(table1.Columns[0], foreignKey.ReferencedColumns[0]);
            Assert.True(foreignKey.CascadeDelete);

            var index = table0.Indexes[0];

            Assert.Equal("MyIndex", index.Name);
            Assert.Same(table0, index.Table);
            Assert.Same(table0.Columns[0], index.Columns[0]);
            Assert.True(index.IsUnique);
        }

        [Fact]
        public void Build_fills_in_names_if_StorageName_not_specified()
        {
            // TODO: Add and Index when supported by DatabaseBuilder.

            var modelBuilder = new ModelBuilder();

            modelBuilder.Entity<Blog>()
                .Key(k => k.BlogId)
                .Properties(p => p.Property(e => e.BlogId));

            modelBuilder.Entity<Post>()
                .Key(k => k.PostId)
                .Properties(p =>
                    {
                        p.Property(e => e.PostId);
                        p.Property(e => e.BelongsToBlogId);
                    })
                .ForeignKeys(f => f.ForeignKey<Blog>(p => p.BelongsToBlogId))
                .Indexes(ixs => ixs.Index(ix => ix.PostId));

            var database = new DatabaseBuilder().GetDatabase(modelBuilder.Model);

            Assert.True(database.Tables.Any(t => t.Name == "Blog"));
            Assert.True(database.Tables.Any(t => t.Name == "Post"));

            Assert.Equal("BlogId", database.GetTable("Blog").Columns.Single().Name);
            Assert.Equal("PostId", database.GetTable("Post").Columns[1].Name);
            Assert.Equal("BelongsToBlogId", database.GetTable("Post").Columns[0].Name);

            Assert.Equal("PK_Blog", database.GetTable("Blog").PrimaryKey.Name);
            Assert.Equal("PK_Post", database.GetTable("Post").PrimaryKey.Name);

            Assert.Equal("FK_Post_Blog_BelongsToBlogId", database.GetTable("Post").ForeignKeys.Single().Name);

            Assert.Equal("IX_Post_PostId", database.GetTable("Post").Indexes.Single().Name);
        }

        private class Blog
        {
            public int BlogId { get; set; }
        }

        private class Post
        {
            public int PostId { get; set; }
            public int BelongsToBlogId { get; set; }
        }

        [Fact]
        public void Name_for_multi_column_FKs()
        {
            var modelBuilder = new ModelBuilder();

            modelBuilder.Entity<Principal>()
                .Key(k => new { k.Id0, k.Id1 });

            modelBuilder.Entity<Dependent>()
                .Key(k => k.Id)
                .ForeignKeys(f => f.ForeignKey<Principal>(p => new { p.FkAAA, p.FkZZZ }));

            var builder = new DatabaseBuilder();
            var name = builder.GetDatabase(modelBuilder.Model).GetTable("Dependent").ForeignKeys.Single().Name;

            Assert.Equal("FK_Dependent_Principal_FkAAA_FkZZZ", name);
        }

        private class Principal
        {
            public int Id0 { get; set; }
            public int Id1 { get; set; }
        }

        private class Dependent
        {
            public int Id { get; set; }
            public int FkAAA { get; set; }
            public int FkZZZ { get; set; }
        }

        [Fact]
        public void Name_for_multi_column_Indexes()
        {
            var modelBuilder = new ModelBuilder();

            modelBuilder.Entity<Dependent>()
                .ToTable("MyTable")
                .Properties(
                    ps =>
                        {
                            ps.Property(e => e.Id);
                            ps.Property(e => e.FkAAA).ColumnName("ColumnAaa");
                            ps.Property(e => e.FkZZZ).ColumnName("ColumnZzz");
                        })
                .Key(e => e.Id)
                .Indexes(
                    ixs => ixs.Index(e => new { e.FkAAA, e.FkZZZ }));

            var builder = new DatabaseBuilder();
            var name = builder.GetDatabase(modelBuilder.Model).GetTable("MyTable").Indexes.Single().Name;

            Assert.Equal("IX_MyTable_ColumnAaa_ColumnZzz", name);
        }

        private static IModel CreateModel()
        {
            var model = new Metadata.Model { StorageName = "MyDatabase" };

            var dependentEntityType = new EntityType("Dependent");
            dependentEntityType.SetSchema("dbo");
            dependentEntityType.SetTableName("MyTable0");

            var principalEntityType = new EntityType("Principal");
            principalEntityType.SetSchema("dbo");
            principalEntityType.SetTableName("MyTable1");

            var dependentProperty = dependentEntityType.AddProperty("Id", typeof(int));
            var principalProperty = principalEntityType.AddProperty("Id", typeof(int));
            principalProperty.ValueGenerationOnSave = ValueGenerationOnSave.WhenInserting;

            model.AddEntityType(principalEntityType);
            model.AddEntityType(dependentEntityType);

            principalProperty.Annotations.Add(new Annotation(
                MetadataExtensions.Annotations.StorageTypeName, "int"));
            dependentProperty.Annotations.Add(new Annotation(
                MetadataExtensions.Annotations.StorageTypeName, "int"));

            dependentEntityType.SetKey(dependentProperty);
            principalEntityType.SetKey(principalProperty);
            dependentEntityType.GetKey().SetKeyName("MyPK0");
            principalEntityType.GetKey().SetKeyName("MyPK1");

            var foreignKey = dependentEntityType.AddForeignKey(principalEntityType.GetKey(), dependentProperty);
            foreignKey.SetKeyName("MyFK");
            foreignKey.Annotations.Add(new Annotation(
                MetadataExtensions.Annotations.CascadeDelete, "True"));

            var index = dependentEntityType.AddIndex(dependentProperty);
            index.SetIndexName("MyIndex");
            index.IsUnique = true;

            return model;
        }
    }
}
