//
//  SPDX-FileName: ThrowingInFinalExit.cs
//  SPDX-FileCopyrightText: Copyright (c) Jarl Gullberg
//  SPDX-License-Identifier: LGPL-3.0-or-later
//

using System.Threading;
using System.Threading.Tasks;
using Remora.Results;
using Remora.StateMachine.Tests.States;

#pragma warning disable CS1591

namespace Remora.StateMachine.Tests.Graphs.Erroring;

/// <summary>
/// Contains the definitions of a graph that throws an exception during a call to <see cref="IState.ExitAsync"/> of the
/// final state.
/// </summary>
public static class ThrowingInFinalExit
{
    public record A(IStateVerificationAdapter VerificationAdapter) :
        TestableState<A>(VerificationAdapter),
        IInitiatingState,
        ITransition<B>
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
    }

    public record B(IStateVerificationAdapter VerificationAdapter) :
        TestableState<B>(VerificationAdapter),
        ITerminatingState
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
    }
}
