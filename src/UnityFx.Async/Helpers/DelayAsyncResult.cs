// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Threading;

namespace UnityFx.Async
{
	/// <summary>
	/// Helper class used in <see cref="AsyncResult.Delay(int)"/> implementation.
	/// </summary>
	internal class DelayAsyncResult : AsyncResult
	{
		#region data

		private readonly Timer _timer;

		#endregion

		#region interface

		public DelayAsyncResult(int millisecondsDelay)
		{
			_timer = new Timer(TimerCompletionCallback, this, millisecondsDelay, Timeout.Infinite);
		}

		#endregion

		#region AsyncResult

		protected override void OnCompleted()
		{
			_timer.Dispose();
			base.OnCompleted();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				_timer.Dispose();
			}

			base.Dispose(disposing);
		}

		#endregion

		#region implementation

		private static void TimerCompletionCallback(object state)
		{
			var asyncResult = state as AsyncResult;
			asyncResult.TrySetCompleted(false);
		}

		#endregion
	}
}
