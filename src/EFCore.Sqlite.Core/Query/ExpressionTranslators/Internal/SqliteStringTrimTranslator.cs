// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;

namespace Microsoft.EntityFrameworkCore.Sqlite.Query.ExpressionTranslators.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class SqliteStringTrimTranslator : IMethodCallTranslator
    {
        // Method defined in netstandard2.0
        private static readonly MethodInfo _methodInfoWithoutArgs
            = typeof(string).GetRuntimeMethod(nameof(string.Trim), Array.Empty<Type>());

        // Method defined in netcoreapp2.0 only
        private static readonly MethodInfo _methodInfoWithCharArg
            = typeof(string).GetRuntimeMethod(nameof(string.Trim), new[] { typeof(char) });

        // Method defined in netstandard2.0
        private static readonly MethodInfo _methodInfoWithCharArrayArg
            = typeof(string).GetRuntimeMethod(nameof(string.Trim), new[] { typeof(char[]) });

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
            var methodInfo = methodCallExpression.Method;

            if (_methodInfoWithoutArgs.Equals(methodInfo)
                || _methodInfoWithCharArg?.Equals(methodInfo) == true
                || _methodInfoWithCharArrayArg.Equals(methodInfo))
            {
                var sqlArguments = new List<Expression>
                {
                    methodCallExpression.Object
                };

                if (methodCallExpression.Arguments.Count == 1)
                {
                    var constantValue = (methodCallExpression.Arguments[0] as ConstantExpression)?.Value;
                    var charactersToTrim = new List<char>();

                    if (constantValue is char singleChar)
                    {
                        charactersToTrim.Add(singleChar);
                    }
                    else if (constantValue is char[] charArray)
                    {
                        charactersToTrim.AddRange(charArray);
                    }

                    if (charactersToTrim.Count > 0)
                    {
                        sqlArguments.Add(Expression.Constant(new string(charactersToTrim.ToArray()), typeof(string)));
                    }
                }

                return new SqlFunctionExpression("trim", methodCallExpression.Type, sqlArguments);
            }

            return null;
        }
    }
}
