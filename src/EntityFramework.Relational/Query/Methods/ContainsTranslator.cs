﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Data.Entity.Relational.Query.Expressions;

namespace Microsoft.Data.Entity.Relational.Query.Methods
{
    public class ContainsTranslator : IMethodCallTranslator
    {
        private static readonly MethodInfo _methodInfo
            = typeof(string).GetRuntimeMethod("Contains", new[] { typeof(string) });

        private static readonly MethodInfo _concat
            = typeof(string).GetRuntimeMethod("Concat", new[] { typeof(string), typeof(string) });

        public virtual Expression Translate(MethodCallExpression methodCallExpression)
        {
            if (ReferenceEquals(methodCallExpression.Method, _methodInfo))
            {
                return new LikeExpression(
                    methodCallExpression.Object,
                    Expression.Add(
                        Expression.Add(
                            new LiteralExpression("%"), 
                            methodCallExpression.Arguments[0], 
                            _concat), 
                        new LiteralExpression("%"), 
                        _concat));
            }

            return null;
        }
    }
}