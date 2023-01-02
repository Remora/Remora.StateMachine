//
//  SPDX-FileName: AsyncDisposableThrowingInFinalExit.cs
//  SPDX-FileCopyrightText: Copyright (c) Jarl Gullberg
//  SPDX-License-Identifier: LGPL-3.0-or-later
//

using System;
using System.Threading;
using System.Threading.Tasks;
using Remora.Results;
using Remora.StateMachine.Tests.States;

#pragma warning disable CS1591

namespace Remora.StateMachine.Tests.Graphs.AsyncDisposable;

/// <summary>
/// Contains the definitions of a graph that throws an exception during a call to <see cref="IState.ExitAsync"/> of the
/// final state.
/// </summary>
public static class AsyncDisposableThrowingInFinalExit
{
    public record A(IStateVerificationAdapter VerificationAdapter) :
        TestableState<A>(VerificationAdapter),
        IInitiatingState,
        ITransition<B>,
        IAsyncDisposable
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
        public ValueTask DisposeAsync()
        {
            this.VerificationAdapter.DisposeAsync<A>();
            return ValueTask.CompletedTask;
        }
    }

    public record B(IStateVerificationAdapter VerificationAdapter) :
        TestableState<B>(VerificationAdapter),
        ITerminatingState,
        IAsyncDisposable
    {
        /// <inheritdoc/>
        public override ValueTask<Result> EnterAsync(CancellationToken ct = default)
        {
            this.Controller.RequestExit();
            return base.EnterAsync(ct);
        }

        /// <inheritdoc />
        public override async ValueTask<Result> ExitAsync(CancellationToken ct = default)
        {
            _ = await base.ExitAsync(ct);
            throw new CoercedException();
        }

        /// <inheritdoc />
        public ValueTask DisposeAsync()
        {
            this.VerificationAdapter.DisposeAsync<B>();
            return ValueTask.CompletedTask;
        }
    }
}
