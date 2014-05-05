// Copyright (c) Microsoft Open Technologies, Inc.
// All Rights Reserved
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR
// CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING
// WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR CONDITIONS OF
// TITLE, FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR
// NON-INFRINGEMENT.
// See the Apache 2 License for the specific language governing
// permissions and limitations under the License.

using System.Linq;

namespace Microsoft.Data.Entity.ChangeTracking
{
    public abstract partial class StateEntry
    {
        internal struct StateData
        {
            private const int BitsPerInt = 32;
            private const int BitsForEntityState = 3;
            private const int BitsForFlags = 1;
            private const int BitsForAdditionalState = BitsForEntityState + BitsForFlags;
            private const int EntityStateMask = 0x07;
            private const int TransparentSidecarMask = 0x08;
            private const int AdditionalStateMask = EntityStateMask | TransparentSidecarMask;

            private readonly int[] _bits;

            public StateData(int propertyCount)
            {
                _bits = new int[(propertyCount + BitsForAdditionalState - 1) / BitsPerInt + 1];
            }

            public void SetAllPropertiesModified(int propertyCount)
            {
                for (var i = 0; i < _bits.Length; i++)
                {
                    _bits[i] |= CreateMaskForWrite(i, propertyCount);
                }
            }

            public EntityState EntityState
            {
                get { return (EntityState)(_bits[0] & EntityStateMask); }
                set { _bits[0] = (_bits[0] & ~EntityStateMask) | (int)value; }
            }

            public bool TransparentSidecarInUse
            {
                get { return (_bits[0] & TransparentSidecarMask) != 0; }
                set { _bits[0] = (_bits[0] & ~TransparentSidecarMask) | (value ? TransparentSidecarMask : 0); }
            }

            public bool IsPropertyModified(int propertyIndex)
            {
                propertyIndex += BitsForAdditionalState;

                return (_bits[propertyIndex / BitsPerInt] & (1 << propertyIndex % BitsPerInt)) != 0;
            }

            public void SetPropertyModified(int propertyIndex, bool isModified)
            {
                propertyIndex += BitsForAdditionalState;

                if (isModified)
                {
                    _bits[propertyIndex / BitsPerInt] |= 1 << propertyIndex % BitsPerInt;
                }
                else
                {
                    _bits[propertyIndex / BitsPerInt] &= ~(1 << propertyIndex % BitsPerInt);
                }
            }

            public bool AnyPropertiesModified()
            {
                return _bits.Where((t, i) => (t & CreateMaskForRead(i)) != 0).Any();
            }

            private static int CreateMaskForRead(int i)
            {
                var mask = unchecked(((int)0xFFFFFFFF));
                if (i == 0)
                {
                    mask &= ~AdditionalStateMask;
                }

                // TODO: Remove keys/readonly indexes from the mask to avoid setting them to modified
                return mask;
            }

            private int CreateMaskForWrite(int i, int propertyCount)
            {
                var mask = CreateMaskForRead(i);

                if (i == _bits.Length - 1)
                {
                    var overlay = unchecked(((int)0xFFFFFFFF));
                    var shift = (propertyCount + BitsForAdditionalState) % BitsPerInt;
                    overlay = shift != 0 ? overlay << shift : 0;
                    mask &= ~overlay;
                }

                // TODO: Remove keys/readonly indexes from the mask to avoid setting them to modified
                return mask;
            }
        }
    }
}
