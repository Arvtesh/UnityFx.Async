// Copyright (c) 2018-2020 Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Threading;

namespace UnityFx.Async
{
	internal sealed class TimerRetryResult<T> : RetryResult<T>
	{
		#region data

		private Timer _timer;
		private TimerCallback _timerCallback;

		#endregion

		#region interface

		internal TimerRetryResult(object opFactory, int millisecondsRetryDelay, int maxRetryCount)
			: base(opFactory, millisecondsRetryDelay, maxRetryCount)
		{
		}

		#endregion

		#region RetryResult

		protected override void BeginWait(int millisecondsDelay)
		{
			if (_timerCallback == null)
			{
				_timerCallback = args => (args as TimerRetryResult<T>).EndWait();
			}

			if (_timer == null)
			{
				_timer = new Timer(_timerCallback, this, millisecondsDelay, Timeout.Infinite);
			}
			else
			{
				_timer.Change(millisecondsDelay, Timeout.Infinite);
			}
		}

		#endregion

		#region AsyncResult

		protected override void OnCompleted()
		{
			base.OnCompleted();

			if (_timer != null)
			{
				_timer.Dispose();
				_timer = null;
			}
		}

		#endregion
	}
}
