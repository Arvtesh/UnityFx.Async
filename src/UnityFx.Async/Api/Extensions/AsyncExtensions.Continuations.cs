// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Threading;

namespace UnityFx.Async
{
	partial class AsyncExtensions
	{
		#region AddCompletionCallback/AddContinuation

		/// <summary>
		/// Adds a completion callback to be executed after the operation has completed. If the operation is completed the <paramref name="continuation"/> is invoked synchronously.
		/// </summary>
		/// <param name="op">The target operation.</param>
		/// <param name="continuation">The callback to be executed when the operation has completed.</param>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="continuation"/> is <see langword="null"/>.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation has been disposed.</exception>
		/// <seealso cref="IAsyncOperationEvents"/>
		/// <seealso cref="AddCompletionCallback(IAsyncOperation, AsyncOperationCallback)"/>
		public static void AddContinuation(this IAsyncOperation op, IAsyncContinuation continuation)
		{
			if (!op.TryAddContinuation(continuation))
			{
				continuation.Invoke(op, true);
			}
		}

		/// <summary>
		/// Adds a completion callback to be executed after the operation has completed. If the operation is completed the <paramref name="action"/> is invoked synchronously.
		/// </summary>
		/// <param name="op">The target operation.</param>
		/// <param name="action">The callback to be executed when the operation has completed.</param>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="action"/> is <see langword="null"/>.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation has been disposed.</exception>
		/// <seealso cref="IAsyncOperationEvents"/>
		/// <seealso cref="AddCompletionCallback(IAsyncOperation, AsyncOperationCallback, SynchronizationContext)"/>
		public static void AddCompletionCallback(this IAsyncOperation op, AsyncOperationCallback action)
		{
			if (!op.TryAddCompletionCallback(action, SynchronizationContext.Current))
			{
				action(op);
			}
		}

		/// <summary>
		/// Adds a completion callback to be executed after the operation has completed. If the operation is completed the <paramref name="action"/> is invoked
		/// on the <paramref name="syncContext"/> specified.
		/// </summary>
		/// <param name="op">The target operation.</param>
		/// <param name="action">The callback to be executed when the operation has completed.</param>
		/// <param name="syncContext">If not <see langword="null"/> method attempts to marshal the continuation to the synchronization context.
		/// Otherwise the callback is invoked on a thread that initiated the operation completion.
		/// </param>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="action"/> is <see langword="null"/>.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation has been disposed.</exception>
		/// <seealso cref="IAsyncOperationEvents"/>
		/// <seealso cref="AddCompletionCallback(IAsyncOperation, AsyncOperationCallback)"/>
		public static void AddCompletionCallback(this IAsyncOperation op, AsyncOperationCallback action, SynchronizationContext syncContext)
		{
			if (!op.TryAddCompletionCallback(action, syncContext))
			{
				if (syncContext == null || syncContext.GetType() == typeof(SynchronizationContext) || syncContext == SynchronizationContext.Current)
				{
					action(op);
				}
				else
				{
					syncContext.Post(args => action(args as IAsyncOperation), op);
				}
			}
		}

		#endregion

		#region ContinueWith

		/// <summary>
		/// Creates a continuation that executes when the target <see cref="IAsyncOperation"/> completes.
		/// </summary>
		/// <param name="op">The operation to continue.</param>
		/// <param name="action">An action to run when the <paramref name="op"/> completes.</param>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="action"/> is <see langword="null"/>.</exception>
		/// <returns>An operation that is executed after <paramref name="op"/> completes.</returns>
		public static IAsyncOperation ContinueWith(this IAsyncOperation op, Action<IAsyncOperation> action)
		{
			return ContinueWith(op, action, AsyncContinuationOptions.CaptureSynchronizationContext);
		}

		/// <summary>
		/// Creates a continuation that executes when the target <see cref="IAsyncOperation"/> completes.
		/// </summary>
		/// <param name="op">The operation to continue.</param>
		/// <param name="action">An action to run when the <paramref name="op"/> completes.</param>
		/// <param name="options">Options for when the <paramref name="action"/> is executed.</param>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="action"/> is <see langword="null"/>.</exception>
		/// <returns>An operation that is executed after <paramref name="op"/> completes.</returns>
		public static IAsyncOperation ContinueWith(this IAsyncOperation op, Action<IAsyncOperation> action, AsyncContinuationOptions options)
		{
			if (action == null)
			{
				throw new ArgumentNullException(nameof(action));
			}

			return new ContinuationResult<VoidResult>(op, options, action, null);
		}

