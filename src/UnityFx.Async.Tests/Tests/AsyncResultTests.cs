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
		public void Constructor_SetsException()
		{
			// Arrange
			var e = new Exception();

			// Act
			var op = new AsyncResult(e);

			// Assert
			AssertFaulted(op, e);
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
		public void FromException_ReturnsFailedOperation_Generic()
		{
			// Arrange
			var e = new InvalidCastException();

			// Act
			var op = AsyncResult.FromException<int>(e);

			// Assert
			AssertFaulted(op, e);
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

		#endregion

		#region interface

		#region SetScheduled

		[Theory]
		[InlineData(AsyncOperationStatus.Created)]
		public void SetScheduled_SetsStatusToScheduled(AsyncOperationStatus status)
		{
			// Arrange
			var op = new AsyncResult(status);

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
			var op = new AsyncResult(status);

			// Act/Assert
			Assert.Throws<InvalidOperationException>(() => op.SetScheduled());
		}

		[Fact]
		public void SetScheduled_ThrowsIfOperationIsDisposed()
		{
			// Arrange
			var op = new AsyncResult(AsyncOperationStatus.RanToCompletion);
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
			var op = new AsyncResult(status);

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
			var op = new AsyncResult(status);

			// Act/Assert
			Assert.Throws<InvalidOperationException>(() => op.SetRunning());
		}

		[Fact]
		public void SetRunning_ThrowsIfOperationIsDisposed()
		{
			// Arrange
			var op = new AsyncResult(AsyncOperationStatus.RanToCompletion);
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
			var op = new AsyncResult();
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
			var op = new AsyncResult(status);

			// Act/Assert
			Assert.Throws<InvalidOperationException>(() => op.SetCanceled(false));
			Assert.True(op.CompletedSynchronously);
		}

		[Theory]
		[InlineData(AsyncOperationStatus.Created)]
		[InlineData(AsyncOperationStatus.Scheduled)]
		[InlineData(AsyncOperationStatus.Running)]
		public void TrySetCanceled_SetsStatusToCanceled(AsyncOperationStatus status)
		{
			// Arrange
			var op = new AsyncResult(status);

			// Act
			var result = op.TrySetCanceled(true);

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
			var op = new AsyncResult(asyncResult => asyncCallbackCalled1 = true, null);
			op.AddCompletionCallback(() => asyncCallbackCalled2 = true, false);

			// Act
			op.TrySetCanceled(true);

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
			op.TrySetCanceled(true);

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
			var op = new AsyncResult();

			// Act
			op.TrySetCanceled(completedSynchronously);

			// Assert
			Assert.Equal(completedSynchronously, op.CompletedSynchronously);
		}

		[Fact]
		public void TrySetCanceled_FailsIfOperationIsCompleted()
		{
			// Arrange
			var op = new AsyncResult(AsyncOperationStatus.RanToCompletion);

			// Act
			var result = op.TrySetCanceled(false);

			// Assert
			Assert.False(result);
			Assert.True(op.CompletedSynchronously);
			AssertCompleted(op);
		}

		[Fact]
		public void TrySetCanceled_ThrowsIfOperationIsDisposed()
		{
			// Arrange
			var op = new AsyncResult(AsyncOperationStatus.RanToCompletion);
			op.Dispose();

			// Act/Assert
			Assert.Throws<ObjectDisposedException>(() => op.TrySetCanceled(true));
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
			var op = new AsyncResult(status);

			// Act/Assert
			Assert.Throws<InvalidOperationException>(() => op.SetException(e, false));
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
			var op = new AsyncResult(status);

			// Act
			var result = op.TrySetException(e, true);

			// Assert
			AssertFaulted(op, e);
			Assert.True(result);
		}

		[Fact]
		public void TrySetException_RaisesCompletionCallbacks()
		{
			// Arrange
			var e = new Exception();
			var asyncCallbackCalled1 = false;
			var asyncCallbackCalled2 = false;
			var op = new AsyncResult(asyncResult => asyncCallbackCalled1 = true, null);
			op.AddCompletionCallback(() => asyncCallbackCalled2 = true, false);

			// Act
			op.TrySetException(e, true);

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
			op.TrySetException(e, true);

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
			var op = new AsyncResult();

			// Act
			op.TrySetException(e, completedSynchronously);

			// Assert
			Assert.Equal(completedSynchronously, op.CompletedSynchronously);
		}

		[Fact]
		public void TrySetException_FailsIfOperationIsCompleted()
		{
			// Arrange
			var op = new AsyncResult(AsyncOperationStatus.RanToCompletion);

			// Act
			var result = op.TrySetCanceled(true);

			// Assert
			Assert.False(result);
			Assert.True(op.CompletedSynchronously);
			AssertCompleted(op);
		}

		[Fact]
		public void TrySetException_ThrowsIfExceptionIsNull()
		{
			// Arrange
			var op = new AsyncResult();

			// Act/Assert
			Assert.Throws<ArgumentNullException>(() => op.TrySetException(null, false));
		}

		[Fact]
		public void TrySetException_ThrowsIfOperationIsDisposed()
		{
			// Arrange
			var e = new Exception();
			var op = new AsyncResult(AsyncOperationStatus.RanToCompletion);
			op.Dispose();

			// Act/Assert
			Assert.Throws<ObjectDisposedException>(() => op.TrySetException(e, false));
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
			var op = new AsyncResult(status);

			// Act/Assert
			Assert.Throws<InvalidOperationException>(() => op.SetCompleted(false));
			Assert.True(op.CompletedSynchronously);
		}

		[Theory]
		[InlineData(AsyncOperationStatus.Created)]
		[InlineData(AsyncOperationStatus.Scheduled)]
		[InlineData(AsyncOperationStatus.Running)]
		public void TrySetCompleted_SetsStatusToRanToCompletion(AsyncOperationStatus status)
		{
			// Arrange
			var op = new AsyncResult(status);

			// Act
			var result = op.TrySetCompleted(true);

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
			var op = new AsyncResult(asyncResult => asyncCallbackCalled1 = true, null);
			op.AddCompletionCallback(() => asyncCallbackCalled2 = true, false);

			// Act
			op.TrySetCompleted(false);

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
			op.TrySetCompleted(false);

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
			var op = new AsyncResult();

			// Act
			op.TrySetCompleted(completedSynchronously);

			// Assert
			Assert.Equal(completedSynchronously, op.CompletedSynchronously);
		}

		[Fact]
		public void TrySetCompleted_FailsIfOperationIsCompleted()
		{
			// Arrange
			var op = new AsyncResult(AsyncOperationStatus.Canceled);

			// Act
			var result = op.TrySetCompleted(false);

			// Assert
			Assert.False(result);
			Assert.True(op.CompletedSynchronously);
			AssertCanceled(op);
		}

		[Fact]
		public void TrySetCompleted_ThrowsIfOperationIsDisposed()
		{
			// Arrange
			var op = new AsyncResult(AsyncOperationStatus.Canceled);
			op.Dispose();

			// Act/Assert
			Assert.Throws<ObjectDisposedException>(() => op.TrySetCompleted(true));
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
			var op = new AsyncResult<int>(status);

			// Act/Assert
			Assert.Throws<InvalidOperationException>(() => op.SetResult(10, false));
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
			var op = new AsyncResult<object>(status);

			// Act
			var result = op.TrySetResult(resultValue, true);

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
			var op = new AsyncResult<int>(asyncResult => asyncCallbackCalled1 = true, null);
			op.AddCompletionCallback(() => asyncCallbackCalled2 = true, false);

			// Act
			op.TrySetResult(10, false);

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
			op.TrySetResult(10, true);

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
			var op = new AsyncResult<object>();

			// Act
			op.TrySetResult(result, completedSynchronously);

			// Assert
			Assert.Equal(completedSynchronously, op.CompletedSynchronously);
		}

		[Fact]
		public void TrySetResult_FailsIfOperationIsCompleted()
		{
			// Arrange
			var op = new AsyncResult<int>(AsyncOperationStatus.Canceled);

			// Act
			var result = op.TrySetResult(10, true);

			// Assert
			Assert.False(result);
			Assert.True(op.CompletedSynchronously);
			AssertCanceled(op);
		}

		[Fact]
		public void TrySetResult_ThrowsIfOperationIsDisposed()
		{
			// Arrange
			var op = new AsyncResult<int>(AsyncOperationStatus.Canceled);
			op.Dispose();

			// Act/Assert
			Assert.Throws<ObjectDisposedException>(() => op.TrySetResult(15, false));
		}

		#endregion

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
			op.SetCompleted(false);

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

		private void AssertCanceled(IAsyncOperation op)
		{
			Assert.Equal(AsyncOperationStatus.Canceled, op.Status);
			Assert.True(op.IsCompleted);
			Assert.True(op.IsCanceled);
			Assert.False(op.IsCompletedSuccessfully);
			Assert.False(op.IsFaulted);
			Assert.Null(op.Exception);
		}

		private void AssertFaulted(IAsyncOperation op, Exception e)
		{
			Assert.Equal(AsyncOperationStatus.Faulted, op.Status);
			Assert.Equal(e, op.Exception);
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
