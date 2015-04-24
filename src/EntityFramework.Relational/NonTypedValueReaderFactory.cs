// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.Common;
using System.Diagnostics;
using Microsoft.Data.Entity.Storage;

namespace Microsoft.Data.Entity.Relational
{
    public class NonTypedValueReaderFactory : IRelationalValueReaderFactory
    {
        private readonly int _offset;
        private readonly int _bufferSize;

        public NonTypedValueReaderFactory(int offset, int count)
        {
            _offset = offset;
            _bufferSize = offset + count;
        }

        public virtual IValueReader CreateValueReader(DbDataReader dataReader)
        {
            Debug.Assert(dataReader != null); // hot path

            var values = new object[dataReader.FieldCount];

            dataReader.GetValues(values);

            for (var i = _offset; i < _bufferSize; i++)
            {
                if (ReferenceEquals(values[i], DBNull.Value))
                {
                    values[i] = null;
                }
            }

            return new ObjectArrayValueReader(values, _offset);
        }
    }
}
