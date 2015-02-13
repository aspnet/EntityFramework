﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Relational.Design.CodeGeneration;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Relational.Design.ReverseEngineering
{
    public abstract class EntityTypeCodeGenerator
    {
        protected readonly ReverseEngineeringGenerator _generator;
        private readonly IEntityType _entityType;
        private readonly string _namespaceName;

        private List<string> _usedNamespaces = new List<string>()
                {
                    "System",
                    "System.Collections.Generic",
                    "Microsoft.Data.Entity",
                    "Microsoft.Data.Entity.Metadata"
                };

        public EntityTypeCodeGenerator(
            [NotNull]ReverseEngineeringGenerator generator,
            [NotNull]IEntityType entityType,
            [CanBeNull]string namespaceName)
        {
            _generator = generator;
            _entityType = entityType;
            _namespaceName = namespaceName;
        }

        public virtual ReverseEngineeringGenerator Generator
        {
            get
            {
                return _generator;
            }
        }

        public virtual IEntityType EntityType
        {
            get
            {
                return _entityType;
            }
        }

        public virtual string ClassName
        {
            get
            {
                return _entityType.Name;
            }
        }

        public virtual string ClassNamespace
        {
            get
            {
                return _namespaceName;
            }
        }

        public virtual void Generate(IndentedStringBuilder sb)
        {
            GenerateCommentHeader(sb);
            GenerateUsings(sb);
            CSharpCodeGeneratorHelper.Instance.BeginNamespace(sb, ClassNamespace);
            CSharpCodeGeneratorHelper.Instance.BeginClass(sb, AccessModifier.Public, ClassName, isPartial: true);
            GenerateProperties(sb);
            CSharpCodeGeneratorHelper.Instance.EndClass(sb);
            CSharpCodeGeneratorHelper.Instance.EndNamespace(sb);
        }

        public virtual void GenerateCommentHeader(IndentedStringBuilder sb)
        {
            CSharpCodeGeneratorHelper.Instance.SingleLineComment(sb, string.Empty);
            CSharpCodeGeneratorHelper.Instance.SingleLineComment(sb, "Generated code");
            CSharpCodeGeneratorHelper.Instance.SingleLineComment(sb, string.Empty);
            sb.AppendLine();
        }

        public virtual void GenerateUsings(IndentedStringBuilder sb)
        {
            var originalNamespaces = new List<string>(_usedNamespaces);
            foreach (var @namespace in _usedNamespaces.Concat(
                _entityType.Properties.Select(p => p.PropertyType.Namespace)
                    .Distinct().Where(ns => !originalNamespaces.Contains(ns)).OrderBy(ns => ns)))
            {
                CSharpCodeGeneratorHelper.Instance.AddUsingStatement(sb, @namespace);
            }

            if (_usedNamespaces.Any())
            {
                sb.AppendLine();
            }
        }

        public virtual void GenerateProperties(IndentedStringBuilder sb)
        {
            GenerateEntityProperties(sb);
            GenerateEntityNavigations(sb);
        }

        public virtual void GenerateEntityProperties(IndentedStringBuilder sb)
        {
            foreach(var property in OrderedEntityProperties())
            {
                GenerateEntityProperty(sb, property);
            }
        }

        public abstract void GenerateEntityProperty(IndentedStringBuilder sb, IProperty property);

        public abstract void GenerateEntityNavigations(IndentedStringBuilder sb);

        public virtual IEnumerable<IProperty> OrderedEntityProperties()
        {
            var primaryKeyProperties = _entityType.GetPrimaryKey().Properties.ToList();
            foreach (var property in primaryKeyProperties)
            {
                yield return property;
            }

            var foreignKeyProperties = _entityType.ForeignKeys.SelectMany(fk => fk.Properties).Distinct().ToList();
            foreach (var property in
                _entityType.Properties
                    .Where(p => !primaryKeyProperties.Contains(p) && !foreignKeyProperties.Contains(p))
                    .OrderBy(p => p.Name))
            {
                yield return property;
            }
        }
    }
}