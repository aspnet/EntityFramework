// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Internal
{
    public class DbSetSource
    {
        private static readonly MethodInfo _genericCreate
            = typeof(DbSetSource).GetTypeInfo().GetDeclaredMethods("CreateConstructor").Single();

        // Stores DbSet<T> objects
        private readonly ThreadSafeDictionaryCache<Type, Func<DbContext, object>> _cache
            = new ThreadSafeDictionaryCache<Type, Func<DbContext, object>>();

        public virtual object Create([NotNull] DbContext context, [NotNull] Type type)
            => _cache.GetOrAdd(
                type,
                t => (Func<DbContext, object>)_genericCreate.MakeGenericMethod(type).Invoke(null, null))(context);

        [UsedImplicitly]
        private static Func<DbContext, object> CreateConstructor<TEntity>() where TEntity : class
            => c => new InternalDbSet<TEntity>(c);
    }
}
