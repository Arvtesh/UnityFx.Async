// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Runtime.CompilerServices;
using System.Threading;
#if !NET35
using System.Threading.Tasks;
#endif

namespace UnityFx.Async
{
#if !NET35

	partial class AsyncExtensions
	{
		#region GetAwaiter/ConfigureAwait

		/// <summary>
		/// Returns the operation awaiter. This method is intended for compiler rather than use directly in code.
		/// </summary>
		/// <param name="op">The operation to await.</param>
		/// <seealso cref="GetAwaiter{TResult}(IAsyncOperation{TResult})"/>
		public static CompilerServices.AsyncAwaiter GetAwaiter(this IAsyncOperation op)
		{
			return new CompilerServices.AsyncAwaiter(op, AsyncCallbackOptions.ExecuteOnCapturedContext);
		}

		/// <summary>
		/// Returns the operation awaiter. This method is intended for compiler rather than use directly in code.
		/// </summary>
		/// <param name="op">The operation to await.</param>
		/// <seealso cref="GetAwaiter(IAsyncOperation)"/>
		public static CompilerServices.AsyncAwaiter<TResult> GetAwaiter<TResult>(this IAsyncOperation<TResult> op)
		{
			return new CompilerServices.AsyncAwaiter<TResult>(op, AsyncCallbackOptions.ExecuteOnCapturedContext);
		}

		/// <summary>
		/// Configures an awaiter used to await this operation.
		/// </summary>
		/// <param name="op">The operation to await.</param>
		/// <param name="continueOnCapturedContext">If <see langword="true"/> attempts to marshal the continuation back to the original context captured.</param>
		/// <returns>An object used to await the operation.</returns>
		public static CompilerServices.AsyncAwaitable ConfigureAwait(this IAsyncOperation op, bool continueOnCapturedContext)
		{
			return new CompilerServices.AsyncAwaitable(op, continueOnCapturedContext ? AsyncCallbackOptions.ExecuteOnCapturedContext : AsyncCallbackOptions.ExecuteSynchronously);
		}

		/// <summary>
		/// Configures an awaiter used to await this operation.
		/// </summary>
		/// <param name="op">The operation to await.</param>
		/// <param name="continueOnCapturedContext">If <see langword="true"/> attempts to marshal the continuation back to the original context captured.</param>
		/// <returns>An object used to await the operation.</returns>
		public static CompilerServices.AsyncAwaitable<TResult> ConfigureAwait<TResult>(this IAsyncOperation<TResult> op, bool continueOnCapturedContext)
		{
			return new CompilerServices.AsyncAwaitable<TResult>(op, continueOnCapturedContext ? AsyncCallbackOptions.ExecuteOnCapturedContext : AsyncCallbackOptions.ExecuteSynchronously);
		}

		/// <summary>
		/// Configures an awaiter used to await this operation.
		/// </summary>
		/// <param name="op">The operation to await.</param>
		/// <param name="continuationOptions">Specifies continuation options.</param>
		/// <returns>An object used to await the operation.</returns>
		public static CompilerServices.AsyncAwaitable ConfigureAwait(this IAsyncOperation op, AsyncCallbackOptions continuationOptions)
		{
			return new CompilerServices.AsyncAwaitable(op, continuationOptions);
		}

		/// <summary>
		/// Configures an awaiter used to await this operation.
		/// </summary>
		/// <param name="op">The operation to await.</param>
		/// <param name="continuationOptions">Specifies continuation options.</param>
		/// <returns>An object used to await the operation.</returns>
		public static CompilerServices.AsyncAwaitable<TResult> ConfigureAwait<TResult>(this IAsyncOperation<TResult> op, AsyncCallbackOptions continuationOptions)
		{
			return new CompilerServices.AsyncAwaitable<TResult>(op, continuationOptions);
		}

		#endregion

		#region ToTask

		/// <summary>
		/// Creates a <see cref="Task"/> instance matching the source <see cref="IAsyncOperation"/>.
		/// </summary>
		/// <param name="op">The target operation.</param>
		/// <seealso cref="ToTask{TResult}(IAsyncOperation{TResult})"/>
		public static Task ToTask(this IAsyncOperation op)
		{
			var status = op.Status;

			if (status == AsyncOperationStatus.RanToCompletion)
			{
				return Task.CompletedTask;
			}
			else if (status == AsyncOperationStatus.Faulted)
			{
				return Task.FromException(op.Exception);
			}
			else if (status == AsyncOperationStatus.Canceled)
			{
				return Task.FromCanceled(new CancellationToken(true));
			}
			else
			{
				var tcs = new TaskCompletionSource<VoidResult>();

				op.AddCompletionCallback(
					asyncOp =>
					{
						status = op.Status;

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
					},
					null);

				return tcs.Task;
			}
		}

		/// <summary>
		/// Creates a <see cref="Task"/> instance matching the source <see cref="IAsyncOperation"/>.
		/// </summary>
		/// <param name="op">The target operation.</param>
		/// <seealso cref="ToTask(IAsyncOperation)"/>
		public static Task<TResult> ToTask<TResult>(this IAsyncOperation<TResult> op)
		{
			var status = op.Status;

			if (status == AsyncOperationStatus.RanToCompletion)
			{
				return Task.FromResult(op.Result);
			}
			else if (status == AsyncOperationStatus.Faulted)
			{
				return Task.FromException<TResult>(op.Exception);
			}
			else if (status == AsyncOperationStatus.Canceled)
			{
				return Task.FromCanceled<TResult>(new CancellationToken(true));
			}
			else
			{
				var tcs = new TaskCompletionSource<TResult>();

				op.AddCompletionCallback(
					asyncOp =>
					{
						status = op.Status;

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
					},
					null);

				return tcs.Task;
			}
		}

		#endregion

		#region ToAsync

		/// <summary>
		/// Creates an <see cref="IAsyncOperation"/> instance that completes when the specified <paramref name="task"/> completes.
		/// </summary>
		/// <param name="task">The task to convert to <see cref="IAsyncOperation"/>.</param>
		/// <returns>An <see cref="IAsyncOperation"/> that represents the <paramref name="task"/>.</returns>
		public static AsyncResult ToAsync(this Task task)
		{
			return AsyncResult.FromTask(task);
		}

		/// <summary>
		/// Creates an <see cref="IAsyncOperation"/> instance that completes when the specified <paramref name="task"/> completes.
		/// </summary>
		/// <param name="task">The task to convert to <see cref="IAsyncOperation"/>.</param>
		/// <returns>An <see cref="IAsyncOperation"/> that represents the <paramref name="task"/>.</returns>
		public static AsyncResult<TResult> ToAsync<TResult>(this Task<TResult> task)
		{
			return AsyncResult.FromTask(task);
		}

		#endregion

		#region implementation
		#endregion
	}

#endif
}
