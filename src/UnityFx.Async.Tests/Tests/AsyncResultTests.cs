// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections;
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
			// Arrange/Act
			var op = new AsyncResult();

			// Assert
			AssertNotCompleted(op, AsyncOperationStatus.Created);
		}

		[Theory]
		[InlineData(AsyncOperationStatus.Created)]
		[InlineData(AsyncOperationStatus.Scheduled)]
		[InlineData(AsyncOperationStatus.Running)]
		public void Constructor_SetsCorrectStatus(AsyncOperationStatus status)
		{
			// Arrange/Act
			var op = new AsyncResult(status);
			var op2 = new AsyncResult(status, null, null);

			// Assert
			AssertNotCompleted(op, status);
			AssertNotCompleted(op2, status);
		}

		[Theory]
		[InlineData(AsyncOperationStatus.RanToCompletion)]
		[InlineData(AsyncOperationStatus.Faulted)]
		[InlineData(AsyncOperationStatus.Canceled)]
		public void Constructor_SetsCorrectCompletionStatus(AsyncOperationStatus status)
		{
			// Arrange/Act
			var op = new AsyncResult(status);
			var op2 = new AsyncResult(status, null);
			var op3 = new AsyncResult(status, null, null);

			// Assert
			AssertCompleted(op, status);
			Assert.True(op.CompletedSynchronously);
			AssertCompleted(op2, status);
			Assert.True(op2.CompletedSynchronously);
			AssertCompleted(op3, status);
			Assert.True(op3.CompletedSynchronously);
		}

		[Theory]
		[InlineData(AsyncOperationStatus.Created, AsyncCreationOptions.None)]
		[InlineData(AsyncOperationStatus.Scheduled, AsyncCreationOptions.RunContinuationsAsynchronously)]
		[InlineData(AsyncOperationStatus.Running, AsyncCreationOptions.SuppressCancellation)]
		[InlineData(AsyncOperationStatus.Faulted, AsyncCreationOptions.RunContinuationsAsynchronously)]
		[InlineData(AsyncOperationStatus.Canceled, AsyncCreationOptions.RunContinuationsAsynchronously | AsyncCreationOptions.SuppressCancellation)]
		public void Constructor_SetsCorrectStatusAndOptions(AsyncOperationStatus status, AsyncCreationOptions options)
		{
			// Arrange/Act
			var op = new AsyncResult(status, options);
			var op2 = new AsyncResult(status, options, null);
			var op3 = new AsyncResult(status, options, null, null);

			// Assert
			Assert.Equal(status, op.Status);
			Assert.Equal(options, op.CreationOptions);
			Assert.Equal(status, op2.Status);
			Assert.Equal(options, op2.CreationOptions);
			Assert.Equal(status, op3.Status);
			Assert.Equal(options, op3.CreationOptions);
		}

		[Fact]
		public void Constructor_SetsAsyncState()
		{
			// Arrange
			var state = new object();

			// Act
			var op = new AsyncResult(null, state);
			var op2 = new AsyncResult(AsyncOperationStatus.Scheduled, state);
			var op3 = new AsyncResult(AsyncCreationOptions.RunContinuationsAsynchronously, state);
			var op4 = new AsyncResult(AsyncOperationStatus.Faulted, AsyncCreationOptions.SuppressCancellation, state);

			// Assert
			Assert.Equal(state, op.AsyncState);
			Assert.Equal(state, op2.AsyncState);
			Assert.Equal(state, op3.AsyncState);
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

		#region IAsyncOperation

		[Fact]
		public void Id_ShouldReturnNonZeroValue()
		{
			// Arrange
			var op = new AsyncCompletionSource();

			// Act/Assert
			Assert.NotEqual(0, op.Id);
		}

		[Theory]
		[InlineData(AsyncOperationStatus.Created, 0.3f)]
		[InlineData(AsyncOperationStatus.Scheduled, 0.3f)]
		[InlineData(AsyncOperationStatus.Running, 0.3f)]
		[InlineData(AsyncOperationStatus.RanToCompletion, 1)]
		[InlineData(AsyncOperationStatus.Faulted, 0)]
		[InlineData(AsyncOperationStatus.Canceled, 0)]
		public void Progress_ReturnsCorrentValue(AsyncOperationStatus status, float expectedValue)
		{
			// Arrange
			var progress = 0.3f;
			var op = new AsyncCompletionSource(status);

			// Act
			op.TrySetProgress(progress);

			// Assert
			Assert.Equal(expectedValue, op.Progress);
		}

		[Theory]
		[InlineData(AsyncOperationStatus.Created, false)]
		[InlineData(AsyncOperationStatus.Scheduled, false)]
		[InlineData(AsyncOperationStatus.Running, false)]
		[InlineData(AsyncOperationStatus.RanToCompletion, true)]
		[InlineData(AsyncOperationStatus.Faulted, false)]
		[InlineData(AsyncOperationStatus.Canceled, false)]
		public void IsCompletedSuccessfully_ReturnsCorrentValue(AsyncOperationStatus status, bool expectedResult)
		{
			// Arrange
			var op = new AsyncResult(status);

			// Act
			var result = op.IsCompletedSuccessfully;

			// Assert
			Assert.Equal(expectedResult, result);
		}

		[Theory]
		[InlineData(AsyncOperationStatus.Created, false)]
		[InlineData(AsyncOperationStatus.Scheduled, false)]
		[InlineData(AsyncOperationStatus.Running, false)]
		[InlineData(AsyncOperationStatus.RanToCompletion, false)]
		[InlineData(AsyncOperationStatus.Faulted, true)]
		[InlineData(AsyncOperationStatus.Canceled, false)]
		public void IsFaulted_ReturnsCorrentValue(AsyncOperationStatus status, bool expectedResult)
		{
			// Arrange
			var op = new AsyncResult(status);

			// Act
			var result = op.IsFaulted;

			// Assert
			Assert.Equal(expectedResult, result);
		}

		[Theory]
		[InlineData(AsyncOperationStatus.Created, false)]
		[InlineData(AsyncOperationStatus.Scheduled, false)]
		[InlineData(AsyncOperationStatus.Running, false)]
		[InlineData(AsyncOperationStatus.RanToCompletion, false)]
		[InlineData(AsyncOperationStatus.Faulted, false)]
		[InlineData(AsyncOperationStatus.Canceled, true)]
		public void IsCancelled_ReturnsCorrentValue(AsyncOperationStatus status, bool expectedResult)
		{
			// Arrange
			var op = new AsyncResult(status);

			// Act
			var result = op.IsCanceled;

			// Assert
			Assert.Equal(expectedResult, result);
		}

		#endregion

		#region IAsyncCancellable

		[Fact]
		public void Cancel_CanBeCalledWhenDisposed()
		{
			// Arrange
			var op = new AsyncResult(AsyncOperationStatus.RanToCompletion);
			op.Dispose();

			// Act/Assert
			op.Cancel();
		}

		[Fact]
		public void Cancel_DefaultImplementationDoesNothing()
		{
			// Arrange
			var op = new AsyncResult();

			// Act
			op.Cancel();

			// Assert
			Assert.False(op.IsCompleted);
		}

		[Theory]
		[InlineData(AsyncCreationOptions.None, true)]
		[InlineData(AsyncCreationOptions.SuppressCancellation, false)]
		public void Cancel_CanBeSuppressed(AsyncCreationOptions options, bool expectedCompleted)
		{
			// Arrange
			var op = new AsyncCompletionSource(options);

			// Act
			op.Cancel();

			// Assert
			Assert.Equal(expectedCompleted, op.IsCompleted);
		}

		#endregion

		#region IAsyncResult

		[Theory]
		[InlineData(AsyncOperationStatus.Created, false)]
		[InlineData(AsyncOperationStatus.Scheduled, false)]
		[InlineData(AsyncOperationStatus.Running, false)]
		[InlineData(AsyncOperationStatus.RanToCompletion, true)]
		[InlineData(AsyncOperationStatus.Faulted, true)]
		[InlineData(AsyncOperationStatus.Canceled, true)]
		public void IsCompleted_ReturnsCorrentValue(AsyncOperationStatus status, bool expectedResult)
		{
			// Arrange
			var op = new AsyncResult(status);

			// Act
			var result = op.IsCompleted;

			// Assert
			Assert.Equal(expectedResult, result);
		}

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

		#region IEnumerator

		[Fact]
		public void Current_IsNull()
		{
			// Arrange/Act
			var op = new AsyncResult();

			// Act
			var result = (op as IEnumerator).Current;

			// Assert
			Assert.Null(result);
		}

		[Fact]
		public void MoveNext_CanBeCalledWhenDisposed()
		{
			// Arrange
			var op = new AsyncResult(AsyncOperationStatus.RanToCompletion);
			op.Dispose();

			// Act
			var result = (op as IEnumerator).MoveNext();

			// Assert
			Assert.False(result);
		}

		[Theory]
		[InlineData(AsyncOperationStatus.Created, true)]
		[InlineData(AsyncOperationStatus.Scheduled, true)]
		[InlineData(AsyncOperationStatus.Running, true)]
		[InlineData(AsyncOperationStatus.RanToCompletion, false)]
		[InlineData(AsyncOperationStatus.Faulted, false)]
		[InlineData(AsyncOperationStatus.Canceled, false)]
		public void MoveNext_ReturnsCorrectValue(AsyncOperationStatus status, bool expectedResult)
		{
			// Arrange
			var op = new AsyncResult(status);

			// Act
			var result = (op as IEnumerator).MoveNext();

			// Assert
			Assert.Equal(expectedResult, result);
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
			Assert.Null(op.Exception);
			Assert.True(op.IsCompleted);
			Assert.True(op.IsCompletedSuccessfully);
			Assert.False(op.IsCanceled);
			Assert.False(op.IsFaulted);
		}

		private void AssertCompleted(IAsyncOperation op, AsyncOperationStatus status)
		{
			if (status == AsyncOperationStatus.RanToCompletion)
			{
				Assert.Null(op.Exception);
			}
			else
			{
				Assert.NotNull(op.Exception);
			}

			Assert.True(op.IsCompleted);
			Assert.Equal(status == AsyncOperationStatus.RanToCompletion, op.IsCompletedSuccessfully);
			Assert.Equal(status == AsyncOperationStatus.Faulted, op.IsFaulted);
			Assert.Equal(status == AsyncOperationStatus.Canceled, op.IsCanceled);
			Assert.Equal(status, op.Status);
		}

		private void AssertCompletedWithResult<T>(IAsyncOperation<T> op, T result)
		{
			Assert.Equal(AsyncOperationStatus.RanToCompletion, op.Status);
			Assert.Equal(result, op.Result);
			Assert.Null(op.Exception);
			Assert.True(op.IsCompleted);
			Assert.True(op.IsCompletedSuccessfully);
			Assert.False(op.IsCanceled);
			Assert.False(op.IsFaulted);
		}

		private void AssertCanceled(IAsyncOperation op, OperationCanceledException e)
		{
			Assert.Equal(AsyncOperationStatus.Canceled, op.Status);
			Assert.Equal(e, op.Exception);
			Assert.True(op.IsCompleted);
			Assert.True(op.IsCanceled);
			Assert.False(op.IsCompletedSuccessfully);
			Assert.False(op.IsFaulted);
		}

		private void AssertCanceled(IAsyncOperation op)
		{
			Assert.Equal(AsyncOperationStatus.Canceled, op.Status);
			Assert.IsType<OperationCanceledException>(op.Exception);
			Assert.True(op.IsCompleted);
			Assert.True(op.IsCanceled);
			Assert.False(op.IsCompletedSuccessfully);
			Assert.False(op.IsFaulted);
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
