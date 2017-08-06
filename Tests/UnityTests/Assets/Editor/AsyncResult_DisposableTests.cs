using System;
using System.Threading;
using NUnit.Framework;
using UnityEngine;

namespace UnityFx.Async.Tests
{
	[TestFixture]
	public class AsyncResult_DisposableTests
	{
		[Test]
		public void NewInstanceIsNotDisposed()
		{
			var op = new AsyncResult();
			Assert.IsFalse(op.IsDisposed);
		}

		[Test]
		public void DisposeChangesOperationState()
		{
			var op = new AsyncResult();
			Assert.IsFalse(op.IsDisposed);
			op.Dispose();
			Assert.IsTrue(op.IsDisposed);
		}

		[Test]
		public void DisposeCanBeCalledMultipleTimes()
		{
			var op = new AsyncResult();
			Assert.IsFalse(op.IsDisposed);
			op.Dispose();
			Assert.IsTrue(op.IsDisposed);
			op.Dispose();
			Assert.IsTrue(op.IsDisposed);
		}

		[Test]
		public void MethodsOfDisposedOperationThrow()
		{
			var op = new AsyncResult<int>();
			op.Dispose();
			Assert.IsTrue(op.IsDisposed);

			// IAsyncOperationController
			Assert.Throws<ObjectDisposedException>(() => op.SetResult(20));
			Assert.Throws<ObjectDisposedException>(() => op.TrySetResult(20));
			Assert.Throws<ObjectDisposedException>(() => op.SetProgress(0.5f));
			Assert.Throws<ObjectDisposedException>(() => op.SetCanceled());
			Assert.Throws<ObjectDisposedException>(() => op.TrySetCanceled());
			Assert.Throws<ObjectDisposedException>(() => op.SetException(null));
			Assert.Throws<ObjectDisposedException>(() => op.TrySetException(null));
			Assert.Throws<ObjectDisposedException>(() => op.SetCompleted());
			Assert.Throws<ObjectDisposedException>(() => op.TrySetCompleted());

			// IAsyncOperation
			Assert.Throws<ObjectDisposedException>(() => { var n = op.Result; });
			Assert.DoesNotThrow(() => { var n = op.Progress; });
			Assert.DoesNotThrow(() => { var n = op.Status; });
			Assert.DoesNotThrow(() => { var n = op.Exception; });
			Assert.DoesNotThrow(() => { var n = op.IsCompletedSuccessfully; });
			Assert.DoesNotThrow(() => { var n = op.IsFaulted; });
			Assert.DoesNotThrow(() => { var n = op.IsCanceled; });

			// IAsyncResult
			Assert.Throws<ObjectDisposedException>(() => { var n = op.AsyncWaitHandle; });
			Assert.DoesNotThrow(() => { var n = op.AsyncState; });
			Assert.DoesNotThrow(() => { var n = op.CompletedSynchronously; });
			Assert.DoesNotThrow(() => { var n = op.IsCompleted; });

			// IEnumerator
			Assert.DoesNotThrow(() => { var n = op.Current; });
			Assert.DoesNotThrow(() => op.MoveNext());
			Assert.Throws<NotSupportedException>(() => op.Reset());

			// IDisposable
			Assert.DoesNotThrow(() => op.Dispose());

			// Object
			Assert.DoesNotThrow(() => op.ToString());
		}
	}
}
