// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Relational
{
    public class NonTypedValueReaderFactoryFactory : IRelationalValueReaderFactoryFactory
    {
        public virtual IRelationalValueReaderFactory CreateValueReaderFactory(IEnumerable<Type> valueTypes, int offset)
            => new NonTypedValueReaderFactory(offset, Check.NotNull(valueTypes, nameof(valueTypes)).Count());
    }
}
