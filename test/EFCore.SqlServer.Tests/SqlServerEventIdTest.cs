// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Extensions.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Internal;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore
{
    public class SqlServerEventIdTest : EventIdTestBase
    {
        [ConditionalFact]
        public void Every_eventId_has_a_logger_method_and_logs_when_level_enabled()
        {
            var entityType = new EntityType(typeof(object), new Model(new ConventionSet()), ConfigurationSource.Convention);
            var property = new Property(
                "A", typeof(int), null, null, entityType, ConfigurationSource.Convention, ConfigurationSource.Convention);
            entityType.Model.FinalizeModel();

            var fakeFactories = new Dictionary<Type, Func<object>>
            {
                { typeof(IList<string>), () => new List<string> { "Fake1", "Fake2" } },
                { typeof(IProperty), () => property },
                { typeof(IReadOnlyProperty), () => property },
                { typeof(string), () => "Fake" }
            };

            TestEventLogging(
                typeof(SqlServerEventId),
                typeof(SqlServerLoggerExtensions),
                new SqlServerLoggingDefinitions(),
                fakeFactories);
        }
    }
}
