﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Entity.Relational.Query;
using Microsoft.Data.Entity.Relational.Query.Methods;
using Microsoft.Data.Entity.Sqlite.Query.Methods;
using Microsoft.Framework.Logging;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Sqlite
{
    public class SqliteCompositeMethodCallTranslator : RelationalCompositeMethodCallTranslator
    {
        private readonly List<IMethodCallTranslator> _sqliteTranslators = new List<IMethodCallTranslator>()
        {
            new MathAbsTranslator(),
            new StringToLowerTranslator(),
            new StringToUpperTranslator(),
        };

        public SqliteCompositeMethodCallTranslator([NotNull] ILoggerFactory loggerFactory)
            : base(loggerFactory)
        {
        }

        protected override IReadOnlyList<IMethodCallTranslator> Translators
            => base.Translators.Concat(_sqliteTranslators).ToList();
    }
}
