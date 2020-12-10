// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore
{
    public class SeedingSqliteTest : SeedingTestBase
    {
        protected override TestStore TestStore => SqliteTestStore.Create("sqlliteTestStore");

        protected override SeedingContext CreateContextWithEmptyDatabase(string testId)
            => new SeedingSqliteContext(testId);

        protected override KeylessSeedingContext CreateKeylessContextWithEmptyDatabase(string testId)
            => new KeylessSeedingSqliteContext(testId);

        protected class SeedingSqliteContext : SeedingContext
        {
            public SeedingSqliteContext(string testId)
                : base(testId)
            {
            }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder.UseSqlite(($"Data Source = Seeds{TestId}.db"));
        }

        protected class KeylessSeedingSqliteContext : KeylessSeedingContext
        {
            public KeylessSeedingSqliteContext(string testId)
                : base(testId)
            {
            }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder.UseSqlite(($"Data Source = KeylessSeeds{TestId}.db"));
        }
    }
}
