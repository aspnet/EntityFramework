﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Core.Utilities
{
    using System;
    using System.Diagnostics;
    using JetBrains.Annotations;
    using Microsoft.Data.Core.Resources;

    [DebuggerStepThrough]
    public static class Check
    {
        public static void NotNull(object value, [InvokerParameterName] [NotNull] string parameterName)
        {
            NotEmpty(parameterName, "parameterName");

            if (value == null)
            {
                throw new ArgumentNullException(parameterName);
            }
        }

        public static S NotNull<T, S>(T value, [InvokerParameterName] [NotNull] string parameterName, Func<T, S> result)
        {
            NotNull(value, parameterName);

            return result(value);
        }

//        public static T Inline<T>(T value, Action checker)
//            where T : class
//        {
//            checker();
//
//            return value;
//        }

        public static string NotEmpty(string value, [InvokerParameterName] [NotNull] string parameterName)
        {
            if (string.IsNullOrWhiteSpace(parameterName))
            {
                throw new ArgumentException(Strings.ArgumentIsNullOrWhitespace("parameterName"));
            }

            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException(Strings.ArgumentIsNullOrWhitespace(parameterName));
            }

            return value;
        }
    }
}
