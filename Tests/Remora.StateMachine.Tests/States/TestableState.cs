//
//  SPDX-FileName: TestableState.cs
//  SPDX-FileCopyrightText: Copyright (c) Jarl Gullberg
//  SPDX-License-Identifier: LGPL-3.0-or-later
//

using System.Threading;
using System.Threading.Tasks;
using Remora.Results;

namespace Remora.StateMachine.Tests.States;

/// <summary>
/// Represents a testable state.
/// </summary>
/// <typeparam name="TActualState">The actual state type.</typeparam>
/// <param name="VerificationAdapter">The verification adapter.</param>
public abstract record TestableState<TActualState>(IStateVerificationAdapter VerificationAdapter) : State
    where TActualState : TestableState<TActualState>
{
    /// <inheritdoc/>
    public override ValueTask<Result> EnterAsync(CancellationToken ct = default)
    {
        this.VerificationAdapter.EnterAsync<TActualState>();
        return base.EnterAsync(ct);
    }

    /// <inheritdoc/>
    public override ValueTask<Result> ExitAsync(CancellationToken ct = default)
    {
        this.VerificationAdapter.ExitAsync<TActualState>();
        return base.EnterAsync(ct);
    }
}
