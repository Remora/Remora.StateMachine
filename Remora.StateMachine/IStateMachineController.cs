//
//  SPDX-FileName: IStateMachineController.cs
//  SPDX-FileCopyrightText: Copyright (c) Jarl Gullberg
//  SPDX-License-Identifier: LGPL-3.0-or-later
//

using JetBrains.Annotations;

namespace Remora.StateMachine;

/// <summary>
/// Represents control mechanisms for a state machine.
/// </summary>
[PublicAPI]
public interface IStateMachineController
{
    /// <summary>
    /// Requests that the state machine transitions from the current state to the target state. The current state must
    /// implement <see cref="ITransition{TState}"/> for the requested state; if it does not, an exception must be thrown
    /// and the machine must be aborted.
    ///
    /// The state transition happens independently of the request to transit - the task does not represent the
    /// transition itself, merely the request to do so for the controller.
    /// </summary>
    /// <remarks>
    /// This method is thread-safe.
    /// </remarks>
    /// <typeparam name="TState">The type of the state to transition to.</typeparam>
    void RequestTransit<TState>() where TState : IState;

    /// <summary>
    /// Requests that the state machine exits as its next state instead of transitioning to another state. The current
    /// state must implement <see cref="ITerminatingState"/>; if it does not, an exception must be thrown and the
    /// machine must be aborted.
    /// </summary>
    /// <remarks>
    /// This method is thread-safe.
    /// </remarks>
    void RequestExit();
}
