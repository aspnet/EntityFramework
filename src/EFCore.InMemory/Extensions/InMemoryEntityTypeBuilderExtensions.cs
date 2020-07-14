// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Extension methods for <see cref="EntityTypeBuilder" /> for the in-memory provider.
    /// </summary>
    public static class InMemoryEntityTypeBuilderExtensions
    {
        /// <summary>
        ///     Configures a query used to provide data for an entity type.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="query"> The query that will provide the underlying data for the entity type. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static EntityTypeBuilder<TEntity> ToQuery<TEntity>(
            [NotNull] this EntityTypeBuilder<TEntity> entityTypeBuilder,
            [NotNull] Expression<Func<IQueryable<TEntity>>> query)
            where TEntity : class
        {
            Check.NotNull(query, nameof(query));

            InMemoryEntityTypeExtensions.SetDefiningQuery(entityTypeBuilder.Metadata, query);

            return entityTypeBuilder;
        }

        /// <summary>
        ///     Configures a query used to provide data for an entity type.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="query"> The query that will provide the underlying data for the entity type. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same builder instance if the query was set, <see langword="null" /> otherwise.
        /// </returns>
        public static IConventionEntityTypeBuilder HasDefiningQuery(
            [NotNull] this IConventionEntityTypeBuilder entityTypeBuilder,
            [CanBeNull] LambdaExpression query,
            bool fromDataAnnotation = false)
        {
            if (CanSetDefiningQuery(entityTypeBuilder, query, fromDataAnnotation))
            {
                InMemoryEntityTypeExtensions.SetDefiningQuery(entityTypeBuilder.Metadata, query, fromDataAnnotation);

                return entityTypeBuilder;
            }

            return null;
        }

        /// <summary>
        ///     Returns a value indicating whether the given defining query can be set from the current configuration source.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="query"> The query that will provide the underlying data for the keyless entity type. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <see langword="true" /> if the given defining query can be set. </returns>
        public static bool CanSetDefiningQuery(
            [NotNull] this IConventionEntityTypeBuilder entityTypeBuilder,
            [CanBeNull] LambdaExpression query,
            bool fromDataAnnotation = false)
#pragma warning disable EF1001 // Internal EF Core API usage.
            => entityTypeBuilder.CanSetAnnotation(CoreAnnotationNames.DefiningQuery, query, fromDataAnnotation);
#pragma warning restore EF1001 // Internal EF Core API usage.
    }
}
