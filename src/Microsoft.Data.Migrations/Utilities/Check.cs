﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using JetBrains.Annotations;

namespace Microsoft.Data.Migrations.Utilities
{
    [DebuggerStepThrough]
    internal static class Check
    {
        [ContractAnnotation("value:null => halt")]
        public static T NotNull<T>([NoEnumeration] T value, [InvokerParameterName] [NotNull] string parameterName)
        {
            NotEmpty(parameterName, "parameterName");

            if (ReferenceEquals(value, null))
            {
                throw new ArgumentNullException(parameterName);
            }

            return value;
        }

        [ContractAnnotation("value:null => halt")]
        public static string NotEmpty(string value, [InvokerParameterName] [NotNull] string parameterName)
        {
            if (ReferenceEquals(parameterName, null))
            {
                throw new ArgumentNullException("parameterName");
            }

            if (parameterName.Length == 0)
            {
                throw new ArgumentException(Strings.ArgumentIsEmpty("parameterName"));
            }

            if (ReferenceEquals(value, null))
            {
                throw new ArgumentNullException(parameterName);
            }

            if (value.Length == 0)
            {
                throw new ArgumentException(Strings.ArgumentIsEmpty(parameterName));
            }

            return value;
        }

        public static T IsDefined<T>(T value, [InvokerParameterName] [NotNull] string parameterName)
            where T : struct
        {
            NotEmpty(parameterName, "parameterName");

            if (!Enum.IsDefined(typeof(T), value))
            {
                throw new ArgumentException(Strings.InvalidEnumValue(parameterName, typeof(T)));
            }

            return value;
        }
    }
}
