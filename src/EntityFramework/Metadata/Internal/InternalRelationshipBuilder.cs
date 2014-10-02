// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata.ModelConventions;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata.Internal
{
    public class InternalRelationshipBuilder : InternalMetadataItemBuilder<ForeignKey>
    {
        private EntityType _principalType;
        private EntityType _dependentType;
        private Navigation _navigationToPrincipal;
        private Navigation _navigationToDependent;
        private IReadOnlyList<Property> _dependentProperties = ImmutableList<Property>.Empty;
        private IReadOnlyList<Property> _principalProperties = ImmutableList<Property>.Empty;

        public InternalRelationshipBuilder(
            [NotNull] ForeignKey foreignKey, [NotNull] InternalModelBuilder modelBuilder,
            [NotNull] EntityType principalType, [NotNull] EntityType dependentType,
            [CanBeNull] Navigation navigationToPrincipal, [CanBeNull] Navigation navigationToDependent)
            : base(foreignKey, modelBuilder)
        {
            Check.NotNull(principalType, "principalType");
            Check.NotNull(dependentType, "dependentType");

            _principalType = principalType;
            _dependentType = dependentType;
            _navigationToPrincipal = navigationToPrincipal;
            _navigationToDependent = navigationToDependent;
        }

        private InternalRelationshipBuilder(ForeignKey foreignKey, InternalRelationshipBuilder currentBuilder)
            : this(foreignKey, currentBuilder.ModelBuilder, currentBuilder._principalType, currentBuilder._dependentType,
                currentBuilder._navigationToPrincipal, currentBuilder._navigationToDependent)
        {
            _dependentProperties = currentBuilder._dependentProperties;
            _principalProperties = currentBuilder._principalProperties;
        }

        public virtual EntityType PrincipalType
        {
            get { return _principalType; }
        }

        public virtual EntityType DependentType
        {
            get { return _dependentType; }
        }

        public virtual void Required(bool required)
        {
            Metadata.IsRequired = required;
        }

        public virtual InternalRelationshipBuilder Invert()
        {
            var navigationToDependent = _navigationToDependent;
            _navigationToDependent = _navigationToPrincipal;
            _navigationToPrincipal = navigationToDependent;

            var dependentType = _dependentType;
            _dependentType = _principalType;
            _principalType = dependentType;

            var dependentProperties = _dependentProperties;
            _dependentProperties = _principalProperties;
            _principalProperties = dependentProperties;

            if (_navigationToDependent != null)
            {
                _navigationToDependent.PointsToPrincipal = false;
            }

            if (_navigationToPrincipal != null)
            {
                _navigationToPrincipal.PointsToPrincipal = true;
            }

            return this;
        }

        public virtual InternalRelationshipBuilder ForeignKey([NotNull] IList<PropertyInfo> propertyAccessList)
        {
            Check.NotNull(propertyAccessList, "propertyAccessList");

            return ForeignKey(propertyAccessList.Select(p => _dependentType.GetOrAddProperty(p)));
        }

        public virtual InternalRelationshipBuilder ForeignKey([NotNull] IReadOnlyList<string> propertyNames)
        {
            Check.NotNull(propertyNames, "propertyNames");

            return ForeignKey(propertyNames.Select(p => _dependentType.GetProperty(p)));
        }

        public virtual InternalRelationshipBuilder ForeignKey([NotNull] IEnumerable<Property> properties)
        {
            Check.NotNull(properties, "properties");

            _dependentProperties = properties.ToArray();

            if (Metadata.Properties.SequenceEqual(_dependentProperties))
            {
                return this;
            }

            return ReplaceForeignKey();
        }

        public virtual InternalRelationshipBuilder ReferencedKey([NotNull] IList<PropertyInfo> propertyAccessList)
        {
            Check.NotNull(propertyAccessList, "propertyAccessList");

            return ReferencedKey(propertyAccessList.Select(p => _principalType.GetOrAddProperty(p)));
        }

        public virtual InternalRelationshipBuilder ReferencedKey([NotNull] IReadOnlyList<string> propertyNames)
        {
            Check.NotNull(propertyNames, "propertyNames");

            return ReferencedKey(propertyNames.Select(p => _principalType.GetProperty(p)));
        }

        public virtual InternalRelationshipBuilder ReferencedKey([NotNull] IEnumerable<Property> properties)
        {
            Check.NotNull(properties, "properties");

            _principalProperties = properties.ToArray();

            if (Metadata.ReferencedProperties.SequenceEqual(_principalProperties))
            {
                return this;
            }

            return ReplaceForeignKey();
        }

        public virtual InternalRelationshipBuilder OneToOneForeignKey(
            [NotNull] Type specifiedDependentType,
            [NotNull] IList<PropertyInfo> propertyAccessList)
        {
            Check.NotNull(specifiedDependentType, "specifiedDependentType");
            Check.NotNull(propertyAccessList, "propertyAccessList");

            ForeignInvertIfNeeded(specifiedDependentType);

            return ForeignKey(propertyAccessList);
        }

        public virtual InternalRelationshipBuilder OneToOneForeignKey(
            [NotNull] Type specifiedDependentType,
            [NotNull] IReadOnlyList<string> propertyNames)
        {
            Check.NotNull(specifiedDependentType, "specifiedDependentType");
            Check.NotNull(propertyNames, "propertyNames");

            ForeignInvertIfNeeded(specifiedDependentType);

            return ForeignKey(propertyNames);
        }

        public virtual InternalRelationshipBuilder OneToOneForeignKey(
            [NotNull] string specifiedDependentTypeName,
            [NotNull] IReadOnlyList<string> propertyNames)
        {
            Check.NotNull(specifiedDependentTypeName, "specifiedDependentTypeName");
            Check.NotNull(propertyNames, "propertyNames");

            ForeignInvertIfNeeded(ModelBuilder.Metadata.GetEntityType(specifiedDependentTypeName));

            return ForeignKey(propertyNames);
        }

        private void ForeignInvertIfNeeded(Type specifiedDependentType)
        {
            ForeignInvertIfNeeded(ModelBuilder.GetOrAddEntity(specifiedDependentType).Metadata);
        }

        private void ForeignInvertIfNeeded(EntityType entityType)
        {
            if (entityType != DependentType)
            {
                Invert();
            }
        }

        public virtual InternalRelationshipBuilder OneToOneReferencedKey(
            [NotNull] Type specifiedPrincipalType,
            [NotNull] IReadOnlyList<string> propertyNames)
        {
            Check.NotNull(specifiedPrincipalType, "specifiedPrincipalType");
            Check.NotNull(propertyNames, "propertyNames");

            return ReferenceInvertIfNeeded(specifiedPrincipalType).ReferencedKey(propertyNames);
        }

        public virtual InternalRelationshipBuilder OneToOneReferencedKey(
            [NotNull] string specifiedPrincipalTypeName,
            [NotNull] IReadOnlyList<string> propertyNames)
        {
            Check.NotNull(specifiedPrincipalTypeName, "specifiedPrincipalTypeName");
            Check.NotNull(propertyNames, "propertyNames");

            return ReferenceInvertIfNeeded(ModelBuilder.Metadata.GetEntityType(specifiedPrincipalTypeName)).ReferencedKey(propertyNames);
        }


        public virtual InternalRelationshipBuilder OneToOneReferencedKey(
            [NotNull] Type specifiedPrincipalType,
            [NotNull] IList<PropertyInfo> propertyAccessList)
        {
            Check.NotNull(specifiedPrincipalType, "specifiedPrincipalType");
            Check.NotNull(propertyAccessList, "propertyAccessList");

            return ReferenceInvertIfNeeded(specifiedPrincipalType).ReferencedKey(propertyAccessList);
        }

        private InternalRelationshipBuilder ReferenceInvertIfNeeded(Type specifiedPrincipalType)
        {
            return ReferenceInvertIfNeeded(ModelBuilder.GetOrAddEntity(specifiedPrincipalType).Metadata);
        }

        private InternalRelationshipBuilder ReferenceInvertIfNeeded(EntityType entityType)
        {
            return entityType == PrincipalType
                ? this
                : Invert().ReplaceForeignKey();
        }

        private InternalRelationshipBuilder ReplaceForeignKey()
        {
            var newForeignKey = new ForeignKeyConvention().FindOrCreateForeignKey(
                _principalType,
                _dependentType,
                _navigationToPrincipal != null ? _navigationToPrincipal.Name : null,
                _navigationToDependent != null ? _navigationToDependent.Name : null,
                _dependentProperties,
                _principalProperties,
                Metadata.IsUnique);

            if (_navigationToPrincipal != null)
            {
                _dependentType.RemoveNavigation(_navigationToPrincipal);
            }

            if (_navigationToDependent != null)
            {
                _principalType.RemoveNavigation(_navigationToDependent);
            }

            var entityType = Metadata.EntityType;

            // TODO: Remove FK only if it was added by convention
            // Issue #213
            entityType.RemoveForeignKey(Metadata);

            // TODO: Remove principal key only if it was added by convention
            // Issue #213
            var currentPrincipalKey = Metadata.ReferencedKey;
            if (currentPrincipalKey != newForeignKey.ReferencedKey
                && currentPrincipalKey != currentPrincipalKey.EntityType.TryGetPrimaryKey()
                && currentPrincipalKey.Properties.All(p => p.IsShadowProperty))
            {
                currentPrincipalKey.EntityType.RemoveKey(currentPrincipalKey);
            }

            var propertiesInUse = entityType.Keys.SelectMany(k => k.Properties)
                .Concat(entityType.ForeignKeys.SelectMany(k => k.Properties))
                .Concat(Metadata.ReferencedEntityType.Keys.SelectMany(k => k.Properties))
                .Concat(Metadata.ReferencedEntityType.ForeignKeys.SelectMany(k => k.Properties))
                .Concat(_principalProperties)
                .Concat(_dependentProperties)
                .Where(p => p.IsShadowProperty)
                .Distinct();

            var propertiesToRemove = Metadata.Properties
                .Concat(Metadata.ReferencedKey.Properties)
                .Where(p => p.IsShadowProperty)
                .Distinct()
                .Except(propertiesInUse);

            // TODO: Remove property only if it was added by convention
            // Issue #213
            foreach (var property in propertiesToRemove)
            {
                property.EntityType.RemoveProperty(property);
            }

            if (_navigationToPrincipal != null)
            {
                _navigationToPrincipal.ForeignKey = newForeignKey;
                _dependentType.AddNavigation(_navigationToPrincipal);
            }

            if (_navigationToDependent != null)
            {
                _navigationToDependent.ForeignKey = newForeignKey;
                _principalType.AddNavigation(_navigationToDependent);
            }

            return new InternalRelationshipBuilder(newForeignKey, this);
        }
    }
}
