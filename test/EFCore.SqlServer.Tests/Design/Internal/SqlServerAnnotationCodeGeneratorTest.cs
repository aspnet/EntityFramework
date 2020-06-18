﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.SqlServer.Design.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Metadata.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Design.Internal
{
    public class SqlServerAnnotationCodeGeneratorTest
    {
        [ConditionalFact]
        public void GenerateFluentApi_IKey_works_when_clustered()
        {
            var generator = CreateGenerator();

            var modelBuilder = new ModelBuilder(SqlServerConventionSetBuilder.Build());
            modelBuilder.Entity(
                "Post",
                x =>
                {
                    x.Property<int>("Id");
                    x.HasKey("Id").IsClustered();
                });
            var key = modelBuilder.Model.FindEntityType("Post").GetKeys().Single();
            var annotation = key.FindAnnotation(SqlServerAnnotationNames.Clustered);

            var result = generator.GenerateFluentApi(key, annotation);

            Assert.Equal("IsClustered", result.Method);

            Assert.Equal(0, result.Arguments.Count);
        }

        [ConditionalFact]
        public void GenerateFluentApi_IKey_works_when_nonclustered()
        {
            var generator = CreateGenerator();
            var modelBuilder = new ModelBuilder(SqlServerConventionSetBuilder.Build());
            modelBuilder.Entity(
                "Post",
                x =>
                {
                    x.Property<int>("Id");
                    x.HasKey("Id").IsClustered(false);
                });
            var key = modelBuilder.Model.FindEntityType("Post").GetKeys().Single();
            var annotation = key.FindAnnotation(SqlServerAnnotationNames.Clustered);

            var result = generator.GenerateFluentApi(key, annotation);

            Assert.Equal("IsClustered", result.Method);

            Assert.Equal(1, result.Arguments.Count);
            Assert.Equal(false, result.Arguments[0]);
        }

        [ConditionalFact]
        public void GenerateFluentApi_IIndex_works_when_clustered()
        {
            var generator = CreateGenerator();
            var modelBuilder = new ModelBuilder(SqlServerConventionSetBuilder.Build());
            modelBuilder.Entity(
                "Post",
                x =>
                {
                    x.Property<int>("Id");
                    x.Property<string>("Name");
                    x.HasIndex("Name").IsClustered();
                });
            var index = modelBuilder.Model.FindEntityType("Post").GetIndexes().Single();
            var annotation = index.FindAnnotation(SqlServerAnnotationNames.Clustered);

            var result = generator.GenerateFluentApi(index, annotation);

            Assert.Equal("IsClustered", result.Method);

            Assert.Equal(0, result.Arguments.Count);
        }

        [ConditionalFact]
        public void GenerateFluentApi_IIndex_works_when_nonclustered()
        {
            var generator = CreateGenerator();
            var modelBuilder = new ModelBuilder(SqlServerConventionSetBuilder.Build());
            modelBuilder.Entity(
                "Post",
                x =>
                {
                    x.Property<int>("Id");
                    x.Property<string>("Name");
                    x.HasIndex("Name").IsClustered(false);
                });
            var index = modelBuilder.Model.FindEntityType("Post").GetIndexes().Single();
            var annotation = index.FindAnnotation(SqlServerAnnotationNames.Clustered);

            var result = generator.GenerateFluentApi(index, annotation);

            Assert.Equal("IsClustered", result.Method);

            Assert.Equal(1, result.Arguments.Count);
            Assert.Equal(false, result.Arguments[0]);
        }

        [ConditionalFact]
        public void GenerateFluentApi_IIndex_works_with_fillfactor()
        {
            var generator = CreateGenerator();
            var modelBuilder = new ModelBuilder(SqlServerConventionSetBuilder.Build());
            modelBuilder.Entity(
                "Post",
                x =>
                {
                    x.Property<int>("Id");
                    x.Property<string>("Name");
                    x.HasIndex("Name").HasFillFactor(90);
                });

            var index = modelBuilder.Model.FindEntityType("Post").GetIndexes().Single();
            var annotation = index.FindAnnotation(SqlServerAnnotationNames.FillFactor);
            var result = generator.GenerateFluentApi(index, annotation);

            Assert.Equal("HasFillFactor", result.Method);
            Assert.Equal(1, result.Arguments.Count);
            Assert.Equal(90, result.Arguments[0]);
        }

        [ConditionalFact]
        public void GenerateFluentApi_IIndex_works_with_includes()
        {
            var generator = CreateGenerator();
            var modelBuilder = new ModelBuilder(SqlServerConventionSetBuilder.Build());
            modelBuilder.Entity(
                "Post",
                x =>
                {
                    x.Property<int>("Id");
                    x.Property<string>("FirstName");
                    x.Property<string>("LastName");
                    x.HasIndex("LastName").IncludeProperties("FirstName");
                });
            var index = modelBuilder.Model.FindEntityType("Post").GetIndexes().Single();
            var annotation = index.FindAnnotation(SqlServerAnnotationNames.Include);

            var result = generator.GenerateFluentApi(index, annotation);

            Assert.Equal("IncludeProperties", result.Method);

            Assert.Equal(1, result.Arguments.Count);
            var properties = Assert.IsType<string[]>(result.Arguments[0]);
            Assert.Equal(new[] { "FirstName" }, properties.AsEnumerable());
        }

        [ConditionalFact]
        public void GenerateFluentApi_IModel_works_with_identity()
        {
            var generator = CreateGenerator();
            var modelBuilder = new ModelBuilder(SqlServerConventionSetBuilder.Build());
            modelBuilder.UseIdentityColumns(seed: 5, increment: 10);

            var annotations = modelBuilder.Model.GetAnnotations().ToDictionary(a => a.Name, a => a);
            var result = generator.GenerateFluentApiCalls(modelBuilder.Model, annotations).Single();

            Assert.Equal("UseIdentityColumns", result.Method);

            Assert.Collection(result.Arguments,
                seed => Assert.Equal(5, seed),
                increment => Assert.Equal(10, increment));
        }

        [ConditionalFact]
        public void GenerateFluentApi_IProperty_works_with_identity()
        {
            var generator = CreateGenerator();
            var modelBuilder = new ModelBuilder(SqlServerConventionSetBuilder.Build());
            modelBuilder.Entity("Post", x => x.Property<int>("Id").UseIdentityColumn(5, 10));
            var property = modelBuilder.Model.FindEntityType("Post").FindProperty("Id");

            var annotations = property.GetAnnotations().ToDictionary(a => a.Name, a => a);
            var result = generator.GenerateFluentApiCalls(property, annotations).Single();

            Assert.Equal("UseIdentityColumn", result.Method);

            Assert.Collection(result.Arguments,
                seed => Assert.Equal(5, seed),
                increment => Assert.Equal(10, increment));
        }

        [ConditionalFact]
        public void GenerateFluentApi_IModel_works_with_HiLo()
        {
            var generator = CreateGenerator();
            var modelBuilder = new ModelBuilder(SqlServerConventionSetBuilder.Build());
            modelBuilder.UseHiLo("HiLoIndexName", "HiLoIndexSchema");

            var annotations = modelBuilder.Model.GetAnnotations().ToDictionary(a => a.Name, a => a);
            var result = generator.GenerateFluentApiCalls(modelBuilder.Model, annotations).Single();

            Assert.Equal("UseHiLo", result.Method);

            Assert.Collection(result.Arguments,
                name => Assert.Equal("HiLoIndexName", name),
                schema => Assert.Equal("HiLoIndexSchema", schema));
        }

        [ConditionalFact]
        public void GenerateFluentApi_IProperty_works_with_HiLo()
        {
            var generator = CreateGenerator();
            var modelBuilder = new ModelBuilder(SqlServerConventionSetBuilder.Build());
            modelBuilder.Entity("Post", x => x.Property<int>("Id").UseHiLo("HiLoIndexName", "HiLoIndexSchema"));
            var property = modelBuilder.Model.FindEntityType("Post").FindProperty("Id");

            var annotations = property.GetAnnotations().ToDictionary(a => a.Name, a => a);
            var result = generator.GenerateFluentApiCalls(property, annotations).Single();

            Assert.Equal("UseHiLo", result.Method);

            Assert.Collection(result.Arguments,
                name => Assert.Equal("HiLoIndexName", name),
                schema => Assert.Equal("HiLoIndexSchema", schema));
        }

        private SqlServerAnnotationCodeGenerator CreateGenerator()
            => new SqlServerAnnotationCodeGenerator(
                new AnnotationCodeGeneratorDependencies(
                    new SqlServerTypeMappingSource(
                        new TypeMappingSourceDependencies(
                            new ValueConverterSelector(
                                new ValueConverterSelectorDependencies()),
                            Array.Empty<ITypeMappingSourcePlugin>()),
                        new RelationalTypeMappingSourceDependencies(
                            Array.Empty<IRelationalTypeMappingSourcePlugin>()))));
    }
}
