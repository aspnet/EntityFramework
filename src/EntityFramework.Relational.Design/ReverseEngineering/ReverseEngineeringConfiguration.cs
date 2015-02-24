﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using System.Reflection;

namespace Microsoft.Data.Entity.Relational.Design.ReverseEngineering
{
    public class ReverseEngineeringConfiguration
    {
        public virtual IDatabaseMetadataModelProvider Provider { get; [param:  NotNull] set; }
        public virtual string ConnectionString { get;[param: NotNull] set; }
        public virtual string OutputPath { get;[param: NotNull] set; }
        public virtual string Namespace { get;[param: NotNull] set; }
        public virtual string ContextClassName { get;[param: CanBeNull] set; }
    }
}
