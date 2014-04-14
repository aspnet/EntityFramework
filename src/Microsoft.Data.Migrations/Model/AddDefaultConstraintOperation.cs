// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Data.Migrations.Utilities;
using Microsoft.Data.Relational;

namespace Microsoft.Data.Migrations.Model
{
    public class AddDefaultConstraintOperation : MigrationOperation
    {
        private readonly SchemaQualifiedName _tableName;
        private readonly string _columnName;
        // TODO: Consider grouping these into struct with object and boolean
        // that indicates whether it is value or sql.
        private readonly object _defaultValue;
        private readonly string _defaultSql;

        public AddDefaultConstraintOperation(
            SchemaQualifiedName tableName,
            [NotNull] string columnName,
            [CanBeNull] object defaultValue,
            [CanBeNull] string defaultSql)
        {
            Check.NotEmpty(columnName, "columnName");

            // TODO: Validate input. Either defaultValue or defaultSql must not be null, but not both.

            _tableName = tableName;
            _columnName = columnName;
            _defaultValue = defaultValue;
            _defaultSql = defaultSql;
        }

        public virtual SchemaQualifiedName TableName
        {
            get { return _tableName; }
        }

        public virtual string ColumnName
        {
            get { return _columnName; }
        }

        public virtual object DefaultValue
        {
            get { return _defaultValue; }
        }

        public virtual string DefaultSql
        {
            get { return _defaultSql; }
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
