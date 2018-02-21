// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;

namespace UnityFx.Async
{
#if UNITYFX_SUPPORT_TAP

	/// <summary>
	/// Provides an object that waits for the completion of an asynchronous operation. This type and its members are intended for compiler use only.
	/// </summary>
	/// <seealso cref="IAsyncOperation"/>
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	public struct AsyncAwaiter : INotifyCompletion
	{
		#region data

		private readonly IAsyncOperation _op;
		private readonly bool _continueOnCapturedContext;

		#endregion

		#region interface

		/// <summary>
		/// Initializes a new instance of the <see cref="AsyncAwaiter"/> struct.
		/// </summary>
		public AsyncAwaiter(IAsyncOperation op, bool continueOnCapturedContext)
		{
			_op = op;
			_continueOnCapturedContext = continueOnCapturedContext;
		}

		#endregion

		#region internals

		internal static void OnCompletedInternal(IAsyncOperation op, Action continuation, bool continueOnCapturedContext)
		{
			var syncContext = continueOnCapturedContext ? SynchronizationContext.Current : null;

			if (op is AsyncResult ar)
			{
				ar.SetContinuationForAwait(continuation, syncContext);
			}
			else if (!op.TryAddCompletionCallback(o => continuation(), syncContext))
			{
				continuation();
			}
		}

		internal static void GetResultInternal(IAsyncOperation op)
		{
			if (!op.IsCompletedSuccessfully)
			{
				AsyncExtensions.ThrowIfFaultedOrCanceled(op);
			}
		}

		#endregion

		#region IAwaiter

		/// <summary>
		/// Gets whether the underlying operation is completed.
		/// </summary>
		/// <value>The operation completion flag.</value>
		public bool IsCompleted => _op.IsCompleted;

		/// <summary>
		/// Returns the source result value.
		/// </summary>
		public void GetResult()
		{
			GetResultInternal(_op);
		}

		#endregion

		#region INotifyCompletion

		/// <inheritdoc/>
		public void OnCompleted(Action continuation)
		{
			OnCompletedInternal(_op, continuation, _continueOnCapturedContext);
		}

		#endregion
	}

#endif
}
