// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;

namespace UnityFx.Async
{
	internal static class CallbackUtility
	{
		#region data
		#endregion

		#region interface

		public static void InvokeCompletionCallback(IAsyncOperation op, object continuation, bool inline)
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

		public static void InvokeCompletionCallbackAsync(IAsyncOperation op, object continuation, SynchronizationContext syncContext, bool inline)
		{
			if (syncContext != null && syncContext.GetType() != typeof(SynchronizationContext))
			{
				syncContext.Post(args => InvokeCompletionCallback(op, args, inline), continuation);
			}
			else
			{
				ThreadPool.QueueUserWorkItem(args => InvokeCompletionCallback(op, args, inline), continuation);
			}
		}

		public static void InvokeProgressCallback(IAsyncOperation op, object callback)
		{
			switch (callback)
			{
#if !NET35
				case IProgress<float> p:
					p.Report(op.Progress);
					break;
#endif

				case AsyncOperationCallback ac:
					ac.Invoke(op);
					break;

				case ProgressChangedEventHandler ph:
					ph.Invoke(op, new ProgressChangedEventArgs((int)(op.Progress * 100), op.AsyncState));
					break;
			}
		}

		#endregion

		#region implementation
		#endregion
	}
}
