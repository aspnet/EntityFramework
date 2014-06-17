// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Identity;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.ChangeTracking
{
    public abstract partial class StateEntry
    {
        private readonly DbContextConfiguration _configuration;
        private readonly IEntityType _entityType;
        private StateData _stateData;
        private Sidecar[] _sidecars;

        /// <summary>
        ///     This constructor is intended only for use when creating test doubles that will override members
        ///     with mocked or faked behavior. Use of this constructor for other purposes may result in unexpected
        ///     behavior including but not limited to throwing <see cref="NullReferenceException" />.
        /// </summary>
        protected StateEntry()
        {
        }

        protected StateEntry(
            [NotNull] DbContextConfiguration configuration,
            [NotNull] IEntityType entityType)
        {
            Check.NotNull(configuration, "configuration");
            Check.NotNull(entityType, "entityType");

            _configuration = configuration;
            _entityType = entityType;
            _stateData = new StateData(entityType.Properties.Count);
        }

        [CanBeNull]
        public abstract object Entity { get; }

        public virtual IEntityType EntityType
        {
            get { return _entityType; }
        }

        public virtual DbContextConfiguration Configuration
        {
            get { return _configuration; }
        }

        public virtual Sidecar OriginalValues
        {
            get
            {
                return TryGetSidecar(Sidecar.WellKnownNames.OriginalValues)
                       ?? AddSidecar(_configuration.Services.OriginalValuesFactory.Create(this));
            }
        }

        public virtual Sidecar RelationshipsSnapshot
        {
            get
            {
                return TryGetSidecar(Sidecar.WellKnownNames.RelationshipsSnapshot)
                       ?? AddSidecar(_configuration.Services.RelationshipsSnapshotFactory.Create(this));
            }
        }

        public virtual Sidecar AddSidecar([NotNull] Sidecar sidecar)
        {
            Check.NotNull(sidecar, "sidecar");

            var newArray = new[] { sidecar };
            _sidecars = _sidecars == null
                ? newArray
                : newArray.Concat(_sidecars).ToArray();

            if (sidecar.TransparentRead
                || sidecar.TransparentWrite
                || sidecar.AutoCommit)
            {
                _stateData.TransparentSidecarInUse = true;
            }

            return sidecar;
        }

        public virtual Sidecar TryGetSidecar([NotNull] string name)
        {
            Check.NotEmpty(name, "name");

            return _sidecars == null
                ? null
                : _sidecars.FirstOrDefault(s => s.Name == name);
        }

        public virtual void RemoveSidecar([NotNull] string name)
        {
            Check.NotEmpty(name, "name");

            if (_sidecars == null)
            {
                return;
            }

            _sidecars = _sidecars.Where(v => v.Name != name).ToArray();

            if (_sidecars.Length == 0)
            {
                _sidecars = null;
                _stateData.TransparentSidecarInUse = false;
            }
            else
            {
                _stateData.TransparentSidecarInUse
                    = _sidecars.Any(s => s.TransparentRead || s.TransparentWrite || s.AutoCommit);
            }
        }

        private void SetEntityState(EntityState entityState)
        {
            SetEntityState(entityState, GenerateValues(GetValueGenerators(entityState)));
        }

        public virtual async Task SetEntityStateAsync(
            EntityState entityState, CancellationToken cancellationToken = default(CancellationToken))
        {
            Check.IsDefined(entityState, "entityState");

            SetEntityState(entityState, await GenerateValuesAsync(GetValueGenerators(entityState), cancellationToken));
        }

        private Tuple<IProperty, object>[] GenerateValues(Tuple<IProperty, IValueGenerator>[] generators)
        {
            return generators == null
                ? null
                : generators
                    .Select(t => t.Item2 == null ? null : Tuple.Create(t.Item1, t.Item2.Next(_configuration, t.Item1)))
                    .ToArray();
        }

        private async Task<Tuple<IProperty, object>[]> GenerateValuesAsync(
            Tuple<IProperty, IValueGenerator>[] generators,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (generators == null)
            {
                return null;
            }

            var values = new Tuple<IProperty, object>[generators.Length];
            for (var i = 0; i < generators.Length; i++)
            {
                var generator = generators[i].Item2;
                if (generator != null)
                {
                    var property = generators[i].Item1;
                    values[i] = Tuple.Create(property, await generator.NextAsync(_configuration, property, cancellationToken));
                }
            }
            return values;
        }

        private Tuple<IProperty, IValueGenerator>[] GetValueGenerators(EntityState newState)
        {
            if (newState != EntityState.Added
                || EntityState == EntityState.Added)
            {
                return null;
            }
            var properties = _entityType.Properties.Where(p => p.ValueGenerationOnAdd != ValueGenerationOnAdd.None);
            return properties.Select(p => Tuple.Create(p, _configuration.ValueGeneratorCache.GetGenerator(p))).ToArray();
        }

        private void SetEntityState(EntityState newState, Tuple<IProperty, object>[] generatedValues)
        {
            // The entity state can be Modified even if some properties are not modified so always
            // set all properties to modified if the entity state is explicitly set to Modified.
            if (newState == EntityState.Modified)
            {
                _stateData.SetAllPropertiesModified(_entityType.Properties.Count());
            }

            var oldState = _stateData.EntityState;
            if (oldState == newState)
            {
                return;
            }

            // An Added entity does not yet exist in the database. If it is then marked as deleted there is
            // nothing to delete because it was not yet inserted, so just make sure it doesn't get inserted.
            if (oldState == EntityState.Added
                && newState == EntityState.Deleted)
            {
                newState = EntityState.Unknown;
            }

            _configuration.Services.StateEntryNotifier.StateChanging(this, newState);

            _stateData.EntityState = newState;

            if (newState == EntityState.Added)
            {
                foreach (var generatedValue in generatedValues.Where(v => v != null))
                {
                    this[generatedValue.Item1] = generatedValue.Item2;
                }
            }
            else
            {
                Contract.Assert(generatedValues == null);
            }

            if (oldState == EntityState.Unknown)
            {
                _configuration.Services.StateManager.StartTracking(this);
            }
            else if (newState == EntityState.Unknown)
            {
                // TODO: Does changing to Unknown really mean stop tracking?
                _configuration.Services.StateManager.StopTracking(this);
            }

            _configuration.Services.StateEntryNotifier.StateChanged(this, oldState);
        }

        public virtual EntityState EntityState
        {
            get { return _stateData.EntityState; }
            set
            {
                Check.IsDefined(value, "value");

                SetEntityState(value);
            }
        }

        public virtual bool IsPropertyModified([NotNull] IProperty property)
        {
            Check.NotNull(property, "property");

            if (_stateData.EntityState != EntityState.Modified)
            {
                return false;
            }

            return _stateData.IsPropertyModified(property.Index);
        }

        public virtual void SetPropertyModified([NotNull] IProperty property, bool isModified)
        {
            Check.NotNull(property, "property");

            // TODO: Restore original value to reject changes when isModified is false

            _stateData.SetPropertyModified(property.Index, isModified);

            // Don't change entity state if it is Added or Deleted
            var currentState = _stateData.EntityState;
            if (isModified && currentState == EntityState.Unchanged)
            {
                var notifier = _configuration.Services.StateEntryNotifier;
                notifier.StateChanging(this, EntityState.Modified);
                _stateData.EntityState = EntityState.Modified;
                notifier.StateChanged(this, currentState);
            }
            else if (!isModified
                     && !_stateData.AnyPropertiesModified())
            {
                var notifier = _configuration.Services.StateEntryNotifier;
                notifier.StateChanging(this, EntityState.Unchanged);
                _stateData.EntityState = EntityState.Unchanged;
                notifier.StateChanged(this, currentState);
            }
        }

        protected virtual object ReadPropertyValue([NotNull] IPropertyBase propertyBase)
        {
            Check.NotNull(propertyBase, "propertyBase");

            Contract.Assert(!(propertyBase is IProperty) || ((IProperty)propertyBase).IsClrProperty);

            return _configuration.Services.ClrPropertyGetterSource.GetAccessor(propertyBase).GetClrValue(Entity);
        }

        protected virtual void WritePropertyValue([NotNull] IPropertyBase propertyBase, [CanBeNull] object value)
        {
            Check.NotNull(propertyBase, "propertyBase");

            Contract.Assert(!(propertyBase is IProperty) || ((IProperty)propertyBase).IsClrProperty);

            _configuration.Services.ClrPropertySetterSource.GetAccessor(propertyBase).SetClrValue(Entity, value);
        }

        public virtual object this[[param: NotNull] IPropertyBase property]
        {
            get
            {
                Check.NotNull(property, "property");

                if (_stateData.TransparentSidecarInUse)
                {
                    foreach (var sidecar in _sidecars)
                    {
                        if (sidecar.TransparentRead
                            && sidecar.HasValue(property))
                        {
                            return sidecar[property];
                        }
                    }
                }

                return ReadPropertyValue(property);
            }
            [param: CanBeNull]
            set
            {
                Check.NotNull(property, "property");

                if (_stateData.TransparentSidecarInUse)
                {
                    var wrote = false;
                    foreach (var sidecar in _sidecars)
                    {
                        if (sidecar.TransparentWrite
                            && sidecar.CanStoreValue(property))
                        {
                            sidecar[property] = value;
                            wrote = true;
                        }
                    }
                    if (wrote)
                    {
                        return;
                    }
                }

                var currentValue = this[property];

                if (!Equals(currentValue, value))
                {
                    PropertyChanging(property);

                    WritePropertyValue(property, value);

                    PropertyChanged(property);
                }
            }
        }

        public virtual EntityKey GetPrimaryKeyValue()
        {
            return CreateKey(_entityType, _entityType.GetKey().Properties, this);
        }

        public virtual EntityKey GetDependentKeyValue([NotNull] IForeignKey foreignKey)
        {
            Check.NotNull(foreignKey, "foreignKey");

            return CreateKey(foreignKey.ReferencedEntityType, foreignKey.Properties, this);
        }

        public virtual EntityKey GetPrincipalKeyValue([NotNull] IForeignKey foreignKey)
        {
            Check.NotNull(foreignKey, "foreignKey");

            return CreateKey(foreignKey.ReferencedEntityType, foreignKey.ReferencedProperties, this);
        }

        private EntityKey CreateKey(IEntityType entityType, IReadOnlyList<IProperty> properties, StateEntry entry)
        {
            return _configuration.Services.EntityKeyFactorySource
                .GetKeyFactory(properties)
                .Create(entityType, properties, entry);
        }

        public virtual EntityKey GetDependentKeySnapshot([NotNull] IForeignKey foreignKey)
        {
            Check.NotNull(foreignKey, "foreignKey");

            return _configuration.Services.EntityKeyFactorySource
                .GetKeyFactory(foreignKey.Properties)
                .Create(foreignKey.ReferencedEntityType, foreignKey.Properties, RelationshipsSnapshot);
        }

        public virtual object[] GetValueBuffer()
        {
            return _entityType.Properties.Select(p => this[p]).ToArray();
        }

        public virtual void PropertyChanging([NotNull] IPropertyBase propertyBase)
        {
            Check.NotNull(propertyBase, "propertyBase");

            if (_entityType.UseLazyOriginalValues)
            {
                OriginalValues.EnsureSnapshot(propertyBase);

                // TODO: Consider making snapshot temporary here since it is no longer required after PropertyChanged is called
                RelationshipsSnapshot.TakeSnapshot(propertyBase);
            }
        }

        public virtual void PropertyChanged([NotNull] IPropertyBase propertyBase)
        {
            Check.NotNull(propertyBase, "propertyBase");

            var property = propertyBase as IProperty;

            if (property != null)
            {
                SetPropertyModified(property, true);

                if (DetectForeignKeyChange(property))
                {
                    RelationshipsSnapshot.TakeSnapshot(property);
                }
            }
            else
            {
                var navigation = propertyBase as INavigation;

                if (navigation != null)
                {
                    if (DetectNavigationChange(navigation))
                    {
                        RelationshipsSnapshot.TakeSnapshot(navigation);
                    }
                }
            }
        }

        public virtual bool DetectChanges()
        {
            var originalValues = TryGetSidecar(Sidecar.WellKnownNames.OriginalValues);

            // TODO: Consider more efficient/higher-level/abstract mechanism for checking if DetectChanges is needed
            if (_entityType.Type == null
                || originalValues == null
                || typeof(INotifyPropertyChanged).GetTypeInfo().IsAssignableFrom(_entityType.Type.GetTypeInfo()))
            {
                return false;
            }

            var changedFkProperties = new List<IProperty>();
            var foundChanges = false;
            foreach (var property in EntityType.Properties)
            {
                // TODO: Perf: don't lookup accessor twice
                if (!Equals(this[property], originalValues[property]))
                {
                    SetPropertyModified(property, true);
                    foundChanges = true;
                }

                if (DetectForeignKeyChange(property))
                {
                    changedFkProperties.Add(property);
                }
            }

            foreach (var property in changedFkProperties)
            {
                RelationshipsSnapshot.TakeSnapshot(property);
            }

            foreach (var navigation in EntityType.Navigations)
            {
                if (DetectNavigationChange(navigation))
                {
                    RelationshipsSnapshot.TakeSnapshot(navigation);
                }
            }

            return foundChanges;
        }

        private bool DetectForeignKeyChange(IProperty property)
        {
            // TODO: Consider flag/index for fast check for FK
            if (_entityType.ForeignKeys.SelectMany(fk => fk.Properties).Contains(property))
            {
                var snapshotValue = RelationshipsSnapshot[property];
                var currentValue = this[property];

                // TODO: Ensure structural equality where necessary--e.g. byte arrays
                if (!Equals(currentValue, snapshotValue))
                {
                    var notifier = _configuration.Services.StateEntryNotifier;
                    notifier.ForeignKeyPropertyChanged(this, property, snapshotValue, currentValue);

                    return true;
                }
            }

            return false;
        }

        private bool DetectNavigationChange(INavigation navigation)
        {
            if (navigation.PointsToPrincipal
                || navigation.ForeignKey.IsUnique)
            {
                var snapshotValue = RelationshipsSnapshot[navigation];
                var currentValue = this[navigation];

                if (!ReferenceEquals(currentValue, snapshotValue))
                {
                    var notifier = _configuration.Services.StateEntryNotifier;
                    notifier.NavigationReferenceChanged(this, navigation, snapshotValue, currentValue);

                    return true;
                }
            }
            else
            {
                // TODO: Handle collections
            }

            return false;
        }

        public virtual void AcceptChanges()
        {
            var currentState = EntityState;
            if (currentState == EntityState.Unchanged
                || currentState == EntityState.Unknown)
            {
                return;
            }

            if (currentState == EntityState.Added
                || currentState == EntityState.Modified)
            {
                var originalValues = TryGetSidecar(Sidecar.WellKnownNames.OriginalValues);
                if (originalValues != null)
                {
                    originalValues.UpdateSnapshot();
                }

                EntityState = EntityState.Unchanged;
            }
            else if (currentState == EntityState.Deleted)
            {
                EntityState = EntityState.Unknown;
            }
        }

        public virtual void AutoCommitSidecars()
        {
            if (_stateData.TransparentSidecarInUse)
            {
                foreach (var sidecar in _sidecars)
                {
                    if (sidecar.AutoCommit)
                    {
                        sidecar.Commit();
                    }
                }
            }
        }

        public virtual StateEntry PrepareToSave()
        {
            var storeGenerated = _entityType.Properties
                .Where(p => p.ValueGenerationOnSave != ValueGenerationOnSave.None).ToList();

            if (storeGenerated.Any())
            {
                AddSidecar(_configuration.Services.StoreGeneratedValuesFactory.Create(this, storeGenerated));
            }

            return this;
        }

        public virtual void AutoRollbackSidecars()
        {
            if (_stateData.TransparentSidecarInUse)
            {
                foreach (var sidecar in _sidecars)
                {
                    if (sidecar.AutoCommit)
                    {
                        sidecar.Rollback();
                    }
                }
            }
        }
    }
}