		/// <summary>
		/// Creates a continuation that executes when the target <see cref="IAsyncOperation"/> completes.
		/// </summary>
		/// <param name="op">The operation to continue.</param>
		/// <param name="action">An action to run when the <paramref name="op"/> completes.</param>
		/// <param name="userState">A user-defined state object that is passed as second argument to <paramref name="action"/>.</param>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="action"/> is <see langword="null"/>.</exception>
		/// <returns>An operation that is executed after <paramref name="op"/> completes.</returns>
		public static IAsyncOperation ContinueWith(this IAsyncOperation op, Action<IAsyncOperation, object> action, object userState)
		{
			return ContinueWith(op, action, userState, AsyncContinuationOptions.CaptureSynchronizationContext);
		}

		/// <summary>
		/// Creates a continuation that executes when the target <see cref="IAsyncOperation"/> completes.
		/// </summary>
		/// <param name="op">The operation to continue.</param>
		/// <param name="action">An action to run when the <paramref name="op"/> completes.</param>
		/// <param name="userState">A user-defined state object that is passed as second argument to <paramref name="action"/>.</param>
		/// <param name="options">Options for when the <paramref name="action"/> is executed.</param>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="action"/> is <see langword="null"/>.</exception>
		/// <returns>An operation that is executed after <paramref name="op"/> completes.</returns>
		public static IAsyncOperation ContinueWith(this IAsyncOperation op, Action<IAsyncOperation, object> action, object userState, AsyncContinuationOptions options)
		{
			if (action == null)
			{
				throw new ArgumentNullException(nameof(action));
			}

			return new ContinuationResult<VoidResult>(op, options, action, userState);
		}

		/// <summary>
		/// Creates a continuation that executes when the target <see cref="IAsyncOperation"/> completes.
		/// </summary>
		/// <param name="op">The operation to continue.</param>
		/// <param name="action">An action to run when the <paramref name="op"/> completes.</param>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="action"/> is <see langword="null"/>.</exception>
		/// <returns>An operation that is executed after <paramref name="op"/> completes.</returns>
		public static IAsyncOperation<TResult> ContinueWith<TResult>(this IAsyncOperation op, Func<IAsyncOperation, TResult> action)
		{
			return ContinueWith(op, action, AsyncContinuationOptions.CaptureSynchronizationContext);
		}

		/// <summary>
		/// Creates a continuation that executes when the target <see cref="IAsyncOperation"/> completes.
		/// </summary>
		/// <param name="op">The operation to continue.</param>
		/// <param name="action">An action to run when the <paramref name="op"/> completes.</param>
		/// <param name="options">Options for when the <paramref name="action"/> is executed.</param>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="action"/> is <see langword="null"/>.</exception>
		/// <returns>An operation that is executed after <paramref name="op"/> completes.</returns>
		public static IAsyncOperation<TResult> ContinueWith<TResult>(this IAsyncOperation op, Func<IAsyncOperation, TResult> action, AsyncContinuationOptions options)
		{
			if (action == null)
			{
				throw new ArgumentNullException(nameof(action));
			}

			return new ContinuationResult<TResult>(op, options, action, null);
		}

		/// <summary>
		/// Creates a continuation that executes when the target <see cref="IAsyncOperation"/> completes.
		/// </summary>
		/// <param name="op">The operation to continue.</param>
		/// <param name="action">An action to run when the <paramref name="op"/> completes.</param>
		/// <param name="userState">A user-defined state object that is passed as second argument to <paramref name="action"/>.</param>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="action"/> is <see langword="null"/>.</exception>
		/// <returns>An operation that is executed after <paramref name="op"/> completes.</returns>
		public static IAsyncOperation<TResult> ContinueWith<TResult>(this IAsyncOperation op, Func<IAsyncOperation, object, TResult> action, object userState)
		{
			return ContinueWith(op, action, userState, AsyncContinuationOptions.CaptureSynchronizationContext);
		}

