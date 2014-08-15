﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata
{
    public class ModelBuilderFactory : IModelBuilderFactory
    {
        public virtual ConventionModelBuilder CreateConventionBuilder([NotNull] Model model)
        {
            Check.NotNull(model, "model");

            return new ConventionModelBuilder(model);
        }
    }
}
