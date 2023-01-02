//
//  SPDX-FileName: IStateVerificationAdapter.cs
//  SPDX-FileCopyrightText: Copyright (c) Jarl Gullberg
//  SPDX-License-Identifier: LGPL-3.0-or-later
//

using Remora.Results;

namespace Remora.StateMachine.Tests.States;

/// <summary>
/// Represents a verification adapter for transition-related methods in states.
///
/// This adapter is not meant for implementation; rather, it is to be used with Moq and its verification proxies.
/// </summary>
public interface IStateVerificationAdapter
{
    /// <summary>
    /// Verifies that the given state has been entered.
    /// </summary>
    /// <typeparam name="TState">The state.</typeparam>
    void EnterAsync<TState>() where TState : State;

    /// <summary>
    /// Verifies that the given state has been exited.
    /// </summary>
    /// <typeparam name="TState">The state.</typeparam>
    void ExitAsync<TState>() where TState : State;

    /// <summary>
    /// Verifies that the given state has been created when transitioning.
    /// </summary>
    /// <typeparam name="TState">The state.</typeparam>
    /// <typeparam name="TCreatedState">The created state.</typeparam>
    /// <param name="state">The created state instance.</param>
    void CreateAsync<TState, TCreatedState>(Result<TCreatedState> state)
        where TState : State
        where TCreatedState : State;

    /// <summary>
    /// Verifies that the given state has been transitioned from.
    /// </summary>
    /// <typeparam name="TState">The state.</typeparam>
    /// <typeparam name="TNextState">The next state.</typeparam>
    void TransitAsync<TState, TNextState>()
        where TState : State
        where TNextState : State;

    /// <summary>
    /// Verifies that the give state was disposed.
    /// </summary>
    /// <typeparam name="TState">The state.</typeparam>
    void Dispose<TState>() where TState : State;

    /// <summary>
    /// Verifies that the give state was disposed.
    /// </summary>
    /// <typeparam name="TState">The state.</typeparam>
    void DisposeAsync<TState>() where TState : State;
}
