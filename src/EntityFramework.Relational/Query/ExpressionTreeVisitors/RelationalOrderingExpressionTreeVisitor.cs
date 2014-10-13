// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Query.ExpressionTreeVisitors;
using Microsoft.Data.Entity.Relational.Utilities;
using Remotion.Linq.Clauses;

namespace Microsoft.Data.Entity.Relational.Query.ExpressionTreeVisitors
{
    public class RelationalOrderingExpressionTreeVisitor : DefaultQueryExpressionTreeVisitor
    {
        private readonly Ordering _ordering;

        public RelationalOrderingExpressionTreeVisitor(
            [NotNull] RelationalQueryModelVisitor queryModelVisitor, [NotNull] Ordering ordering)
            : base(Check.NotNull(queryModelVisitor, "queryModelVisitor"))
        {
            Check.NotNull(ordering, "ordering");

            _ordering = ordering;
        }

        protected override Expression VisitMemberExpression(MemberExpression memberExpression)
        {
            ((RelationalQueryModelVisitor)base.QueryModelVisitor)
                .BindMemberExpression(
                    memberExpression,
                    (property, querySource, selectExpression)
                        => selectExpression
                            .AddToProjection(
                                selectExpression
                                    .AddToOrderBy(property, querySource, _ordering.OrderingDirection)));

            return base.VisitMemberExpression(memberExpression);
        }

        protected override Expression VisitMethodCallExpression(MethodCallExpression methodCallExpression)
        {
            ((RelationalQueryModelVisitor)base.QueryModelVisitor)
                .BindMethodCallExpression(
                    methodCallExpression,
                    (property, querySource, selectExpression)
                        => selectExpression
                            .AddToProjection(
                                selectExpression
                                    .AddToOrderBy(property, querySource, _ordering.OrderingDirection)));

            return base.VisitMethodCallExpression(methodCallExpression);
        }
    }
}
