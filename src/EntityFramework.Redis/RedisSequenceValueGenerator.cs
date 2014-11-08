// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Identity;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Redis.Utilities;

namespace Microsoft.Data.Entity.Redis
{
    public class RedisSequenceValueGenerator : BlockOfSequentialValuesGenerator
    {
        public RedisSequenceValueGenerator([NotNull] string sequenceName, int blockSize)
            : base(sequenceName, blockSize)
        {
        }

        public override long GetNewCurrentValue(StateEntry stateEntry, IProperty property)
        {
            // TODO: Decouple from DbContextConfiguration (Issue #641)
            var database = (RedisDatabase)stateEntry.Configuration.Database;
            return database.GetNextGeneratedValue(property, BlockSize, SequenceName);
        }

        public override async Task<long> GetNewCurrentValueAsync(
            StateEntry stateEntry, IProperty property, CancellationToken cancellationToken)
        {
            Check.NotNull(stateEntry, "stateEntry");
            Check.NotNull(property, "property");

            cancellationToken.ThrowIfCancellationRequested();

            // TODO: Decouple from DbContextConfiguration (Issue #641)
            var database = (RedisDatabase)stateEntry.Configuration.Database;

            return
                await database.GetNextGeneratedValueAsync(
                    property, BlockSize, SequenceName, cancellationToken)
                    .WithCurrentCulture();
        }
    }
}
