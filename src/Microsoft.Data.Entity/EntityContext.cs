// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using JetBrains.Annotations;
    using Microsoft.Data.Entity.Utilities;

    public class EntityContext : IDisposable
    {
        private readonly EntityConfiguration _entityConfiguration;

        public EntityContext([NotNull] EntityConfiguration entityConfiguration)
        {
            Check.NotNull(entityConfiguration, "entityConfiguration");

            _entityConfiguration = entityConfiguration;
        }

        public virtual int SaveChanges()
        {
            // TODO
            return 0;
        }

        public virtual Task<int> SaveChangesAsync()
        {
            return SaveChangesAsync(CancellationToken.None);
        }

        public virtual Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        {
            // TODO
            return Task.FromResult(0);
        }

        public void Dispose()
        {
            // TODO
        }

        public virtual Database Database
        {
            get { return new Database(); }
        }
    }
}
