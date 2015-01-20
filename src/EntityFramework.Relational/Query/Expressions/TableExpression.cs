﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Relational.Query.Sql;
using Microsoft.Data.Entity.Utilities;
using Remotion.Linq.Clauses;
using Remotion.Linq.Parsing;

namespace Microsoft.Data.Entity.Relational.Query.Expressions
{
    public class TableExpression : TableExpressionBase
    {
        public TableExpression(
            [NotNull] string table,
            [CanBeNull] string schema,
            [NotNull] string alias,
            [NotNull] IQuerySource querySource)
            : base(
                Check.NotNull(querySource, "querySource"),
                Check.NotEmpty(alias, "alias"))
        {
            Check.NotEmpty(table, "table");
            Check.NotNull(querySource, "querySource");

            Table = table;
            Schema = schema;
        }

        public virtual string Table { get; }

        public virtual string Schema { get; }

        public override Expression Accept([NotNull] ExpressionTreeVisitor visitor)
        {
            Check.NotNull(visitor, "visitor");

            var specificVisitor = visitor as ISqlExpressionVisitor;

            return specificVisitor != null
                ? specificVisitor.VisitTableExpression(this)
                : base.Accept(visitor);
        }

        public override string ToString()
        {
            return Table + " " + Alias;
        }
    }
}
