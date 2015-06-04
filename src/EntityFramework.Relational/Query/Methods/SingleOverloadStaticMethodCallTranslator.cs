﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Data.Entity.Relational.Query.Expressions;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Relational.Query.Methods
{
    public abstract class SingleOverloadStaticMethodCallTranslator : IMethodCallTranslator
    {
        private readonly Type _declaringType;
        private readonly string _clrMethodName;
        private readonly string _sqlFunctionName;

        public SingleOverloadStaticMethodCallTranslator([NotNull] Type declaringType, [NotNull] string clrMethodName, [NotNull] string sqlFunctionName)
        {
            _declaringType = declaringType;
            _clrMethodName = clrMethodName;
            _sqlFunctionName = sqlFunctionName;
        }

        public virtual Expression Translate([NotNull] MethodCallExpression methodCallExpression)
        {
            var methodInfo = _declaringType.GetTypeInfo().GetDeclaredMethods(_clrMethodName).SingleOrDefault();
            if (methodInfo == methodCallExpression.Method)
            {
                return new SqlFunctionExpression(_sqlFunctionName, methodCallExpression.Arguments, methodCallExpression.Type);
            }

            return null;
        }
    }
}
