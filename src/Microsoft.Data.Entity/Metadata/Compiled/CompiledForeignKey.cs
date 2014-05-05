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

using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Data.Entity.Metadata.Compiled
{
    public abstract class CompiledForeignKey : NoAnnotations
    {
        private readonly IModel _model;

        protected CompiledForeignKey(IModel model)
        {
            _model = model;
        }

        protected abstract ForeignKeyDefinition Definition { get; }

        public IReadOnlyList<IProperty> Properties
        {
            get { return Definition.DependentPropertyIndexes.Select(i => EntityType.Properties[i]).ToArray(); }
        }

        public IReadOnlyList<IProperty> ReferencedProperties
        {
            get { return Definition.PrincipalPropertyIndexes.Select(i => ReferencedEntityType.Properties[i]).ToArray(); }
        }

        public IEntityType ReferencedEntityType
        {
            get { return _model.EntityTypes[Definition.PrincipalIndex]; }
        }

        public IEntityType EntityType
        {
            get { return _model.EntityTypes[Definition.DependentIndex]; }
        }

        public bool IsRequired
        {
            get { return Properties.Any(p => !p.IsNullable); }
        }

        public bool IsUnique
        {
            get { return false; }
        }

        protected struct ForeignKeyDefinition
        {
            public short DependentIndex;
            public short PrincipalIndex;
            public short[] DependentPropertyIndexes;
            public short[] PrincipalPropertyIndexes;

            public ForeignKeyDefinition(
                short dependentIndex, short[] dependentPropertyIndexes, short principalIndex, short[] principalPropertyIndexes)
            {
                DependentIndex = dependentIndex;
                DependentPropertyIndexes = dependentPropertyIndexes;
                PrincipalIndex = principalIndex;
                PrincipalPropertyIndexes = principalPropertyIndexes;
            }
        }
    }
}
