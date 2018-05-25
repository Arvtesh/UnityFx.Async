// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
#if !NET35
using System.Threading.Tasks;
#endif

namespace UnityFx.Async
{
	internal class AsyncContinuation
	{
		#region data

		private static WaitCallback _waitCallback;
		private static SendOrPostCallback _postCallback;

		private IAsyncOperation _op;
		private SynchronizationContext _syncContext;
		private object _continuation;

		#endregion

		#region interface

		internal AsyncContinuation(IAsyncOperation op, SynchronizationContext syncContext, object continuation)
		{
			_op = op;
			_syncContext = syncContext;
			_continuation = continuation;
		}

		internal void InvokeAsync()
		{
			if (_syncContext != null)
			{
				InvokeOnSyncContext(_syncContext);
			}
			else
			{
				var syncContext = SynchronizationContext.Current;

				if (syncContext != null)
				{
					InvokeOnSyncContext(syncContext);
				}
				else
				{
					if (_waitCallback == null)
					{
						_waitCallback = PostCallback;
					}

					ThreadPool.QueueUserWorkItem(_waitCallback, this);
				}
			}
		}

		internal void Invoke()
		{
			if (_syncContext == null || _syncContext == SynchronizationContext.Current)
			{
				InvokeInline(_op, _continuation, false);
			}
			else
			{
				InvokeOnSyncContext(_syncContext);
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

		internal static bool CanInvokeInline(IAsyncOperation op, SynchronizationContext syncContext)
		{
			return syncContext == null || syncContext == SynchronizationContext.Current;
		}

		internal static void InvokeInline(IAsyncOperation op, object continuation, bool inline)
		{
			switch (continuation)
			{
				case IAsyncContinuation c:
					c.Invoke(op, inline);
					break;

				case AsyncOperationCallback aoc:
					aoc.Invoke(op);
					break;

				case Action a:
					a.Invoke();
					break;

				case AsyncCallback ac:
					ac.Invoke(op);
					break;

				case AsyncCompletedEventHandler eh:
					eh.Invoke(op, new AsyncCompletedEventArgs(op.Exception, op.IsCanceled, op.AsyncState));
					break;
			}
		}

#if !NET35

		internal static void InvokeTaskContinuation(IAsyncOperation op, TaskCompletionSource<VoidResult> tcs)
		{
			var status = op.Status;

			if (status == AsyncOperationStatus.RanToCompletion)
			{
				tcs.TrySetResult(null);
			}
			else if (status == AsyncOperationStatus.Faulted)
			{
				tcs.TrySetException(op.Exception);
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
				tcs.TrySetException(op.Exception);
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

		private void InvokeOnSyncContext(SynchronizationContext syncContext)
		{
			Debug.Assert(_syncContext != null);

			if (_postCallback == null)
			{
				_postCallback = PostCallback;
			}

			syncContext.Post(_postCallback, this);
		}

		private static void PostCallback(object args)
		{
			var c = args as AsyncContinuation;
			InvokeInline(c._op, c._continuation, false);
		}

		#endregion
	}
}
