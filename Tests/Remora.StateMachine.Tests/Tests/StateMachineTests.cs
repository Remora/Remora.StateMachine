//
//  SPDX-FileName: StateMachineTests.cs
//  SPDX-FileCopyrightText: Copyright (c) Jarl Gullberg
//  SPDX-License-Identifier: LGPL-3.0-or-later
//

using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Remora.StateMachine.Tests.Graphs.Broken;
using Remora.StateMachine.Tests.Graphs.Disposable;
using Remora.StateMachine.Tests.Graphs.Erroring;
using Remora.StateMachine.Tests.Graphs.Successful;
using Remora.StateMachine.Tests.States;
using Xunit;

namespace Remora.StateMachine.Tests;

/// <summary>
/// Tests the <see cref="StateMachine"/> class.
/// </summary>
public static class StateMachineTests
{
    /// <summary>
    /// Tests ideal happy-path cases of the <see cref="StateMachine.RunAsync{TState}(TState,CancellationToken)"/>
    /// method.
    /// </summary>
    public static class Ideal
    {
        /// <summary>
        /// Tests whether the state machine can execute a non-forking, self-terminating graph.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public static async Task CanExecuteNonForkingSelfTerminatingGraph()
        {
            var machine = new StateMachine();

            var adapter = Mock.Of<IStateVerificationAdapter>();
            var initialState = new NonForkingSelfTerminating.A(adapter) { Controller = machine };

            var result = await machine.RunAsync(initialState);
            Assert.True(result.IsSuccess);
        }

        /// <summary>
        /// Tests whether the state machine has the correct execution flow for a non-forking, self-terminating graph.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public static async Task HasCorrectFlowForNonForkingSelfTerminatingGraph()
        {
            var machine = new StateMachine();

            var callSequence = new MockSequence();
            var adapterMock = new Mock<IStateVerificationAdapter>();
            adapterMock.InSequence(callSequence).Setup(m => m.EnterAsync<NonForkingSelfTerminating.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.TransitAsync<NonForkingSelfTerminating.A, NonForkingSelfTerminating.B>());
            adapterMock.InSequence(callSequence).Setup(m => m.ExitAsync<NonForkingSelfTerminating.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.EnterAsync<NonForkingSelfTerminating.B>());
            adapterMock.InSequence(callSequence).Setup(m => m.ExitAsync<NonForkingSelfTerminating.B>());

            var initialState = new NonForkingSelfTerminating.A(adapterMock.Object) { Controller = machine };

            var result = await machine.RunAsync(initialState);
            Assert.True(result.IsSuccess);

            adapterMock.Verify(m => m.EnterAsync<NonForkingSelfTerminating.A>(), Times.Once);
            adapterMock.Verify(m => m.TransitAsync<NonForkingSelfTerminating.A, NonForkingSelfTerminating.B>(), Times.Once);
            adapterMock.Verify(m => m.ExitAsync<NonForkingSelfTerminating.A>(), Times.Once);
            adapterMock.Verify(m => m.EnterAsync<NonForkingSelfTerminating.B>(), Times.Once);
            adapterMock.Verify(m => m.ExitAsync<NonForkingSelfTerminating.B>(), Times.Once);
        }

        /// <summary>
        /// Tests whether the state machine can execute a non-forking, non-terminating graph.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public static async Task CanExecuteNonForkingNonTerminatingGraph()
        {
            var machine = new StateMachine();

            var adapter = Mock.Of<IStateVerificationAdapter>();

            var tokenSource = new CancellationTokenSource();
            tokenSource.CancelAfter(TimeSpan.FromSeconds(1));

            var initialState = new NonForkingNonTerminating.A(adapter, tokenSource.Token) { Controller = machine };

            var result = await machine.RunAsync(initialState, CancellationToken.None);
            Assert.True(result.IsSuccess);
        }

