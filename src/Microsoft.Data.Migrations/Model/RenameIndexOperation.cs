// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Data.Migrations.Utilities;
using Microsoft.Data.Relational;

namespace Microsoft.Data.Migrations.Model
{
    public class RenameIndexOperation : MigrationOperation
    {
        private readonly SchemaQualifiedName _tableName;
        private readonly string _indexName;
        private readonly string _newIndexName;

        public RenameIndexOperation(
            SchemaQualifiedName tableName,
            [NotNull] string indexName,
            [NotNull] string newIndexName)
        {
            Check.NotEmpty(indexName, "indexName");
            Check.NotEmpty(newIndexName, "newIndexName");

            _tableName = tableName;
            _indexName = indexName;
            _newIndexName = newIndexName;
        }

        public virtual SchemaQualifiedName TableName
        {
            get { return _tableName; }
        }

        public virtual string IndexName
        {
            get { return _indexName; }
        }

        public virtual string NewIndexName
        {
            get { return _newIndexName; }
        }

        public override void GenerateSql([NotNull] MigrationOperationSqlGenerator generator, [NotNull] IndentedStringBuilder stringBuilder, bool generateIdempotentSql)
        {
            Check.NotNull(generator, "generator");
            Check.NotNull(stringBuilder, "stringBuilder");

            generator.Generate(this, stringBuilder, generateIdempotentSql);
        }

        public override void GenerateCode([NotNull] MigrationCodeGenerator generator, [NotNull] IndentedStringBuilder stringBuilder)
        {
            Check.NotNull(generator, "generator");
            Check.NotNull(stringBuilder, "stringBuilder");

            generator.Generate(this, stringBuilder);
        }
    }
}
