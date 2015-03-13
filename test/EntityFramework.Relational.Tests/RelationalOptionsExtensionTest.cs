﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data.Common;
using Microsoft.Data.Entity.Infrastructure;
using Moq;
using Xunit;
using CoreStrings = Microsoft.Data.Entity.Internal.Strings;

namespace Microsoft.Data.Entity.Relational.Tests
{
    public class RelationalOptionsExtensionTest
    {
        private const string ConnectionStringKey = "ConnectionString";
        private const string CommandTimeoutKey = "CommandTimeout";
        private const string MaxBatchSizeKey = "MaxBatchSize";

        private const string ConnectionString = "Fraggle=Rock";

        [Fact]
        public void Can_set_Connection()
        {
            var optionsExtension = new TestRelationalOptionsExtension(CreateOptions(new Dictionary<string, string>()));

            Assert.Null(optionsExtension.Connection);

            var connection = Mock.Of<DbConnection>();
            optionsExtension.Connection = connection;

            Assert.Same(connection, optionsExtension.Connection);
        }

        [Fact]
        public void Throws_when_setting_Connection_to_null()
        {
            Assert.Throws<ArgumentNullException>(() => { new TestRelationalOptionsExtension(CreateOptions(new Dictionary<string, string>())).Connection = null; });
        }

        [Fact]
        public void Can_set_ConnectionString()
        {
            var optionsExtension = new TestRelationalOptionsExtension(CreateOptions(new Dictionary<string, string>()));

            Assert.Null(optionsExtension.ConnectionString);

            optionsExtension.ConnectionString = ConnectionString;

            Assert.Equal(ConnectionString, optionsExtension.ConnectionString);
        }

        [Fact]
        public void Throws_when_setting_ConnectionString_to_null()
        {
            Assert.Throws<ArgumentNullException>(() => { new TestRelationalOptionsExtension(CreateOptions(new Dictionary<string, string>())).ConnectionString = null; });
        }

        [Fact]
        public void Configure_sets_ConnectionString_to_value_specified_in_raw_options()
        {
            var rawOptions = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { { ConnectionStringKey, ConnectionString } };
            var optionsExtension = new TestRelationalOptionsExtension(CreateOptions(rawOptions));

            Assert.Equal(ConnectionString, optionsExtension.ConnectionString);
        }

        [Fact]
        public void Configure_does_not_set_ConnectionString_if_value_already_set()
        {
            const string originalConnectionString = "The=Doozers";
            var rawOptions = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { { ConnectionStringKey, ConnectionString } };

            var optionsExtension = new TestRelationalOptionsExtension(CreateOptions(rawOptions)) { ConnectionString = originalConnectionString };

            Assert.Equal(originalConnectionString, optionsExtension.ConnectionString);
        }

        [Fact]
        public void Configure_does_not_set_ConnectionString_if_not_specified_in_raw_options()
        {
            var rawOptions = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var optionsExtension = new TestRelationalOptionsExtension(CreateOptions(rawOptions));

            Assert.Null(optionsExtension.ConnectionString);
        }

        [Fact]
        public void Can_set_CommandTimeout()
        {
            var optionsExtension = new TestRelationalOptionsExtension(CreateOptions(new Dictionary<string, string>()));

            Assert.Null(optionsExtension.CommandTimeout);

            optionsExtension.CommandTimeout = 1;

            Assert.Equal(1, optionsExtension.CommandTimeout);
        }

        [Fact]
        public void Throws_if_CommandTimeout_out_of_range()
        {
            Assert.Equal(
                Strings.InvalidCommandTimeout,
                Assert.Throws<InvalidOperationException>(
                    () => { new TestRelationalOptionsExtension(CreateOptions(new Dictionary<string, string>())).CommandTimeout = -1; }).Message);
        }

        [Fact]
        public void Configure_sets_CommandTimeout_to_value_specified_in_raw_options()
        {
            var rawOptions = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { { CommandTimeoutKey, "1" } };
            var optionsExtension = new TestRelationalOptionsExtension(CreateOptions(rawOptions));

            Assert.Equal(1, optionsExtension.CommandTimeout);
        }

