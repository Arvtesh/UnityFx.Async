// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections;
#if !NET35
using System.Runtime.ExceptionServices;
#endif
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

		/// <summary>
		/// Blocks calling thread until the operation is completed.
		/// </summary>
		public static void Wait(this IAsyncOperation op)
		{
			if (!op.IsCompleted)
			{
				op.AsyncWaitHandle.WaitOne();
			}
		}

		/// <summary>
		/// Blocks calling thread until the operation is completed. After that rethrows the operation exception (if any).
		/// </summary>
		public static void Join(this IAsyncOperation op)
		{
			Wait(op);
			ThrowIfFaulted(op);
		}

		/// <summary>
		/// Blocks calling thread until the operation is completed. After that rethrows the operation exception (if any).
		/// </summary>
		public static T Join<T>(this IAsyncOperation<T> op)
		{
			Wait(op);
			ThrowIfFaulted(op);
			return op.Result;
		}

		/// <summary>
		/// Throws exception if the operation has failed.
		/// </summary>
		public static void ThrowIfFaulted(this IAsyncOperation op)
		{
			if (op.IsFaulted)
			{
				var e = op.Exception;

				if (e != null)
				{
#if !NET35
					ExceptionDispatchInfo.Capture(e).Throw();
#else
					throw e;
#endif
				}
				else if (op.IsCanceled)
				{
					throw new OperationCanceledException(op.ToString());
				}
				else
				{
					throw new Exception(op.ToString());
				}
			}
		}

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

		#region IAsyncOperationCompletionSource

		/// <summary>
		/// Transitions the underlying <see cref="IAsyncOperation"/> into the <see cref="AsyncOperationStatus.Canceled"/> state.
		/// </summary>
		/// <param name="acs">The copmletion source instance.</param>
		/// <exception cref="InvalidOperationException">Thrown if the transition fails.</exception>
		/// <seealso cref="TrySetCanceled(IAsyncOperationCompletionSource)"/>
		public static void SetCanceled(this IAsyncOperationCompletionSource acs)
		{
			acs.SetCanceled(false);
		}

		/// <summary>
		/// Attempts to transition the underlying <see cref="IAsyncOperation"/> into the <see cref="AsyncOperationStatus.Canceled"/> state.
		/// </summary>
		/// <param name="acs">The copmletion source instance.</param>
		/// <returns>Returns <see langword="true"/> if the attemp was successfull; <see langword="false"/> otherwise.</returns>
		/// <seealso cref="SetCanceled(IAsyncOperationCompletionSource)"/>
		public static bool TrySetCanceled(this IAsyncOperationCompletionSource acs)
		{
			return acs.TrySetCanceled(false);
		}

		/// <summary>
		/// Transitions the underlying <see cref="IAsyncOperation"/> into the <see cref="AsyncOperationStatus.Faulted"/> state.
		/// </summary>
		/// <param name="acs">The copmletion source instance.</param>
		/// <param name="e">An exception that caused the operation to end prematurely.</param>
		/// <exception cref="InvalidOperationException">Thrown if the transition fails.</exception>
		/// <seealso cref="TrySetException(IAsyncOperationCompletionSource, Exception)"/>
		public static void SetException(this IAsyncOperationCompletionSource acs, Exception e)
		{
			acs.SetException(e, false);
		}

		/// <summary>
		/// Attempts to transition the underlying <see cref="IAsyncOperation"/> into the <see cref="AsyncOperationStatus.Faulted"/> state.
		/// </summary>
		/// <param name="acs">The copmletion source instance.</param>
		/// <param name="e">An exception that caused the operation to end prematurely.</param>
		/// <returns>Returns <see langword="true"/> if the attemp was successfull; <see langword="false"/> otherwise.</returns>
		/// <seealso cref="SetException(IAsyncOperationCompletionSource, Exception)"/>
		public static bool TrySetException(this IAsyncOperationCompletionSource acs, Exception e)
		{
			return acs.TrySetException(e, false);
		}

		/// <summary>
		/// Transitions the underlying <see cref="IAsyncOperation"/> into the <see cref="AsyncOperationStatus.RanToCompletion"/> state.
		/// </summary>
		/// <param name="acs">The copmletion source instance.</param>
		/// <exception cref="InvalidOperationException">Thrown if the transition fails.</exception>
		/// <seealso cref="TrySetCompleted(IAsyncOperationCompletionSource)"/>
		public static void SetCompleted(this IAsyncOperationCompletionSource acs)
		{
			acs.SetCompleted(false);
		}

		/// <summary>
		/// Attempts to transition the underlying <see cref="IAsyncOperation"/> into the <see cref="AsyncOperationStatus.RanToCompletion"/> state.
		/// </summary>
		/// <param name="acs">The copmletion source instance.</param>
		/// <returns>Returns <see langword="true"/> if the attemp was successfull; <see langword="false"/> otherwise.</returns>
		/// <seealso cref="SetCompleted(IAsyncOperationCompletionSource)"/>
		public static bool TrySetCompleted(this IAsyncOperationCompletionSource acs)
		{
			return acs.TrySetCompleted(false);
		}

		/// <summary>
		/// Transitions the underlying <see cref="IAsyncOperation{T}"/> into the <see cref="AsyncOperationStatus.RanToCompletion"/> state.
		/// </summary>
		/// <param name="acs">The copmletion source instance.</param>
		/// <param name="result">The operation result.</param>
		/// <exception cref="InvalidOperationException">Thrown if the transition fails.</exception>
		/// <seealso cref="TrySetResult{T}(IAsyncOperationCompletionSource{T}, T)"/>
		public static void SetResult<T>(this IAsyncOperationCompletionSource<T> acs, T result)
		{
			acs.SetResult(result, false);
		}

		/// <summary>
		/// Attempts to transition the underlying <see cref="IAsyncOperation{T}"/> into the <see cref="AsyncOperationStatus.RanToCompletion"/> state.
		/// </summary>
		/// <param name="acs">The copmletion source instance.</param>
		/// <param name="result">The operation result.</param>
		/// <returns>Returns <see langword="true"/> if the attemp was successfull; <see langword="false"/> otherwise.</returns>
		/// <seealso cref="SetResult{T}(IAsyncOperationCompletionSource{T}, T)"/>
		public static bool TrySetResult<T>(this IAsyncOperationCompletionSource<T> acs, T result)
		{
			return acs.TrySetResult(result, false);
		}

		#endregion
	}
}
