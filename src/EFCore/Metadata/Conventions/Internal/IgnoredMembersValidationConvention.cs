// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class IgnoredMembersValidationConvention : IModelBuiltConvention
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public IgnoredMembersValidationConvention([NotNull] IDiagnosticsLogger<DbLoggerCategory.Model> logger)
        {
            Logger = logger;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected virtual IDiagnosticsLogger<DbLoggerCategory.Model> Logger { get; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalModelBuilder Apply(InternalModelBuilder modelBuilder)
        {
            foreach (var entityType in modelBuilder.Metadata.GetEntityTypes())
            {
                foreach (var ignoredMember in entityType.GetIgnoredMembers())
                {
                    var property = entityType.FindProperty(ignoredMember);
                    if (property != null)
                    {
                        if (property.DeclaringEntityType != entityType)
                        {
                            throw new InvalidOperationException(
                                CoreStrings.InheritedPropertyCannotBeIgnored(
                                    ignoredMember, entityType.DisplayName(), property.DeclaringEntityType.DisplayName()));
                        }

                        Debug.Assert(false);
                    }

                    var navigation = entityType.FindNavigation(ignoredMember);
                    if (navigation != null)
                    {
                        if (navigation.DeclaringEntityType != entityType)
                        {
                            throw new InvalidOperationException(
                                CoreStrings.InheritedPropertyCannotBeIgnored(
                                    ignoredMember, entityType.DisplayName(), navigation.DeclaringEntityType.DisplayName()));
                        }

                        Debug.Assert(false);
                    }
                }
            }

            return modelBuilder;
        }
    }
}