		/// <summary>
		/// Creates a continuation that executes when the target <see cref="IAsyncOperation"/> completes.
		/// </summary>
		/// <param name="op">The operation to continue.</param>
		/// <param name="action">An action to run when the <paramref name="op"/> completes.</param>
		/// <param name="userState">A user-defined state object that is passed as second argument to <paramref name="action"/>.</param>
		/// <param name="options">Options for when the <paramref name="action"/> is executed.</param>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="action"/> is <see langword="null"/>.</exception>
		/// <returns>An operation that is executed after <paramref name="op"/> completes.</returns>
		public static IAsyncOperation<TResult> ContinueWith<TResult>(this IAsyncOperation op, Func<IAsyncOperation, object, TResult> action, object userState, AsyncContinuationOptions options)
		{
			if (action == null)
			{
				throw new ArgumentNullException(nameof(action));
			}

			return new ContinuationResult<TResult>(op, options, action, userState);
		}

		/// <summary>
		/// Creates a continuation that executes when the target <see cref="IAsyncOperation{TResult}"/> completes.
		/// </summary>
		/// <param name="op">The operation to continue.</param>
		/// <param name="action">An action to run when the <paramref name="op"/> completes.</param>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="action"/> is <see langword="null"/>.</exception>
		/// <returns>An operation that is executed after <paramref name="op"/> completes.</returns>
		public static IAsyncOperation ContinueWith<TResult>(this IAsyncOperation<TResult> op, Action<IAsyncOperation<TResult>> action)
		{
			return ContinueWith(op, action, AsyncContinuationOptions.CaptureSynchronizationContext);
		}

		/// <summary>
		/// Creates a continuation that executes when the target <see cref="IAsyncOperation{TResult}"/> completes.
		/// </summary>
		/// <param name="op">The operation to continue.</param>
		/// <param name="action">An action to run when the <paramref name="op"/> completes.</param>
		/// <param name="options">Options for when the <paramref name="action"/> is executed.</param>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="action"/> is <see langword="null"/>.</exception>
		/// <returns>An operation that is executed after <paramref name="op"/> completes.</returns>
		public static IAsyncOperation ContinueWith<TResult>(this IAsyncOperation<TResult> op, Action<IAsyncOperation<TResult>> action, AsyncContinuationOptions options)
		{
			if (action == null)
			{
				throw new ArgumentNullException(nameof(action));
			}

			return new ContinuationResult<TResult, VoidResult>(op, options, action, null);
		}

		/// <summary>
		/// Creates a continuation that executes when the target <see cref="IAsyncOperation{TResult}"/> completes.
		/// </summary>
		/// <param name="op">The operation to continue.</param>
		/// <param name="action">An action to run when the <paramref name="op"/> completes.</param>
		/// <param name="userState">A user-defined state object that is passed as second argument to <paramref name="action"/>.</param>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="action"/> is <see langword="null"/>.</exception>
		/// <returns>An operation that is executed after <paramref name="op"/> completes.</returns>
		public static IAsyncOperation ContinueWith<TResult>(this IAsyncOperation<TResult> op, Action<IAsyncOperation<TResult>, object> action, object userState)
		{
			return ContinueWith(op, action, userState, AsyncContinuationOptions.CaptureSynchronizationContext);
		}

		/// <summary>
		/// Creates a continuation that executes when the target <see cref="IAsyncOperation{TResult}"/> completes.
		/// </summary>
		/// <param name="op">The operation to continue.</param>
		/// <param name="action">An action to run when the <paramref name="op"/> completes.</param>
		/// <param name="userState">A user-defined state object that is passed as second argument to <paramref name="action"/>.</param>
		/// <param name="options">Options for when the <paramref name="action"/> is executed.</param>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="action"/> is <see langword="null"/>.</exception>
		/// <returns>An operation that is executed after <paramref name="op"/> completes.</returns>
		public static IAsyncOperation ContinueWith<TResult>(this IAsyncOperation<TResult> op, Action<IAsyncOperation<TResult>, object> action, object userState, AsyncContinuationOptions options)
		{
			if (action == null)
			{
				throw new ArgumentNullException(nameof(action));
			}

			return new ContinuationResult<TResult, VoidResult>(op, options, action, userState);
		}

