// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Threading;
#if UNITYFX_SUPPORT_TAP
using System.Threading.Tasks;
#endif

namespace UnityFx.Async
{
	internal abstract class AsyncContinuation : IAsyncContinuation
	{
		#region data

		private static SendOrPostCallback _postCallback;

		private readonly SynchronizationContext _syncContext;
		private IAsyncOperation _op;

		#endregion

		#region interface

		internal AsyncContinuation(SynchronizationContext syncContext)
		{
			_syncContext = syncContext;
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
			if (continuation is IAsyncContinuation c)
			{
				c.Invoke(op);
			}
			else
			{
				InvokeDelegate(op, continuation);
			}
		}

		internal static void InvokeDelegate(IAsyncOperation op, object continuation)
		{
			switch (continuation)
			{
				case AsyncOperationCallback aoc:
					aoc.Invoke(op);
					break;

				////case Action<IAsyncOperation> aop:
				////	aop.Invoke(op);
				////	break;

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

		protected abstract void OnInvoke(IAsyncOperation op);

		#endregion

		#region IAsyncContinuation

		public void Invoke(IAsyncOperation op)
		{
			if (_syncContext == null || _syncContext == SynchronizationContext.Current)
			{
				OnInvoke(op);
			}
			else
			{
				_op = op;

				if (_postCallback == null)
				{
					_postCallback = args =>
					{
						var c = args as AsyncContinuation;
						c.OnInvoke(c._op);
					};
				}

				_syncContext.Post(_postCallback, this);
			}
		}

		#endregion

		#region implementation
		#endregion
	}
}
