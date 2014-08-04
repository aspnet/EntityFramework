// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata.ModelConventions;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata
{
    public class ModelBuilder
    {
        private readonly Model _model;

        public ModelBuilder()
        {
            _model = new Model();
        }

        public ModelBuilder([NotNull] Model model)
        {
            Check.NotNull(model, "model");

            _model = model;
        }

        // TODO: Consider whether this is needed/desirable; currently builder is not full-fidelity
        public virtual Model Model
        {
            get { return _model; }
        }

        public virtual EntityBuilder Entity([NotNull] string name)
        {
            Check.NotEmpty(name, "name");

            return new EntityBuilder(GetOrAddEntity(name), this);
        }

        public virtual EntityBuilder<T> Entity<T>()
        {
            return new EntityBuilder<T>(GetOrAddEntity(typeof(T)), this);
        }

        public virtual ModelBuilder Entity([NotNull] string name, [NotNull] Action<EntityBuilder> entityBuilder)
        {
            Check.NotEmpty(name, "name");
            Check.NotNull(entityBuilder, "entityBuilder");

            entityBuilder(Entity(name));

            return this;
        }

        public virtual ModelBuilder Entity<T>([NotNull] Action<EntityBuilder<T>> entityBuilder)
        {
            Check.NotNull(entityBuilder, "entityBuilder");

            entityBuilder(Entity<T>());

            return this;
        }

        internal EntityType GetOrAddEntity(string name)
        {
            var entityType = _model.TryGetEntityType(name);

            if (entityType == null)
            {
                _model.AddEntityType(entityType = new EntityType(name));
                OnEntityTypeAdded(entityType);
            }

            return entityType;
        }

        internal EntityType GetOrAddEntity(Type type)
        {
            var entityType = _model.TryGetEntityType(type);

            if (entityType == null)
            {
                _model.AddEntityType(entityType = new EntityType(type));
                OnEntityTypeAdded(entityType);
            }

            return entityType;
        }

        protected virtual void OnEntityTypeAdded([NotNull] EntityType entityType)
        {
        }

        public virtual ModelBuilder Annotation([NotNull] string annotation, [NotNull] string value)
        {
            Check.NotEmpty(annotation, "annotation");
            Check.NotEmpty(value, "value");

            _model[annotation] = value;

            return this;
        }

        public class MetadataBuilder<TMetadata, TMetadataBuilder>
            where TMetadata : MetadataBase
            where TMetadataBuilder : MetadataBuilder<TMetadata, TMetadataBuilder>
        {
            private readonly TMetadata _metadata;
            private readonly ModelBuilder _modelBuilder;

            internal MetadataBuilder(TMetadata metadata)
                : this(metadata, null)
            {
            }

            internal MetadataBuilder(TMetadata metadata, ModelBuilder modelBuilder)
            {
                _metadata = metadata;
                _modelBuilder = modelBuilder;
            }

            public TMetadataBuilder Annotation([NotNull] string annotation, [NotNull] string value)
            {
                Check.NotEmpty(annotation, "annotation");
                Check.NotEmpty(value, "value");

                _metadata[annotation] = value;

                return (TMetadataBuilder)this;
            }

            protected TMetadata Metadata
            {
                get { return _metadata; }
            }

            protected ModelBuilder ModelBuilder
            {
                get { return _modelBuilder; }
            }
        }

        public class EntityBuilderBase<TMetadataBuilder> : MetadataBuilder<EntityType, TMetadataBuilder>
            where TMetadataBuilder : MetadataBuilder<EntityType, TMetadataBuilder>
        {
            internal EntityBuilderBase(EntityType entityType, ModelBuilder modelBuilder)
                : base(entityType, modelBuilder)
            {
            }

            public KeyBuilder Key([NotNull] params string[] propertyNames)
            {
                Check.NotNull(propertyNames, "propertyNames");

                Metadata.SetKey(propertyNames.Select(n => Metadata.GetProperty(n)).ToArray());

                return new KeyBuilder(Metadata.GetKey());
            }

            public class KeyBuilder : MetadataBuilder<Key, KeyBuilder>
            {
                internal KeyBuilder(Key key)
                    : base(key)
                {
                }
            }

            public virtual PropertyBuilder Property<TProperty>(
                [NotNull] string name, bool shadowProperty = false, bool concurrencyToken = false)
            {
                Check.NotEmpty(name, "name");

                var property
                    = Metadata.TryGetProperty(name)
                      ?? Metadata.AddProperty(name, typeof(TProperty), shadowProperty, concurrencyToken);

                return new PropertyBuilder(property);
            }

            public class PropertyBuilder : MetadataBuilder<Property, PropertyBuilder>
            {
                internal PropertyBuilder(Property property)
                    : base(property)
                {
                }

                // TODO Consider if this should be relational only
                public PropertyBuilder UseStoreSequence()
                {
                    Metadata.ValueGenerationOnAdd = ValueGenerationOnAdd.Server;
                    Metadata.ValueGenerationOnSave = ValueGenerationOnSave.None;

                    return this;
                }

                // TODO Consider if this should be relational only
                public PropertyBuilder UseStoreSequence([NotNull] string sequenceName, int blockSize)
                {
                    Check.NotEmpty(sequenceName, "sequenceName");

                    // TODO: Make these constants in some class once decided if this should be relational-only
                    Metadata["StoreSequenceName"] = sequenceName;
                    Metadata["StoreSequenceBlockSize"] = blockSize.ToString();

                    return UseStoreSequence();
                }
            }

            public EntityBuilderBase<TMetadataBuilder> ForeignKeys([NotNull] Action<ForeignKeysBuilder> foreignKeysBuilder)
            {
                Check.NotNull(foreignKeysBuilder, "foreignKeysBuilder");

                foreignKeysBuilder(new ForeignKeysBuilder(Metadata, ModelBuilder));

                return this;
            }

            public class ForeignKeysBuilder
            {
                private readonly EntityType _entityType;
                private readonly ModelBuilder _modelBuilder;

                internal ForeignKeysBuilder(EntityType entityType, ModelBuilder modelBuilder)
                {
                    _entityType = entityType;
                    _modelBuilder = modelBuilder;
                }

                protected EntityType EntityType
                {
                    get { return _entityType; }
                }

                protected ModelBuilder ModelBuilder
                {
                    get { return _modelBuilder; }
                }

                public ForeignKeyBuilder ForeignKey([NotNull] string referencedEntityTypeName, [NotNull] params string[] propertyNames)
                {
                    Check.NotNull(referencedEntityTypeName, "referencedEntityTypeName");
                    Check.NotNull(propertyNames, "propertyNames");

                    var principalType = _modelBuilder._model.GetEntityType(referencedEntityTypeName);
                    var dependentProperties = propertyNames.Select(n => _entityType.GetProperty(n)).ToArray();

                    // TODO: This code currently assumes that the FK maps to a PK on the principal end
                    var foreignKey = _entityType.AddForeignKey(principalType.GetKey(), dependentProperties);

                    return new ForeignKeyBuilder(foreignKey);
                }

                public class ForeignKeyBuilder : MetadataBuilder<ForeignKey, ForeignKeyBuilder>
                {
                    internal ForeignKeyBuilder(ForeignKey foreignKey)
                        : base(foreignKey)
                    {
                    }

                    public ForeignKeyBuilder IsUnique()
                    {
                        Metadata.IsUnique = true;

                        return this;
                    }
                }
            }

            public EntityBuilderBase<TMetadataBuilder> Indexes([NotNull] Action<IndexesBuilder> indexesBuilder)
            {
                Check.NotNull(indexesBuilder, "indexesBuilder");

                indexesBuilder(new IndexesBuilder(Metadata));

                return this;
            }

            public class IndexesBuilder
            {
                private readonly EntityType _entityType;

                internal IndexesBuilder(EntityType entityType)
                {
                    _entityType = entityType;
                }

                protected EntityType EntityType
                {
                    get { return _entityType; }
                }

                public IndexBuilder Index([NotNull] params string[] propertyNames)
                {
                    Check.NotNull(propertyNames, "propertyNames");

                    var properties = propertyNames.Select(n => _entityType.GetProperty(n)).ToArray();
                    var index = _entityType.AddIndex(properties);

                    return new IndexBuilder(index);
                }

                public class IndexBuilder : MetadataBuilder<Index, IndexBuilder>
                {
                    internal IndexBuilder(Index index)
                        : base(index)
                    {
                    }

                    public IndexBuilder IsUnique()
                    {
                        Metadata.IsUnique = true;

                        return this;
                    }
                }
            }
        }

        public class EntityBuilder : EntityBuilderBase<EntityBuilder>
        {
            internal EntityBuilder(EntityType entityType, ModelBuilder modelBuilder)
                : base(entityType, modelBuilder)
            {
            }
        }

        public class EntityBuilder<TEntity> : EntityBuilderBase<EntityBuilder<TEntity>>
        {
            internal EntityBuilder(EntityType entityType, ModelBuilder modelBuilder)
                : base(entityType, modelBuilder)
            {
            }

            public KeyBuilder Key([NotNull] Expression<Func<TEntity, object>> keyExpression)
            {
                Check.NotNull(keyExpression, "keyExpression");

                Metadata.SetKey(
                    keyExpression.GetPropertyAccessList()
                        .Select(pi => Metadata.TryGetProperty(pi.Name)
                                      ?? Metadata.AddProperty(pi))
                        .ToArray());

                return new KeyBuilder(Metadata.GetKey());
            }

            public virtual PropertyBuilder Property([NotNull] Expression<Func<TEntity, object>> propertyExpression)
            {
                var propertyInfo = propertyExpression.GetPropertyAccess();

                var property
                    = Metadata.TryGetProperty(propertyInfo.Name)
                      ?? Metadata.AddProperty(propertyInfo);

                return new PropertyBuilder(property);
            }

            public EntityBuilder<TEntity> ForeignKeys([NotNull] Action<ForeignKeysBuilder> foreignKeysBuilder)
            {
                Check.NotNull(foreignKeysBuilder, "foreignKeysBuilder");

                foreignKeysBuilder(new ForeignKeysBuilder(Metadata, ModelBuilder));

                return this;
            }

            public new class ForeignKeysBuilder : EntityBuilderBase<EntityBuilder<TEntity>>.ForeignKeysBuilder
            {
                internal ForeignKeysBuilder(EntityType entityType, ModelBuilder modelBuilder)
                    : base(entityType, modelBuilder)
                {
                }

                public ForeignKeyBuilder ForeignKey<TReferencedEntityType>(
                    [NotNull] Expression<Func<TEntity, object>> foreignKeyExpression, bool isUnique = false)
                {
                    Check.NotNull(foreignKeyExpression, "foreignKeyExpression");

                    var principalType = ModelBuilder.Entity<TReferencedEntityType>().Metadata;

                    var dependentProperties
                        = foreignKeyExpression.GetPropertyAccessList()
                            .Select(pi => EntityType.TryGetProperty(pi.Name) ?? EntityType.AddProperty(pi))
                            .ToArray();

                    // TODO: This code currently assumes that the FK maps to a PK on the principal end
                    var foreignKey = EntityType.AddForeignKey(principalType.GetKey(), dependentProperties);
                    foreignKey.IsUnique = isUnique;

                    return new ForeignKeyBuilder(foreignKey);
                }
            }

            public EntityBuilder<TEntity> Indexes([NotNull] Action<IndexesBuilder> indexesBuilder)
            {
                Check.NotNull(indexesBuilder, "indexesBuilder");

                indexesBuilder(new IndexesBuilder(Metadata));

                return this;
            }

            public new class IndexesBuilder : EntityBuilderBase<EntityBuilder<TEntity>>.IndexesBuilder
            {
                internal IndexesBuilder(EntityType entityType)
                    : base(entityType)
                {
                }

                public IndexBuilder Index([NotNull] Expression<Func<TEntity, object>> indexExpression)
                {
                    Check.NotNull(indexExpression, "indexExpression");

                    var properties
                        = indexExpression.GetPropertyAccessList()
                            .Select(pi => EntityType.TryGetProperty(pi.Name) ?? EntityType.AddProperty(pi))
                            .ToArray();
                    var index = EntityType.AddIndex(properties);

                    return new IndexBuilder(index);
                }
            }

            public OneToManyBuilder OneToMany<TRelatedEntity>(
                [CanBeNull] Expression<Func<TEntity, IEnumerable<TRelatedEntity>>> navigationExpression = null,
                [CanBeNull] Expression<Func<TRelatedEntity, TEntity>> inverseNavigationExpression = null)
            {
                // TODO: Checking for bad/inconsistent FK/navigation/type configuration in this method and below

                var dependentType = ModelBuilder.Entity<TRelatedEntity>().Metadata;

                // Find either navigation that already exists
                var navNameToDependent = navigationExpression != null ? navigationExpression.GetPropertyAccess().Name : null;
                var navNameToPrincipal = inverseNavigationExpression != null ? inverseNavigationExpression.GetPropertyAccess().Name : null;

                var navToDependent = Metadata.Navigations.FirstOrDefault(e => e.Name == navNameToDependent);
                var navToPrincipal = dependentType.Navigations.FirstOrDefault(e => e.Name == navNameToPrincipal);

                // Find the associated FK on an already existing navigation, or create one by convention
                // TODO: If FK isn't already specified, then creating the navigation should cause it to be found/created
                // by convention, but this part of conventions is not done yet, so we do it here instead--kind of h.acky

                var foreignKey = navToDependent != null
                    ? navToDependent.ForeignKey
                    : navToPrincipal != null
                        ? navToPrincipal.ForeignKey
                        : new ForeignKeyConvention().FindOrCreateForeignKey(Metadata, dependentType, navNameToPrincipal);

                if (navNameToDependent != null
                    && navToDependent == null)
                {
                    Metadata.AddNavigation(new Navigation(foreignKey, navNameToDependent, pointsToPrincipal: false));
                }

                if (navNameToPrincipal != null
                    && navToPrincipal == null)
                {
                    dependentType.AddNavigation(new Navigation(foreignKey, navNameToPrincipal, pointsToPrincipal: true));
                }

                return new OneToManyBuilder(foreignKey, navToPrincipal, navToDependent);
            }

            public class OneToManyBuilder : MetadataBuilder<ForeignKey, OneToManyBuilder>
            {
                private readonly Navigation _navigationToPrincipal;
                private readonly Navigation _navigationToDependent;

                internal OneToManyBuilder(ForeignKey metadata, Navigation navigationToPrincipal, Navigation navigationToDependent)
                    : base(metadata)
                {
                    _navigationToPrincipal = navigationToPrincipal;
                    _navigationToDependent = navigationToDependent;
                }
            }
        }
    }
}
