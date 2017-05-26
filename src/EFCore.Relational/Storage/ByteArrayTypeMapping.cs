// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data;
using System.Globalization;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Storage
{
    /// <summary>
    ///     <para>
    ///         Represents the mapping between a .NET <see cref="byte" /> array type and a database type.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public class ByteArrayTypeMapping : RelationalTypeMapping<byte[]>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ByteArrayTypeMapping" /> class.
        /// </summary>
        /// <param name="storeType"> The name of the database type. </param>
        public ByteArrayTypeMapping([NotNull] string storeType)
            : this(storeType, dbType: System.Data.DbType.Binary, unicode: false, size: null)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ByteArrayTypeMapping" /> class.
        /// </summary>
        /// <param name="storeType"> The name of the database type. </param>
        /// <param name="dbType"> The <see cref="System.Data.DbType" /> to be used. </param>
        /// <param name="unicode"> A value indicating whether the type should handle Unicode data or not. </param>
        /// <param name="size"> The size of data the property is configured to store, or null if no size is configured. </param>
        /// <param name="hasNonDefaultUnicode"> A value indicating whether the Unicode setting has been manually configured to a non-default value. </param>
        /// <param name="hasNonDefaultSize"> A value indicating whether the size setting has been manually configured to a non-default value. </param>
        public ByteArrayTypeMapping(
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
            => new ByteArrayTypeMapping(
                storeType,
                DbType,
                IsUnicode,
                size,
                HasNonDefaultUnicode,
                hasNonDefaultSize: size != Size);

        /// <summary>
        ///     Generates the SQL representation of a literal value.
        /// </summary>
        /// <param name="value">The literal value.</param>
        /// <returns>
        ///     The generated string.
        /// </returns>
        public override string GenerateSqlLiteral([CanBeNull]object value)
        {
            if (value != null)
            {
                return GenerateByteArraySqlLiteral((byte[])value);
            }

            return base.GenerateSqlLiteral(value);
        }

        /// <summary>
        ///     Generates the SQL representation of a byte[] literal value.
        /// </summary>
        /// <param name="value">The literal value.</param>
        /// <returns>
        ///     The generated string.
        /// </returns>
        public static string GenerateByteArraySqlLiteral([NotNull]byte[] value)
        {
            Check.NotNull(value, nameof(value));

            if (value != null)
            {
                var stringBuilder = new StringBuilder();
                stringBuilder.Append("X'");

                foreach (var @byte in ((byte[])value))
                {
                    stringBuilder.Append(@byte.ToString("X2", CultureInfo.InvariantCulture));
                }

                stringBuilder.Append("'");
                return stringBuilder.ToString();
            }

            return null;
        }
    }
}
