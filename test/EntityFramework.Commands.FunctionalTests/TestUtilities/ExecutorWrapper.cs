﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#if NET451

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Microsoft.Data.Entity.Commands.TestUtilities
{
    // NOTE: If you break this file, you probably broke the PowerShell module too.
    public class ExecutorWrapper : IDisposable
    {
        private const string AssemblyName = "EntityFramework.Commands";
        private const string TypeName = "Microsoft.Data.Entity.Commands.Executor";

        private readonly string _targetDir;
        private readonly AppDomain _domain;
        private readonly object _executor;

        public ExecutorWrapper(string targetDir, string targetFileName, string projectDir, string rootNamespace)
        {
            _targetDir = targetDir;
            _domain = AppDomain.CreateDomain(
                "ExecutorWrapper",
                null,
                new AppDomainSetup { ApplicationBase = targetDir, ShadowCopyFiles = "true" });
            _executor = _domain.CreateInstanceAndUnwrap(
                AssemblyName,
                TypeName,
                false,
                0,
                null,
                new[]
                {
                    new Hashtable
                    {
                        { "targetDir", targetDir },
                        { "targetFileName", targetFileName },
                        { "projectDir", projectDir },
                        { "rootNamespace", rootNamespace }
                    }
                },
                null,
                null);
        }

        public string GetContextType(string name)
        {
            return InvokeOperation<string>("GetContextType", new Hashtable { { "name", name } });
        }

        public IEnumerable<string> AddMigration(string migrationName, string contextTypeName)
        {
            return InvokeOperation<IEnumerable<string>>(
                "AddMigration",
                new Hashtable { { "migrationName", migrationName }, { "contextTypeName", contextTypeName } });
        }

        public void ApplyMigration(string migrationName, string contextTypeName)
        {
            InvokeOperation(
                "ApplyMigration",
                new Hashtable { { "migrationName", migrationName }, { "contextTypeName", contextTypeName } });
        }

        public string ScriptMigration(
            string fromMigrationName,
            string toMigrationName,
            bool idempotent,
            string contextTypeName)
        {
            return InvokeOperation<string>(
                "ScriptMigration",
                new Hashtable
                {
                    { "fromMigrationName", fromMigrationName },
                    { "toMigrationName", toMigrationName },
                    { "idempotent", idempotent },
                    { "contextTypeName", contextTypeName }
                });
        }

        public IEnumerable<IDictionary> GetContextTypes()
        {
            return InvokeOperation<IEnumerable<IDictionary>>("GetContextTypes");
        }

        public IEnumerable<IDictionary> GetMigrations(string contextTypeName)
        {
            return InvokeOperation<IEnumerable<IDictionary>>(
                "GetMigrations",
                new Hashtable { { "contextTypeName", contextTypeName } });
        }

        public void Dispose()
        {
            AppDomain.Unload(_domain);
        }

        private void InvokeOperation(string operation)
        {
            InvokeOperation(operation, new Hashtable());
        }

        private TResult InvokeOperation<TResult>(string operation)
        {
            return InvokeOperation<TResult>(operation, new Hashtable());
        }

        private TResult InvokeOperation<TResult>(string operation, Hashtable arguments)
        {
            return (TResult)InvokeOperationImpl(operation, arguments);
        }

        private void InvokeOperation(string operation, Hashtable arguments)
        {
            InvokeOperationImpl(operation, arguments, isVoid: true);
        }

        private object InvokeOperationImpl(string operation, Hashtable arguments, bool isVoid = false)
        {
            var handler = new Handler();

            // TODO: Set current directory
            _domain.CreateInstance(
                AssemblyName,
                TypeName + "+" + operation,
                false,
                0,
                null,
                new[] { _executor, handler, arguments },
                null,
                null);

            if (handler.ErrorType != null)
            {
                throw new CommandException(handler.ErrorMessage, handler.ErrorStackTrace, handler.ErrorType);
            }
            if (!isVoid && !handler.HasResult)
            {
                throw new InvalidOperationException(
                    string.Format("A value was not returned for operation '{0}'.", operation));
            }

            return handler.Result;
        }
    }
}

#endif
