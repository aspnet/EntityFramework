﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Migrations.Utilities;
using Microsoft.Data.Relational;

namespace Microsoft.Data.Migrations.Model
{
    public class DropPrimaryKeyOperation : MigrationOperation
    {
        private readonly SchemaQualifiedName _tableName;
        private readonly string _primaryKeyName;

        public DropPrimaryKeyOperation(SchemaQualifiedName tableName, [NotNull] string primaryKeyName)
        {
            Check.NotEmpty(primaryKeyName, "primaryKeyName");

            _tableName = tableName;
            _primaryKeyName = primaryKeyName;
        }

        public virtual SchemaQualifiedName TableName
        {
            get { return _tableName; }
        }

        public virtual string PrimaryKeyName
        {
            get { return _primaryKeyName; }
        }

        public override bool IsDestructiveChange
        {
            get { return true; }
        }

        public override void Accept([NotNull] MigrationOperationVisitor visitor)
        {
            Check.NotNull(visitor, "visitor");

            visitor.Visit(this);
        }
    }
}
