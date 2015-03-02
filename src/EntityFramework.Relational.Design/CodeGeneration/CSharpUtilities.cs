﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Relational.Design.CodeGeneration
{
    public class CSharpUtilities
    {
        private static readonly HashSet<string> _cSharpKeywords = new HashSet<string>()
        {
            "abstract",
            "as",
            "base",
            "bool",
            "break",
            "byte",
            "case",
            "catch",
            "char",
            "checked",
            "class",
            "const",
            "continue",
            "decimal",
            "default",
            "delegate",
            "do",
            "double",
            "else",
            "enum",
            "event",
            "explicit",
            "extern",
            "false",
            "finally",
            "fixed",
            "float",
            "for",
            "foreach",
            "goto",
            "if",
            "implicit",
            "in",
            "int",
            "interface",
            "internal",
            "is",
            "lock",
            "long",
            "namespace",
            "new",
            "null",
            "object",
            "operator",
            "out",
            "override",
            "params",
            "private",
            "protected",
            "public",
            "readonly",
            "ref",
            "return",
            "sbyte",
            "sealed",
            "short",
            "sizeof",
            "stackalloc",
            "static",
            "string",
            "struct",
            "switch",
            "this",
            "throw",
            "true",
            "try",
            "typeof",
            "uint",
            "ulong",
            "unchecked",
            "unsafe",
            "ushort",
            "using",
            "virtual",
            "void",
            "volatile",
            "while",
        };

        private static readonly CSharpUtilities _instance = new CSharpUtilities();

        public static CSharpUtilities Instance
        {
            get
            {
                return _instance;
            }
        }

        public virtual string DelimitString([NotNull] string value)
        {
            Check.NotNull(value, nameof(value));

            return "\"" + EscapeString(value) + "\"";
        }

        public virtual string EscapeString([NotNull] string str)
        {
            Check.NotEmpty(str, nameof(str));

            return str.Replace("\"", "\\\"");
        }

        public virtual string EscapeVerbatimString([NotNull] string str)
        {
            Check.NotEmpty(str, nameof(str));

            return str.Replace("\"", "\"\"");
        }

        public virtual string GenerateLiteral([NotNull] byte[] value)
        {
            Check.NotNull(value, nameof(value));

            return "new byte[] {" + string.Join(", ", value) + "}";
        }

        public virtual string GenerateLiteral(bool value)
        {
            return value ? "true" : "false";
        }

        public virtual string GenerateLiteral(int value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }

        public virtual string GenerateLiteral(long value)
        {
            return value.ToString(CultureInfo.InvariantCulture + "L");
        }

        public virtual string GenerateLiteral(decimal value)
        {
            return value.ToString(CultureInfo.InvariantCulture) + "m";
        }

        public virtual string GenerateLiteral(float value)
        {
            return value.ToString(CultureInfo.InvariantCulture) + "f";
        }

        public virtual string GenerateLiteral(double value)
        {
            return value.ToString(CultureInfo.InvariantCulture) + "D";
        }

        public virtual string GenerateLiteral(TimeSpan value)
        {
            return "new TimeSpan(" + value.Ticks + ")";
        }

        public virtual string GenerateLiteral(DateTime value)
        {
            return "new DateTime(" + value.Ticks + ", DateTimeKind."
                   + Enum.GetName(typeof(DateTimeKind), value.Kind) + ")";
        }

        public virtual string GenerateLiteral(DateTimeOffset value)
        {
            return "new DateTimeOffset(" + value.Ticks + ", "
                   + GenerateLiteral(value.Offset) + ")";
        }

        public virtual string GenerateLiteral(Guid value)
        {
            return "new Guid(" + GenerateLiteral(value.ToString()) + ")";
        }

        public virtual string GenerateLiteral([NotNull] string value)
        {
            Check.NotNull(value, nameof(value));

            return "\"" + EscapeString(value) + "\"";
        }

        public virtual string GenerateVerbatimStringLiteral([NotNull] string value)
        {
            Check.NotNull(value, nameof(value));

            return "@\"" + EscapeVerbatimString(value) + "\"";
        }

        public virtual string GenerateLiteralForUnknownType([NotNull] object value)
        {
            Check.NotNull(value, nameof(value));

            if (value.GetType().GetTypeInfo().IsEnum)
            {
                return Enum.Format(value.GetType(), value, "D");
            }

            var generateLiteralMethodInfo =
                typeof(CSharpUtilities)
                    .GetRuntimeMethod("GenerateLiteral", new Type[] { value.GetType() });
            if (generateLiteralMethodInfo != null)
            {
                return (string)generateLiteralMethodInfo.Invoke(this, new object[] { value });
            }

            return string.Format(CultureInfo.InvariantCulture, "{0}", value);
        }

        public virtual string GenerateCSharpIdentifier(
            [NotNull] string identifier, [CanBeNull]ICollection<string> existingIdentifiers)
        {
            Check.NotEmpty(identifier, nameof(identifier));

            var invalidCharsRegex
                = new Regex(@"[^\p{Ll}\p{Lu}\p{Lt}\p{Lo}\p{Nd}\p{Nl}\p{Mn}\p{Mc}\p{Cf}\p{Pc}\p{Lm}]");

            var proposedIdentifier = invalidCharsRegex.Replace(identifier, "_");
            if (string.IsNullOrEmpty(proposedIdentifier))
            {
                proposedIdentifier = "_";
            }

            var firstChar = proposedIdentifier[0];
            if ((!char.IsLetter(firstChar) && firstChar != '_'))
            {
                proposedIdentifier = "_" + proposedIdentifier;
            }
            else if (_cSharpKeywords.Contains(proposedIdentifier))
            {
                proposedIdentifier = "_" + proposedIdentifier;
            }

            string finalIdentifier = proposedIdentifier;
            if (existingIdentifiers != null)
            {
                var suffix = 1;
                while (existingIdentifiers.Contains(finalIdentifier))
                {
                    finalIdentifier = proposedIdentifier + suffix.ToString();
                    suffix++;
                }
            }

            return finalIdentifier;
        }
    }
}