// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;

namespace Microsoft.EntityFrameworkCore.Migrations.Operations
{
    /// <summary>
    ///     A <see cref="MigrationOperation" /> for dropping an existing column.
    /// </summary>
    [DebuggerDisplay("ALTER TABLE {Table} DROP COLUMN {Name}")]
    public class DropColumnOperation : ColumnOperation, ITableMigrationOperation
    {
        /// <summary>
        ///     Creates a new instance of the <see cref="DropColumnOperation" />.
        /// </summary>
        // ReSharper disable once VirtualMemberCallInConstructor
        public DropColumnOperation()
            => IsDestructiveChange = true;
    }
}
