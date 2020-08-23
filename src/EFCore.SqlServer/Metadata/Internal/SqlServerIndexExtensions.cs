// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.SqlServer.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static class SqlServerIndexExtensions
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static bool AreCompatibleForSqlServer(
            [NotNull] this IIndex index,
            [NotNull] IIndex duplicateIndex,
            in StoreObjectIdentifier storeObject,
            bool shouldThrow)
        {
            if (index.GetIncludeProperties() != duplicateIndex.GetIncludeProperties())
            {
                if (index.GetIncludeProperties() == null
                    || duplicateIndex.GetIncludeProperties() == null
                    || !SameColumnNames(index, duplicateIndex, storeObject))
                {
                    if (shouldThrow)
                    {
                        throw new InvalidOperationException(
                            SqlServerStrings.DuplicateIndexIncludedMismatch(
                                index.Properties.Format(),
                                index.DeclaringEntityType.DisplayName(),
                                duplicateIndex.Properties.Format(),
                                duplicateIndex.DeclaringEntityType.DisplayName(),
                                index.DeclaringEntityType.GetSchemaQualifiedTableName(),
                                index.GetDatabaseName(storeObject),
                                FormatInclude(index, storeObject),
                                FormatInclude(duplicateIndex, storeObject)));
                    }

                    return false;
                }
            }

            if (index.IsCreatedOnline() != duplicateIndex.IsCreatedOnline())
            {
                if (shouldThrow)
                {
                    throw new InvalidOperationException(
                        SqlServerStrings.DuplicateIndexOnlineMismatch(
                            index.Properties.Format(),
                            index.DeclaringEntityType.DisplayName(),
                            duplicateIndex.Properties.Format(),
                            duplicateIndex.DeclaringEntityType.DisplayName(),
                            index.DeclaringEntityType.GetSchemaQualifiedTableName(),
                            index.GetDatabaseName(storeObject)));
                }

                return false;
            }

            if (index.IsClustered(storeObject) != duplicateIndex.IsClustered(storeObject))
            {
                if (shouldThrow)
                {
                    throw new InvalidOperationException(
                        SqlServerStrings.DuplicateIndexClusteredMismatch(
                            index.Properties.Format(),
                            index.DeclaringEntityType.DisplayName(),
                            duplicateIndex.Properties.Format(),
                            duplicateIndex.DeclaringEntityType.DisplayName(),
                            index.DeclaringEntityType.GetSchemaQualifiedTableName(),
                            index.GetDatabaseName(storeObject)));
                }

                return false;
            }

            if (index.GetFillFactor() != duplicateIndex.GetFillFactor())
            {
                if (shouldThrow)
                {
                    throw new InvalidOperationException(
                        SqlServerStrings.DuplicateIndexFillFactorMismatch(
                            index.Properties.Format(),
                            index.DeclaringEntityType.DisplayName(),
                            duplicateIndex.Properties.Format(),
                            duplicateIndex.DeclaringEntityType.DisplayName(),
                            index.DeclaringEntityType.GetSchemaQualifiedTableName(),
                            index.GetDatabaseName(storeObject)));
                }

                return false;
            }

            return true;

            static bool SameColumnNames(IIndex index, IIndex duplicateIndex, StoreObjectIdentifier storeObject)
                => index.GetIncludeProperties().Select(
                        p => index.DeclaringEntityType.FindProperty(p).GetColumnName(storeObject))
                    .SequenceEqual(
                        duplicateIndex.GetIncludeProperties().Select(
                            p => duplicateIndex.DeclaringEntityType.FindProperty(p)
                                .GetColumnName(storeObject)));
        }

        private static string FormatInclude(IIndex index, StoreObjectIdentifier storeObject)
            => index.GetIncludeProperties() == null
                ? "{}"
                : "{'"
                + string.Join(
                    "', '",
                    index.GetIncludeProperties().Select(
                        p => index.DeclaringEntityType.FindProperty(p)
                            ?.GetColumnName(storeObject)))
                + "'}";
    }
}