        /// <summary>
        /// Tests whether the state machine has the correct execution flow for a non-forking, non-terminating graph.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public static async Task HasCorrectFlowForNonForkingNonTerminatingGraph()
        {
            var machine = new StateMachine();

            var callSequence = new MockSequence();
            var adapterMock = new Mock<IStateVerificationAdapter>();
            adapterMock.InSequence(callSequence).Setup(m => m.EnterAsync<NonForkingNonTerminating.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.TransitAsync<NonForkingNonTerminating.A, NonForkingNonTerminating.B>());
            adapterMock.InSequence(callSequence).Setup(m => m.ExitAsync<NonForkingNonTerminating.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.EnterAsync<NonForkingNonTerminating.B>());
            adapterMock.InSequence(callSequence).Setup(m => m.ExitAsync<NonForkingNonTerminating.B>());

            var tokenSource = new CancellationTokenSource();
            tokenSource.CancelAfter(TimeSpan.FromSeconds(1));

            var initialState = new NonForkingNonTerminating.A(adapterMock.Object, tokenSource.Token) { Controller = machine };

            var result = await machine.RunAsync(initialState, CancellationToken.None);
            Assert.True(result.IsSuccess);

            adapterMock.Verify(m => m.EnterAsync<NonForkingNonTerminating.A>(), Times.Once);
            adapterMock.Verify(m => m.TransitAsync<NonForkingNonTerminating.A, NonForkingNonTerminating.B>(), Times.Once);
            adapterMock.Verify(m => m.ExitAsync<NonForkingNonTerminating.A>(), Times.Once);
            adapterMock.Verify(m => m.EnterAsync<NonForkingNonTerminating.B>(), Times.Once);
            adapterMock.Verify(m => m.ExitAsync<NonForkingNonTerminating.B>(), Times.Once);
        }

        /// <summary>
        /// Tests whether the state machine can execute a forking, self-terminating graph.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public static async Task CanExecuteForkingSelfTerminatingGraph()
        {
            var machine = new StateMachine();

            var adapter = Mock.Of<IStateVerificationAdapter>();
            var initialState = new ForkingSelfTerminating.A(adapter) { Controller = machine };

            var result = await machine.RunAsync(initialState);
            Assert.True(result.IsSuccess);
        }

        /// <summary>
        /// Tests whether the state machine has the correct execution flow for a forking, self-terminating graph.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public static async Task HasCorrectFlowForForkingSelfTerminatingGraph()
        {
            var machine = new StateMachine();

            var callSequence = new MockSequence();
            var adapterMock = new Mock<IStateVerificationAdapter>();
            adapterMock.InSequence(callSequence).Setup(m => m.EnterAsync<ForkingSelfTerminating.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.TransitAsync<ForkingSelfTerminating.A, ForkingSelfTerminating.B>());
            adapterMock.InSequence(callSequence).Setup(m => m.ExitAsync<ForkingSelfTerminating.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.EnterAsync<ForkingSelfTerminating.B>());
            adapterMock.InSequence(callSequence).Setup(m => m.ExitAsync<ForkingSelfTerminating.B>());

            var initialState = new ForkingSelfTerminating.A(adapterMock.Object) { Controller = machine };

            var result = await machine.RunAsync(initialState);
            Assert.True(result.IsSuccess);

            adapterMock.Verify(m => m.EnterAsync<ForkingSelfTerminating.A>(), Times.Once);
            adapterMock.Verify(m => m.TransitAsync<ForkingSelfTerminating.A, ForkingSelfTerminating.B>(), Times.Once);
            adapterMock.Verify(m => m.ExitAsync<ForkingSelfTerminating.A>(), Times.Once);
            adapterMock.Verify(m => m.EnterAsync<ForkingSelfTerminating.B>(), Times.Once);
            adapterMock.Verify(m => m.ExitAsync<ForkingSelfTerminating.B>(), Times.Once);
        }

        /// <summary>
        /// Tests whether the state machine can execute a forking, non-terminating graph.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public static async Task CanExecuteForkingNonTerminatingGraph()
        {
            var machine = new StateMachine();

            var tokenSource = new CancellationTokenSource();
            tokenSource.CancelAfter(TimeSpan.FromSeconds(1));

            var adapter = Mock.Of<IStateVerificationAdapter>();
            var initialState = new ForkingNonTerminating.A(adapter, tokenSource.Token) { Controller = machine };

            var result = await machine.RunAsync(initialState, CancellationToken.None);
            Assert.True(result.IsSuccess);
        }

