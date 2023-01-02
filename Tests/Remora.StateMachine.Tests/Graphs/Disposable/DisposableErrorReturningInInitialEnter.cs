//
//  SPDX-FileName: DisposableErrorReturningInInitialEnter.cs
//  SPDX-FileCopyrightText: Copyright (c) Jarl Gullberg
//  SPDX-License-Identifier: LGPL-3.0-or-later
//

using System;
using System.Threading;
using System.Threading.Tasks;
using Remora.Results;
using Remora.StateMachine.Tests.States;

#pragma warning disable CS1591

namespace Remora.StateMachine.Tests.Graphs.Disposable;

/// <summary>
/// Contains the definitions of a graph that returns an error during a call to <see cref="IState.EnterAsync"/> of the
/// initial state.
/// </summary>
public static class DisposableErrorReturningInInitialEnter
{
    public record A(IStateVerificationAdapter VerificationAdapter) :
        TestableState<A>(VerificationAdapter),
        IInitiatingState,
        ITransition<B>,
        IDisposable
    {
        /// <inheritdoc />
        public override async ValueTask<Result> EnterAsync(CancellationToken ct = default)
        {
            _ = await base.EnterAsync(ct);
            return new CoercedError();
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
    }

    public record B(IStateVerificationAdapter VerificationAdapter) :
        TestableState<B>(VerificationAdapter),
        ITerminatingState,
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
    }
}
