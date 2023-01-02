//
//  SPDX-FileName: ITransition.cs
//  SPDX-FileCopyrightText: Copyright (c) Jarl Gullberg
//  SPDX-License-Identifier: LGPL-3.0-or-later
//

using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Remora.Results;

namespace Remora.StateMachine;

/// <summary>
/// Represents a transition from one state to another. A state that allows transit from itself to another state must
/// know how to construct that state's initial state from its own.
/// </summary>
/// <typeparam name="TState">The state that can be transited to.</typeparam>
[PublicAPI]
public interface ITransition<TState> where TState : IState
{
    /// <summary>
    /// Creates the initial state for the target state.
    /// </summary>
    /// <returns>The target state.</returns>
    ValueTask<Result<TState>> CreateAsync();

    /// <summary>
    /// Called when transitioning from this state to the target state, but before this state is exited.
    /// </summary>
    /// <remarks>By default, this method does nothing.</remarks>
    /// <param name="ct">The cancellation token for the state machine.</param>
    /// <returns>The result of the transition.</returns>
    ValueTask<Result> TransitAsync(CancellationToken ct = default) => new(Result.FromSuccess());
}
