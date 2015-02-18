﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using System.Data.SqlClient;

namespace Microsoft.Data.Entity.SqlServer.Design.ReverseEngineering.Model
{
    public class Table
    {
        public const string Query =
@"SELECT
    quotename(TABLE_SCHEMA) + quotename(TABLE_NAME) [Id]
  , TABLE_SCHEMA [SchemaName]
  , TABLE_NAME   [Name]
  FROM
  INFORMATION_SCHEMA.TABLES
  WHERE
  TABLE_TYPE = 'BASE TABLE'
";

        public virtual string Id { get; [param: CanBeNull] set; }
        public virtual string SchemaName { get; [param: CanBeNull] set; }
        public virtual string TableName { get; [param: CanBeNull] set; }

        public static Table CreateFromReader([NotNull] SqlDataReader reader)
        {
            var table = new Table();
            table.Id = reader.IsDBNull(0) ? null : reader.GetString(0);
            table.SchemaName = reader.IsDBNull(1) ? null : reader.GetString(1);
            table.TableName = reader.IsDBNull(2) ? null : reader.GetString(2);

            return table;
        }

        public override string ToString()
        {
            return "T[Id=" + Id
                + ", Schema=" + SchemaName
                + ", Name=" + TableName
                + "]";
        }
    }
}