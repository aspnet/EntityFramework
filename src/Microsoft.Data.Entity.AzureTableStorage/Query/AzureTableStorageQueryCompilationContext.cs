﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Query;

namespace Microsoft.Data.Entity.AzureTableStorage.Query
{
    public class AzureTableStorageQueryCompilationContext : QueryCompilationContext
    {
        public AzureTableStorageQueryCompilationContext([NotNull] IModel model)
            : base(model)
        {
        }

        public override EntityQueryModelVisitor CreateVisitor()
        {
            return new AzureTableStorageQueryModelVisitor(this);
        }
    }
}
