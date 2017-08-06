using System;
using System.Threading;
using NUnit.Framework;
using UnityEngine;

namespace UnityFx.Async.Tests
{
	[TestFixture]
	public class AsyncResult_InitializationTests
	{
		[Test]
		public void ConstructorTest1()
		{
			AssertStatus(new AsyncResult(), AsyncOperationStatus.Created, null, null);
			AssertStatus(new AsyncResult<int>(), AsyncOperationStatus.Created, null, null);
		}

		[Test]
		public void ConstructorTest2()
		{
			var asyncState = new object();

			AssertStatus(new AsyncResult(asyncState), AsyncOperationStatus.Created, null, asyncState);
			AssertStatus(new AsyncResult<int>(asyncState), AsyncOperationStatus.Created, null, asyncState);
		}

		[Test]
		public void ConstructorTest3()
		{
			var asyncState = new object();
			var e = new Exception();

			AssertStatus(new AsyncResult(asyncState, e), AsyncOperationStatus.Faulted, e, asyncState);
			AssertStatus(new AsyncResult<int>(asyncState, e), AsyncOperationStatus.Faulted, e, asyncState);
		}

		[Test]
		public void ConstructorTest4()
		{
			var asyncState = new object();
			var token1 = CancellationToken.None;
			var token2 = new CancellationToken(true);

			AssertStatus(new AsyncResult(asyncState, token1), AsyncOperationStatus.Created, null, asyncState);
			AssertStatus(new AsyncResult<int>(asyncState, token1), AsyncOperationStatus.Created, null, asyncState);
			AssertStatus(new AsyncResult(asyncState, token2), AsyncOperationStatus.Canceled, null, asyncState);
			AssertStatus(new AsyncResult<int>(asyncState, token2), AsyncOperationStatus.Canceled, null, asyncState);
		}

		[Test]
		public void ConstructorTest5()
		{
			var asyncState = new object();

			AssertStatus(new AsyncResult(asyncState, AsyncOperationStatus.Created), AsyncOperationStatus.Created, null, asyncState);
			AssertStatus(new AsyncResult<int>(asyncState, AsyncOperationStatus.Created), AsyncOperationStatus.Created, null, asyncState);
			AssertStatus(new AsyncResult(asyncState, AsyncOperationStatus.Running), AsyncOperationStatus.Running, null, asyncState);
			AssertStatus(new AsyncResult<int>(asyncState, AsyncOperationStatus.Running), AsyncOperationStatus.Running, null, asyncState);
			AssertStatus(new AsyncResult(asyncState, AsyncOperationStatus.RanToCompletion), AsyncOperationStatus.RanToCompletion, null, asyncState);
			AssertStatus(new AsyncResult<int>(asyncState, AsyncOperationStatus.RanToCompletion), AsyncOperationStatus.RanToCompletion, null, asyncState);
			AssertStatus(new AsyncResult(asyncState, AsyncOperationStatus.Faulted), AsyncOperationStatus.Faulted, null, asyncState);
			AssertStatus(new AsyncResult<int>(asyncState, AsyncOperationStatus.Faulted), AsyncOperationStatus.Faulted, null, asyncState);
			AssertStatus(new AsyncResult(asyncState, AsyncOperationStatus.Canceled), AsyncOperationStatus.Canceled, null, asyncState);
			AssertStatus(new AsyncResult<int>(asyncState, AsyncOperationStatus.Canceled), AsyncOperationStatus.Canceled, null, asyncState);
		}

		[Test]
		public void ConstructorTest6()
		{
			var asyncState = new object();
			var token1 = CancellationToken.None;
			var token2 = new CancellationToken(true);

			AssertStatus(new AsyncResult(asyncState, token1, AsyncOperationStatus.Created), AsyncOperationStatus.Created, null, asyncState);
			AssertStatus(new AsyncResult<int>(asyncState, token1, AsyncOperationStatus.Created), AsyncOperationStatus.Created, null, asyncState);
			AssertStatus(new AsyncResult(asyncState, token1, AsyncOperationStatus.Running), AsyncOperationStatus.Running, null, asyncState);
			AssertStatus(new AsyncResult<int>(asyncState, token1, AsyncOperationStatus.Running), AsyncOperationStatus.Running, null, asyncState);
			AssertStatus(new AsyncResult(asyncState, token1, AsyncOperationStatus.RanToCompletion), AsyncOperationStatus.RanToCompletion, null, asyncState);
			AssertStatus(new AsyncResult<int>(asyncState, token1, AsyncOperationStatus.RanToCompletion), AsyncOperationStatus.RanToCompletion, null, asyncState);
			AssertStatus(new AsyncResult(asyncState, token1, AsyncOperationStatus.Faulted), AsyncOperationStatus.Faulted, null, asyncState);
			AssertStatus(new AsyncResult<int>(asyncState, token1, AsyncOperationStatus.Faulted), AsyncOperationStatus.Faulted, null, asyncState);
			AssertStatus(new AsyncResult(asyncState, token1, AsyncOperationStatus.Canceled), AsyncOperationStatus.Canceled, null, asyncState);
			AssertStatus(new AsyncResult<int>(asyncState, token1, AsyncOperationStatus.Canceled), AsyncOperationStatus.Canceled, null, asyncState);

			AssertStatus(new AsyncResult(asyncState, token2, AsyncOperationStatus.Created), AsyncOperationStatus.Canceled, null, asyncState);
			AssertStatus(new AsyncResult<int>(asyncState, token2, AsyncOperationStatus.Created), AsyncOperationStatus.Canceled, null, asyncState);
			AssertStatus(new AsyncResult(asyncState, token2, AsyncOperationStatus.Running), AsyncOperationStatus.Canceled, null, asyncState);
			AssertStatus(new AsyncResult<int>(asyncState, token2, AsyncOperationStatus.Running), AsyncOperationStatus.Canceled, null, asyncState);
			AssertStatus(new AsyncResult(asyncState, token2, AsyncOperationStatus.RanToCompletion), AsyncOperationStatus.Canceled, null, asyncState);
			AssertStatus(new AsyncResult<int>(asyncState, token2, AsyncOperationStatus.RanToCompletion), AsyncOperationStatus.Canceled, null, asyncState);
			AssertStatus(new AsyncResult(asyncState, token2, AsyncOperationStatus.Faulted), AsyncOperationStatus.Canceled, null, asyncState);
			AssertStatus(new AsyncResult<int>(asyncState, token2, AsyncOperationStatus.Faulted), AsyncOperationStatus.Canceled, null, asyncState);
			AssertStatus(new AsyncResult(asyncState, token2, AsyncOperationStatus.Canceled), AsyncOperationStatus.Canceled, null, asyncState);
			AssertStatus(new AsyncResult<int>(asyncState, token2, AsyncOperationStatus.Canceled), AsyncOperationStatus.Canceled, null, asyncState);
		}

