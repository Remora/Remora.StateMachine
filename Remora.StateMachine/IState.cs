//
//  SPDX-FileName: IState.cs
//  SPDX-FileCopyrightText: Copyright (c) Jarl Gullberg
//  SPDX-License-Identifier: LGPL-3.0-or-later
//

using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Remora.Results;

#pragma warning disable SA1402

namespace Remora.StateMachine;

/// <summary>
/// Represents a state within a state machine that can be entered or exited. A state carries - as the name implies -
/// stateful information about its unit of work along with the business logic that operates on it.
/// </summary>
[PublicAPI]
public interface IState
{
    /// <summary>
    /// Gets the controller associated with the state's running state machine.
    /// </summary>
    IStateMachineController Controller { get; init; }

    /// <summary>
    /// Called whenever this state is entered. This operation should not block and must return before the state can be
    /// exited; compare it to a StartAsync method on a background service.
    ///
    /// The state may queue a transition to another state directly after this state is entered by using
    /// <see cref="IStateMachineController.RequestTransit{TState}"/>; this is mainly useful when a state performs some
    /// type of blocking computation or other non-forking logic in its enter method.
    /// </summary>
    /// <remarks>By default, this method does nothing.</remarks>
    /// <param name="ct">The cancellation token for the state machine.</param>
    /// <returns>The result of the state entrance.</returns>
    ValueTask<Result> EnterAsync(CancellationToken ct = default) => new(Result.FromSuccess());

    /// <summary>
    /// Called whenever this state is exited. This operation should not block and must return before another state can
    /// be entered; compare it to a StopAsync method on a background service.
    ///
    /// This method is not guaranteed to be called if the state machine is cancelled. If your state has some required
    /// cleanup it needs to perform on exit no matter what, consider implementing <see cref="IDisposable"/> or
    /// <see cref="IAsyncDisposable"/> instead of or complementary to this method.
    /// </summary>
    /// <remarks>By default, this method does nothing.</remarks>
    /// <param name="ct">The cancellation token for the state machine.</param>
    /// <returns>The result of the state exit.</returns>
    ValueTask<Result> ExitAsync(CancellationToken ct = default) => new(Result.FromSuccess());
}

/// <summary>
/// Represents a state that may enter the state machine.
/// </summary>
[PublicAPI]
public interface IInitiatingState : IState
{
}

/// <summary>
/// Represents a state that may exit the state machine.
/// </summary>
[PublicAPI]
public interface ITerminatingState : IState
{
}
