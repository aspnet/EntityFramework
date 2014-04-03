// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Migrations.Utilities;
using Microsoft.Data.Relational;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Migrations.Model
{
    public class CreateIndexOperation : MigrationOperation
    {
        private readonly SchemaQualifiedName _tableName;
        private readonly string _indexName;
        private readonly IReadOnlyList<string> _columnNames;
        private readonly bool _isUnique;
        private readonly bool _isClustered;

        public CreateIndexOperation(
            SchemaQualifiedName tableName,
            [NotNull] string indexName,
            [NotNull] IReadOnlyList<string> columnNames,
            bool isUnique,
            bool isClustered)
        {
            Check.NotEmpty(indexName, "indexName");
            Check.NotNull(columnNames, "columnNames");

            _tableName = tableName;
            _indexName = indexName;
            _columnNames = columnNames;
            _isUnique = isUnique;
            _isClustered = isClustered;
        }

        public virtual SchemaQualifiedName TableName
        {
            get { return _tableName; }
        }

        public virtual string IndexName
        {
            get { return _indexName; }
        }

        public virtual IReadOnlyList<string> ColumnNames
        {
            get { return _columnNames; }
        }

        public virtual bool IsUnique
        {
            get { return _isUnique; }
        }

        public virtual bool IsClustered
        {
            get { return _isClustered; }
        }

        public override void GenerateSql([NotNull] MigrationOperationSqlGenerator visitor, [NotNull] IndentedStringBuilder stringBuilder, bool generateIdempotentSql)
        {
            Check.NotNull(visitor, "visitor");
            Check.NotNull(stringBuilder, "stringBuilder");

            visitor.Generate(this, stringBuilder, generateIdempotentSql);
        }
    }
}
