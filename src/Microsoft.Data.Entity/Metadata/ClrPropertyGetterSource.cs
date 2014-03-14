﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Reflection;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata
{
    public class ClrPropertyGetterSource : ClrAccessorSource<IClrPropertyGetter>
    {
        protected override IClrPropertyGetter CreateGeneric<TEntity, TValue>(PropertyInfo property)
        {
            Check.NotNull(property, "property");

            // TODO: Handle case where there is not setter or setter is private on a base type
            var getterDelegate = (Func<TEntity, TValue>)property.GetMethod.CreateDelegate(typeof(Func<TEntity, TValue>));

            return new ClrPropertyGetter<TEntity, TValue>(getterDelegate);
        }
    }
}
