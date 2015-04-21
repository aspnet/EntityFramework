// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data.Common;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Storage;

namespace Microsoft.Data.Entity.Relational
{
    public interface IRelationalValueReaderFactory
    {
        IValueReader CreateValueReader(
            [NotNull] DbDataReader dataReader,
            [NotNull] IEnumerable<Type> valueTypes,
            int offset);
    }
}
