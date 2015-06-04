// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Query;
using Microsoft.Data.Entity.Relational.Query.Expressions;
using Microsoft.Data.Entity.Relational.Query.Methods;
using Microsoft.Data.Entity.Relational.Query.Sql;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.Logging;
using JetBrains.Annotations;
using Remotion.Linq.Clauses;

namespace Microsoft.Data.Entity.Relational.Query
{
    public class RelationalQueryCompilationContext : QueryCompilationContext
    {
        private readonly List<RelationalQueryModelVisitor> _relationalQueryModelVisitors
            = new List<RelationalQueryModelVisitor>();

        public RelationalQueryCompilationContext(
            [NotNull] IModel model,
            [NotNull] ILogger logger,
            [NotNull] ILinqOperatorProvider linqOperatorProvider,
            [NotNull] IResultOperatorHandler resultOperatorHandler,
            [NotNull] IEntityMaterializerSource entityMaterializerSource,
            [NotNull] IEntityKeyFactorySource entityKeyFactorySource,
            [NotNull] IClrAccessorSource<IClrPropertyGetter> clrPropertyGetterSource,
            [NotNull] IQueryMethodProvider queryMethodProvider,
            [NotNull] IMethodCallTranslator compositeMethodCallTranslator,
            [NotNull] IMemberTranslator compositeMemberTranslator,
            [NotNull] IRelationalValueBufferFactoryFactory valueBufferFactoryFactory)
            : base(
                Check.NotNull(model, nameof(model)),
                Check.NotNull(logger, nameof(logger)),
                Check.NotNull(linqOperatorProvider, nameof(linqOperatorProvider)),
                Check.NotNull(resultOperatorHandler, nameof(resultOperatorHandler)),
                Check.NotNull(entityMaterializerSource, nameof(entityMaterializerSource)),
                Check.NotNull(entityKeyFactorySource, nameof(entityKeyFactorySource)),
                Check.NotNull(clrPropertyGetterSource, nameof(clrPropertyGetterSource)))
        {
            Check.NotNull(queryMethodProvider, nameof(queryMethodProvider));
            Check.NotNull(compositeMethodCallTranslator, nameof(compositeMethodCallTranslator));
            Check.NotNull(compositeMemberTranslator, nameof(compositeMemberTranslator));
            Check.NotNull(valueBufferFactoryFactory, nameof(valueBufferFactoryFactory));

            QueryMethodProvider = queryMethodProvider;
            CompositeMethodCallTranslator = compositeMethodCallTranslator;
            CompositeMemberTranslator = compositeMemberTranslator;
            ValueBufferFactoryFactory = valueBufferFactoryFactory;
        }

        public override EntityQueryModelVisitor CreateQueryModelVisitor(
            EntityQueryModelVisitor parentEntityQueryModelVisitor)
        {
            var relationalQueryModelVisitor
                = new RelationalQueryModelVisitor(
                    this, (RelationalQueryModelVisitor)parentEntityQueryModelVisitor);

            _relationalQueryModelVisitors.Add(relationalQueryModelVisitor);

            return relationalQueryModelVisitor;
        }

        public virtual SelectExpression FindSelectExpression([NotNull] IQuerySource querySource)
        {
            Check.NotNull(querySource, nameof(querySource));

            return
                (from v in _relationalQueryModelVisitors
                    let selectExpression = v.TryGetQuery(querySource)
                    where selectExpression != null
                    select selectExpression)
                    .Single();
        }

        public virtual IQueryMethodProvider QueryMethodProvider { get; }

        public virtual IMethodCallTranslator CompositeMethodCallTranslator { get; }

        public virtual IMemberTranslator CompositeMemberTranslator { get; }

        public virtual IRelationalValueBufferFactoryFactory ValueBufferFactoryFactory { get; }

        public virtual ISqlQueryGenerator CreateSqlQueryGenerator([NotNull] SelectExpression selectExpression)
        {
            Check.NotNull(selectExpression, nameof(selectExpression));

            return new DefaultQuerySqlGenerator(selectExpression);
        }

        public virtual string GetTableName([NotNull] IEntityType entityType)
        {
            Check.NotNull(entityType, nameof(entityType));

            return entityType.Relational().Table;
        }

        public virtual string GetSchema([NotNull] IEntityType entityType)
        {
            Check.NotNull(entityType, nameof(entityType));

            return entityType.Relational().Schema;
        }

        public virtual string GetColumnName([NotNull] IProperty property)
        {
            Check.NotNull(property, nameof(property));

            return property.Relational().Column;
        }
    }
}
