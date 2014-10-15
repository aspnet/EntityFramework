// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.Storage
{
    public abstract class DataStoreCreator
    {
        public abstract bool EnsureDeleted([NotNull] IModel model);
        public abstract Task<bool> EnsureDeletedAsync([NotNull] IModel model, CancellationToken cancellationToken = default(CancellationToken));
        public abstract bool EnsureCreated([NotNull] IModel model);
        public abstract Task<bool> EnsureCreatedAsync([NotNull] IModel model, CancellationToken cancellationToken = default(CancellationToken));
    }
}
