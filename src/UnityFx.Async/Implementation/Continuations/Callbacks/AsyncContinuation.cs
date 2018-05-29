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
	internal class AsyncContinuation : AsyncInvokable
	{
		#region interface

		internal AsyncContinuation(IAsyncOperation op, SynchronizationContext syncContext, object continuation)
			: base(op, syncContext, continuation)
		{
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

		internal static void InvokeInline(IAsyncOperation op, object continuation, bool inline)
		{
			switch (continuation)
			{
#if !NET35
				case IProgress<float> p:
					p.Report(op.Progress);
					break;
#endif

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

				case ProgressChangedEventHandler ph:
					ph.Invoke(op, new ProgressChangedEventArgs((int)(op.Progress * 100), op.AsyncState));
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

		#region AsyncInvokable

		protected override void Invoke(IAsyncOperation op, object continuation)
		{
			InvokeInline(op, continuation, false);
		}

		#endregion
	}
}
