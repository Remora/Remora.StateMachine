//
//  SPDX-FileName: StateMachine.cs
//  SPDX-FileCopyrightText: Copyright (c) Jarl Gullberg
//  SPDX-License-Identifier: LGPL-3.0-or-later
//

using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Remora.Results;

namespace Remora.StateMachine;

/// <summary>
/// Implements a state machine.
/// </summary>
[PublicAPI]
public sealed class StateMachine : IStateMachine, IStateMachineController
{
    /// <summary>
    /// Holds a completion source for a state's request to exit the machine.
    /// </summary>
    private TaskCompletionSource<int> _exitRequest;

    /// <summary>
    /// Holds a completion source for a state's request to transition to another state.
    /// </summary>
    private TaskCompletionSource<Func<CancellationToken, ValueTask<Result<IState>>>> _transitionRequest;

    /// <summary>
    /// Holds the current state.
    /// </summary>
    private IState? _currentState;

    /// <summary>
    /// Holds a value indicating whether the machine is currently running.
    /// </summary>
    private bool _isRunning;

    /// <summary>
    /// Initializes a new instance of the <see cref="StateMachine"/> class.
    /// </summary>
    public StateMachine()
    {
        _transitionRequest = new();
        _exitRequest = new();
    }

    /// <inheritdoc />
    public async ValueTask<Result> RunAsync<TState>(TState initialState, CancellationToken ct = default)
        where TState : IInitiatingState
    {
        if (_isRunning)
        {
            throw new InvalidOperationException("The state machine is already running");
        }

        _isRunning = true;

        _transitionRequest = new();
        _exitRequest = new();

        _currentState = initialState;

        try
        {
            var enterInitial = await _currentState.EnterAsync(ct);
            if (!enterInitial.IsSuccess)
            {
                return enterInitial;
            }

            while (!ct.IsCancellationRequested)
            {
                var request = await Task.WhenAny(_transitionRequest.Task, _exitRequest.Task);
                if (request == _transitionRequest.Task)
                {
                    var transition = await _transitionRequest.Task;
                    _transitionRequest = new(); // allow new requests to happen during the transit itself

                    var transit = await transition(ct);
                    if (!transit.IsSuccess)
                    {
                        return (Result)transit;
                    }

                    var next = transit.Entity;

                    try
                    {
                        var exitCurrent = await _currentState.ExitAsync(ct);
                        if (!exitCurrent.IsSuccess)
                        {
                            await DisposeIfRequired(next);
                            return exitCurrent;
                        }

                        var enterNext = await next.EnterAsync(ct);
                        if (!enterNext.IsSuccess)
                        {
                            await DisposeIfRequired(next);
                            return enterNext;
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        // next may dangle unless disposed here should either of the above throw
                        // _currentState is taken care of in the outer code
                        await DisposeIfRequired(next);
                    }

                    await DisposeIfRequired(_currentState);
                    _currentState = next;
                }
                else
                {
                    await _exitRequest.Task;

                    var exitFinal = await _currentState.ExitAsync(ct);
                    if (!exitFinal.IsSuccess)
                    {
                        return exitFinal;
                    }

                    break;
                }
            }
        }
        catch (OperationCanceledException)
        {
            // cancellation is fine
        }
        finally
        {
            // Canceled or graceful exit
            await DisposeIfRequired(_currentState);

            _currentState = null;
            _isRunning = false;
        }

        return Result.FromSuccess();
    }

    /// <inheritdoc/>
    void IStateMachineController.RequestTransit<TState>()
    {
        static async ValueTask<Result<IState>> PerformTransition
        (
            ITransition<TState> transition,
            CancellationToken ct = default
        )
        {
            var transit = await transition.TransitAsync(ct);
            if (!transit.IsSuccess)
            {
                return Result<IState>.FromError(transit);
            }

            var createNext = await transition.CreateAsync();
            return createNext.IsSuccess
                ? createNext.Entity
                : Result<IState>.FromError(createNext);
        }

        if (_currentState is not ITransition<TState> transition)
        {
            throw new InvalidOperationException($"The current state cannot transition to {typeof(TState).Name}");
        }

        if (!_transitionRequest.TrySetResult(ct => PerformTransition(transition, ct)))
        {
            throw new InvalidOperationException("A state transition has already been requested");
        }
    }

    /// <inheritdoc/>
    void IStateMachineController.RequestExit()
    {
        if (!_exitRequest.TrySetResult(0))
        {
            throw new InvalidOperationException("Machine exit has already been requested");
        }
    }

    /// <summary>
    /// Disposes the state using <see cref="IAsyncDisposable"/> or <see cref="IDisposable"/> should the state
    /// implement either or both of the interfaces. Both methods will be called if the type implements both interfaces,
    /// but the async one will execute first.
    /// </summary>
    /// <param name="state">The state to dispose.</param>
    /// <typeparam name="TState">The type of the state.</typeparam>
    private static async ValueTask DisposeIfRequired<TState>(TState state) where TState : IState
    {
        if (state is IAsyncDisposable asyncDisposable)
        {
            await asyncDisposable.DisposeAsync();
        }

        if (state is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}
