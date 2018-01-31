// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Reflection;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class LazyLoaderParameterBindingFactory : ServiceParameterBindingFactory
    {
        private static readonly MethodInfo _loadMethod = typeof(ILazyLoader).GetMethod(nameof(ILazyLoader.Load));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public LazyLoaderParameterBindingFactory()
            : base(typeof(ILazyLoader))
        {
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override bool CanBind(
            Type parameterType,
            string parameterName)
            => IsLazyLoader(parameterType)
               || IsLazyLoaderMethod(parameterType, parameterName);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override ParameterBinding Bind(
            IMutableEntityType entityType,
            Type parameterType,
            string parameterName)
        {
            entityType.SetNavigationAccessMode(PropertyAccessMode.Field);

            return parameterType == typeof(ILazyLoader)
                ? new DefaultServiceParameterBinding(
                    typeof(ILazyLoader),
                    typeof(ILazyLoader),
                    entityType.GetServiceProperties().FirstOrDefault(p => IsLazyLoader(p.ClrType)))
                : new ServiceMethodParameterBinding(
                    typeof(Action<object, string>),
                    typeof(ILazyLoader),
                    _loadMethod,
                    entityType.GetServiceProperties().FirstOrDefault(p => IsLazyLoaderMethod(p.ClrType, p.Name)));
        }

        private static bool IsLazyLoader(Type type)
            => type == typeof(ILazyLoader);

        private static bool IsLazyLoaderMethod(Type type, string name)
            => type == typeof(Action<object, string>)
               && name.Equals("lazyLoader", StringComparison.OrdinalIgnoreCase);
    }
}
