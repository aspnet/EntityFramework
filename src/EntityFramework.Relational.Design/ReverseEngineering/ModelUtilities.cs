﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.Relational.Design.ReverseEngineering
{
    public class ModelUtilities
    {
        private static readonly ModelUtilities _instance = new ModelUtilities();

        public static ModelUtilities Instance
        {
            get
            {
                return _instance;
            }
        }

        public virtual string GenerateLambdaToKey(
            [NotNull] IEnumerable<IProperty> properties,
            [NotNull] string lambdaIdentifier)
        {
            var sb = new StringBuilder();

            if (properties.Count() > 1)
            {
                sb.Append("new { ");
                sb.Append(string.Join(", ", properties.Select(p => lambdaIdentifier + "." + p.Name)));
                sb.Append(" }");
            }
            else
            {
                sb.Append(lambdaIdentifier + "." + properties.ElementAt(0).Name);
            }

            return sb.ToString();
        }

        public virtual IEnumerable<IProperty> OrderedProperties([NotNull] IEntityType entityType)
        {
            var primaryKeyProperties = entityType.GetPrimaryKey().Properties.ToList();
            foreach (var property in primaryKeyProperties)
            {
                yield return property;
            }

            var foreignKeyProperties = entityType.ForeignKeys.SelectMany(fk => fk.Properties).Distinct().ToList();
            foreach (var property in
                entityType
                .Properties
                .Except(primaryKeyProperties)
                .Except(foreignKeyProperties)
                .OrderBy(p => p.Name))
            {
                yield return property;
            }
        }
    }
}