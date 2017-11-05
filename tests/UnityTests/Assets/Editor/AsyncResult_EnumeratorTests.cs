using System;
using System.Threading;
using NUnit.Framework;
using UnityEngine;

namespace UnityFx.Async.Tests
{
	[TestFixture]
	public class AsyncResult_EnumeratorTests
	{
		[Test]
		public void ResetThrowsNotSupportedException()
		{
			var op = new AsyncResult();
			Assert.Throws<NotSupportedException>(() => op.Reset());
		}

		[Test]
		public void CurrentIsNull()
		{
			var op = new AsyncResult();
			Assert.IsNull(op.Current);
		}

		[Test]
		public void MoveNextChangesOperationStatusToRunning()
		{
			var op = new AsyncResult();
			Assert.AreEqual(AsyncOperationStatus.Created, op.Status);
			Assert.IsTrue(op.MoveNext());
			Assert.AreEqual(AsyncOperationStatus.Running, op.Status);
			Assert.IsTrue(op.MoveNext());
			Assert.AreEqual(AsyncOperationStatus.Running, op.Status);
		}
	}
}
