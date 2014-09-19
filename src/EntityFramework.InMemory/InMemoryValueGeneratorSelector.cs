// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Identity;
using Microsoft.Data.Entity.InMemory.Utilities;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.InMemory
{
    public class InMemoryValueGeneratorSelector : ValueGeneratorSelector
    {
        private readonly SimpleValueGeneratorFactory<InMemoryValueGenerator> _inMemoryFactory;

        public InMemoryValueGeneratorSelector(
            [NotNull] SimpleValueGeneratorFactory<GuidValueGenerator> guidFactory,
            [NotNull] SimpleValueGeneratorFactory<InMemoryValueGenerator> inMemoryFactory)
            : base(guidFactory)
        {
            Check.NotNull(inMemoryFactory, "inMemoryFactory");

            _inMemoryFactory = inMemoryFactory;
        }

        public override IValueGeneratorFactory Select(IProperty property)
        {
            Check.NotNull(property, "property");

            if (property.ValueGeneration == ValueGeneration.OnAdd
                && property.PropertyType.IsInteger())
            {
                return _inMemoryFactory;
            }

            return base.Select(property);
        }
    }
}
