//
//  SPDX-FileName: IStateMachine.cs
//  SPDX-FileCopyrightText: Copyright (c) Jarl Gullberg
//  SPDX-License-Identifier: LGPL-3.0-or-later
//

using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Remora.Results;

namespace Remora.StateMachine;

/// <summary>
/// Represents a state machine that handles transitions between states.
/// </summary>
[PublicAPI]
public interface IStateMachine
{
    /// <summary>
    /// Runs the state machine until completion or cancellation, whichever comes first.
    /// </summary>
    /// <remarks>
    /// The state machine may terminate early if any of its states fail to transition. If so, the causal error will be
    /// returned.
    /// </remarks>
    /// <typeparam name="TState">The type of the initial state.</typeparam>
    /// <param name="initialState">
    /// The initial state of the machine. Will be entered as part of the machine's startup.
    /// </param>
    /// <param name="ct">The cancellation token for the state machine.</param>
    /// <returns>The result of the machine's execution.</returns>
    ValueTask<Result> RunAsync<TState>(TState initialState, CancellationToken ct = default)
        where TState : IInitiatingState;
}
