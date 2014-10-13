﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Metadata;
using Microsoft.Data.Entity.SqlServer.Metadata;
using Xunit;

namespace Microsoft.Data.Entity.SqlServer.Tests.Metadata
{
    public class SqlServerMetadataExtensionsTest
    {
        [Fact]
        public void Can_get_and_set_column_name()
        {
            var modelBuilder = new BasicModelBuilder();

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.Name)
                .Metadata;

            Assert.Equal("Name", property.SqlServer().Column);
            Assert.Equal("Name", ((IProperty)property).SqlServer().Column);

            property.Relational().Column = "Eman";

            Assert.Equal("Name", property.Name);
            Assert.Equal("Name", ((IProperty)property).Name);
            Assert.Equal("Eman", property.Relational().Column);
            Assert.Equal("Eman", ((IProperty)property).Relational().Column);
            Assert.Equal("Eman", property.SqlServer().Column);
            Assert.Equal("Eman", ((IProperty)property).SqlServer().Column);

            property.SqlServer().Column = "MyNameIs";

            Assert.Equal("Name", property.Name);
            Assert.Equal("Name", ((IProperty)property).Name);
            Assert.Equal("Eman", property.Relational().Column);
            Assert.Equal("Eman", ((IProperty)property).Relational().Column);
            Assert.Equal("MyNameIs", property.SqlServer().Column);
            Assert.Equal("MyNameIs", ((IProperty)property).SqlServer().Column);

            property.SqlServer().Column = null;

            Assert.Equal("Name", property.Name);
            Assert.Equal("Name", ((IProperty)property).Name);
            Assert.Equal("Eman", property.Relational().Column);
            Assert.Equal("Eman", ((IProperty)property).Relational().Column);
            Assert.Equal("Eman", property.SqlServer().Column);
            Assert.Equal("Eman", ((IProperty)property).SqlServer().Column);
        }

        [Fact]
        public void Can_get_and_set_table_name()
        {
            var modelBuilder = new BasicModelBuilder();

            var entityType = modelBuilder
                .Entity<Customer>()
                .Metadata;

            Assert.Equal("Customer", entityType.SqlServer().Table);
            Assert.Equal("Customer", ((IEntityType)entityType).SqlServer().Table);

            entityType.Relational().Table = "Customizer";

            Assert.Equal("Customer", entityType.SimpleName);
            Assert.Equal("Customer", ((IEntityType)entityType).SimpleName);
            Assert.Equal("Customizer", entityType.Relational().Table);
            Assert.Equal("Customizer", ((IEntityType)entityType).Relational().Table);
            Assert.Equal("Customizer", entityType.SqlServer().Table);
            Assert.Equal("Customizer", ((IEntityType)entityType).SqlServer().Table);

            entityType.SqlServer().Table = "Custardizer";

            Assert.Equal("Customer", entityType.SimpleName);
            Assert.Equal("Customer", ((IEntityType)entityType).SimpleName);
            Assert.Equal("Customizer", entityType.Relational().Table);
            Assert.Equal("Customizer", ((IEntityType)entityType).Relational().Table);
            Assert.Equal("Custardizer", entityType.SqlServer().Table);
            Assert.Equal("Custardizer", ((IEntityType)entityType).SqlServer().Table);

            entityType.SqlServer().Table = null;

            Assert.Equal("Customer", entityType.SimpleName);
            Assert.Equal("Customer", ((IEntityType)entityType).SimpleName);
            Assert.Equal("Customizer", entityType.Relational().Table);
            Assert.Equal("Customizer", ((IEntityType)entityType).Relational().Table);
            Assert.Equal("Customizer", entityType.SqlServer().Table);
            Assert.Equal("Customizer", ((IEntityType)entityType).SqlServer().Table);
        }

        [Fact]
        public void Can_get_and_set_schema_name()
        {
            var modelBuilder = new BasicModelBuilder();

            var entityType = modelBuilder
                .Entity<Customer>()
                .Metadata;

            Assert.Null(entityType.Relational().Schema);
            Assert.Null(((IEntityType)entityType).Relational().Schema);
            Assert.Null(entityType.SqlServer().Schema);
            Assert.Null(((IEntityType)entityType).SqlServer().Schema);

            entityType.Relational().Schema = "db0";

            Assert.Equal("db0", entityType.Relational().Schema);
            Assert.Equal("db0", ((IEntityType)entityType).Relational().Schema);
            Assert.Equal("db0", entityType.SqlServer().Schema);
            Assert.Equal("db0", ((IEntityType)entityType).SqlServer().Schema);

            entityType.SqlServer().Schema = "dbOh";

            Assert.Equal("db0", entityType.Relational().Schema);
            Assert.Equal("db0", ((IEntityType)entityType).Relational().Schema);
            Assert.Equal("dbOh", entityType.SqlServer().Schema);
            Assert.Equal("dbOh", ((IEntityType)entityType).SqlServer().Schema);

            entityType.SqlServer().Schema = null;

            Assert.Equal("db0", entityType.Relational().Schema);
            Assert.Equal("db0", ((IEntityType)entityType).Relational().Schema);
            Assert.Equal("db0", entityType.SqlServer().Schema);
            Assert.Equal("db0", ((IEntityType)entityType).SqlServer().Schema);
        }

