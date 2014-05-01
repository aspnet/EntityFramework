﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;
using Remotion.Linq;

namespace Microsoft.Data.Entity.Query
{
    public class EntityQueryable<TResult> : QueryableBase<TResult>, IAsyncEnumerable<TResult>
    {
        public EntityQueryable([NotNull] EntityQueryExecutor entityQueryExecutor)
            : base(new EntityQueryProvider(Check.NotNull(entityQueryExecutor, "entityQueryExecutor")))
        {
        }

        public EntityQueryable([NotNull] EntityQueryProvider provider, [NotNull] Expression expression)
            : base(
                Check.NotNull(provider, "provider"),
                Check.NotNull(expression, "expression"))
        {
        }

        IAsyncEnumerator<TResult> IAsyncEnumerable<TResult>.GetEnumerator()
        {
            return ((EntityQueryProvider)Provider).AsyncQuery<TResult>(Expression).GetEnumerator();
        }
    }
}
