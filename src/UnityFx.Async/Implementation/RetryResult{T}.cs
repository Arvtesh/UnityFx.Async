// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Threading;

namespace UnityFx.Async
{
	internal class RetryResult<T> : AsyncResult<T>
	{
		#region data

		private readonly Func<IAsyncOperation<T>> _opFactory;
		private readonly AsyncResult<T> _op;
		private readonly AsyncOperationCallback _opCompletionCallback;
		private readonly int _millisecondsRetryDelay;
		private readonly int _maxRetryCount;

		private Timer _timer;
		private TimerCallback _timerCallback;
		private IAsyncOperation _lastOp;
		private int _numberOfRetriesLeft;

		#endregion

		#region interface

		public RetryResult(Func<IAsyncOperation<T>> opFactory, int millisecondsRetryDelay, int maxRetryCount)
			: base(AsyncOperationStatus.Running)
		{
			_opFactory = opFactory;
			_millisecondsRetryDelay = millisecondsRetryDelay;
			_maxRetryCount = maxRetryCount;
			_opCompletionCallback = OnOperationCompleted;
			_numberOfRetriesLeft = maxRetryCount;

			StartOperation(true);
		}

		public RetryResult(AsyncResult<T> op, int millisecondsRetryDelay, int maxRetryCount)
			: base(AsyncOperationStatus.Running)
		{
			_op = op;
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
				var op = _op ?? _opFactory();

				if (_op != null)
				{
					_op.TryStart();
				}

				if (!op.TryAddCompletionCallback(_opCompletionCallback, null))
				{
					if (op.IsCompletedSuccessfully)
					{
						TrySetResult(op.Result, calledFromConstructor);
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
				TrySetResult((op as IAsyncOperation<T>).Result, false);
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
				if (_op != null)
				{
					try
					{
						_op.Reset();
						StartOperation(false);
					}
					catch (Exception e)
					{
						TrySetException(e, false);
					}
				}
				else
				{
					StartOperation(false);
				}
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

		#endregion
	}
}
