// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     A base type for conventions that configure model aspects based on whether the member type
    ///     is a non-nullable reference type.
    /// </summary>
    public abstract class NonNullableConventionBase : IModelFinalizedConvention
    {
        // For the interpretation of nullability metadata, see
        // https://github.com/dotnet/roslyn/blob/master/docs/features/nullable-metadata.md

        private const string StateAnnotationName = "NonNullableConventionState";
        private const string NullableAttributeFullName = "System.Runtime.CompilerServices.NullableAttribute";
        private const string NullableContextAttributeFullName = "System.Runtime.CompilerServices.NullableContextAttribute";

        /// <summary>
        ///     Creates a new instance of <see cref="NonNullableConventionBase" />.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this convention. </param>
        protected NonNullableConventionBase([NotNull] ProviderConventionSetBuilderDependencies dependencies)
        {
            Dependencies = dependencies;
        }

        /// <summary>
        ///     Parameter object containing service dependencies.
        /// </summary>
        protected virtual ProviderConventionSetBuilderDependencies Dependencies { get; }

        private byte? GetNullabilityContextFlag(NonNullabilityConventionState state, Attribute[] attributes)
        {
            if (attributes.FirstOrDefault(a => a.GetType().FullName == NullableContextAttributeFullName) is Attribute attribute)
            {
                var attributeType = attribute.GetType();

                if (attributeType != state.NullableContextAttrType)
                {
                    state.NullableContextFlagFieldInfo = attributeType.GetField("Flag");
                    state.NullableContextAttrType = attributeType;
                }

                if (state.NullableContextFlagFieldInfo?.GetValue(attribute) is byte flag)
                {
                    return flag;
                }
            }

            return null;
        }

        /// <summary>
        ///     Returns a value indicating whether the member type is a non-nullable reference type.
        /// </summary>
        /// <param name="modelBuilder"> The model builder used to build the model. </param>
        /// <param name="memberInfo"> The member info. </param>
        /// <returns> <c>true</c> if the member type is a non-nullable reference type. </returns>
        protected virtual bool IsNonNullableRefType(
            [NotNull] IConventionModelBuilder modelBuilder,
            [NotNull] MemberInfo memberInfo)
        {
            if (memberInfo.GetMemberType().IsValueType)
            {
                return false;
            }

            var state = GetOrInitializeState(modelBuilder);

            // For C# 8.0 nullable types, the C# currently synthesizes a NullableAttribute that expresses nullability into assemblies
            // it produces. If the model is spread across more than one assembly, there will be multiple versions of this attribute,
            // so look for it by name, caching to avoid reflection on every check.
            // Note that this may change - if https://github.com/dotnet/corefx/issues/36222 is done we can remove all of this.

            // First look for NullableAttribute on the member itself
            if (Attribute.GetCustomAttributes(memberInfo)
                    .FirstOrDefault(a => a.GetType().FullName == NullableAttributeFullName) is Attribute attribute)
            {
                var attributeType = attribute.GetType();

                if (attributeType != state.NullableAttrType)
                {
                    state.NullableFlagsFieldInfo = attributeType.GetField("NullableFlags");
                    state.NullableAttrType = attributeType;
                }

                if (state.NullableFlagsFieldInfo?.GetValue(attribute) is byte[] flags)
                {
                    return flags.FirstOrDefault() == 1;
                }
            }

            // No attribute on the member, try to find a NullableContextAttribute on the declaring type
            var type = memberInfo.DeclaringType;
            if (type != null)
            {
                if (state.TypeNonNullabilityContextCache.TryGetValue(type, out var cachedTypeNonNullable))
                {
                    return cachedTypeNonNullable;
                }

                var typeContextFlag = GetNullabilityContextFlag(state, Attribute.GetCustomAttributes(type));
                if (typeContextFlag.HasValue)
                {
                    return state.TypeNonNullabilityContextCache[type] = typeContextFlag.Value == 1;
                }
            }

            // Not found at the type level, try at the module level
            var module = memberInfo.Module;
            if (!state.ModuleNonNullabilityContextCache.TryGetValue(module, out var moduleNonNullable))
            {
                var moduleContextFlag = GetNullabilityContextFlag(state, Attribute.GetCustomAttributes(memberInfo.Module));
                moduleNonNullable = state.ModuleNonNullabilityContextCache[module] =
                    moduleContextFlag.HasValue && moduleContextFlag == 1;
            }

            if (type != null)
            {
                state.TypeNonNullabilityContextCache[type] = moduleNonNullable;
            }

            return moduleNonNullable;
        }

        private NonNullabilityConventionState GetOrInitializeState(IConventionModelBuilder modelBuilder)
            => (NonNullabilityConventionState)(
                modelBuilder.Metadata.FindAnnotation(StateAnnotationName) ??
                modelBuilder.Metadata.AddAnnotation(StateAnnotationName, new NonNullabilityConventionState())
            ).Value;

        /// <summary>
        ///     Called after a model is finalized. Removes the cached state annotation used by this convention.
        /// </summary>
        /// <param name="modelBuilder"> The builder for the model. </param>
        /// <param name="context"> Additional information associated with convention execution. </param>
        public virtual void ProcessModelFinalized(IConventionModelBuilder modelBuilder, IConventionContext<IConventionModelBuilder> context)
            => modelBuilder.Metadata.RemoveAnnotation(StateAnnotationName);

        private class NonNullabilityConventionState
        {
            public Type NullableAttrType;
            public Type NullableContextAttrType;
            public FieldInfo NullableFlagsFieldInfo;
            public FieldInfo NullableContextFlagFieldInfo;
            public Dictionary<Type, bool> TypeNonNullabilityContextCache { get; } = new Dictionary<Type, bool>();
            public Dictionary<Module, bool> ModuleNonNullabilityContextCache { get; } = new Dictionary<Module, bool>();
        }
    }
}
