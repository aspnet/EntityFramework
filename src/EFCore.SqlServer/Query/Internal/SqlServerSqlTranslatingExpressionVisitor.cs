// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Internal
{
    public class SqlServerSqlTranslatingExpressionVisitor : RelationalSqlTranslatingExpressionVisitor
    {
        private static readonly HashSet<string> _dateTimeDataTypes
            = new HashSet<string>
            {
                "time",
                "date",
                "datetime",
                "datetime2",
                "datetimeoffset"
            };

        private static readonly HashSet<ExpressionType> _arithmeticOperatorTypes
            = new HashSet<ExpressionType>
            {
                ExpressionType.Add,
                ExpressionType.Subtract,
                ExpressionType.Multiply,
                ExpressionType.Divide,
                ExpressionType.Modulo
            };

        // TODO: Possibly make this protected in base
        private readonly ISqlExpressionFactory _sqlExpressionFactory;

        public SqlServerSqlTranslatingExpressionVisitor(
            [NotNull] RelationalSqlTranslatingExpressionVisitorDependencies dependencies,
            [NotNull] IModel model,
            [NotNull] QueryableMethodTranslatingExpressionVisitor queryableMethodTranslatingExpressionVisitor)
            : base(dependencies, model, queryableMethodTranslatingExpressionVisitor)
        {
            _sqlExpressionFactory = dependencies.SqlExpressionFactory;
        }

        protected override Expression VisitBinary(BinaryExpression binaryExpression)
        {
            Check.NotNull(binaryExpression, nameof(binaryExpression));

            var visitedExpression = (SqlExpression)base.VisitBinary(binaryExpression);

            if (visitedExpression == null)
            {
                return null;
            }

            return visitedExpression is SqlBinaryExpression sqlBinary
                && _arithmeticOperatorTypes.Contains(sqlBinary.OperatorType)
                && (_dateTimeDataTypes.Contains(GetProviderType(sqlBinary.Left))
                    || _dateTimeDataTypes.Contains(GetProviderType(sqlBinary.Right)))
                    ? null
                    : visitedExpression;
        }

        protected override Expression VisitUnary(UnaryExpression unaryExpression)
        {
            if (unaryExpression.NodeType == ExpressionType.ArrayLength
             && unaryExpression.Operand.Type == typeof(byte[])
             && Visit(unaryExpression.Operand) is ColumnExpression columnExpression)
            {
                var intMapping = _sqlExpressionFactory.FindMapping(typeof(int));

                return _sqlExpressionFactory.Convert(
                    _sqlExpressionFactory.Function("DATALENGTH", new[] { columnExpression }, typeof(int?), intMapping),
                    typeof(int?), intMapping);
            }

            return base.VisitUnary(unaryExpression);
        }

        public override SqlExpression TranslateLongCount(Expression expression = null)
        {
            if (expression != null)
            {
                // TODO: Translate Count with predicate for GroupBy
                return null;
            }

            return _sqlExpressionFactory.ApplyDefaultTypeMapping(
                _sqlExpressionFactory.Function("COUNT_BIG", new[] { _sqlExpressionFactory.Fragment("*") }, typeof(long)));
        }

        private static string GetProviderType(SqlExpression expression)
        {
            return expression.TypeMapping?.StoreType;
        }
    }
}
