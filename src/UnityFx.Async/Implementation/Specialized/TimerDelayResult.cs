// Copyright (c) 2018-2020 Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Threading;

namespace UnityFx.Async
{
	internal sealed class TimerDelayResult : AsyncResult
	{
		#region data

		private readonly Timer _timer;

		#endregion

		#region interface

		public TimerDelayResult(int millisecondsDelay)
		{
			_timer = new Timer(
				state => (state as AsyncResult).TrySetCompleted(),
				this,
				millisecondsDelay,
				Timeout.Infinite);
		}

		#endregion

		#region AsyncResult

		protected override void OnCancel()
		{
			TrySetCanceled();
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
