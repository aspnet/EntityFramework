// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     A convention that adds service properties to entity types.
    /// </summary>
    public class ServicePropertyDiscoveryConvention :
        IEntityTypeAddedConvention,
        IEntityTypeBaseTypeChangedConvention,
        IEntityTypeMemberIgnoredConvention,
        IModelFinalizingConvention
    {
        /// <summary>
        ///     Creates a new instance of <see cref="ServicePropertyDiscoveryConvention" />.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this convention. </param>
        public ServicePropertyDiscoveryConvention(ProviderConventionSetBuilderDependencies dependencies)
        {
            Dependencies = dependencies;
        }

        /// <summary>
        ///     Parameter object containing service dependencies.
        /// </summary>
        protected virtual ProviderConventionSetBuilderDependencies Dependencies { get; }

        /// <summary>
        ///     Called after an entity type is added to the model.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type. </param>
        /// <param name="context"> Additional information associated with convention execution. </param>
        public virtual void ProcessEntityTypeAdded(
            IConventionEntityTypeBuilder entityTypeBuilder,
            IConventionContext<IConventionEntityTypeBuilder> context)
            => Process(entityTypeBuilder);

        /// <summary>
        ///     Called after the base type of an entity type changes.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type. </param>
        /// <param name="newBaseType"> The new base entity type. </param>
        /// <param name="oldBaseType"> The old base entity type. </param>
        /// <param name="context"> Additional information associated with convention execution. </param>
        public virtual void ProcessEntityTypeBaseTypeChanged(
            IConventionEntityTypeBuilder entityTypeBuilder,
            IConventionEntityType? newBaseType,
            IConventionEntityType? oldBaseType,
            IConventionContext<IConventionEntityType> context)
        {
            if (entityTypeBuilder.Metadata.BaseType == newBaseType)
            {
                Process(entityTypeBuilder);
            }
        }

        private void Process(IConventionEntityTypeBuilder entityTypeBuilder)
        {
            var entityType = entityTypeBuilder.Metadata;
            var candidates = entityType.GetRuntimeProperties().Values;

            foreach (var propertyInfo in candidates)
            {
                var name = propertyInfo.GetSimpleMemberName();
                if (entityTypeBuilder.IsIgnored(name)
                    || entityType.FindProperty(propertyInfo) != null
                    || entityType.FindNavigation(propertyInfo) != null
                    || !propertyInfo.IsCandidateProperty(publicOnly: false)
                    || (propertyInfo.IsCandidateProperty()
                        && Dependencies.TypeMappingSource.FindMapping(propertyInfo) != null))
                {
                    continue;
                }

                var factory = Dependencies.ParameterBindingFactories.FindFactory(propertyInfo.PropertyType, name);
                if (factory == null)
                {
                    continue;
                }

                var duplicateMap = GetDuplicateServiceProperties(entityType);
                if (duplicateMap != null
                    && duplicateMap.TryGetValue(propertyInfo.PropertyType, out var duplicateServiceProperties))
                {
                    duplicateServiceProperties.Add(name);

                    return;
                }

                var otherServicePropertySameType = entityType.GetServiceProperties()
                    .FirstOrDefault(p => p.ClrType == propertyInfo.PropertyType);
                if (otherServicePropertySameType != null
                    && otherServicePropertySameType.Name != name)
                {
                    if (ConfigurationSource.Convention.Overrides(otherServicePropertySameType.GetConfigurationSource()))
                    {
                        otherServicePropertySameType.DeclaringEntityType.RemoveServiceProperty(otherServicePropertySameType.Name);
                    }

                    AddDuplicateServiceProperty(entityTypeBuilder, propertyInfo.PropertyType, name);
                    AddDuplicateServiceProperty(entityTypeBuilder, propertyInfo.PropertyType, otherServicePropertySameType.Name);

                    return;
                }

                entityTypeBuilder.ServiceProperty(propertyInfo)?.HasParameterBinding(
                    (ServiceParameterBinding)factory.Bind(entityType, propertyInfo.PropertyType, name));
            }
        }

        /// <summary>
        ///     Called after an entity type member is ignored.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type. </param>
        /// <param name="name"> The name of the ignored member. </param>
        /// <param name="context"> Additional information associated with convention execution. </param>
        public virtual void ProcessEntityTypeMemberIgnored(
            IConventionEntityTypeBuilder entityTypeBuilder,
            string name,
            IConventionContext<string> context)
        {
            var entityType = entityTypeBuilder.Metadata;
            var duplicateMap = GetDuplicateServiceProperties(entityType);
            if (duplicateMap == null)
            {
                return;
            }

            var member = (MemberInfo?)entityType.GetRuntimeProperties().Find(name)
                ?? entityType.GetRuntimeFields().Find(name)!;
            var type = member.GetMemberType();
            if (duplicateMap.TryGetValue(type, out var duplicateServiceProperties)
                && duplicateServiceProperties.Remove(member.Name))
            {
                if (duplicateServiceProperties.Count != 1)
                {
                    return;
                }

                var otherName = duplicateServiceProperties.First();
                var otherMember = (MemberInfo?)entityType.GetRuntimeProperties().Find(otherName)
                    ?? entityType.GetRuntimeFields().Find(name)!;
                var factory = Dependencies.ParameterBindingFactories.FindFactory(type, otherName)!;
                entityType.Builder.ServiceProperty(otherMember)?.HasParameterBinding(
                    (ServiceParameterBinding)factory.Bind(entityType, type, otherName));
                duplicateMap.Remove(type);
                if (duplicateMap.Count == 0)
                {
                    SetDuplicateServiceProperties(entityType.Builder, null);
                }
            }
        }

        /// <inheritdoc />
        public virtual void ProcessModelFinalizing(
            IConventionModelBuilder modelBuilder,
            IConventionContext<IConventionModelBuilder> context)
        {
            foreach (var entityType in modelBuilder.Metadata.GetEntityTypes())
            {
                var duplicateMap = GetDuplicateServiceProperties(entityType);
                if (duplicateMap == null)
                {
                    continue;
                }

                foreach (var duplicateServiceProperties in duplicateMap)
                {
                    foreach (var duplicateServicePropertyName in duplicateServiceProperties.Value)
                    {
                        if (entityType.FindProperty(duplicateServicePropertyName) == null
                            && entityType.FindNavigation(duplicateServicePropertyName) == null)
                        {
                            throw new InvalidOperationException(
                                CoreStrings.AmbiguousServiceProperty(
                                    duplicateServicePropertyName,
                                    duplicateServiceProperties.Key.ShortDisplayName(),
                                    entityType.DisplayName()));
                        }
                    }
                }

                SetDuplicateServiceProperties(entityType.Builder, null);
            }
        }

        private static void AddDuplicateServiceProperty(
            IConventionEntityTypeBuilder entityTypeBuilder,
            Type propertyType,
            string propertyName)
        {
            var duplicateMap = GetDuplicateServiceProperties(entityTypeBuilder.Metadata)
                ?? new Dictionary<Type, HashSet<string>>(1);

            if (!duplicateMap.TryGetValue(propertyType, out var duplicateServiceProperties))
            {
                duplicateServiceProperties = new HashSet<string>();
                duplicateMap[propertyType] = duplicateServiceProperties;
            }

            duplicateServiceProperties.Add(propertyName);

            SetDuplicateServiceProperties(entityTypeBuilder, duplicateMap);
        }

        private static Dictionary<Type, HashSet<string>>? GetDuplicateServiceProperties(IConventionEntityType entityType)
            => entityType.FindAnnotation(CoreAnnotationNames.DuplicateServiceProperties)?.Value
                as Dictionary<Type, HashSet<string>>;

        private static void SetDuplicateServiceProperties(
            IConventionEntityTypeBuilder entityTypeBuilder,
            Dictionary<Type, HashSet<string>>? duplicateServiceProperties)
            => entityTypeBuilder.HasAnnotation(CoreAnnotationNames.DuplicateServiceProperties, duplicateServiceProperties);
    }
}
