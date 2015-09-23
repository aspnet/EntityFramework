// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Metadata.Internal
{
    public class InternalPropertyBuilder : InternalMetadataItemBuilder<Property>
    {
        private ConfigurationSource? _clrTypeConfigurationSource;
        private ConfigurationSource? _isReadOnlyAfterSaveConfigurationSource;
        private ConfigurationSource? _isReadOnlyBeforeSaveConfigurationSource;
        private ConfigurationSource? _isRequiredConfigurationSource;
        private ConfigurationSource? _isConcurrencyTokenConfigurationSource;
        private ConfigurationSource? _isShadowPropertyConfigurationSource;
        private ConfigurationSource? _maxLengthConfigurationSource;
        private ConfigurationSource? _requiresValueGeneratorConfigurationSource;
        private ConfigurationSource? _valueGeneratedConfigurationSource;

        public InternalPropertyBuilder([NotNull] Property property, [NotNull] InternalModelBuilder modelBuilder, bool existing)
            : base(property, modelBuilder)
        {
            if (existing)
            {
                if (Metadata.IsNullable != null)
                {
                    _isRequiredConfigurationSource = ConfigurationSource.Explicit;
                }
                if (Metadata.GetMaxLength() != null)
                {
                    _maxLengthConfigurationSource = ConfigurationSource.Explicit;
                }
                if (Metadata.IsConcurrencyToken != null)
                {
                    _isConcurrencyTokenConfigurationSource = ConfigurationSource.Explicit;
                }
                if (Metadata.IsReadOnlyAfterSave != null)
                {
                    _isReadOnlyAfterSaveConfigurationSource = ConfigurationSource.Explicit;
                }
                if (Metadata.IsReadOnlyBeforeSave != null)
                {
                    _isReadOnlyBeforeSaveConfigurationSource = ConfigurationSource.Explicit;
                }
                if (Metadata.IsShadowProperty != null)
                {
                    _isShadowPropertyConfigurationSource = ConfigurationSource.Explicit;
                }
                if (Metadata.ClrType != null)
                {
                    _clrTypeConfigurationSource = ConfigurationSource.Explicit;
                }
                if (Metadata.RequiresValueGenerator != null)
                {
                    _requiresValueGeneratorConfigurationSource = ConfigurationSource.Explicit;
                }
                if (Metadata.ValueGenerated != null)
                {
                    _valueGeneratedConfigurationSource = ConfigurationSource.Explicit;
                }
            }
        }

        public virtual bool IsRequired(bool? isRequired, ConfigurationSource configurationSource)
        {
            if (CanSetRequired(isRequired, configurationSource))
            {
                _isRequiredConfigurationSource = configurationSource.Max(_isRequiredConfigurationSource);

                Metadata.IsNullable = !isRequired;
                return true;
            }

            return false;
        }

        public virtual bool CanSetRequired(bool? isRequired, ConfigurationSource configurationSource)
            => configurationSource.CanSet(_isRequiredConfigurationSource, Metadata.IsNullable.HasValue)
               || ((IProperty)Metadata).IsNullable == !isRequired;

        public virtual bool HasMaxLength(int? maxLength, ConfigurationSource configurationSource)
        {
            if (configurationSource.CanSet(_maxLengthConfigurationSource, Metadata.GetMaxLength().HasValue)
                || Metadata.GetMaxLength().Value == maxLength)
            {
                _maxLengthConfigurationSource = configurationSource.Max(_maxLengthConfigurationSource);

                Metadata.SetMaxLength(maxLength);
                return true;
            }

            return false;
        }

        public virtual bool IsConcurrencyToken(bool? isConcurrencyToken, ConfigurationSource configurationSource)
        {
            if (configurationSource.CanSet(_isConcurrencyTokenConfigurationSource, Metadata.IsConcurrencyToken.HasValue)
                || Metadata.IsConcurrencyToken.Value == isConcurrencyToken)
            {
                _isConcurrencyTokenConfigurationSource = configurationSource.Max(_isConcurrencyTokenConfigurationSource);

                Metadata.IsConcurrencyToken = isConcurrencyToken;
                return true;
            }

            return false;
        }

        public virtual bool ReadOnlyAfterSave(bool? isReadOnlyAfterSave, ConfigurationSource configurationSource)
        {
            if (configurationSource.CanSet(_isReadOnlyAfterSaveConfigurationSource, Metadata.IsReadOnlyAfterSave.HasValue)
                || Metadata.IsReadOnlyAfterSave == isReadOnlyAfterSave)
            {
                _isReadOnlyAfterSaveConfigurationSource = configurationSource.Max(_isReadOnlyAfterSaveConfigurationSource);

                Metadata.IsReadOnlyAfterSave = isReadOnlyAfterSave;
                return true;
            }

            return false;
        }

        public virtual bool ReadOnlyBeforeSave(bool? isReadOnlyBeforeSave, ConfigurationSource configurationSource)
        {
            if (configurationSource.CanSet(_isReadOnlyBeforeSaveConfigurationSource, Metadata.IsReadOnlyBeforeSave.HasValue)
                || Metadata.IsReadOnlyBeforeSave == isReadOnlyBeforeSave)
            {
                _isReadOnlyBeforeSaveConfigurationSource = configurationSource.Max(_isReadOnlyBeforeSaveConfigurationSource);

                Metadata.IsReadOnlyBeforeSave = isReadOnlyBeforeSave;
                return true;
            }

            return false;
        }

        public virtual bool Shadow(bool? isShadowProperty, ConfigurationSource configurationSource)
        {
            if (configurationSource.CanSet(_isShadowPropertyConfigurationSource, Metadata.IsShadowProperty.HasValue)
                || Metadata.IsShadowProperty == isShadowProperty)
            {
                _isShadowPropertyConfigurationSource = configurationSource.Max(_isShadowPropertyConfigurationSource);

                Metadata.IsShadowProperty = isShadowProperty;
                return true;
            }

            return false;
        }

        public virtual bool ClrType([CanBeNull] Type propertyType, ConfigurationSource configurationSource)
        {
            if (configurationSource.CanSet(_clrTypeConfigurationSource, Metadata.ClrType != null)
                || Metadata.ClrType == propertyType)
            {
                _clrTypeConfigurationSource = configurationSource.Max(_clrTypeConfigurationSource);

                Metadata.ClrType = propertyType;
                return true;
            }

            return false;
        }

        public virtual bool UseValueGenerator(bool? generateValue, ConfigurationSource configurationSource)
        {
            if (configurationSource.CanSet(_requiresValueGeneratorConfigurationSource, Metadata.RequiresValueGenerator.HasValue)
                || Metadata.RequiresValueGenerator.Value == generateValue)
            {
                _requiresValueGeneratorConfigurationSource = configurationSource.Max(_requiresValueGeneratorConfigurationSource);

                Metadata.RequiresValueGenerator = generateValue;
                return true;
            }

            return false;
        }

        public virtual bool ValueGenerated(ValueGenerated? valueGenerated, ConfigurationSource configurationSource)
        {
            if (configurationSource.CanSet(_valueGeneratedConfigurationSource, Metadata.ValueGenerated.HasValue)
                || Metadata.ValueGenerated == valueGenerated)
            {
                _valueGeneratedConfigurationSource = configurationSource.Max(_valueGeneratedConfigurationSource);

                Metadata.ValueGenerated = valueGenerated;
                return true;
            }

            return false;
        }
    }
}
