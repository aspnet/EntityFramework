﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Core.Utilities
{
    using System.Linq;
    using System.Threading.Tasks;
    using Xunit;

    public class EnumerableExtensionsFacts
    {
        [Fact]
        public void OrderByOrdinal_should_respect_case()
        {
            Assert.Equal(new[] { "A", "a", "b" }, new[] { "b", "A", "a" }.OrderByOrdinal(s => s));
        }

        [Fact]
        public void Join_empty_input_returns_empty_string()
        {
            Assert.Equal("", new object[] { }.Join());
        }

        [Fact]
        public void Join_single_element_does_not_use_separator()
        {
            Assert.Equal("42", new object[] { 42 }.Join());
        }

        [Fact]
        public void Join_should_use_comma_by_default()
        {
            Assert.Equal("42, bar", new object[] { 42, "bar" }.Join());
        }

        [Fact]
        public void Join_should_use_explicit_separator_when_provided()
        {
            Assert.Equal("42-bar", new object[] { 42, "bar" }.Join("-"));
        }

        [Fact]
        public async Task SelectAsync_should_apply_selector_over_sequence()
        {
            Assert.Equal(
                new[] { "aa", "bb" },
                await new[] { "AA", "BB" }.SelectAsync(s => Task.FromResult(s.ToLower())));
        }

        [Fact]
        public async Task SelectManyAsync_should_apply_selector_over_inner_sequences()
        {
            Assert.Equal(
                new[] { "a", "b", "c", "d" },
                await new[] { "ab", "cd" }
                    .SelectManyAsync(s => Task.FromResult(s.Select(c => c.ToString()))));
        }

        [Fact]
        public async Task WhereAsync_should_apply_filter_over_sequence()
        {
            Assert.Equal(
                new[] { "BB" },
                await new[] { "AA", "BB" }.WhereAsync(s => Task.FromResult(s.StartsWith("B"))));
        }
    }
}