        /// <summary>
        /// Tests whether the state machine has the correct execution flow for a forking, non-terminating graph.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public static async Task HasCorrectFlowForkingNonTerminatingGraph()
        {
            var machine = new StateMachine();

            var callSequence = new MockSequence();
            var adapterMock = new Mock<IStateVerificationAdapter>();
            adapterMock.InSequence(callSequence).Setup(m => m.EnterAsync<ForkingNonTerminating.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.TransitAsync<ForkingNonTerminating.A, ForkingNonTerminating.B>());
            adapterMock.InSequence(callSequence).Setup(m => m.ExitAsync<ForkingNonTerminating.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.EnterAsync<ForkingNonTerminating.B>());
            adapterMock.InSequence(callSequence).Setup(m => m.ExitAsync<ForkingNonTerminating.B>());

            var tokenSource = new CancellationTokenSource();
            tokenSource.CancelAfter(TimeSpan.FromSeconds(1));

            var initialState = new ForkingNonTerminating.A(adapterMock.Object, tokenSource.Token) { Controller = machine };

            var result = await machine.RunAsync(initialState, CancellationToken.None);
            Assert.True(result.IsSuccess);

            adapterMock.Verify(m => m.EnterAsync<ForkingNonTerminating.A>(), Times.Once);
            adapterMock.Verify(m => m.TransitAsync<ForkingNonTerminating.A, ForkingNonTerminating.B>(), Times.Once);
            adapterMock.Verify(m => m.ExitAsync<ForkingNonTerminating.A>(), Times.Once);
            adapterMock.Verify(m => m.EnterAsync<ForkingNonTerminating.B>(), Times.Once);
            adapterMock.Verify(m => m.ExitAsync<ForkingNonTerminating.B>(), Times.Once);
        }
    }

    /// <summary>
    /// Tests the <see cref="StateMachine.RunAsync{TState}(TState,CancellationToken)"/> method's fault handling.
    /// </summary>
    public static class Faults
    {
        /// <summary>
        /// Tests whether the state machine propagates exceptions thrown by a call to <see cref="IState.EnterAsync"/> of
        /// the initial state.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public static async Task ThrowsIfInitialStateThrowsInEnter()
        {
            var machine = new StateMachine();

            var adapter = Mock.Of<IStateVerificationAdapter>();
            var initialState = new ThrowingInInitialEnter.A(adapter) { Controller = machine };

            await Assert.ThrowsAsync<CoercedException>(async () => await machine.RunAsync(initialState));
        }

        /// <summary>
        /// Tests whether the state machine has the correct execution flow for exceptions thrown by a call to
        /// <see cref="IState.EnterAsync"/> of the initial state.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public static async Task HasCorrectFlowIfInitialStateThrowsInEnter()
        {
            var machine = new StateMachine();

            var callSequence = new MockSequence();
            var adapterMock = new Mock<IStateVerificationAdapter>();
            adapterMock.InSequence(callSequence).Setup(m => m.EnterAsync<ThrowingInInitialEnter.A>());

            var initialState = new ThrowingInInitialEnter.A(adapterMock.Object) { Controller = machine };

            await Assert.ThrowsAsync<CoercedException>(async () => await machine.RunAsync(initialState));

            adapterMock.Verify(m => m.EnterAsync<ThrowingInInitialEnter.A>(), Times.Once);
            adapterMock.Verify(m => m.TransitAsync<ThrowingInInitialEnter.A, ThrowingInInitialEnter.B>(), Times.Never);
            adapterMock.Verify(m => m.ExitAsync<ThrowingInInitialEnter.A>(), Times.Never);
            adapterMock.Verify(m => m.EnterAsync<ThrowingInInitialEnter.B>(), Times.Never);
            adapterMock.Verify(m => m.ExitAsync<ThrowingInInitialEnter.B>(), Times.Never);
        }

        /// <summary>
        /// Tests whether the state machine propagates errors returned by a call to <see cref="IState.EnterAsync"/> of
        /// the initial state.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public static async Task ReturnsErrorIfInitialStateReturnsErrorFromEnter()
        {
            var machine = new StateMachine();

            var adapter = Mock.Of<IStateVerificationAdapter>();
            var initialState = new ErrorReturningInInitialEnter.A(adapter) { Controller = machine };

            var result = await machine.RunAsync(initialState);
            Assert.False(result.IsSuccess);
            Assert.IsType<CoercedError>(result.Error);
        }

        /// <summary>
        /// Tests whether the state machine has the correct execution flow for errors returned by a call to
        /// <see cref="IState.EnterAsync"/> of the initial state.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public static async Task HasCorrectFlowIfInitialStateReturnsErrorFromEnter()
        {
            var machine = new StateMachine();

            var callSequence = new MockSequence();
            var adapterMock = new Mock<IStateVerificationAdapter>();
            adapterMock.InSequence(callSequence).Setup(m => m.EnterAsync<ErrorReturningInInitialEnter.A>());

            var initialState = new ErrorReturningInInitialEnter.A(adapterMock.Object) { Controller = machine };

            var result = await machine.RunAsync(initialState);
            Assert.False(result.IsSuccess);
            Assert.IsType<CoercedError>(result.Error);

            adapterMock.Verify(m => m.EnterAsync<ErrorReturningInInitialEnter.A>(), Times.Once);
            adapterMock.Verify(m => m.TransitAsync<ErrorReturningInInitialEnter.A, ErrorReturningInInitialEnter.B>(), Times.Never);
            adapterMock.Verify(m => m.ExitAsync<ErrorReturningInInitialEnter.A>(), Times.Never);
            adapterMock.Verify(m => m.EnterAsync<ErrorReturningInInitialEnter.B>(), Times.Never);
            adapterMock.Verify(m => m.ExitAsync<ErrorReturningInInitialEnter.B>(), Times.Never);
        }

