﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using System.Reflection;
using Microsoft.Data.Entity.Metadata;
using Xunit;

namespace Microsoft.Data.Entity
{
    public class ApiConsistencyTest : ApiConsistencyTestBase
    {
        [Fact]
        public void FluentApiMethodsShouldNotReturnVoid()
        {
            var fluentApiTypes = new[] { typeof(ModelBuilder) };

            var voidMethods
                = from t in GetAllTypes(fluentApiTypes)
                  where t.IsVisible
                  from m in t.GetMethods(PublicInstance)
                  where m.DeclaringType != null
                        && m.DeclaringType.Assembly == TargetAssembly
                        && m.ReturnType == typeof(void)
                  select t.Name + "." + m.Name;

            Assert.Equal("", string.Join("\r\n", voidMethods));
        }

        protected override Assembly TargetAssembly
        {
            get { return typeof(Metadata.Entity).Assembly; }
        }
    }
}
