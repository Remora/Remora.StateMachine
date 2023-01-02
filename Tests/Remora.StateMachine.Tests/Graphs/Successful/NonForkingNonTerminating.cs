//
//  SPDX-FileName: NonForkingNonTerminating.cs
//  SPDX-FileCopyrightText: Copyright (c) Jarl Gullberg
//  SPDX-License-Identifier: LGPL-3.0-or-later
//

using System.Threading;
using System.Threading.Tasks;
using Remora.Results;
using Remora.StateMachine.Tests.Extensions;
using Remora.StateMachine.Tests.States;

namespace Remora.StateMachine.Tests.Graphs.Successful;

/// <summary>
/// Contains the definitions for a non-forking (that is, the graph contains no states that return from EnterAsync before
/// a transition is requested), non-terminating (that is, a graph that must be cancelled to end) state graph.
/// </summary>
public class NonForkingNonTerminating
{
    public record A(IStateVerificationAdapter VerificationAdapter, CancellationToken TerminationToken) :
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
            var next = new B(this.VerificationAdapter, this.TerminationToken) { Controller = this.Controller };

            this.VerificationAdapter.CreateAsync<A, B>(next);
            return new(next);
        }
    }

    public record B(IStateVerificationAdapter VerificationAdapter, CancellationToken TerminationToken) :
        TestableState<B>(VerificationAdapter),
        ITerminatingState
    {
        /// <inheritdoc/>
        public override async ValueTask<Result> EnterAsync(CancellationToken ct = default)
        {
            await this.TerminationToken;

            this.Controller.RequestExit();
            return await base.EnterAsync(ct);
        }
    }
}
