// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace UnityFx.Async
{
	public class AsyncResultQueueTests
	{
		[Fact]
		public void DefaultConstructor_InitializesEmptyQueue()
		{
			// Arrange/Act
			var queue = new AsyncResultQueue<AsyncResult>();

			// Assert
			Assert.True(queue.IsEmpty);
			Assert.False(queue.Suspended);
			Assert.Empty(queue);
			Assert.Equal(0, queue.MaxCount);
			Assert.Null(queue.Current);
			Assert.False(queue.IsReadOnly);
		}

		[Fact]
		public void TryAdd_AddsNewOperation()
		{
			// Arrage
			var queue = new AsyncResultQueue<AsyncResult>();
			var op = new AsyncResult();

			// Act
			var result = queue.TryAdd(op);

			// Assert
			Assert.True(result);
			Assert.False(queue.IsEmpty);
			Assert.NotEmpty(queue);
			Assert.Equal(op, queue.Current);
			Assert.Equal(AsyncOperationStatus.Running, op.Status);
		}

		[Fact]
		public void TryAdd_IgnoresCompletedOperations()
		{
			// Arrage
			var queue = new AsyncResultQueue<AsyncResult>();
			var op = AsyncResult.CompletedOperation;

			// Act
			var result = queue.TryAdd(op);

			// Assert
			Assert.False(result);
			Assert.True(queue.IsEmpty);
			Assert.Empty(queue);
			Assert.Null(queue.Current);
		}

		[Fact]
		public void Contains_FindsExistingOperation()
		{
			// Arrage
			var op = new AsyncResult();
			var queue = new AsyncResultQueue<AsyncResult>() { op };

			// Act
			var result = queue.Contains(op);

			// Assert
			Assert.True(result);
		}

		[Fact]
		public void Contains_FailsWhenOperationIsNotInQueue()
		{
			// Arrage
			var op = new AsyncResult();
			var op2 = new AsyncResult();
			var queue = new AsyncResultQueue<AsyncResult>() { op };

			// Act
			var result = queue.Contains(op2);

			// Assert
			Assert.False(result);
		}

		[Fact]
		public void Contains_FailsWhenQueueIsEmpty()
		{
			// Arrage
			var op = new AsyncResult();
			var queue = new AsyncResultQueue<AsyncResult>();

			// Act
			var result = queue.Contains(op);

			// Assert
			Assert.False(result);
		}

		[Fact]
		public void Remove_RemovesExistingOperation()
		{
			// Arrage
			var op = new AsyncResult();
			var queue = new AsyncResultQueue<AsyncResult>() { op };

			// Act
			var result = queue.Remove(op);

			// Assert
			Assert.True(result);
			Assert.True(queue.IsEmpty);
			Assert.Empty(queue);
			Assert.Null(queue.Current);
		}

		[Fact]
		public void Remove_FailsWhenOperationIsNotInQueue()
		{
			// Arrage
			var op = new AsyncResult();
			var op2 = new AsyncResult();
			var queue = new AsyncResultQueue<AsyncResult>() { op };

			// Act
			var result = queue.Remove(op2);

			// Assert
			Assert.False(result);
			Assert.False(queue.IsEmpty);
			Assert.NotEmpty(queue);
			Assert.Equal(op, queue.Current);
		}

		[Fact]
		public void Remove_FailsWhenQueueIsEmpty()
		{
			// Arrage
			var op = new AsyncResult();
			var queue = new AsyncResultQueue<AsyncResult>();

			// Act
			var result = queue.Remove(op);

			// Assert
			Assert.False(result);
		}

		[Fact]
		public void Remove_FailsWhenOperationIsNull()
		{
			// Arrage
			var queue = new AsyncResultQueue<AsyncResult>();

			// Act
			var result = queue.Remove(null);

			// Assert
			Assert.False(result);
		}

		[Fact]
		public void Clear_RemovesAllOperations()
		{
			// Arrage
			var op = new AsyncResult();
			var op2 = new AsyncResult();
			var queue = new AsyncResultQueue<AsyncResult>() { op, op2 };

			// Act
			queue.Clear();

			// Assert
			Assert.True(queue.IsEmpty);
			Assert.Empty(queue);
			Assert.Null(queue.Current);
		}
	}
}
