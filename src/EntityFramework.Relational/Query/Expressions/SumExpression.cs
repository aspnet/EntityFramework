// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Relational.Query.Sql;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Relational.Query.Expressions
{
    public class SumExpression : AggregateExpression
    {
        public SumExpression([NotNull] Expression expression)
            : base(Check.NotNull(expression, nameof(expression)))
        {
        }

        protected override Expression Accept([NotNull] ExpressionVisitor visitor)
        {
            Check.NotNull(visitor, nameof(visitor));

            var specificVisitor = visitor as ISqlExpressionVisitor;

            return specificVisitor != null 
                ? specificVisitor.VisitSum(this) 
                : base.Accept(visitor);
        }
    }
}
