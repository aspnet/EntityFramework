﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.Data.Entity.Infrastructure;
using Xunit;

namespace Microsoft.Data.Entity.AzureTableStorage.Tests
{
    public class AtsDbContextExtensionsTests
    {
        [Fact]
        public void It_constructs_connection_string()
        {
            var options = new DbContextOptions();

            AtsDbContextExtensions.UseAzureTableStorage(options, "Moria", "mellon");

            var result = GetAtsExtension(options);
            Assert.NotNull(result);
            Assert.Equal("DefaultEndpointsProtocol=https;AccountName=Moria;AccountKey=mellon;", result.ConnectionString);
        }

        [Fact]
        public void It_passes_through_connection_settings()
        {
            var options = new DbContextOptions();
            var connectionString = "Speak friend and enter";

            AtsDbContextExtensions.UseAzureTableStorage(options, connectionString, false);

            var result = GetAtsExtension(options);
            Assert.NotNull(result);
            Assert.Equal(connectionString, result.ConnectionString);
            Assert.False(result.UseBatching);
        }

        [Fact]
        public void It_sets_batching_options()
        {
            var options = new DbContextOptions();

            AtsDbContextExtensions.UseAzureTableStorage(options, "not empty", true);

            var result = GetAtsExtension(options);
            Assert.NotNull(result);
            Assert.True(result.UseBatching);
        }

        [Theory]
        [InlineData("","key","accountName")]
        [InlineData("name","","accountKey")]
        public void It_disallows_empty_account_creds(string name, string key, string paramName)
        {
            var options = new DbContextOptions();

            Assert.Equal(
                Strings.FormatArgumentIsEmpty(paramName),
                Assert.Throws<ArgumentException>(() => AtsDbContextExtensions.UseAzureTableStorage(options, name,key)).Message
                );
 
        }

        [Fact]
        public void It_disallows_empty_connection_strings()
        {
            var options = new DbContextOptions();

            Assert.Equal(
                Strings.FormatArgumentIsEmpty("connectionString"),
                Assert.Throws<ArgumentException>(() => AtsDbContextExtensions.UseAzureTableStorage(options, "")).Message
                );
        }

        private static AtsOptionsExtension GetAtsExtension(DbContextOptions options)
        {
            var result = ((IDbContextOptionsExtensions)options).Extensions
                .First(s => s.GetType() == typeof(AtsOptionsExtension)) as AtsOptionsExtension;
            return result;
        }
    }
}
