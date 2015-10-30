// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Remotion.Linq.Clauses;

namespace Microsoft.Data.Entity.Query.ExpressionVisitors
{
    public abstract class Shaper
    {
        private IQuerySource _querySource;

        protected Shaper([NotNull] IQuerySource querySource)
        {
            _querySource = querySource;
        }

        public virtual bool IsShaperForQuerySource([NotNull] IQuerySource querySource)
            => _querySource == querySource;

        public virtual void UpdateQuerySource([NotNull] IQuerySource querySource)
        {
            _querySource = querySource;
        }

        public virtual int ValueBufferOffset { get; set; }

        public abstract Type Type { get; }
    }
}
