﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionTranslators.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class GetValueOrDefaultTranslator : IMethodCallTranslator
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Expression Translate(
            MethodCallExpression methodCallExpression,
            IDiagnosticsLogger<DbLoggerCategory.Query> logger)
        {
            if (methodCallExpression.Method.Name == nameof(Nullable<int>.GetValueOrDefault)
                && methodCallExpression.Type.IsNumeric())
            {
                if (methodCallExpression.Arguments.Count == 0)
                {
                    return Expression.Coalesce(
                        methodCallExpression.Object,
                        methodCallExpression.Type.GenerateDefaultValueConstantExpression());
                }

                if (methodCallExpression.Arguments.Count == 1)
                {
                    return Expression.Coalesce(
                        methodCallExpression.Object,
                        methodCallExpression.Arguments[0]);
                }
            }

            return null;
        }
    }
}