        /// <summary>
        /// Tests whether the state machine propagates exceptions thrown by a call to <see cref="IState.EnterAsync"/>.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public static async Task ThrowsIfStateThrowsInEnter()
        {
            var machine = new StateMachine();

            var adapter = Mock.Of<IStateVerificationAdapter>();
            var initialState = new ThrowingInEnter.A(adapter) { Controller = machine };

            await Assert.ThrowsAsync<CoercedException>(async () => await machine.RunAsync(initialState));
        }

        /// <summary>
        /// Tests whether the state machine has the correct execution flow for exceptions thrown by a call to
        /// <see cref="IState.EnterAsync"/>.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public static async Task HasCorrectFlowIfStateThrowsInEnter()
        {
            var machine = new StateMachine();

            var callSequence = new MockSequence();
            var adapterMock = new Mock<IStateVerificationAdapter>();
            adapterMock.InSequence(callSequence).Setup(m => m.EnterAsync<ThrowingInEnter.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.TransitAsync<ThrowingInEnter.A, ThrowingInEnter.B>());
            adapterMock.InSequence(callSequence).Setup(m => m.ExitAsync<ThrowingInEnter.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.EnterAsync<ThrowingInEnter.A>());

            var initialState = new ThrowingInEnter.A(adapterMock.Object) { Controller = machine };

            await Assert.ThrowsAsync<CoercedException>(async () => await machine.RunAsync(initialState));

            adapterMock.Verify(m => m.EnterAsync<ThrowingInEnter.A>(), Times.Once);
            adapterMock.Verify(m => m.TransitAsync<ThrowingInEnter.A, ThrowingInEnter.B>(), Times.Once);
            adapterMock.Verify(m => m.ExitAsync<ThrowingInEnter.A>(), Times.Once);
            adapterMock.Verify(m => m.EnterAsync<ThrowingInEnter.B>(), Times.Once);
            adapterMock.Verify(m => m.ExitAsync<ThrowingInEnter.B>(), Times.Never);
        }

        /// <summary>
        /// Tests whether the state machine propagates errors returned by a call to <see cref="IState.EnterAsync"/>.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public static async Task ReturnsErrorIfStateReturnsErrorFromEnter()
        {
            var machine = new StateMachine();

            var adapter = Mock.Of<IStateVerificationAdapter>();
            var initialState = new ErrorReturningInEnter.A(adapter) { Controller = machine };

            var result = await machine.RunAsync(initialState);
            Assert.False(result.IsSuccess);
            Assert.IsType<CoercedError>(result.Error);
        }

        /// <summary>
        /// Tests whether the state machine has the correct execution flow for errors returned by a call to
        /// <see cref="IState.EnterAsync"/>.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public static async Task HasCorrectFlowIfStateReturnsErrorFromEnter()
        {
            var machine = new StateMachine();

            var callSequence = new MockSequence();
            var adapterMock = new Mock<IStateVerificationAdapter>();
            adapterMock.InSequence(callSequence).Setup(m => m.EnterAsync<ErrorReturningInEnter.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.TransitAsync<ErrorReturningInEnter.A, ErrorReturningInEnter.B>());
            adapterMock.InSequence(callSequence).Setup(m => m.ExitAsync<ErrorReturningInEnter.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.EnterAsync<ErrorReturningInEnter.A>());

            var initialState = new ErrorReturningInEnter.A(adapterMock.Object) { Controller = machine };

            var result = await machine.RunAsync(initialState);
            Assert.False(result.IsSuccess);
            Assert.IsType<CoercedError>(result.Error);

            adapterMock.Verify(m => m.EnterAsync<ErrorReturningInEnter.A>(), Times.Once);
            adapterMock.Verify(m => m.TransitAsync<ErrorReturningInEnter.A, ErrorReturningInEnter.B>(), Times.Once);
            adapterMock.Verify(m => m.ExitAsync<ErrorReturningInEnter.A>(), Times.Once);
            adapterMock.Verify(m => m.EnterAsync<ErrorReturningInEnter.B>(), Times.Once);
            adapterMock.Verify(m => m.ExitAsync<ErrorReturningInEnter.B>(), Times.Never);
        }

