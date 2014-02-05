﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Threading;


namespace Microsoft.Data.Entity.Utilities
{
    [DebuggerStepThrough]
    internal sealed class LazyRef<T>
        where T : class
    {
        private Func<T> _initializer;
        private object _syncLock;

        private T _value;

        internal LazyRef()
        {
        }

        public LazyRef(Func<T> initializer)
        {
            Check.NotNull(initializer, "initializer");

            _initializer = initializer;
        }

        public T Value
        {
            get
            {
                if (_value == null)
                {
                    var syncLock = new object();

                    syncLock
                        = Interlocked.CompareExchange(ref _syncLock, syncLock, null)
                          ?? syncLock;

                    lock (syncLock)
                    {
                        if (_value == null)
                        {
                            _value = _initializer();

                            _syncLock = null;
                            _initializer = null;
                        }
                    }
                }

                return _value;
            }
        }

        public void ExchangeValue(Func<T, T> newValueCreator)
        {
            Check.NotNull(newValueCreator, "newValueCreator");

            T originalValue, newValue;

            do
            {
                originalValue = Value;
                newValue = newValueCreator(originalValue);

                if (ReferenceEquals(newValue, originalValue))
                {
                    return;
                }
            }
            while (Interlocked.CompareExchange(ref _value, newValue, originalValue) != originalValue);
        }

        public bool HasValue
        {
            get { return _value != null; }
        }
    }
}
