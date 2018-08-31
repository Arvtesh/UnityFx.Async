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
		}

		[Fact]
		public void Add_AddsNewOperation()
		{
			// Arrage
			var queue = new AsyncResultQueue<AsyncResult>();
			var op = new AsyncResult();

			// Act
			var result = queue.Add(op);

			// Assert
			Assert.True(result);
			Assert.False(queue.IsEmpty);
			Assert.NotEmpty(queue);
			Assert.Equal(op, queue.Current);
			Assert.Equal(AsyncOperationStatus.Running, op.Status);
		}

		[Fact]
		public void Add_ThrowsOnCompletedOperations()
		{
			// Arrage
			var queue = new AsyncResultQueue<AsyncResult>();
			var op = AsyncResult.CompletedOperation;

			// Act/Assert
			Assert.Throws<InvalidOperationException>(() => queue.Add(op));
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
