// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Threading;
#if UNITYFX_SUPPORT_TAP
using System.Threading.Tasks;
#endif

namespace UnityFx.Async
{
	internal class AsyncContinuation
	{
		#region data

		private static SendOrPostCallback _postCallback;

		private readonly AsyncResult _op;
		private readonly SynchronizationContext _syncContext;
		private readonly object _continuation;

		#endregion

		#region interface

		internal AsyncContinuation(AsyncResult op, SynchronizationContext syncContext, object continuation)
		{
			_op = op;
			_syncContext = syncContext;
			_continuation = continuation;
		}

		internal void Invoke()
		{
			if (_syncContext == null || _syncContext == SynchronizationContext.Current)
			{
				InvokeInternal(_op, _continuation);
			}
			else
			{
				if (_postCallback == null)
				{
					_postCallback = args =>
					{
						var c = args as AsyncContinuation;
						InvokeInternal(c._op, c._continuation);
					};
				}

				_syncContext.Post(_postCallback, this);
			}
		}

		internal static bool CanInvoke(IAsyncOperation op, AsyncContinuationOptions options)
		{
			if (op.IsCompletedSuccessfully)
			{
				return (options & AsyncContinuationOptions.NotOnRanToCompletion) == 0;
			}

			if (op.IsFaulted)
			{
				return (options & AsyncContinuationOptions.NotOnFaulted) == 0;
			}

			return (options & AsyncContinuationOptions.NotOnCanceled) == 0;
		}

		internal static void Invoke(IAsyncOperation op, object continuation)
		{
			if (continuation is AsyncContinuation c)
			{
				c.Invoke();
			}
			else
			{
				InvokeInternal(op, continuation);
			}
		}

#if UNITYFX_SUPPORT_TAP

		internal static void InvokeTaskContinuation(IAsyncOperation op, TaskCompletionSource<object> tcs)
		{
			var status = op.Status;

			if (status == AsyncOperationStatus.RanToCompletion)
			{
				tcs.TrySetResult(null);
			}
			else if (status == AsyncOperationStatus.Faulted)
			{
				tcs.TrySetException(op.Exception.InnerExceptions);
			}
			else if (status == AsyncOperationStatus.Canceled)
			{
				tcs.TrySetCanceled();
			}
		}

		internal static void InvokeTaskContinuation<T>(IAsyncOperation<T> op, TaskCompletionSource<T> tcs)
		{
			var status = op.Status;

			if (status == AsyncOperationStatus.RanToCompletion)
			{
				tcs.TrySetResult(op.Result);
			}
			else if (status == AsyncOperationStatus.Faulted)
			{
				tcs.TrySetException(op.Exception.InnerExceptions);
			}
			else if (status == AsyncOperationStatus.Canceled)
			{
				tcs.TrySetCanceled();
			}
		}

#endif

		#endregion

		#region Object

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(obj, _continuation))
			{
				return true;
			}

			return base.Equals(obj);
		}

		public override int GetHashCode()
		{
			return _continuation.GetHashCode();
		}

		#endregion

		#region implementation

		private static void InvokeInternal(IAsyncOperation op, object continuation)
		{
			switch (continuation)
			{
				case AsyncOperationCallback aoc:
					aoc.Invoke(op);
					break;

				case Action a:
					a.Invoke();
					break;

				case AsyncCallback ac:
					ac.Invoke(op);
					break;

				case EventHandler eh:
					eh.Invoke(op, EventArgs.Empty);
					break;
			}
		}

		#endregion
	}
}
