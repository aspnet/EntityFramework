// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.ChangeTracking
{
    public abstract class DictionarySidecar : Sidecar
    {
        private readonly Dictionary<IPropertyBase, object> _values;

        /// <summary>
        ///     This constructor is intended only for use when creating test doubles that will override members
        ///     with mocked or faked behavior. Use of this constructor for other purposes may result in unexpected
        ///     behavior including but not limited to throwing <see cref="NullReferenceException" />.
        /// </summary>
        protected DictionarySidecar()
        {
        }

        protected DictionarySidecar(
            [NotNull] StateEntry stateEntry,
            [NotNull] IEnumerable<IPropertyBase> properties)
            : base(stateEntry)
        {
            Check.NotNull(properties, "properties");

            _values = properties.ToDictionary(p => p, p => (object)null);
        }

        public override bool CanStoreValue(IPropertyBase property)
        {
            Check.NotNull(property, "property");

            return _values.ContainsKey(property);
        }

        protected override object ReadValue(IPropertyBase property)
        {
            Check.NotNull(property, "property");

            return _values[property];
        }

        protected override void WriteValue(IPropertyBase property, object value)
        {
            Check.NotNull(property, "property");

            _values[property] = value;
        }
    }
}
