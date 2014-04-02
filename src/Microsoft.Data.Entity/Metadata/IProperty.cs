﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Metadata
{
    public interface IProperty : IPropertyBase
    {
        Type PropertyType { get; }
        bool IsNullable { get; }
        ValueGenerationStrategy ValueGenerationStrategy { get; }
        int Index { get; }
        int ShadowIndex { get; }
        int OriginalValueIndex { get; }
        bool IsClrProperty { get; }
        bool IsConcurrencyToken { get; }
    }
}
