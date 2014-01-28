﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Core.Utilities
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using JetBrains.Annotations;

    [DebuggerStepThrough]
    public static class TypeExtensions
    {
        public static Type ElementType([NotNull] this Type type)
        {
            Check.NotNull(type, "type");

            var typeInfo = type.GetTypeInfo();

            if (typeInfo.IsGenericType
                && (type.GetGenericTypeDefinition() == typeof(IQueryable<>)
                    || type.GetGenericTypeDefinition() == typeof(IEnumerable<>)))
            {
                return typeInfo.GenericTypeArguments.Single();
            }

            return type;
        }
    }
}
