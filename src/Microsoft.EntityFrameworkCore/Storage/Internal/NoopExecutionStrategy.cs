// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.EntityFrameworkCore.Storage.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class NoopExecutionStrategy : IExecutionStrategy
    {
        private NoopExecutionStrategy()
        {
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static NoopExecutionStrategy Instance = new NoopExecutionStrategy();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public bool RetriesOnFailure => false;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public void Execute<TState>(Action<TState> operation, TState state) => operation(state);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public TResult Execute<TState, TResult>(Func<TState, TResult> operation, TState state) => operation(state);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public Task ExecuteAsync<TState>(
            Func<TState, CancellationToken, Task> operation,
            TState state,
            CancellationToken cancellationToken = new CancellationToken())
            => operation(state, cancellationToken);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public Task<TResult> ExecuteAsync<TState, TResult>(
            Func<TState, CancellationToken, Task<TResult>> operation,
            TState state,
            CancellationToken cancellationToken = new CancellationToken())
            => operation(state, cancellationToken);
    }
}
