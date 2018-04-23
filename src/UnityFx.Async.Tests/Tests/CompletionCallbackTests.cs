// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using Xunit;

namespace UnityFx.Async
{
	public class CompletionCallbackTests
	{
		[Fact]
		public void TryAddCompletionCallback_FailsIfOperationIsCompleted()
		{
			// Arrange
			var op = new AsyncCompletionSource();
			op.SetCanceled();

			// Act
			var result = op.TryAddCompletionCallback(_ => { }, null);

			// Assert
			Assert.False(result);
		}

		[Fact]
		public void TryAddCompletionCallback_FailsIfOperationIsCompletedSynchronously()
		{
			// Arrange
			var op = AsyncResult.CompletedOperation;

			// Act
			var result = op.TryAddCompletionCallback(_ => { }, null);

			// Assert
			Assert.False(result);
		}

		[Fact]
		public async Task TryAddCompletionCallback_ExecutesWhenOperationCompletes()
		{
			// Arrange
			var op = AsyncResult.Delay(1);
			var callbackCalled = false;

			op.TryAddCompletionCallback(_ => callbackCalled = true, null);

			// Act
			await op;

			// Assert
			Assert.True(callbackCalled);
		}

		[Fact]
		public async Task TryAddCompletionCallback_IsThreadSafe()
		{
			// Arrange
			var op = new AsyncCompletionSource();
			var counter = 0;

			void CompletionCallback(IAsyncOperation o)
			{
				++counter;
			}

			void TestMethod()
			{
				for (var i = 0; i < 10000; ++i)
				{
					op.TryAddCompletionCallback(CompletionCallback);
				}
			}

			// Act
			await Task.WhenAll(
				Task.Run(new Action(TestMethod)),
				Task.Run(new Action(TestMethod)),
				Task.Run(new Action(TestMethod)));

			// Assert
			op.SetCompleted();
			Assert.Equal(30000, counter);
		}

		[Fact]
		public void TryAddContinuation_ExecutesWhenOperationCompletes()
		{
			// Arrange
			var op = new AsyncCompletionSource();
			var continuation = Substitute.For<IAsyncContinuation>();
			op.TryAddContinuation(continuation);

			// Act
			op.SetCompleted();

			// Assert
			continuation.Received(1).Invoke(op, false);
			continuation.Received(0).Invoke(op, true);
		}

		[Fact]
		public void AddContinuation_ExecutesIfOperationIsCompletedSynchronously()
		{
			// Arrange
			var op = AsyncResult.CompletedOperation;
			var continuation = Substitute.For<IAsyncContinuation>();

			// Act
			op.AddContinuation(continuation);

			// Assert
			continuation.Received(1).Invoke(op, true);
			continuation.Received(0).Invoke(op, false);
		}

		[Fact]
		public void AddCompletionCallback_ExecutesIfOperationIsCompletedSynchronously()
		{
			// Arrange
			var op = AsyncResult.CanceledOperation;
			var callbackCalled = false;

			// Act
			op.AddCompletionCallback(_ => callbackCalled = true, null);

			// Assert
			Assert.True(callbackCalled);
		}

		[Fact]
		public async Task TryAddCompletionCallback_ContinuationsAreRunOnCorrectSynchronozationContext()
		{
			// Arrange
			var op = new AsyncCompletionSource();
			var op2 = new AsyncCompletionSource();
			var sc = Substitute.For<SynchronizationContext>();
			var tid = 0;
			var tidActual = 0;

			op.TryAddCompletionCallback(_ => { }, sc);
			op2.TryAddCompletionCallback(_ => tidActual = Thread.CurrentThread.ManagedThreadId, null);

			// Act
			await Task.Run(() => op.SetCompleted());
			await Task.Run(() => { tid = Thread.CurrentThread.ManagedThreadId; op2.SetCompleted(); });

			// Assert
			sc.Received(1).Post(Arg.Any<SendOrPostCallback>(), Arg.Any<object>());
			Assert.Equal(tid, tidActual);
		}
	}
}
