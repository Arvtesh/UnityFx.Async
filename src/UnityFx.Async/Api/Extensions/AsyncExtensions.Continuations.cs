// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Threading;

namespace UnityFx.Async
{
	partial class AsyncExtensions
	{
		#region AddCompletionCallback

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
			if (action == null)
			{
				throw new ArgumentNullException(nameof(action));
			}

			var result = new AsyncCompletionSource(AsyncOperationStatus.Running);

			op.AddCompletionCallback(asyncOp =>
			{
				try
				{
					action(asyncOp);
					result.TrySetCompleted();
				}
				catch (Exception e)
				{
					result.TrySetException(e, false);
				}
			});

			return result;
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

			var result = new AsyncCompletionSource(AsyncOperationStatus.Running);
			var syncContext = (options & AsyncContinuationOptions.CaptureSynchronizationContext) != 0 ? SynchronizationContext.Current : null;

			op.AddCompletionCallback(
				asyncOp =>
				{
					try
					{
						if (AsyncContinuation.CanInvoke(asyncOp, options))
						{
							action(asyncOp);
							result.TrySetCompleted();
						}
						else
						{
							result.TrySetCanceled();
						}
					}
					catch (Exception e)
					{
						result.TrySetException(e, false);
					}
				},
				syncContext);

			return result;
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
			if (action == null)
			{
				throw new ArgumentNullException(nameof(action));
			}

			var result = new AsyncCompletionSource(AsyncOperationStatus.Running);

			op.AddCompletionCallback(asyncOp =>
			{
				try
				{
					action(asyncOp, userState);
					result.TrySetCompleted();
				}
				catch (Exception e)
				{
					result.TrySetException(e, false);
				}
			});

			return result;
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

			var result = new AsyncCompletionSource(AsyncOperationStatus.Running);
			var syncContext = (options & AsyncContinuationOptions.CaptureSynchronizationContext) != 0 ? SynchronizationContext.Current : null;

			op.AddCompletionCallback(
				asyncOp =>
				{
					try
					{
						if (AsyncContinuation.CanInvoke(asyncOp, options))
						{
							action(asyncOp, userState);
							result.TrySetCompleted();
						}
						else
						{
							result.TrySetCanceled();
						}
					}
					catch (Exception e)
					{
						result.TrySetException(e, false);
					}
				},
				syncContext);

			return result;
		}

		/// <summary>
		/// Creates a continuation that executes when the target <see cref="IAsyncOperation"/> completes.
		/// </summary>
		/// <param name="op">The operation to continue.</param>
		/// <param name="action">An action to run when the <paramref name="op"/> completes.</param>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="action"/> is <see langword="null"/>.</exception>
		/// <returns>An operation that is executed after <paramref name="op"/> completes.</returns>
		public static IAsyncOperation<T> ContinueWith<T>(this IAsyncOperation op, Func<IAsyncOperation, T> action)
		{
			if (action == null)
			{
				throw new ArgumentNullException(nameof(action));
			}

			var result = new AsyncCompletionSource<T>(AsyncOperationStatus.Running);

			op.AddCompletionCallback(asyncOp =>
			{
				try
				{
					var resultValue = action(asyncOp);
					result.TrySetResult(resultValue);
				}
				catch (Exception e)
				{
					result.TrySetException(e, false);
				}
			});

			return result;
		}

		/// <summary>
		/// Creates a continuation that executes when the target <see cref="IAsyncOperation"/> completes.
		/// </summary>
		/// <param name="op">The operation to continue.</param>
		/// <param name="action">An action to run when the <paramref name="op"/> completes.</param>
		/// <param name="options">Options for when the <paramref name="action"/> is executed.</param>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="action"/> is <see langword="null"/>.</exception>
		/// <returns>An operation that is executed after <paramref name="op"/> completes.</returns>
		public static IAsyncOperation<T> ContinueWith<T>(this IAsyncOperation op, Func<IAsyncOperation, T> action, AsyncContinuationOptions options)
		{
			if (action == null)
			{
				throw new ArgumentNullException(nameof(action));
			}

			var result = new AsyncCompletionSource<T>(AsyncOperationStatus.Running);
			var syncContext = (options & AsyncContinuationOptions.CaptureSynchronizationContext) != 0 ? SynchronizationContext.Current : null;

			op.AddCompletionCallback(
				asyncOp =>
				{
					try
					{
						if (AsyncContinuation.CanInvoke(asyncOp, options))
						{
							var resultValue = action(asyncOp);
							result.TrySetResult(resultValue);
						}
						else
						{
							result.TrySetCanceled();
						}
					}
					catch (Exception e)
					{
						result.TrySetException(e, false);
					}
				},
				syncContext);

			return result;
		}

