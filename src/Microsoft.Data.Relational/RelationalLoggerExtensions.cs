﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.AspNet.Logging;
using Microsoft.Data.Relational.Utilities;

namespace Microsoft.Data.Relational
{
    public static class RelationalLoggerExtensions
    {
        public static void WriteSql([NotNull] this ILogger logger, [NotNull] string sql)
        {
            Check.NotNull(logger, "logger");
            Check.NotEmpty(sql, "sql");

            logger.WriteCore(TraceType.Verbose, RelationalLoggingEventIds.Sql, sql, null, (o, _) => (string)o);
        }
    }
}
