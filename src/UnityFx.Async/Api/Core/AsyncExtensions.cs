﻿// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.ComponentModel;
#if !NET35
using System.Runtime.ExceptionServices;
#endif
using System.Threading;
#if UNITYFX_SUPPORT_TAP
using System.Threading.Tasks;
#endif

namespace UnityFx.Async
{
	/// <summary>
	/// Extension methods for <see cref="IAsyncOperation"/> related classes.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	public static class AsyncExtensions
	{
		#region IAsyncOperation

		/// <summary>
		/// Blocks calling thread until the operation is completed.
		/// </summary>
		/// <param name="op">The operation to wait for.</param>
		/// <seealso cref="Join(IAsyncOperation)"/>
		/// <seealso cref="Join{T}(IAsyncOperation{T})"/>
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
		/// <param name="op">The operation to join.</param>
		/// <seealso cref="Join{T}(IAsyncOperation{T})"/>
		/// <seealso cref="Wait(IAsyncOperation)"/>
		public static void Join(this IAsyncOperation op)
		{
			Wait(op);
			ThrowIfFaulted(op);
		}

		/// <summary>
		/// Blocks calling thread until the operation is completed. After that rethrows the operation exception (if any).
		/// </summary>
		/// <param name="op">The operation to join.</param>
		/// <seealso cref="Join(IAsyncOperation)"/>
		/// <seealso cref="Wait(IAsyncOperation)"/>
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

		/// <summary>
		/// Adds a completion callback to be executed after the operation has finished. If the operation is completed the <paramref name="action"/> is invoked synchronously.
		/// </summary>
		/// <param name="op">The target operation.</param>
		/// <param name="action">The callback to be executed when the operation has completed.</param>
		/// <param name="continueOnCapturedContext">If <see langword="true"/> method attempts to marshal the continuation back to the current synchronization context.
		/// Otherwise the callback is invoked on a thread that initiated the operation completion.
		/// </param>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="action"/> is <see langword="null"/>.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation has been disposed.</exception>
		/// <seealso cref="IAsyncOperationEvents"/>
		public static void AddCompletionCallback(this IAsyncOperation op, AsyncOperationCallback action, bool continueOnCapturedContext)
		{
			var context = continueOnCapturedContext ? SynchronizationContext.Current : null;

			if (!op.TryAddCompletionCallback(action, context))
			{
				action(op);
			}
		}

		/// <summary>
		/// Creates a continuation that executes when the target <see cref="IAsyncOperation"/> completes.
		/// </summary>
		/// <remarks>
		/// <para>The <paramref name="action"/> is expected to start another asynchronous operation. When that operation is completed it
		/// should use the second <paramref name="action"/> argument to complete the continuation. If the <paramref name="op"/>
		/// is already completed the <paramref name="action"/> is being called synchronously.</para>
		/// <para>Continuation behaviour is very close to TPL: if <see cref="SynchronizationContext"/> is set the continuation posted onto it.
		/// Otherwise it is executed on a thread that initiated the operation completion.</para>
		/// </remarks>
		/// <typeparam name="T">Type of the operation to continue.</typeparam>
		/// <param name="op">The operation to continue.</param>
		/// <param name="action">An action to run when the <paramref name="op"/> completes.</param>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="action"/> is <see langword="null"/>.</exception>
		/// <returns>An operation that is executed after <paramref name="op"/> completes.</returns>
		/// <seealso cref="ContinueWith{T}(T, Action{T, IAsyncCompletionSource, object}, object)"/>
		/// <seealso cref="ContinueWith{T, U}(T, Action{T, IAsyncCompletionSource{U}})"/>
		/// <seealso cref="TransformWith{T, U}(T, Func{T, U})"/>
		public static IAsyncOperation ContinueWith<T>(this T op, Action<T, IAsyncCompletionSource> action) where T : IAsyncOperation
		{
			if (action == null)
			{
				throw new ArgumentNullException(nameof(action));
			}

			var result = new AsyncResult(AsyncOperationStatus.Scheduled);

			op.AddCompletionCallback(
				asyncOp =>
				{
					try
					{
						result.SetRunning();
						action(op, result);
					}
					catch (Exception e)
					{
						result.TrySetException(e, false);
					}
				},
				true);

			return result;
		}

		/// <summary>
		/// Creates a continuation that executes when the target <see cref="IAsyncOperation"/> completes.
		/// </summary>
		/// <remarks>
		/// See <see cref="ContinueWith{T}(T, Action{T, IAsyncCompletionSource})">ContinueWith</see> remarks for details.
		/// </remarks>
		/// <typeparam name="T">Type of the operation to continue.</typeparam>
		/// <param name="op">The operation to continue.</param>
		/// <param name="action">An action to run when the <paramref name="op"/> completes.</param>
		/// <param name="state">User-defined state that is passed as last argument of <paramref name="action"/>.</param>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="action"/> is <see langword="null"/>.</exception>
		/// <returns>An operation that is executed after <paramref name="op"/> completes.</returns>
		/// <seealso cref="ContinueWith{T}(T, Action{T, IAsyncCompletionSource})"/>
		/// <seealso cref="ContinueWith{T, U}(T, Action{T, IAsyncCompletionSource{U}})"/>
		/// <seealso cref="TransformWith{T, U}(T, Func{T, U})"/>
		public static IAsyncOperation ContinueWith<T>(this T op, Action<T, IAsyncCompletionSource, object> action, object state) where T : IAsyncOperation
		{
			if (action == null)
			{
				throw new ArgumentNullException(nameof(action));
			}

			var result = new AsyncResult(AsyncOperationStatus.Scheduled);

			op.AddCompletionCallback(
				asyncOp =>
				{
					try
					{
						result.SetRunning();
						action(op, result, state);
					}
					catch (Exception e)
					{
						result.TrySetException(e, false);
					}
				},
				true);

			return result;
		}

		/// <summary>
		/// Creates a continuation that executes when the target <see cref="IAsyncOperation"/> completes.
		/// </summary>
		/// <remarks>
		/// See <see cref="ContinueWith{T}(T, Action{T, IAsyncCompletionSource})">ContinueWith</see> remarks for details.
		/// </remarks>
		/// <typeparam name="T">Type of the operation to continue.</typeparam>
		/// <typeparam name="U">Result type of the continuation operation.</typeparam>
		/// <param name="op">The operation to continue.</param>
		/// <param name="action">An action to run when the <paramref name="op"/> completes.</param>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="action"/> is <see langword="null"/>.</exception>
		/// <returns>An operation that is executed after <paramref name="op"/> completes.</returns>
		/// <seealso cref="ContinueWith{T, U}(T, Action{T, IAsyncCompletionSource{U}, object}, object)"/>
		/// <seealso cref="ContinueWith{T}(T, Action{T, IAsyncCompletionSource})"/>
		/// <seealso cref="TransformWith{T, U}(T, Func{T, U})"/>
		public static IAsyncOperation<U> ContinueWith<T, U>(this T op, Action<T, IAsyncCompletionSource<U>> action) where T : IAsyncOperation
		{
			if (action == null)
			{
				throw new ArgumentNullException(nameof(action));
			}

			var result = new AsyncResult<U>(AsyncOperationStatus.Scheduled);

			op.AddCompletionCallback(
				asyncOp =>
				{
					try
					{
						result.SetRunning();
						action(op, result);
					}
					catch (Exception e)
					{
						result.TrySetException(e, false);
					}
				},
				true);

			return result;
		}

		/// <summary>
		/// Creates a continuation that executes when the target <see cref="IAsyncOperation"/> completes.
		/// </summary>
		/// <remarks>
		/// See <see cref="ContinueWith{T}(T, Action{T, IAsyncCompletionSource})">ContinueWith</see> remarks for details.
		/// </remarks>
		/// <typeparam name="T">Type of the operation to continue.</typeparam>
		/// <typeparam name="U">Result type of the continuation operation.</typeparam>
		/// <param name="op">The operation to continue.</param>
		/// <param name="action">An action to run when the <paramref name="op"/> completes.</param>
		/// <param name="state">User-defined state that is passed as last argument of <paramref name="action"/>.</param>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="action"/> is <see langword="null"/>.</exception>
		/// <exception cref="NotSupportedException">Thrown if the target <see cref="IAsyncOperation"/> does not implement <see cref="IAsyncOperationEvents"/>.</exception>
		/// <returns>An operation that is executed after <paramref name="op"/> completes.</returns>
		/// <seealso cref="ContinueWith{T, U}(T, Action{T, IAsyncCompletionSource{U}})"/>
		/// <seealso cref="ContinueWith{T}(T, Action{T, IAsyncCompletionSource})"/>
		/// <seealso cref="TransformWith{T, U}(T, Func{T, U})"/>
		public static IAsyncOperation<U> ContinueWith<T, U>(this T op, Action<T, IAsyncCompletionSource<U>, object> action, object state) where T : IAsyncOperation
		{
			if (action == null)
			{
				throw new ArgumentNullException(nameof(action));
			}

			var result = new AsyncResult<U>(AsyncOperationStatus.Scheduled);

			op.AddCompletionCallback(
				asyncOp =>
				{
					try
					{
						result.SetRunning();
						action(op, result, state);
					}
					catch (Exception e)
					{
						result.TrySetException(e, false);
					}
				},
				true);

			return result;
		}

		/// <summary>
		/// Creates a continuation that transforms the target <see cref="IAsyncOperation"/> result.
		/// </summary>
		/// <typeparam name="T">Type of the operation to continue.</typeparam>
		/// <typeparam name="U">Result type of the continuation operation.</typeparam>
		/// <param name="op">The operation which result is to be transformed.</param>
		/// <param name="resultTransformer">A function used for the <paramref name="op"/> result transformation.</param>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="resultTransformer"/> is <see langword="null"/>.</exception>
		/// <returns>An operation with the transformed result vlaue.</returns>
		/// <seealso cref="ContinueWith{T}(T, Action{T, IAsyncCompletionSource})"/>
		/// <seealso cref="ContinueWith{T, U}(T, Action{T, IAsyncCompletionSource{U}})"/>
		public static IAsyncOperation<U> TransformWith<T, U>(this T op, Func<T, U> resultTransformer) where T : class, IAsyncOperation
		{
			if (resultTransformer == null)
			{
				throw new ArgumentNullException(nameof(resultTransformer));
			}

			var result = new AsyncResult<U>(AsyncOperationStatus.Scheduled);

			op.AddCompletionCallback(
				asyncOp =>
				{
					try
					{
						result.SetResult(resultTransformer(op), false);
					}
					catch (Exception e)
					{
						result.TrySetException(e, false);
					}
				},
				false);

			return result;
		}

#if UNITYFX_SUPPORT_TAP

		/// <summary>
		/// Returns the operation awaiter. This method is intended for compiler rather than use directly in code.
		/// </summary>
		/// <seealso cref="GetAwaiter{T}(IAsyncOperation{T})"/>
		public static AsyncResultAwaiter GetAwaiter(this IAsyncOperation op)
		{
			return new AsyncResultAwaiter(op);
		}

		/// <summary>
		/// Returns the operation awaiter. This method is intended for compiler rather than use directly in code.
		/// </summary>
		/// <seealso cref="GetAwaiter(IAsyncOperation)"/>
		public static AsyncResultAwaiter<T> GetAwaiter<T>(this IAsyncOperation<T> op)
		{
			return new AsyncResultAwaiter<T>(op);
		}

		/// <summary>
		/// Creates a <see cref="Task"/> instance matching the source <see cref="IAsyncOperation"/>.
		/// </summary>
		/// <seealso cref="ToTask{T}(IAsyncOperation{T})"/>
		public static Task ToTask(this IAsyncOperation op)
		{
			var result = new TaskCompletionSource<object>();

			op.AddCompletionCallback(
				asyncOp =>
				{
					if (asyncOp.IsCompletedSuccessfully)
					{
						result.TrySetResult(null);
					}
					else if (asyncOp.IsCanceled)
					{
						result.TrySetCanceled();
					}
					else
					{
						result.TrySetException(asyncOp.Exception);
					}
				},
				false);

			return result.Task;
		}

		/// <summary>
		/// Creates a <see cref="Task"/> instance matching the source <see cref="IAsyncOperation"/>.
		/// </summary>
		/// <seealso cref="ToTask(IAsyncOperation)"/>
		public static Task<T> ToTask<T>(this IAsyncOperation<T> op)
		{
			var result = new TaskCompletionSource<T>();

			op.AddCompletionCallback(
				asyncOp =>
				{
					if (op.IsCompletedSuccessfully)
					{
						result.TrySetResult(op.Result);
					}
					else if (op.IsCanceled)
					{
						result.TrySetCanceled();
					}
					else
					{
						result.TrySetException(op.Exception);
					}
				},
				false);

			return result.Task;
		}

#endif

#if !NET35

		/// <summary>
		/// Creates a <see cref="IObservable{T}"/> instance that can be used to track the source operation progress.
		/// </summary>
		/// <typeparam name="T">Type of the operation result.</typeparam>
		/// <param name="op">The operation to track.</param>
		/// <returns>Returns an <see cref="IObservable{T}"/> instance that can be used to track the operation.</returns>
		public static IObservable<T> ToObservable<T>(this IAsyncOperation<T> op)
		{
			if (op is AsyncResult<T> ar)
			{
				return ar;
			}

			return new AsyncObservable<T>(op);
		}

#endif

		#endregion
	}
}