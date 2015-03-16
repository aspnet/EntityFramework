﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Commands.Utilities;
using Microsoft.Data.Entity.Relational.Design.CodeGeneration;
using Microsoft.Data.Entity.Relational.Design.ReverseEngineering;
using Microsoft.Data.Entity.Relational.Design.Utilities;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.Commands
{
    public class DatabaseTool
    {
        public static readonly string _defaultReverseEngineeringProviderAssembly = "EntityFramework.SqlServer.Design";

        private readonly ServiceProvider _serviceProvider;
        private readonly LazyRef<ILogger> _logger;

        public DatabaseTool(
            [CanBeNull] IServiceProvider serviceProvider,
            [NotNull] ILoggerProvider loggerProvider)
        {
            Check.NotNull(loggerProvider, nameof(loggerProvider));

            _serviceProvider = new ServiceProvider(serviceProvider);
            var loggerFactory = new LoggerFactory();
            loggerFactory.AddProvider(loggerProvider);
            _logger = new LazyRef<ILogger>(() => loggerFactory.CreateLogger<DatabaseTool>());
            _serviceProvider.AddService(typeof(ILogger), _logger.Value);
            _serviceProvider.AddService(typeof(CSharpCodeGeneratorHelper), new CSharpCodeGeneratorHelper());
            _serviceProvider.AddService(typeof(ModelUtilities), new ModelUtilities());
        }

        public virtual IEnumerable<string> ReverseEngineer(
            [NotNull] string providerAssemblyName,
            [NotNull] string connectionString,
            [NotNull] string rootNamespace,
            [NotNull] string projectDir)
        {
            Check.NotNull(providerAssemblyName, nameof(providerAssemblyName));
            Check.NotEmpty(connectionString, nameof(connectionString));
            Check.NotEmpty(rootNamespace, nameof(rootNamespace));
            Check.NotEmpty(projectDir, nameof(projectDir));

            var assembly = GetReverseEngineerProviderAssembly(providerAssemblyName);
            if (assembly == null)
            {
                _logger.Value.LogWarning(Strings.CannotFindAssembly(providerAssemblyName));
                return new List<string>();
            }

            var configuration = new ReverseEngineeringConfiguration()
            {
                ProviderAssembly = assembly,
                ConnectionString = connectionString,
                Namespace = rootNamespace,
                OutputPath = projectDir
            };

            var generator = new ReverseEngineeringGenerator(_serviceProvider);
            return generator.GenerateAsync(configuration).Result;
        }

        private Assembly GetReverseEngineerProviderAssembly(string providerAssemblyName)
        {
            var assemblyName = new AssemblyName(providerAssemblyName);
            return Assembly.Load(assemblyName);
        }
    }
}