        [Fact]
        public void Configure_does_not_set_CommandTimeout_if_value_already_set()
        {
            var rawOptions = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { { CommandTimeoutKey, "1" } };

            var optionsExtension = new TestRelationalOptionsExtension(CreateOptions(rawOptions)) { CommandTimeout = 42 };

            Assert.Equal(42, optionsExtension.CommandTimeout);
        }

        [Fact]
        public void Configure_does_not_set_CommandTimeout_if_not_specified_in_raw_options()
        {
            var rawOptions = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var optionsExtension = new TestRelationalOptionsExtension(CreateOptions(rawOptions));

            Assert.Null(optionsExtension.CommandTimeout);
        }

        [Fact]
        public void Configure_throws_if_CommandTimeout_specified_in_raw_options_is_invalid()
        {
            var rawOptions = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { { CommandTimeoutKey, "one" } };

            Assert.Equal(
                CoreStrings.IntegerConfigurationValueFormatError(CommandTimeoutKey, "one"),
                Assert.Throws<InvalidOperationException>(
                    () => new TestRelationalOptionsExtension(CreateOptions(rawOptions))).Message);
        }

        [Fact]
        public void Can_set_MaxBatchSize()
        {
            var optionsExtension = new TestRelationalOptionsExtension((CreateOptions(new Dictionary<string, string>())));

            Assert.Null(optionsExtension.MaxBatchSize);

            optionsExtension.MaxBatchSize = 1;

            Assert.Equal(1, optionsExtension.MaxBatchSize);
        }

        [Fact]
        public void Throws_if_MaxBatchSize_out_of_range()
        {
            Assert.Equal(
                Strings.InvalidMaxBatchSize,
                Assert.Throws<InvalidOperationException>(
                    () => { new TestRelationalOptionsExtension((CreateOptions(new Dictionary<string, string>()))).MaxBatchSize = -1; }).Message);
        }

        [Fact]
        public void Configure_sets_MaxBatchSize_to_value_specified_in_raw_options()
        {
            var rawOptions = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { { MaxBatchSizeKey, "1" } };
            var optionsExtension = new TestRelationalOptionsExtension(CreateOptions(rawOptions));

            Assert.Equal(1, optionsExtension.MaxBatchSize);
        }

        [Fact]
        public void Configure_does_not_set_MaxBatchSize_if_value_already_set()
        {
            var rawOptions = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { { MaxBatchSizeKey, "1" } };

            var optionsExtension = new TestRelationalOptionsExtension(CreateOptions(rawOptions)) { MaxBatchSize = 42 };

            Assert.Equal(42, optionsExtension.MaxBatchSize);
        }

        [Fact]
        public void Configure_does_not_set_MaxBatchSize_if_not_specified_in_raw_options()
        {
            var rawOptions = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var optionsExtension = new TestRelationalOptionsExtension(CreateOptions(rawOptions));

            Assert.Null(optionsExtension.MaxBatchSize);
        }

        [Fact]
        public void Configure_throws_if_MaxBatchSize_specified_in_raw_options_is_invalid()
        {
            var rawOptions = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { { MaxBatchSizeKey, "one" } };

            Assert.Equal(
                CoreStrings.IntegerConfigurationValueFormatError(MaxBatchSizeKey, "one"),
                Assert.Throws<InvalidOperationException>(
                    () => new TestRelationalOptionsExtension(CreateOptions(rawOptions))).Message);
        }

        private static DbContextOptions<DbContext> CreateOptions(IReadOnlyDictionary<string, string> rawOptions)
        {
            return new DbContextOptions<DbContext>(rawOptions, new Dictionary<Type, IDbContextOptionsExtension>(), null);
        }

        private class TestRelationalOptionsExtension : RelationalOptionsExtension
        {
            public TestRelationalOptionsExtension(IDbContextOptions options)
                : base(options)
            {
            }

            public override void ApplyServices(EntityFrameworkServicesBuilder builder)
            {
                throw new NotImplementedException();
            }
        }
    }
}
