// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Diagnostics;
using System.Threading;

namespace UnityFx.Async
{
	internal class RetryResult<T> : AsyncResult<T>
	{
		#region data

		private readonly object _opFactory;
		private readonly AsyncOperationCallback _opCompletionCallback;
		private readonly int _millisecondsRetryDelay;
		private readonly int _maxRetryCount;

		private Timer _timer;
		private TimerCallback _timerCallback;
		private IAsyncOperation _lastOp;
		private int _numberOfRetriesLeft;

		#endregion

		#region interface

		internal RetryResult(object opFactory, int millisecondsRetryDelay, int maxRetryCount)
			: base(AsyncOperationStatus.Running)
		{
			_opFactory = opFactory;
			_millisecondsRetryDelay = millisecondsRetryDelay;
			_maxRetryCount = maxRetryCount;
			_opCompletionCallback = OnOperationCompleted;
			_numberOfRetriesLeft = maxRetryCount;

			StartOperation(true);
		}

		#endregion

		#region AsyncResult

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				_timer?.Dispose();
				_timer = null;
			}

			base.Dispose(disposing);
		}

		#endregion

		#region implementation

		private void StartOperation(bool calledFromConstructor)
		{
			try
			{
				IAsyncOperation op;

				if (_opFactory is Func<IAsyncOperation> f1)
				{
					op = f1();
				}
				else if (_opFactory is Func<IAsyncOperation<T>> f2)
				{
					op = f2();
				}
				else
				{
					throw new InvalidOperationException("Invalid delegate type.");
				}

				if (!op.TryAddCompletionCallback(_opCompletionCallback, null))
				{
					if (op.IsCompletedSuccessfully)
					{
						TrySetCompleted(op, calledFromConstructor);
					}
					else
					{
						TrySetException(op.Exception, calledFromConstructor);
					}
				}
			}
			catch (Exception e)
			{
				TrySetException(e, calledFromConstructor);
			}
		}

		private void OnOperationCompleted(IAsyncOperation op)
		{
			_lastOp = op;

			if (op.IsCompletedSuccessfully)
			{
				TrySetCompleted(op, false);
			}
			else if (_millisecondsRetryDelay > 0)
			{
				if (_timerCallback == null)
				{
					_timerCallback = OnTimer;
				}

				if (_timer == null)
				{
					_timer = new Timer(_timerCallback, null, _millisecondsRetryDelay, Timeout.Infinite);
				}
				else
				{
					_timer.Change(_millisecondsRetryDelay, Timeout.Infinite);
				}
			}
			else
			{
				OnTimer(null);
			}
		}

		private void OnTimer(object args)
		{
			if (_maxRetryCount == 0 || --_numberOfRetriesLeft > 0)
			{
				StartOperation(false);
			}
			else if (_lastOp.IsFaulted)
			{
				TrySetException(_lastOp.Exception, false);
			}
			else if (_lastOp.IsCanceled)
			{
				TrySetCanceled(false);
			}
			else
			{
				// NOTE: should not get here.
				TrySetException(new Exception("Maximum number of retries exceeded."), false);
			}
		}

		private void TrySetCompleted(IAsyncOperation op, bool completedSynchronously)
		{
			Debug.Assert(op.IsCompletedSuccessfully);

			if (op is IAsyncOperation<T> rop)
			{
				TrySetResult(rop.Result, completedSynchronously);
			}
			else
			{
				TrySetCompleted(completedSynchronously);
			}
		}

		#endregion
	}
}
