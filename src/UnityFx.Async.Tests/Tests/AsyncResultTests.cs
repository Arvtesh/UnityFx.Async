// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace UnityFx.Async
{
	public class AsyncResultTests
	{
		#region constructors

		[Fact]
		public void DefaultConstructor_SetsStatusToCreated()
		{
			// Act
			var op = new AsyncResult();

			// Assert
			AssertNotCompleted(op, AsyncOperationStatus.Created);
		}

		[Fact]
		public void Constructor_SetsStatusToScheduled()
		{
			// Act
			var op = new AsyncResult(AsyncOperationStatus.Scheduled);

			// Assert
			AssertNotCompleted(op, AsyncOperationStatus.Scheduled);
		}

		[Fact]
		public void Constructor_SetsStatusToRunning()
		{
			// Act
			var op = new AsyncResult(AsyncOperationStatus.Running);

			// Assert
			AssertNotCompleted(op, AsyncOperationStatus.Running);
		}

		[Fact]
		public void Constructor_SetsStatusToCompleted()
		{
			// Act
			var op = new AsyncResult(AsyncOperationStatus.RanToCompletion);

			// Assert
			AssertCompleted(op);
			Assert.True(op.CompletedSynchronously);
		}

		[Fact]
		public void Constructor_SetsStatusToFaulted()
		{
			// Act
			var op = new AsyncResult(AsyncOperationStatus.Faulted);

			// Assert
			AssertFaulted(op);
			Assert.True(op.CompletedSynchronously);
		}

		[Fact]
		public void Constructor_SetsStatusToCanceled()
		{
			// Act
			var op = new AsyncResult(AsyncOperationStatus.Canceled);

			// Assert
			AssertCanceled(op);
			Assert.True(op.CompletedSynchronously);
		}

		[Fact]
		public void Constructor_SetsAsyncState()
		{
			// Arrange
			var state = new object();

			// Act
			var op = new AsyncResult(null, state);

			// Assert
			AssertNotCompleted(op, AsyncOperationStatus.Created);
			Assert.Equal(state, op.AsyncState);
		}

		#endregion

		#region static methods

		[Fact]
		public void CompletedOperation_ReturnsCompletedOperation()
		{
			// Act
			var op = AsyncResult.CompletedOperation;

			// Assert
			AssertCompleted(op);
			Assert.True(op.CompletedSynchronously);
		}

		[Fact]
		public void FromCanceled_ReturnsCanceledOperation()
		{
			// Act
			var op = AsyncResult.FromCanceled();

			// Assert
			AssertCanceled(op);
			Assert.True(op.CompletedSynchronously);
		}

		[Fact]
		public void FromCanceled_ReturnsCanceledOperation_Generic()
		{
			// Act
			var op = AsyncResult.FromCanceled<int>();

			// Assert
			AssertCanceled(op);
			Assert.True(op.CompletedSynchronously);
		}

		[Fact]
		public void FromException_ReturnsFailedOperation()
		{
			// Arrange
			var e = new InvalidCastException();

			// Act
			var op = AsyncResult.FromException(e);

			// Assert
			AssertFaulted(op, e);
			Assert.True(op.CompletedSynchronously);
		}

		[Fact]
		public void FromException_ReturnsCanceledOperation()
		{
			// Arrange
			var e = new OperationCanceledException();

			// Act
			var op = AsyncResult.FromException(e);

			// Assert
			AssertCanceled(op, e);
			Assert.True(op.CompletedSynchronously);
		}

		[Fact]
		public void FromResult_ReturnsCompletedOperation()
		{
			// Arrange
			var result = 25;

			// Act
			var op = AsyncResult.FromResult(result);

			// Assert
			AssertCompletedWithResult(op, result);
			Assert.True(op.CompletedSynchronously);
		}

		[Fact]
		public async Task Retry_CompletesWhenSourceCompletes()
		{
			// Arrange
			var counter = 3;

			IAsyncOperation OpFactory()
			{
				if (--counter > 0)
				{
					return AsyncResult.FromException(new Exception());
				}
				else
				{
					return AsyncResult.Delay(1);
				}
			}

			// Act
			var op = AsyncResult.Retry(OpFactory, 1);
			await op;

			// Assert
			AssertCompleted(op);
		}

		[Fact]
		public async Task Retry_CompletesAfterMaxRetriesExceeded()
		{
			// Arrange
			var counter = 3;
			var e = new Exception();

			IAsyncOperation OpFactory()
			{
				--counter;
				return AsyncResult.FromException(e);
			}

			// Act
			var op = AsyncResult.Retry(OpFactory, 1, 1);

			try
			{
				await op;
			}
			catch
			{
			}

			// Assert
			AssertFaulted(op, e);
			Assert.Equal(2, counter);
		}

		[Fact]
		public async Task WhenAll_CompletesWhenAllOperationsCompleted()
		{
			// Arrange
			var op1 = AsyncResult.Delay(1);
			var op2 = AsyncResult.Delay(2);

			// Act
			await AsyncResult.WhenAll(op1, op2);

			// Assert
			AssertCompleted(op1);
			AssertCompleted(op2);
		}

		[Fact]
		public async Task WhenAny_CompletesWhenAnyOperationCompletes()
		{
			// Arrange
			var op1 = AsyncResult.Delay(1);
			var op2 = AsyncResult.Delay(Timeout.Infinite);

			// Act
			await AsyncResult.WhenAny(op1, op2);

			// Assert
			AssertCompleted(op1);
		}

		#endregion

		#region interface

		#region SetScheduled

		[Theory]
		[InlineData(AsyncOperationStatus.Created)]
		public void SetScheduled_SetsStatusToScheduled(AsyncOperationStatus status)
		{
			// Arrange
			var op = new AsyncCompletionSource(status);

			// Act
			op.SetScheduled();

			// Assert
			AssertNotCompleted(op, AsyncOperationStatus.Scheduled);
		}

		[Theory]
		[InlineData(AsyncOperationStatus.Scheduled)]
		[InlineData(AsyncOperationStatus.Running)]
		[InlineData(AsyncOperationStatus.RanToCompletion)]
		[InlineData(AsyncOperationStatus.Faulted)]
		[InlineData(AsyncOperationStatus.Canceled)]
		public void SetScheduled_ThrowsIfOperationIsNotCreated(AsyncOperationStatus status)
		{
			// Arrange
			var op = new AsyncCompletionSource(status);

			// Act/Assert
			Assert.Throws<InvalidOperationException>(() => op.SetScheduled());
		}

		[Fact]
		public void SetScheduled_ThrowsIfOperationIsDisposed()
		{
			// Arrange
			var op = new AsyncCompletionSource(AsyncOperationStatus.RanToCompletion);
			op.Dispose();

			// Act/Assert
			Assert.Throws<ObjectDisposedException>(() => op.SetScheduled());
		}

		#endregion

		#region SetRunning

		[Theory]
		[InlineData(AsyncOperationStatus.Created)]
		[InlineData(AsyncOperationStatus.Scheduled)]
		public void SetRunning_SetsStatusToRunning(AsyncOperationStatus status)
		{
			// Arrange
			var op = new AsyncCompletionSource(status);

			// Act
			op.SetRunning();

			// Assert
			AssertNotCompleted(op, AsyncOperationStatus.Running);
		}

		[Theory]
		[InlineData(AsyncOperationStatus.Running)]
		[InlineData(AsyncOperationStatus.RanToCompletion)]
		[InlineData(AsyncOperationStatus.Faulted)]
		[InlineData(AsyncOperationStatus.Canceled)]
		public void SetRunning_ThrowsIfOperationIsNotCreatedOrScheduled(AsyncOperationStatus status)
		{
			// Arrange
			var op = new AsyncCompletionSource(status);

			// Act/Assert
			Assert.Throws<InvalidOperationException>(() => op.SetRunning());
		}

		[Fact]
		public void SetRunning_ThrowsIfOperationIsDisposed()
		{
			// Arrange
			var op = new AsyncCompletionSource(AsyncOperationStatus.RanToCompletion);
			op.Dispose();

			// Act/Assert
			Assert.Throws<ObjectDisposedException>(() => op.SetRunning());
		}

		#endregion

		#endregion

		#region async/await

		[Fact]
		public async Task Await_CollbackIsTriggered()
		{
			// Arrange
			var op = new AsyncCompletionSource();
			var task = Task.Run(() =>
			{
				Thread.Sleep(10);
				op.SetCompleted();
			});

			// Act
			await op;

			// Assert
			AssertCompleted(op);
		}

		[Fact]
		public async Task Await_ShouldThrowIfFaulted()
		{
			// Arrange
			var op = new AsyncCompletionSource();
			var expectedException = new Exception();
			var actualException = default(Exception);
			var task = Task.Run(() =>
			{
				Thread.Sleep(10);
				op.SetException(expectedException);
			});

			// Act
			try
			{
				await op;
			}
			catch (Exception e)
			{
				actualException = e;
			}

			// Assert
			Assert.Equal(expectedException, actualException);
			AssertFaulted(op);
		}

		[Fact]
		public async Task Await_ShouldThrowIfCanceled()
		{
			// Arrange
			var op = new AsyncCompletionSource();
			var actualException = default(Exception);
			var task = Task.Run(() =>
			{
				Thread.Sleep(10);
				op.SetCanceled();
			});

			// Act
			try
			{
				await op;
			}
			catch (Exception e)
			{
				actualException = e;
			}

			// Assert
			Assert.IsType<OperationCanceledException>(actualException);
			AssertCanceled(op);
		}

		#endregion

		#region IAsyncCompletionSource

		#region SetCanceled/TrySetCanceled

		[Theory]
		[InlineData(AsyncOperationStatus.RanToCompletion)]
		[InlineData(AsyncOperationStatus.Faulted)]
		[InlineData(AsyncOperationStatus.Canceled)]
		public void SetCanceled_ThrowsIfOperationIsCompleted(AsyncOperationStatus status)
		{
			// Arrange
			var op = new AsyncCompletionSource(status);

			// Act/Assert
			Assert.Throws<InvalidOperationException>(() => op.SetCanceled());
			Assert.True(op.CompletedSynchronously);
		}

		[Theory]
		[InlineData(AsyncOperationStatus.Created)]
		[InlineData(AsyncOperationStatus.Scheduled)]
		[InlineData(AsyncOperationStatus.Running)]
		public void TrySetCanceled_SetsStatusToCanceled(AsyncOperationStatus status)
		{
			// Arrange
			var op = new AsyncCompletionSource(status);

			// Act
			var result = op.TrySetCanceled();

			// Assert
			AssertCanceled(op);
			Assert.True(result);
		}

		[Fact]
		public void TrySetCanceled_RaisesCompletionCallbacks()
		{
			// Arrange
			var asyncCallbackCalled1 = false;
			var asyncCallbackCalled2 = false;
			var op = new AsyncCompletionSource(asyncResult => asyncCallbackCalled1 = true, null);
			op.AddCompletionCallback(asyncOp => asyncCallbackCalled2 = true, AsyncContinuationOptions.None);

			// Act
			op.TrySetCanceled();

			// Assert
			Assert.True(asyncCallbackCalled1);
			Assert.True(asyncCallbackCalled2);
		}

		[Fact]
		public void TrySetCanceled_CallsOnCompleted()
		{
			// Arrange
			var op = new AsyncResultOverrides();

			// Act
			op.TrySetCanceled();

			// Assert
			Assert.True(op.OnCompletedCalled);
			Assert.True(op.OnStatusChangedCalled);
		}

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public void TrySetCanceled_SetsCompletedSynchronously(bool completedSynchronously)
		{
			// Arrange
			var op = new AsyncCompletionSource();

			// Act
			op.TrySetCanceled(completedSynchronously);

			// Assert
			Assert.Equal(completedSynchronously, op.CompletedSynchronously);
		}

		[Fact]
		public void TrySetCanceled_FailsIfOperationIsCompleted()
		{
			// Arrange
			var op = new AsyncCompletionSource(AsyncOperationStatus.RanToCompletion);

			// Act
			var result = op.TrySetCanceled();

			// Assert
			Assert.False(result);
			Assert.True(op.CompletedSynchronously);
			AssertCompleted(op);
		}

		[Fact]
		public void TrySetCanceled_ThrowsIfOperationIsDisposed()
		{
			// Arrange
			var op = new AsyncCompletionSource(AsyncOperationStatus.RanToCompletion);
			op.Dispose();

			// Act/Assert
			Assert.Throws<ObjectDisposedException>(() => op.TrySetCanceled());
		}

		#endregion

		#region SetException/TrySetException

		[Theory]
		[InlineData(AsyncOperationStatus.RanToCompletion)]
		[InlineData(AsyncOperationStatus.Faulted)]
		[InlineData(AsyncOperationStatus.Canceled)]
		public void SetException_ThrowsIfOperationIsCompleted(AsyncOperationStatus status)
		{
			// Arrange
			var e = new Exception();
			var op = new AsyncCompletionSource(status);

			// Act/Assert
			Assert.Throws<InvalidOperationException>(() => op.SetException(e));
			Assert.True(op.CompletedSynchronously);
		}

		[Theory]
		[InlineData(AsyncOperationStatus.Created)]
		[InlineData(AsyncOperationStatus.Scheduled)]
		[InlineData(AsyncOperationStatus.Running)]
		public void TrySetException_SetsStatusToFaulted(AsyncOperationStatus status)
		{
			// Arrange
			var e = new Exception();
			var op = new AsyncCompletionSource(status);

			// Act
			var result = op.TrySetException(e);

			// Assert
			AssertFaulted(op, e);
			Assert.True(result);
		}

		[Theory]
		[InlineData(AsyncOperationStatus.Created)]
		[InlineData(AsyncOperationStatus.Scheduled)]
		[InlineData(AsyncOperationStatus.Running)]
		public void TrySetException_SetsStatusToCanceled(AsyncOperationStatus status)
		{
			// Arrange
			var e = new OperationCanceledException();
			var op = new AsyncCompletionSource(status);

			// Act
			var result = op.TrySetException(e);

			// Assert
			AssertCanceled(op);
			Assert.True(result);
		}

		[Fact]
		public void TrySetException_RaisesCompletionCallbacks()
		{
			// Arrange
			var e = new Exception();
			var asyncCallbackCalled1 = false;
			var asyncCallbackCalled2 = false;
			var op = new AsyncCompletionSource(asyncResult => asyncCallbackCalled1 = true, null);
			op.AddCompletionCallback(asyncOp => asyncCallbackCalled2 = true, AsyncContinuationOptions.None);

			// Act
			op.TrySetException(e);

			// Assert
			Assert.True(asyncCallbackCalled1);
			Assert.True(asyncCallbackCalled2);
		}

		[Fact]
		public void TrySetException_CallsOnCompleted()
		{
			// Arrange
			var e = new Exception();
			var op = new AsyncResultOverrides();

			// Act
			op.TrySetException(e);

			// Assert
			Assert.Equal(e, op.OnCompletedException);
			Assert.True(op.OnCompletedCalled);
			Assert.True(op.OnStatusChangedCalled);
		}

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public void TrySetException_SetsCompletedSynchronously(bool completedSynchronously)
		{
			// Arrange
			var e = new Exception();
			var op = new AsyncCompletionSource();

			// Act
			op.TrySetException(e, completedSynchronously);

			// Assert
			Assert.Equal(completedSynchronously, op.CompletedSynchronously);
		}

		[Fact]
		public void TrySetException_FailsIfOperationIsCompleted()
		{
			// Arrange
			var op = new AsyncCompletionSource(AsyncOperationStatus.RanToCompletion);

			// Act
			var result = op.TrySetCanceled();

			// Assert
			Assert.False(result);
			Assert.True(op.CompletedSynchronously);
			AssertCompleted(op);
		}

		[Fact]
		public void TrySetException_ThrowsIfExceptionIsNull()
		{
			// Arrange
			var op = new AsyncCompletionSource();

			// Act/Assert
			Assert.Throws<ArgumentNullException>(() => op.TrySetException(null));
		}

		[Fact]
		public void TrySetException_ThrowsIfOperationIsDisposed()
		{
			// Arrange
			var e = new Exception();
			var op = new AsyncCompletionSource(AsyncOperationStatus.RanToCompletion);
			op.Dispose();

			// Act/Assert
			Assert.Throws<ObjectDisposedException>(() => op.TrySetException(e));
		}

		#endregion

		#region SetCompleted/TrySetCompleted

		[Theory]
		[InlineData(AsyncOperationStatus.RanToCompletion)]
		[InlineData(AsyncOperationStatus.Faulted)]
		[InlineData(AsyncOperationStatus.Canceled)]
		public void SetCompleted_ThrowsIfOperationIsCompleted(AsyncOperationStatus status)
		{
			// Arrange
			var op = new AsyncCompletionSource(status);

			// Act/Assert
			Assert.Throws<InvalidOperationException>(() => op.SetCompleted());
			Assert.True(op.CompletedSynchronously);
		}

		[Theory]
		[InlineData(AsyncOperationStatus.Created)]
		[InlineData(AsyncOperationStatus.Scheduled)]
		[InlineData(AsyncOperationStatus.Running)]
		public void TrySetCompleted_SetsStatusToRanToCompletion(AsyncOperationStatus status)
		{
			// Arrange
			var op = new AsyncCompletionSource(status);

			// Act
			var result = op.TrySetCompleted();

			// Assert
			AssertCompleted(op);
			Assert.True(result);
		}

		[Fact]
		public void TrySetCompleted_RaisesCompletionCallbacks()
		{
			// Arrange
			var asyncCallbackCalled1 = false;
			var asyncCallbackCalled2 = false;
			var op = new AsyncCompletionSource(asyncResult => asyncCallbackCalled1 = true, null);
			op.AddCompletionCallback(asyncOp => asyncCallbackCalled2 = true, AsyncContinuationOptions.None);

			// Act
			op.TrySetCompleted();

			// Assert
			Assert.True(asyncCallbackCalled1);
			Assert.True(asyncCallbackCalled2);
		}

		[Fact]
		public void TrySetCompleted_CallsOnCompleted()
		{
			// Arrange
			var op = new AsyncResultOverrides();

			// Act
			op.TrySetCompleted();

			// Assert
			Assert.True(op.OnCompletedCalled);
			Assert.True(op.OnStatusChangedCalled);
		}

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public void TrySetCompleted_SetsCompletedSynchronously(bool completedSynchronously)
		{
			// Arrange
			var op = new AsyncCompletionSource();

			// Act
			op.TrySetCompleted(completedSynchronously);

			// Assert
			Assert.Equal(completedSynchronously, op.CompletedSynchronously);
		}

		[Fact]
		public void TrySetCompleted_FailsIfOperationIsCompleted()
		{
			// Arrange
			var op = new AsyncCompletionSource(AsyncOperationStatus.Canceled);

			// Act
			var result = op.TrySetCompleted();

			// Assert
			Assert.False(result);
			Assert.True(op.CompletedSynchronously);
			AssertCanceled(op);
		}

		[Fact]
		public void TrySetCompleted_ThrowsIfOperationIsDisposed()
		{
			// Arrange
			var op = new AsyncCompletionSource(AsyncOperationStatus.Canceled);
			op.Dispose();

			// Act/Assert
			Assert.Throws<ObjectDisposedException>(() => op.TrySetCompleted());
		}

		#endregion

		#region SetResult/TrySetResult

		[Theory]
		[InlineData(AsyncOperationStatus.RanToCompletion)]
		[InlineData(AsyncOperationStatus.Faulted)]
		[InlineData(AsyncOperationStatus.Canceled)]
		public void SetResult_ThrowsIfOperationIsCompleted(AsyncOperationStatus status)
		{
			// Arrange
			var op = new AsyncCompletionSource<int>(status);

			// Act/Assert
			Assert.Throws<InvalidOperationException>(() => op.SetResult(10));
			Assert.True(op.CompletedSynchronously);
		}

		[Theory]
		[InlineData(AsyncOperationStatus.Created)]
		[InlineData(AsyncOperationStatus.Scheduled)]
		[InlineData(AsyncOperationStatus.Running)]
		public void TrySetResult_SetsStatusToRanToCompletion(AsyncOperationStatus status)
		{
			// Arrange
			var resultValue = new object();
			var op = new AsyncCompletionSource<object>(status);

			// Act
			var result = op.TrySetResult(resultValue);

			// Assert
			AssertCompletedWithResult(op, resultValue);
			Assert.True(result);
		}

		[Fact]
		public void TrySetResult_RaisesCompletionCallbacks()
		{
			// Arrange
			var asyncCallbackCalled1 = false;
			var asyncCallbackCalled2 = false;
			var op = new AsyncCompletionSource<int>(asyncResult => asyncCallbackCalled1 = true, null);
			op.AddCompletionCallback(asyncOp => asyncCallbackCalled2 = true, AsyncContinuationOptions.None);

			// Act
			op.TrySetResult(10);

			// Assert
			Assert.True(asyncCallbackCalled1);
			Assert.True(asyncCallbackCalled2);
		}

		[Fact]
		public void TrySetResult_CallsOnCompleted()
		{
			// Arrange
			var op = new AsyncResultOverrides();

			// Act
			op.TrySetResult(10);

			// Assert
			Assert.True(op.OnCompletedCalled);
			Assert.True(op.OnStatusChangedCalled);
		}

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public void TrySetResult_SetsCompletedSynchronously(bool completedSynchronously)
		{
			// Arrange
			var result = new object();
			var op = new AsyncCompletionSource<object>();

			// Act
			op.TrySetResult(result, completedSynchronously);

			// Assert
			Assert.Equal(completedSynchronously, op.CompletedSynchronously);
		}

		[Fact]
		public void TrySetResult_FailsIfOperationIsCompleted()
		{
			// Arrange
			var op = new AsyncCompletionSource<int>(AsyncOperationStatus.Canceled);

			// Act
			var result = op.TrySetResult(10);

			// Assert
			Assert.False(result);
			Assert.True(op.CompletedSynchronously);
			AssertCanceled(op);
		}

		[Fact]
		public void TrySetResult_ThrowsIfOperationIsDisposed()
		{
			// Arrange
			var op = new AsyncCompletionSource<int>(AsyncOperationStatus.Canceled);
			op.Dispose();

			// Act/Assert
			Assert.Throws<ObjectDisposedException>(() => op.TrySetResult(15));
		}

		#endregion

		#endregion

		#region IAsyncOperationEvents

		[Fact]
		public void TryAddCompletionCallback_FailsIfOperationIsCompleted()
		{
			// Arrange
			var op = new AsyncCompletionSource();
			op.SetCanceled();

			// Act
			var result = op.TryAddCompletionCallback(_ => { }, AsyncContinuationOptions.None, null);

			// Assert
			Assert.False(result);
		}

		[Fact]
		public void TryAddCompletionCallback_FailsIfOperationIsCompletedSynchronously()
		{
			// Arrange
			var op = AsyncResult.CompletedOperation;

			// Act
			var result = op.TryAddCompletionCallback(_ => { }, AsyncContinuationOptions.None, null);

			// Assert
			Assert.False(result);
		}

		#endregion

		#region IAsyncResult

		[Fact]
		public void AsyncWaitHandle_ThrowsIfDisposed()
		{
			// Arrange
			var op = new AsyncResult(AsyncOperationStatus.Canceled);
			op.Dispose();

			// Act/Assert
			Assert.Throws<ObjectDisposedException>(() => op.AsyncWaitHandle);
		}

		#endregion

		#region IDisposable

		[Fact]
		public void Dispose_ThrowsIfOperationIsNotCompleted()
		{
			// Arrange
			var op = new AsyncResult();

			// Act/Assert
			Assert.Throws<InvalidOperationException>(() => op.Dispose());
		}

		[Fact]
		public void Dispose_CanBeCalledMultipleTimes()
		{
			// Arrange
			var op = new AsyncResult(AsyncOperationStatus.Canceled);

			// Act/Assert
			op.Dispose();
			op.Dispose();
		}

		[Fact]
		public void Dispose_CallsDispose()
		{
			// Arrange
			var op = new AsyncResultOverrides();
			op.TrySetCompleted();

			// Act
			op.Dispose();

			// Assert
			Assert.True(op.DisposeCalled);
		}

		#endregion

		#region implementation

		private void AssertNotCompleted(IAsyncOperation op, AsyncOperationStatus status)
		{
			Assert.Equal(status, op.Status);
			Assert.False(op.IsCompleted);
			Assert.False(op.IsCompletedSuccessfully);
			Assert.False(op.IsCanceled);
			Assert.False(op.IsFaulted);
			Assert.False(op.CompletedSynchronously);
			Assert.Null(op.Exception);
		}

		private void AssertCompleted(IAsyncOperation op)
		{
			Assert.Equal(AsyncOperationStatus.RanToCompletion, op.Status);
			Assert.True(op.IsCompleted);
			Assert.True(op.IsCompletedSuccessfully);
			Assert.False(op.IsCanceled);
			Assert.False(op.IsFaulted);
			Assert.Null(op.Exception);
		}

		private void AssertCompletedWithResult<T>(IAsyncOperation<T> op, T result)
		{
			Assert.Equal(AsyncOperationStatus.RanToCompletion, op.Status);
			Assert.Equal(result, op.Result);
			Assert.True(op.IsCompleted);
			Assert.True(op.IsCompletedSuccessfully);
			Assert.False(op.IsCanceled);
			Assert.False(op.IsFaulted);
			Assert.Null(op.Exception);
		}

		private void AssertCanceled(IAsyncOperation op, OperationCanceledException e)
		{
			Assert.Equal(AsyncOperationStatus.Canceled, op.Status);
			Assert.Equal(e, op.Exception.InnerException);
			Assert.True(op.IsCompleted);
			Assert.True(op.IsCanceled);
			Assert.False(op.IsCompletedSuccessfully);
			Assert.False(op.IsFaulted);
		}

		private void AssertCanceled(IAsyncOperation op)
		{
			Assert.Equal(AsyncOperationStatus.Canceled, op.Status);
			Assert.True(op.IsCompleted);
			Assert.True(op.IsCanceled);
			Assert.False(op.IsCompletedSuccessfully);
			Assert.False(op.IsFaulted);
		}

		private void AssertFaulted(IAsyncOperation op, Exception e)
		{
			Assert.Equal(AsyncOperationStatus.Faulted, op.Status);
			Assert.Equal(e, op.Exception.InnerException);
			Assert.True(op.IsCompleted);
			Assert.True(op.IsFaulted);
			Assert.False(op.IsCompletedSuccessfully);
			Assert.False(op.IsCanceled);
		}

		private void AssertFaulted(IAsyncOperation op)
		{
			Assert.Equal(AsyncOperationStatus.Faulted, op.Status);
			Assert.NotNull(op.Exception);
			Assert.True(op.IsCompleted);
			Assert.True(op.IsFaulted);
			Assert.False(op.IsCompletedSuccessfully);
			Assert.False(op.IsCanceled);
		}

		#endregion
	}
}