        /// <summary>
        /// Tests whether the state machine propagates exceptions thrown by a call to <see cref="IState.ExitAsync"/>.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public static async Task ThrowsIfStateThrowsInExit()
        {
            var machine = new StateMachine();

            var adapter = Mock.Of<IStateVerificationAdapter>();
            var initialState = new ThrowingInExit.A(adapter) { Controller = machine };

            await Assert.ThrowsAsync<CoercedException>(async () => await machine.RunAsync(initialState));
        }

        /// <summary>
        /// Tests whether the state machine has the correct execution flow for exceptions thrown by a call to
        /// <see cref="IState.ExitAsync"/>.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public static async Task HasCorrectFlowIfStateThrowsInExit()
        {
            var machine = new StateMachine();

            var callSequence = new MockSequence();
            var adapterMock = new Mock<IStateVerificationAdapter>();
            adapterMock.InSequence(callSequence).Setup(m => m.EnterAsync<ThrowingInExit.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.TransitAsync<ThrowingInExit.A, ThrowingInExit.B>());
            adapterMock.InSequence(callSequence).Setup(m => m.ExitAsync<ThrowingInExit.A>());

            var initialState = new ThrowingInExit.A(adapterMock.Object) { Controller = machine };

            await Assert.ThrowsAsync<CoercedException>(async () => await machine.RunAsync(initialState));

            adapterMock.Verify(m => m.EnterAsync<ThrowingInExit.A>(), Times.Once);
            adapterMock.Verify(m => m.TransitAsync<ThrowingInExit.A, ThrowingInExit.B>(), Times.Once);
            adapterMock.Verify(m => m.ExitAsync<ThrowingInExit.A>(), Times.Once);
            adapterMock.Verify(m => m.EnterAsync<ThrowingInExit.B>(), Times.Never);
            adapterMock.Verify(m => m.ExitAsync<ThrowingInExit.B>(), Times.Never);
        }

        /// <summary>
        /// Tests whether the state machine propagates errors returned by a call to <see cref="IState.ExitAsync"/>.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public static async Task ReturnsErrorIfStateReturnsErrorFromExit()
        {
            var machine = new StateMachine();

            var adapter = Mock.Of<IStateVerificationAdapter>();
            var initialState = new ErrorReturningInExit.A(adapter) { Controller = machine };

            var result = await machine.RunAsync(initialState);
            Assert.False(result.IsSuccess);
            Assert.IsType<CoercedError>(result.Error);
        }

        /// <summary>
        /// Tests whether the state machine has the correct execution flow for errors returned by a call to
        /// <see cref="IState.ExitAsync"/>.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public static async Task HasCorrectFlowIfStateReturnsErrorExit()
        {
            var machine = new StateMachine();

            var callSequence = new MockSequence();
            var adapterMock = new Mock<IStateVerificationAdapter>();
            adapterMock.InSequence(callSequence).Setup(m => m.EnterAsync<ErrorReturningInExit.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.TransitAsync<ErrorReturningInExit.A, ErrorReturningInExit.B>());
            adapterMock.InSequence(callSequence).Setup(m => m.ExitAsync<ErrorReturningInExit.A>());

            var initialState = new ErrorReturningInExit.A(adapterMock.Object) { Controller = machine };

            var result = await machine.RunAsync(initialState);
            Assert.False(result.IsSuccess);
            Assert.IsType<CoercedError>(result.Error);

            adapterMock.Verify(m => m.EnterAsync<ErrorReturningInExit.A>(), Times.Once);
            adapterMock.Verify(m => m.TransitAsync<ErrorReturningInExit.A, ErrorReturningInExit.B>(), Times.Once);
            adapterMock.Verify(m => m.ExitAsync<ErrorReturningInExit.A>(), Times.Once);
            adapterMock.Verify(m => m.EnterAsync<ErrorReturningInExit.B>(), Times.Never);
            adapterMock.Verify(m => m.ExitAsync<ErrorReturningInExit.B>(), Times.Never);
        }

