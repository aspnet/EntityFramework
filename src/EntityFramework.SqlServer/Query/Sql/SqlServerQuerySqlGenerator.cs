// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Query.Expressions;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Query.Sql
{
    public class SqlServerQuerySqlGenerator : DefaultQuerySqlGenerator, ISqlServerExpressionVisitor
    {
        public SqlServerQuerySqlGenerator(
            [NotNull] IParameterNameGeneratorFactory parameterNameGeneratorFactory,
            [NotNull] IRelationalCommandBuilderFactory commandBuilderFactory,
            [NotNull] SelectExpression selectExpression)
            : base(
                  parameterNameGeneratorFactory,
                  commandBuilderFactory,
                  selectExpression)
        {
        }

        protected override string DelimitIdentifier(string identifier)
            => "[" + identifier.Replace("]", "]]") + "]";

        public override Expression VisitCount(CountExpression countExpression)
        {
            Check.NotNull(countExpression, nameof(countExpression));

            if (countExpression.Type == typeof(long))
            {
                Sql.Append("COUNT_BIG(*)");

                return countExpression;
            }

            return base.VisitCount(countExpression);
        }

        protected override void GenerateLimitOffset(SelectExpression selectExpression)
        {
            if (selectExpression.Projection.OfType<RowNumberExpression>().Any())
            {
                return;
            }

            if (selectExpression.Offset != null
                && !selectExpression.OrderBy.Any())
            {
                Sql.AppendLine().Append("ORDER BY @@ROWCOUNT");
            }

            base.GenerateLimitOffset(selectExpression);
        }

        public virtual Expression VisitRowNumber(RowNumberExpression rowNumberExpression)
        {
            Check.NotNull(rowNumberExpression, nameof(rowNumberExpression));

            Sql.Append("ROW_NUMBER() OVER(");
            GenerateOrderBy(rowNumberExpression.Orderings);
            Sql.Append(") AS ").Append(DelimitIdentifier(rowNumberExpression.ColumnExpression.Name));

            return rowNumberExpression;
        }

        public override Expression VisitSqlFunction(SqlFunctionExpression sqlFunctionExpression)
        {
            if (sqlFunctionExpression.FunctionName.StartsWith("@@"))
            {
                Sql.Append(sqlFunctionExpression.FunctionName);
                return sqlFunctionExpression;
            }
            return base.VisitSqlFunction(sqlFunctionExpression);
        }
    }
}
