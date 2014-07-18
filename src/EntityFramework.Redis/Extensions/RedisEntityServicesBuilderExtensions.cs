﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity;
using Microsoft.Data.Entity.Redis;
using Microsoft.Data.Entity.Redis.Utilities;
using Microsoft.Data.Entity.Storage;

namespace Microsoft.Framework.DependencyInjection
{
    public static class RedisEntityServicesBuilderExtensions
    {
        public static EntityServicesBuilder AddRedis([NotNull] this EntityServicesBuilder builder)
        {
            Check.NotNull(builder, "builder");

            builder.ServiceCollection
                .AddSingleton<DataStoreSource, RedisDataStoreSource>()
                .AddScoped<RedisOptionsExtension>()
                .AddScoped<RedisDataStore>()
                .AddScoped<RedisConnection>()
                .AddScoped<RedisDataStoreCreator>()
                .AddScoped<RedisDatabase>();

            return builder;
        }
    }
}
