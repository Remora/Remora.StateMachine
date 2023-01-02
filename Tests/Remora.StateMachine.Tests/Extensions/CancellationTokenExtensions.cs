//
//  SPDX-FileName: CancellationTokenExtensions.cs
//  SPDX-FileCopyrightText: Copyright (c) Jarl Gullberg
//  SPDX-License-Identifier: LGPL-3.0-or-later
//

using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Remora.StateMachine.Tests.Extensions;

/// <summary>
/// Defines extensions to the <see cref="CancellationToken"/> struct.
/// </summary>
public static class CancellationTokenExtensions
{
    /// <summary>
    /// Gets an awaiter for the given cancellation token.
    /// </summary>
    /// <param name="cancellationToken">The token.</param>
    /// <returns>The awaiter.</returns>
    public static CancellationTokenAwaiter GetAwaiter(this CancellationToken cancellationToken)
    {
        return new CancellationTokenAwaiter(cancellationToken);
    }

    /// <summary>
    /// Represents an awaiter for a cancellation token.
    /// </summary>
    public readonly struct CancellationTokenAwaiter : INotifyCompletion
    {
        private readonly CancellationToken _cancellationToken;

        /// <summary>
        /// Initializes a new instance of the <see cref="CancellationTokenAwaiter"/> struct.
        /// </summary>
        /// <param name="cancellationToken">The token.</param>
        public CancellationTokenAwaiter(CancellationToken cancellationToken)
        {
            _cancellationToken = cancellationToken;
        }

        /// <summary>
        /// Gets a value indicating whether the awaited task is complete.
        /// </summary>
        public bool IsCompleted => _cancellationToken.IsCancellationRequested;

        /// <summary>
        /// Sets the continuation that runs upon task completion.
        /// </summary>
        /// <param name="continuation">The continuation.</param>
        public void OnCompleted(Action continuation) => _cancellationToken.Register(continuation);

        /// <summary>
        /// Gets the result of the task.
        /// </summary>
        public void GetResult() => _cancellationToken.WaitHandle.WaitOne();
    }
}
