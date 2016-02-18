// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.FunctionalTests;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests
{
    public class GearsOfWarQuerySqlServerTest : GearsOfWarQueryTestBase<SqlServerTestStore, GearsOfWarQuerySqlServerFixture>
    {
        public override void Include_multiple_one_to_one_and_one_to_many()
        {
            base.Include_multiple_one_to_one_and_one_to_many();

            Assert.Equal(
                @"SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[Note], [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [CogTag] AS [t]
LEFT JOIN (
    SELECT [g].*
    FROM [Gear] AS [g]
    WHERE ([g].[Discriminator] = 'Officer') OR ([g].[Discriminator] = 'Gear')
) AS [g] ON ([t].[GearNickName] = [g].[Nickname]) AND ([t].[GearSquadId] = [g].[SquadId])
ORDER BY [g].[FullName]

SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [g].[FullName], [w].[SynergyWithId]
FROM [CogTag] AS [t]
LEFT JOIN (
    SELECT [g].*
    FROM [Gear] AS [g]
    WHERE ([g].[Discriminator] = 'Officer') OR ([g].[Discriminator] = 'Gear')
) AS [g] ON ([t].[GearNickName] = [g].[Nickname]) AND ([t].[GearSquadId] = [g].[SquadId])
LEFT JOIN [Weapon] AS [w] ON [w].[OwnerFullName] = [g].[FullName]
WHERE [g].[FullName] IS NOT NULL
ORDER BY [g].[FullName]",
                Sql);
        }

        public override void Include_multiple_one_to_one_and_one_to_many_self_reference()
        {
            base.Include_multiple_one_to_one_and_one_to_many_self_reference();

            Assert.Equal(
                @"SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[Note], [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [CogTag] AS [t]
LEFT JOIN (
    SELECT [g].*
    FROM [Gear] AS [g]
    WHERE ([g].[Discriminator] = 'Officer') OR ([g].[Discriminator] = 'Gear')
) AS [g] ON ([t].[GearNickName] = [g].[Nickname]) AND ([t].[GearSquadId] = [g].[SquadId])
ORDER BY [g].[FullName]

SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [g].[FullName], [w].[SynergyWithId]
FROM [CogTag] AS [t]
LEFT JOIN (
    SELECT [g].*
    FROM [Gear] AS [g]
    WHERE ([g].[Discriminator] = 'Officer') OR ([g].[Discriminator] = 'Gear')
) AS [g] ON ([t].[GearNickName] = [g].[Nickname]) AND ([t].[GearSquadId] = [g].[SquadId])
LEFT JOIN [Weapon] AS [w] ON [w].[OwnerFullName] = [g].[FullName]
WHERE [g].[FullName] IS NOT NULL
ORDER BY [g].[FullName]",
                Sql);
        }

        public override void Include_multiple_one_to_one_and_one_to_one_and_one_to_many()
        {
            base.Include_multiple_one_to_one_and_one_to_one_and_one_to_many();

            Assert.Equal(
                @"SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[Note], [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [s].[Id], [s].[InternalNumber], [s].[Name]
FROM [CogTag] AS [t]
LEFT JOIN (
    SELECT [g].*
    FROM [Gear] AS [g]
    WHERE ([g].[Discriminator] = 'Officer') OR ([g].[Discriminator] = 'Gear')
) AS [g] ON ([t].[GearNickName] = [g].[Nickname]) AND ([t].[GearSquadId] = [g].[SquadId])
LEFT JOIN [Squad] AS [s] ON [g].[SquadId] = [s].[Id]
ORDER BY [s].[Id]

SELECT [g].[Nickname], [s].[Id], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [CogTag] AS [t]
LEFT JOIN (
    SELECT [g].*
    FROM [Gear] AS [g]
    WHERE ([g].[Discriminator] = 'Officer') OR ([g].[Discriminator] = 'Gear')
) AS [g] ON ([t].[GearNickName] = [g].[Nickname]) AND ([t].[GearSquadId] = [g].[SquadId])
LEFT JOIN [Squad] AS [s] ON [g].[SquadId] = [s].[Id]
LEFT JOIN (
    SELECT [g].*
    FROM [Gear] AS [g]
    WHERE ([g].[Discriminator] = 'Officer') OR ([g].[Discriminator] = 'Gear')
) AS [g0] ON [g0].[SquadId] = [s].[Id]
WHERE [s].[Id] IS NOT NULL
ORDER BY [s].[Id]", 
                Sql);
        }

        public override void Include_multiple_one_to_one_optional_and_one_to_one_required()
        {
            base.Include_multiple_one_to_one_optional_and_one_to_one_required();

            Assert.Equal(
                @"SELECT [t].[Id], [t].[GearNickName], [t].[GearSquadId], [t].[Note], [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [s].[Id], [s].[InternalNumber], [s].[Name]
FROM [CogTag] AS [t]
LEFT JOIN (
    SELECT [g].*
    FROM [Gear] AS [g]
    WHERE ([g].[Discriminator] = 'Officer') OR ([g].[Discriminator] = 'Gear')
) AS [g] ON ([t].[GearNickName] = [g].[Nickname]) AND ([t].[GearSquadId] = [g].[SquadId])
LEFT JOIN [Squad] AS [s] ON [g].[SquadId] = [s].[Id]", 
                Sql);
        }

        public override void Include_multiple_circular()
        {
            base.Include_multiple_circular();

            Assert.Equal(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [c].[Name], [c].[Location]
FROM [Gear] AS [g]
INNER JOIN [City] AS [c] ON [g].[CityOrBirthName] = [c].[Name]
WHERE [g].[Discriminator] IN ('Officer', 'Gear')
ORDER BY [c].[Name]

SELECT [g].[Nickname], [g].[SquadId], [t0].[Name], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM (
    SELECT [c].[Name]
    FROM [Gear] AS [g]
    INNER JOIN [City] AS [c] ON [g].[CityOrBirthName] = [c].[Name]
    WHERE [g].[Discriminator] IN ('Officer', 'Gear')
) AS [t0]
LEFT JOIN (
    SELECT [g].*
    FROM [Gear] AS [g]
    WHERE ([g].[Discriminator] = 'Officer') OR ([g].[Discriminator] = 'Gear')
) AS [g] ON [g].[AssignedCityName] = [t0].[Name]
WHERE [t0].[Name] IS NOT NULL
ORDER BY [t0].[Name]",
                Sql);
        }

        public override void Include_multiple_circular_with_filter()
        {
            base.Include_multiple_circular_with_filter();

            Assert.Equal(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [c].[Name], [c].[Location]
FROM [Gear] AS [g]
INNER JOIN [City] AS [c] ON [g].[CityOrBirthName] = [c].[Name]
WHERE [g].[Discriminator] IN ('Officer', 'Gear') AND ([g].[Nickname] = 'Marcus')
ORDER BY [c].[Name]

SELECT [g].[Nickname], [g].[SquadId], [t0].[Name], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM (
    SELECT [c].[Name]
    FROM [Gear] AS [g]
    INNER JOIN [City] AS [c] ON [g].[CityOrBirthName] = [c].[Name]
    WHERE [g].[Discriminator] IN ('Officer', 'Gear') AND ([g].[Nickname] = 'Marcus')
) AS [t0]
LEFT JOIN (
    SELECT [g].*
    FROM [Gear] AS [g]
    WHERE ([g].[Discriminator] = 'Officer') OR ([g].[Discriminator] = 'Gear')
) AS [g] ON [g].[AssignedCityName] = [t0].[Name]
WHERE [t0].[Name] IS NOT NULL
ORDER BY [t0].[Name]",
                Sql);
        }

        public override void Include_using_alternate_key()
        {
            base.Include_using_alternate_key();

            Assert.Equal(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gear] AS [g]
WHERE [g].[Discriminator] IN ('Officer', 'Gear') AND ([g].[Nickname] = 'Marcus')
ORDER BY [g].[FullName]

SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [t0].[FullName], [w].[SynergyWithId]
FROM (
    SELECT [g].[FullName]
    FROM [Gear] AS [g]
    WHERE [g].[Discriminator] IN ('Officer', 'Gear') AND ([g].[Nickname] = 'Marcus')
) AS [t0]
LEFT JOIN [Weapon] AS [w] ON [w].[OwnerFullName] = [t0].[FullName]
WHERE [t0].[FullName] IS NOT NULL
ORDER BY [t0].[FullName]",
                Sql);
        }

        public override void Include_multiple_include_then_include()
        {
            base.Include_multiple_include_then_include();

            Assert.Equal(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [c].[Name], [c].[Location], [c0].[Name], [c0].[Location], [c1].[Name], [c1].[Location], [c2].[Name], [c2].[Location]
FROM [Gear] AS [g]
LEFT JOIN [City] AS [c] ON [g].[AssignedCityName] = [c].[Name]
LEFT JOIN [City] AS [c0] ON [g].[AssignedCityName] = [c0].[Name]
INNER JOIN [City] AS [c1] ON [g].[CityOrBirthName] = [c1].[Name]
INNER JOIN [City] AS [c2] ON [g].[CityOrBirthName] = [c2].[Name]
WHERE [g].[Discriminator] IN ('Officer', 'Gear')
ORDER BY [g].[Nickname], [c].[Name], [c0].[Name], [c1].[Name], [c2].[Name]

SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [t0].[Name], [g].[Discriminator], [g].[FullName], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [c].[Id], [c].[GearNickName], [c].[GearSquadId], [c].[Note]
FROM (
    SELECT [g].[Nickname], [c].[Name]
    FROM [Gear] AS [g]
    LEFT JOIN [City] AS [c] ON [g].[AssignedCityName] = [c].[Name]
    WHERE [g].[Discriminator] IN ('Officer', 'Gear')
) AS [t0]
LEFT JOIN (
    SELECT [g].*
    FROM [Gear] AS [g]
    WHERE ([g].[Discriminator] = 'Officer') OR ([g].[Discriminator] = 'Gear')
) AS [g] ON [g].[CityOrBirthName] = [t0].[Name]
LEFT JOIN [CogTag] AS [c] ON ([c].[GearNickName] = [g].[Nickname]) AND ([c].[GearSquadId] = [g].[SquadId])
WHERE [t0].[Name] IS NOT NULL
ORDER BY [t0].[Nickname], [t0].[Name]

SELECT [g].[Nickname], [g].[SquadId], [t0].[Name0], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [c].[Id], [c].[GearNickName], [c].[GearSquadId], [c].[Note]
FROM (
    SELECT [g].[Nickname], [c].[Name], [c0].[Name] AS [Name0]
    FROM [Gear] AS [g]
    LEFT JOIN [City] AS [c] ON [g].[AssignedCityName] = [c].[Name]
    LEFT JOIN [City] AS [c0] ON [g].[AssignedCityName] = [c0].[Name]
    WHERE [g].[Discriminator] IN ('Officer', 'Gear')
) AS [t0]
LEFT JOIN (
    SELECT [g].*
    FROM [Gear] AS [g]
    WHERE ([g].[Discriminator] = 'Officer') OR ([g].[Discriminator] = 'Gear')
) AS [g] ON [g].[AssignedCityName] = [t0].[Name0]
LEFT JOIN [CogTag] AS [c] ON ([c].[GearNickName] = [g].[Nickname]) AND ([c].[GearSquadId] = [g].[SquadId])
WHERE [t0].[Name0] IS NOT NULL
ORDER BY [t0].[Nickname], [t0].[Name], [t0].[Name0]

SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [t0].[Name1], [g].[Discriminator], [g].[FullName], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [c].[Id], [c].[GearNickName], [c].[GearSquadId], [c].[Note]
FROM (
    SELECT [g].[Nickname], [c].[Name], [c0].[Name] AS [Name0], [c1].[Name] AS [Name1]
    FROM [Gear] AS [g]
    LEFT JOIN [City] AS [c] ON [g].[AssignedCityName] = [c].[Name]
    LEFT JOIN [City] AS [c0] ON [g].[AssignedCityName] = [c0].[Name]
    INNER JOIN [City] AS [c1] ON [g].[CityOrBirthName] = [c1].[Name]
    WHERE [g].[Discriminator] IN ('Officer', 'Gear')
) AS [t0]
LEFT JOIN (
    SELECT [g].*
    FROM [Gear] AS [g]
    WHERE ([g].[Discriminator] = 'Officer') OR ([g].[Discriminator] = 'Gear')
) AS [g] ON [g].[CityOrBirthName] = [t0].[Name1]
LEFT JOIN [CogTag] AS [c] ON ([c].[GearNickName] = [g].[Nickname]) AND ([c].[GearSquadId] = [g].[SquadId])
WHERE [t0].[Name1] IS NOT NULL
ORDER BY [t0].[Nickname], [t0].[Name], [t0].[Name0], [t0].[Name1]

SELECT [g].[Nickname], [g].[SquadId], [t0].[Name2], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [c].[Id], [c].[GearNickName], [c].[GearSquadId], [c].[Note]
FROM (
    SELECT [g].[Nickname], [c].[Name], [c0].[Name] AS [Name0], [c1].[Name] AS [Name1], [c2].[Name] AS [Name2]
    FROM [Gear] AS [g]
    LEFT JOIN [City] AS [c] ON [g].[AssignedCityName] = [c].[Name]
    LEFT JOIN [City] AS [c0] ON [g].[AssignedCityName] = [c0].[Name]
    INNER JOIN [City] AS [c1] ON [g].[CityOrBirthName] = [c1].[Name]
    INNER JOIN [City] AS [c2] ON [g].[CityOrBirthName] = [c2].[Name]
    WHERE [g].[Discriminator] IN ('Officer', 'Gear')
) AS [t0]
LEFT JOIN (
    SELECT [g].*
    FROM [Gear] AS [g]
    WHERE ([g].[Discriminator] = 'Officer') OR ([g].[Discriminator] = 'Gear')
) AS [g] ON [g].[AssignedCityName] = [t0].[Name2]
LEFT JOIN [CogTag] AS [c] ON ([c].[GearNickName] = [g].[Nickname]) AND ([c].[GearSquadId] = [g].[SquadId])
WHERE [t0].[Name2] IS NOT NULL
ORDER BY [t0].[Nickname], [t0].[Name], [t0].[Name0], [t0].[Name1], [t0].[Name2]",
                Sql);
        }

        public override void Include_navigation_on_derived_type()
        {
            base.Include_navigation_on_derived_type();

            Assert.Equal(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gear] AS [g]
WHERE [g].[Discriminator] = 'Officer'
ORDER BY [g].[Nickname], [g].[SquadId]

SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [t0].[Nickname], [t0].[SquadId], [g].[Rank]
FROM (
    SELECT [g].[Nickname], [g].[SquadId]
    FROM [Gear] AS [g]
    WHERE [g].[Discriminator] = 'Officer'
) AS [t0]
LEFT JOIN (
    SELECT [g].*
    FROM [Gear] AS [g]
    WHERE ([g].[Discriminator] = 'Officer') OR ([g].[Discriminator] = 'Gear')
) AS [g] ON ([g].[LeaderNickname] = [t0].[Nickname]) AND ([g].[LeaderSquadId] = [t0].[SquadId])
WHERE [t0].[Nickname] IS NOT NULL AND [t0].[SquadId] IS NOT NULL
ORDER BY [t0].[Nickname], [t0].[SquadId]",
                Sql);
        }

        public override void Select_Where_Navigation_Included()
        {
            base.Select_Where_Navigation_Included();

            Assert.Equal(
                @"SELECT [o].[Id], [o].[GearNickName], [o].[GearSquadId], [o].[Note], [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [CogTag] AS [o]
INNER JOIN [Gear] AS [o.Gear] ON ([o].[GearNickName] = [o.Gear].[Nickname]) AND ([o].[GearSquadId] = [o.Gear].[SquadId])
LEFT JOIN (
    SELECT [g].*
    FROM [Gear] AS [g]
    WHERE ([g].[Discriminator] = 'Officer') OR ([g].[Discriminator] = 'Gear')
) AS [g] ON ([o].[GearNickName] = [g].[Nickname]) AND ([o].[GearSquadId] = [g].[SquadId])
WHERE [o.Gear].[Nickname] = 'Marcus'",
                Sql);
        }

        public override void Include_with_join_reference1()
        {
            base.Include_with_join_reference1();

            Assert.Equal(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [c].[Name], [c].[Location]
FROM [Gear] AS [g]
INNER JOIN [CogTag] AS [t] ON ([g].[SquadId] = [t].[GearSquadId]) AND ([g].[Nickname] = [t].[GearNickName])
INNER JOIN [City] AS [c] ON [g].[CityOrBirthName] = [c].[Name]
WHERE [g].[Discriminator] IN ('Officer', 'Gear')",
                Sql);
        }

        public override void Include_with_join_reference2()
        {
            base.Include_with_join_reference2();

            Assert.Equal(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [c].[Name], [c].[Location]
FROM [CogTag] AS [t]
INNER JOIN [Gear] AS [g] ON ([t].[GearSquadId] = [g].[SquadId]) AND ([t].[GearNickName] = [g].[Nickname])
INNER JOIN [City] AS [c] ON [g].[CityOrBirthName] = [c].[Name]",
                Sql);
        }

        public override void Include_with_join_collection1()
        {
            base.Include_with_join_collection1();

            Assert.Equal(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gear] AS [g]
INNER JOIN [CogTag] AS [t] ON ([g].[SquadId] = [t].[GearSquadId]) AND ([g].[Nickname] = [t].[GearNickName])
WHERE [g].[Discriminator] IN ('Officer', 'Gear')
ORDER BY [g].[FullName]

SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [t0].[FullName], [w].[SynergyWithId]
FROM (
    SELECT [g].[FullName]
    FROM [Gear] AS [g]
    INNER JOIN [CogTag] AS [t] ON ([g].[SquadId] = [t].[GearSquadId]) AND ([g].[Nickname] = [t].[GearNickName])
    WHERE [g].[Discriminator] IN ('Officer', 'Gear')
) AS [t0]
LEFT JOIN [Weapon] AS [w] ON [w].[OwnerFullName] = [t0].[FullName]
WHERE [t0].[FullName] IS NOT NULL
ORDER BY [t0].[FullName]",
                Sql);
        }

        public override void Include_with_join_collection2()
        {
            base.Include_with_join_collection2();

            Assert.Equal(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [CogTag] AS [t]
INNER JOIN [Gear] AS [g] ON ([t].[GearSquadId] = [g].[SquadId]) AND ([t].[GearNickName] = [g].[Nickname])
ORDER BY [g].[FullName]

SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [g].[FullName], [w].[SynergyWithId]
FROM [CogTag] AS [t]
INNER JOIN [Gear] AS [g] ON ([t].[GearSquadId] = [g].[SquadId]) AND ([t].[GearNickName] = [g].[Nickname])
LEFT JOIN [Weapon] AS [w] ON [w].[OwnerFullName] = [g].[FullName]
WHERE [g].[FullName] IS NOT NULL
ORDER BY [g].[FullName]",
                Sql);
        }

        public override void Include_with_join_multi_level()
        {
            base.Include_with_join_multi_level();

            Assert.Equal(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank], [c].[Name], [c].[Location]
FROM [Gear] AS [g]
INNER JOIN [CogTag] AS [t] ON ([g].[SquadId] = [t].[GearSquadId]) AND ([g].[Nickname] = [t].[GearNickName])
INNER JOIN [City] AS [c] ON [g].[CityOrBirthName] = [c].[Name]
WHERE [g].[Discriminator] IN ('Officer', 'Gear')
ORDER BY [c].[Name]

SELECT [g].[Nickname], [g].[SquadId], [t0].[Name], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM (
    SELECT [c].[Name]
    FROM [Gear] AS [g]
    INNER JOIN [CogTag] AS [t] ON ([g].[SquadId] = [t].[GearSquadId]) AND ([g].[Nickname] = [t].[GearNickName])
    INNER JOIN [City] AS [c] ON [g].[CityOrBirthName] = [c].[Name]
    WHERE [g].[Discriminator] IN ('Officer', 'Gear')
) AS [t0]
LEFT JOIN (
    SELECT [g].*
    FROM [Gear] AS [g]
    WHERE ([g].[Discriminator] = 'Officer') OR ([g].[Discriminator] = 'Gear')
) AS [g] ON [g].[AssignedCityName] = [t0].[Name]
WHERE [t0].[Name] IS NOT NULL
ORDER BY [t0].[Name]",
                Sql);
        }

        public override void Include_with_join_and_inheritance1()
        {
            base.Include_with_join_and_inheritance1();

            Assert.Equal(
                @"SELECT [t0].[Nickname], [t0].[SquadId], [t0].[AssignedCityName], [t0].[CityOrBirthName], [t0].[Discriminator], [t0].[FullName], [t0].[LeaderNickname], [t0].[LeaderSquadId], [t0].[Rank], [c].[Name], [c].[Location]
FROM [CogTag] AS [t]
INNER JOIN (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
    FROM [Gear] AS [g]
    WHERE [g].[Discriminator] = 'Officer'
) AS [t0] ON ([t].[GearSquadId] = [t0].[SquadId]) AND ([t].[GearNickName] = [t0].[Nickname])
INNER JOIN [City] AS [c] ON [t0].[CityOrBirthName] = [c].[Name]",
                Sql);
        }

        public override void Include_with_join_and_inheritance2()
        {
            base.Include_with_join_and_inheritance2();

            Assert.Equal(
                @"SELECT [t0].[Nickname], [t0].[SquadId], [t0].[AssignedCityName], [t0].[CityOrBirthName], [t0].[Discriminator], [t0].[FullName], [t0].[LeaderNickname], [t0].[LeaderSquadId], [t0].[Rank]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
    FROM [Gear] AS [g]
    WHERE [g].[Discriminator] = 'Officer'
) AS [t0]
INNER JOIN [CogTag] AS [t] ON ([t0].[SquadId] = [t].[GearSquadId]) AND ([t0].[Nickname] = [t].[GearNickName])
ORDER BY [t0].[FullName]

SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [t0].[FullName], [w].[SynergyWithId]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
    FROM [Gear] AS [g]
    WHERE [g].[Discriminator] = 'Officer'
) AS [t0]
INNER JOIN [CogTag] AS [t] ON ([t0].[SquadId] = [t].[GearSquadId]) AND ([t0].[Nickname] = [t].[GearNickName])
LEFT JOIN [Weapon] AS [w] ON [w].[OwnerFullName] = [t0].[FullName]
WHERE [t0].[FullName] IS NOT NULL
ORDER BY [t0].[FullName]",
                Sql);
        }

        public override void Include_with_join_and_inheritance3()
        {
            base.Include_with_join_and_inheritance3();

            Assert.Equal(
                @"SELECT [t0].[Nickname], [t0].[SquadId], [t0].[AssignedCityName], [t0].[CityOrBirthName], [t0].[Discriminator], [t0].[FullName], [t0].[LeaderNickname], [t0].[LeaderSquadId], [t0].[Rank]
FROM [CogTag] AS [t]
INNER JOIN (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
    FROM [Gear] AS [g]
    WHERE [g].[Discriminator] = 'Officer'
) AS [t0] ON ([t].[GearSquadId] = [t0].[SquadId]) AND ([t].[GearNickName] = [t0].[Nickname])
ORDER BY [t0].[Nickname], [t0].[SquadId]

SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [t0].[Nickname], [t0].[SquadId], [g].[Rank]
FROM [CogTag] AS [t]
INNER JOIN (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
    FROM [Gear] AS [g]
    WHERE [g].[Discriminator] = 'Officer'
) AS [t0] ON ([t].[GearSquadId] = [t0].[SquadId]) AND ([t].[GearNickName] = [t0].[Nickname])
LEFT JOIN (
    SELECT [g].*
    FROM [Gear] AS [g]
    WHERE ([g].[Discriminator] = 'Officer') OR ([g].[Discriminator] = 'Gear')
) AS [g] ON ([g].[LeaderNickname] = [t0].[Nickname]) AND ([g].[LeaderSquadId] = [t0].[SquadId])
WHERE [t0].[Nickname] IS NOT NULL AND [t0].[SquadId] IS NOT NULL
ORDER BY [t0].[Nickname], [t0].[SquadId]",
                Sql);
        }

        public override void Include_with_nested_navigation_in_order_by()
        {
            base.Include_with_nested_navigation_in_order_by();

            Assert.Equal(
                @"SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId], [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Weapon] AS [w]
INNER JOIN [Gear] AS [w.Owner] ON [w].[OwnerFullName] = [w.Owner].[FullName]
INNER JOIN [City] AS [w.Owner.CityOfBirth] ON [w.Owner].[CityOrBirthName] = [w.Owner.CityOfBirth].[Name]
LEFT JOIN (
    SELECT [g].*
    FROM [Gear] AS [g]
    WHERE ([g].[Discriminator] = 'Officer') OR ([g].[Discriminator] = 'Gear')
) AS [g] ON [w].[OwnerFullName] = [g].[FullName]
ORDER BY [w.Owner.CityOfBirth].[Name]",
                Sql);
        }

        public override void Where_enum()
        {
            base.Where_enum();

            Assert.Equal(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [Gear] AS [g]
WHERE [g].[Discriminator] IN ('Officer', 'Gear') AND ([g].[Rank] = 2)",
                Sql);
        }

        public override void Where_nullable_enum_with_constant()
        {
            base.Where_nullable_enum_with_constant();

            Assert.Equal(
                @"SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapon] AS [w]
WHERE [w].[AmmunitionType] = 1",
                Sql);
        }

        public override void Where_nullable_enum_with_null_constant()
        {
            base.Where_nullable_enum_with_null_constant();

            Assert.Equal(
                @"SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapon] AS [w]
WHERE [w].[AmmunitionType] IS NULL",
                Sql);
        }

        public override void Where_nullable_enum_with_non_nullable_parameter()
        {
            base.Where_nullable_enum_with_non_nullable_parameter();

            Assert.Equal(
                @"@__p_0: 1

SELECT [w].[Id], [w].[AmmunitionType], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapon] AS [w]
WHERE [w].[AmmunitionType] = @__p_0",
                Sql);
        }

        public override void Where_nullable_enum_with_nullable_parameter()
        {
            base.Where_nullable_enum_with_nullable_parameter();

            Assert.Equal(
                @"@__ammunitionType_0: Cartridge

SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapon] AS [w]
WHERE [w].[AmmunitionType] = @__ammunitionType_0

SELECT [w].[Id], [w].[AmmunitionType], [w].[IsAutomatic], [w].[Name], [w].[OwnerFullName], [w].[SynergyWithId]
FROM [Weapon] AS [w]
WHERE [w].[AmmunitionType] IS NULL",
                Sql);
        }

        public override void Select_inverted_boolean()
        {
            base.Select_inverted_boolean();

            Assert.Equal(
                @"SELECT [w].[Id], CASE
    WHEN [w].[IsAutomatic] = 0
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END
FROM [Weapon] AS [w]
WHERE [w].[IsAutomatic] = 1",
                Sql);
        }

        public override void Select_comparison_with_null()
        {
            base.Select_comparison_with_null();

            Assert.Equal(
                @"@__ammunitionType_1: Cartridge
@__ammunitionType_0: Cartridge

SELECT [w].[Id], CASE
    WHEN [w].[AmmunitionType] = @__ammunitionType_1
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END
FROM [Weapon] AS [w]
WHERE [w].[AmmunitionType] = @__ammunitionType_0

SELECT [w].[Id], CASE
    WHEN [w].[AmmunitionType] IS NULL
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END
FROM [Weapon] AS [w]
WHERE [w].[AmmunitionType] IS NULL",
                Sql);
        }

        public override void Select_ternary_operation_with_boolean()
        {
            base.Select_ternary_operation_with_boolean();

            Assert.Equal(
                @"SELECT [w].[Id], CASE
    WHEN [w].[IsAutomatic] = 1
    THEN 1 ELSE 0
END
FROM [Weapon] AS [w]",
                Sql);
        }

        public override void Select_ternary_operation_with_inverted_boolean()
        {
            base.Select_ternary_operation_with_inverted_boolean();

            Assert.Equal(
                @"SELECT [w].[Id], CASE
    WHEN [w].[IsAutomatic] = 0
    THEN 1 ELSE 0
END
FROM [Weapon] AS [w]",
                Sql);
        }

        public override void Select_ternary_operation_with_has_value_not_null()
        {
            // TODO: Optimize this query (See #4267)
            base.Select_ternary_operation_with_has_value_not_null();

            Assert.Equal(
                @"SELECT [w].[Id], CASE
    WHEN [w].[AmmunitionType] IS NOT NULL AND (([w].[AmmunitionType] = 1) AND [w].[AmmunitionType] IS NOT NULL)
    THEN 'Yes' ELSE 'No'
END
FROM [Weapon] AS [w]
WHERE [w].[AmmunitionType] IS NOT NULL AND (([w].[AmmunitionType] = 1) AND [w].[AmmunitionType] IS NOT NULL)",
                Sql);
        }

        public override void Select_ternary_operation_multiple_conditions()
        {
            base.Select_ternary_operation_multiple_conditions();

            Assert.Equal(
                @"SELECT [w].[Id], CASE
    WHEN ([w].[AmmunitionType] = 2) AND ([w].[SynergyWithId] = 1)
    THEN 'Yes' ELSE 'No'
END
FROM [Weapon] AS [w]",
                Sql);
        }

        public override void Select_ternary_operation_multiple_conditions_2()
        {
            base.Select_ternary_operation_multiple_conditions_2();

            Assert.Equal(
                @"SELECT [w].[Id], CASE
    WHEN [w].[IsAutomatic] = 0 AND (([w].[SynergyWithId] = 1) AND [w].[SynergyWithId] IS NOT NULL)
    THEN 'Yes' ELSE 'No'
END
FROM [Weapon] AS [w]",
                Sql);
        }

        public override void Select_multiple_conditions()
        {
            base.Select_multiple_conditions();

            Assert.Equal(
                @"SELECT [w].[Id], CASE
    WHEN [w].[IsAutomatic] = 0 AND (([w].[SynergyWithId] = 1) AND [w].[SynergyWithId] IS NOT NULL)
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END
FROM [Weapon] AS [w]",
                Sql);
        }

        public override void Select_nested_ternary_operations()
        {
            base.Select_nested_ternary_operations();

            Assert.Equal(
                @"SELECT [w].[Id], CASE
    WHEN [w].[IsAutomatic] = 0
    THEN CASE
        WHEN ([w].[AmmunitionType] = 1) AND [w].[AmmunitionType] IS NOT NULL
        THEN 'ManualCartridge' ELSE 'Manual'
    END ELSE 'Auto'
END
FROM [Weapon] AS [w]",
                Sql);
        }

        public override void Select_Where_Navigation_Scalar_Equals_Navigation_Scalar()
        {
            base.Select_Where_Navigation_Scalar_Equals_Navigation_Scalar();

            Assert.Equal(
                @"",
                Sql);
        }

        public override void Select_Singleton_Navigation_With_Member_Access()
        {
            base.Select_Singleton_Navigation_With_Member_Access();

            Assert.Equal(
                @"SELECT [ct].[Id], [ct].[GearNickName], [ct].[GearSquadId], [ct].[Note], [ct.Gear].[Nickname], [ct.Gear].[SquadId], [ct.Gear].[AssignedCityName], [ct.Gear].[CityOrBirthName], [ct.Gear].[Discriminator], [ct.Gear].[FullName], [ct.Gear].[LeaderNickname], [ct.Gear].[LeaderSquadId], [ct.Gear].[Rank]
FROM [CogTag] AS [ct]
LEFT JOIN [Gear] AS [ct.Gear] ON ([ct].[GearNickName] = [ct.Gear].[Nickname]) AND ([ct].[GearSquadId] = [ct.Gear].[SquadId])
ORDER BY [ct].[GearNickName], [ct].[GearSquadId]",
                Sql);
        }

        public override void Select_Where_Navigation()
        {
            base.Select_Where_Navigation();

            Assert.Equal(
                @"SELECT [ct].[Id], [ct].[GearNickName], [ct].[GearSquadId], [ct].[Note], [ct.Gear].[Nickname], [ct.Gear].[SquadId], [ct.Gear].[AssignedCityName], [ct.Gear].[CityOrBirthName], [ct.Gear].[Discriminator], [ct.Gear].[FullName], [ct.Gear].[LeaderNickname], [ct.Gear].[LeaderSquadId], [ct.Gear].[Rank]
FROM [CogTag] AS [ct]
LEFT JOIN [Gear] AS [ct.Gear] ON ([ct].[GearNickName] = [ct.Gear].[Nickname]) AND ([ct].[GearSquadId] = [ct.Gear].[SquadId])
ORDER BY [ct].[GearNickName], [ct].[GearSquadId]",
                Sql);
        }

        public override void Select_Where_Navigation_Client()
        {
            base.Select_Where_Navigation_Client();

            Assert.Equal(
                @"SELECT [o].[Id], [o].[GearNickName], [o].[GearSquadId], [o].[Note], [o.Gear].[Nickname], [o.Gear].[SquadId], [o.Gear].[AssignedCityName], [o.Gear].[CityOrBirthName], [o.Gear].[Discriminator], [o.Gear].[FullName], [o.Gear].[LeaderNickname], [o.Gear].[LeaderSquadId], [o.Gear].[Rank]
FROM [CogTag] AS [o]
LEFT JOIN [Gear] AS [o.Gear] ON ([o].[GearNickName] = [o.Gear].[Nickname]) AND ([o].[GearSquadId] = [o.Gear].[SquadId])
WHERE [o].[GearNickName] IS NOT NULL OR [o].[GearSquadId] IS NOT NULL
ORDER BY [o].[GearNickName], [o].[GearSquadId]",
                Sql);
        }

        public override void Select_Where_Navigation_Equals_Navigation()
        {
            base.Select_Where_Navigation_Equals_Navigation();

            Assert.Equal(
                @"SELECT [ct1].[Id], [ct1].[GearNickName], [ct1].[GearSquadId], [ct1].[Note], [ct2].[Id], [ct2].[GearNickName], [ct2].[GearSquadId], [ct2].[Note]
FROM [CogTag] AS [ct1]
CROSS JOIN [CogTag] AS [ct2]
WHERE (([ct1].[GearNickName] = [ct2].[GearNickName]) OR ([ct1].[GearNickName] IS NULL AND [ct2].[GearNickName] IS NULL)) AND (([ct1].[GearSquadId] = [ct2].[GearSquadId]) OR ([ct1].[GearSquadId] IS NULL AND [ct2].[GearSquadId] IS NULL))",
                Sql);
        }

        public override void Select_Where_Navigation_Null()
        {
            base.Select_Where_Navigation_Null();

            Assert.Equal(
                @"SELECT [ct].[Id], [ct].[GearNickName], [ct].[GearSquadId], [ct].[Note]
FROM [CogTag] AS [ct]
WHERE [ct].[GearNickName] IS NULL AND [ct].[GearSquadId] IS NULL",
                Sql);
        }

        public override void Select_Where_Navigation_Null_Reverse()
        {
            base.Select_Where_Navigation_Null_Reverse();

            Assert.Equal(
                @"SELECT [ct].[Id], [ct].[GearNickName], [ct].[GearSquadId], [ct].[Note]
FROM [CogTag] AS [ct]
WHERE [ct].[GearNickName] IS NULL AND [ct].[GearSquadId] IS NULL",
                Sql);
        }

        public override void Select_Where_Navigation_Scalar_Equals_Navigation_Scalar_Projected()
        {
            base.Select_Where_Navigation_Scalar_Equals_Navigation_Scalar_Projected();

            Assert.StartsWith(
                @"SELECT [ct2.Gear].[Nickname], [ct2.Gear].[SquadId], [ct2.Gear].[AssignedCityName], [ct2.Gear].[CityOrBirthName], [ct2.Gear].[Discriminator], [ct2.Gear].[FullName], [ct2.Gear].[LeaderNickname], [ct2.Gear].[LeaderSquadId], [ct2.Gear].[Rank]
FROM [Gear] AS [ct2.Gear]
WHERE ([ct2.Gear].[Discriminator] = 'Officer') OR ([ct2.Gear].[Discriminator] = 'Gear')

SELECT [ct1].[Id], [ct1].[GearNickName], [ct1].[GearSquadId], [ct1].[Note], [ct1.Gear].[Nickname], [ct1.Gear].[SquadId], [ct1.Gear].[AssignedCityName], [ct1.Gear].[CityOrBirthName], [ct1.Gear].[Discriminator], [ct1.Gear].[FullName], [ct1.Gear].[LeaderNickname], [ct1.Gear].[LeaderSquadId], [ct1.Gear].[Rank]
FROM [CogTag] AS [ct1]
LEFT JOIN [Gear] AS [ct1.Gear] ON ([ct1].[GearNickName] = [ct1.Gear].[Nickname]) AND ([ct1].[GearSquadId] = [ct1.Gear].[SquadId])
ORDER BY [ct1].[GearNickName], [ct1].[GearSquadId]",
                Sql);
        }

        public override void Singleton_Navigation_With_Member_Access()
        {
            base.Singleton_Navigation_With_Member_Access();

            Assert.Equal(
                @"SELECT [ct].[Id], [ct].[GearNickName], [ct].[GearSquadId], [ct].[Note], [ct.Gear].[Nickname], [ct.Gear].[SquadId], [ct.Gear].[AssignedCityName], [ct.Gear].[CityOrBirthName], [ct.Gear].[Discriminator], [ct.Gear].[FullName], [ct.Gear].[LeaderNickname], [ct.Gear].[LeaderSquadId], [ct.Gear].[Rank]
FROM [CogTag] AS [ct]
LEFT JOIN [Gear] AS [ct.Gear] ON ([ct].[GearNickName] = [ct.Gear].[Nickname]) AND ([ct].[GearSquadId] = [ct.Gear].[SquadId])
ORDER BY [ct].[GearNickName], [ct].[GearSquadId]",
                Sql);
        }

        public override void GroupJoin_Composite_Key()
        {
            base.GroupJoin_Composite_Key();

            Assert.Equal(
                @"SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
FROM [CogTag] AS [ct]
LEFT JOIN [Gear] AS [g] ON ([ct].[GearNickName] = [g].[Nickname]) AND ([ct].[GearSquadId] = [g].[SquadId])
ORDER BY [ct].[GearNickName], [ct].[GearSquadId]",
                Sql);
        }

        public override void Join_navigation_translated_to_subquery_composite_key()
        {
            base.Join_navigation_translated_to_subquery_composite_key();

            Assert.Equal(
                @"SELECT [g].[FullName], [t].[Note]
FROM [Gear] AS [g]
INNER JOIN [CogTag] AS [t] ON [g].[FullName] = (
    SELECT TOP(1) [subQuery0].[FullName]
    FROM [Gear] AS [subQuery0]
    WHERE ([subQuery0].[Nickname] = [t].[GearNickName]) AND ([subQuery0].[SquadId] = [t].[GearSquadId])
)",
                Sql);
        }

        public override void Collection_with_inheritance_and_join_include_joined()
        {
            base.Collection_with_inheritance_and_join_include_joined();

            Assert.Equal(
                @"SELECT [t0].[Nickname], [t0].[SquadId], [t0].[AssignedCityName], [t0].[CityOrBirthName], [t0].[Discriminator], [t0].[FullName], [t0].[LeaderNickname], [t0].[LeaderSquadId], [t0].[Rank], [c].[Id], [c].[GearNickName], [c].[GearSquadId], [c].[Note]
FROM [CogTag] AS [t]
INNER JOIN (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
    FROM [Gear] AS [g]
    WHERE [g].[Discriminator] = 'Officer'
) AS [t0] ON ([t].[GearSquadId] = [t0].[SquadId]) AND ([t].[GearNickName] = [t0].[Nickname])
LEFT JOIN [CogTag] AS [c] ON ([c].[GearNickName] = [t0].[Nickname]) AND ([c].[GearSquadId] = [t0].[SquadId])",
                Sql);
        }

        public override void Collection_with_inheritance_and_join_include_source()
        {
            base.Collection_with_inheritance_and_join_include_source();

            Assert.Equal(
                @"SELECT [t0].[Nickname], [t0].[SquadId], [t0].[AssignedCityName], [t0].[CityOrBirthName], [t0].[Discriminator], [t0].[FullName], [t0].[LeaderNickname], [t0].[LeaderSquadId], [t0].[Rank], [c].[Id], [c].[GearNickName], [c].[GearSquadId], [c].[Note]
FROM (
    SELECT [g].[Nickname], [g].[SquadId], [g].[AssignedCityName], [g].[CityOrBirthName], [g].[Discriminator], [g].[FullName], [g].[LeaderNickname], [g].[LeaderSquadId], [g].[Rank]
    FROM [Gear] AS [g]
    WHERE [g].[Discriminator] = 'Officer'
) AS [t0]
INNER JOIN [CogTag] AS [t] ON ([t0].[SquadId] = [t].[GearSquadId]) AND ([t0].[Nickname] = [t].[GearNickName])
LEFT JOIN [CogTag] AS [c] ON ([c].[GearNickName] = [t0].[Nickname]) AND ([c].[GearSquadId] = [t0].[SquadId])",
                Sql);
        }

        public GearsOfWarQuerySqlServerTest(GearsOfWarQuerySqlServerFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            TestSqlLoggerFactory.CaptureOutput(testOutputHelper);
        }

        protected override void ClearLog() => TestSqlLoggerFactory.Reset();

        private static string Sql => TestSqlLoggerFactory.Sql;
    }
}
