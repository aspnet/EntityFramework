﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.SQLite.Utilities;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.Data.Entity.SQLite
{
    public class SQLiteConfigurationExtension : RelationalConfigurationExtension
    {
        protected override void ApplyServices(EntityServicesBuilder builder)
        {
            Check.NotNull(builder, "builder");

            builder.AddSQLite();
        }
    }
}
