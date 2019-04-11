﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionTranslators.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class EnumHasFlagTranslator : IMethodCallTranslator
    {
        private static readonly MethodInfo _methodInfo
            = typeof(Enum).GetRuntimeMethod(nameof(Enum.HasFlag), new[] { typeof(Enum) });

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
            Check.NotNull(methodCallExpression, nameof(methodCallExpression));

            if (Equals(methodCallExpression.Method, _methodInfo))
            {
                var argument = methodCallExpression.Arguments[0];
                argument = argument.RemoveConvert();

                // ReSharper disable once PossibleNullReferenceException
                var objectEnumType = methodCallExpression.Object.Type.UnwrapNullableType();
                var argumentEnumType = argument.Type.UnwrapNullableType();

                if (argument is ConstantExpression constantExpression)
                {
                    if (constantExpression.Value == null)
                    {
                        return null;
                    }

                    argumentEnumType = constantExpression.Value.GetType();
                    argument = Expression.Constant(constantExpression.Value, argumentEnumType);
                }

                if (objectEnumType != argumentEnumType)
                {
                    return null;
                }

                var objectType = objectEnumType.UnwrapEnumType();

                var convertedObjectExpression = Expression.Convert(methodCallExpression.Object, objectType);
                var convertedArgumentExpression = Expression.Convert(argument, objectType);

                return Expression.Equal(
                    Expression.And(
                        convertedObjectExpression,
                        convertedArgumentExpression),
                    convertedArgumentExpression);
            }

            return null;
        }
    }
}