        /// <summary>
        /// Tests whether the state machine propagates exceptions thrown by a call to <see cref="IState.ExitAsync"/> of
        /// the final state.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public static async Task ThrowsIfStateThrowsInFinalExit()
        {
            var machine = new StateMachine();

            var adapter = Mock.Of<IStateVerificationAdapter>();
            var initialState = new ThrowingInFinalExit.A(adapter) { Controller = machine };

            await Assert.ThrowsAsync<CoercedException>(async () => await machine.RunAsync(initialState));
        }

        /// <summary>
        /// Tests whether the state machine has the correct execution flow for exceptions thrown by a call to
        /// <see cref="IState.ExitAsync"/> of the final state.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public static async Task HasCorrectFlowIfStateThrowsInFinalExit()
        {
            var machine = new StateMachine();

            var callSequence = new MockSequence();
            var adapterMock = new Mock<IStateVerificationAdapter>();
            adapterMock.InSequence(callSequence).Setup(m => m.EnterAsync<ThrowingInFinalExit.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.TransitAsync<ThrowingInFinalExit.A, ThrowingInFinalExit.B>());
            adapterMock.InSequence(callSequence).Setup(m => m.ExitAsync<ThrowingInFinalExit.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.EnterAsync<ThrowingInFinalExit.B>());
            adapterMock.InSequence(callSequence).Setup(m => m.ExitAsync<ThrowingInFinalExit.B>());

            var initialState = new ThrowingInFinalExit.A(adapterMock.Object) { Controller = machine };

            await Assert.ThrowsAsync<CoercedException>(async () => await machine.RunAsync(initialState));

            adapterMock.Verify(m => m.EnterAsync<ThrowingInFinalExit.A>(), Times.Once);
            adapterMock.Verify(m => m.TransitAsync<ThrowingInFinalExit.A, ThrowingInFinalExit.B>(), Times.Once);
            adapterMock.Verify(m => m.ExitAsync<ThrowingInFinalExit.A>(), Times.Once);
            adapterMock.Verify(m => m.EnterAsync<ThrowingInFinalExit.B>(), Times.Once);
            adapterMock.Verify(m => m.ExitAsync<ThrowingInFinalExit.B>(), Times.Once);
        }

        /// <summary>
        /// Tests whether the state machine propagates errors returned by a call to <see cref="IState.ExitAsync"/> of
        /// the final state.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public static async Task ReturnsErrorIfStateReturnsErrorFromFinalExit()
        {
            var machine = new StateMachine();

            var adapter = Mock.Of<IStateVerificationAdapter>();
            var initialState = new ErrorReturningInFinalExit.A(adapter) { Controller = machine };

            var result = await machine.RunAsync(initialState);
            Assert.False(result.IsSuccess);
            Assert.IsType<CoercedError>(result.Error);
        }

        /// <summary>
        /// Tests whether the state machine has the correct execution flow for errors returned by a call to
        /// <see cref="IState.ExitAsync"/> of the final state.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public static async Task HasCorrectFlowIfStateReturnsErrorInFinalExit()
        {
            var machine = new StateMachine();

            var callSequence = new MockSequence();
            var adapterMock = new Mock<IStateVerificationAdapter>();
            adapterMock.InSequence(callSequence).Setup(m => m.EnterAsync<ErrorReturningInFinalExit.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.TransitAsync<ErrorReturningInFinalExit.A, ErrorReturningInFinalExit.B>());
            adapterMock.InSequence(callSequence).Setup(m => m.ExitAsync<ErrorReturningInFinalExit.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.EnterAsync<ErrorReturningInFinalExit.B>());
            adapterMock.InSequence(callSequence).Setup(m => m.ExitAsync<ErrorReturningInFinalExit.B>());

            var initialState = new ErrorReturningInFinalExit.A(adapterMock.Object) { Controller = machine };

            var result = await machine.RunAsync(initialState);
            Assert.False(result.IsSuccess);
            Assert.IsType<CoercedError>(result.Error);

            adapterMock.Verify(m => m.EnterAsync<ErrorReturningInFinalExit.A>(), Times.Once);
            adapterMock.Verify(m => m.TransitAsync<ErrorReturningInFinalExit.A, ErrorReturningInFinalExit.B>(), Times.Once);
            adapterMock.Verify(m => m.ExitAsync<ErrorReturningInFinalExit.A>(), Times.Once);
            adapterMock.Verify(m => m.EnterAsync<ErrorReturningInFinalExit.B>(), Times.Once);
            adapterMock.Verify(m => m.ExitAsync<ErrorReturningInFinalExit.B>(), Times.Once);
        }