		/// <summary>
		/// Creates a continuation that executes when the target <see cref="IAsyncOperation"/> completes.
		/// </summary>
		/// <param name="op">The operation to continue.</param>
		/// <param name="action">An action to run when the <paramref name="op"/> completes.</param>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="action"/> is <see langword="null"/>.</exception>
		/// <returns>An operation that is executed after <paramref name="op"/> completes.</returns>
		public static IAsyncOperation<TNewResult> ContinueWith<TResult, TNewResult>(this IAsyncOperation<TResult> op, Func<IAsyncOperation<TResult>, TNewResult> action)
		{
			return ContinueWith(op, action, AsyncContinuationOptions.CaptureSynchronizationContext);
		}

		/// <summary>
		/// Creates a continuation that executes when the target <see cref="IAsyncOperation"/> completes.
		/// </summary>
		/// <param name="op">The operation to continue.</param>
		/// <param name="action">An action to run when the <paramref name="op"/> completes.</param>
		/// <param name="options">Options for when the <paramref name="action"/> is executed.</param>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="action"/> is <see langword="null"/>.</exception>
		/// <returns>An operation that is executed after <paramref name="op"/> completes.</returns>
		public static IAsyncOperation<TNewResult> ContinueWith<TResult, TNewResult>(this IAsyncOperation<TResult> op, Func<IAsyncOperation<TResult>, TNewResult> action, AsyncContinuationOptions options)
		{
			if (action == null)
			{
				throw new ArgumentNullException(nameof(action));
			}

			return new ContinuationResult<TResult, TNewResult>(op, options, action, null);
		}

		/// <summary>
		/// Creates a continuation that executes when the target <see cref="IAsyncOperation"/> completes.
		/// </summary>
		/// <param name="op">The operation to continue.</param>
		/// <param name="action">An action to run when the <paramref name="op"/> completes.</param>
		/// <param name="userState">A user-defined state object that is passed as second argument to <paramref name="action"/>.</param>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="action"/> is <see langword="null"/>.</exception>
		/// <returns>An operation that is executed after <paramref name="op"/> completes.</returns>
		public static IAsyncOperation<TNewResult> ContinueWith<TResult, TNewResult>(this IAsyncOperation<TResult> op, Func<IAsyncOperation<TResult>, object, TNewResult> action, object userState)
		{
			return ContinueWith(op, action, userState, AsyncContinuationOptions.CaptureSynchronizationContext);
		}

		/// <summary>
		/// Creates a continuation that executes when the target <see cref="IAsyncOperation"/> completes.
		/// </summary>
		/// <param name="op">The operation to continue.</param>
		/// <param name="action">An action to run when the <paramref name="op"/> completes.</param>
		/// <param name="userState">A user-defined state object that is passed as second argument to <paramref name="action"/>.</param>
		/// <param name="options">Options for when the <paramref name="action"/> is executed.</param>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="action"/> is <see langword="null"/>.</exception>
		/// <returns>An operation that is executed after <paramref name="op"/> completes.</returns>
		public static IAsyncOperation<TNewResult> ContinueWith<TResult, TNewResult>(this IAsyncOperation<TResult> op, Func<IAsyncOperation<TResult>, object, TNewResult> action, object userState, AsyncContinuationOptions options)
		{
			if (action == null)
			{
				throw new ArgumentNullException(nameof(action));
			}

			return new ContinuationResult<TResult, TNewResult>(op, options, action, userState);
		}

		#endregion

		#region Unwrap

		/// <summary>
		/// Creates a proxy <see cref="IAsyncOperation"/> that represents the asynchronous operation of a <c>IAsyncOperation&lt;IAsyncOperation&gt;</c>.
		/// </summary>
		/// <param name="op">The source operation.</param>
		/// <returns>The unwrapped operation.</returns>
		public static IAsyncOperation Unwrap(this IAsyncOperation<IAsyncOperation> op)
		{
			return new UnwrapResult<VoidResult>(op);
		}

		/// <summary>
		/// Creates a proxy <see cref="IAsyncOperation{TResult}"/> that represents the asynchronous operation of a <c>IAsyncOperation&lt;IAsyncOperation&lt;TResult&gt;&gt;</c>.
		/// </summary>
		/// <param name="op">The source operation.</param>
		/// <returns>The unwrapped operation.</returns>
		public static IAsyncOperation<TResult> Unwrap<TResult>(this IAsyncOperation<IAsyncOperation<TResult>> op)
		{
			return new UnwrapResult<TResult>(op);
		}

		#endregion
	}
}
