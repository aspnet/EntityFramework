﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.ChangeTracking
{
    public abstract partial class StateEntry
    {
        private readonly ContextConfiguration _configuration;
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
            [NotNull] ContextConfiguration configuration,
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

        public virtual ContextConfiguration Configuration
        {
            get { return _configuration; }
        }

        public virtual Sidecar OriginalValues
        {
            get
            {
                return TryGetSidecar(Sidecar.WellKnownNames.OriginalValues)
                       ?? AddSidecar(_configuration.OriginalValuesFactory.Create(this));
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
            // TODO: Decide what to do here when we decide what to do with sync/async paths in general
            var generatedValue = entityState == EntityState.Added && EntityState != EntityState.Added
                ? GenerateKeyValue().Result
                : null;

            SetEntityState(entityState, generatedValue);
        }

        public virtual async Task SetEntityStateAsync(
            EntityState entityState, CancellationToken cancellationToken = default(CancellationToken))
        {
            Check.IsDefined(entityState, "entityState");

            var generatedValue = entityState == EntityState.Added && EntityState != EntityState.Added
                ? await GenerateKeyValue(cancellationToken).ConfigureAwait(false)
                : null;

            SetEntityState(entityState, generatedValue);
        }

        private Task<object> GenerateKeyValue(CancellationToken cancellationToken = default(CancellationToken))
        {
            var keyProperty = _entityType.GetKey().Properties.Single(); // TODO: Composite keys not implemented yet.
            var identityGenerator = _configuration.ActiveIdentityGenerators.GetOrAdd(keyProperty);

            return identityGenerator != null
                ? identityGenerator.NextAsync(cancellationToken)
                : Task.FromResult<object>(null);
        }

        private void SetEntityState(EntityState entityState, object generatedValue)
        {
            // The entity state can be Modified even if some properties are not modified so always
            // set all properties to modified if the entity state is explicitly set to Modified.
            if (entityState == EntityState.Modified)
            {
                _stateData.SetAllPropertiesModified(_entityType.Properties.Count());
            }

            var oldState = _stateData.EntityState;
            if (oldState == entityState)
            {
                return;
            }

            _configuration.StateEntryNotifier.StateChanging(this, entityState);

            _stateData.EntityState = entityState;

            if (entityState == EntityState.Added
                && generatedValue != null)
            {
                this[_entityType.GetKey().Properties.Single()] = generatedValue; // TODO: Composite keys not implemented yet.
            }

            if (oldState == EntityState.Unknown)
            {
                _configuration.StateManager.StartTracking(this);
            }
            else if (entityState == EntityState.Unknown)
            {
                // TODO: Does changing to Unknown really mean stop tracking?
                _configuration.StateManager.StopTracking(this);
            }

            _configuration.StateEntryNotifier.StateChanged(this, oldState);
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
                var notifier = _configuration.StateEntryNotifier;
                notifier.StateChanging(this, EntityState.Modified);
                _stateData.EntityState = EntityState.Modified;
                notifier.StateChanged(this, currentState);
            }
            else if (!isModified
                     && !_stateData.AnyPropertiesModified())
            {
                var notifier = _configuration.StateEntryNotifier;
                notifier.StateChanging(this, EntityState.Unchanged);
                _stateData.EntityState = EntityState.Unchanged;
                notifier.StateChanged(this, currentState);
            }
        }

        protected abstract object ReadPropertyValue([NotNull] IProperty property);
        protected abstract void WritePropertyValue([NotNull] IProperty property, [CanBeNull] object value);

        public virtual object this[[param: NotNull] IProperty property]
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
            return CreateKey(foreignKey.ReferencedEntityType, foreignKey.Properties, this);
        }

        public virtual EntityKey GetPrincipalKeyValue([NotNull] IForeignKey foreignKey)
        {
            return CreateKey(foreignKey.ReferencedEntityType, foreignKey.ReferencedProperties, this);
        }

        private EntityKey CreateKey(IEntityType entityType, IReadOnlyList<IProperty> properties, StateEntry entry)
        {
            return _configuration.EntityKeyFactorySource
                .GetKeyFactory(properties)
                .Create(entityType, properties, entry);
        }

        public virtual object[] GetValueBuffer()
        {
            return _entityType.Properties.Select(p => this[p]).ToArray();
        }

        public virtual void PropertyChanging([NotNull] IProperty property)
        {
            Check.NotNull(property, "property");

            if (!_entityType.UseLazyOriginalValues)
            {
                return;
            }

            OriginalValues.EnsureSnapshot(property);
        }

        public virtual void PropertyChanged([NotNull] IProperty property)
        {
            Check.NotNull(property, "property");

            SetPropertyModified(property, true);
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

            var foundChanges = false;
            foreach (var property in EntityType.Properties)
            {
                // TODO: Perf: don't lookup accessor twice
                if (!Equals(this[property], originalValues[property]))
                {
                    SetPropertyModified(property, true);
                    foundChanges = true;
                }
            }

            return foundChanges;
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
                .Where(p => p.ValueGenerationStrategy == ValueGenerationStrategy.StoreIdentity
                            || p.ValueGenerationStrategy == ValueGenerationStrategy.StoreComputed).ToList();

            if (storeGenerated.Any())
            {
                AddSidecar(_configuration.StoreGeneratedValuesFactory.Create(this, storeGenerated));
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
