// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data.Common;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Query;
using Microsoft.Data.Entity.Relational.Utilities;
using Remotion.Linq.Clauses;
using Remotion.Linq.Parsing;

namespace Microsoft.Data.Entity.Relational.Query.ExpressionTreeVisitors
{
    public class ResultTransformingExpressionTreeVisitor<TResult> : ExpressionTreeVisitor
    {
        private readonly IQuerySource _outerQuerySource;
        private readonly RelationalQueryCompilationContext _relationalQueryCompilationContext;

        public ResultTransformingExpressionTreeVisitor(
            [NotNull] IQuerySource outerQuerySource,
            [NotNull] RelationalQueryCompilationContext relationalQueryCompilationContext)
        {
            Check.NotNull(outerQuerySource, "outerQuerySource");
            Check.NotNull(relationalQueryCompilationContext, "relationalQueryCompilationContext");

            _outerQuerySource = outerQuerySource;
            _relationalQueryCompilationContext = relationalQueryCompilationContext;
        }

        protected override Expression VisitMethodCallExpression([NotNull] MethodCallExpression methodCallExpression)
        {
            Check.NotNull(methodCallExpression, "methodCallExpression");

            var newObject = VisitExpression(methodCallExpression.Object);

            if (newObject != methodCallExpression.Object)
            {
                return newObject;
            }

            var newArguments = VisitAndConvert(methodCallExpression.Arguments, "VisitMethodCallExpression");

            if ((methodCallExpression.Method.MethodIsClosedFormOf(RelationalQueryModelVisitor.CreateEntityMethodInfo)
                 || ReferenceEquals(methodCallExpression.Method, RelationalQueryModelVisitor.CreateValueReaderMethodInfo))
                && ((ConstantExpression)methodCallExpression.Arguments[0]).Value == _outerQuerySource)
            {
                return methodCallExpression.Arguments[3];
            }

            if (methodCallExpression.Method.MethodIsClosedFormOf(
                QuerySourceScope.GetResultMethodInfo)
                && ((ConstantExpression)methodCallExpression.Arguments[0]).Value == _outerQuerySource)
            {
                return
                    QuerySourceScope.GetResult(
                        methodCallExpression.Object,
                        _outerQuerySource,
                        typeof(TResult));
            }

            if (newArguments != methodCallExpression.Arguments)
            {
                if (methodCallExpression.Method.MethodIsClosedFormOf(
                    _relationalQueryCompilationContext.QueryMethodProvider.QueryMethod))
                {
                    return Expression.Call(
                        _relationalQueryCompilationContext.QueryMethodProvider.QueryMethod
                            .MakeGenericMethod(typeof(DbDataReader)),
                        newArguments);
                }

                if (methodCallExpression.Method.MethodIsClosedFormOf(
                    _relationalQueryCompilationContext.LinqOperatorProvider.Select))
                {
                    return ResultOperatorHandler.CallWithPossibleCancellationToken(
                        _relationalQueryCompilationContext.QueryMethodProvider.GetResultMethod
                            .MakeGenericMethod(typeof(TResult)),
                        newArguments[0]);
                }

                if (methodCallExpression.Method.MethodIsClosedFormOf(
                    _relationalQueryCompilationContext.LinqOperatorProvider.SelectMany))
                {
                    return Expression.Call(
                        _relationalQueryCompilationContext.LinqOperatorProvider.SelectMany
                            .MakeGenericMethod(typeof(QuerySourceScope), typeof(DbDataReader)),
                        newArguments);
                }

                return Expression.Call(methodCallExpression.Method, newArguments);
            }

            return methodCallExpression;
        }

        protected override Expression VisitLambdaExpression([NotNull] LambdaExpression lambdaExpression)
        {
            Check.NotNull(lambdaExpression, "lambdaExpression");

            var newBodyExpression = VisitExpression(lambdaExpression.Body);

            return newBodyExpression != lambdaExpression.Body
                ? Expression.Lambda(newBodyExpression, lambdaExpression.Parameters)
                : lambdaExpression;
        }
    }
}