		/// <summary>
		/// Creates a continuation that executes when the target <see cref="IAsyncOperation"/> completes.
		/// </summary>
		/// <param name="op">The operation to continue.</param>
		/// <param name="action">An action to run when the <paramref name="op"/> completes.</param>
		/// <param name="userState">A user-defined state object that is passed as second argument to <paramref name="action"/>.</param>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="action"/> is <see langword="null"/>.</exception>
		/// <returns>An operation that is executed after <paramref name="op"/> completes.</returns>
		public static IAsyncOperation<T> ContinueWith<T>(this IAsyncOperation op, Func<IAsyncOperation, object, T> action, object userState)
		{
			if (action == null)
			{
				throw new ArgumentNullException(nameof(action));
			}

			var result = new AsyncCompletionSource<T>(AsyncOperationStatus.Running);

			op.AddCompletionCallback(asyncOp =>
			{
				try
				{
					var resultValue = action(asyncOp, userState);
					result.TrySetResult(resultValue);
				}
				catch (Exception e)
				{
					result.TrySetException(e, false);
				}
			});

			return result;
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
		public static IAsyncOperation<T> ContinueWith<T>(this IAsyncOperation op, Func<IAsyncOperation, object, T> action, object userState, AsyncContinuationOptions options)
		{
			if (action == null)
			{
				throw new ArgumentNullException(nameof(action));
			}

			var result = new AsyncCompletionSource<T>(AsyncOperationStatus.Running);
			var syncContext = (options & AsyncContinuationOptions.CaptureSynchronizationContext) != 0 ? SynchronizationContext.Current : null;

			op.AddCompletionCallback(
				asyncOp =>
				{
					try
					{
						if (AsyncContinuation.CanInvoke(asyncOp, options))
						{
							var resultValue = action(asyncOp, userState);
							result.TrySetResult(resultValue);
						}
						else
						{
							result.TrySetCanceled();
						}
					}
					catch (Exception e)
					{
						result.TrySetException(e, false);
					}
				},
				syncContext);

			return result;
		}

		/// <summary>
		/// Creates a continuation that executes when the target <see cref="IAsyncOperation{T}"/> completes.
		/// </summary>
		/// <param name="op">The operation to continue.</param>
		/// <param name="action">An action to run when the <paramref name="op"/> completes.</param>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="action"/> is <see langword="null"/>.</exception>
		/// <returns>An operation that is executed after <paramref name="op"/> completes.</returns>
		public static IAsyncOperation ContinueWith<T>(this IAsyncOperation<T> op, Action<IAsyncOperation<T>> action)
		{
			if (action == null)
			{
				throw new ArgumentNullException(nameof(action));
			}

			var result = new AsyncCompletionSource(AsyncOperationStatus.Running);

			op.AddCompletionCallback(asyncOp =>
			{
				try
				{
					action(asyncOp as IAsyncOperation<T>);
					result.TrySetCompleted();
				}
				catch (Exception e)
				{
					result.TrySetException(e, false);
				}
			});

			return result;
		}

		/// <summary>
		/// Creates a continuation that executes when the target <see cref="IAsyncOperation{T}"/> completes.
		/// </summary>
		/// <param name="op">The operation to continue.</param>
		/// <param name="action">An action to run when the <paramref name="op"/> completes.</param>
		/// <param name="options">Options for when the <paramref name="action"/> is executed.</param>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="action"/> is <see langword="null"/>.</exception>
		/// <returns>An operation that is executed after <paramref name="op"/> completes.</returns>
		public static IAsyncOperation ContinueWith<T>(this IAsyncOperation<T> op, Action<IAsyncOperation<T>> action, AsyncContinuationOptions options)
		{
			if (action == null)
			{
				throw new ArgumentNullException(nameof(action));
			}

			var result = new AsyncCompletionSource(AsyncOperationStatus.Running);
			var syncContext = (options & AsyncContinuationOptions.CaptureSynchronizationContext) != 0 ? SynchronizationContext.Current : null;

			op.AddCompletionCallback(
				asyncOp =>
				{
					try
					{
						if (AsyncContinuation.CanInvoke(asyncOp, options))
						{
							action(asyncOp as IAsyncOperation<T>);
							result.TrySetCompleted();
						}
						else
						{
							result.TrySetCanceled();
						}
					}
					catch (Exception e)
					{
						result.TrySetException(e, false);
					}
				},
				syncContext);

			return result;
		}

		/// <summary>
		/// Creates a continuation that executes when the target <see cref="IAsyncOperation{T}"/> completes.
		/// </summary>
		/// <param name="op">The operation to continue.</param>
		/// <param name="action">An action to run when the <paramref name="op"/> completes.</param>
		/// <param name="userState">A user-defined state object that is passed as second argument to <paramref name="action"/>.</param>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="action"/> is <see langword="null"/>.</exception>
		/// <returns>An operation that is executed after <paramref name="op"/> completes.</returns>
		public static IAsyncOperation ContinueWith<T>(this IAsyncOperation<T> op, Action<IAsyncOperation<T>, object> action, object userState)
		{
			if (action == null)
			{
				throw new ArgumentNullException(nameof(action));
			}

			var result = new AsyncCompletionSource(AsyncOperationStatus.Running);

			op.AddCompletionCallback(asyncOp =>
			{
				try
				{
					action(asyncOp as IAsyncOperation<T>, userState);
					result.TrySetCompleted();
				}
				catch (Exception e)
				{
					result.TrySetException(e, false);
				}
			});

			return result;
		}

		/// <summary>
		/// Creates a continuation that executes when the target <see cref="IAsyncOperation{T}"/> completes.
		/// </summary>
		/// <param name="op">The operation to continue.</param>
		/// <param name="action">An action to run when the <paramref name="op"/> completes.</param>
		/// <param name="userState">A user-defined state object that is passed as second argument to <paramref name="action"/>.</param>
		/// <param name="options">Options for when the <paramref name="action"/> is executed.</param>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="action"/> is <see langword="null"/>.</exception>
		/// <returns>An operation that is executed after <paramref name="op"/> completes.</returns>
		public static IAsyncOperation ContinueWith<T>(this IAsyncOperation<T> op, Action<IAsyncOperation<T>, object> action, object userState, AsyncContinuationOptions options)
		{
			if (action == null)
			{
				throw new ArgumentNullException(nameof(action));
			}

			var result = new AsyncCompletionSource(AsyncOperationStatus.Running);
			var syncContext = (options & AsyncContinuationOptions.CaptureSynchronizationContext) != 0 ? SynchronizationContext.Current : null;

			op.AddCompletionCallback(
				asyncOp =>
				{
					try
					{
						if (AsyncContinuation.CanInvoke(asyncOp, options))
						{
							action(asyncOp as IAsyncOperation<T>, userState);
							result.TrySetCompleted();
						}
						else
						{
							result.TrySetCanceled();
						}
					}
					catch (Exception e)
					{
						result.TrySetException(e, false);
					}
				},
				syncContext);

			return result;
		}

		/// <summary>
		/// Creates a continuation that executes when the target <see cref="IAsyncOperation"/> completes.
		/// </summary>
		/// <param name="op">The operation to continue.</param>
		/// <param name="action">An action to run when the <paramref name="op"/> completes.</param>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="action"/> is <see langword="null"/>.</exception>
		/// <returns>An operation that is executed after <paramref name="op"/> completes.</returns>
		public static IAsyncOperation<U> ContinueWith<T, U>(this IAsyncOperation<T> op, Func<IAsyncOperation<T>, U> action)
		{
			if (action == null)
			{
				throw new ArgumentNullException(nameof(action));
			}

			var result = new AsyncCompletionSource<U>(AsyncOperationStatus.Running);

			op.AddCompletionCallback(asyncOp =>
			{
				try
				{
					var resultValue = action(asyncOp as IAsyncOperation<T>);
					result.TrySetResult(resultValue);
				}
				catch (Exception e)
				{
					result.TrySetException(e, false);
				}
			});

			return result;
		}

		/// <summary>
		/// Creates a continuation that executes when the target <see cref="IAsyncOperation"/> completes.
		/// </summary>
		/// <param name="op">The operation to continue.</param>
		/// <param name="action">An action to run when the <paramref name="op"/> completes.</param>
		/// <param name="options">Options for when the <paramref name="action"/> is executed.</param>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="action"/> is <see langword="null"/>.</exception>
		/// <returns>An operation that is executed after <paramref name="op"/> completes.</returns>
		public static IAsyncOperation<U> ContinueWith<T, U>(this IAsyncOperation<T> op, Func<IAsyncOperation<T>, U> action, AsyncContinuationOptions options)
		{
			if (action == null)
			{
				throw new ArgumentNullException(nameof(action));
			}

			var result = new AsyncCompletionSource<U>(AsyncOperationStatus.Running);
			var syncContext = (options & AsyncContinuationOptions.CaptureSynchronizationContext) != 0 ? SynchronizationContext.Current : null;

			op.AddCompletionCallback(
				asyncOp =>
				{
					try
					{
						if (AsyncContinuation.CanInvoke(asyncOp, options))
						{
							var resultValue = action(asyncOp as IAsyncOperation<T>);
							result.TrySetResult(resultValue);
						}
						else
						{
							result.TrySetCanceled();
						}
					}
					catch (Exception e)
					{
						result.TrySetException(e, false);
					}
				},
				syncContext);

			return result;
		}

		/// <summary>
		/// Creates a continuation that executes when the target <see cref="IAsyncOperation"/> completes.
		/// </summary>
		/// <param name="op">The operation to continue.</param>
		/// <param name="action">An action to run when the <paramref name="op"/> completes.</param>
		/// <param name="userState">A user-defined state object that is passed as second argument to <paramref name="action"/>.</param>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="action"/> is <see langword="null"/>.</exception>
		/// <returns>An operation that is executed after <paramref name="op"/> completes.</returns>
		public static IAsyncOperation<U> ContinueWith<T, U>(this IAsyncOperation<T> op, Func<IAsyncOperation<T>, object, U> action, object userState)
		{
			if (action == null)
			{
				throw new ArgumentNullException(nameof(action));
			}

			var result = new AsyncCompletionSource<U>(AsyncOperationStatus.Running);

			op.AddCompletionCallback(asyncOp =>
			{
				try
				{
					var resultValue = action(asyncOp as IAsyncOperation<T>, userState);
					result.TrySetResult(resultValue);
				}
				catch (Exception e)
				{
					result.TrySetException(e, false);
				}
			});

			return result;
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
		public static IAsyncOperation<U> ContinueWith<T, U>(this IAsyncOperation<T> op, Func<IAsyncOperation<T>, object, U> action, object userState, AsyncContinuationOptions options)
		{
			if (action == null)
			{
				throw new ArgumentNullException(nameof(action));
			}

			var result = new AsyncCompletionSource<U>(AsyncOperationStatus.Running);
			var syncContext = (options & AsyncContinuationOptions.CaptureSynchronizationContext) != 0 ? SynchronizationContext.Current : null;

			op.AddCompletionCallback(
				asyncOp =>
				{
					try
					{
						if (AsyncContinuation.CanInvoke(asyncOp, options))
						{
							var resultValue = action(asyncOp as IAsyncOperation<T>, userState);
							result.TrySetResult(resultValue);
						}
						else
						{
							result.TrySetCanceled();
						}
					}
					catch (Exception e)
					{
						result.TrySetException(e, false);
					}
				},
				syncContext);

			return result;
		}

		#endregion
	}
}
