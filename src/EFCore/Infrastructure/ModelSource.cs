// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Threading;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    /// <summary>
    ///     <para>
    ///         An implementation of <see cref="IModelSource" /> that produces a model based on
    ///         the <see cref="DbSet{TEntity}" /> properties exposed on the context. The model is cached to avoid
    ///         recreating it every time it is requested.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Singleton"/>. This means a single instance
    ///         is used by many <see cref="DbContext"/> instances. The implementation must be thread-safe.
    ///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped"/>.
    ///     </para>
    /// </summary>
    public class ModelSource : IModelSource
    {
        private readonly ConcurrentDictionary<object, Lazy<IModel>> _models
            = new ConcurrentDictionary<object, Lazy<IModel>>();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [EntityFrameworkInternal]
        public ModelSource([NotNull] ModelSourceDependencies dependencies)
        {
            Check.NotNull(dependencies, nameof(dependencies));

            Dependencies = dependencies;
        }

        /// <summary>
        ///     Dependencies used to create a <see cref="ModelSource" />
        /// </summary>
        protected virtual ModelSourceDependencies Dependencies { get; }

        /// <summary>
        ///     Returns the model from the cache, or creates a model if it is not present in the cache.
        /// </summary>
        /// <param name="context"> The context the model is being produced for. </param>
        /// <param name="conventionSetBuilder"> The convention set to use when creating the model. </param>
        /// <param name="validator"> The validator to verify the model can be successfully used with the context. </param>
        /// <param name="loggers"> The loggers to use. </param>
        /// <returns> The model to be used. </returns>
        public virtual IModel GetModel(
            DbContext context,
            IConventionSetBuilder conventionSetBuilder,
            IModelValidator validator,
            DiagnosticsLoggers loggers)
            => _models.GetOrAdd(
                Dependencies.ModelCacheKeyFactory.Create(context),
                // Using a Lazy here so that OnModelCreating, etc. really only gets called once, since it may not be thread safe.
                k => new Lazy<IModel>(
                    () => CreateModel(context, conventionSetBuilder, validator, loggers),
                    LazyThreadSafetyMode.ExecutionAndPublication)).Value;

        /// <summary>
        ///     Creates the model. This method is called when the model was not found in the cache.
        /// </summary>
        /// <param name="context"> The context the model is being produced for. </param>
        /// <param name="conventionSetBuilder"> The convention set to use when creating the model. </param>
        /// <param name="validator"> The validator to verify the model can be successfully used with the context. </param>
        /// <param name="loggers"> The loggers to use. </param>
        /// <returns> The model to be used. </returns>
        protected virtual IModel CreateModel(
            [NotNull] DbContext context,
            [NotNull] IConventionSetBuilder conventionSetBuilder,
            [NotNull] IModelValidator validator,
            DiagnosticsLoggers loggers)
        {
            Check.NotNull(context, nameof(context));
            Check.NotNull(validator, nameof(validator));
            Check.NotNull(loggers, nameof(loggers));

            var conventionSet = conventionSetBuilder.CreateConventionSet();
            conventionSet.ModelBuiltConventions.Add(new ValidatingConvention(validator, loggers));

            var modelBuilder = new ModelBuilder(conventionSet);

            Dependencies.ModelCustomizer.Customize(modelBuilder, context);

            return modelBuilder.FinalizeModel();
        }
    }
}