        /// <summary>
        /// Tests whether the state machine propagates exceptions thrown by a call to
        /// <see cref="ITransition{TState}.TransitAsync"/>.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public static async Task ThrowsIfStateThrowsInTransition()
        {
            var machine = new StateMachine();

            var adapter = Mock.Of<IStateVerificationAdapter>();
            var initialState = new ThrowingInTransition.A(adapter) { Controller = machine };

            await Assert.ThrowsAsync<CoercedException>(async () => await machine.RunAsync(initialState));
        }

        /// <summary>
        /// Tests whether the state machine has the correct execution flow for exceptions thrown by a call to
        /// <see cref="ITransition{TState}.TransitAsync"/>.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public static async Task HasCorrectFlowIfStateThrowsInTransition()
        {
            var machine = new StateMachine();

            var callSequence = new MockSequence();
            var adapterMock = new Mock<IStateVerificationAdapter>();
            adapterMock.InSequence(callSequence).Setup(m => m.EnterAsync<ThrowingInTransition.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.TransitAsync<ThrowingInTransition.A, ThrowingInTransition.B>());
            adapterMock.InSequence(callSequence).Setup(m => m.ExitAsync<ThrowingInTransition.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.EnterAsync<ThrowingInTransition.B>());
            adapterMock.InSequence(callSequence).Setup(m => m.ExitAsync<ThrowingInTransition.B>());

            var initialState = new ThrowingInTransition.A(adapterMock.Object) { Controller = machine };

            await Assert.ThrowsAsync<CoercedException>(async () => await machine.RunAsync(initialState));

            adapterMock.Verify(m => m.EnterAsync<ThrowingInTransition.A>(), Times.Once);
            adapterMock.Verify(m => m.TransitAsync<ThrowingInTransition.A, ThrowingInTransition.B>(), Times.Once);
            adapterMock.Verify(m => m.ExitAsync<ThrowingInTransition.A>(), Times.Never);
            adapterMock.Verify(m => m.EnterAsync<ThrowingInTransition.B>(), Times.Never);
            adapterMock.Verify(m => m.ExitAsync<ThrowingInTransition.B>(), Times.Never);
        }

        /// <summary>
        /// Tests whether the state machine propagates errors returned by a call to
        /// <see cref="ITransition{TState}.TransitAsync"/>.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public static async Task ReturnsErrorIfStateReturnsErrorFromTransition()
        {
            var machine = new StateMachine();

            var adapter = Mock.Of<IStateVerificationAdapter>();
            var initialState = new ErrorReturningInTransition.A(adapter) { Controller = machine };

            var result = await machine.RunAsync(initialState);
            Assert.False(result.IsSuccess);
            Assert.IsType<CoercedError>(result.Error);
        }

        /// <summary>
        /// Tests whether the state machine has the correct execution flow for errors returned by a call to
        /// <see cref="ITransition{TState}.TransitAsync"/>.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public static async Task HasCorrectFlowIfStateReturnsErrorInTransition()
        {
            var machine = new StateMachine();

            var callSequence = new MockSequence();
            var adapterMock = new Mock<IStateVerificationAdapter>();
            adapterMock.InSequence(callSequence).Setup(m => m.EnterAsync<ErrorReturningInTransition.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.TransitAsync<ErrorReturningInTransition.A, ErrorReturningInTransition.B>());
            adapterMock.InSequence(callSequence).Setup(m => m.ExitAsync<ErrorReturningInTransition.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.EnterAsync<ErrorReturningInTransition.B>());
            adapterMock.InSequence(callSequence).Setup(m => m.ExitAsync<ErrorReturningInTransition.B>());

            var initialState = new ErrorReturningInTransition.A(adapterMock.Object) { Controller = machine };

            var result = await machine.RunAsync(initialState);
            Assert.False(result.IsSuccess);
            Assert.IsType<CoercedError>(result.Error);

            adapterMock.Verify(m => m.EnterAsync<ErrorReturningInTransition.A>(), Times.Once);
            adapterMock.Verify(m => m.TransitAsync<ErrorReturningInTransition.A, ErrorReturningInTransition.B>(), Times.Once);
            adapterMock.Verify(m => m.ExitAsync<ErrorReturningInTransition.A>(), Times.Never);
            adapterMock.Verify(m => m.EnterAsync<ErrorReturningInTransition.B>(), Times.Never);
            adapterMock.Verify(m => m.ExitAsync<ErrorReturningInTransition.B>(), Times.Never);
        }
    }

