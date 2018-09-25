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
		/// Provides an object that waits for the completion of an asynchronous operation. This type and its members are intended for compiler use only.
		/// </summary>
		/// <seealso cref="IAsyncOperation"/>
		public struct AsyncAwaiter : INotifyCompletion
		{
			private readonly IAsyncOperation _op;
			private readonly AsyncCallbackOptions _options;

			/// <summary>
			/// Initializes a new instance of the <see cref="AsyncAwaiter"/> struct.
			/// </summary>
			public AsyncAwaiter(IAsyncOperation op, AsyncCallbackOptions options)
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
			public void GetResult()
			{
				if (!_op.IsCompletedSuccessfully)
				{
					ThrowIfNonSuccess(_op);
				}
			}

			/// <inheritdoc/>
			public void OnCompleted(Action continuation)
			{
				SetAwaitContinuation(_op, continuation, _options);
			}
		}

		/// <summary>
		/// Provides an object that waits for the completion of an asynchronous operation. This type and its members are intended for compiler use only.
		/// </summary>
		/// <seealso cref="IAsyncOperation{TResult}"/>
		public struct AsyncAwaiter<T> : INotifyCompletion
		{
			private readonly IAsyncOperation<T> _op;
			private readonly AsyncCallbackOptions _options;

			/// <summary>
			/// Initializes a new instance of the <see cref="AsyncAwaiter{T}"/> struct.
			/// </summary>
			public AsyncAwaiter(IAsyncOperation<T> op, AsyncCallbackOptions options)
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
			public T GetResult()
			{
				if (!_op.IsCompletedSuccessfully)
				{
					ThrowIfNonSuccess(_op);
				}

				return _op.Result;
			}

			/// <inheritdoc/>
			public void OnCompleted(Action continuation)
			{
				SetAwaitContinuation(_op, continuation, _options);
			}
		}

		/// <summary>
		/// Provides an awaitable object that allows for configured awaits on <see cref="IAsyncOperation"/>. This type is intended for compiler use only.
		/// </summary>
		/// <seealso cref="IAsyncOperation"/>
		public struct ConfiguredAsyncAwaitable
		{
			private readonly AsyncAwaiter _awaiter;

			/// <summary>
			/// Initializes a new instance of the <see cref="ConfiguredAsyncAwaitable"/> struct.
			/// </summary>
			public ConfiguredAsyncAwaitable(IAsyncOperation op, AsyncCallbackOptions options)
			{
				_awaiter = new AsyncAwaiter(op, options);
			}

			/// <summary>
			/// Returns the awaiter.
			/// </summary>
			public AsyncAwaiter GetAwaiter() => _awaiter;
		}

		/// <summary>
		/// Provides an awaitable object that allows for configured awaits on <see cref="IAsyncOperation{TResult}"/>. This type is intended for compiler use only.
		/// </summary>
		/// <seealso cref="IAsyncOperation{TResult}"/>
		public struct ConfiguredAsyncAwaitable<T>
		{
			private readonly AsyncAwaiter<T> _awaiter;

			/// <summary>
			/// Initializes a new instance of the <see cref="ConfiguredAsyncAwaitable{T}"/> struct.
			/// </summary>
			public ConfiguredAsyncAwaitable(IAsyncOperation<T> op, AsyncCallbackOptions options)
			{
				_awaiter = new AsyncAwaiter<T>(op, options);
			}

			/// <summary>
			/// Returns the awaiter.
			/// </summary>
			public AsyncAwaiter<T> GetAwaiter() => _awaiter;
		}

		/// <summary>
		/// Returns the operation awaiter. This method is intended for compiler rather than use directly in code.
		/// </summary>
		/// <param name="op">The operation to await.</param>
		/// <seealso cref="GetAwaiter{TResult}(IAsyncOperation{TResult})"/>
		public static AsyncAwaiter GetAwaiter(this IAsyncOperation op)
		{
			return new AsyncAwaiter(op, AsyncCallbackOptions.ExecuteOnCapturedContext);
		}

		/// <summary>
		/// Returns the operation awaiter. This method is intended for compiler rather than use directly in code.
		/// </summary>
		/// <param name="op">The operation to await.</param>
		/// <seealso cref="GetAwaiter(IAsyncOperation)"/>
		public static AsyncAwaiter<TResult> GetAwaiter<TResult>(this IAsyncOperation<TResult> op)
		{
			return new AsyncAwaiter<TResult>(op, AsyncCallbackOptions.ExecuteOnCapturedContext);
		}

		/// <summary>
		/// Configures an awaiter used to await this operation.
		/// </summary>
		/// <param name="op">The operation to await.</param>
		/// <param name="continueOnCapturedContext">If <see langword="true"/> attempts to marshal the continuation back to the original context captured.</param>
		/// <returns>An object used to await the operation.</returns>
		public static ConfiguredAsyncAwaitable ConfigureAwait(this IAsyncOperation op, bool continueOnCapturedContext)
		{
			return new ConfiguredAsyncAwaitable(op, continueOnCapturedContext ? AsyncCallbackOptions.ExecuteOnCapturedContext : AsyncCallbackOptions.ExecuteSynchronously);
		}

		/// <summary>
		/// Configures an awaiter used to await this operation.
		/// </summary>
		/// <param name="op">The operation to await.</param>
		/// <param name="continueOnCapturedContext">If <see langword="true"/> attempts to marshal the continuation back to the original context captured.</param>
		/// <returns>An object used to await the operation.</returns>
		public static ConfiguredAsyncAwaitable<TResult> ConfigureAwait<TResult>(this IAsyncOperation<TResult> op, bool continueOnCapturedContext)
		{
			return new ConfiguredAsyncAwaitable<TResult>(op, continueOnCapturedContext ? AsyncCallbackOptions.ExecuteOnCapturedContext : AsyncCallbackOptions.ExecuteSynchronously);
		}

		/// <summary>
		/// Configures an awaiter used to await this operation.
		/// </summary>
		/// <param name="op">The operation to await.</param>
		/// <param name="continuationOptions">Specifies continuation options.</param>
		/// <returns>An object used to await the operation.</returns>
		public static ConfiguredAsyncAwaitable ConfigureAwait(this IAsyncOperation op, AsyncCallbackOptions continuationOptions)
		{
			return new ConfiguredAsyncAwaitable(op, continuationOptions);
		}

		/// <summary>
		/// Configures an awaiter used to await this operation.
		/// </summary>
		/// <param name="op">The operation to await.</param>
		/// <param name="continuationOptions">Specifies continuation options.</param>
		/// <returns>An object used to await the operation.</returns>
		public static ConfiguredAsyncAwaitable<TResult> ConfigureAwait<TResult>(this IAsyncOperation<TResult> op, AsyncCallbackOptions continuationOptions)
		{
			return new ConfiguredAsyncAwaitable<TResult>(op, continuationOptions);
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

		private static void SetAwaitContinuation(IAsyncOperation op, Action continuation, AsyncCallbackOptions options)
		{
			var syncContext = GetContext(options);

			if (op is AsyncResult ar)
			{
				ar.SetContinuationForAwait(continuation, syncContext);
			}
			else
			{
				op.AddCompletionCallback(asyncOp => continuation(), syncContext);
			}
		}

		#endregion
	}

#endif
}
