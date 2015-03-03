﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Data.Entity.Storage;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.Redis
{
    public class RedisConnection : DataStoreConnection
    {
        private readonly string _connectionString;
        private readonly int _database = -1;

        /// <summary>
        ///     For testing. Improper usage may lead to NullReference exceptions
        /// </summary>
        protected RedisConnection()
        {
        }

        public RedisConnection([NotNull] DbContextService<IDbContextOptions> options, [NotNull] ILoggerFactory loggerFactory)
            : base(loggerFactory)
        {
            Check.NotNull(options, "options");
            Check.NotNull(loggerFactory, "loggerFactory");
            var optionsExtension = RedisOptionsExtension.Extract(options.Service);

            _connectionString = optionsExtension.HostName + ":" + optionsExtension.Port;
            _database = optionsExtension.Database;
        }

        public virtual string ConnectionString
        {
            get { return _connectionString; }
        }

        public virtual int Database
        {
            get { return _database; }
        }
    }
}
