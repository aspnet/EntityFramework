// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.ChangeTracking.Internal
{
    // This is lower-level change tracking services used by the ChangeTracker and other parts of the system
    public class StateManager
    {
        private readonly Dictionary<object, InternalEntityEntry> _entityReferenceMap
            = new Dictionary<object, InternalEntityEntry>(ReferenceEqualityComparer.Instance);

        private readonly Dictionary<EntityKey, InternalEntityEntry> _identityMap = new Dictionary<EntityKey, InternalEntityEntry>();
        private readonly IEntityKeyFactorySource _keyFactorySource;
        private readonly InternalEntityEntryFactory _factory;
        private readonly InternalEntityEntrySubscriber _subscriber;
        private readonly IModel _model;
        private readonly IDataStore _dataStore;

        /// <summary>
        ///     This constructor is intended only for use when creating test doubles that will override members
        ///     with mocked or faked behavior. Use of this constructor for other purposes may result in unexpected
        ///     behavior including but not limited to throwing <see cref="NullReferenceException" />.
        /// </summary>
        protected StateManager()
        {
        }

        public StateManager(
            [NotNull] InternalEntityEntryFactory factory,
            [NotNull] IEntityKeyFactorySource entityKeyFactorySource,
            [NotNull] InternalEntityEntrySubscriber subscriber,
            [NotNull] InternalEntityEntryNotifier notifier,
            [NotNull] ValueGenerationManager valueGeneration,
            [NotNull] IModel model,
            [NotNull] IDataStore dataStore)
        {
            _keyFactorySource = entityKeyFactorySource;
            _factory = factory;
            _subscriber = subscriber;
            Notify = notifier;
            ValueGeneration = valueGeneration;
            _model = model;
            _dataStore = dataStore;
        }

        public virtual InternalEntityEntryNotifier Notify { get; }

        public virtual ValueGenerationManager ValueGeneration { get; }

        public virtual InternalEntityEntry CreateNewEntry([NotNull] IEntityType entityType)
        {
            // TODO: Consider entities without parameterless constructor--use o/c mapping info?
            // Issue #240
            var entity = entityType.HasClrType ? Activator.CreateInstance(entityType.Type) : null;

            return _subscriber.SnapshotAndSubscribe(_factory.Create(this, entityType, entity));
        }

        public virtual InternalEntityEntry GetOrCreateEntry([NotNull] object entity)
        {
            // TODO: Consider how to handle derived types that are not explicitly in the model
            // Issue #743
            InternalEntityEntry entry;
            if (!_entityReferenceMap.TryGetValue(entity, out entry))
            {
                var entityType = _model.GetEntityType(entity.GetType());

                entry = _subscriber.SnapshotAndSubscribe(_factory.Create(this, entityType, entity));

                _entityReferenceMap[entity] = entry;
            }
            return entry;
        }

        public virtual InternalEntityEntry StartTracking(
            [NotNull] IEntityType entityType, [NotNull] object entity, [NotNull] IValueReader valueReader)
        {
            // TODO: Perf: Pre-compute this for speed
            var keyProperties = entityType.GetPrimaryKey().Properties;
            var keyValue = _keyFactorySource.GetKeyFactory(keyProperties).Create(entityType, keyProperties, valueReader);

            if (keyValue == EntityKey.InvalidEntityKey)
            {
                throw new InvalidOperationException(Strings.InvalidPrimaryKey(entityType.Name));
            }

            var existingEntry = TryGetEntry(keyValue);

            if (existingEntry != null)
            {
                if (existingEntry.Entity != entity)
                {
                    throw new InvalidOperationException(Strings.IdentityConflict(entityType.Name));
                }

                return existingEntry;
            }

            var newEntry = _subscriber.SnapshotAndSubscribe(_factory.Create(this, entityType, entity, valueReader));

            _identityMap.Add(keyValue, newEntry);
            _entityReferenceMap[entity] = newEntry;

            newEntry.SetEntityState(EntityState.Unchanged);

            return newEntry;
        }

        public virtual InternalEntityEntry TryGetEntry([NotNull] EntityKey keyValue)
        {
            InternalEntityEntry entry;
            _identityMap.TryGetValue(keyValue, out entry);
            return entry;
        }

        public virtual InternalEntityEntry TryGetEntry([NotNull] object entity)
        {
            InternalEntityEntry entry;
            _entityReferenceMap.TryGetValue(entity, out entry);
            return entry;
        }

        public virtual IEnumerable<InternalEntityEntry> Entries => _identityMap.Values;

        public virtual InternalEntityEntry StartTracking([NotNull] InternalEntityEntry entry)
        {
            var entityType = entry.EntityType;

            if (entry.StateManager != this)
            {
                throw new InvalidOperationException(Strings.WrongStateManager(entityType.FullName));
            }

            InternalEntityEntry existingEntry;
            if (entry.Entity != null)
            {
                if (!_entityReferenceMap.TryGetValue(entry.Entity, out existingEntry))
                {
                    _entityReferenceMap[entry.Entity] = entry;
                }
                else if (existingEntry != entry)
                {
                    throw new InvalidOperationException(Strings.MultipleEntries(entityType.FullName));
                }
            }

            var keyValue = GetPrimaryKeyValueChecked(entry);

            if (_identityMap.TryGetValue(keyValue, out existingEntry))
            {
                if (existingEntry != entry)
                {
                    // TODO: Consider specialized exception types
                    // Issue #611
                    throw new InvalidOperationException(Strings.IdentityConflict(entityType.FullName));
                }
            }
            else
            {
                _identityMap[keyValue] = entry;
            }

            return entry;
        }

        public virtual void StopTracking([NotNull] InternalEntityEntry entry)
        {
            if (entry.Entity != null)
            {
                _entityReferenceMap.Remove(entry.Entity);
            }

            var keyValue = entry.GetPrimaryKeyValue();

            InternalEntityEntry existingEntry;
            if (_identityMap.TryGetValue(keyValue, out existingEntry)
                && existingEntry == entry)
            {
                _identityMap.Remove(keyValue);
            }
        }

        public virtual InternalEntityEntry GetPrincipal([NotNull] IPropertyAccessor dependentEntry, [NotNull] IForeignKey foreignKey)
        {
            var dependentKeyValue = dependentEntry.GetDependentKeyValue(foreignKey);

            if (dependentKeyValue == EntityKey.InvalidEntityKey)
            {
                return null;
            }

            var referencedEntityType = foreignKey.ReferencedEntityType;
            var referencedProperties = foreignKey.ReferencedProperties;

            // TODO: Perf: Add additional indexes so that this isn't a linear lookup
            var principals = Entries.Where(
                e => e.EntityType == referencedEntityType
                     && dependentKeyValue.Equals(
                         e.GetPrincipalKey(foreignKey, referencedEntityType, referencedProperties))).ToList();

            if (principals.Count > 1)
            {
                // TODO: Better exception message
                // Issue #739
                throw new InvalidOperationException("Multiple matching principals.");
            }

            return principals.FirstOrDefault();
        }

        public virtual void UpdateIdentityMap([NotNull] InternalEntityEntry entry, [NotNull] EntityKey oldKey)
        {
            if (entry.EntityState == EntityState.Detached)
            {
                return;
            }

            var newKey = GetPrimaryKeyValueChecked(entry);

            if (oldKey.Equals(newKey))
            {
                return;
            }

            InternalEntityEntry existingEntry;
            if (_identityMap.TryGetValue(newKey, out existingEntry)
                && existingEntry != entry)
            {
                throw new InvalidOperationException(Strings.IdentityConflict(entry.EntityType.FullName));
            }

            _identityMap.Remove(oldKey);
            _identityMap[newKey] = entry;
        }

        private EntityKey GetPrimaryKeyValueChecked(InternalEntityEntry entry)
        {
            var keyValue = entry.GetPrimaryKeyValue();

            if (keyValue == EntityKey.InvalidEntityKey)
            {
                throw new InvalidOperationException(Strings.InvalidPrimaryKey(entry.EntityType.FullName));
            }

            return keyValue;
        }

        public virtual IEnumerable<InternalEntityEntry> GetDependents([NotNull] InternalEntityEntry principalEntry, [NotNull] IForeignKey foreignKey)
        {
            var principalKeyValue = principalEntry.GetPrincipalKeyValue(foreignKey);

            // TODO: Perf: Add additional indexes so that this isn't a linear lookup
            return principalKeyValue == EntityKey.InvalidEntityKey
                ? Enumerable.Empty<InternalEntityEntry>()
                : Entries.Where(
                    e => e.EntityType == foreignKey.EntityType
                         && principalKeyValue.Equals(e.GetDependentKeyValue(foreignKey)));
        }

        [DebuggerStepThrough]
        public virtual int SaveChanges()
        {
            var entriesToSave = Entries
                .Where(e => e.EntityState == EntityState.Added
                            || e.EntityState == EntityState.Modified
                            || e.EntityState == EntityState.Deleted)
                .Select(e => e.PrepareToSave())
                .ToList();

            if (!entriesToSave.Any())
            {
                return 0;
            }

            try
            {
                var result = SaveChanges(entriesToSave);

                // TODO: When transactions supported, make it possible to commit/accept at end of all transactions
                // Issue #744
                foreach (var entry in entriesToSave)
                {
                    entry.AutoCommitSidecars();
                    entry.AcceptChanges();
                }

                return result;
            }
            catch
            {
                foreach (var entry in entriesToSave)
                {
                    entry.AutoRollbackSidecars();
                }
                throw;
            }
        }

        public virtual async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var entriesToSave = Entries
                .Where(e => e.EntityState == EntityState.Added
                            || e.EntityState == EntityState.Modified
                            || e.EntityState == EntityState.Deleted)
                .Select(e => e.PrepareToSave())
                .ToList();

            if (!entriesToSave.Any())
            {
                return 0;
            }

            try
            {
                var result
                    = await SaveChangesAsync(entriesToSave, cancellationToken)
                        .WithCurrentCulture();

                // TODO: When transactions supported, make it possible to commit/accept at end of all transactions
                // Issue #744
                foreach (var entry in entriesToSave)
                {
                    entry.AutoCommitSidecars();
                    entry.AcceptChanges();
                }

                return result;
            }
            catch
            {
                foreach (var entry in entriesToSave)
                {
                    entry.AutoRollbackSidecars();
                }
                throw;
            }
        }

        protected virtual int SaveChanges(
            [NotNull] IReadOnlyList<InternalEntityEntry> entriesToSave) => _dataStore.SaveChanges(entriesToSave);

        protected virtual async Task<int> SaveChangesAsync(
            [NotNull] IReadOnlyList<InternalEntityEntry> entriesToSave,
            CancellationToken cancellationToken = default(CancellationToken))
            => await _dataStore.SaveChangesAsync(entriesToSave, cancellationToken).WithCurrentCulture();
    }
}
