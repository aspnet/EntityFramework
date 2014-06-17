// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.Data.Entity.AzureTableStorage.Tests.Helpers
{
    public class PocoTestType
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public string ETag { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public Guid Guid { get; set; }
        public long BigCount { get; set; }
        public int Count { get; set; }
        public string Name { get; set; }
        public double Price { get; set; }
        public DateTime CustomerSince { get; set; }
        public bool IsEnchanted { get; set; }
        public byte[] Buffer { get; set; }

        public PocoTestType NestedObj { get; set; }
    }
}
