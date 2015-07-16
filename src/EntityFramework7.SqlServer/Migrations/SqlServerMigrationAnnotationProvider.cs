﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Migrations.Infrastructure;
using Microsoft.Data.Entity.SqlServer.Metadata;

namespace Microsoft.Data.Entity.SqlServer.Migrations
{
    public class SqlServerMigrationAnnotationProvider : MigrationAnnotationProvider
    {
        public override IEnumerable<IAnnotation> For(IKey key)
            => key.Annotations.Where(a => a.Name == SqlServerAnnotationNames.Prefix + SqlServerAnnotationNames.Clustered);

        public override IEnumerable<IAnnotation> For(IIndex index)
            => index.Annotations.Where(a => a.Name == SqlServerAnnotationNames.Prefix + SqlServerAnnotationNames.Clustered);

        public override IEnumerable<IAnnotation> For(IProperty property)
        {
            if (property.SqlServer().IdentityStrategy == SqlServerIdentityStrategy.IdentityColumn)
            {
                yield return new Annotation(
                    SqlServerAnnotationNames.Prefix + SqlServerAnnotationNames.ValueGenerationStrategy,
                    SqlServerIdentityStrategy.IdentityColumn.ToString());
            }
        }
    }
}
