// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Migrations.Operations;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Migrations.Infrastructure
{
    // TODO: Structural matching
    public class ModelDiffer : IModelDiffer
    {
        private static readonly Type[] _dropOperationTypes =
        {
            typeof(DropIndexOperation),
            typeof(DropPrimaryKeyOperation),
            typeof(DropSequenceOperation),
            typeof(DropUniqueConstraintOperation)
        };

        private static readonly Type[] _alterOperationTypes =
        {
            typeof(AddColumnOperation),
            typeof(AddForeignKeyOperation),
            typeof(AddPrimaryKeyOperation),
            typeof(AddUniqueConstraintOperation),
            typeof(AlterColumnOperation),
            typeof(AlterSequenceOperation),
            typeof(CreateIndexOperation)
        };

        private static readonly Type[] _renameOperationTypes =
        {
            typeof(RenameColumnOperation),
            typeof(RenameSequenceOperation)
        };

        public ModelDiffer(
            [NotNull] IRelationalTypeMapper typeMapper,
            [NotNull] IRelationalMetadataExtensionProvider metadataExtensions,
            [NotNull] IMigrationAnnotationProvider annotations)
        {
            Check.NotNull(typeMapper, nameof(typeMapper));
            Check.NotNull(metadataExtensions, nameof(metadataExtensions));
            Check.NotNull(annotations, nameof(annotations));

            TypeMapper = typeMapper;
            MetadataExtensions = metadataExtensions;
            Annotations = annotations;
        }

        protected virtual IRelationalTypeMapper TypeMapper { get; }
        protected virtual IRelationalMetadataExtensionProvider MetadataExtensions { get; }
        protected virtual IMigrationAnnotationProvider Annotations { get; }

        public virtual bool HasDifferences(IModel source, [CanBeNull] IModel target)
            => Diff(source, target, new ModelDifferContext()).Any();

        public virtual IReadOnlyList<MigrationOperation> GetDifferences(IModel source, IModel target)
        {
            var diffContext = new ModelDifferContext();

            return Sort(Diff(source, target, diffContext), diffContext);
        }

        protected virtual IReadOnlyList<MigrationOperation> Sort(
            [NotNull] IEnumerable<MigrationOperation> operations,
            [NotNull] ModelDifferContext diffContext)
        {
            Check.NotNull(operations, nameof(operations));

            var dropForeignKeyOperations = new List<MigrationOperation>();
            var dropOperations = new List<MigrationOperation>();
            var dropColumnOperations = new List<MigrationOperation>();
            var dropTableOperations = new List<DropTableOperation>();
            var createSchemaOperations = new List<MigrationOperation>();
            var createSequenceOperations = new List<MigrationOperation>();
            var createTableOperations = new List<CreateTableOperation>();
            var alterOperations = new List<MigrationOperation>();
            var renameOperations = new List<MigrationOperation>();
            var renameIndexOperations = new List<MigrationOperation>();
            var renameTableOperations = new List<MigrationOperation>();
            var leftovers = new List<MigrationOperation>();

            foreach (var operation in operations)
            {
                var type = operation.GetType();
                if (type == typeof(DropForeignKeyOperation))
                {
                    dropForeignKeyOperations.Add(operation);
                }
                else if (_dropOperationTypes.Contains(type))
                {
                    dropOperations.Add(operation);
                }
                else if (type == typeof(DropColumnOperation))
                {
                    dropColumnOperations.Add(operation);
                }
                else if (type == typeof(DropTableOperation))
                {
                    dropTableOperations.Add((DropTableOperation)operation);
                }
                else if (type == typeof(CreateSchemaOperation))
                {
                    createSchemaOperations.Add(operation);
                }
                else if (type == typeof(CreateSequenceOperation))
                {
                    createSequenceOperations.Add(operation);
                }
                else if (type == typeof(CreateTableOperation))
                {
                    createTableOperations.Add((CreateTableOperation)operation);
                }
                else if (_alterOperationTypes.Contains(type))
                {
                    alterOperations.Add(operation);
                }
                else if (_renameOperationTypes.Contains(type))
                {
                    renameOperations.Add(operation);
                }
                else if (type == typeof(RenameIndexOperation))
                {
                    renameIndexOperations.Add(operation);
                }
                else if (type == typeof(RenameTableOperation))
                {
                    renameTableOperations.Add(operation);
                }
                else
                {
                    Debug.Assert(false, "Unexpected operation type: " + operation.GetType());
                    leftovers.Add(operation);
                }
            }

            var createTableGraph = new Multigraph<CreateTableOperation, AddForeignKeyOperation>();
            createTableGraph.AddVertices(createTableOperations);
            foreach (var createTableOperation in createTableOperations)
            {
                foreach (var addForeignKeyOperation in createTableOperation.ForeignKeys)
                {
                    if (addForeignKeyOperation.Table == addForeignKeyOperation.ReferencedTable
                        && addForeignKeyOperation.Schema == addForeignKeyOperation.ReferencedSchema)
                    {
                        continue;
                    }

                    var principalCreateTableOperation = createTableOperations.FirstOrDefault(
                        o => o.Name == addForeignKeyOperation.ReferencedTable
                            && o.Schema == addForeignKeyOperation.ReferencedSchema);
                    if (principalCreateTableOperation != null)
                    {
                        createTableGraph.AddEdge(principalCreateTableOperation, createTableOperation, addForeignKeyOperation);
                    }
                }
            }
            createTableOperations = createTableGraph.TopologicalSort(
                    (principalCreateTableOperation, createTableOperation, addForeignKeyOperations) =>
                    {
                        foreach (var addForeignKeyOperation in addForeignKeyOperations)
                        {
                            createTableOperation.ForeignKeys.Remove(addForeignKeyOperation);
                            alterOperations.Add(addForeignKeyOperation);
                        }

                        return true;
                    }).ToList();

            var dropTableGraph = new Multigraph<DropTableOperation, IForeignKey>();
            dropTableGraph.AddVertices(dropTableOperations);
            foreach (var dropTableOperation in dropTableOperations)
            {
                var entityType = diffContext.GetMetadata(dropTableOperation);
                foreach (var foreignKey in entityType.GetForeignKeysInHierarchy())
                {
                    var principalRootEntityType = foreignKey.PrincipalEntityType.RootType();
                    if (entityType == principalRootEntityType)
                    {
                        continue;
                    }

                    var principalDropTableOperation = diffContext.FindDrop(principalRootEntityType);
                    if (principalDropTableOperation != null)
                    {
                        dropTableGraph.AddEdge(dropTableOperation, principalDropTableOperation, foreignKey);
                    }
                }
            }
            var newDiffContext = new ModelDifferContext();
            dropTableOperations = dropTableGraph.TopologicalSort(
                (dropTableOperation, principalDropTableOperation, foreignKeys) =>
                {
                    dropForeignKeyOperations.AddRange(foreignKeys.SelectMany(c => Remove(c, newDiffContext)));

                    return true;
                }).ToList();

            return dropForeignKeyOperations
                .Concat(dropOperations)
                .Concat(dropColumnOperations)
                .Concat(dropTableOperations)
                .Concat(createSchemaOperations)
                .Concat(createSequenceOperations)
                .Concat(createTableOperations)
                .Concat(alterOperations)
                .Concat(renameOperations)
                .Concat(renameIndexOperations)
                .Concat(renameTableOperations)
                .Concat(leftovers)
                .ToArray();
        }

        #region IModel

        protected virtual IEnumerable<MigrationOperation> Diff(
            [CanBeNull] IModel source,
            [CanBeNull] IModel target,
            [NotNull] ModelDifferContext diffContext)
            => source != null && target != null
                ? Diff(GetSchemas(source), GetSchemas(target))
                    .Concat(Diff(source.GetRootEntityTypes(), target.GetRootEntityTypes(), diffContext))
                    .Concat(
                        Diff(MetadataExtensions.GetAnnotations(source).Sequences, MetadataExtensions.GetAnnotations(target).Sequences))
                    .Concat(
                        Diff(
                            source.EntityTypes.SelectMany(t => t.GetForeignKeys()),
                            target.EntityTypes.SelectMany(t => t.GetForeignKeys()),
                            diffContext))
                : target != null
                    ? Add(target, diffContext)
                    : source != null
                        ? Remove(source, diffContext)
                        : Enumerable.Empty<MigrationOperation>();

        protected virtual IEnumerable<MigrationOperation> Add(IModel target, ModelDifferContext diffContext)
            => GetSchemas(target).SelectMany(Add)
                .Concat(target.GetRootEntityTypes().SelectMany(t => Add(t, diffContext)))
                .Concat(MetadataExtensions.GetAnnotations(target).Sequences.SelectMany(Add))
                .Concat(target.EntityTypes.SelectMany(t => t.GetForeignKeys()).SelectMany(k => Add(k, diffContext)));

        protected virtual IEnumerable<MigrationOperation> Remove(IModel source, ModelDifferContext diffContext) =>
            source.GetRootEntityTypes().SelectMany(t => Remove(t, diffContext))
                .Concat(MetadataExtensions.GetAnnotations(source).Sequences.SelectMany(Remove));

        #endregion

        #region Schema

        protected virtual IEnumerable<MigrationOperation> Diff(IEnumerable<string> source, IEnumerable<string> target)
            => DiffCollection(
                source, target,
                Diff, Add, Remove,
                (s, t) => s == t);

        protected virtual IEnumerable<MigrationOperation> Diff(string source, string target)
            => Enumerable.Empty<MigrationOperation>();

        protected virtual IEnumerable<MigrationOperation> Add(string target)
        {
            yield return new CreateSchemaOperation { Name = target };
        }

        protected virtual IEnumerable<MigrationOperation> Remove(string source) => Enumerable.Empty<MigrationOperation>();

        #endregion

        #region IEntityType

        protected virtual IEnumerable<MigrationOperation> Diff(
            IEnumerable<IEntityType> source,
            IEnumerable<IEntityType> target,
            ModelDifferContext diffContext)
            => DiffCollection(
                source,
                target,
                (s, t) => Diff(s, t, diffContext),
                t => Add(t, diffContext),
                s => Remove(s, diffContext),
                (s, t) => string.Equals(s.Name, t.Name, StringComparison.OrdinalIgnoreCase),
                (s, t) => string.Equals(
                    MetadataExtensions.GetAnnotations(s).Schema,
                    MetadataExtensions.GetAnnotations(t).Schema,
                    StringComparison.OrdinalIgnoreCase)
                          && string.Equals(
                              MetadataExtensions.GetAnnotations(s).TableName,
                              MetadataExtensions.GetAnnotations(t).TableName,
                              StringComparison.OrdinalIgnoreCase),
                (s, t) => string.Equals(
                    MetadataExtensions.GetAnnotations(s).TableName,
                    MetadataExtensions.GetAnnotations(t).TableName,
                    StringComparison.OrdinalIgnoreCase));

        protected virtual IEnumerable<MigrationOperation> Diff(
            IEntityType source,
            IEntityType target,
            ModelDifferContext diffContext)
        {
            var sourceExtensions = MetadataExtensions.GetAnnotations(source);
            var targetExtensions = MetadataExtensions.GetAnnotations(target);

            var schemaChanged = sourceExtensions.Schema != targetExtensions.Schema;
            var renamed = sourceExtensions.TableName != targetExtensions.TableName;
            if (schemaChanged || renamed)
            {
                yield return new RenameTableOperation
                {
                    Schema = sourceExtensions.Schema,
                    Name = sourceExtensions.TableName,
                    NewSchema = schemaChanged ? targetExtensions.Schema : null,
                    NewName = renamed ? targetExtensions.TableName : null
                };
            }

            diffContext.AddMapping(source, target);

            var operations = Diff(source.GetPropertiesInHierarchy(), target.GetPropertiesInHierarchy(), diffContext)
                .Concat(Diff(source.GetKeys(), target.GetKeys(), diffContext))
                .Concat(Diff(source.GetIndexesInHierarchy(), target.GetIndexesInHierarchy(), diffContext));
            foreach (var operation in operations)
            {
                yield return operation;
            }
        }

        protected virtual IEnumerable<MigrationOperation> Add(IEntityType target, ModelDifferContext diffContext)
        {
            var targetExtensions = MetadataExtensions.GetAnnotations(target);

            var createTableOperation = new CreateTableOperation
            {
                Schema = targetExtensions.Schema,
                Name = targetExtensions.TableName
            };
            CopyAnnotations(Annotations.For(target), createTableOperation);

            createTableOperation.Columns.AddRange(target.GetPropertiesInHierarchy().SelectMany(p => Add(p, inline: true)).Cast<AddColumnOperation>());
            var primaryKey = target.GetPrimaryKey();
            createTableOperation.PrimaryKey = Add(primaryKey).Cast<AddPrimaryKeyOperation>().Single();
            createTableOperation.UniqueConstraints.AddRange(
                target.GetKeys().Where(k => k != primaryKey).SelectMany(Add).Cast<AddUniqueConstraintOperation>());

            diffContext.AddCreate(target, createTableOperation);

            yield return createTableOperation;

            foreach (var operation in target.GetIndexesInHierarchy().SelectMany(Add))
            {
                yield return operation;
            }
        }

        protected virtual IEnumerable<MigrationOperation> Remove(IEntityType source, ModelDifferContext diffContext)
        {
            var sourceExtensions = MetadataExtensions.GetAnnotations(source);

            var operation = new DropTableOperation
            {
                Schema = sourceExtensions.Schema,
                Name = sourceExtensions.TableName
            };
            diffContext.AddDrop(source, operation);

            yield return operation;
        }

        #endregion

        #region IProperty

        protected virtual IEnumerable<MigrationOperation> Diff(
            IEnumerable<IProperty> source,
            IEnumerable<IProperty> target,
            ModelDifferContext diffContext)
            => DiffCollection(
                source,
                target,
                (s, t) =>
                {
                    diffContext.AddMapping(s, t);

                    return Diff(s, t);
                },
                t => Add(t),
                Remove,
                (s, t) => string.Equals(s.Name, t.Name, StringComparison.OrdinalIgnoreCase),
                (s, t) => string.Equals(
                    MetadataExtensions.GetAnnotations(s).ColumnName,
                    MetadataExtensions.GetAnnotations(t).ColumnName,
                    StringComparison.OrdinalIgnoreCase));

        protected virtual IEnumerable<MigrationOperation> Diff(IProperty source, IProperty target)
        {
            var sourceExtensions = MetadataExtensions.GetAnnotations(source);
            var sourceEntityTypeExtensions = MetadataExtensions.GetAnnotations(source.DeclaringEntityType.RootType());
            var targetExtensions = MetadataExtensions.GetAnnotations(target);

            if (sourceExtensions.ColumnName != targetExtensions.ColumnName)
            {
                yield return new RenameColumnOperation
                {
                    Schema = sourceEntityTypeExtensions.Schema,
                    Table = sourceEntityTypeExtensions.TableName,
                    Name = sourceExtensions.ColumnName,
                    NewName = targetExtensions.ColumnName
                };
            }

            var sourceColumnType = sourceExtensions.ColumnType
                                   ?? TypeMapper.MapPropertyType(source).DefaultTypeName;

            var targetColumnType = targetExtensions.ColumnType
                                   ?? TypeMapper.MapPropertyType(target).DefaultTypeName;

            var targetAnnotations = Annotations.For(target);

            var isSourceColumnNullable = source.IsColumnNullable();
            var isTargetColumnNullable = target.IsColumnNullable();
            var isNullableChanged = isSourceColumnNullable != isTargetColumnNullable;
            var columnTypeChanged = sourceColumnType != targetColumnType;
            if (isNullableChanged
                || columnTypeChanged
                || sourceExtensions.GeneratedValueSql != targetExtensions.GeneratedValueSql
                || sourceExtensions.DefaultValue != targetExtensions.DefaultValue
                || HasDifferences(Annotations.For(source), targetAnnotations))
            {
                var isDestructiveChange = (isNullableChanged && isSourceColumnNullable)
                                          // TODO: Detect type narrowing
                                          || columnTypeChanged;

                var alterColumnOperation = new AlterColumnOperation
                {
                    Schema = sourceEntityTypeExtensions.Schema,
                    Table = sourceEntityTypeExtensions.TableName,
                    Name = sourceExtensions.ColumnName,
                    Type = targetColumnType,
                    IsNullable = isTargetColumnNullable,
                    DefaultValue = targetExtensions.DefaultValue,
                    DefaultValueSql = target.ValueGenerated == ValueGenerated.OnAdd ? targetExtensions.GeneratedValueSql : null,
                    ComputedColumnSql = target.ValueGenerated == ValueGenerated.OnAddOrUpdate ? targetExtensions.GeneratedValueSql : null,
                    IsDestructiveChange = isDestructiveChange
                };
                CopyAnnotations(targetAnnotations, alterColumnOperation);

                yield return alterColumnOperation;
            }
        }

        protected virtual IEnumerable<MigrationOperation> Add(IProperty target, bool inline = false)
        {
            var targetExtensions = MetadataExtensions.GetAnnotations(target);
            var targetEntityTypeExtensions = MetadataExtensions.GetAnnotations(target.DeclaringEntityType.RootType());

            var operation = new AddColumnOperation
            {
                Schema = targetEntityTypeExtensions.Schema,
                Table = targetEntityTypeExtensions.TableName,
                Name = targetExtensions.ColumnName,
                Type = targetExtensions.ColumnType ?? TypeMapper.MapPropertyType(target).DefaultTypeName,
                IsNullable = target.IsColumnNullable(),
                DefaultValue = targetExtensions.DefaultValue
                    ?? (inline || target.IsColumnNullable()
                        ? null
                        : GetDefaultValue(target.ClrType)),
                DefaultValueSql = target.ValueGenerated == ValueGenerated.OnAdd ? targetExtensions.GeneratedValueSql : null,
                ComputedColumnSql = target.ValueGenerated == ValueGenerated.OnAddOrUpdate ? targetExtensions.GeneratedValueSql : null,
            };
            CopyAnnotations(Annotations.For(target), operation);

            yield return operation;
        }

        protected virtual IEnumerable<MigrationOperation> Remove(IProperty source)
        {
            var sourceExtensions = MetadataExtensions.GetAnnotations(source);
            var sourceEntityTypeExtensions = MetadataExtensions.GetAnnotations(source.DeclaringEntityType.RootType());

            yield return new DropColumnOperation
            {
                Schema = sourceEntityTypeExtensions.Schema,
                Table = sourceEntityTypeExtensions.TableName,
                Name = sourceExtensions.ColumnName
            };
        }

        #endregion

        #region IKey

        protected virtual IEnumerable<MigrationOperation> Diff(
            IEnumerable<IKey> source,
            IEnumerable<IKey> target,
            ModelDifferContext diffContext)
            => DiffCollection(
                source, target,
                Diff, Add, Remove,
                (s, t) => MetadataExtensions.GetAnnotations(s).Name == MetadataExtensions.GetAnnotations(t).Name
                          && s.Properties.Select(diffContext.FindTarget).SequenceEqual(t.Properties)
                          && s.IsPrimaryKey() == t.IsPrimaryKey());

        protected virtual IEnumerable<MigrationOperation> Diff(IKey source, IKey target)
            => HasDifferences(Annotations.For(source), Annotations.For(target))
                ? Remove(source).Concat(Add(target))
                : Enumerable.Empty<MigrationOperation>();

        protected virtual IEnumerable<MigrationOperation> Add(IKey target)
        {
            var targetExtensions = MetadataExtensions.GetAnnotations(target);
            var targetEntityTypeExtensions = MetadataExtensions.GetAnnotations(target.EntityType.RootType());

            MigrationOperation operation;
            if (target.IsPrimaryKey())
            {
                operation = new AddPrimaryKeyOperation
                {
                    Schema = targetEntityTypeExtensions.Schema,
                    Table = targetEntityTypeExtensions.TableName,
                    Name = targetExtensions.Name,
                    Columns = GetColumnNames(target.Properties)
                };
            }
            else
            {
                operation = new AddUniqueConstraintOperation
                {
                    Schema = targetEntityTypeExtensions.Schema,
                    Table = targetEntityTypeExtensions.TableName,
                    Name = targetExtensions.Name,
                    Columns = GetColumnNames(target.Properties)
                };
            }
            CopyAnnotations(Annotations.For(target), operation);

            yield return operation;
        }

        protected virtual IEnumerable<MigrationOperation> Remove(IKey source)
        {
            var sourceExtensions = MetadataExtensions.GetAnnotations(source);
            var sourceEntityTypeExtensions = MetadataExtensions.GetAnnotations(source.EntityType.RootType());

            if (source.IsPrimaryKey())
            {
                yield return new DropPrimaryKeyOperation
                {
                    Schema = sourceEntityTypeExtensions.Schema,
                    Table = sourceEntityTypeExtensions.TableName,
                    Name = sourceExtensions.Name
                };
            }
            else
            {
                yield return new DropUniqueConstraintOperation
                {
                    Schema = sourceEntityTypeExtensions.Schema,
                    Table = sourceEntityTypeExtensions.TableName,
                    Name = sourceExtensions.Name
                };
            }
        }

        #endregion

        #region IForeignKey

        protected virtual IEnumerable<MigrationOperation> Diff(
            IEnumerable<IForeignKey> source,
            IEnumerable<IForeignKey> target,
            ModelDifferContext diffContext)
            => DiffCollection(
                source,
                target,
                (s, t) => Diff(s, t, diffContext),
                t => Add(t, diffContext),
                s => Remove(s, diffContext),
                (s, t) =>
                    {
                        return MetadataExtensions.GetAnnotations(s).Name == MetadataExtensions.GetAnnotations(t).Name
                               && s.Properties.Select(diffContext.FindTarget).SequenceEqual(t.Properties)
                               && diffContext.FindTarget(s.PrincipalEntityType.RootType()) == t.PrincipalEntityType.RootType()
                               && s.PrincipalKey.Properties.Select(diffContext.FindTarget).SequenceEqual(t.PrincipalKey.Properties);
                    });

        protected virtual IEnumerable<MigrationOperation> Diff(IForeignKey source, IForeignKey target, ModelDifferContext diffContext)
            => HasDifferences(Annotations.For(source), Annotations.For(target))
                ? Remove(source, diffContext).Concat(Add(target, diffContext))
                : Enumerable.Empty<MigrationOperation>();

        protected virtual IEnumerable<MigrationOperation> Add(IForeignKey target, ModelDifferContext diffContext)
        {
            var targetExtensions = MetadataExtensions.GetAnnotations(target);
            var declaringRootEntityType = target.DeclaringEntityType.RootType();
            var targetEntityTypeExtensions = MetadataExtensions.GetAnnotations(declaringRootEntityType);
            var targetPrincipalEntityTypeExtensions = MetadataExtensions.GetAnnotations(target.PrincipalEntityType.RootType());

            // TODO: Set OnDelete (See #1084)
            var operation = new AddForeignKeyOperation
            {
                Schema = targetEntityTypeExtensions.Schema,
                Table = targetEntityTypeExtensions.TableName,
                Name = targetExtensions.Name,
                Columns = GetColumnNames(target.Properties),
                ReferencedSchema = targetPrincipalEntityTypeExtensions.Schema,
                ReferencedTable = targetPrincipalEntityTypeExtensions.TableName,
                ReferencedColumns = GetColumnNames(target.PrincipalKey.Properties)
            };
            CopyAnnotations(Annotations.For(target), operation);

            var createTableOperation = diffContext.FindCreate(declaringRootEntityType);
            if (createTableOperation != null)
            {
                createTableOperation.ForeignKeys.Add(operation);
            }
            else
            {
                yield return operation;
            }
        }

        protected virtual IEnumerable<MigrationOperation> Remove(IForeignKey source, ModelDifferContext diffContext)
        {
            var sourceExtensions = MetadataExtensions.GetAnnotations(source);
            var declaringRootEntityType = source.DeclaringEntityType.RootType();
            var sourceEntityTypeExtensions = MetadataExtensions.GetAnnotations(declaringRootEntityType);

            var dropTableOperation = diffContext.FindDrop(declaringRootEntityType);
            if (dropTableOperation == null)
            {
                yield return new DropForeignKeyOperation
                {
                    Schema = sourceEntityTypeExtensions.Schema,
                    Table = sourceEntityTypeExtensions.TableName,
                    Name = sourceExtensions.Name
                };
            }
        }

        #endregion

        #region IIndex

        protected virtual IEnumerable<MigrationOperation> Diff(
            IEnumerable<IIndex> source,
            IEnumerable<IIndex> target,
            ModelDifferContext diffContext)
            => DiffCollection(
                source, target,
                Diff, Add, Remove,
                (s, t) => string.Equals(
                    MetadataExtensions.GetAnnotations(s).Name,
                    MetadataExtensions.GetAnnotations(t).Name,
                    StringComparison.OrdinalIgnoreCase)
                          && s.Properties.Select(diffContext.FindTarget).SequenceEqual(t.Properties),
                (s, t) => s.Properties.Select(diffContext.FindTarget).SequenceEqual(t.Properties));

        protected virtual IEnumerable<MigrationOperation> Diff(IIndex source, IIndex target)
        {
            var sourceEntityTypeExtensions = MetadataExtensions.GetAnnotations(source.DeclaringEntityType.RootType());
            var sourceName = MetadataExtensions.GetAnnotations(source).Name;
            var targetName = MetadataExtensions.GetAnnotations(target).Name;

            if (sourceName != targetName)
            {
                yield return new RenameIndexOperation
                {
                    Schema = sourceEntityTypeExtensions.Schema,
                    Table = sourceEntityTypeExtensions.TableName,
                    Name = sourceName,
                    NewName = targetName
                };
            }

            if (source.IsUnique != target.IsUnique
                || HasDifferences(Annotations.For(source), Annotations.For(target)))
            {
                var operations = Remove(source).Concat(Add(target));
                foreach (var operation in operations)
                {
                    yield return operation;
                }
            }
        }

        protected virtual IEnumerable<MigrationOperation> Add(IIndex target)
        {
            var targetExtensions = MetadataExtensions.GetAnnotations(target);
            var targetEntityTypeExtensions = MetadataExtensions.GetAnnotations(target.DeclaringEntityType.RootType());

            var operation = new CreateIndexOperation
            {
                Name = targetExtensions.Name,
                Schema = targetEntityTypeExtensions.Schema,
                Table = targetEntityTypeExtensions.TableName,
                Columns = GetColumnNames(target.Properties),
                IsUnique = target.IsUnique
            };
            CopyAnnotations(Annotations.For(target), operation);

            yield return operation;
        }

        protected virtual IEnumerable<MigrationOperation> Remove(IIndex source)
        {
            var sourceExtensions = MetadataExtensions.GetAnnotations(source);
            var sourceEntityTypeExtensions = MetadataExtensions.GetAnnotations(source.DeclaringEntityType.RootType());

            yield return new DropIndexOperation
            {
                Name = sourceExtensions.Name,
                Schema = sourceEntityTypeExtensions.Schema,
                Table = sourceEntityTypeExtensions.TableName
            };
        }

        #endregion

        #region ISequence

        protected virtual IEnumerable<MigrationOperation> Diff(IEnumerable<ISequence> source, IEnumerable<ISequence> target)
            => DiffCollection(
                source, target,
                Diff, Add, Remove,
                (s, t) => string.Equals(s.Schema, t.Schema, StringComparison.OrdinalIgnoreCase)
                          && string.Equals(s.Name, t.Name, StringComparison.OrdinalIgnoreCase)
                          && s.ClrType == t.ClrType,
                (s, t) => string.Equals(s.Name, t.Name, StringComparison.OrdinalIgnoreCase)
                          && s.ClrType == t.ClrType);

        protected virtual IEnumerable<MigrationOperation> Diff(ISequence source, ISequence target)
        {
            var schemaChanged = source.Schema != target.Schema;
            var renamed = source.Name != target.Name;
            if (schemaChanged || renamed)
            {
                yield return new RenameSequenceOperation
                {
                    Schema = source.Schema,
                    Name = source.Name,
                    NewSchema = schemaChanged ? target.Schema : null,
                    NewName = renamed ? target.Name : null
                };
            }

            if (source.IncrementBy != target.IncrementBy
                || source.MaxValue != target.MaxValue
                || source.MinValue != target.MinValue
                || source.IsCyclic != target.IsCyclic)
            {
                yield return new AlterSequenceOperation
                {
                    Schema = source.Schema,
                    Name = source.Name,
                    IncrementBy = target.IncrementBy,
                    MinValue = target.MinValue,
                    MaxValue = target.MaxValue,
                    Cycle = target.IsCyclic
                };
            }
        }

        protected virtual IEnumerable<MigrationOperation> Add(ISequence target)
        {
            yield return new CreateSequenceOperation
            {
                Schema = target.Schema,
                Name = target.Name,
                Type = TypeMapper.GetDefaultMapping(target.ClrType).DefaultTypeName,
                StartWith = target.StartValue,
                IncrementBy = target.IncrementBy,
                MinValue = target.MinValue,
                MaxValue = target.MaxValue,
                Cycle = target.IsCyclic
            };
        }

        protected virtual IEnumerable<MigrationOperation> Remove(ISequence source)
        {
            yield return new DropSequenceOperation
            {
                Schema = source.Schema,
                Name = source.Name
            };
        }

        #endregion

        protected virtual IEnumerable<MigrationOperation> DiffCollection<T>(
            IEnumerable<T> sources,
            IEnumerable<T> targets,
            Func<T, T, IEnumerable<MigrationOperation>> diff,
            Func<T, IEnumerable<MigrationOperation>> add,
            Func<T, IEnumerable<MigrationOperation>> remove,
            params Func<T, T, bool>[] predicates)
        {
            var sourceList = sources.ToList();
            var targetList = targets.ToList();

            foreach (var predicate in predicates)
            {
                for (var i = sourceList.Count - 1; i >= 0; i--)
                {
                    var source = sourceList[i];
                    var paired = false;

                    for (var j = targetList.Count - 1; j >= 0; j--)
                    {
                        var target = targetList[j];

                        if (predicate(source, target))
                        {
                            targetList.RemoveAt(j);
                            paired = true;

                            foreach (var operation in diff(source, target))
                            {
                                yield return operation;
                            }

                            break;
                        }
                    }

                    if (paired)
                    {
                        sourceList.RemoveAt(i);
                    }
                }
            }

            foreach (var source in sourceList)
            {
                foreach (var operation in remove(source))
                {
                    yield return operation;
                }
            }

            foreach (var target in targetList)
            {
                foreach (var operation in add(target))
                {
                    yield return operation;
                }
            }
        }

        protected virtual string[] GetColumnNames(IEnumerable<IProperty> properties)
            => properties.Select(p => MetadataExtensions.GetAnnotations(p).ColumnName).ToArray();

        protected virtual bool HasDifferences(IEnumerable<IAnnotation> source, IEnumerable<IAnnotation> target)
        {
            var unmatched = new List<IAnnotation>(target);

            foreach (var annotation in source)
            {
                var index = unmatched.FindIndex(a => a.Name == annotation.Name && a.Value == annotation.Value);
                if (index == -1)
                {
                    return true;
                }

                unmatched.RemoveAt(index);
            }

            return unmatched.Count != 0;
        }

        protected virtual void CopyAnnotations(IEnumerable<IAnnotation> annotations, Annotatable annotatable)
        {
            foreach (var annotation in annotations)
            {
                annotatable.AddAnnotation(annotation.Name, annotation.Value);
            }
        }

        protected virtual IEnumerable<string> GetSchemas(IModel model)
            => model.GetRootEntityTypes().Select(t => MetadataExtensions.GetAnnotations(t).Schema).Where(s => !string.IsNullOrEmpty(s))
                .Distinct();

        protected virtual object GetDefaultValue(Type type)
            => type == typeof(string)
                ? string.Empty
                : type.IsArray
                    ? Array.CreateInstance(type.GetElementType(), 0)
                    : type.UnwrapNullableType().GetDefaultValue();
    }
}
