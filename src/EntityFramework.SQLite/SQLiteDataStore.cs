﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Query;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.Relational.Query;
using Microsoft.Data.Entity.Relational.Query.Methods;
using Microsoft.Data.Entity.SQLite.Query;
using Microsoft.Data.Entity.SQLite.Utilities;

namespace Microsoft.Data.Entity.SQLite
{
    public class SQLiteDataStore : RelationalDataStore
    {
        public SQLiteDataStore(
            [NotNull] DbContextConfiguration configuration,
            [NotNull] SQLiteConnection connection,
            [NotNull] SQLiteCommandBatchPreparer batchPreparer,
            [NotNull] SQLiteBatchExecutor batchExecutor)
            : base(configuration, connection, batchPreparer, batchExecutor)
        {
        }

        protected override RelationalValueReaderFactory ValueReaderFactory
        {
            get { return new RelationalObjectArrayValueReaderFactory(); }
        }

        protected override RelationalQueryCompilationContext CreateQueryCompilationContext(
            ILinqOperatorProvider linqOperatorProvider,
            IResultOperatorHandler resultOperatorHandler,
            IQueryMethodProvider queryMethodProvider,
            IMethodCallTranslator methodCallTranslator)
        {
            Check.NotNull(linqOperatorProvider, "linqOperatorProvider");
            Check.NotNull(resultOperatorHandler, "resultOperatorHandler");
            Check.NotNull(queryMethodProvider, "queryMethodProvider");

            return new SQLiteQueryCompilationContext(
                Model, 
                Logger, 
                linqOperatorProvider, 
                resultOperatorHandler, 
                EntityMaterializerSource,
                queryMethodProvider, 
                methodCallTranslator);
        }
    }
}
