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
	/// <seealso cref="IAsyncOperation{T}"/>
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	public struct AsyncAwaiter<T> : INotifyCompletion
	{
		#region data

		private readonly IAsyncOperation<T> _op;
		private readonly bool _continueOnCapturedContext;

		#endregion

		#region interface

		/// <summary>
		/// Initializes a new instance of the <see cref="AsyncAwaiter{T}"/> struct.
		/// </summary>
		public AsyncAwaiter(IAsyncOperation<T> op, bool continueOnCapturedContext)
		{
			_op = op;
			_continueOnCapturedContext = continueOnCapturedContext;
	}

		#endregion

		#region IAwaiter

		/// <summary>
		/// Gets a value indicating whether the underlying operation is completed.
		/// </summary>
		/// <value>The operation completion flag.</value>
		public bool IsCompleted => _op.IsCompleted;

		/// <summary>
		/// Returns the source result value.
		/// </summary>
		/// <returns>Returns the underlying operation result.</returns>
		public T GetResult()
		{
			AsyncAwaiter.GetResultInternal(_op);
			return _op.Result;
		}

		#endregion

		#region INotifyCompletion

		/// <inheritdoc/>
		public void OnCompleted(Action continuation)
		{
			AsyncAwaiter.OnCompletedInternal(_op, continuation, _continueOnCapturedContext);
		}

		#endregion
	}

#endif
}
