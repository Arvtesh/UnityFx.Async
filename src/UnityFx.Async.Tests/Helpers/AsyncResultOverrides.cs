// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;

namespace UnityFx.Async
{
	public class AsyncResultOverrides : AsyncResult<int>
	{
		public bool OnStatusChangedCalled { get; private set; }
		public Exception OnCompletedException { get; private set; }
		public bool OnCompletedCalled { get; private set; }
		public bool DisposeCalled { get; private set; }

		public bool TrySetCanceled() => TrySetCanceled(false);
		public bool TrySetException(Exception e) => TrySetException(e, false);
		public bool TrySetCompleted() => TrySetCompleted(false);
		public bool TrySetResult(int result) => TrySetResult(result, false);

		protected override void OnStatusChanged(AsyncOperationStatus status)
		{
			OnStatusChangedCalled = true;

			base.OnStatusChanged(status);
		}

		protected override void OnCompleted()
		{
			OnCompletedException = Exception?.InnerException;
			OnCompletedCalled = true;

			base.OnCompleted();
		}

		protected override void Dispose(bool disposing)
		{
			DisposeCalled = true;

			base.Dispose(disposing);
		}
	}
}
