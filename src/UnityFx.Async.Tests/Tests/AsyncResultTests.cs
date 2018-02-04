// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
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
		#endregion

		#region extensions
		#endregion

		#region IAsyncCompletionSource

		#region SetCanceled/TrySetCanceled

		[Fact]
		public void SetCanceled_ThrowsIfOperationIsCompleted()
		{
			// Arrange
			var op = new AsyncResult(AsyncOperationStatus.RanToCompletion);

			// Act/Assert
			Assert.Throws<InvalidOperationException>(() => op.SetCanceled(false));
			Assert.True(op.CompletedSynchronously);
			AssertCompleted(op);
		}

		[Fact]
		public void TrySetCanceled_SetsStatusToCanceled()
		{
			// Arrange
			var op = new AsyncResult();

			// Act
			var result = op.TrySetCanceled(true);

			// Assert
			AssertCanceled(op);
			Assert.True(result);
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

		[Fact]
		public void SetException_ThrowsIfOperationIsCompleted()
		{
			// Arrange
			var e = new Exception();
			var op = new AsyncResult(AsyncOperationStatus.RanToCompletion);

			// Act/Assert
			Assert.Throws<InvalidOperationException>(() => op.SetException(e, false));
			Assert.True(op.CompletedSynchronously);
			AssertCompleted(op);
		}

		[Fact]
		public void TrySetException_SetsStatusToFaulted()
		{
			// Arrange
			var e = new Exception();
			var op = new AsyncResult();

			// Act
			var result = op.TrySetException(e, true);

			// Assert
			AssertFaulted(op, e);
			Assert.True(result);
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

		[Fact]
		public void SetCompleted_ThrowsIfOperationIsCompleted()
		{
			// Arrange
			var op = new AsyncResult(AsyncOperationStatus.Canceled);

			// Act/Assert
			Assert.Throws<InvalidOperationException>(() => op.SetCompleted(false));
			Assert.True(op.CompletedSynchronously);
			AssertCanceled(op);
		}

		[Fact]
		public void TrySetCompleted_SetsStatusToRanToCompletion()
		{
			// Arrange
			var op = new AsyncResult();

			// Act
			var result = op.TrySetCompleted(true);

			// Assert
			AssertCompleted(op);
			Assert.True(result);
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

		[Fact]
		public void SetResult_ThrowsIfOperationIsCompleted()
		{
			// Arrange
			var result = new object();
			var op = new AsyncResult<object>(AsyncOperationStatus.Canceled);

			// Act/Assert
			Assert.Throws<InvalidOperationException>(() => op.SetResult(result, false));
			Assert.True(op.CompletedSynchronously);
			AssertCanceled(op);
		}

		[Fact]
		public void TrySetResult_SetsStatusToRanToCompletion()
		{
			// Arrange
			var resultValue = new object();
			var op = new AsyncResult<object>();

			// Act
			var result = op.TrySetResult(resultValue, true);

			// Assert
			AssertCompletedWithResult(op, resultValue);
			Assert.True(result);
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

		#region IAsyncOperationEvents
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
