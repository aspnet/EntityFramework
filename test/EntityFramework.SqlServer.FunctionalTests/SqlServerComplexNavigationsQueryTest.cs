﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.FunctionalTests;
using Microsoft.Data.Entity.Relational.FunctionalTests;
using Xunit;

namespace Microsoft.Data.Entity.SqlServer.FunctionalTests
{
    public class SqlServerComplexNavigationsQueryTest : ComplexNavigationsQueryTestBase<SqlServerTestStore, SqlServerComplexNavigationsQueryFixture>
    {
        public SqlServerComplexNavigationsQueryTest(SqlServerComplexNavigationsQueryFixture fixture)
            : base(fixture)
        {
        }

        public override void Multi_level_include_one_to_many_optional_and_one_to_many_optional_produces_valid_sql()
        {
            base.Multi_level_include_one_to_many_optional_and_one_to_many_optional_produces_valid_sql();

            Assert.Equal(
@"SELECT [e].[Id], [e].[Level1Id], [e].[Level1Id1], [e].[Name]
FROM [Level1] AS [e]
ORDER BY [e].[Id]

SELECT [l].[Id], [l].[Level1Id], [l].[Level1Id1], [l].[Level1_Optional_Id], [l].[Level1_Required_Id], [l].[Level2Id], [l].[Level2Id1], [l].[Name], [l].[OneToOne_Optional_PK_InverseId]
FROM [Level2] AS [l]
INNER JOIN (
    SELECT DISTINCT [e].[Id]
    FROM [Level1] AS [e]
) AS [e] ON [l].[Level1Id1] = [e].[Id]
ORDER BY [e].[Id], [l].[Id]

SELECT [l].[Id], [l].[Level2Id], [l].[Level2Id1], [l].[Level2_Optional_Id], [l].[Level2_Required_Id], [l].[Level3Id], [l].[Level3Id1], [l].[Name], [l].[OneToOne_Optional_PK_InverseId]
FROM [Level3] AS [l]
INNER JOIN (
    SELECT DISTINCT [e].[Id], [l].[Id] AS [Id0]
    FROM [Level2] AS [l]
    INNER JOIN (
        SELECT DISTINCT [e].[Id]
        FROM [Level1] AS [e]
    ) AS [e] ON [l].[Level1Id1] = [e].[Id]
) AS [l0] ON [l].[Level2Id1] = [l0].[Id0]
ORDER BY [l0].[Id], [l0].[Id0]", Sql);
        }

        public override void Multi_level_include_correct_PK_is_chosen_as_the_join_predicate_for_queries_that_join_same_table_multiple_times()
        {
            base.Multi_level_include_correct_PK_is_chosen_as_the_join_predicate_for_queries_that_join_same_table_multiple_times();

            Assert.Equal(
@"SELECT [e].[Id], [e].[Level1Id], [e].[Level1Id1], [e].[Name]
FROM [Level1] AS [e]
ORDER BY [e].[Id]

SELECT [l].[Id], [l].[Level1Id], [l].[Level1Id1], [l].[Level1_Optional_Id], [l].[Level1_Required_Id], [l].[Level2Id], [l].[Level2Id1], [l].[Name], [l].[OneToOne_Optional_PK_InverseId]
FROM [Level2] AS [l]
INNER JOIN (
    SELECT DISTINCT [e].[Id]
    FROM [Level1] AS [e]
) AS [e] ON [l].[Level1Id1] = [e].[Id]
ORDER BY [e].[Id], [l].[Id]

SELECT [l].[Id], [l].[Level2Id], [l].[Level2Id1], [l].[Level2_Optional_Id], [l].[Level2_Required_Id], [l].[Level3Id], [l].[Level3Id1], [l].[Name], [l].[OneToOne_Optional_PK_InverseId], [l1].[Id], [l1].[Level1Id], [l1].[Level1Id1], [l1].[Level1_Optional_Id], [l1].[Level1_Required_Id], [l1].[Level2Id], [l1].[Level2Id1], [l1].[Name], [l1].[OneToOne_Optional_PK_InverseId]
FROM [Level3] AS [l]
INNER JOIN (
    SELECT DISTINCT [e].[Id], [l].[Id] AS [Id0]
    FROM [Level2] AS [l]
    INNER JOIN (
        SELECT DISTINCT [e].[Id]
        FROM [Level1] AS [e]
    ) AS [e] ON [l].[Level1Id1] = [e].[Id]
) AS [l0] ON [l].[Level2Id1] = [l0].[Id0]
INNER JOIN [Level2] AS [l1] ON [l].[Level2Id] = [l1].[Id]
ORDER BY [l0].[Id], [l0].[Id0], [l1].[Id]

SELECT [l].[Id], [l].[Level2Id], [l].[Level2Id1], [l].[Level2_Optional_Id], [l].[Level2_Required_Id], [l].[Level3Id], [l].[Level3Id1], [l].[Name], [l].[OneToOne_Optional_PK_InverseId]
FROM [Level3] AS [l]
INNER JOIN (
    SELECT DISTINCT [l0].[Id], [l0].[Id0], [l1].[Id] AS [Id1]
    FROM [Level3] AS [l]
    INNER JOIN (
        SELECT DISTINCT [e].[Id], [l].[Id] AS [Id0]
        FROM [Level2] AS [l]
        INNER JOIN (
            SELECT DISTINCT [e].[Id]
            FROM [Level1] AS [e]
        ) AS [e] ON [l].[Level1Id1] = [e].[Id]
    ) AS [l0] ON [l].[Level2Id1] = [l0].[Id0]
    INNER JOIN [Level2] AS [l1] ON [l].[Level2Id] = [l1].[Id]
) AS [l1] ON [l].[Level2Id1] = [l1].[Id1]
ORDER BY [l1].[Id], [l1].[Id0]", Sql);
        }

        private static string Sql
        {
            get { return TestSqlLoggerFactory.Sql; }
        }
    }
}
