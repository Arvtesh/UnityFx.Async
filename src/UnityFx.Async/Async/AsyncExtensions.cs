// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections;
using System.Threading;
#if !NET35
using System.Threading.Tasks;
#endif

namespace UnityFx.Async
{
	/// <summary>
	/// Defines extension methods realted to <see cref="IAsyncOperation"/>.
	/// </summary>
	public static class AsyncExtensions
	{
		#region IAsyncOperation

#if !NET35

		/// <summary>
		/// Returns the operation awaiter. This method is intended for compiler rather than use directly in code.
		/// </summary>
		public static AsyncResultAwaiter GetAwaiter(this IAsyncOperation op)
		{
			return new AsyncResultAwaiter(op);
		}

		/// <summary>
		/// Returns the operation awaiter. This method is intended for compiler rather than use directly in code.
		/// </summary>
		public static AsyncResultAwaiter<T> GetAwaiter<T>(this IAsyncOperation<T> op)
		{
			return new AsyncResultAwaiter<T>(op);
		}

		/// <summary>
		/// Created a <see cref="Task"/> instance matching the source <see cref="IAsyncOperation"/>.
		/// </summary>
		public static Task ToTask(this IAsyncOperation op)
		{
			if (op is IAsyncContinuationContainer c)
			{
				var result = new TaskCompletionSource<object>(op);

				c.AddContinuation(() =>
				{
					if (op.IsCompletedSuccessfully)
					{
						result.SetResult(null);
					}
					else if (op.IsCanceled)
					{
						result.SetCanceled();
					}
					else
					{
						result.SetException(op.Exception);
					}
				});

				return result.Task;
			}
			else
			{
				throw new NotSupportedException();
			}
		}

		/// <summary>
		/// Created a <see cref="Task"/> instance matching the source <see cref="IAsyncOperation"/>.
		/// </summary>
		public static Task<T> ToTask<T>(this IAsyncOperation<T> op)
		{
			if (op is IAsyncContinuationContainer c)
			{
				var result = new TaskCompletionSource<T>(op);

				c.AddContinuation(() =>
				{
					if (op.IsCompletedSuccessfully)
					{
						result.SetResult(op.Result);
					}
					else if (op.IsCanceled)
					{
						result.SetCanceled();
					}
					else
					{
						result.SetException(op.Exception);
					}
				});

				return result.Task;
			}
			else
			{
				throw new NotSupportedException();
			}
		}

#endif

		#endregion
	}
}
