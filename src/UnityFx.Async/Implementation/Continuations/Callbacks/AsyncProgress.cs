// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;

namespace UnityFx.Async
{
	internal class AsyncProgress : AsyncInvokable
	{
		#region interface

		internal AsyncProgress(IAsyncOperation op, SynchronizationContext syncContext, object callback)
			: base(op, syncContext, callback)
		{
		}

		internal static void InvokeInline(IAsyncOperation op, object callback)
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

		#region AsyncInvokable

		protected override void Invoke(IAsyncOperation op, object continuation)
		{
			InvokeInline(op, continuation);
		}

		#endregion
	}
}
