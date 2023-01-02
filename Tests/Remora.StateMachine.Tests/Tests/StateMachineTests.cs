//
//  SPDX-FileName: StateMachineTests.cs
//  SPDX-FileCopyrightText: Copyright (c) Jarl Gullberg
//  SPDX-License-Identifier: LGPL-3.0-or-later
//

using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Remora.StateMachine.Tests.Graphs.AsyncDisposable;
using Remora.StateMachine.Tests.Graphs.Broken;
using Remora.StateMachine.Tests.Graphs.Disposable;
using Remora.StateMachine.Tests.Graphs.DisposableAndAsyncDisposable;
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
            tokenSource.Cancel();

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
            tokenSource.Cancel();

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
            tokenSource.Cancel();

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
            tokenSource.Cancel();

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
            adapterMock.InSequence(callSequence).Setup(m => m.EnterAsync<DisposableNonForkingSelfTerminating.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.TransitAsync<DisposableNonForkingSelfTerminating.A, DisposableNonForkingSelfTerminating.B>());
            adapterMock.InSequence(callSequence).Setup(m => m.ExitAsync<DisposableNonForkingSelfTerminating.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.Dispose<DisposableNonForkingSelfTerminating.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.EnterAsync<DisposableNonForkingSelfTerminating.B>());
            adapterMock.InSequence(callSequence).Setup(m => m.ExitAsync<DisposableNonForkingSelfTerminating.B>());
            adapterMock.InSequence(callSequence).Setup(m => m.Dispose<DisposableNonForkingSelfTerminating.B>());

            var initialState = new DisposableNonForkingSelfTerminating.A(adapterMock.Object) { Controller = machine };

            var result = await machine.RunAsync(initialState);
            Assert.True(result.IsSuccess);

            adapterMock.Verify(m => m.EnterAsync<DisposableNonForkingSelfTerminating.A>(), Times.Once);
            adapterMock.Verify(m => m.TransitAsync<DisposableNonForkingSelfTerminating.A, DisposableNonForkingSelfTerminating.B>(), Times.Once);
            adapterMock.Verify(m => m.ExitAsync<DisposableNonForkingSelfTerminating.A>(), Times.Once);
            adapterMock.Verify(m => m.Dispose<DisposableNonForkingSelfTerminating.A>(), Times.Once);
            adapterMock.Verify(m => m.EnterAsync<DisposableNonForkingSelfTerminating.B>(), Times.Once);
            adapterMock.Verify(m => m.ExitAsync<DisposableNonForkingSelfTerminating.B>(), Times.Once);
            adapterMock.Verify(m => m.Dispose<DisposableNonForkingSelfTerminating.B>(), Times.Once);
        }

        /// <summary>
        /// Tests whether states are disposed of correctly in the ideal happy-path cases.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public static async Task DisposesStatesCorrectlyForNonForkingNonTerminatingGraph()
        {
            var machine = new StateMachine();

            var callSequence = new MockSequence();
            var adapterMock = new Mock<IStateVerificationAdapter>();
            adapterMock.InSequence(callSequence).Setup(m => m.EnterAsync<DisposableNonForkingNonTerminating.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.TransitAsync<DisposableNonForkingNonTerminating.A, DisposableNonForkingNonTerminating.B>());
            adapterMock.InSequence(callSequence).Setup(m => m.ExitAsync<DisposableNonForkingNonTerminating.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.Dispose<DisposableNonForkingNonTerminating.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.EnterAsync<DisposableNonForkingNonTerminating.B>());
            adapterMock.InSequence(callSequence).Setup(m => m.ExitAsync<DisposableNonForkingNonTerminating.B>());
            adapterMock.InSequence(callSequence).Setup(m => m.Dispose<DisposableNonForkingNonTerminating.B>());

            var tokenSource = new CancellationTokenSource();
            tokenSource.Cancel();

            var initialState = new DisposableNonForkingNonTerminating.A(adapterMock.Object, tokenSource.Token) { Controller = machine };

            var result = await machine.RunAsync(initialState, CancellationToken.None);
            Assert.True(result.IsSuccess);

            adapterMock.Verify(m => m.EnterAsync<DisposableNonForkingNonTerminating.A>(), Times.Once);
            adapterMock.Verify(m => m.TransitAsync<DisposableNonForkingNonTerminating.A, DisposableNonForkingNonTerminating.B>(), Times.Once);
            adapterMock.Verify(m => m.ExitAsync<DisposableNonForkingNonTerminating.A>(), Times.Once);
            adapterMock.Verify(m => m.Dispose<DisposableNonForkingNonTerminating.A>(), Times.Once);
            adapterMock.Verify(m => m.EnterAsync<DisposableNonForkingNonTerminating.B>(), Times.Once);
            adapterMock.Verify(m => m.ExitAsync<DisposableNonForkingNonTerminating.B>(), Times.Once);
            adapterMock.Verify(m => m.Dispose<DisposableNonForkingNonTerminating.B>(), Times.Once);
        }

        /// <summary>
        /// Tests whether states are disposed of correctly in the ideal happy-path cases.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public static async Task DisposesStatesCorrectlyForForkingSelfTerminatingGraph()
        {
            var machine = new StateMachine();

            var callSequence = new MockSequence();
            var adapterMock = new Mock<IStateVerificationAdapter>();
            adapterMock.InSequence(callSequence).Setup(m => m.EnterAsync<DisposableForkingSelfTerminating.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.TransitAsync<DisposableForkingSelfTerminating.A, DisposableForkingSelfTerminating.B>());
            adapterMock.InSequence(callSequence).Setup(m => m.ExitAsync<DisposableForkingSelfTerminating.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.Dispose<DisposableForkingSelfTerminating.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.EnterAsync<DisposableForkingSelfTerminating.B>());
            adapterMock.InSequence(callSequence).Setup(m => m.ExitAsync<DisposableForkingSelfTerminating.B>());
            adapterMock.InSequence(callSequence).Setup(m => m.Dispose<DisposableForkingSelfTerminating.B>());

            var initialState = new DisposableForkingSelfTerminating.A(adapterMock.Object) { Controller = machine };

            var result = await machine.RunAsync(initialState);
            Assert.True(result.IsSuccess);

            adapterMock.Verify(m => m.EnterAsync<DisposableForkingSelfTerminating.A>(), Times.Once);
            adapterMock.Verify(m => m.TransitAsync<DisposableForkingSelfTerminating.A, DisposableForkingSelfTerminating.B>(), Times.Once);
            adapterMock.Verify(m => m.ExitAsync<DisposableForkingSelfTerminating.A>(), Times.Once);
            adapterMock.Verify(m => m.Dispose<DisposableForkingSelfTerminating.A>(), Times.Once);
            adapterMock.Verify(m => m.EnterAsync<DisposableForkingSelfTerminating.B>(), Times.Once);
            adapterMock.Verify(m => m.ExitAsync<DisposableForkingSelfTerminating.B>(), Times.Once);
            adapterMock.Verify(m => m.Dispose<DisposableForkingSelfTerminating.B>(), Times.Once);
        }

        /// <summary>
        /// Tests whether states are disposed of correctly in the ideal happy-path cases.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public static async Task DisposesStatesCorrectlyForForkingNonTerminatingGraph()
        {
            var machine = new StateMachine();

            var callSequence = new MockSequence();
            var adapterMock = new Mock<IStateVerificationAdapter>();
            adapterMock.InSequence(callSequence).Setup(m => m.EnterAsync<DisposableForkingNonTerminating.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.TransitAsync<DisposableForkingNonTerminating.A, DisposableForkingNonTerminating.B>());
            adapterMock.InSequence(callSequence).Setup(m => m.ExitAsync<DisposableForkingNonTerminating.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.Dispose<DisposableForkingNonTerminating.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.EnterAsync<DisposableForkingNonTerminating.B>());
            adapterMock.InSequence(callSequence).Setup(m => m.ExitAsync<DisposableForkingNonTerminating.B>());
            adapterMock.InSequence(callSequence).Setup(m => m.Dispose<DisposableForkingNonTerminating.B>());

            var tokenSource = new CancellationTokenSource();
            tokenSource.Cancel();

            var initialState = new DisposableForkingNonTerminating.A(adapterMock.Object, tokenSource.Token) { Controller = machine };

            var result = await machine.RunAsync(initialState, CancellationToken.None);
            Assert.True(result.IsSuccess);

            adapterMock.Verify(m => m.EnterAsync<DisposableForkingNonTerminating.A>(), Times.Once);
            adapterMock.Verify(m => m.TransitAsync<DisposableForkingNonTerminating.A, DisposableForkingNonTerminating.B>(), Times.Once);
            adapterMock.Verify(m => m.ExitAsync<DisposableForkingNonTerminating.A>(), Times.Once);
            adapterMock.Verify(m => m.Dispose<DisposableForkingNonTerminating.A>(), Times.Once);
            adapterMock.Verify(m => m.EnterAsync<DisposableForkingNonTerminating.B>(), Times.Once);
            adapterMock.Verify(m => m.ExitAsync<DisposableForkingNonTerminating.B>(), Times.Once);
            adapterMock.Verify(m => m.Dispose<DisposableForkingNonTerminating.B>(), Times.Once);
        }

        /// <summary>
        /// Tests whether states are disposed of correctly for exceptions thrown by a call to
        /// <see cref="IState.EnterAsync"/> of the initial state.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public static async Task DisposesStatesCorrectlyIfInitialStateThrowsInEnter()
        {
            var machine = new StateMachine();

            var callSequence = new MockSequence();
            var adapterMock = new Mock<IStateVerificationAdapter>();
            adapterMock.InSequence(callSequence).Setup(m => m.EnterAsync<DisposableThrowingInInitialEnter.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.Dispose<DisposableThrowingInInitialEnter.A>());

            var initialState = new DisposableThrowingInInitialEnter.A(adapterMock.Object) { Controller = machine };

            await Assert.ThrowsAsync<CoercedException>(async () => await machine.RunAsync(initialState));

            adapterMock.Verify(m => m.EnterAsync<DisposableThrowingInInitialEnter.A>(), Times.Once);
            adapterMock.Verify(m => m.TransitAsync<DisposableThrowingInInitialEnter.A, DisposableThrowingInInitialEnter.B>(), Times.Never);
            adapterMock.Verify(m => m.ExitAsync<DisposableThrowingInInitialEnter.A>(), Times.Never);
            adapterMock.Verify(m => m.Dispose<DisposableThrowingInInitialEnter.A>(), Times.Once);
            adapterMock.Verify(m => m.EnterAsync<DisposableThrowingInInitialEnter.B>(), Times.Never);
            adapterMock.Verify(m => m.ExitAsync<DisposableThrowingInInitialEnter.B>(), Times.Never);
            adapterMock.Verify(m => m.Dispose<DisposableThrowingInInitialEnter.B>(), Times.Never);
        }

        /// <summary>
        /// Tests whether states are disposed of correctly for errors returned by a call to
        /// <see cref="IState.EnterAsync"/> of the initial state.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public static async Task DisposesStatesCorrectlyIfInitialStateReturnsErrorFromEnter()
        {
            var machine = new StateMachine();

            var callSequence = new MockSequence();
            var adapterMock = new Mock<IStateVerificationAdapter>();
            adapterMock.InSequence(callSequence).Setup(m => m.EnterAsync<DisposableErrorReturningInInitialEnter.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.Dispose<DisposableErrorReturningInInitialEnter.A>());

            var initialState = new DisposableErrorReturningInInitialEnter.A(adapterMock.Object) { Controller = machine };

            var result = await machine.RunAsync(initialState);
            Assert.False(result.IsSuccess);
            Assert.IsType<CoercedError>(result.Error);

            adapterMock.Verify(m => m.EnterAsync<DisposableErrorReturningInInitialEnter.A>(), Times.Once);
            adapterMock.Verify(m => m.TransitAsync<DisposableErrorReturningInInitialEnter.A, DisposableErrorReturningInInitialEnter.B>(), Times.Never);
            adapterMock.Verify(m => m.ExitAsync<DisposableErrorReturningInInitialEnter.A>(), Times.Never);
            adapterMock.Verify(m => m.Dispose<DisposableErrorReturningInInitialEnter.A>(), Times.Once);
            adapterMock.Verify(m => m.EnterAsync<DisposableErrorReturningInInitialEnter.B>(), Times.Never);
            adapterMock.Verify(m => m.ExitAsync<DisposableErrorReturningInInitialEnter.B>(), Times.Never);
            adapterMock.Verify(m => m.Dispose<DisposableErrorReturningInInitialEnter.B>(), Times.Never);
        }

        /// <summary>
        /// Tests whether states are disposed of correctly for exceptions thrown by a call to
        /// <see cref="IState.EnterAsync"/>.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public static async Task DisposesStatesCorrectlyIfStateThrowsInEnter()
        {
            var machine = new StateMachine();

            var callSequence = new MockSequence();
            var adapterMock = new Mock<IStateVerificationAdapter>();
            adapterMock.InSequence(callSequence).Setup(m => m.EnterAsync<DisposableThrowingInEnter.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.TransitAsync<DisposableThrowingInEnter.A, DisposableThrowingInEnter.B>());
            adapterMock.InSequence(callSequence).Setup(m => m.ExitAsync<DisposableThrowingInEnter.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.Dispose<DisposableThrowingInEnter.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.EnterAsync<DisposableThrowingInEnter.B>());
            adapterMock.InSequence(callSequence).Setup(m => m.Dispose<DisposableThrowingInEnter.B>());

            var initialState = new DisposableThrowingInEnter.A(adapterMock.Object) { Controller = machine };

            await Assert.ThrowsAsync<CoercedException>(async () => await machine.RunAsync(initialState));

            adapterMock.Verify(m => m.EnterAsync<DisposableThrowingInEnter.A>(), Times.Once);
            adapterMock.Verify(m => m.TransitAsync<DisposableThrowingInEnter.A, DisposableThrowingInEnter.B>(), Times.Once);
            adapterMock.Verify(m => m.ExitAsync<DisposableThrowingInEnter.A>(), Times.Once);
            adapterMock.Verify(m => m.Dispose<DisposableThrowingInEnter.A>(), Times.Once);
            adapterMock.Verify(m => m.EnterAsync<DisposableThrowingInEnter.B>(), Times.Once);
            adapterMock.Verify(m => m.ExitAsync<DisposableThrowingInEnter.B>(), Times.Never);
            adapterMock.Verify(m => m.Dispose<DisposableThrowingInEnter.B>(), Times.Once);
        }

        /// <summary>
        /// Tests whether states are disposed of correctly for errors returned by a call to
        /// <see cref="IState.EnterAsync"/>.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public static async Task DisposesStatesCorrectlyIfStateReturnsErrorFromEnter()
        {
            var machine = new StateMachine();

            var callSequence = new MockSequence();
            var adapterMock = new Mock<IStateVerificationAdapter>();
            adapterMock.InSequence(callSequence).Setup(m => m.EnterAsync<DisposableErrorReturningInEnter.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.TransitAsync<DisposableErrorReturningInEnter.A, DisposableErrorReturningInEnter.B>());
            adapterMock.InSequence(callSequence).Setup(m => m.ExitAsync<DisposableErrorReturningInEnter.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.Dispose<DisposableErrorReturningInEnter.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.EnterAsync<DisposableErrorReturningInEnter.B>());
            adapterMock.InSequence(callSequence).Setup(m => m.Dispose<DisposableErrorReturningInEnter.B>());

            var initialState = new DisposableErrorReturningInEnter.A(adapterMock.Object) { Controller = machine };

            var result = await machine.RunAsync(initialState);
            Assert.False(result.IsSuccess);
            Assert.IsType<CoercedError>(result.Error);

            adapterMock.Verify(m => m.EnterAsync<DisposableErrorReturningInEnter.A>(), Times.Once);
            adapterMock.Verify(m => m.TransitAsync<DisposableErrorReturningInEnter.A, DisposableErrorReturningInEnter.B>(), Times.Once);
            adapterMock.Verify(m => m.ExitAsync<DisposableErrorReturningInEnter.A>(), Times.Once);
            adapterMock.Verify(m => m.Dispose<DisposableErrorReturningInEnter.A>(), Times.Once);
            adapterMock.Verify(m => m.EnterAsync<DisposableErrorReturningInEnter.B>(), Times.Once);
            adapterMock.Verify(m => m.ExitAsync<DisposableErrorReturningInEnter.B>(), Times.Never);
            adapterMock.Verify(m => m.Dispose<DisposableErrorReturningInEnter.B>(), Times.Once);
        }

        /// <summary>
        /// Tests whether states are disposed of correctly for exceptions thrown by a call to
        /// <see cref="IState.ExitAsync"/>.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public static async Task DisposesStatesCorrectlyIfStateThrowsInExit()
        {
            var machine = new StateMachine();

            var callSequence = new MockSequence();
            var adapterMock = new Mock<IStateVerificationAdapter>();
            adapterMock.InSequence(callSequence).Setup(m => m.EnterAsync<DisposableThrowingInExit.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.TransitAsync<DisposableThrowingInExit.A, DisposableThrowingInExit.B>());
            adapterMock.InSequence(callSequence).Setup(m => m.ExitAsync<DisposableThrowingInExit.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.Dispose<DisposableThrowingInExit.A>());

            var initialState = new DisposableThrowingInExit.A(adapterMock.Object) { Controller = machine };

            await Assert.ThrowsAsync<CoercedException>(async () => await machine.RunAsync(initialState));

            adapterMock.Verify(m => m.EnterAsync<DisposableThrowingInExit.A>(), Times.Once);
            adapterMock.Verify(m => m.TransitAsync<DisposableThrowingInExit.A, DisposableThrowingInExit.B>(), Times.Once);
            adapterMock.Verify(m => m.ExitAsync<DisposableThrowingInExit.A>(), Times.Once);
            adapterMock.Verify(m => m.Dispose<DisposableThrowingInExit.A>(), Times.Once);
            adapterMock.Verify(m => m.EnterAsync<DisposableThrowingInExit.B>(), Times.Never);
            adapterMock.Verify(m => m.ExitAsync<DisposableThrowingInExit.B>(), Times.Never);
            adapterMock.Verify(m => m.Dispose<DisposableThrowingInExit.B>(), Times.Once);
        }

        /// <summary>
        /// Tests whether states are disposed of correctly for errors returned by a call to
        /// <see cref="IState.ExitAsync"/>.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public static async Task DisposesStatesCorrectlyIfStateReturnsErrorExit()
        {
            var machine = new StateMachine();

            var callSequence = new MockSequence();
            var adapterMock = new Mock<IStateVerificationAdapter>();
            adapterMock.InSequence(callSequence).Setup(m => m.EnterAsync<DisposableErrorReturningInExit.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.TransitAsync<DisposableErrorReturningInExit.A, DisposableErrorReturningInExit.B>());
            adapterMock.InSequence(callSequence).Setup(m => m.ExitAsync<DisposableErrorReturningInExit.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.Dispose<DisposableErrorReturningInExit.A>());

            var initialState = new DisposableErrorReturningInExit.A(adapterMock.Object) { Controller = machine };

            var result = await machine.RunAsync(initialState);
            Assert.False(result.IsSuccess);
            Assert.IsType<CoercedError>(result.Error);

            adapterMock.Verify(m => m.EnterAsync<DisposableErrorReturningInExit.A>(), Times.Once);
            adapterMock.Verify(m => m.TransitAsync<DisposableErrorReturningInExit.A, DisposableErrorReturningInExit.B>(), Times.Once);
            adapterMock.Verify(m => m.ExitAsync<DisposableErrorReturningInExit.A>(), Times.Once);
            adapterMock.Verify(m => m.Dispose<DisposableErrorReturningInExit.A>(), Times.Once);
            adapterMock.Verify(m => m.EnterAsync<DisposableErrorReturningInExit.B>(), Times.Never);
            adapterMock.Verify(m => m.ExitAsync<DisposableErrorReturningInExit.B>(), Times.Never);
            adapterMock.Verify(m => m.Dispose<DisposableErrorReturningInExit.B>(), Times.Once);
        }

        /// <summary>
        /// Tests whether states are disposed of correctly for exceptions thrown by a call to
        /// <see cref="IState.ExitAsync"/> of the final state.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public static async Task DisposesStatesCorrectlyIfStateThrowsInFinalExit()
        {
            var machine = new StateMachine();

            var callSequence = new MockSequence();
            var adapterMock = new Mock<IStateVerificationAdapter>();
            adapterMock.InSequence(callSequence).Setup(m => m.EnterAsync<DisposableThrowingInFinalExit.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.TransitAsync<DisposableThrowingInFinalExit.A, DisposableThrowingInFinalExit.B>());
            adapterMock.InSequence(callSequence).Setup(m => m.ExitAsync<DisposableThrowingInFinalExit.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.Dispose<DisposableThrowingInFinalExit.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.EnterAsync<DisposableThrowingInFinalExit.B>());
            adapterMock.InSequence(callSequence).Setup(m => m.ExitAsync<DisposableThrowingInFinalExit.B>());
            adapterMock.InSequence(callSequence).Setup(m => m.Dispose<DisposableThrowingInFinalExit.B>());

            var initialState = new DisposableThrowingInFinalExit.A(adapterMock.Object) { Controller = machine };

            await Assert.ThrowsAsync<CoercedException>(async () => await machine.RunAsync(initialState));

            adapterMock.Verify(m => m.EnterAsync<DisposableThrowingInFinalExit.A>(), Times.Once);
            adapterMock.Verify(m => m.TransitAsync<DisposableThrowingInFinalExit.A, DisposableThrowingInFinalExit.B>(), Times.Once);
            adapterMock.Verify(m => m.ExitAsync<DisposableThrowingInFinalExit.A>(), Times.Once);
            adapterMock.Verify(m => m.Dispose<DisposableThrowingInFinalExit.A>(), Times.Once);
            adapterMock.Verify(m => m.EnterAsync<DisposableThrowingInFinalExit.B>(), Times.Once);
            adapterMock.Verify(m => m.ExitAsync<DisposableThrowingInFinalExit.B>(), Times.Once);
            adapterMock.Verify(m => m.Dispose<DisposableThrowingInFinalExit.B>(), Times.Once);
        }

        /// <summary>
        /// Tests whether states are disposed of correctly for errors returned by a call to
        /// <see cref="IState.ExitAsync"/> of the final state.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public static async Task DisposesStatesCorrectlyIfStateReturnsErrorInFinalExit()
        {
            var machine = new StateMachine();

            var callSequence = new MockSequence();
            var adapterMock = new Mock<IStateVerificationAdapter>();
            adapterMock.InSequence(callSequence).Setup(m => m.EnterAsync<DisposableErrorReturningInFinalExit.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.TransitAsync<DisposableErrorReturningInFinalExit.A, DisposableErrorReturningInFinalExit.B>());
            adapterMock.InSequence(callSequence).Setup(m => m.ExitAsync<DisposableErrorReturningInFinalExit.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.Dispose<DisposableErrorReturningInFinalExit.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.EnterAsync<DisposableErrorReturningInFinalExit.B>());
            adapterMock.InSequence(callSequence).Setup(m => m.ExitAsync<DisposableErrorReturningInFinalExit.B>());
            adapterMock.InSequence(callSequence).Setup(m => m.Dispose<DisposableErrorReturningInFinalExit.B>());

            var initialState = new DisposableErrorReturningInFinalExit.A(adapterMock.Object) { Controller = machine };

            var result = await machine.RunAsync(initialState);
            Assert.False(result.IsSuccess);
            Assert.IsType<CoercedError>(result.Error);

            adapterMock.Verify(m => m.EnterAsync<DisposableErrorReturningInFinalExit.A>(), Times.Once);
            adapterMock.Verify(m => m.TransitAsync<DisposableErrorReturningInFinalExit.A, DisposableErrorReturningInFinalExit.B>(), Times.Once);
            adapterMock.Verify(m => m.ExitAsync<DisposableErrorReturningInFinalExit.A>(), Times.Once);
            adapterMock.Verify(m => m.Dispose<DisposableErrorReturningInFinalExit.A>(), Times.Once);
            adapterMock.Verify(m => m.EnterAsync<DisposableErrorReturningInFinalExit.B>(), Times.Once);
            adapterMock.Verify(m => m.ExitAsync<DisposableErrorReturningInFinalExit.B>(), Times.Once);
            adapterMock.Verify(m => m.Dispose<DisposableErrorReturningInFinalExit.B>(), Times.Once);
        }

        /// <summary>
        /// Tests whether states are disposed of correctly for exceptions thrown by a call to
        /// <see cref="ITransition{TState}.TransitAsync"/>.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public static async Task DisposesStatesCorrectlyIfStateThrowsInTransition()
        {
            var machine = new StateMachine();

            var callSequence = new MockSequence();
            var adapterMock = new Mock<IStateVerificationAdapter>();
            adapterMock.InSequence(callSequence).Setup(m => m.EnterAsync<DisposableThrowingInTransition.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.TransitAsync<DisposableThrowingInTransition.A, DisposableThrowingInTransition.B>());
            adapterMock.InSequence(callSequence).Setup(m => m.ExitAsync<DisposableThrowingInTransition.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.Dispose<DisposableThrowingInTransition.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.EnterAsync<DisposableThrowingInTransition.B>());
            adapterMock.InSequence(callSequence).Setup(m => m.ExitAsync<DisposableThrowingInTransition.B>());
            adapterMock.InSequence(callSequence).Setup(m => m.Dispose<DisposableThrowingInTransition.B>());

            var initialState = new DisposableThrowingInTransition.A(adapterMock.Object) { Controller = machine };

            await Assert.ThrowsAsync<CoercedException>(async () => await machine.RunAsync(initialState));

            adapterMock.Verify(m => m.EnterAsync<DisposableThrowingInTransition.A>(), Times.Once);
            adapterMock.Verify(m => m.TransitAsync<DisposableThrowingInTransition.A, DisposableThrowingInTransition.B>(), Times.Once);
            adapterMock.Verify(m => m.ExitAsync<DisposableThrowingInTransition.A>(), Times.Never);
            adapterMock.Verify(m => m.Dispose<DisposableThrowingInTransition.A>(), Times.Once);
            adapterMock.Verify(m => m.EnterAsync<DisposableThrowingInTransition.B>(), Times.Never);
            adapterMock.Verify(m => m.ExitAsync<DisposableThrowingInTransition.B>(), Times.Never);
            adapterMock.Verify(m => m.Dispose<DisposableThrowingInTransition.B>(), Times.Never);
        }

        /// <summary>
        /// Tests whether states are disposed of correctly for errors returned by a call to
        /// <see cref="ITransition{TState}.TransitAsync"/>.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public static async Task DisposesStatesCorrectlyIfStateReturnsErrorInTransition()
        {
            var machine = new StateMachine();

            var callSequence = new MockSequence();
            var adapterMock = new Mock<IStateVerificationAdapter>();
            adapterMock.InSequence(callSequence).Setup(m => m.EnterAsync<DisposableErrorReturningInTransition.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.TransitAsync<DisposableErrorReturningInTransition.A, DisposableErrorReturningInTransition.B>());
            adapterMock.InSequence(callSequence).Setup(m => m.ExitAsync<DisposableErrorReturningInTransition.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.Dispose<DisposableErrorReturningInTransition.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.EnterAsync<DisposableErrorReturningInTransition.B>());
            adapterMock.InSequence(callSequence).Setup(m => m.ExitAsync<DisposableErrorReturningInTransition.B>());
            adapterMock.InSequence(callSequence).Setup(m => m.Dispose<DisposableErrorReturningInTransition.B>());

            var initialState = new DisposableErrorReturningInTransition.A(adapterMock.Object) { Controller = machine };

            var result = await machine.RunAsync(initialState);
            Assert.False(result.IsSuccess);
            Assert.IsType<CoercedError>(result.Error);

            adapterMock.Verify(m => m.EnterAsync<DisposableErrorReturningInTransition.A>(), Times.Once);
            adapterMock.Verify(m => m.TransitAsync<DisposableErrorReturningInTransition.A, DisposableErrorReturningInTransition.B>(), Times.Once);
            adapterMock.Verify(m => m.ExitAsync<DisposableErrorReturningInTransition.A>(), Times.Never);
            adapterMock.Verify(m => m.Dispose<DisposableErrorReturningInTransition.A>(), Times.Once);
            adapterMock.Verify(m => m.EnterAsync<DisposableErrorReturningInTransition.B>(), Times.Never);
            adapterMock.Verify(m => m.ExitAsync<DisposableErrorReturningInTransition.B>(), Times.Never);
            adapterMock.Verify(m => m.Dispose<DisposableErrorReturningInTransition.B>(), Times.Never);
        }
    }

    /// <summary>
    /// Tests the <see cref="StateMachine.RunAsync{TState}(TState,CancellationToken)"/> method's asynchronous disposal
    /// handling.
    /// </summary>
    public static class AsyncDisposal
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
            adapterMock.InSequence(callSequence).Setup(m => m.EnterAsync<AsyncDisposableNonForkingSelfTerminating.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.TransitAsync<AsyncDisposableNonForkingSelfTerminating.A, AsyncDisposableNonForkingSelfTerminating.B>());
            adapterMock.InSequence(callSequence).Setup(m => m.ExitAsync<AsyncDisposableNonForkingSelfTerminating.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.DisposeAsync<AsyncDisposableNonForkingSelfTerminating.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.EnterAsync<AsyncDisposableNonForkingSelfTerminating.B>());
            adapterMock.InSequence(callSequence).Setup(m => m.ExitAsync<AsyncDisposableNonForkingSelfTerminating.B>());
            adapterMock.InSequence(callSequence).Setup(m => m.DisposeAsync<AsyncDisposableNonForkingSelfTerminating.B>());

            var initialState = new AsyncDisposableNonForkingSelfTerminating.A(adapterMock.Object) { Controller = machine };

            var result = await machine.RunAsync(initialState);
            Assert.True(result.IsSuccess);

            adapterMock.Verify(m => m.EnterAsync<AsyncDisposableNonForkingSelfTerminating.A>(), Times.Once);
            adapterMock.Verify(m => m.TransitAsync<AsyncDisposableNonForkingSelfTerminating.A, AsyncDisposableNonForkingSelfTerminating.B>(), Times.Once);
            adapterMock.Verify(m => m.ExitAsync<AsyncDisposableNonForkingSelfTerminating.A>(), Times.Once);
            adapterMock.Verify(m => m.DisposeAsync<AsyncDisposableNonForkingSelfTerminating.A>(), Times.Once);
            adapterMock.Verify(m => m.EnterAsync<AsyncDisposableNonForkingSelfTerminating.B>(), Times.Once);
            adapterMock.Verify(m => m.ExitAsync<AsyncDisposableNonForkingSelfTerminating.B>(), Times.Once);
            adapterMock.Verify(m => m.DisposeAsync<AsyncDisposableNonForkingSelfTerminating.B>(), Times.Once);
        }

        /// <summary>
        /// Tests whether states are disposed of correctly in the ideal happy-path cases.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public static async Task DisposesStatesCorrectlyForNonForkingNonTerminatingGraph()
        {
            var machine = new StateMachine();

            var callSequence = new MockSequence();
            var adapterMock = new Mock<IStateVerificationAdapter>();
            adapterMock.InSequence(callSequence).Setup(m => m.EnterAsync<AsyncDisposableNonForkingNonTerminating.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.TransitAsync<AsyncDisposableNonForkingNonTerminating.A, AsyncDisposableNonForkingNonTerminating.B>());
            adapterMock.InSequence(callSequence).Setup(m => m.ExitAsync<AsyncDisposableNonForkingNonTerminating.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.DisposeAsync<AsyncDisposableNonForkingNonTerminating.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.EnterAsync<AsyncDisposableNonForkingNonTerminating.B>());
            adapterMock.InSequence(callSequence).Setup(m => m.ExitAsync<AsyncDisposableNonForkingNonTerminating.B>());
            adapterMock.InSequence(callSequence).Setup(m => m.DisposeAsync<AsyncDisposableNonForkingNonTerminating.B>());

            var tokenSource = new CancellationTokenSource();
            tokenSource.Cancel();

            var initialState = new AsyncDisposableNonForkingNonTerminating.A(adapterMock.Object, tokenSource.Token) { Controller = machine };

            var result = await machine.RunAsync(initialState, CancellationToken.None);
            Assert.True(result.IsSuccess);

            adapterMock.Verify(m => m.EnterAsync<AsyncDisposableNonForkingNonTerminating.A>(), Times.Once);
            adapterMock.Verify(m => m.TransitAsync<AsyncDisposableNonForkingNonTerminating.A, AsyncDisposableNonForkingNonTerminating.B>(), Times.Once);
            adapterMock.Verify(m => m.ExitAsync<AsyncDisposableNonForkingNonTerminating.A>(), Times.Once);
            adapterMock.Verify(m => m.DisposeAsync<AsyncDisposableNonForkingNonTerminating.A>(), Times.Once);
            adapterMock.Verify(m => m.EnterAsync<AsyncDisposableNonForkingNonTerminating.B>(), Times.Once);
            adapterMock.Verify(m => m.ExitAsync<AsyncDisposableNonForkingNonTerminating.B>(), Times.Once);
            adapterMock.Verify(m => m.DisposeAsync<AsyncDisposableNonForkingNonTerminating.B>(), Times.Once);
        }

        /// <summary>
        /// Tests whether states are disposed of correctly in the ideal happy-path cases.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public static async Task DisposesStatesCorrectlyForForkingSelfTerminatingGraph()
        {
            var machine = new StateMachine();

            var callSequence = new MockSequence();
            var adapterMock = new Mock<IStateVerificationAdapter>();
            adapterMock.InSequence(callSequence).Setup(m => m.EnterAsync<AsyncDisposableForkingSelfTerminating.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.TransitAsync<AsyncDisposableForkingSelfTerminating.A, AsyncDisposableForkingSelfTerminating.B>());
            adapterMock.InSequence(callSequence).Setup(m => m.ExitAsync<AsyncDisposableForkingSelfTerminating.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.DisposeAsync<AsyncDisposableForkingSelfTerminating.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.EnterAsync<AsyncDisposableForkingSelfTerminating.B>());
            adapterMock.InSequence(callSequence).Setup(m => m.ExitAsync<AsyncDisposableForkingSelfTerminating.B>());
            adapterMock.InSequence(callSequence).Setup(m => m.DisposeAsync<AsyncDisposableForkingSelfTerminating.B>());

            var initialState = new AsyncDisposableForkingSelfTerminating.A(adapterMock.Object) { Controller = machine };

            var result = await machine.RunAsync(initialState);
            Assert.True(result.IsSuccess);

            adapterMock.Verify(m => m.EnterAsync<AsyncDisposableForkingSelfTerminating.A>(), Times.Once);
            adapterMock.Verify(m => m.TransitAsync<AsyncDisposableForkingSelfTerminating.A, AsyncDisposableForkingSelfTerminating.B>(), Times.Once);
            adapterMock.Verify(m => m.ExitAsync<AsyncDisposableForkingSelfTerminating.A>(), Times.Once);
            adapterMock.Verify(m => m.DisposeAsync<AsyncDisposableForkingSelfTerminating.A>(), Times.Once);
            adapterMock.Verify(m => m.EnterAsync<AsyncDisposableForkingSelfTerminating.B>(), Times.Once);
            adapterMock.Verify(m => m.ExitAsync<AsyncDisposableForkingSelfTerminating.B>(), Times.Once);
            adapterMock.Verify(m => m.DisposeAsync<AsyncDisposableForkingSelfTerminating.B>(), Times.Once);
        }

        /// <summary>
        /// Tests whether states are disposed of correctly in the ideal happy-path cases.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public static async Task DisposesStatesCorrectlyForForkingNonTerminatingGraph()
        {
            var machine = new StateMachine();

            var callSequence = new MockSequence();
            var adapterMock = new Mock<IStateVerificationAdapter>();
            adapterMock.InSequence(callSequence).Setup(m => m.EnterAsync<AsyncDisposableForkingNonTerminating.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.TransitAsync<AsyncDisposableForkingNonTerminating.A, AsyncDisposableForkingNonTerminating.B>());
            adapterMock.InSequence(callSequence).Setup(m => m.ExitAsync<AsyncDisposableForkingNonTerminating.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.DisposeAsync<AsyncDisposableForkingNonTerminating.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.EnterAsync<AsyncDisposableForkingNonTerminating.B>());
            adapterMock.InSequence(callSequence).Setup(m => m.ExitAsync<AsyncDisposableForkingNonTerminating.B>());
            adapterMock.InSequence(callSequence).Setup(m => m.DisposeAsync<AsyncDisposableForkingNonTerminating.B>());

            var tokenSource = new CancellationTokenSource();
            tokenSource.Cancel();

            var initialState = new AsyncDisposableForkingNonTerminating.A(adapterMock.Object, tokenSource.Token) { Controller = machine };

            var result = await machine.RunAsync(initialState, CancellationToken.None);
            Assert.True(result.IsSuccess);

            adapterMock.Verify(m => m.EnterAsync<AsyncDisposableForkingNonTerminating.A>(), Times.Once);
            adapterMock.Verify(m => m.TransitAsync<AsyncDisposableForkingNonTerminating.A, AsyncDisposableForkingNonTerminating.B>(), Times.Once);
            adapterMock.Verify(m => m.ExitAsync<AsyncDisposableForkingNonTerminating.A>(), Times.Once);
            adapterMock.Verify(m => m.DisposeAsync<AsyncDisposableForkingNonTerminating.A>(), Times.Once);
            adapterMock.Verify(m => m.EnterAsync<AsyncDisposableForkingNonTerminating.B>(), Times.Once);
            adapterMock.Verify(m => m.ExitAsync<AsyncDisposableForkingNonTerminating.B>(), Times.Once);
            adapterMock.Verify(m => m.DisposeAsync<AsyncDisposableForkingNonTerminating.B>(), Times.Once);
        }

        /// <summary>
        /// Tests whether states are disposed of correctly for exceptions thrown by a call to
        /// <see cref="IState.EnterAsync"/> of the initial state.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public static async Task DisposesStatesCorrectlyIfInitialStateThrowsInEnter()
        {
            var machine = new StateMachine();

            var callSequence = new MockSequence();
            var adapterMock = new Mock<IStateVerificationAdapter>();
            adapterMock.InSequence(callSequence).Setup(m => m.EnterAsync<AsyncDisposableThrowingInInitialEnter.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.DisposeAsync<AsyncDisposableThrowingInInitialEnter.A>());

            var initialState = new AsyncDisposableThrowingInInitialEnter.A(adapterMock.Object) { Controller = machine };

            await Assert.ThrowsAsync<CoercedException>(async () => await machine.RunAsync(initialState));

            adapterMock.Verify(m => m.EnterAsync<AsyncDisposableThrowingInInitialEnter.A>(), Times.Once);
            adapterMock.Verify(m => m.TransitAsync<AsyncDisposableThrowingInInitialEnter.A, AsyncDisposableThrowingInInitialEnter.B>(), Times.Never);
            adapterMock.Verify(m => m.ExitAsync<AsyncDisposableThrowingInInitialEnter.A>(), Times.Never);
            adapterMock.Verify(m => m.DisposeAsync<AsyncDisposableThrowingInInitialEnter.A>(), Times.Once);
            adapterMock.Verify(m => m.EnterAsync<AsyncDisposableThrowingInInitialEnter.B>(), Times.Never);
            adapterMock.Verify(m => m.ExitAsync<AsyncDisposableThrowingInInitialEnter.B>(), Times.Never);
            adapterMock.Verify(m => m.DisposeAsync<AsyncDisposableThrowingInInitialEnter.B>(), Times.Never);
        }

        /// <summary>
        /// Tests whether states are disposed of correctly for errors returned by a call to
        /// <see cref="IState.EnterAsync"/> of the initial state.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public static async Task DisposesStatesCorrectlyIfInitialStateReturnsErrorFromEnter()
        {
            var machine = new StateMachine();

            var callSequence = new MockSequence();
            var adapterMock = new Mock<IStateVerificationAdapter>();
            adapterMock.InSequence(callSequence).Setup(m => m.EnterAsync<AsyncDisposableErrorReturningInInitialEnter.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.DisposeAsync<AsyncDisposableErrorReturningInInitialEnter.A>());

            var initialState = new AsyncDisposableErrorReturningInInitialEnter.A(adapterMock.Object) { Controller = machine };

            var result = await machine.RunAsync(initialState);
            Assert.False(result.IsSuccess);
            Assert.IsType<CoercedError>(result.Error);

            adapterMock.Verify(m => m.EnterAsync<AsyncDisposableErrorReturningInInitialEnter.A>(), Times.Once);
            adapterMock.Verify(m => m.TransitAsync<AsyncDisposableErrorReturningInInitialEnter.A, AsyncDisposableErrorReturningInInitialEnter.B>(), Times.Never);
            adapterMock.Verify(m => m.ExitAsync<AsyncDisposableErrorReturningInInitialEnter.A>(), Times.Never);
            adapterMock.Verify(m => m.DisposeAsync<AsyncDisposableErrorReturningInInitialEnter.A>(), Times.Once);
            adapterMock.Verify(m => m.EnterAsync<AsyncDisposableErrorReturningInInitialEnter.B>(), Times.Never);
            adapterMock.Verify(m => m.ExitAsync<AsyncDisposableErrorReturningInInitialEnter.B>(), Times.Never);
            adapterMock.Verify(m => m.DisposeAsync<AsyncDisposableErrorReturningInInitialEnter.B>(), Times.Never);
        }

        /// <summary>
        /// Tests whether states are disposed of correctly for exceptions thrown by a call to
        /// <see cref="IState.EnterAsync"/>.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public static async Task DisposesStatesCorrectlyIfStateThrowsInEnter()
        {
            var machine = new StateMachine();

            var callSequence = new MockSequence();
            var adapterMock = new Mock<IStateVerificationAdapter>();
            adapterMock.InSequence(callSequence).Setup(m => m.EnterAsync<AsyncDisposableThrowingInEnter.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.TransitAsync<AsyncDisposableThrowingInEnter.A, AsyncDisposableThrowingInEnter.B>());
            adapterMock.InSequence(callSequence).Setup(m => m.ExitAsync<AsyncDisposableThrowingInEnter.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.DisposeAsync<AsyncDisposableThrowingInEnter.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.EnterAsync<AsyncDisposableThrowingInEnter.B>());
            adapterMock.InSequence(callSequence).Setup(m => m.DisposeAsync<AsyncDisposableThrowingInEnter.B>());

            var initialState = new AsyncDisposableThrowingInEnter.A(adapterMock.Object) { Controller = machine };

            await Assert.ThrowsAsync<CoercedException>(async () => await machine.RunAsync(initialState));

            adapterMock.Verify(m => m.EnterAsync<AsyncDisposableThrowingInEnter.A>(), Times.Once);
            adapterMock.Verify(m => m.TransitAsync<AsyncDisposableThrowingInEnter.A, AsyncDisposableThrowingInEnter.B>(), Times.Once);
            adapterMock.Verify(m => m.ExitAsync<AsyncDisposableThrowingInEnter.A>(), Times.Once);
            adapterMock.Verify(m => m.DisposeAsync<AsyncDisposableThrowingInEnter.A>(), Times.Once);
            adapterMock.Verify(m => m.EnterAsync<AsyncDisposableThrowingInEnter.B>(), Times.Once);
            adapterMock.Verify(m => m.ExitAsync<AsyncDisposableThrowingInEnter.B>(), Times.Never);
            adapterMock.Verify(m => m.DisposeAsync<AsyncDisposableThrowingInEnter.B>(), Times.Once);
        }

        /// <summary>
        /// Tests whether states are disposed of correctly for errors returned by a call to
        /// <see cref="IState.EnterAsync"/>.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public static async Task DisposesStatesCorrectlyIfStateReturnsErrorFromEnter()
        {
            var machine = new StateMachine();

            var callSequence = new MockSequence();
            var adapterMock = new Mock<IStateVerificationAdapter>();
            adapterMock.InSequence(callSequence).Setup(m => m.EnterAsync<AsyncDisposableErrorReturningInEnter.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.TransitAsync<AsyncDisposableErrorReturningInEnter.A, AsyncDisposableErrorReturningInEnter.B>());
            adapterMock.InSequence(callSequence).Setup(m => m.ExitAsync<AsyncDisposableErrorReturningInEnter.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.DisposeAsync<AsyncDisposableErrorReturningInEnter.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.EnterAsync<AsyncDisposableErrorReturningInEnter.B>());
            adapterMock.InSequence(callSequence).Setup(m => m.DisposeAsync<AsyncDisposableErrorReturningInEnter.B>());

            var initialState = new AsyncDisposableErrorReturningInEnter.A(adapterMock.Object) { Controller = machine };

            var result = await machine.RunAsync(initialState);
            Assert.False(result.IsSuccess);
            Assert.IsType<CoercedError>(result.Error);

            adapterMock.Verify(m => m.EnterAsync<AsyncDisposableErrorReturningInEnter.A>(), Times.Once);
            adapterMock.Verify(m => m.TransitAsync<AsyncDisposableErrorReturningInEnter.A, AsyncDisposableErrorReturningInEnter.B>(), Times.Once);
            adapterMock.Verify(m => m.ExitAsync<AsyncDisposableErrorReturningInEnter.A>(), Times.Once);
            adapterMock.Verify(m => m.DisposeAsync<AsyncDisposableErrorReturningInEnter.A>(), Times.Once);
            adapterMock.Verify(m => m.EnterAsync<AsyncDisposableErrorReturningInEnter.B>(), Times.Once);
            adapterMock.Verify(m => m.ExitAsync<AsyncDisposableErrorReturningInEnter.B>(), Times.Never);
            adapterMock.Verify(m => m.DisposeAsync<AsyncDisposableErrorReturningInEnter.B>(), Times.Once);
        }

        /// <summary>
        /// Tests whether states are disposed of correctly for exceptions thrown by a call to
        /// <see cref="IState.ExitAsync"/>.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public static async Task DisposesStatesCorrectlyIfStateThrowsInExit()
        {
            var machine = new StateMachine();

            var callSequence = new MockSequence();
            var adapterMock = new Mock<IStateVerificationAdapter>();
            adapterMock.InSequence(callSequence).Setup(m => m.EnterAsync<AsyncDisposableThrowingInExit.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.TransitAsync<AsyncDisposableThrowingInExit.A, AsyncDisposableThrowingInExit.B>());
            adapterMock.InSequence(callSequence).Setup(m => m.ExitAsync<AsyncDisposableThrowingInExit.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.DisposeAsync<AsyncDisposableThrowingInExit.A>());

            var initialState = new AsyncDisposableThrowingInExit.A(adapterMock.Object) { Controller = machine };

            await Assert.ThrowsAsync<CoercedException>(async () => await machine.RunAsync(initialState));

            adapterMock.Verify(m => m.EnterAsync<AsyncDisposableThrowingInExit.A>(), Times.Once);
            adapterMock.Verify(m => m.TransitAsync<AsyncDisposableThrowingInExit.A, AsyncDisposableThrowingInExit.B>(), Times.Once);
            adapterMock.Verify(m => m.ExitAsync<AsyncDisposableThrowingInExit.A>(), Times.Once);
            adapterMock.Verify(m => m.DisposeAsync<AsyncDisposableThrowingInExit.A>(), Times.Once);
            adapterMock.Verify(m => m.EnterAsync<AsyncDisposableThrowingInExit.B>(), Times.Never);
            adapterMock.Verify(m => m.ExitAsync<AsyncDisposableThrowingInExit.B>(), Times.Never);
            adapterMock.Verify(m => m.DisposeAsync<AsyncDisposableThrowingInExit.B>(), Times.Once);
        }

        /// <summary>
        /// Tests whether states are disposed of correctly for errors returned by a call to
        /// <see cref="IState.ExitAsync"/>.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public static async Task DisposesStatesCorrectlyIfStateReturnsErrorExit()
        {
            var machine = new StateMachine();

            var callSequence = new MockSequence();
            var adapterMock = new Mock<IStateVerificationAdapter>();
            adapterMock.InSequence(callSequence).Setup(m => m.EnterAsync<AsyncDisposableErrorReturningInExit.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.TransitAsync<AsyncDisposableErrorReturningInExit.A, AsyncDisposableErrorReturningInExit.B>());
            adapterMock.InSequence(callSequence).Setup(m => m.ExitAsync<AsyncDisposableErrorReturningInExit.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.DisposeAsync<AsyncDisposableErrorReturningInExit.A>());

            var initialState = new AsyncDisposableErrorReturningInExit.A(adapterMock.Object) { Controller = machine };

            var result = await machine.RunAsync(initialState);
            Assert.False(result.IsSuccess);
            Assert.IsType<CoercedError>(result.Error);

            adapterMock.Verify(m => m.EnterAsync<AsyncDisposableErrorReturningInExit.A>(), Times.Once);
            adapterMock.Verify(m => m.TransitAsync<AsyncDisposableErrorReturningInExit.A, AsyncDisposableErrorReturningInExit.B>(), Times.Once);
            adapterMock.Verify(m => m.ExitAsync<AsyncDisposableErrorReturningInExit.A>(), Times.Once);
            adapterMock.Verify(m => m.DisposeAsync<AsyncDisposableErrorReturningInExit.A>(), Times.Once);
            adapterMock.Verify(m => m.EnterAsync<AsyncDisposableErrorReturningInExit.B>(), Times.Never);
            adapterMock.Verify(m => m.ExitAsync<AsyncDisposableErrorReturningInExit.B>(), Times.Never);
            adapterMock.Verify(m => m.DisposeAsync<AsyncDisposableErrorReturningInExit.B>(), Times.Once);
        }

        /// <summary>
        /// Tests whether states are disposed of correctly for exceptions thrown by a call to
        /// <see cref="IState.ExitAsync"/> of the final state.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public static async Task DisposesStatesCorrectlyIfStateThrowsInFinalExit()
        {
            var machine = new StateMachine();

            var callSequence = new MockSequence();
            var adapterMock = new Mock<IStateVerificationAdapter>();
            adapterMock.InSequence(callSequence).Setup(m => m.EnterAsync<AsyncDisposableThrowingInFinalExit.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.TransitAsync<AsyncDisposableThrowingInFinalExit.A, AsyncDisposableThrowingInFinalExit.B>());
            adapterMock.InSequence(callSequence).Setup(m => m.ExitAsync<AsyncDisposableThrowingInFinalExit.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.DisposeAsync<AsyncDisposableThrowingInFinalExit.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.EnterAsync<AsyncDisposableThrowingInFinalExit.B>());
            adapterMock.InSequence(callSequence).Setup(m => m.ExitAsync<AsyncDisposableThrowingInFinalExit.B>());
            adapterMock.InSequence(callSequence).Setup(m => m.DisposeAsync<AsyncDisposableThrowingInFinalExit.B>());

            var initialState = new AsyncDisposableThrowingInFinalExit.A(adapterMock.Object) { Controller = machine };

            await Assert.ThrowsAsync<CoercedException>(async () => await machine.RunAsync(initialState));

            adapterMock.Verify(m => m.EnterAsync<AsyncDisposableThrowingInFinalExit.A>(), Times.Once);
            adapterMock.Verify(m => m.TransitAsync<AsyncDisposableThrowingInFinalExit.A, AsyncDisposableThrowingInFinalExit.B>(), Times.Once);
            adapterMock.Verify(m => m.ExitAsync<AsyncDisposableThrowingInFinalExit.A>(), Times.Once);
            adapterMock.Verify(m => m.DisposeAsync<AsyncDisposableThrowingInFinalExit.A>(), Times.Once);
            adapterMock.Verify(m => m.EnterAsync<AsyncDisposableThrowingInFinalExit.B>(), Times.Once);
            adapterMock.Verify(m => m.ExitAsync<AsyncDisposableThrowingInFinalExit.B>(), Times.Once);
            adapterMock.Verify(m => m.DisposeAsync<AsyncDisposableThrowingInFinalExit.B>(), Times.Once);
        }

        /// <summary>
        /// Tests whether states are disposed of correctly for errors returned by a call to
        /// <see cref="IState.ExitAsync"/> of the final state.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public static async Task DisposesStatesCorrectlyIfStateReturnsErrorInFinalExit()
        {
            var machine = new StateMachine();

            var callSequence = new MockSequence();
            var adapterMock = new Mock<IStateVerificationAdapter>();
            adapterMock.InSequence(callSequence).Setup(m => m.EnterAsync<AsyncDisposableErrorReturningInFinalExit.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.TransitAsync<AsyncDisposableErrorReturningInFinalExit.A, AsyncDisposableErrorReturningInFinalExit.B>());
            adapterMock.InSequence(callSequence).Setup(m => m.ExitAsync<AsyncDisposableErrorReturningInFinalExit.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.DisposeAsync<AsyncDisposableErrorReturningInFinalExit.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.EnterAsync<AsyncDisposableErrorReturningInFinalExit.B>());
            adapterMock.InSequence(callSequence).Setup(m => m.ExitAsync<AsyncDisposableErrorReturningInFinalExit.B>());
            adapterMock.InSequence(callSequence).Setup(m => m.DisposeAsync<AsyncDisposableErrorReturningInFinalExit.B>());

            var initialState = new AsyncDisposableErrorReturningInFinalExit.A(adapterMock.Object) { Controller = machine };

            var result = await machine.RunAsync(initialState);
            Assert.False(result.IsSuccess);
            Assert.IsType<CoercedError>(result.Error);

            adapterMock.Verify(m => m.EnterAsync<AsyncDisposableErrorReturningInFinalExit.A>(), Times.Once);
            adapterMock.Verify(m => m.TransitAsync<AsyncDisposableErrorReturningInFinalExit.A, AsyncDisposableErrorReturningInFinalExit.B>(), Times.Once);
            adapterMock.Verify(m => m.ExitAsync<AsyncDisposableErrorReturningInFinalExit.A>(), Times.Once);
            adapterMock.Verify(m => m.DisposeAsync<AsyncDisposableErrorReturningInFinalExit.A>(), Times.Once);
            adapterMock.Verify(m => m.EnterAsync<AsyncDisposableErrorReturningInFinalExit.B>(), Times.Once);
            adapterMock.Verify(m => m.ExitAsync<AsyncDisposableErrorReturningInFinalExit.B>(), Times.Once);
            adapterMock.Verify(m => m.DisposeAsync<AsyncDisposableErrorReturningInFinalExit.B>(), Times.Once);
        }

        /// <summary>
        /// Tests whether states are disposed of correctly for exceptions thrown by a call to
        /// <see cref="ITransition{TState}.TransitAsync"/>.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public static async Task DisposesStatesCorrectlyIfStateThrowsInTransition()
        {
            var machine = new StateMachine();

            var callSequence = new MockSequence();
            var adapterMock = new Mock<IStateVerificationAdapter>();
            adapterMock.InSequence(callSequence).Setup(m => m.EnterAsync<AsyncDisposableThrowingInTransition.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.TransitAsync<AsyncDisposableThrowingInTransition.A, AsyncDisposableThrowingInTransition.B>());
            adapterMock.InSequence(callSequence).Setup(m => m.ExitAsync<AsyncDisposableThrowingInTransition.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.DisposeAsync<AsyncDisposableThrowingInTransition.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.EnterAsync<AsyncDisposableThrowingInTransition.B>());
            adapterMock.InSequence(callSequence).Setup(m => m.ExitAsync<AsyncDisposableThrowingInTransition.B>());
            adapterMock.InSequence(callSequence).Setup(m => m.DisposeAsync<AsyncDisposableThrowingInTransition.B>());

            var initialState = new AsyncDisposableThrowingInTransition.A(adapterMock.Object) { Controller = machine };

            await Assert.ThrowsAsync<CoercedException>(async () => await machine.RunAsync(initialState));

            adapterMock.Verify(m => m.EnterAsync<AsyncDisposableThrowingInTransition.A>(), Times.Once);
            adapterMock.Verify(m => m.TransitAsync<AsyncDisposableThrowingInTransition.A, AsyncDisposableThrowingInTransition.B>(), Times.Once);
            adapterMock.Verify(m => m.ExitAsync<AsyncDisposableThrowingInTransition.A>(), Times.Never);
            adapterMock.Verify(m => m.DisposeAsync<AsyncDisposableThrowingInTransition.A>(), Times.Once);
            adapterMock.Verify(m => m.EnterAsync<AsyncDisposableThrowingInTransition.B>(), Times.Never);
            adapterMock.Verify(m => m.ExitAsync<AsyncDisposableThrowingInTransition.B>(), Times.Never);
            adapterMock.Verify(m => m.DisposeAsync<AsyncDisposableThrowingInTransition.B>(), Times.Never);
        }

        /// <summary>
        /// Tests whether states are disposed of correctly for errors returned by a call to
        /// <see cref="ITransition{TState}.TransitAsync"/>.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public static async Task DisposesStatesCorrectlyIfStateReturnsErrorInTransition()
        {
            var machine = new StateMachine();

            var callSequence = new MockSequence();
            var adapterMock = new Mock<IStateVerificationAdapter>();
            adapterMock.InSequence(callSequence).Setup(m => m.EnterAsync<AsyncDisposableErrorReturningInTransition.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.TransitAsync<AsyncDisposableErrorReturningInTransition.A, AsyncDisposableErrorReturningInTransition.B>());
            adapterMock.InSequence(callSequence).Setup(m => m.ExitAsync<AsyncDisposableErrorReturningInTransition.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.DisposeAsync<AsyncDisposableErrorReturningInTransition.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.EnterAsync<AsyncDisposableErrorReturningInTransition.B>());
            adapterMock.InSequence(callSequence).Setup(m => m.ExitAsync<AsyncDisposableErrorReturningInTransition.B>());
            adapterMock.InSequence(callSequence).Setup(m => m.DisposeAsync<AsyncDisposableErrorReturningInTransition.B>());

            var initialState = new AsyncDisposableErrorReturningInTransition.A(adapterMock.Object) { Controller = machine };

            var result = await machine.RunAsync(initialState);
            Assert.False(result.IsSuccess);
            Assert.IsType<CoercedError>(result.Error);

            adapterMock.Verify(m => m.EnterAsync<AsyncDisposableErrorReturningInTransition.A>(), Times.Once);
            adapterMock.Verify(m => m.TransitAsync<AsyncDisposableErrorReturningInTransition.A, AsyncDisposableErrorReturningInTransition.B>(), Times.Once);
            adapterMock.Verify(m => m.ExitAsync<AsyncDisposableErrorReturningInTransition.A>(), Times.Never);
            adapterMock.Verify(m => m.DisposeAsync<AsyncDisposableErrorReturningInTransition.A>(), Times.Once);
            adapterMock.Verify(m => m.EnterAsync<AsyncDisposableErrorReturningInTransition.B>(), Times.Never);
            adapterMock.Verify(m => m.ExitAsync<AsyncDisposableErrorReturningInTransition.B>(), Times.Never);
            adapterMock.Verify(m => m.DisposeAsync<AsyncDisposableErrorReturningInTransition.B>(), Times.Never);
        }
    }

    /// <summary>
    /// Tests the <see cref="StateMachine.RunAsync{TState}(TState,CancellationToken)"/> method's combined disposal and
    /// asynchronous disposal handling.
    /// </summary>
    public static class DisposalAndAsyncDisposal
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
            adapterMock.InSequence(callSequence).Setup(m => m.EnterAsync<DisposableAndAsyncDisposableNonForkingSelfTerminating.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.TransitAsync<DisposableAndAsyncDisposableNonForkingSelfTerminating.A, DisposableAndAsyncDisposableNonForkingSelfTerminating.B>());
            adapterMock.InSequence(callSequence).Setup(m => m.ExitAsync<DisposableAndAsyncDisposableNonForkingSelfTerminating.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.DisposeAsync<DisposableAndAsyncDisposableNonForkingSelfTerminating.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.Dispose<DisposableAndAsyncDisposableNonForkingSelfTerminating.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.EnterAsync<DisposableAndAsyncDisposableNonForkingSelfTerminating.B>());
            adapterMock.InSequence(callSequence).Setup(m => m.ExitAsync<DisposableAndAsyncDisposableNonForkingSelfTerminating.B>());
            adapterMock.InSequence(callSequence).Setup(m => m.DisposeAsync<DisposableAndAsyncDisposableNonForkingSelfTerminating.B>());
            adapterMock.InSequence(callSequence).Setup(m => m.Dispose<DisposableAndAsyncDisposableNonForkingSelfTerminating.B>());

            var initialState = new DisposableAndAsyncDisposableNonForkingSelfTerminating.A(adapterMock.Object) { Controller = machine };

            var result = await machine.RunAsync(initialState);
            Assert.True(result.IsSuccess);

            adapterMock.Verify(m => m.EnterAsync<DisposableAndAsyncDisposableNonForkingSelfTerminating.A>(), Times.Once);
            adapterMock.Verify(m => m.TransitAsync<DisposableAndAsyncDisposableNonForkingSelfTerminating.A, DisposableAndAsyncDisposableNonForkingSelfTerminating.B>(), Times.Once);
            adapterMock.Verify(m => m.ExitAsync<DisposableAndAsyncDisposableNonForkingSelfTerminating.A>(), Times.Once);
            adapterMock.Verify(m => m.DisposeAsync<DisposableAndAsyncDisposableNonForkingSelfTerminating.A>(), Times.Once);
            adapterMock.Verify(m => m.Dispose<DisposableAndAsyncDisposableNonForkingSelfTerminating.A>(), Times.Once);
            adapterMock.Verify(m => m.EnterAsync<DisposableAndAsyncDisposableNonForkingSelfTerminating.B>(), Times.Once);
            adapterMock.Verify(m => m.ExitAsync<DisposableAndAsyncDisposableNonForkingSelfTerminating.B>(), Times.Once);
            adapterMock.Verify(m => m.DisposeAsync<DisposableAndAsyncDisposableNonForkingSelfTerminating.B>(), Times.Once);
            adapterMock.Verify(m => m.Dispose<DisposableAndAsyncDisposableNonForkingSelfTerminating.B>(), Times.Once);
        }

        /// <summary>
        /// Tests whether states are disposed of correctly in the ideal happy-path cases.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public static async Task DisposesStatesCorrectlyForNonForkingNonTerminatingGraph()
        {
            var machine = new StateMachine();

            var callSequence = new MockSequence();
            var adapterMock = new Mock<IStateVerificationAdapter>();
            adapterMock.InSequence(callSequence).Setup(m => m.EnterAsync<DisposableAndAsyncDisposableNonForkingNonTerminating.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.TransitAsync<DisposableAndAsyncDisposableNonForkingNonTerminating.A, DisposableAndAsyncDisposableNonForkingNonTerminating.B>());
            adapterMock.InSequence(callSequence).Setup(m => m.ExitAsync<DisposableAndAsyncDisposableNonForkingNonTerminating.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.DisposeAsync<DisposableAndAsyncDisposableNonForkingNonTerminating.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.Dispose<DisposableAndAsyncDisposableNonForkingNonTerminating.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.EnterAsync<DisposableAndAsyncDisposableNonForkingNonTerminating.B>());
            adapterMock.InSequence(callSequence).Setup(m => m.ExitAsync<DisposableAndAsyncDisposableNonForkingNonTerminating.B>());
            adapterMock.InSequence(callSequence).Setup(m => m.DisposeAsync<DisposableAndAsyncDisposableNonForkingNonTerminating.B>());
            adapterMock.InSequence(callSequence).Setup(m => m.Dispose<DisposableAndAsyncDisposableNonForkingNonTerminating.B>());

            var tokenSource = new CancellationTokenSource();
            tokenSource.Cancel();

            var initialState = new DisposableAndAsyncDisposableNonForkingNonTerminating.A(adapterMock.Object, tokenSource.Token) { Controller = machine };

            var result = await machine.RunAsync(initialState, CancellationToken.None);
            Assert.True(result.IsSuccess);

            adapterMock.Verify(m => m.EnterAsync<DisposableAndAsyncDisposableNonForkingNonTerminating.A>(), Times.Once);
            adapterMock.Verify(m => m.TransitAsync<DisposableAndAsyncDisposableNonForkingNonTerminating.A, DisposableAndAsyncDisposableNonForkingNonTerminating.B>(), Times.Once);
            adapterMock.Verify(m => m.ExitAsync<DisposableAndAsyncDisposableNonForkingNonTerminating.A>(), Times.Once);
            adapterMock.Verify(m => m.DisposeAsync<DisposableAndAsyncDisposableNonForkingNonTerminating.A>(), Times.Once);
            adapterMock.Verify(m => m.Dispose<DisposableAndAsyncDisposableNonForkingNonTerminating.A>(), Times.Once);
            adapterMock.Verify(m => m.EnterAsync<DisposableAndAsyncDisposableNonForkingNonTerminating.B>(), Times.Once);
            adapterMock.Verify(m => m.ExitAsync<DisposableAndAsyncDisposableNonForkingNonTerminating.B>(), Times.Once);
            adapterMock.Verify(m => m.DisposeAsync<DisposableAndAsyncDisposableNonForkingNonTerminating.B>(), Times.Once);
            adapterMock.Verify(m => m.Dispose<DisposableAndAsyncDisposableNonForkingNonTerminating.B>(), Times.Once);
        }

        /// <summary>
        /// Tests whether states are disposed of correctly in the ideal happy-path cases.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public static async Task DisposesStatesCorrectlyForForkingSelfTerminatingGraph()
        {
            var machine = new StateMachine();

            var callSequence = new MockSequence();
            var adapterMock = new Mock<IStateVerificationAdapter>();
            adapterMock.InSequence(callSequence).Setup(m => m.EnterAsync<DisposableAndAsyncDisposableForkingSelfTerminating.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.TransitAsync<DisposableAndAsyncDisposableForkingSelfTerminating.A, DisposableAndAsyncDisposableForkingSelfTerminating.B>());
            adapterMock.InSequence(callSequence).Setup(m => m.ExitAsync<DisposableAndAsyncDisposableForkingSelfTerminating.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.DisposeAsync<DisposableAndAsyncDisposableForkingSelfTerminating.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.Dispose<DisposableAndAsyncDisposableForkingSelfTerminating.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.EnterAsync<DisposableAndAsyncDisposableForkingSelfTerminating.B>());
            adapterMock.InSequence(callSequence).Setup(m => m.ExitAsync<DisposableAndAsyncDisposableForkingSelfTerminating.B>());
            adapterMock.InSequence(callSequence).Setup(m => m.DisposeAsync<DisposableAndAsyncDisposableForkingSelfTerminating.B>());
            adapterMock.InSequence(callSequence).Setup(m => m.Dispose<DisposableAndAsyncDisposableForkingSelfTerminating.B>());

            var initialState = new DisposableAndAsyncDisposableForkingSelfTerminating.A(adapterMock.Object) { Controller = machine };

            var result = await machine.RunAsync(initialState);
            Assert.True(result.IsSuccess);

            adapterMock.Verify(m => m.EnterAsync<DisposableAndAsyncDisposableForkingSelfTerminating.A>(), Times.Once);
            adapterMock.Verify(m => m.TransitAsync<DisposableAndAsyncDisposableForkingSelfTerminating.A, DisposableAndAsyncDisposableForkingSelfTerminating.B>(), Times.Once);
            adapterMock.Verify(m => m.ExitAsync<DisposableAndAsyncDisposableForkingSelfTerminating.A>(), Times.Once);
            adapterMock.Verify(m => m.DisposeAsync<DisposableAndAsyncDisposableForkingSelfTerminating.A>(), Times.Once);
            adapterMock.Verify(m => m.Dispose<DisposableAndAsyncDisposableForkingSelfTerminating.A>(), Times.Once);
            adapterMock.Verify(m => m.EnterAsync<DisposableAndAsyncDisposableForkingSelfTerminating.B>(), Times.Once);
            adapterMock.Verify(m => m.ExitAsync<DisposableAndAsyncDisposableForkingSelfTerminating.B>(), Times.Once);
            adapterMock.Verify(m => m.DisposeAsync<DisposableAndAsyncDisposableForkingSelfTerminating.B>(), Times.Once);
            adapterMock.Verify(m => m.Dispose<DisposableAndAsyncDisposableForkingSelfTerminating.B>(), Times.Once);
        }

        /// <summary>
        /// Tests whether states are disposed of correctly in the ideal happy-path cases.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public static async Task DisposesStatesCorrectlyForForkingNonTerminatingGraph()
        {
            var machine = new StateMachine();

            var callSequence = new MockSequence();
            var adapterMock = new Mock<IStateVerificationAdapter>();
            adapterMock.InSequence(callSequence).Setup(m => m.EnterAsync<DisposableAndAsyncDisposableForkingNonTerminating.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.TransitAsync<DisposableAndAsyncDisposableForkingNonTerminating.A, DisposableAndAsyncDisposableForkingNonTerminating.B>());
            adapterMock.InSequence(callSequence).Setup(m => m.ExitAsync<DisposableAndAsyncDisposableForkingNonTerminating.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.DisposeAsync<DisposableAndAsyncDisposableForkingNonTerminating.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.Dispose<DisposableAndAsyncDisposableForkingNonTerminating.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.EnterAsync<DisposableAndAsyncDisposableForkingNonTerminating.B>());
            adapterMock.InSequence(callSequence).Setup(m => m.ExitAsync<DisposableAndAsyncDisposableForkingNonTerminating.B>());
            adapterMock.InSequence(callSequence).Setup(m => m.DisposeAsync<DisposableAndAsyncDisposableForkingNonTerminating.B>());
            adapterMock.InSequence(callSequence).Setup(m => m.Dispose<DisposableAndAsyncDisposableForkingNonTerminating.B>());

            var tokenSource = new CancellationTokenSource();
            tokenSource.Cancel();

            var initialState = new DisposableAndAsyncDisposableForkingNonTerminating.A(adapterMock.Object, tokenSource.Token) { Controller = machine };

            var result = await machine.RunAsync(initialState, CancellationToken.None);
            Assert.True(result.IsSuccess);

            adapterMock.Verify(m => m.EnterAsync<DisposableAndAsyncDisposableForkingNonTerminating.A>(), Times.Once);
            adapterMock.Verify(m => m.TransitAsync<DisposableAndAsyncDisposableForkingNonTerminating.A, DisposableAndAsyncDisposableForkingNonTerminating.B>(), Times.Once);
            adapterMock.Verify(m => m.ExitAsync<DisposableAndAsyncDisposableForkingNonTerminating.A>(), Times.Once);
            adapterMock.Verify(m => m.DisposeAsync<DisposableAndAsyncDisposableForkingNonTerminating.A>(), Times.Once);
            adapterMock.Verify(m => m.Dispose<DisposableAndAsyncDisposableForkingNonTerminating.A>(), Times.Once);
            adapterMock.Verify(m => m.EnterAsync<DisposableAndAsyncDisposableForkingNonTerminating.B>(), Times.Once);
            adapterMock.Verify(m => m.ExitAsync<DisposableAndAsyncDisposableForkingNonTerminating.B>(), Times.Once);
            adapterMock.Verify(m => m.DisposeAsync<DisposableAndAsyncDisposableForkingNonTerminating.B>(), Times.Once);
            adapterMock.Verify(m => m.Dispose<DisposableAndAsyncDisposableForkingNonTerminating.B>(), Times.Once);
        }

        /// <summary>
        /// Tests whether states are disposed of correctly for exceptions thrown by a call to
        /// <see cref="IState.EnterAsync"/> of the initial state.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public static async Task DisposesStatesCorrectlyIfInitialStateThrowsInEnter()
        {
            var machine = new StateMachine();

            var callSequence = new MockSequence();
            var adapterMock = new Mock<IStateVerificationAdapter>();
            adapterMock.InSequence(callSequence).Setup(m => m.EnterAsync<DisposableAndAsyncDisposableThrowingInInitialEnter.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.DisposeAsync<DisposableAndAsyncDisposableThrowingInInitialEnter.A>());

            var initialState = new DisposableAndAsyncDisposableThrowingInInitialEnter.A(adapterMock.Object) { Controller = machine };

            await Assert.ThrowsAsync<CoercedException>(async () => await machine.RunAsync(initialState));

            adapterMock.Verify(m => m.EnterAsync<DisposableAndAsyncDisposableThrowingInInitialEnter.A>(), Times.Once);
            adapterMock.Verify(m => m.TransitAsync<DisposableAndAsyncDisposableThrowingInInitialEnter.A, DisposableAndAsyncDisposableThrowingInInitialEnter.B>(), Times.Never);
            adapterMock.Verify(m => m.ExitAsync<DisposableAndAsyncDisposableThrowingInInitialEnter.A>(), Times.Never);
            adapterMock.Verify(m => m.DisposeAsync<DisposableAndAsyncDisposableThrowingInInitialEnter.A>(), Times.Once);
            adapterMock.Verify(m => m.Dispose<DisposableAndAsyncDisposableThrowingInInitialEnter.A>(), Times.Once);
            adapterMock.Verify(m => m.EnterAsync<DisposableAndAsyncDisposableThrowingInInitialEnter.B>(), Times.Never);
            adapterMock.Verify(m => m.ExitAsync<DisposableAndAsyncDisposableThrowingInInitialEnter.B>(), Times.Never);
            adapterMock.Verify(m => m.DisposeAsync<DisposableAndAsyncDisposableThrowingInInitialEnter.B>(), Times.Never);
            adapterMock.Verify(m => m.Dispose<DisposableAndAsyncDisposableThrowingInInitialEnter.B>(), Times.Never);
        }

        /// <summary>
        /// Tests whether states are disposed of correctly for errors returned by a call to
        /// <see cref="IState.EnterAsync"/> of the initial state.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public static async Task DisposesStatesCorrectlyIfInitialStateReturnsErrorFromEnter()
        {
            var machine = new StateMachine();

            var callSequence = new MockSequence();
            var adapterMock = new Mock<IStateVerificationAdapter>();
            adapterMock.InSequence(callSequence).Setup(m => m.EnterAsync<DisposableAndAsyncDisposableErrorReturningInInitialEnter.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.DisposeAsync<DisposableAndAsyncDisposableErrorReturningInInitialEnter.A>());

            var initialState = new DisposableAndAsyncDisposableErrorReturningInInitialEnter.A(adapterMock.Object) { Controller = machine };

            var result = await machine.RunAsync(initialState);
            Assert.False(result.IsSuccess);
            Assert.IsType<CoercedError>(result.Error);

            adapterMock.Verify(m => m.EnterAsync<DisposableAndAsyncDisposableErrorReturningInInitialEnter.A>(), Times.Once);
            adapterMock.Verify(m => m.TransitAsync<DisposableAndAsyncDisposableErrorReturningInInitialEnter.A, DisposableAndAsyncDisposableErrorReturningInInitialEnter.B>(), Times.Never);
            adapterMock.Verify(m => m.ExitAsync<DisposableAndAsyncDisposableErrorReturningInInitialEnter.A>(), Times.Never);
            adapterMock.Verify(m => m.DisposeAsync<DisposableAndAsyncDisposableErrorReturningInInitialEnter.A>(), Times.Once);
            adapterMock.Verify(m => m.Dispose<DisposableAndAsyncDisposableErrorReturningInInitialEnter.A>(), Times.Once);
            adapterMock.Verify(m => m.EnterAsync<DisposableAndAsyncDisposableErrorReturningInInitialEnter.B>(), Times.Never);
            adapterMock.Verify(m => m.ExitAsync<DisposableAndAsyncDisposableErrorReturningInInitialEnter.B>(), Times.Never);
            adapterMock.Verify(m => m.DisposeAsync<DisposableAndAsyncDisposableErrorReturningInInitialEnter.B>(), Times.Never);
            adapterMock.Verify(m => m.Dispose<DisposableAndAsyncDisposableErrorReturningInInitialEnter.B>(), Times.Never);
        }

        /// <summary>
        /// Tests whether states are disposed of correctly for exceptions thrown by a call to
        /// <see cref="IState.EnterAsync"/>.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public static async Task DisposesStatesCorrectlyIfStateThrowsInEnter()
        {
            var machine = new StateMachine();

            var callSequence = new MockSequence();
            var adapterMock = new Mock<IStateVerificationAdapter>();
            adapterMock.InSequence(callSequence).Setup(m => m.EnterAsync<DisposableAndAsyncDisposableThrowingInEnter.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.TransitAsync<DisposableAndAsyncDisposableThrowingInEnter.A, DisposableAndAsyncDisposableThrowingInEnter.B>());
            adapterMock.InSequence(callSequence).Setup(m => m.ExitAsync<DisposableAndAsyncDisposableThrowingInEnter.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.DisposeAsync<DisposableAndAsyncDisposableThrowingInEnter.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.Dispose<DisposableAndAsyncDisposableThrowingInEnter.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.EnterAsync<DisposableAndAsyncDisposableThrowingInEnter.B>());
            adapterMock.InSequence(callSequence).Setup(m => m.DisposeAsync<DisposableAndAsyncDisposableThrowingInEnter.B>());
            adapterMock.InSequence(callSequence).Setup(m => m.Dispose<DisposableAndAsyncDisposableThrowingInEnter.B>());

            var initialState = new DisposableAndAsyncDisposableThrowingInEnter.A(adapterMock.Object) { Controller = machine };

            await Assert.ThrowsAsync<CoercedException>(async () => await machine.RunAsync(initialState));

            adapterMock.Verify(m => m.EnterAsync<DisposableAndAsyncDisposableThrowingInEnter.A>(), Times.Once);
            adapterMock.Verify(m => m.TransitAsync<DisposableAndAsyncDisposableThrowingInEnter.A, DisposableAndAsyncDisposableThrowingInEnter.B>(), Times.Once);
            adapterMock.Verify(m => m.ExitAsync<DisposableAndAsyncDisposableThrowingInEnter.A>(), Times.Once);
            adapterMock.Verify(m => m.DisposeAsync<DisposableAndAsyncDisposableThrowingInEnter.A>(), Times.Once);
            adapterMock.Verify(m => m.Dispose<DisposableAndAsyncDisposableThrowingInEnter.A>(), Times.Once);
            adapterMock.Verify(m => m.EnterAsync<DisposableAndAsyncDisposableThrowingInEnter.B>(), Times.Once);
            adapterMock.Verify(m => m.ExitAsync<DisposableAndAsyncDisposableThrowingInEnter.B>(), Times.Never);
            adapterMock.Verify(m => m.DisposeAsync<DisposableAndAsyncDisposableThrowingInEnter.B>(), Times.Once);
            adapterMock.Verify(m => m.Dispose<DisposableAndAsyncDisposableThrowingInEnter.B>(), Times.Once);
        }

        /// <summary>
        /// Tests whether states are disposed of correctly for errors returned by a call to
        /// <see cref="IState.EnterAsync"/>.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public static async Task DisposesStatesCorrectlyIfStateReturnsErrorFromEnter()
        {
            var machine = new StateMachine();

            var callSequence = new MockSequence();
            var adapterMock = new Mock<IStateVerificationAdapter>();
            adapterMock.InSequence(callSequence).Setup(m => m.EnterAsync<DisposableAndAsyncDisposableErrorReturningInEnter.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.TransitAsync<DisposableAndAsyncDisposableErrorReturningInEnter.A, DisposableAndAsyncDisposableErrorReturningInEnter.B>());
            adapterMock.InSequence(callSequence).Setup(m => m.ExitAsync<DisposableAndAsyncDisposableErrorReturningInEnter.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.DisposeAsync<DisposableAndAsyncDisposableErrorReturningInEnter.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.Dispose<DisposableAndAsyncDisposableErrorReturningInEnter.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.EnterAsync<DisposableAndAsyncDisposableErrorReturningInEnter.B>());
            adapterMock.InSequence(callSequence).Setup(m => m.DisposeAsync<DisposableAndAsyncDisposableErrorReturningInEnter.B>());
            adapterMock.InSequence(callSequence).Setup(m => m.Dispose<DisposableAndAsyncDisposableErrorReturningInEnter.B>());

            var initialState = new DisposableAndAsyncDisposableErrorReturningInEnter.A(adapterMock.Object) { Controller = machine };

            var result = await machine.RunAsync(initialState);
            Assert.False(result.IsSuccess);
            Assert.IsType<CoercedError>(result.Error);

            adapterMock.Verify(m => m.EnterAsync<DisposableAndAsyncDisposableErrorReturningInEnter.A>(), Times.Once);
            adapterMock.Verify(m => m.TransitAsync<DisposableAndAsyncDisposableErrorReturningInEnter.A, DisposableAndAsyncDisposableErrorReturningInEnter.B>(), Times.Once);
            adapterMock.Verify(m => m.ExitAsync<DisposableAndAsyncDisposableErrorReturningInEnter.A>(), Times.Once);
            adapterMock.Verify(m => m.DisposeAsync<DisposableAndAsyncDisposableErrorReturningInEnter.A>(), Times.Once);
            adapterMock.Verify(m => m.Dispose<DisposableAndAsyncDisposableErrorReturningInEnter.A>(), Times.Once);
            adapterMock.Verify(m => m.EnterAsync<DisposableAndAsyncDisposableErrorReturningInEnter.B>(), Times.Once);
            adapterMock.Verify(m => m.ExitAsync<DisposableAndAsyncDisposableErrorReturningInEnter.B>(), Times.Never);
            adapterMock.Verify(m => m.DisposeAsync<DisposableAndAsyncDisposableErrorReturningInEnter.B>(), Times.Once);
            adapterMock.Verify(m => m.Dispose<DisposableAndAsyncDisposableErrorReturningInEnter.B>(), Times.Once);
        }

        /// <summary>
        /// Tests whether states are disposed of correctly for exceptions thrown by a call to
        /// <see cref="IState.ExitAsync"/>.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public static async Task DisposesStatesCorrectlyIfStateThrowsInExit()
        {
            var machine = new StateMachine();

            var callSequence = new MockSequence();
            var adapterMock = new Mock<IStateVerificationAdapter>();
            adapterMock.InSequence(callSequence).Setup(m => m.EnterAsync<DisposableAndAsyncDisposableThrowingInExit.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.TransitAsync<DisposableAndAsyncDisposableThrowingInExit.A, DisposableAndAsyncDisposableThrowingInExit.B>());
            adapterMock.InSequence(callSequence).Setup(m => m.ExitAsync<DisposableAndAsyncDisposableThrowingInExit.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.DisposeAsync<DisposableAndAsyncDisposableThrowingInExit.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.Dispose<DisposableAndAsyncDisposableThrowingInExit.A>());

            var initialState = new DisposableAndAsyncDisposableThrowingInExit.A(adapterMock.Object) { Controller = machine };

            await Assert.ThrowsAsync<CoercedException>(async () => await machine.RunAsync(initialState));

            adapterMock.Verify(m => m.EnterAsync<DisposableAndAsyncDisposableThrowingInExit.A>(), Times.Once);
            adapterMock.Verify(m => m.TransitAsync<DisposableAndAsyncDisposableThrowingInExit.A, DisposableAndAsyncDisposableThrowingInExit.B>(), Times.Once);
            adapterMock.Verify(m => m.ExitAsync<DisposableAndAsyncDisposableThrowingInExit.A>(), Times.Once);
            adapterMock.Verify(m => m.DisposeAsync<DisposableAndAsyncDisposableThrowingInExit.A>(), Times.Once);
            adapterMock.Verify(m => m.Dispose<DisposableAndAsyncDisposableThrowingInExit.A>(), Times.Once);
            adapterMock.Verify(m => m.EnterAsync<DisposableAndAsyncDisposableThrowingInExit.B>(), Times.Never);
            adapterMock.Verify(m => m.ExitAsync<DisposableAndAsyncDisposableThrowingInExit.B>(), Times.Never);
            adapterMock.Verify(m => m.DisposeAsync<DisposableAndAsyncDisposableThrowingInExit.B>(), Times.Once);
            adapterMock.Verify(m => m.Dispose<DisposableAndAsyncDisposableThrowingInExit.B>(), Times.Once);
        }

        /// <summary>
        /// Tests whether states are disposed of correctly for errors returned by a call to
        /// <see cref="IState.ExitAsync"/>.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public static async Task DisposesStatesCorrectlyIfStateReturnsErrorExit()
        {
            var machine = new StateMachine();

            var callSequence = new MockSequence();
            var adapterMock = new Mock<IStateVerificationAdapter>();
            adapterMock.InSequence(callSequence).Setup(m => m.EnterAsync<DisposableAndAsyncDisposableErrorReturningInExit.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.TransitAsync<DisposableAndAsyncDisposableErrorReturningInExit.A, DisposableAndAsyncDisposableErrorReturningInExit.B>());
            adapterMock.InSequence(callSequence).Setup(m => m.ExitAsync<DisposableAndAsyncDisposableErrorReturningInExit.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.DisposeAsync<DisposableAndAsyncDisposableErrorReturningInExit.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.Dispose<DisposableAndAsyncDisposableErrorReturningInExit.A>());

            var initialState = new DisposableAndAsyncDisposableErrorReturningInExit.A(adapterMock.Object) { Controller = machine };

            var result = await machine.RunAsync(initialState);
            Assert.False(result.IsSuccess);
            Assert.IsType<CoercedError>(result.Error);

            adapterMock.Verify(m => m.EnterAsync<DisposableAndAsyncDisposableErrorReturningInExit.A>(), Times.Once);
            adapterMock.Verify(m => m.TransitAsync<DisposableAndAsyncDisposableErrorReturningInExit.A, DisposableAndAsyncDisposableErrorReturningInExit.B>(), Times.Once);
            adapterMock.Verify(m => m.ExitAsync<DisposableAndAsyncDisposableErrorReturningInExit.A>(), Times.Once);
            adapterMock.Verify(m => m.DisposeAsync<DisposableAndAsyncDisposableErrorReturningInExit.A>(), Times.Once);
            adapterMock.Verify(m => m.Dispose<DisposableAndAsyncDisposableErrorReturningInExit.A>(), Times.Once);
            adapterMock.Verify(m => m.EnterAsync<DisposableAndAsyncDisposableErrorReturningInExit.B>(), Times.Never);
            adapterMock.Verify(m => m.ExitAsync<DisposableAndAsyncDisposableErrorReturningInExit.B>(), Times.Never);
            adapterMock.Verify(m => m.DisposeAsync<DisposableAndAsyncDisposableErrorReturningInExit.B>(), Times.Once);
            adapterMock.Verify(m => m.Dispose<DisposableAndAsyncDisposableErrorReturningInExit.B>(), Times.Once);
        }

        /// <summary>
        /// Tests whether states are disposed of correctly for exceptions thrown by a call to
        /// <see cref="IState.ExitAsync"/> of the final state.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public static async Task DisposesStatesCorrectlyIfStateThrowsInFinalExit()
        {
            var machine = new StateMachine();

            var callSequence = new MockSequence();
            var adapterMock = new Mock<IStateVerificationAdapter>();
            adapterMock.InSequence(callSequence).Setup(m => m.EnterAsync<DisposableAndAsyncDisposableThrowingInFinalExit.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.TransitAsync<DisposableAndAsyncDisposableThrowingInFinalExit.A, DisposableAndAsyncDisposableThrowingInFinalExit.B>());
            adapterMock.InSequence(callSequence).Setup(m => m.ExitAsync<DisposableAndAsyncDisposableThrowingInFinalExit.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.DisposeAsync<DisposableAndAsyncDisposableThrowingInFinalExit.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.Dispose<DisposableAndAsyncDisposableThrowingInFinalExit.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.EnterAsync<DisposableAndAsyncDisposableThrowingInFinalExit.B>());
            adapterMock.InSequence(callSequence).Setup(m => m.ExitAsync<DisposableAndAsyncDisposableThrowingInFinalExit.B>());
            adapterMock.InSequence(callSequence).Setup(m => m.DisposeAsync<DisposableAndAsyncDisposableThrowingInFinalExit.B>());
            adapterMock.InSequence(callSequence).Setup(m => m.Dispose<DisposableAndAsyncDisposableThrowingInFinalExit.B>());

            var initialState = new DisposableAndAsyncDisposableThrowingInFinalExit.A(adapterMock.Object) { Controller = machine };

            await Assert.ThrowsAsync<CoercedException>(async () => await machine.RunAsync(initialState));

            adapterMock.Verify(m => m.EnterAsync<DisposableAndAsyncDisposableThrowingInFinalExit.A>(), Times.Once);
            adapterMock.Verify(m => m.TransitAsync<DisposableAndAsyncDisposableThrowingInFinalExit.A, DisposableAndAsyncDisposableThrowingInFinalExit.B>(), Times.Once);
            adapterMock.Verify(m => m.ExitAsync<DisposableAndAsyncDisposableThrowingInFinalExit.A>(), Times.Once);
            adapterMock.Verify(m => m.DisposeAsync<DisposableAndAsyncDisposableThrowingInFinalExit.A>(), Times.Once);
            adapterMock.Verify(m => m.Dispose<DisposableAndAsyncDisposableThrowingInFinalExit.A>(), Times.Once);
            adapterMock.Verify(m => m.EnterAsync<DisposableAndAsyncDisposableThrowingInFinalExit.B>(), Times.Once);
            adapterMock.Verify(m => m.ExitAsync<DisposableAndAsyncDisposableThrowingInFinalExit.B>(), Times.Once);
            adapterMock.Verify(m => m.DisposeAsync<DisposableAndAsyncDisposableThrowingInFinalExit.B>(), Times.Once);
            adapterMock.Verify(m => m.Dispose<DisposableAndAsyncDisposableThrowingInFinalExit.B>(), Times.Once);
        }

        /// <summary>
        /// Tests whether states are disposed of correctly for errors returned by a call to
        /// <see cref="IState.ExitAsync"/> of the final state.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public static async Task DisposesStatesCorrectlyIfStateReturnsErrorInFinalExit()
        {
            var machine = new StateMachine();

            var callSequence = new MockSequence();
            var adapterMock = new Mock<IStateVerificationAdapter>();
            adapterMock.InSequence(callSequence).Setup(m => m.EnterAsync<DisposableAndAsyncDisposableErrorReturningInFinalExit.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.TransitAsync<DisposableAndAsyncDisposableErrorReturningInFinalExit.A, DisposableAndAsyncDisposableErrorReturningInFinalExit.B>());
            adapterMock.InSequence(callSequence).Setup(m => m.ExitAsync<DisposableAndAsyncDisposableErrorReturningInFinalExit.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.DisposeAsync<DisposableAndAsyncDisposableErrorReturningInFinalExit.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.Dispose<DisposableAndAsyncDisposableErrorReturningInFinalExit.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.EnterAsync<DisposableAndAsyncDisposableErrorReturningInFinalExit.B>());
            adapterMock.InSequence(callSequence).Setup(m => m.ExitAsync<DisposableAndAsyncDisposableErrorReturningInFinalExit.B>());
            adapterMock.InSequence(callSequence).Setup(m => m.DisposeAsync<DisposableAndAsyncDisposableErrorReturningInFinalExit.B>());
            adapterMock.InSequence(callSequence).Setup(m => m.Dispose<DisposableAndAsyncDisposableErrorReturningInFinalExit.B>());

            var initialState = new DisposableAndAsyncDisposableErrorReturningInFinalExit.A(adapterMock.Object) { Controller = machine };

            var result = await machine.RunAsync(initialState);
            Assert.False(result.IsSuccess);
            Assert.IsType<CoercedError>(result.Error);

            adapterMock.Verify(m => m.EnterAsync<DisposableAndAsyncDisposableErrorReturningInFinalExit.A>(), Times.Once);
            adapterMock.Verify(m => m.TransitAsync<DisposableAndAsyncDisposableErrorReturningInFinalExit.A, DisposableAndAsyncDisposableErrorReturningInFinalExit.B>(), Times.Once);
            adapterMock.Verify(m => m.ExitAsync<DisposableAndAsyncDisposableErrorReturningInFinalExit.A>(), Times.Once);
            adapterMock.Verify(m => m.DisposeAsync<DisposableAndAsyncDisposableErrorReturningInFinalExit.A>(), Times.Once);
            adapterMock.Verify(m => m.Dispose<DisposableAndAsyncDisposableErrorReturningInFinalExit.A>(), Times.Once);
            adapterMock.Verify(m => m.EnterAsync<DisposableAndAsyncDisposableErrorReturningInFinalExit.B>(), Times.Once);
            adapterMock.Verify(m => m.ExitAsync<DisposableAndAsyncDisposableErrorReturningInFinalExit.B>(), Times.Once);
            adapterMock.Verify(m => m.DisposeAsync<DisposableAndAsyncDisposableErrorReturningInFinalExit.B>(), Times.Once);
            adapterMock.Verify(m => m.Dispose<DisposableAndAsyncDisposableErrorReturningInFinalExit.B>(), Times.Once);
        }

        /// <summary>
        /// Tests whether states are disposed of correctly for exceptions thrown by a call to
        /// <see cref="ITransition{TState}.TransitAsync"/>.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public static async Task DisposesStatesCorrectlyIfStateThrowsInTransition()
        {
            var machine = new StateMachine();

            var callSequence = new MockSequence();
            var adapterMock = new Mock<IStateVerificationAdapter>();
            adapterMock.InSequence(callSequence).Setup(m => m.EnterAsync<DisposableAndAsyncDisposableThrowingInTransition.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.TransitAsync<DisposableAndAsyncDisposableThrowingInTransition.A, DisposableAndAsyncDisposableThrowingInTransition.B>());
            adapterMock.InSequence(callSequence).Setup(m => m.ExitAsync<DisposableAndAsyncDisposableThrowingInTransition.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.DisposeAsync<DisposableAndAsyncDisposableThrowingInTransition.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.Dispose<DisposableAndAsyncDisposableThrowingInTransition.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.EnterAsync<DisposableAndAsyncDisposableThrowingInTransition.B>());
            adapterMock.InSequence(callSequence).Setup(m => m.ExitAsync<DisposableAndAsyncDisposableThrowingInTransition.B>());
            adapterMock.InSequence(callSequence).Setup(m => m.DisposeAsync<DisposableAndAsyncDisposableThrowingInTransition.B>());
            adapterMock.InSequence(callSequence).Setup(m => m.Dispose<DisposableAndAsyncDisposableThrowingInTransition.B>());

            var initialState = new DisposableAndAsyncDisposableThrowingInTransition.A(adapterMock.Object) { Controller = machine };

            await Assert.ThrowsAsync<CoercedException>(async () => await machine.RunAsync(initialState));

            adapterMock.Verify(m => m.EnterAsync<DisposableAndAsyncDisposableThrowingInTransition.A>(), Times.Once);
            adapterMock.Verify(m => m.TransitAsync<DisposableAndAsyncDisposableThrowingInTransition.A, DisposableAndAsyncDisposableThrowingInTransition.B>(), Times.Once);
            adapterMock.Verify(m => m.ExitAsync<DisposableAndAsyncDisposableThrowingInTransition.A>(), Times.Never);
            adapterMock.Verify(m => m.DisposeAsync<DisposableAndAsyncDisposableThrowingInTransition.A>(), Times.Once);
            adapterMock.Verify(m => m.Dispose<DisposableAndAsyncDisposableThrowingInTransition.A>(), Times.Once);
            adapterMock.Verify(m => m.EnterAsync<DisposableAndAsyncDisposableThrowingInTransition.B>(), Times.Never);
            adapterMock.Verify(m => m.ExitAsync<DisposableAndAsyncDisposableThrowingInTransition.B>(), Times.Never);
            adapterMock.Verify(m => m.DisposeAsync<DisposableAndAsyncDisposableThrowingInTransition.B>(), Times.Never);
            adapterMock.Verify(m => m.Dispose<DisposableAndAsyncDisposableThrowingInTransition.B>(), Times.Never);
        }

        /// <summary>
        /// Tests whether states are disposed of correctly for errors returned by a call to
        /// <see cref="ITransition{TState}.TransitAsync"/>.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public static async Task DisposesStatesCorrectlyIfStateReturnsErrorInTransition()
        {
            var machine = new StateMachine();

            var callSequence = new MockSequence();
            var adapterMock = new Mock<IStateVerificationAdapter>();
            adapterMock.InSequence(callSequence).Setup(m => m.EnterAsync<DisposableAndAsyncDisposableErrorReturningInTransition.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.TransitAsync<DisposableAndAsyncDisposableErrorReturningInTransition.A, DisposableAndAsyncDisposableErrorReturningInTransition.B>());
            adapterMock.InSequence(callSequence).Setup(m => m.ExitAsync<DisposableAndAsyncDisposableErrorReturningInTransition.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.DisposeAsync<DisposableAndAsyncDisposableErrorReturningInTransition.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.Dispose<DisposableAndAsyncDisposableErrorReturningInTransition.A>());
            adapterMock.InSequence(callSequence).Setup(m => m.EnterAsync<DisposableAndAsyncDisposableErrorReturningInTransition.B>());
            adapterMock.InSequence(callSequence).Setup(m => m.ExitAsync<DisposableAndAsyncDisposableErrorReturningInTransition.B>());
            adapterMock.InSequence(callSequence).Setup(m => m.DisposeAsync<DisposableAndAsyncDisposableErrorReturningInTransition.B>());
            adapterMock.InSequence(callSequence).Setup(m => m.Dispose<DisposableAndAsyncDisposableErrorReturningInTransition.B>());

            var initialState = new DisposableAndAsyncDisposableErrorReturningInTransition.A(adapterMock.Object) { Controller = machine };

            var result = await machine.RunAsync(initialState);
            Assert.False(result.IsSuccess);
            Assert.IsType<CoercedError>(result.Error);

            adapterMock.Verify(m => m.EnterAsync<DisposableAndAsyncDisposableErrorReturningInTransition.A>(), Times.Once);
            adapterMock.Verify(m => m.TransitAsync<DisposableAndAsyncDisposableErrorReturningInTransition.A, DisposableAndAsyncDisposableErrorReturningInTransition.B>(), Times.Once);
            adapterMock.Verify(m => m.ExitAsync<DisposableAndAsyncDisposableErrorReturningInTransition.A>(), Times.Never);
            adapterMock.Verify(m => m.DisposeAsync<DisposableAndAsyncDisposableErrorReturningInTransition.A>(), Times.Once);
            adapterMock.Verify(m => m.Dispose<DisposableAndAsyncDisposableErrorReturningInTransition.A>(), Times.Once);
            adapterMock.Verify(m => m.EnterAsync<DisposableAndAsyncDisposableErrorReturningInTransition.B>(), Times.Never);
            adapterMock.Verify(m => m.ExitAsync<DisposableAndAsyncDisposableErrorReturningInTransition.B>(), Times.Never);
            adapterMock.Verify(m => m.DisposeAsync<DisposableAndAsyncDisposableErrorReturningInTransition.B>(), Times.Never);
            adapterMock.Verify(m => m.Dispose<DisposableAndAsyncDisposableErrorReturningInTransition.B>(), Times.Never);
        }
    }
}
