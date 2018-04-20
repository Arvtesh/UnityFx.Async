// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Diagnostics;
using System.Threading;
#if UNITYFX_SUPPORT_TAP
using System.Threading.Tasks;
#endif

namespace UnityFx.Async
{
	internal class AsyncContinuation : IAsyncContinuation
	{
		#region data

		private static WaitCallback _waitCallback;
		private static SendOrPostCallback _postCallback;

		private SynchronizationContext _syncContext;
		private object _continuation;
		private bool _runAsynchronously;
		private IAsyncOperation _op;

		#endregion

		#region interface

		internal AsyncContinuation(SynchronizationContext syncContext, object continuation, bool runAsynchronously)
		{
			_syncContext = syncContext;
			_continuation = continuation;
			_runAsynchronously = runAsynchronously;
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

		internal static void InvokeDelegate(IAsyncOperation op, object continuation)
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

#if UNITYFX_SUPPORT_TAP

		internal static void InvokeTaskContinuation(IAsyncOperation op, TaskCompletionSource<VoidResult> tcs)
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

		#region IAsyncContinuation

		public void Invoke(IAsyncOperation op)
		{
			////if (CanInvokeInline(op, _syncContext))
			////{
			////	InvokeDelegate(op, _continuation);
			////}
			////else
			////{
			////	if (_syncContext != null)
			////	{
			////		InvokeOnSyncContext(op, _syncContext);
			////	}
			////	else if (SynchronizationContext.Current != null)
			////	{
			////		InvokeOnSyncContext(op, SynchronizationContext.Current);
			////	}
			////	else
			////	{
			////		InvokeAsync(op);
			////	}
			////}

			if (_syncContext == null || _syncContext == SynchronizationContext.Current)
			{
				InvokeDelegate(op, _continuation);
			}
			else
			{
				InvokeOnSyncContext(op, _syncContext);
			}
		}

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

		private void InvokeOnSyncContext(IAsyncOperation op, SynchronizationContext syncContext)
		{
			Debug.Assert(_syncContext != null);

			_op = op;

			if (_postCallback == null)
			{
				_postCallback = PostCallback;
			}

			syncContext.Post(_postCallback, this);
		}

		private void InvokeAsync(IAsyncOperation op)
		{
			_op = op;

			if (_waitCallback == null)
			{
				_waitCallback = PostCallback;
			}

			ThreadPool.QueueUserWorkItem(_waitCallback, this);
		}

		private static void PostCallback(object args)
		{
			var c = args as AsyncContinuation;
			InvokeDelegate(c._op, c._continuation);
		}

		#endregion
	}
}
