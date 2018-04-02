// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Threading;

namespace UnityFx.Async
{
	internal class ThenResult<T, U> : AsyncResult<U>, IAsyncContinuation
	{
		#region data

		private static SendOrPostCallback _postCallback;

		private readonly SynchronizationContext _syncContext;
		private readonly object _successCallback;
		private readonly Action<Exception> _errorCallback;
		private IAsyncOperation _op;

		#endregion

		#region interface

		public ThenResult(object successCallback, Action<Exception> errorCallback)
			: base(AsyncOperationStatus.Running)
		{
			_syncContext = SynchronizationContext.Current;
			_successCallback = successCallback;
			_errorCallback = errorCallback;
		}

		protected bool InvokeSuccessCallback(IAsyncOperation op)
		{
			var result = false;

			switch (_successCallback)
			{
				case Action a:
					a.Invoke();
					result = true;
					break;

				case Action<T> a1:
					a1.Invoke((op as IAsyncOperation<T>).Result);
					result = true;
					break;

				case Func<IAsyncOperation<U>> f3:
					f3().AddCompletionCallback(op2 => TryCopyCompletionState(op2 as IAsyncOperation<U>, false), null);
					break;

				case Func<IAsyncOperation> f1:
					f1().AddCompletionCallback(op2 => TryCopyCompletionState(op2, false), null);
					break;

				case Func<T, IAsyncOperation<U>> f4:
					f4((op as IAsyncOperation<T>).Result).AddCompletionCallback(op2 => TryCopyCompletionState(op2 as IAsyncOperation<U>, false), null);
					break;

				case Func<T, IAsyncOperation> f2:
					f2((op as IAsyncOperation<T>).Result).AddCompletionCallback(op2 => TryCopyCompletionState(op2, false), null);
					break;

				default:
					// Should not get here.
					throw new InvalidOperationException();
			}

			return result;
		}

		protected void InvokeErrorCallback(IAsyncOperation op)
		{
			_errorCallback?.Invoke(op.Exception.InnerException);
		}

		#endregion

		#region IAsyncContinuation

		public void Invoke(IAsyncOperation op, bool completedSynchronously)
		{
			if (_syncContext == null || _syncContext == SynchronizationContext.Current)
			{
				InvokeCallbacks(op, completedSynchronously);
			}
			else if (op.IsCompletedSuccessfully || _errorCallback != null)
			{
				_op = op;

				if (_postCallback == null)
				{
					_postCallback = args =>
					{
						var c = args as ThenResult<T, U>;
						c.InvokeCallbacks(c._op, false);
					};
				}

				_syncContext.Post(_postCallback, this);
			}
			else
			{
				TrySetException(op.Exception, completedSynchronously);
			}
		}

		#endregion

		#region implementation

		private void InvokeCallbacks(IAsyncOperation op, bool completedSynchronously)
		{
			try
			{
				if (op.IsCompletedSuccessfully)
				{
					if (InvokeSuccessCallback(op))
					{
						TrySetCompleted(completedSynchronously);
					}
				}
				else
				{
					InvokeErrorCallback(op);
					TrySetException(op.Exception, completedSynchronously);
				}
			}
			catch (Exception e)
			{
				TrySetException(e, completedSynchronously);
			}
		}

		#endregion
	}
}
