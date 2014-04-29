﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Metadata
{
    public interface IEntityType : IMetadata
    {
        string Name { get; }

        [CanBeNull]
        Type Type { get; }

        IKey GetKey();

        [CanBeNull]
        IProperty TryGetProperty([NotNull] string name);

        [NotNull]
        IProperty GetProperty([NotNull] string name);

        IReadOnlyList<IProperty> Properties { get; }
        IReadOnlyList<IForeignKey> ForeignKeys { get; }
        IReadOnlyList<INavigation> Navigations { get; }
        int ShadowPropertyCount { get; }
        int OriginalValueCount { get; }
        bool HasClrType { get; }
        bool UseLazyOriginalValues { get; }
        string StorageName { get; }
    }
}
