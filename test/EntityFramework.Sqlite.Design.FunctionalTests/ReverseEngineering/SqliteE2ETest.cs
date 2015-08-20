﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.Data.Entity.Relational.Design.FunctionalTests.ReverseEngineering;
using Microsoft.Data.Entity.Relational.Design.ReverseEngineering;
using Microsoft.Data.Entity.Sqlite.Design;
using Microsoft.Data.Entity.Sqlite.Design.ReverseEngineering;
using Microsoft.Data.Entity.Sqlite.FunctionalTests;
using Xunit;
using Xunit.Abstractions;

namespace EntityFramework.Sqlite.Design.FunctionalTests.ReverseEngineering
{
    public class SqliteE2ETest : E2ETestBase
    {
        public SqliteE2ETest(ITestOutputHelper output)
            : base(output)
        {
        }

        [Fact]
        public async void One_to_one()
        {
            using (var testStore = SqliteTestStore.GetOrCreateShared("OneToOne").AsTransient())
            {
                testStore.ExecuteNonQuery(@"
CREATE TABLE IF NOT EXISTS Principal ( 
    Id INTEGER PRIMARY KEY AUTOINCREMENT
);
CREATE TABLE IF NOT EXISTS Dependent (
    Id INT,
    PrincipalId INT NOT NULL UNIQUE,
    PRIMARY KEY (Id),
    FOREIGN KEY (PrincipalId) REFERENCES Principal (Id)
);
");
                testStore.Transaction.Commit();

                var results = await Generator.GenerateAsync(new ReverseEngineeringConfiguration
                    {
                        Provider = MetadataModelProvider,
                        ConnectionString = testStore.Connection.ConnectionString,
                        ProjectPath = "testout",
                        ProjectRootNamespace = "E2E.Sqlite",
                    });

                AssertLog(new LoggerMessages());

                var expectedFileSet = new FileSet(new FileSystemFileService(), "ReverseEngineering/Expected/OneToOne")
                    {
                        Files =
                            {
                                "ModelContext.expected",
                                "Dependent.expected",
                                "Principal.expected"
                            }
                    };
                var actualFileSet = new FileSet(InMemoryFiles, "testout")
                    {
                        Files = results.Select(Path.GetFileName).ToList()
                    };
                AssertEqualFileContents(expectedFileSet, actualFileSet);
                AssertCompile(actualFileSet);
            }
        }

        [Fact]
        public async void One_to_many()
        {
            using (var testStore = SqliteTestStore.GetOrCreateShared("OneToMany").AsTransient())
            {
                testStore.ExecuteNonQuery(@"
CREATE TABLE IF NOT EXISTS OneToManyPrincipal ( 
    OneToManyPrincipalID1 INT,
    OneToManyPrincipalID2 INT,
    Other TEXT NOT NULL,
    PRIMARY KEY (OneToManyPrincipalID1, OneToManyPrincipalID2) 
);
CREATE TABLE IF NOT EXISTS OneToManyDependent (
    OneToManyDependentID1 INT NOT NULL,
    OneToManyDependentID2 INT NOT NULL,
    SomeDependentEndColumn VARCHAR NOT NULL,
    OneToManyDependentFK1 INT,
    OneToManyDependentFK2 INT,
    PRIMARY KEY (OneToManyDependentID1, OneToManyDependentID2),
    FOREIGN KEY ( OneToManyDependentFK1, OneToManyDependentFK2) 
        REFERENCES OneToManyPrincipal ( OneToManyPrincipalID1, OneToManyPrincipalID2  )
);
");
                testStore.Transaction.Commit();

                var results = await Generator.GenerateAsync(new ReverseEngineeringConfiguration
                    {
                        Provider = MetadataModelProvider,
                        ConnectionString = testStore.Connection.ConnectionString,
                        ProjectPath = "testout",
                        ProjectRootNamespace = "E2E.Sqlite",
                    });

                AssertLog(new LoggerMessages());

                var expectedFileSet = new FileSet(new FileSystemFileService(), "ReverseEngineering/Expected/OneToMany")
                    {
                        Files =
                            {
                                "ModelContext.expected",
                                "OneToManyDependent.expected",
                                "OneToManyPrincipal.expected"
                            }
                    };
                var actualFileSet = new FileSet(InMemoryFiles, "testout")
                    {
                        Files = results.Select(Path.GetFileName).ToList()
                    };
                AssertEqualFileContents(expectedFileSet, actualFileSet);
                AssertCompile(actualFileSet);
            }
        }

        [Fact]
        public async void Many_to_many()
        {
            using (var testStore = SqliteTestStore.GetOrCreateShared("ManyToMany").AsTransient())
            {
                testStore.ExecuteNonQuery(@"
CREATE TABLE Users ( Id PRIMARY KEY);
CREATE TABLE Groups (Id PRIMARY KEY);
CREATE TABLE Users_Groups (
    Id PRIMARY KEY,
    UserId, 
    GroupId, 
    UNIQUE (UserId, GroupId), 
    FOREIGN KEY (UserId) REFERENCES Users (Id), 
    FOREIGN KEY (GroupId) REFERENCES Groups (Id)
);
");
                testStore.Transaction.Commit();

                var results = await Generator.GenerateAsync(new ReverseEngineeringConfiguration
                    {
                        Provider = MetadataModelProvider,
                        ConnectionString = testStore.Connection.ConnectionString,
                        ProjectPath = "testout",
                        ProjectRootNamespace = "E2E.Sqlite",
                    });

                AssertLog(new LoggerMessages());

                var expectedFileSet = new FileSet(new FileSystemFileService(), "ReverseEngineering/Expected/ManyToMany")
                    {
                        Files =
                            {
                                "ModelContext.expected",
                                "Groups.expected",
                                "Users.expected",
                                "Users_Groups.expected"
                            }
                    };
                var actualFileSet = new FileSet(InMemoryFiles, "testout")
                    {
                        Files = results.Select(Path.GetFileName).ToList()
                    };
                AssertEqualFileContents(expectedFileSet, actualFileSet);
                AssertCompile(actualFileSet);
            }
        }

        [Fact]
        public async void Self_referencing()
        {
            using (var testStore = SqliteTestStore.GetOrCreateShared("SelfRef").AsTransient())
            {
                testStore.ExecuteNonQuery(@"CREATE TABLE SelfRef (
    Id INTEGER PRIMARY KEY,
    SelfForeignKey INTEGER,
    FOREIGN KEY (SelfForeignKey) REFERENCES SelfRef (Id)
);");
                testStore.Transaction.Commit();

                var results = await Generator.GenerateAsync(new ReverseEngineeringConfiguration
                    {
                        Provider = MetadataModelProvider,
                        ConnectionString = testStore.Connection.ConnectionString,
                        ProjectPath = "testout",
                        ProjectRootNamespace = "E2E.Sqlite",
                    });

                AssertLog(new LoggerMessages());

                var expectedFileSet = new FileSet(new FileSystemFileService(), "ReverseEngineering/Expected/SelfRef")
                    {
                        Files =
                            {
                                "ModelContext.expected",
                                "SelfRef.expected"
                            }
                    };
                var actualFileSet = new FileSet(InMemoryFiles, "testout")
                    {
                        Files = results.Select(Path.GetFileName).ToList()
                    };
                AssertEqualFileContents(expectedFileSet, actualFileSet);
                AssertCompile(actualFileSet);
            }
        }

        [Fact]
        public async void It_handles_unsafe_names()
        {
            using (var testStore = SqliteTestStore.CreateScratch())
            {
                testStore.ExecuteNonQuery(@"
CREATE TABLE 'Named with space' ( Id PRIMARY KEY );
CREATE TABLE '123 Invalid Class Name' ( Id PRIMARY KEY);
CREATE TABLE 'Bad characters `~!@#$%^&*()+=-[];''"",.<>/?|\ ' ( Id PRIMARY KEY);
CREATE TABLE ' Bad columns ' (
    'Space jam' PRIMARY KEY,
    '123 Go`',
    'Bad to the bone. `~!@#$%^&*()+=-[];''"",.<>/?|\ ',
    'Next one is all bad',
    '@#$%^&*()'
);
CREATE TABLE Keywords (
    namespace PRIMARY KEY,
    virtual,
    public,
    class,
    string,
    FOREIGN KEY (class) REFERENCES string (string)
);
CREATE TABLE String (
    string PRIMARY KEY,
    FOREIGN KEY (string) REFERENCES String (string)
);
");

                var results = await Generator.GenerateAsync(new ReverseEngineeringConfiguration
                    {
                        Provider = MetadataModelProvider,
                        ConnectionString = testStore.Connection.ConnectionString,
                        ProjectPath = "testout",
                        ProjectRootNamespace = "E2E.Sqlite",
                    });

                AssertLog(new LoggerMessages());

                var files = new FileSet(InMemoryFiles, "testout")
                    {
                        Files = results.Select(Path.GetFileName).ToList()
                    };
                AssertCompile(files);
            }
        }

        [Fact]
        public async void Missing_primary_key()
        {
            using (var testStore = SqliteTestStore.CreateScratch())
            {
                testStore.ExecuteNonQuery("CREATE TABLE Alicia ( Keys TEXT );");

                var results = await Generator.GenerateAsync(new ReverseEngineeringConfiguration
                    {
                        Provider = MetadataModelProvider,
                        ConnectionString = testStore.Connection.ConnectionString,
                        ProjectPath = "testout",
                        ProjectRootNamespace = "E2E.Sqlite",
                    });
                var errorMessage = Strings.MissingPrimaryKey("Alicia");
                var expectedLog = new LoggerMessages
                    {
                        Warn =
                            {
                                errorMessage
                            }
                    };
                AssertLog(expectedLog);
                Assert.Contains(errorMessage, InMemoryFiles.RetrieveFileContents("testout", "Alicia.cs"));
            }
        }

        [Fact]
        public async void Principal_missing_primary_key()
        {
            using (var testStore = SqliteTestStore.GetOrCreateShared("NoPrincipal").AsTransient())
            {
                testStore.ExecuteNonQuery(@"CREATE TABLE Dependent ( 
    Id PRIMARY KEY,
    PrincipalId INT,
    FOREIGN KEY (PrincipalId) REFERENCES Principal(Id)
);
CREATE TABLE Principal ( Id INT);");
                testStore.Transaction.Commit();

                var results = await Generator.GenerateAsync(new ReverseEngineeringConfiguration
                    {
                        Provider = MetadataModelProvider,
                        ConnectionString = testStore.Connection.ConnectionString,
                        ProjectPath = "testout",
                        ProjectRootNamespace = "E2E.Sqlite",
                    });

                var expectedLog = new LoggerMessages
                    {
                        Warn =
                            {
                                Strings.MissingPrimaryKey("Principal"),
                                Strings.ForeignKeyScaffoldError("Dependent","PrincipalId"),
                            }
                    };
                AssertLog(expectedLog);

                var expectedFileSet = new FileSet(new FileSystemFileService(), "ReverseEngineering/Expected/NoPrincipalPk")
                    {
                        Files =
                            {
                                "ModelContext.expected",
                                "Dependent.expected",
                                "Principal.expected",
                            }
                    };
                var actualFileSet = new FileSet(InMemoryFiles, "testout")
                    {
                        Files = results.Select(Path.GetFileName).ToList()
                    };
                AssertEqualFileContents(expectedFileSet, actualFileSet);
                AssertCompile(actualFileSet);
            }
        }

        [Fact]
        public async void It_uses_templates()
        {
            var dbContextFileName = "EntityFramework.Sqlite.Design." + ReverseEngineeringGenerator.DbContextTemplateFileName;
            var entityTypeFileName = "EntityFramework.Sqlite.Design." + ReverseEngineeringGenerator.EntityTypeTemplateFileName;
            var entityTemplate = "This is an entity type template! (For real)";
            var contextTemplate = "Also a 100% legit template";
            var outputDir = "gen";
            var templatesDir = "templates";

            using (var testStore = SqliteTestStore.CreateScratch())
            {
                testStore.ExecuteNonQuery("CREATE TABLE RealMccoy ( Col1 text PRIMARY KEY); ");

                InMemoryFiles.OutputFile(templatesDir, dbContextFileName, contextTemplate);
                InMemoryFiles.OutputFile(templatesDir, entityTypeFileName, entityTemplate);

                var config = new ReverseEngineeringConfiguration
                    {
                        ConnectionString = testStore.Connection.ConnectionString,
                        Provider = MetadataModelProvider,
                        ProjectPath = outputDir,
                        CustomTemplatePath = templatesDir,
                        ProjectRootNamespace = "Test",
                    };
                var filePaths = await Generator.GenerateAsync(config);

                var expectedLog = new LoggerMessages
                    {
                        Info =
                            {
                                "Using custom template " + Path.Combine(templatesDir, dbContextFileName),
                                "Using custom template " + Path.Combine(templatesDir, entityTypeFileName)
                            }
                    };
                AssertLog(expectedLog);

                Assert.Equal(2, filePaths.Count);

                foreach (var fileName in filePaths.Select(Path.GetFileName))
                {
                    var fileContents = InMemoryFiles.RetrieveFileContents(outputDir, fileName);
                    var contents = fileName.EndsWith("Context.cs") ? contextTemplate : entityTemplate;
                    Assert.Equal(contents, fileContents);
                }
            }
        }

        [Fact]
        public void It_outputs_templates()
        {
            var dbContextFileName = "EntityFramework.Sqlite.Design." + ReverseEngineeringGenerator.DbContextTemplateFileName;
            var entityTypeFileName = "EntityFramework.Sqlite.Design." + ReverseEngineeringGenerator.EntityTypeTemplateFileName;
            var outputDir = "templates/";

            var filePaths = Generator.Customize(MetadataModelProvider, outputDir);

            AssertLog(new LoggerMessages());

            Assert.Collection(filePaths,
                file1 => Assert.Equal(file1, Path.Combine(outputDir, dbContextFileName)),
                file2 => Assert.Equal(file2, Path.Combine(outputDir, entityTypeFileName)));

            var dbContextTemplateContents = InMemoryFiles.RetrieveFileContents(
                outputDir, dbContextFileName);
            Assert.Equal(MetadataModelProvider.DbContextTemplate, dbContextTemplateContents);

            var entityTypeTemplateContents = InMemoryFiles.RetrieveFileContents(
                outputDir, entityTypeFileName);
            Assert.Equal(MetadataModelProvider.EntityTypeTemplate, entityTypeTemplateContents);
        }

        protected override E2ECompiler GetCompiler() => new E2ECompiler
            {
                NamedReferences =
                    {
                        "EntityFramework.Core",
                        "EntityFramework.Relational",
                        "EntityFramework.Sqlite",
                        // ReSharper disable once RedundantCommaInInitializer
                        "Microsoft.Data.Sqlite",
#if DNXCORE50
                        "System.Data.Common",
                        "System.Linq.Expressions",
                        "System.Reflection",
#else
                    },
                References =
                    {
                        MetadataReference.CreateFromFile(
                            Assembly.Load(new AssemblyName(
                                "System.Data, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")).Location)
#endif
                    }
            };

        protected override string ProviderName => "EntityFramework.Sqlite.Design";
        protected override IDesignTimeMetadataProviderFactory GetFactory() => new SqliteDesignTimeMetadataProviderFactory();
    }
}
