// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Runtime.CompilerServices;
using System.Threading;
#if UNITYFX_SUPPORT_TAP
using System.Threading.Tasks;
#endif

namespace UnityFx.Async
{
#if UNITYFX_SUPPORT_TAP

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
			private readonly bool _continueOnCapturedContext;

			/// <summary>
			/// Initializes a new instance of the <see cref="AsyncAwaiter"/> struct.
			/// </summary>
			public AsyncAwaiter(IAsyncOperation op, bool continueOnCapturedContext)
			{
				_op = op;
				_continueOnCapturedContext = continueOnCapturedContext;
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
					ThrowIfNonSuccess(_op, false);
				}
			}

			/// <inheritdoc/>
			public void OnCompleted(Action continuation)
			{
				SetAwaitContiniation(_op, continuation, _continueOnCapturedContext);
			}
		}

		/// <summary>
		/// Provides an object that waits for the completion of an asynchronous operation. This type and its members are intended for compiler use only.
		/// </summary>
		/// <seealso cref="IAsyncOperation{T}"/>
		public struct AsyncAwaiter<T> : INotifyCompletion
		{
			private readonly IAsyncOperation<T> _op;
			private readonly bool _continueOnCapturedContext;

			/// <summary>
			/// Initializes a new instance of the <see cref="AsyncAwaiter{T}"/> struct.
			/// </summary>
			public AsyncAwaiter(IAsyncOperation<T> op, bool continueOnCapturedContext)
			{
				_op = op;
				_continueOnCapturedContext = continueOnCapturedContext;
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
					ThrowIfNonSuccess(_op, false);
				}

				return _op.Result;
			}

			/// <inheritdoc/>
			public void OnCompleted(Action continuation)
			{
				SetAwaitContiniation(_op, continuation, _continueOnCapturedContext);
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
			public ConfiguredAsyncAwaitable(IAsyncOperation op, bool continueOnCapturedContext)
			{
				_awaiter = new AsyncAwaiter(op, continueOnCapturedContext);
			}

			/// <summary>
			/// Returns the awaiter.
			/// </summary>
			public AsyncAwaiter GetAwaiter() => _awaiter;
		}

		/// <summary>
		/// Provides an awaitable object that allows for configured awaits on <see cref="IAsyncOperation{T}"/>. This type is intended for compiler use only.
		/// </summary>
		/// <seealso cref="IAsyncOperation{T}"/>
		public struct ConfiguredAsyncAwaitable<T>
		{
			private readonly AsyncAwaiter<T> _awaiter;

			/// <summary>
			/// Initializes a new instance of the <see cref="ConfiguredAsyncAwaitable{T}"/> struct.
			/// </summary>
			public ConfiguredAsyncAwaitable(IAsyncOperation<T> op, bool continueOnCapturedContext)
			{
				_awaiter = new AsyncAwaiter<T>(op, continueOnCapturedContext);
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
		/// <seealso cref="GetAwaiter{T}(IAsyncOperation{T})"/>
		public static AsyncAwaiter GetAwaiter(this IAsyncOperation op)
		{
			return new AsyncAwaiter(op, false);
		}

		/// <summary>
		/// Returns the operation awaiter. This method is intended for compiler rather than use directly in code.
		/// </summary>
		/// <param name="op">The operation to await.</param>
		/// <seealso cref="GetAwaiter(IAsyncOperation)"/>
		public static AsyncAwaiter<T> GetAwaiter<T>(this IAsyncOperation<T> op)
		{
			return new AsyncAwaiter<T>(op, false);
		}

		/// <summary>
		/// Configures an awaiter used to await this operation.
		/// </summary>
		/// <param name="op">The operation to await.</param>
		/// <param name="continueOnCapturedContext">If <see langword="true"/> attempts to marshal the continuation back to the original context captured.</param>
		/// <returns>An object used to await the operation.</returns>
		public static ConfiguredAsyncAwaitable ConfigureAwait(this IAsyncOperation op, bool continueOnCapturedContext)
		{
			return new ConfiguredAsyncAwaitable(op, continueOnCapturedContext);
		}

		/// <summary>
		/// Configures an awaiter used to await this operation.
		/// </summary>
		/// <param name="op">The operation to await.</param>
		/// <param name="continueOnCapturedContext">If <see langword="true"/> attempts to marshal the continuation back to the original context captured.</param>
		/// <returns>An object used to await the operation.</returns>
		public static ConfiguredAsyncAwaitable<T> ConfigureAwait<T>(this IAsyncOperation<T> op, bool continueOnCapturedContext)
		{
			return new ConfiguredAsyncAwaitable<T>(op, continueOnCapturedContext);
		}

		#endregion

		#region ToTask

		/// <summary>
		/// Creates a <see cref="Task"/> instance matching the source <see cref="IAsyncOperation"/>.
		/// </summary>
		/// <param name="op">The target operation.</param>
		/// <seealso cref="ToTask{T}(IAsyncOperation{T})"/>
		public static Task ToTask(this IAsyncOperation op)
		{
			var status = op.Status;

			if (status == AsyncOperationStatus.RanToCompletion)
			{
				return Task.CompletedTask;
			}
			else if (status == AsyncOperationStatus.Faulted)
			{
				return Task.FromException(op.Exception.InnerException);
			}
			else if (status == AsyncOperationStatus.Canceled)
			{
				return Task.FromCanceled(CancellationToken.None);
			}
			else
			{
				var result = new TaskCompletionSource<object>();

				if (!op.TryAddCompletionCallback(asyncOp => AsyncContinuation.InvokeTaskContinuation(asyncOp, result), null))
				{
					AsyncContinuation.InvokeTaskContinuation(op, result);
				}

				return result.Task;
			}
		}

		/// <summary>
		/// Creates a <see cref="Task"/> instance matching the source <see cref="IAsyncOperation"/>.
		/// </summary>
		/// <param name="op">The target operation.</param>
		/// <seealso cref="ToTask(IAsyncOperation)"/>
		public static Task<T> ToTask<T>(this IAsyncOperation<T> op)
		{
			var status = op.Status;

			if (status == AsyncOperationStatus.RanToCompletion)
			{
				return Task.FromResult(op.Result);
			}
			else if (status == AsyncOperationStatus.Faulted)
			{
				return Task.FromException<T>(op.Exception.InnerException);
			}
			else if (status == AsyncOperationStatus.Canceled)
			{
				return Task.FromCanceled<T>(CancellationToken.None);
			}
			else
			{
				var result = new TaskCompletionSource<T>();

				if (!op.TryAddCompletionCallback(asyncOp => AsyncContinuation.InvokeTaskContinuation(asyncOp as IAsyncOperation<T>, result), null))
				{
					AsyncContinuation.InvokeTaskContinuation(op, result);
				}

				return result.Task;
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
			var result = new AsyncCompletionSource(AsyncOperationStatus.Running);

			task.ContinueWith(
				t =>
				{
					if (t.IsFaulted)
					{
						result.SetException(t.Exception);
					}
					else if (t.IsCanceled)
					{
						result.SetCanceled();
					}
					else
					{
						result.SetCompleted();
					}
				},
				TaskContinuationOptions.ExecuteSynchronously);

			return result;
		}

		/// <summary>
		/// Creates an <see cref="IAsyncOperation"/> instance that completes when the specified <paramref name="task"/> completes.
		/// </summary>
		/// <param name="task">The task to convert to <see cref="IAsyncOperation"/>.</param>
		/// <returns>An <see cref="IAsyncOperation"/> that represents the <paramref name="task"/>.</returns>
		public static AsyncResult<T> ToAsync<T>(this Task<T> task)
		{
			var result = new AsyncCompletionSource<T>(AsyncOperationStatus.Running);

			task.ContinueWith(
				t =>
				{
					if (t.IsFaulted)
					{
						result.SetException(t.Exception);
					}
					else if (t.IsCanceled)
					{
						result.SetCanceled();
					}
					else
					{
						result.SetResult(t.Result);
					}
				},
				TaskContinuationOptions.ExecuteSynchronously);

			return result;
		}

		#endregion

		#region Implementation

		private static void SetAwaitContiniation(IAsyncOperation op, Action continuation, bool captureSynchronizationContext)
		{
			var syncContext = captureSynchronizationContext ? SynchronizationContext.Current : null;

			if (op is AsyncResult ar)
			{
				ar.SetContinuationForAwait(continuation, syncContext);
			}
			else if (!op.TryAddCompletionCallback(asyncOp => continuation(), syncContext))
			{
				continuation();
			}
		}

		#endregion
	}

#endif
}