        [Fact]
        public void Can_get_and_set_column_type()
        {
            var modelBuilder = new BasicModelBuilder();

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.Name)
                .Metadata;

            Assert.Null(property.Relational().ColumnType);
            Assert.Null(((IProperty)property).Relational().ColumnType);
            Assert.Null(property.SqlServer().ColumnType);
            Assert.Null(((IProperty)property).SqlServer().ColumnType);

            property.Relational().ColumnType = "nvarchar(max)";

            Assert.Equal("nvarchar(max)", property.Relational().ColumnType);
            Assert.Equal("nvarchar(max)", ((IProperty)property).Relational().ColumnType);
            Assert.Equal("nvarchar(max)", property.SqlServer().ColumnType);
            Assert.Equal("nvarchar(max)", ((IProperty)property).SqlServer().ColumnType);

            property.SqlServer().ColumnType = "nvarchar(verstappen)";

            Assert.Equal("nvarchar(max)", property.Relational().ColumnType);
            Assert.Equal("nvarchar(max)", ((IProperty)property).Relational().ColumnType);
            Assert.Equal("nvarchar(verstappen)", property.SqlServer().ColumnType);
            Assert.Equal("nvarchar(verstappen)", ((IProperty)property).SqlServer().ColumnType);

            property.SqlServer().ColumnType = null;

