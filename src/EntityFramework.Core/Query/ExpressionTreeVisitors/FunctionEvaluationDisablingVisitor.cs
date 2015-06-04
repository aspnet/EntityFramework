﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Data.Entity.Query.Expressions;
using Remotion.Linq.Clauses.Expressions;

namespace Microsoft.Data.Entity.Query.ExpressionTreeVisitors
{
    public class FunctionEvaluationDisablingVisitor : ExpressionTreeVisitorBase
    {
        public static readonly MethodInfo DbContextSetMethodInfo
            = typeof(DbContext).GetTypeInfo().GetDeclaredMethod("Set");

        private static MethodInfo[] _nonDeterministicMethodInfos = new MethodInfo[]
        {
                typeof(Guid).GetTypeInfo().GetDeclaredMethod("NewGuid"),
                typeof(DateTime).GetTypeInfo().GetDeclaredProperty("Now").GetMethod,
        };

        protected override Expression VisitMethodCallExpression(MethodCallExpression expression)
        {
            if (expression.Method.IsGenericMethod)
            {
                var genericMethodDefinition = expression.Method.GetGenericMethodDefinition();
                if (ReferenceEquals(genericMethodDefinition, QueryExtensions.PropertyMethodInfo)
                    || ReferenceEquals(genericMethodDefinition, DbContextSetMethodInfo))
                {
                    return base.VisitMethodCallExpression(expression);
                }
            }

            if (IsQueryable(expression.Object) || IsQueryable(expression.Arguments.FirstOrDefault()))
            {
                return base.VisitMethodCallExpression(expression);
            }

            var newObject = VisitExpression(expression.Object);
            var newArguments = VisitAndConvert(expression.Arguments, "VisitMethodCallExpression");

            var newMethodCall = newObject != expression.Object || newArguments != expression.Arguments
                ? Expression.Call(newObject, expression.Method, newArguments)
                : expression;

            return _nonDeterministicMethodInfos.Contains(expression.Method)
                ? (Expression)new MethodCallEvaluationPreventingExpression(newMethodCall)
                : newMethodCall;
        }

        private bool IsQueryable(Expression expression)
        {
            return expression == null
                ? false
                : typeof(IQueryable).GetTypeInfo().IsAssignableFrom(expression.Type.GetTypeInfo());
        }

        protected override Expression VisitMemberExpression(MemberExpression expression)
        {
            var propertyInfo = expression.Member as PropertyInfo;

            return propertyInfo != null && _nonDeterministicMethodInfos.Contains(propertyInfo.GetMethod)
                ? (Expression)new PropertyEvaluationPreventingExpression(expression)
                : expression;
        }

        protected override Expression VisitSubQueryExpression(SubQueryExpression expression)
        {
            var clonedModel = expression.QueryModel.Clone();
            clonedModel.TransformExpressions(VisitExpression);

            return new SubQueryExpression(clonedModel);
        }
    }
}
