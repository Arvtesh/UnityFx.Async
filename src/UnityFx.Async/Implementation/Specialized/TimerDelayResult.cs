// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Threading;

namespace UnityFx.Async
{
	internal class TimerDelayResult : AsyncResult
	{
		#region data

		private readonly Timer _timer;

		#endregion

		#region interface

		public TimerDelayResult(int millisecondsDelay)
			: base(AsyncOperationStatus.Running)
		{
			_timer = new Timer(
				state => (state as AsyncResult).TrySetCompleted(false),
				this,
				millisecondsDelay,
				Timeout.Infinite);
		}

		#endregion

		#region AsyncResult

		protected override void OnCancel()
		{
			TrySetCanceled(false);
		}

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
	}
}
