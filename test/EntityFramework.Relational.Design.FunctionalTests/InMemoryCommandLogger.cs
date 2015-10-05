// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.Data.Entity.Design.Internal;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Microsoft.Data.Entity.Scaffolding.FunctionalTests
{
    public class InMemoryCommandLogger : CommandLogger
    {
        public LoggerMessages Messages = new LoggerMessages();
        private readonly ITestOutputHelper _output;

        public InMemoryCommandLogger(string name, ITestOutputHelper output)
            : base(name)
        {
            // _output = output;
        }

        public override bool IsEnabled(LogLevel logLevel) => true;

        protected override void WriteError(string message)
        {
            _output?.WriteLine("[ERROR]: " + message);
            Messages.Error.Add(message);
        }

        protected override void WriteWarning(string message)
        {
            _output?.WriteLine("[WARN]: " + message);
            Messages.Warn.Add(message);
        }

        protected override void WriteInformation(string message)
        {
            _output?.WriteLine("[INFO]: " + message);
            Messages.Info.Add(message);
        }

        protected override void WriteVerbose(string message)
        {
            _output?.WriteLine("[VERBOSE]: " + message);
            Messages.Verbose.Add(message);
        }

        protected override void WriteDebug(string message)
        {
            _output?.WriteLine("[DEBUG]: " + message);
            Messages.Debug.Add(message);
        }
    }

    public class LoggerMessages
    {
        public List<string> Error = new List<string>();
        public List<string> Warn = new List<string>();
        public List<string> Info = new List<string>();
        public List<string> Verbose = new List<string>();
        public List<string> Debug = new List<string>();
    }
}
