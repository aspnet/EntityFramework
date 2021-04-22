// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.Update.Internal;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Update
{
    public class ModificationCommandComparerTest
    {
        [ConditionalFact]
        public void Compare_returns_0_only_for_commands_that_are_equal()
        {
            var modelBuilder = new ModelBuilder(TestRelationalConventionSetBuilder.Build());
            var model = modelBuilder.Model;
            var entityType = model.AddEntityType(typeof(object));
            var key = entityType.AddProperty("Id", typeof(int));
            entityType.SetPrimaryKey(key);

            var optionsBuilder = new DbContextOptionsBuilder()
                .UseModel(RelationalTestHelpers.Instance.Finalize(modelBuilder))
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .UseInternalServiceProvider(InMemoryFixture.DefaultServiceProvider);

            var stateManager = new DbContext(optionsBuilder.Options).GetService<IStateManager>();

            var modificationCommandFactory = new ModificationCommandFactory();
            var columnModificationFactory = new ColumnModificationFactory();

            var entry1 = stateManager.GetOrCreateEntry(new object());
            entry1[(IProperty)key] = 1;
            entry1.SetEntityState(EntityState.Added);
            var modificationCmdBuilderAdded = new ModificationCommandBuilder("A", null, new ParameterNameGenerator().GenerateNext, false, null, modificationCommandFactory, columnModificationFactory);
            modificationCmdBuilderAdded.AddEntry(entry1, true);

            var entry2 = stateManager.GetOrCreateEntry(new object());
            entry2[(IProperty)key] = 2;
            entry2.SetEntityState(EntityState.Modified);
            var modificationCmdBuilderModified = new ModificationCommandBuilder("A", null, new ParameterNameGenerator().GenerateNext, false, null, modificationCommandFactory, columnModificationFactory);
            modificationCmdBuilderModified.AddEntry(entry2, true);

            var entry3 = stateManager.GetOrCreateEntry(new object());
            entry3[(IProperty)key] = 3;
            entry3.SetEntityState(EntityState.Deleted);
            var modificationCmdBuilderDeleted = new ModificationCommandBuilder("A", null, new ParameterNameGenerator().GenerateNext, false, null, modificationCommandFactory, columnModificationFactory);
            modificationCmdBuilderDeleted.AddEntry(entry3, true);

            var mCC = new ModificationCommandComparer();

            Assert.Same(modificationCmdBuilderAdded.GetModificationCommand(), modificationCmdBuilderAdded.GetModificationCommand());

            Assert.True(0 == mCC.Compare(modificationCmdBuilderAdded.GetModificationCommand(), modificationCmdBuilderAdded.GetModificationCommand()));
            Assert.True(0 == mCC.Compare(null, null));
            Assert.True(
                0
                == mCC.Compare(
                    new ModificationCommand("A", "dbo", new ParameterNameGenerator().GenerateNext, false, null, columnModificationFactory),
                    new ModificationCommand("A", "dbo", new ParameterNameGenerator().GenerateNext, false, null, columnModificationFactory)));

            Assert.True(0 > mCC.Compare(null, new ModificationCommand("A", null, new ParameterNameGenerator().GenerateNext, false, null, columnModificationFactory)));
            Assert.True(0 < mCC.Compare(new ModificationCommand("A", null, new ParameterNameGenerator().GenerateNext, false, null, columnModificationFactory), null));

            Assert.True(
                0
                > mCC.Compare(
                    new ModificationCommand("A", null, new ParameterNameGenerator().GenerateNext, false, null, columnModificationFactory),
                    new ModificationCommand("A", "dbo", new ParameterNameGenerator().GenerateNext, false, null, columnModificationFactory)));
            Assert.True(
                0
                < mCC.Compare(
                    new ModificationCommand("A", "dbo", new ParameterNameGenerator().GenerateNext, false, null, columnModificationFactory),
                    new ModificationCommand("A", null, new ParameterNameGenerator().GenerateNext, false, null, columnModificationFactory)));

            Assert.True(
                0
                > mCC.Compare(
                    new ModificationCommand("A", "dbo", new ParameterNameGenerator().GenerateNext, false, null, columnModificationFactory),
                    new ModificationCommand("A", "foo", new ParameterNameGenerator().GenerateNext, false, null, columnModificationFactory)));
            Assert.True(
                0
                < mCC.Compare(
                    new ModificationCommand("A", "foo", new ParameterNameGenerator().GenerateNext, false, null, columnModificationFactory),
                    new ModificationCommand("A", "dbo", new ParameterNameGenerator().GenerateNext, false, null, columnModificationFactory)));

            Assert.True(
                0
                > mCC.Compare(
                    new ModificationCommand("A", null, new ParameterNameGenerator().GenerateNext, false, null, columnModificationFactory),
                    new ModificationCommand("B", null, new ParameterNameGenerator().GenerateNext, false, null, columnModificationFactory)));
            Assert.True(
                0
                < mCC.Compare(
                    new ModificationCommand("B", null, new ParameterNameGenerator().GenerateNext, false, null, columnModificationFactory),
                    new ModificationCommand("A", null, new ParameterNameGenerator().GenerateNext, false, null, columnModificationFactory)));

            Assert.True(0 > mCC.Compare(modificationCmdBuilderModified.GetModificationCommand(), modificationCmdBuilderAdded.GetModificationCommand()));
            Assert.True(0 < mCC.Compare(modificationCmdBuilderAdded.GetModificationCommand(), modificationCmdBuilderModified.GetModificationCommand()));

            Assert.True(0 > mCC.Compare(modificationCmdBuilderDeleted.GetModificationCommand(), modificationCmdBuilderAdded.GetModificationCommand()));
            Assert.True(0 < mCC.Compare(modificationCmdBuilderAdded.GetModificationCommand(), modificationCmdBuilderDeleted.GetModificationCommand()));

            Assert.True(0 > mCC.Compare(modificationCmdBuilderDeleted.GetModificationCommand(), modificationCmdBuilderModified.GetModificationCommand()));
            Assert.True(0 < mCC.Compare(modificationCmdBuilderModified.GetModificationCommand(), modificationCmdBuilderDeleted.GetModificationCommand()));
        }

        [ConditionalFact]
        public void Compare_returns_0_only_for_entries_that_have_same_key_values()
        {
            Compare_returns_0_only_for_entries_that_have_same_key_values_generic<short>(-1, 1);
            Compare_returns_0_only_for_entries_that_have_same_key_values_generic(1, 2);
            Compare_returns_0_only_for_entries_that_have_same_key_values_generic<long>(1, 2);
            Compare_returns_0_only_for_entries_that_have_same_key_values_generic<double>(1, 2);
            Compare_returns_0_only_for_entries_that_have_same_key_values_generic<decimal>(1, 2);
            Compare_returns_0_only_for_entries_that_have_same_key_values_generic<float>(1, 2);
            Compare_returns_0_only_for_entries_that_have_same_key_values_generic<byte>(1, 2);
            Compare_returns_0_only_for_entries_that_have_same_key_values_generic<ushort>(1, 2);
            Compare_returns_0_only_for_entries_that_have_same_key_values_generic<uint>(1, 2);
            Compare_returns_0_only_for_entries_that_have_same_key_values_generic<ulong>(1, 2);
            Compare_returns_0_only_for_entries_that_have_same_key_values_generic<sbyte>(1, 2);
            Compare_returns_0_only_for_entries_that_have_same_key_values_generic(false, true);
            Compare_returns_0_only_for_entries_that_have_same_key_values_generic('1', '2');
            Compare_returns_0_only_for_entries_that_have_same_key_values_generic(new DateTime(1, 1, 1), new DateTime(1, 1, 2));
            Compare_returns_0_only_for_entries_that_have_same_key_values_generic(
                new DateTimeOffset(new DateTime(10, 1, 1), TimeSpan.FromMinutes(2)),
                new DateTimeOffset(new DateTime(10, 1, 1), TimeSpan.FromMinutes(1)));
            Compare_returns_0_only_for_entries_that_have_same_key_values_generic(TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(2));
            Compare_returns_0_only_for_entries_that_have_same_key_values_generic(new Guid(), Guid.NewGuid());
            Compare_returns_0_only_for_entries_that_have_same_key_values_generic(FlagsEnum.First, FlagsEnum.First | FlagsEnum.Second);

            Compare_returns_0_only_for_entries_that_have_same_key_values_generic<short?>(-1, 1);
            Compare_returns_0_only_for_entries_that_have_same_key_values_generic<int?>(1, 2);
            Compare_returns_0_only_for_entries_that_have_same_key_values_generic<long?>(1, 2);
            Compare_returns_0_only_for_entries_that_have_same_key_values_generic<double?>(1, 2);
            Compare_returns_0_only_for_entries_that_have_same_key_values_generic<decimal?>(1, 2);
            Compare_returns_0_only_for_entries_that_have_same_key_values_generic<float?>(1, 2);
            Compare_returns_0_only_for_entries_that_have_same_key_values_generic<byte?>(1, 2);
            Compare_returns_0_only_for_entries_that_have_same_key_values_generic<ushort?>(1, 2);
            Compare_returns_0_only_for_entries_that_have_same_key_values_generic<uint?>(1, 2);
            Compare_returns_0_only_for_entries_that_have_same_key_values_generic<ulong?>(1, 2);
            Compare_returns_0_only_for_entries_that_have_same_key_values_generic<sbyte?>(1, 2);
            Compare_returns_0_only_for_entries_that_have_same_key_values_generic<bool?>(false, true);
            Compare_returns_0_only_for_entries_that_have_same_key_values_generic<char?>('1', '2');
            Compare_returns_0_only_for_entries_that_have_same_key_values_generic<DateTime?>(new DateTime(1, 1, 1), new DateTime(1, 1, 2));
            Compare_returns_0_only_for_entries_that_have_same_key_values_generic<DateTimeOffset?>(
                new DateTimeOffset(new DateTime(10, 1, 1), TimeSpan.FromMinutes(2)),
                new DateTimeOffset(new DateTime(10, 1, 1), TimeSpan.FromMinutes(1)));
            Compare_returns_0_only_for_entries_that_have_same_key_values_generic<TimeSpan?>(
                TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(2));
            Compare_returns_0_only_for_entries_that_have_same_key_values_generic<Guid?>(new Guid(), Guid.NewGuid());
            Compare_returns_0_only_for_entries_that_have_same_key_values_generic<FlagsEnum?>(
                FlagsEnum.Default, FlagsEnum.First | FlagsEnum.Second);

            Compare_returns_0_only_for_entries_that_have_same_key_values_generic(new Guid().ToByteArray(), Guid.NewGuid().ToByteArray());

            Compare_returns_0_only_for_entries_that_have_same_key_values_generic("1", "2");
        }

        private void Compare_returns_0_only_for_entries_that_have_same_key_values_generic<T>(T value1, T value2)
        {
            var modelBuilder = new ModelBuilder(TestRelationalConventionSetBuilder.Build());
            var model = modelBuilder.Model;
            var entityType = model.AddEntityType(typeof(object));

            var keyProperty = entityType.AddProperty("Id", typeof(T));
            keyProperty.IsNullable = false;
            entityType.SetPrimaryKey(keyProperty);

            var optionsBuilder = new DbContextOptionsBuilder()
                .UseInternalServiceProvider(InMemoryFixture.DefaultServiceProvider)
                .UseModel(RelationalTestHelpers.Instance.Finalize(modelBuilder))
                .UseInMemoryDatabase(Guid.NewGuid().ToString());

            var stateManager = new DbContext(optionsBuilder.Options).GetService<IStateManager>();

            var modificationCommandFactory = new ModificationCommandFactory();
            var columnModificationFactory = new ColumnModificationFactory();

            var entry1 = stateManager.GetOrCreateEntry(new object());
            entry1[(IProperty)keyProperty] = value1;
            entry1.SetEntityState(EntityState.Modified);
            var modificationCmdBuilder1 = new ModificationCommandBuilder("A", null, new ParameterNameGenerator().GenerateNext, false, null, modificationCommandFactory, columnModificationFactory);
            modificationCmdBuilder1.AddEntry(entry1, true);

            var entry2 = stateManager.GetOrCreateEntry(new object());
            entry2[(IProperty)keyProperty] = value2;
            entry2.SetEntityState(EntityState.Modified);
            var modificationCmdBuilder2 = new ModificationCommandBuilder("A", null, new ParameterNameGenerator().GenerateNext, false, null, modificationCommandFactory, columnModificationFactory);
            modificationCmdBuilder2.AddEntry(entry2, true);

            var modificationCmdBuilder3 = new ModificationCommandBuilder("A", null, new ParameterNameGenerator().GenerateNext, false, null, modificationCommandFactory, columnModificationFactory);
            modificationCmdBuilder3.AddEntry(entry1, true);

            var mCC = new ModificationCommandComparer();

            Assert.True(0 > mCC.Compare(modificationCmdBuilder1.GetModificationCommand(), modificationCmdBuilder2.GetModificationCommand()));
            Assert.True(0 < mCC.Compare(modificationCmdBuilder2.GetModificationCommand(), modificationCmdBuilder1.GetModificationCommand()));
            Assert.True(0 == mCC.Compare(modificationCmdBuilder1.GetModificationCommand(), modificationCmdBuilder3.GetModificationCommand()));
        }

        [Flags]
        private enum FlagsEnum
        {
            Default = 0,
            First = 1 << 0,
            Second = 1 << 2
        }
    }
}
