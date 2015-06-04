﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.Sqlite.Metadata
{
    public class SqlitePropertyExtensions : ReadOnlySqlitePropertyExtensions
    {
        public SqlitePropertyExtensions([NotNull] Property property)
            : base(property)
        {
        }

        public virtual new string Column
        {
            get { return base.Column; }
            [param: CanBeNull]
            set { Property[SqliteNameAnnotation] = value; }
        }

        public virtual new string ColumnType
        {
            get { return base.ColumnType; }
            [param: CanBeNull]
            set { Property[SqliteColumnTypeAnnotation] = value; }
        }

        public virtual new string DefaultExpression
        {
            get { return base.DefaultExpression; }
            [param: CanBeNull]
            set { Property[SqliteDefaultExpressionAnnotation] = value; }
        }

        protected virtual new Property Property => (Property)base.Property;
    }
}
