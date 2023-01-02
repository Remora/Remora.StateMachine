//
//  SPDX-FileName: DisposableForkingNonTerminating.cs
//  SPDX-FileCopyrightText: Copyright (c) Jarl Gullberg
//  SPDX-License-Identifier: LGPL-3.0-or-later
//

using System;
using System.Threading;
using System.Threading.Tasks;
using Remora.Results;
using Remora.StateMachine.Tests.Extensions;
using Remora.StateMachine.Tests.States;

#pragma warning disable CS1591

namespace Remora.StateMachine.Tests.Graphs.Disposable;

/// <summary>
/// Contains the definitions for a forking (that is, the graph contains one or more states that return from EnterAsync
/// before a transition is requested), non-terminating (that is, a graph that must be cancelled to end) state graph.
/// </summary>
public static class DisposableForkingNonTerminating
{
    public record A(IStateVerificationAdapter VerificationAdapter, CancellationToken TerminationToken) :
        TestableState<A>(VerificationAdapter),
        IInitiatingState,
        ITransition<B>,
        IDisposable
    {
        private Task? _transitTask;

        /// <inheritdoc />
        public override ValueTask<Result> EnterAsync(CancellationToken ct = default)
        {
            async Task TransitTask()
            {
                await Task.Yield();
                await Task.Delay(TimeSpan.FromSeconds(1), ct);

                this.Controller.RequestTransit<B>();
            }

            _transitTask = TransitTask();
            return base.EnterAsync(ct);
        }

        /// <inheritdoc />
        public override async ValueTask<Result> ExitAsync(CancellationToken ct = default)
        {
            await _transitTask!;
            return await base.ExitAsync(ct);
        }

        /// <inheritdoc/>
        ValueTask<Result> ITransition<B>.TransitAsync(CancellationToken ct)
        {
            this.VerificationAdapter.TransitAsync<A, B>();
            return new(Result.FromSuccess());
        }

        /// <inheritdoc/>
        public ValueTask<Result<B>> CreateAsync()
        {
            var next = new B(this.VerificationAdapter, this.TerminationToken) { Controller = this.Controller };

            this.VerificationAdapter.CreateAsync<A, B>(next);
            return new(next);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            this.VerificationAdapter.Dispose<A>();
        }
    }

    public record B(IStateVerificationAdapter VerificationAdapter, CancellationToken TerminationToken) :
        TestableState<B>(VerificationAdapter),
        ITerminatingState,
        IDisposable
    {
        private Task? _exitTask;

        /// <inheritdoc/>
        public override ValueTask<Result> EnterAsync(CancellationToken ct = default)
        {
            async Task ExitTask()
            {
                await Task.Yield();
                await this.TerminationToken;

                this.Controller.RequestExit();
            }

            _exitTask = ExitTask();
            return base.EnterAsync(ct);
        }

        /// <inheritdoc/>
        public override async ValueTask<Result> ExitAsync(CancellationToken ct = default)
        {
            await _exitTask!;
            return await base.ExitAsync(ct);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            this.VerificationAdapter.Dispose<B>();
        }
    }
}
