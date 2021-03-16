// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Text;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Represents property mapping to a column.
    /// </summary>
    public interface IColumnMapping : IColumnMappingBase
    {
        /// <summary>
        ///     Gets the target column.
        /// </summary>
        new IColumn Column { get; }

        /// <summary>
        ///     Gets the containing table mapping.
        /// </summary>
        new ITableMapping TableMapping { get; }

        /// <summary>
        ///     <para>
        ///         Creates a human-readable representation of the given metadata.
        ///     </para>
        ///     <para>
        ///         Warning: Do not rely on the format of the returned string.
        ///         It is designed for debugging only and may change arbitrarily between releases.
        ///     </para>
        /// </summary>
        /// <param name="options"> Options for generating the string. </param>
        /// <param name="indent"> The number of indent spaces to use before each new line. </param>
        /// <returns> A human-readable representation. </returns>
        string ToDebugString(MetadataDebugStringOptions options, int indent = 0)
        {
            var builder = new StringBuilder();
            var indentString = new string(' ', indent);

            builder.Append(indentString);

            var singleLine = (options & MetadataDebugStringOptions.SingleLine) != 0;
            if (singleLine)
            {
                builder.Append("ColumnMapping: ");
            }

            builder
                .Append(Property.DeclaringEntityType.DisplayName())
                .Append(".")
                .Append(Property.Name)
                .Append(" - ");

            builder
                .Append(Column.Table.Name)
                .Append(".")
                .Append(Column.Name);

            if (!singleLine && (options & MetadataDebugStringOptions.IncludeAnnotations) != 0)
            {
                builder.Append(AnnotationsToDebugString(indent + 2));
            }

            return builder.ToString();
        }
    }
}
