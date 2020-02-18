// Copyright (c) 2018-2020 Alexander Bogarsukov.
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

#if !DEBUG
		[DebuggerHidden]
#endif
		public static void InvokeCompletionCallback(IAsyncOperation op, object continuation)
		{
			Debug.Assert(op != null);
			Debug.Assert(continuation != null);

			switch (continuation)
			{
				case IAsyncContinuation c:
					c.Invoke(op);
					break;

				case Action<IAsyncOperation> a:
					a.Invoke(op);
					break;

				case Action a:
					a.Invoke();
					break;

				case AsyncCallback a:
					a.Invoke(op);
					break;

				case AsyncCompletedEventHandler eh:
					eh.Invoke(op, new AsyncCompletedEventArgs(op.Exception, op.IsCanceled, op.AsyncState));
					break;
			}
		}

#if !DEBUG
		[DebuggerHidden]
#endif
		public static void InvokeCompletionCallback(IAsyncOperation op, object continuation, SynchronizationContext syncContext, bool invokeAsync)
		{
			Debug.Assert(op != null);
			Debug.Assert(continuation != null);

			void InvokeInline(object callback)
			{
				InvokeCompletionCallback(op, callback);
			}

			if (invokeAsync)
			{
				if (syncContext != null)
				{
					syncContext.Post(InvokeInline, continuation);
				}
				else
				{
					syncContext = SynchronizationContext.Current;

					if (syncContext != null)
					{
						syncContext.Post(InvokeInline, continuation);
					}
					else
					{
						ThreadPool.QueueUserWorkItem(InvokeInline, continuation);
					}
				}
			}
			else if (syncContext == null || syncContext == SynchronizationContext.Current)
			{
				InvokeCompletionCallback(op, continuation);
			}
			else
			{
				syncContext.Post(InvokeInline, continuation);
			}
		}

#if !DEBUG
		[DebuggerHidden]
#endif
		public static void InvokeProgressCallback(IAsyncOperation op, object callback)
		{
			Debug.Assert(op != null);
			Debug.Assert(callback != null);

			switch (callback)
			{
#if !NET35
				case IProgress<float> p:
					p.Report(op.Progress);
					break;
#endif
				case Action<float> af:
					af.Invoke(op.Progress);
					break;

				case ProgressChangedEventHandler ph:
					ph.Invoke(op, new ProgressChangedEventArgs((int)(op.Progress * 100), op.AsyncState));
					break;

				case Action a:
					a.Invoke();
					break;

				case IAsyncContinuation c:
					c.Invoke(op);
					break;

				case Action<IAsyncOperation> ao:
					ao.Invoke(op);
					break;

				case AsyncCallback ac:
					ac.Invoke(op);
					break;
			}
		}

#if !DEBUG
		[DebuggerHidden]
#endif
		public static void InvokeProgressCallback(IAsyncOperation op, object callback, SynchronizationContext syncContext)
		{
			Debug.Assert(op != null);
			Debug.Assert(callback != null);

			if (syncContext == null || syncContext == SynchronizationContext.Current)
			{
				InvokeProgressCallback(op, callback);
			}
			else
			{
				syncContext.Post(args => InvokeProgressCallback(op, args), callback);
			}
		}

		#endregion

		#region implementation
		#endregion
	}
}