		[Test]
		public void ConstructorTest7()
		{
			var asyncState = new object();

			AssertStatus(new AsyncResult<int>(asyncState, 20), AsyncOperationStatus.RanToCompletion, null, asyncState, 20);
		}

		[Test]
		public void StaticInitTest()
		{
			var e = new Exception();
			var token1 = CancellationToken.None;
			var token2 = new CancellationToken(true);

			AssertStatus(AsyncResult.FromCanceled(token1), AsyncOperationStatus.Canceled, null, null);
			AssertStatus(AsyncResult.FromCanceled<int>(token1), AsyncOperationStatus.Canceled, null, null);
			AssertStatus(AsyncResult.FromCanceled(token2), AsyncOperationStatus.Canceled, null, null);
			AssertStatus(AsyncResult.FromCanceled<int>(token2), AsyncOperationStatus.Canceled, null, null);
			AssertStatus(AsyncResult.FromException(e), AsyncOperationStatus.Faulted, e, null);
			AssertStatus(AsyncResult.FromException<int>(e), AsyncOperationStatus.Faulted, e, null);
			AssertStatus(AsyncResult.FromResult(30), AsyncOperationStatus.RanToCompletion, null, null, 30);
		}

		[Test]
		public void StaticPropertyTest()
		{
			AssertStatus(AsyncResult.Completed, AsyncOperationStatus.RanToCompletion, null, null);
			AssertStatus(AsyncResult.Canceled, AsyncOperationStatus.Canceled, null, null);
		}

		private void AssertStatus(IAsyncOperation op, AsyncOperationStatus status, Exception e, object asyncState)
		{
			if (status == AsyncOperationStatus.Created || status == AsyncOperationStatus.Running)
			{
				Assert.IsFalse(op.IsCompleted);
				Assert.IsFalse(op.IsCompletedSuccessfully);
				Assert.IsFalse(op.IsFaulted);
				Assert.IsFalse(op.IsCanceled);
				Assert.IsFalse(op.CompletedSynchronously);

				Assert.AreEqual(0, op.Progress);
			}
			else
			{
				if (status == AsyncOperationStatus.RanToCompletion)
				{
					Assert.IsTrue(op.IsCompletedSuccessfully);
					Assert.IsFalse(op.IsFaulted);
					Assert.IsFalse(op.IsCanceled);
				}
				else if (status == AsyncOperationStatus.Faulted)
				{
					Assert.IsFalse(op.IsCompletedSuccessfully);
					Assert.IsTrue(op.IsFaulted);
					Assert.IsFalse(op.IsCanceled);
				}
				else if (status == AsyncOperationStatus.Canceled)
				{
					Assert.IsFalse(op.IsCompletedSuccessfully);
					Assert.IsTrue(op.IsFaulted);
					Assert.IsTrue(op.IsCanceled);
				}

				Assert.IsTrue(op.IsCompleted);
				Assert.IsTrue(op.CompletedSynchronously);

				Assert.AreEqual(1, op.Progress);
			}
			
			Assert.AreEqual(status, op.Status);
			Assert.AreEqual(asyncState, op.AsyncState);
			Assert.AreEqual(e, op.Exception);
		}

		private void AssertStatus<T>(IAsyncOperation<T> op, AsyncOperationStatus status, Exception e, object asyncState, T result = default(T))
		{
			AssertStatus((IAsyncOperation)op, status, e, asyncState);

			if (op.IsCompletedSuccessfully)
			{
				Assert.AreEqual(result, op.Result);
			}
		}
	}
}