            Assert.Equal("nvarchar(max)", property.Relational().ColumnType);
            Assert.Equal("nvarchar(max)", ((IProperty)property).Relational().ColumnType);
            Assert.Equal("nvarchar(max)", property.SqlServer().ColumnType);
            Assert.Equal("nvarchar(max)", ((IProperty)property).SqlServer().ColumnType);
        }

        [Fact]
        public void Can_get_and_set_column_default_expression()
        {
            var modelBuilder = new BasicModelBuilder();

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.Name)
                .Metadata;

            Assert.Null(property.Relational().DefaultExpression);
            Assert.Null(((IProperty)property).Relational().DefaultExpression);
            Assert.Null(property.SqlServer().DefaultExpression);
            Assert.Null(((IProperty)property).SqlServer().DefaultExpression);

            property.Relational().DefaultExpression = "newsequentialid()";

            Assert.Equal("newsequentialid()", property.Relational().DefaultExpression);
            Assert.Equal("newsequentialid()", ((IProperty)property).Relational().DefaultExpression);
            Assert.Equal("newsequentialid()", property.SqlServer().DefaultExpression);
            Assert.Equal("newsequentialid()", ((IProperty)property).SqlServer().DefaultExpression);

            property.SqlServer().DefaultExpression = "expressyourself()";

            Assert.Equal("newsequentialid()", property.Relational().DefaultExpression);
            Assert.Equal("newsequentialid()", ((IProperty)property).Relational().DefaultExpression);
            Assert.Equal("expressyourself()", property.SqlServer().DefaultExpression);
            Assert.Equal("expressyourself()", ((IProperty)property).SqlServer().DefaultExpression);

            property.SqlServer().DefaultExpression = null;

            Assert.Equal("newsequentialid()", property.Relational().DefaultExpression);
            Assert.Equal("newsequentialid()", ((IProperty)property).Relational().DefaultExpression);
            Assert.Equal("newsequentialid()", property.SqlServer().DefaultExpression);
            Assert.Equal("newsequentialid()", ((IProperty)property).SqlServer().DefaultExpression);
        }

        [Fact]
        public void Can_get_and_set_column_key_name()
        {
            var modelBuilder = new BasicModelBuilder();

            var key = modelBuilder
                .Entity<Customer>()
                .Key(e => e.Id)
                .Metadata;

            Assert.Null(key.Relational().Name);
            Assert.Null(((IKey)key).Relational().Name);
            Assert.Null(key.SqlServer().Name);
            Assert.Null(((IKey)key).SqlServer().Name);

            key.Relational().Name = "PrimaryKey";

            Assert.Equal("PrimaryKey", key.Relational().Name);
            Assert.Equal("PrimaryKey", ((IKey)key).Relational().Name);
            Assert.Equal("PrimaryKey", key.SqlServer().Name);
            Assert.Equal("PrimaryKey", ((IKey)key).SqlServer().Name);

            key.SqlServer().Name = "PrimarySchool";

            Assert.Equal("PrimaryKey", key.Relational().Name);
            Assert.Equal("PrimaryKey", ((IKey)key).Relational().Name);
            Assert.Equal("PrimarySchool", key.SqlServer().Name);
            Assert.Equal("PrimarySchool", ((IKey)key).SqlServer().Name);

            key.SqlServer().Name = null;

            Assert.Equal("PrimaryKey", key.Relational().Name);
            Assert.Equal("PrimaryKey", ((IKey)key).Relational().Name);
            Assert.Equal("PrimaryKey", key.SqlServer().Name);
            Assert.Equal("PrimaryKey", ((IKey)key).SqlServer().Name);
        }

        [Fact]
        public void Can_get_and_set_column_foreign_key_name()
        {
            var modelBuilder = new BasicModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .Key(e => e.Id);

            var foreignKey = modelBuilder
                .Entity<Order>()
                .ForeignKey<Customer>(e => e.CustomerId)
                .Metadata;

            Assert.Null(foreignKey.Relational().Name);
            Assert.Null(((IForeignKey)foreignKey).Relational().Name);
            Assert.Null(foreignKey.SqlServer().Name);
            Assert.Null(((IForeignKey)foreignKey).SqlServer().Name);

            foreignKey.Relational().Name = "FK";

            Assert.Equal("FK", foreignKey.Relational().Name);
            Assert.Equal("FK", ((IForeignKey)foreignKey).Relational().Name);
            Assert.Equal("FK", foreignKey.SqlServer().Name);
            Assert.Equal("FK", ((IForeignKey)foreignKey).SqlServer().Name);

            foreignKey.SqlServer().Name = "KFC";

            Assert.Equal("FK", foreignKey.Relational().Name);
            Assert.Equal("FK", ((IForeignKey)foreignKey).Relational().Name);
            Assert.Equal("KFC", foreignKey.SqlServer().Name);
            Assert.Equal("KFC", ((IForeignKey)foreignKey).SqlServer().Name);

            foreignKey.SqlServer().Name = null;

            Assert.Equal("FK", foreignKey.Relational().Name);
            Assert.Equal("FK", ((IForeignKey)foreignKey).Relational().Name);
            Assert.Equal("FK", foreignKey.SqlServer().Name);
            Assert.Equal("FK", ((IForeignKey)foreignKey).SqlServer().Name);
        }

        [Fact]
        public void Can_get_and_set_index_name()
        {
            var modelBuilder = new BasicModelBuilder();

            var index = modelBuilder
                .Entity<Customer>()
                .Index(e => e.Id)
                .Metadata;

            Assert.Null(index.Relational().Name);
            Assert.Null(((IIndex)index).Relational().Name);
            Assert.Null(index.SqlServer().Name);
            Assert.Null(((IIndex)index).SqlServer().Name);

            index.Relational().Name = "MyIndex";

            Assert.Equal("MyIndex", index.Relational().Name);
            Assert.Equal("MyIndex", ((IIndex)index).Relational().Name);
            Assert.Equal("MyIndex", index.SqlServer().Name);
            Assert.Equal("MyIndex", ((IIndex)index).SqlServer().Name);

            index.SqlServer().Name = "DexKnows";

            Assert.Equal("MyIndex", index.Relational().Name);
            Assert.Equal("MyIndex", ((IIndex)index).Relational().Name);
            Assert.Equal("DexKnows", index.SqlServer().Name);
            Assert.Equal("DexKnows", ((IIndex)index).SqlServer().Name);

            index.SqlServer().Name = null;

            Assert.Equal("MyIndex", index.Relational().Name);
            Assert.Equal("MyIndex", ((IIndex)index).Relational().Name);
            Assert.Equal("MyIndex", index.SqlServer().Name);
            Assert.Equal("MyIndex", ((IIndex)index).SqlServer().Name);
        }

        [Fact]
        public void Can_get_and_set_index_clustering()
        {
            var modelBuilder = new BasicModelBuilder();

            var index = modelBuilder
                .Entity<Customer>()
                .Index(e => e.Id)
                .Metadata;

            Assert.Null(index.SqlServer().IsClustered);
            Assert.Null(((IIndex)index).SqlServer().IsClustered);

            index.SqlServer().IsClustered = true;

            Assert.True(index.SqlServer().IsClustered.Value);
            Assert.True(((IIndex)index).SqlServer().IsClustered.Value);

            index.SqlServer().IsClustered = null;

            Assert.Null(index.SqlServer().IsClustered);
            Assert.Null(((IIndex)index).SqlServer().IsClustered);
        }

        [Fact]
        public void Can_get_and_set_key_clustering()
        {
            var modelBuilder = new BasicModelBuilder();

            var key = modelBuilder
                .Entity<Customer>()
                .Key(e => e.Id)
                .Metadata;

            Assert.Null(key.SqlServer().IsClustered);
            Assert.Null(((IKey)key).SqlServer().IsClustered);

            key.SqlServer().IsClustered = true;

            Assert.True(key.SqlServer().IsClustered.Value);
            Assert.True(((IKey)key).SqlServer().IsClustered.Value);

            key.SqlServer().IsClustered = null;

            Assert.Null(key.SqlServer().IsClustered);
            Assert.Null(((IKey)key).SqlServer().IsClustered);
        }

        [Fact]
        public void Can_get_and_set_sequence()
        {
            var modelBuilder = new BasicModelBuilder();
            var model = modelBuilder.Metadata;

            Assert.Null(model.Relational().TryGetSequence("Foo"));
            Assert.Null(((IModel)model).Relational().TryGetSequence("Foo"));
            Assert.Null(model.SqlServer().TryGetSequence("Foo"));
            Assert.Null(((IModel)model).SqlServer().TryGetSequence("Foo"));

            var sequence = model.SqlServer().GetOrAddSequence("Foo");

            Assert.Null(model.Relational().TryGetSequence("Foo"));
            Assert.Null(((IModel)model).Relational().TryGetSequence("Foo"));
            Assert.Equal("Foo", model.SqlServer().TryGetSequence("Foo").Name);
            Assert.Equal("Foo", ((IModel)model).SqlServer().TryGetSequence("Foo").Name);

            Assert.Equal("Foo", sequence.Name);
            Assert.Null(sequence.Schema);
            Assert.Equal(10, sequence.IncrementBy);
            Assert.Equal(1, sequence.StartValue);
            Assert.Null(sequence.MinValue);
            Assert.Null(sequence.MaxValue);
            Assert.Same(typeof(long), sequence.Type);

            model.SqlServer().AddOrReplaceSequence(new Sequence("Foo", null, 1729, 11, 2001, 2010, typeof(int)));

            Assert.Null(model.Relational().TryGetSequence("Foo"));

            sequence = model.SqlServer().GetOrAddSequence("Foo");

            Assert.Equal("Foo", sequence.Name);
            Assert.Null(sequence.Schema);
            Assert.Equal(11, sequence.IncrementBy);
            Assert.Equal(1729, sequence.StartValue);
            Assert.Equal(2001, sequence.MinValue);
            Assert.Equal(2010, sequence.MaxValue);
            Assert.Same(typeof(int), sequence.Type);
        }

        [Fact]
        public void Can_get_and_set_sequence_with_schema_name()
        {
            var modelBuilder = new BasicModelBuilder();
            var model = modelBuilder.Metadata;

            Assert.Null(model.Relational().TryGetSequence("Foo", "Smoo"));
            Assert.Null(((IModel)model).Relational().TryGetSequence("Foo", "Smoo"));
            Assert.Null(model.SqlServer().TryGetSequence("Foo", "Smoo"));
            Assert.Null(((IModel)model).SqlServer().TryGetSequence("Foo", "Smoo"));

            var sequence = model.SqlServer().GetOrAddSequence("Foo", "Smoo");

            Assert.Null(model.Relational().TryGetSequence("Foo", "Smoo"));
            Assert.Null(((IModel)model).Relational().TryGetSequence("Foo", "Smoo"));
            Assert.Equal("Foo", model.SqlServer().TryGetSequence("Foo", "Smoo").Name);
            Assert.Equal("Foo", ((IModel)model).SqlServer().TryGetSequence("Foo", "Smoo").Name);

            Assert.Equal("Foo", sequence.Name);
            Assert.Equal("Smoo", sequence.Schema);
            Assert.Equal(10, sequence.IncrementBy);
            Assert.Equal(1, sequence.StartValue);
            Assert.Null(sequence.MinValue);
            Assert.Null(sequence.MaxValue);
            Assert.Same(typeof(long), sequence.Type);

            model.SqlServer().AddOrReplaceSequence(new Sequence("Foo", "Smoo", 1729, 11, 2001, 2010, typeof(int)));

            Assert.Null(model.Relational().TryGetSequence("Foo", "Smoo"));

            sequence = model.SqlServer().GetOrAddSequence("Foo", "Smoo");

            Assert.Equal("Foo", sequence.Name);
            Assert.Equal("Smoo", sequence.Schema);
            Assert.Equal(11, sequence.IncrementBy);
            Assert.Equal(1729, sequence.StartValue);
            Assert.Equal(2001, sequence.MinValue);
            Assert.Equal(2010, sequence.MaxValue);
            Assert.Same(typeof(int), sequence.Type);
        }

        [Fact]
        public void Can_add_and_replace_sequence()
        {
            var modelBuilder = new BasicModelBuilder();
            var model = modelBuilder.Metadata;

            model.SqlServer().AddOrReplaceSequence(new Sequence("Foo"));

            Assert.Null(model.Relational().TryGetSequence("Foo"));
            Assert.Null(((IModel)model).Relational().TryGetSequence("Foo"));
            Assert.Equal("Foo", model.SqlServer().TryGetSequence("Foo").Name);
            Assert.Equal("Foo", ((IModel)model).SqlServer().TryGetSequence("Foo").Name);

            var sequence = model.SqlServer().TryGetSequence("Foo");

            Assert.Equal("Foo", sequence.Name);
            Assert.Null(sequence.Schema);
            Assert.Equal(10, sequence.IncrementBy);
            Assert.Equal(1, sequence.StartValue);
            Assert.Null(sequence.MinValue);
            Assert.Null(sequence.MaxValue);
            Assert.Same(typeof(long), sequence.Type);

            model.SqlServer().AddOrReplaceSequence(new Sequence("Foo", null, 1729, 11, 2001, 2010, typeof(int)));

            Assert.Null(model.Relational().TryGetSequence("Foo"));

            sequence = model.SqlServer().TryGetSequence("Foo");

            Assert.Equal("Foo", sequence.Name);
            Assert.Null(sequence.Schema);
            Assert.Equal(11, sequence.IncrementBy);
            Assert.Equal(1729, sequence.StartValue);
            Assert.Equal(2001, sequence.MinValue);
            Assert.Equal(2010, sequence.MaxValue);
            Assert.Same(typeof(int), sequence.Type);
        }

        [Fact]
        public void Can_add_and_replace_sequence_with_schema_name()
        {
            var modelBuilder = new BasicModelBuilder();
            var model = modelBuilder.Metadata;

            model.SqlServer().AddOrReplaceSequence(new Sequence("Foo", "Smoo"));

            Assert.Null(model.Relational().TryGetSequence("Foo", "Smoo"));
            Assert.Null(((IModel)model).Relational().TryGetSequence("Foo", "Smoo"));
            Assert.Equal("Foo", model.SqlServer().TryGetSequence("Foo", "Smoo").Name);
            Assert.Equal("Foo", ((IModel)model).SqlServer().TryGetSequence("Foo", "Smoo").Name);

            var sequence = model.SqlServer().TryGetSequence("Foo", "Smoo");

            Assert.Equal("Foo", sequence.Name);
            Assert.Equal("Smoo", sequence.Schema);
            Assert.Equal(10, sequence.IncrementBy);
            Assert.Equal(1, sequence.StartValue);
            Assert.Null(sequence.MinValue);
            Assert.Null(sequence.MaxValue);
            Assert.Same(typeof(long), sequence.Type);

            model.SqlServer().AddOrReplaceSequence(new Sequence("Foo", "Smoo", 1729, 11, 2001, 2010, typeof(int)));

            Assert.Null(model.Relational().TryGetSequence("Foo", "Smoo"));

            sequence = model.SqlServer().TryGetSequence("Foo", "Smoo");

            Assert.Equal("Foo", sequence.Name);
            Assert.Equal("Smoo", sequence.Schema);
            Assert.Equal(11, sequence.IncrementBy);
            Assert.Equal(1729, sequence.StartValue);
            Assert.Equal(2001, sequence.MinValue);
            Assert.Equal(2010, sequence.MaxValue);
            Assert.Same(typeof(int), sequence.Type);
        }

        [Fact]
        public void Can_get_and_set_value_generation_on_model()
        {
            var modelBuilder = new BasicModelBuilder();
            var model = modelBuilder.Metadata;

            Assert.Null(model.SqlServer().ValueGenerationStrategy);
            Assert.Null(((IModel)model).SqlServer().ValueGenerationStrategy);

            model.SqlServer().ValueGenerationStrategy = SqlServerValueGenerationStrategy.Sequence;

            Assert.Equal(SqlServerValueGenerationStrategy.Sequence, model.SqlServer().ValueGenerationStrategy);
            Assert.Equal(SqlServerValueGenerationStrategy.Sequence, ((IModel)model).SqlServer().ValueGenerationStrategy);

            model.SqlServer().ValueGenerationStrategy = null;

            Assert.Null(model.SqlServer().ValueGenerationStrategy);
            Assert.Null(((IModel)model).SqlServer().ValueGenerationStrategy);
        }

        [Fact]
        public void Can_get_and_set_default_sequence_name_on_model()
        {
            var modelBuilder = new BasicModelBuilder();
            var model = modelBuilder.Metadata;

            Assert.Null(model.SqlServer().DefaultSequenceName);
            Assert.Null(((IModel)model).SqlServer().DefaultSequenceName);

            model.SqlServer().DefaultSequenceName = "Tasty.Snook";

            Assert.Equal("Tasty.Snook", model.SqlServer().DefaultSequenceName);
            Assert.Equal("Tasty.Snook", ((IModel)model).SqlServer().DefaultSequenceName);

            model.SqlServer().DefaultSequenceName = null;

            Assert.Null(model.SqlServer().DefaultSequenceName);
            Assert.Null(((IModel)model).SqlServer().DefaultSequenceName);
        }

        [Fact]
        public void Can_get_and_set_default_sequence_schema_on_model()
        {
            var modelBuilder = new BasicModelBuilder();
            var model = modelBuilder.Metadata;

            Assert.Null(model.SqlServer().DefaultSequenceSchema);
            Assert.Null(((IModel)model).SqlServer().DefaultSequenceSchema);

            model.SqlServer().DefaultSequenceSchema = "Tasty.Snook";

            Assert.Equal("Tasty.Snook", model.SqlServer().DefaultSequenceSchema);
            Assert.Equal("Tasty.Snook", ((IModel)model).SqlServer().DefaultSequenceSchema);

            model.SqlServer().DefaultSequenceSchema = null;

            Assert.Null(model.SqlServer().DefaultSequenceSchema);
            Assert.Null(((IModel)model).SqlServer().DefaultSequenceSchema);
        }

        [Fact]
        public void Can_get_and_set_value_generation_on_property()
        {
            var modelBuilder = new BasicModelBuilder();

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id)
                .Metadata;

            Assert.Null(property.SqlServer().ValueGenerationStrategy);
            Assert.Null(((IProperty)property).SqlServer().ValueGenerationStrategy);
            Assert.Equal(ValueGeneration.None, property.ValueGeneration);

            property.SqlServer().ValueGenerationStrategy = SqlServerValueGenerationStrategy.Sequence;

            Assert.Equal(SqlServerValueGenerationStrategy.Sequence, property.SqlServer().ValueGenerationStrategy);
            Assert.Equal(SqlServerValueGenerationStrategy.Sequence, ((IProperty)property).SqlServer().ValueGenerationStrategy);
            Assert.Equal(ValueGeneration.OnAdd, property.ValueGeneration);

            property.SqlServer().ValueGenerationStrategy = null;

            Assert.Null(property.SqlServer().ValueGenerationStrategy);
            Assert.Null(((IProperty)property).SqlServer().ValueGenerationStrategy);
            Assert.Equal(ValueGeneration.None, property.ValueGeneration);
        }

        [Fact]
        public void Throws_setting_sequence_generation_for_invalid_type()
        {
            var modelBuilder = new BasicModelBuilder();

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.Name)
                .Metadata;

            Assert.Equal(
                Strings.FormatSequenceBadType("Name", typeof(Customer).FullName, "String"),
                Assert.Throws<ArgumentException>(
                    () => property.SqlServer().ValueGenerationStrategy = SqlServerValueGenerationStrategy.Sequence).Message);
        }

        [Fact]
        public void Throws_setting_identity_generation_for_invalid_type()
        {
            var modelBuilder = new BasicModelBuilder();

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.Name)
                .Metadata;

            Assert.Equal(
                Strings.FormatIdentityBadType("Name", typeof(Customer).FullName, "String"),
                Assert.Throws<ArgumentException>(
                    () => property.SqlServer().ValueGenerationStrategy = SqlServerValueGenerationStrategy.Identity).Message);
        }

        [Fact]
        public void Throws_setting_identity_generation_for_byte_property()
        {
            var modelBuilder = new BasicModelBuilder();

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.Byte)
                .Metadata;

            Assert.Equal(
                Strings.FormatIdentityBadType("Byte", typeof(Customer).FullName, "Byte"),
                Assert.Throws<ArgumentException>(
                    () => property.SqlServer().ValueGenerationStrategy = SqlServerValueGenerationStrategy.Identity).Message);
        }

        [Fact]
        public void Can_get_and_set_sequence_name_on_property()
        {
            var modelBuilder = new BasicModelBuilder();

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id)
                .Metadata;

            Assert.Null(property.SqlServer().SequenceName);
            Assert.Null(((IProperty)property).SqlServer().SequenceName);

            property.SqlServer().SequenceName = "Snook";

            Assert.Equal("Snook", property.SqlServer().SequenceName);
            Assert.Equal("Snook", ((IProperty)property).SqlServer().SequenceName);

            property.SqlServer().SequenceName = null;

            Assert.Null(property.SqlServer().SequenceName);
            Assert.Null(((IProperty)property).SqlServer().SequenceName);
        }

        [Fact]
        public void Can_get_and_set_sequence_schema_on_property()
        {
            var modelBuilder = new BasicModelBuilder();

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id)
                .Metadata;

            Assert.Null(property.SqlServer().SequenceSchema);
            Assert.Null(((IProperty)property).SqlServer().SequenceSchema);

            property.SqlServer().SequenceSchema = "Tasty";

            Assert.Equal("Tasty", property.SqlServer().SequenceSchema);
            Assert.Equal("Tasty", ((IProperty)property).SqlServer().SequenceSchema);

            property.SqlServer().SequenceSchema = null;

            Assert.Null(property.SqlServer().SequenceSchema);
            Assert.Null(((IProperty)property).SqlServer().SequenceSchema);
        }

        [Fact]
        public void TryGetSequence_returns_null_if_property_is_not_configured_for_sequence_value_generation()
        {
            var modelBuilder = new BasicModelBuilder();

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id)
                .Metadata;

            modelBuilder.Model.SqlServer().AddOrReplaceSequence(new Sequence("DaneelOlivaw"));

            Assert.Null(property.SqlServer().TryGetSequence());
            Assert.Null(((IProperty)property).SqlServer().TryGetSequence());

            property.SqlServer().SequenceName = "DaneelOlivaw";

            Assert.Null(property.SqlServer().TryGetSequence());
            Assert.Null(((IProperty)property).SqlServer().TryGetSequence());

            modelBuilder.Model.SqlServer().ValueGenerationStrategy = SqlServerValueGenerationStrategy.Identity;

            Assert.Null(property.SqlServer().TryGetSequence());
            Assert.Null(((IProperty)property).SqlServer().TryGetSequence());

            modelBuilder.Model.SqlServer().ValueGenerationStrategy = null;
            property.SqlServer().ValueGenerationStrategy = SqlServerValueGenerationStrategy.Identity;

            Assert.Null(property.SqlServer().TryGetSequence());
            Assert.Null(((IProperty)property).SqlServer().TryGetSequence());
        }

        [Fact]
        public void TryGetSequence_returns_sequence_property_is_marked_for_sequence_generation()
        {
            var modelBuilder = new BasicModelBuilder();

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id)
                .Metadata;

            modelBuilder.Model.SqlServer().AddOrReplaceSequence(new Sequence("DaneelOlivaw"));
            property.SqlServer().SequenceName = "DaneelOlivaw";
            property.SqlServer().ValueGenerationStrategy = SqlServerValueGenerationStrategy.Sequence;

            Assert.Equal("DaneelOlivaw", property.SqlServer().TryGetSequence().Name);
            Assert.Equal("DaneelOlivaw", ((IProperty)property).SqlServer().TryGetSequence().Name);
        }

        [Fact]
        public void TryGetSequence_returns_sequence_model_is_marked_for_sequence_generation()
        {
            var modelBuilder = new BasicModelBuilder();

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id)
                .Metadata;

            modelBuilder.Model.SqlServer().AddOrReplaceSequence(new Sequence("DaneelOlivaw"));
            modelBuilder.Model.SqlServer().ValueGenerationStrategy = SqlServerValueGenerationStrategy.Sequence;
            property.SqlServer().SequenceName = "DaneelOlivaw";

            Assert.Equal("DaneelOlivaw", property.SqlServer().TryGetSequence().Name);
            Assert.Equal("DaneelOlivaw", ((IProperty)property).SqlServer().TryGetSequence().Name);
        }

        [Fact]
        public void TryGetSequence_returns_sequence_property_is_marked_for_sequence_generation_and_model_has_name()
        {
            var modelBuilder = new BasicModelBuilder();

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id)
                .Metadata;

            modelBuilder.Model.SqlServer().AddOrReplaceSequence(new Sequence("DaneelOlivaw"));
            modelBuilder.Model.SqlServer().DefaultSequenceName = "DaneelOlivaw";
            property.SqlServer().ValueGenerationStrategy = SqlServerValueGenerationStrategy.Sequence;

            Assert.Equal("DaneelOlivaw", property.SqlServer().TryGetSequence().Name);
            Assert.Equal("DaneelOlivaw", ((IProperty)property).SqlServer().TryGetSequence().Name);
        }

        [Fact]
        public void TryGetSequence_returns_sequence_model_is_marked_for_sequence_generation_and_model_has_name()
        {
            var modelBuilder = new BasicModelBuilder();

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id)
                .Metadata;

            modelBuilder.Model.SqlServer().AddOrReplaceSequence(new Sequence("DaneelOlivaw"));
            modelBuilder.Model.SqlServer().ValueGenerationStrategy = SqlServerValueGenerationStrategy.Sequence;
            modelBuilder.Model.SqlServer().DefaultSequenceName = "DaneelOlivaw";

            Assert.Equal("DaneelOlivaw", property.SqlServer().TryGetSequence().Name);
            Assert.Equal("DaneelOlivaw", ((IProperty)property).SqlServer().TryGetSequence().Name);
        }

        [Fact]
        public void TryGetSequence_with_schema_returns_sequence_property_is_marked_for_sequence_generation()
        {
            var modelBuilder = new BasicModelBuilder();

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id)
                .Metadata;

            modelBuilder.Model.SqlServer().AddOrReplaceSequence(new Sequence("DaneelOlivaw", "R"));
            property.SqlServer().SequenceName = "DaneelOlivaw";
            property.SqlServer().SequenceSchema = "R";
            property.SqlServer().ValueGenerationStrategy = SqlServerValueGenerationStrategy.Sequence;

            Assert.Equal("DaneelOlivaw", property.SqlServer().TryGetSequence().Name);
            Assert.Equal("DaneelOlivaw", ((IProperty)property).SqlServer().TryGetSequence().Name);
            Assert.Equal("R", property.SqlServer().TryGetSequence().Schema);
            Assert.Equal("R", ((IProperty)property).SqlServer().TryGetSequence().Schema);
        }

        [Fact]
        public void TryGetSequence_with_schema_returns_sequence_model_is_marked_for_sequence_generation()
        {
            var modelBuilder = new BasicModelBuilder();

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id)
                .Metadata;

            modelBuilder.Model.SqlServer().AddOrReplaceSequence(new Sequence("DaneelOlivaw", "R"));
            modelBuilder.Model.SqlServer().ValueGenerationStrategy = SqlServerValueGenerationStrategy.Sequence;
            property.SqlServer().SequenceName = "DaneelOlivaw";
            property.SqlServer().SequenceSchema = "R";

            Assert.Equal("DaneelOlivaw", property.SqlServer().TryGetSequence().Name);
            Assert.Equal("DaneelOlivaw", ((IProperty)property).SqlServer().TryGetSequence().Name);
            Assert.Equal("R", property.SqlServer().TryGetSequence().Schema);
            Assert.Equal("R", ((IProperty)property).SqlServer().TryGetSequence().Schema);
        }

        [Fact]
        public void TryGetSequence_with_schema_returns_sequence_property_is_marked_for_sequence_generation_and_model_has_name()
        {
            var modelBuilder = new BasicModelBuilder();

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id)
                .Metadata;

            modelBuilder.Model.SqlServer().AddOrReplaceSequence(new Sequence("DaneelOlivaw", "R"));
            modelBuilder.Model.SqlServer().DefaultSequenceName = "DaneelOlivaw";
            modelBuilder.Model.SqlServer().DefaultSequenceSchema = "R";
            property.SqlServer().ValueGenerationStrategy = SqlServerValueGenerationStrategy.Sequence;

            Assert.Equal("DaneelOlivaw", property.SqlServer().TryGetSequence().Name);
            Assert.Equal("DaneelOlivaw", ((IProperty)property).SqlServer().TryGetSequence().Name);
            Assert.Equal("R", property.SqlServer().TryGetSequence().Schema);
            Assert.Equal("R", ((IProperty)property).SqlServer().TryGetSequence().Schema);
        }

        [Fact]
        public void TryGetSequence_with_schema_returns_sequence_model_is_marked_for_sequence_generation_and_model_has_name()
        {
            var modelBuilder = new BasicModelBuilder();

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id)
                .Metadata;

            modelBuilder.Model.SqlServer().AddOrReplaceSequence(new Sequence("DaneelOlivaw", "R"));
            modelBuilder.Model.SqlServer().ValueGenerationStrategy = SqlServerValueGenerationStrategy.Sequence;
            modelBuilder.Model.SqlServer().DefaultSequenceName = "DaneelOlivaw";
            modelBuilder.Model.SqlServer().DefaultSequenceSchema = "R";

            Assert.Equal("DaneelOlivaw", property.SqlServer().TryGetSequence().Name);
            Assert.Equal("DaneelOlivaw", ((IProperty)property).SqlServer().TryGetSequence().Name);
            Assert.Equal("R", property.SqlServer().TryGetSequence().Schema);
            Assert.Equal("R", ((IProperty)property).SqlServer().TryGetSequence().Schema);
        }

        private class Customer
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public byte Byte { get; set; }
        }

        private class Order
        {
            public int OrderId { get; set; }
            public int CustomerId { get; set; }
        }
    }
}
