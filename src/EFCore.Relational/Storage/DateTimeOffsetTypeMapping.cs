// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data;
using System.Globalization;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Storage
{
    /// <summary>
    ///     <para>
    ///         Represents the mapping between a .NET <see cref="DateTimeOffset" /> type and a database type.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public class DateTimeOffsetTypeMapping : RelationalTypeMapping<DateTimeOffset>
    {
        private const string DateTimeOffsetFormatConst = @"yyyy-MM-dd HH\:mm\:ss.fffffffzzz";

        /// <summary>
        ///     Initializes a new instance of the <see cref="DateTimeOffsetTypeMapping" /> class.
        /// </summary>
        /// <param name="storeType"> The name of the database type. </param>
        public DateTimeOffsetTypeMapping([NotNull] string storeType)
            : this(storeType, dbType: System.Data.DbType.DateTimeOffset, unicode: false, size: null)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DateTimeOffsetTypeMapping" /> class.
        /// </summary>
        /// <param name="storeType"> The name of the database type. </param>
        /// <param name="dbType"> The <see cref="System.Data.DbType" /> to be used. </param>
        /// <param name="unicode"> A value indicating whether the type should handle Unicode data or not. </param>
        /// <param name="size"> The size of data the property is configured to store, or null if no size is configured. </param>
        /// <param name="hasNonDefaultUnicode"> A value indicating whether the Unicode setting has been manually configured to a non-default value. </param>
        /// <param name="hasNonDefaultSize"> A value indicating whether the size setting has been manually configured to a non-default value. </param>
        public DateTimeOffsetTypeMapping(
            [NotNull] string storeType,
            [CanBeNull] DbType? dbType,
            bool unicode,
            int? size,
            bool hasNonDefaultUnicode = false,
            bool hasNonDefaultSize = false)
            : base(storeType, dbType, unicode, size, hasNonDefaultUnicode, hasNonDefaultSize)
        {
        }

        /// <summary>
        ///     Creates a copy of this mapping.
        /// </summary>
        /// <param name="storeType"> The name of the database type. </param>
        /// <param name="size"> The size of data the property is configured to store, or null if no size is configured. </param>
        /// <returns> The newly created mapping. </returns>
        public override RelationalTypeMapping CreateCopyT([NotNull] string storeType, int? size)
            => new DateTimeOffsetTypeMapping(
                storeType,
                DbType,
                IsUnicode,
                size,
                HasNonDefaultUnicode,
                hasNonDefaultSize: size != Size);

        /// <summary>
        ///     Gets the date time offset format.
        /// </summary>
        protected override string DateTimeOffsetFormat => DateTimeOffsetFormatConst;

        /// <summary>
        ///     Generates the SQL representation of a literal value.
        /// </summary>
        /// <param name="value">The literal value.</param>
        /// <returns>
        ///     The generated string.
        /// </returns>
        public override string GenerateSqlLiteral([CanBeNull]object value)
        {
            return value != null
                ? $"TIMESTAMP '{((DateTimeOffset)value).ToString(DateTimeOffsetFormat, CultureInfo.InvariantCulture)}'" // Interpolation okay; strings
                : base.GenerateSqlLiteral(value);
        }
    }
}
