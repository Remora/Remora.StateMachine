//
//  SPDX-FileName: DisposableAndAsyncDisposableNonForkingSelfTerminating.cs
//  SPDX-FileCopyrightText: Copyright (c) Jarl Gullberg
//  SPDX-License-Identifier: LGPL-3.0-or-later
//

using System;
using System.Threading;
using System.Threading.Tasks;
using Remora.Results;
using Remora.StateMachine.Tests.States;

namespace Remora.StateMachine.Tests.Graphs.DisposableAndAsyncDisposable;

/// <summary>
/// Contains the definitions for a non-forking (that is, the graph contains no states that return from EnterAsync before
/// a transition is requested), self-terminating (that is, a graph that runs to completion and exits on its own) state
/// graph.
/// </summary>
public static class DisposableAndAsyncDisposableNonForkingSelfTerminating
{
    public record A(IStateVerificationAdapter VerificationAdapter) :
        TestableState<A>(VerificationAdapter),
        IInitiatingState,
        ITransition<B>,
        IAsyncDisposable,
        IDisposable
    {
        /// <inheritdoc />
        public override ValueTask<Result> EnterAsync(CancellationToken ct = default)
        {
            this.Controller.RequestTransit<B>();
            return base.EnterAsync(ct);
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
            var next = new B(this.VerificationAdapter) { Controller = this.Controller };

            this.VerificationAdapter.CreateAsync<A, B>(next);
            return new(next);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            this.VerificationAdapter.Dispose<A>();
        }

        /// <inheritdoc />
        public ValueTask DisposeAsync()
        {
            this.VerificationAdapter.DisposeAsync<A>();
            return ValueTask.CompletedTask;
        }
    }

    public record B(IStateVerificationAdapter VerificationAdapter) :
        TestableState<B>(VerificationAdapter),
        ITerminatingState,
        IAsyncDisposable,
        IDisposable
    {
        /// <inheritdoc/>
        public override ValueTask<Result> EnterAsync(CancellationToken ct = default)
        {
            this.Controller.RequestExit();
            return base.EnterAsync(ct);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            this.VerificationAdapter.Dispose<B>();
        }

        /// <inheritdoc />
        public ValueTask DisposeAsync()
        {
            this.VerificationAdapter.DisposeAsync<B>();
            return ValueTask.CompletedTask;
        }
    }
}