    /// <summary>
    /// Tests erroneous usage of the <see cref="StateMachine.RunAsync{TState}(TState,CancellationToken)"/> method.
    /// </summary>
    public static class Errors
    {
        /// <summary>
        /// Tests whether the state machine throws if a state attempts to transition to a state that it doesn't have a
        /// declared relationship to.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public static async Task ThrowsIfStateAttemptsTransitionToNonNeighbour()
        {
            var machine = new StateMachine();

            var adapter = Mock.Of<IStateVerificationAdapter>();
            var initialState = new TransitioningToNonNeighbour.A(adapter) { Controller = machine };

            await Assert.ThrowsAsync<InvalidOperationException>(async () => await machine.RunAsync(initialState));
        }

        /// <summary>
        /// Tests whether the state machine throws if a state attempts to transition to another state multiple times.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public static async Task ThrowsIfStateAttemptsTransitionMultipleTimes()
        {
            var machine = new StateMachine();

            var adapter = Mock.Of<IStateVerificationAdapter>();
            var initialState = new MultipleTransitions.A(adapter) { Controller = machine };

            await Assert.ThrowsAsync<InvalidOperationException>(async () => await machine.RunAsync(initialState));
        }

        /// <summary>
        /// Tests whether the state machine throws if a state attempts to exit the machine multiple times.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public static async Task ThrowsIfStateAttemptsExitMultipleTimes()
        {
            var machine = new StateMachine();

            var adapter = Mock.Of<IStateVerificationAdapter>();
            var initialState = new MultipleExits.A(adapter) { Controller = machine };

            await Assert.ThrowsAsync<InvalidOperationException>(async () => await machine.RunAsync(initialState));
        }

        /// <summary>
        /// Tests whether the state machine throws if the user attempts to run the machine again while it is already
        /// running.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public static async Task ThrowsIfCalledWhileMachineIsRunning()
        {
            var machine = new StateMachine();

            var adapter = Mock.Of<IStateVerificationAdapter>();

            var tokenSource = new CancellationTokenSource();
            var initialState = new NonForkingNonTerminating.A(adapter, tokenSource.Token) { Controller = machine };

            var initialRun = machine.RunAsync(initialState, CancellationToken.None);
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await machine.RunAsync(initialState, CancellationToken.None));

            tokenSource.Cancel();
            await initialRun;
        }
    }

    /// <summary>
    /// Tests the <see cref="StateMachine.RunAsync{TState}(TState,CancellationToken)"/> method's disposal handling.
    /// </summary>
    public static class Disposal
    {
        /// <summary>
        /// Tests whether states are disposed of correctly in the ideal happy-path cases.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public static async Task DisposesStatesCorrectlyForNonForkingSelfTerminatingGraph()
        {
            var machine = new StateMachine();

            var callSequence = new MockSequence();
            var adapterMock = new Mock<IStateVerificationAdapter>();
            adapterMock.InSequence(callSequence).Setup(m => m.EnterAsync<NonForkingSelfTerminating.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.TransitAsync<NonForkingSelfTerminating.A, NonForkingSelfTerminating.B>());
            adapterMock.InSequence(callSequence).Setup(m => m.ExitAsync<NonForkingSelfTerminating.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.Dispose<DisposableNonForkingSelfTerminating.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.EnterAsync<NonForkingSelfTerminating.B>());
            adapterMock.InSequence(callSequence).Setup(m => m.ExitAsync<NonForkingSelfTerminating.B>());
            adapterMock.InSequence(callSequence).Setup(m => m.Dispose<DisposableNonForkingSelfTerminating.B>());

            var initialState = new DisposableNonForkingSelfTerminating.A(adapterMock.Object) { Controller = machine };

            var result = await machine.RunAsync(initialState);
            Assert.True(result.IsSuccess);

            adapterMock.Verify(m => m.EnterAsync<NonForkingSelfTerminating.A>(), Times.Once);
            adapterMock.Verify(m => m.TransitAsync<NonForkingSelfTerminating.A, NonForkingSelfTerminating.B>(), Times.Once);
            adapterMock.Verify(m => m.ExitAsync<NonForkingSelfTerminating.A>(), Times.Once);
            adapterMock.Verify(m => m.Dispose<DisposableNonForkingSelfTerminating.A>(), Times.Once);
            adapterMock.Verify(m => m.EnterAsync<NonForkingSelfTerminating.B>(), Times.Once);
            adapterMock.Verify(m => m.ExitAsync<NonForkingSelfTerminating.B>(), Times.Once);
            adapterMock.Verify(m => m.Dispose<DisposableNonForkingSelfTerminating.B>(), Times.Once);
        }
    }
}
