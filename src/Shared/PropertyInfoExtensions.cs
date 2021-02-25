// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;

#nullable enable

// ReSharper disable once CheckNamespace
namespace System.Reflection
{
    [DebuggerStepThrough]
    internal static class PropertyInfoExtensions
    {
        public static bool IsStatic(this PropertyInfo property)
            => (property.GetMethod ?? property.SetMethod)!.IsStatic;

        public static bool IsCandidateProperty(this PropertyInfo propertyInfo, bool needsWrite = true, bool publicOnly = true)
            => !propertyInfo.IsStatic()
                && propertyInfo.CanRead
                && (!needsWrite || propertyInfo.FindSetterProperty() != null)
                && propertyInfo.GetMethod != null
                && (!publicOnly || propertyInfo.GetMethod.IsPublic)
                && propertyInfo.GetIndexParameters().Length == 0;

        public static bool IsIndexerProperty([NotNull] this PropertyInfo propertyInfo)
        {
            var indexParams = propertyInfo.GetIndexParameters();
            return indexParams.Length == 1
                && indexParams[0].ParameterType == typeof(string);
        }

        public static PropertyInfo? FindGetterProperty([NotNull] this PropertyInfo propertyInfo)
            => propertyInfo.DeclaringType
                .GetPropertiesInHierarchy(propertyInfo.GetSimpleMemberName())
                .FirstOrDefault(p => p.GetMethod != null);

        public static PropertyInfo? FindSetterProperty([NotNull] this PropertyInfo propertyInfo)
            => propertyInfo.DeclaringType
                .GetPropertiesInHierarchy(propertyInfo.GetSimpleMemberName())
                .FirstOrDefault(p => p.SetMethod != null);
    }
}
