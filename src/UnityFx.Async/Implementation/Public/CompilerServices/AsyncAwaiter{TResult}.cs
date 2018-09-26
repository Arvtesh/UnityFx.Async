// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace UnityFx.Async.CompilerServices
{
#if !NET35

	/// <summary>
		/// Provides an object that waits for the completion of an asynchronous operation. This type and its members are intended for compiler use only.
		/// </summary>
		/// <seealso cref="IAsyncOperation{TResult}"/>
		public struct AsyncAwaiter<TResult> : INotifyCompletion
		{
			private readonly IAsyncOperation<TResult> _op;
			private readonly AsyncCallbackOptions _options;

			/// <summary>
			/// Initializes a new instance of the <see cref="AsyncAwaiter{T}"/> struct.
			/// </summary>
			public AsyncAwaiter(IAsyncOperation<TResult> op, AsyncCallbackOptions options)
			{
				_op = op;
				_options = options;
			}

			/// <summary>
			/// Gets a value indicating whether the underlying operation is completed.
			/// </summary>
			/// <value>The operation completion flag.</value>
			public bool IsCompleted => _op.IsCompleted;

			/// <summary>
			/// Returns the source result value.
			/// </summary>
			/// <returns>Returns the underlying operation result.</returns>
			public TResult GetResult()
			{
				if (!_op.IsCompletedSuccessfully)
				{
					_op.ThrowIfNonSuccess();
				}

				return _op.Result;
			}

			/// <inheritdoc/>
			public void OnCompleted(Action continuation)
			{
				AsyncAwaiter.SetAwaitContinuation(_op, continuation, _options);
			}
		}

#endif
}
