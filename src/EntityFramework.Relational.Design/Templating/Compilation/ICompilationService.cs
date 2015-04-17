// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;

namespace Microsoft.Data.Entity.Relational.Design.Templating.Compilation
{
    public interface ICompilationService
    {
        CompilationResult Compile([NotNull]string content, [NotNull]List<MetadataReference> references);
    }
}
