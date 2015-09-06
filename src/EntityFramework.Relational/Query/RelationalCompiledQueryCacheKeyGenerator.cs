﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Query.ExpressionVisitors;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Query
{
    public class RelationalCompiledQueryCacheKeyGenerator : ICompiledQueryCacheKeyGenerator
    {
        private readonly IModel _model;

        public RelationalCompiledQueryCacheKeyGenerator([NotNull] IModel model)
        {
            Check.NotNull(model, nameof(model));

            _model = model;
        }

        public virtual object GenerateCacheKey([NotNull] Expression query, IDatabase database, bool async)
            => new CompiledQueryCacheKey(
                new ExpressionStringBuilder().Build(Check.NotNull(query, nameof(query))),
                _model,
                async,
                ((RelationalDatabase)database).UseRelationalNulls);

        private struct CompiledQueryCacheKey
        {
            private readonly string _query;
            private readonly IModel _model;
            private readonly bool _async;
            private readonly bool _useRelationalNulls;

            public CompiledQueryCacheKey(string query, IModel model, bool async, bool useRelationalNulls)
            {
                _query = query;
                _model = model;
                _async = async;
                _useRelationalNulls = useRelationalNulls;
            }

            public override bool Equals(object obj)
                => !ReferenceEquals(null, obj)
                   && (obj is CompiledQueryCacheKey && Equals((CompiledQueryCacheKey)obj));

            private bool Equals(CompiledQueryCacheKey other)
                => string.Equals(_query, other._query)
                   && _model.Equals(other._model)
                   && _async == other._async
                   && _useRelationalNulls == other._useRelationalNulls;

            public override int GetHashCode()
            {
                unchecked
                {
                    var hashCode = _query.GetHashCode();
                    hashCode = (hashCode * 397) ^ _model.GetHashCode();
                    hashCode = (hashCode * 397) ^ _async.GetHashCode();
                    hashCode = (hashCode * 397) ^ _useRelationalNulls.GetHashCode();

                    return hashCode;
                }
            }
        }
    }
}
