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
	/// Extension methods for <see cref="IAsyncOperation"/> related classes.
	/// </summary>
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
		/// Creates a continuation that executes when the target <see cref="IAsyncOperation"/> completes.
		/// </summary>
		/// <remarks>
		/// The <paramref name="action"/> is expected to start another asynchronous operation. When the operation is completed it
		/// should use the second <paramref name="action"/> argument to complete the continuation. If the <paramref name="op"/>
		/// is already completed the <paramref name="action"/> is being called synchronously.
		/// </remarks>
		/// <typeparam name="T">Type of the operation to continue.</typeparam>
		/// <param name="op">The operation to continue.</param>
		/// <param name="action">An action to run when the <paramref name="op"/> completes.</param>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="action"/> is <see langword="null"/>.</exception>
		/// <exception cref="NotSupportedException">Thrown if the target <see cref="IAsyncOperation"/> does not implement <see cref="IAsyncContinuationController"/>.</exception>
		/// <returns>An operation that is executed after <paramref name="op"/> completes.</returns>
		/// <seealso cref="ContinueWith{T}(T, Action{T, IAsyncOperationCompletionSource, object}, object)"/>
		/// <seealso cref="ContinueWith{T, U}(T, Action{T, IAsyncOperationCompletionSource{U}})"/>
		/// <seealso cref="TransformWith{T, U}(T, Func{T, U})"/>
		public static IAsyncOperation ContinueWith<T>(this T op, Action<T, IAsyncOperationCompletionSource> action) where T : IAsyncOperation
		{
			if (action == null)
			{
				throw new ArgumentNullException(nameof(action));
			}

			if (op is IAsyncContinuationController c)
			{
				var result = new AsyncResult(false);

				c.AddCompletionCallback(() =>
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
				});

				return result;
			}
			else
			{
				throw new NotSupportedException();
			}
		}

		/// <summary>
		/// Creates a continuation that executes when the target <see cref="IAsyncOperation"/> completes.
		/// </summary>
		/// <remarks>
		/// The <paramref name="action"/> is expected to start another asynchronous operation. When the operation is completed it
		/// should use the second <paramref name="action"/> argument to complete the continuation. If the <paramref name="op"/>
		/// is already completed the <paramref name="action"/> is being called synchronously.
		/// </remarks>
		/// <typeparam name="T">Type of the operation to continue.</typeparam>
		/// <param name="op">The operation to continue.</param>
		/// <param name="action">An action to run when the <paramref name="op"/> completes.</param>
		/// <param name="state">User-defined state that is passed as last argument of <paramref name="action"/>.</param>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="action"/> is <see langword="null"/>.</exception>
		/// <exception cref="NotSupportedException">Thrown if the target <see cref="IAsyncOperation"/> does not implement <see cref="IAsyncContinuationController"/>.</exception>
		/// <returns>An operation that is executed after <paramref name="op"/> completes.</returns>
		/// <seealso cref="ContinueWith{T}(T, Action{T, IAsyncOperationCompletionSource})"/>
		/// <seealso cref="ContinueWith{T, U}(T, Action{T, IAsyncOperationCompletionSource{U}})"/>
		/// <seealso cref="TransformWith{T, U}(T, Func{T, U})"/>
		public static IAsyncOperation ContinueWith<T>(this T op, Action<T, IAsyncOperationCompletionSource, object> action, object state) where T : IAsyncOperation
		{
			if (action == null)
			{
				throw new ArgumentNullException(nameof(action));
			}

			if (op is IAsyncContinuationController c)
			{
				var result = new AsyncResult(false);

				c.AddCompletionCallback(() =>
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
				});

				return result;
			}
			else
			{
				throw new NotSupportedException();
			}
		}

		/// <summary>
		/// Creates a continuation that executes when the target <see cref="IAsyncOperation"/> completes.
		/// </summary>
		/// <remarks>
		/// The <paramref name="action"/> is expected to start another asynchronous operation. When the operation is completed it
		/// should use the second <paramref name="action"/> argument to complete the continuation. If the <paramref name="op"/>
		/// is already completed the <paramref name="action"/> is being called synchronously.
		/// </remarks>
		/// <typeparam name="T">Type of the operation to continue.</typeparam>
		/// <typeparam name="U">Result type of the continuation operation.</typeparam>
		/// <param name="op">The operation to continue.</param>
		/// <param name="action">An action to run when the <paramref name="op"/> completes.</param>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="action"/> is <see langword="null"/>.</exception>
		/// <exception cref="NotSupportedException">Thrown if the target <see cref="IAsyncOperation"/> does not implement <see cref="IAsyncContinuationController"/>.</exception>
		/// <returns>An operation that is executed after <paramref name="op"/> completes.</returns>
		/// <seealso cref="ContinueWith{T, U}(T, Action{T, IAsyncOperationCompletionSource{U}, object}, object)"/>
		/// <seealso cref="ContinueWith{T}(T, Action{T, IAsyncOperationCompletionSource})"/>
		/// <seealso cref="TransformWith{T, U}(T, Func{T, U})"/>
		public static IAsyncOperation<U> ContinueWith<T, U>(this T op, Action<T, IAsyncOperationCompletionSource<U>> action) where T : IAsyncOperation
		{
			if (action == null)
			{
				throw new ArgumentNullException(nameof(action));
			}

			if (op is IAsyncContinuationController c)
			{
				var result = new AsyncResult<U>(false);

				c.AddCompletionCallback(() =>
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
				});

				return result;
			}
			else
			{
				throw new NotSupportedException();
			}
		}

		/// <summary>
		/// Creates a continuation that executes when the target <see cref="IAsyncOperation"/> completes.
		/// </summary>
		/// <remarks>
		/// The <paramref name="action"/> is expected to start another asynchronous operation. When the operation is completed it
		/// should use the second <paramref name="action"/> argument to complete the continuation. If the <paramref name="op"/>
		/// is already completed the <paramref name="action"/> is being called synchronously.
		/// </remarks>
		/// <typeparam name="T">Type of the operation to continue.</typeparam>
		/// <typeparam name="U">Result type of the continuation operation.</typeparam>
		/// <param name="op">The operation to continue.</param>
		/// <param name="action">An action to run when the <paramref name="op"/> completes.</param>
		/// <param name="state">User-defined state that is passed as last argument of <paramref name="action"/>.</param>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="action"/> is <see langword="null"/>.</exception>
		/// <exception cref="NotSupportedException">Thrown if the target <see cref="IAsyncOperation"/> does not implement <see cref="IAsyncContinuationController"/>.</exception>
		/// <returns>An operation that is executed after <paramref name="op"/> completes.</returns>
		/// <seealso cref="ContinueWith{T, U}(T, Action{T, IAsyncOperationCompletionSource{U}})"/>
		/// <seealso cref="ContinueWith{T}(T, Action{T, IAsyncOperationCompletionSource})"/>
		/// <seealso cref="TransformWith{T, U}(T, Func{T, U})"/>
		public static IAsyncOperation<U> ContinueWith<T, U>(this T op, Action<T, IAsyncOperationCompletionSource<U>, object> action, object state) where T : IAsyncOperation
		{
			if (action == null)
			{
				throw new ArgumentNullException(nameof(action));
			}

			if (op is IAsyncContinuationController c)
			{
				var result = new AsyncResult<U>(false);

				c.AddCompletionCallback(() =>
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
				});

				return result;
			}
			else
			{
				throw new NotSupportedException();
			}
		}

		/// <summary>
		/// Creates a continuation that transforms the target <see cref="IAsyncOperation"/> result.
		/// </summary>
		/// <typeparam name="T">Type of the operation to continue.</typeparam>
		/// <typeparam name="U">Result type of the continuation operation.</typeparam>
		/// <param name="op">The operation which result is to be transformed.</param>
		/// <param name="resultTransformer">A function used for the <paramref name="op"/> result transformation.</param>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="resultTransformer"/> is <see langword="null"/>.</exception>
		/// <exception cref="NotSupportedException">Thrown if the target <see cref="IAsyncOperation"/> does not implement <see cref="IAsyncContinuationController"/>.</exception>
		/// <returns>An operation with the transformed result vlaue.</returns>
		/// <seealso cref="ContinueWith{T}(T, Action{T, IAsyncOperationCompletionSource})"/>
		/// <seealso cref="ContinueWith{T, U}(T, Action{T, IAsyncOperationCompletionSource{U}})"/>
		public static IAsyncOperation<U> TransformWith<T, U>(this T op, Func<T, U> resultTransformer) where T : class, IAsyncOperation
		{
			if (resultTransformer == null)
			{
				throw new ArgumentNullException(nameof(resultTransformer));
			}

			if (op is IAsyncContinuationController c)
			{
				var result = new AsyncResult<U>(false);

				c.AddCompletionCallback(() =>
				{
					try
					{
						result.SetResult(resultTransformer(op), false);
					}
					catch (Exception e)
					{
						result.TrySetException(e, false);
					}
				});

				return result;
			}
			else
			{
				throw new NotSupportedException();
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
		/// <exception cref="NotSupportedException">Thrown if the target <see cref="IAsyncOperation"/> does not implement <see cref="IAsyncContinuationController"/>.</exception>
		public static Task ToTask(this IAsyncOperation op)
		{
			if (op is IAsyncContinuationController c)
			{
				var result = new TaskCompletionSource<object>();

				c.AddCompletionCallback(() =>
				{
					if (op.IsCompletedSuccessfully)
					{
						result.TrySetResult(null);
					}
					else if (op.IsCanceled)
					{
						result.TrySetCanceled();
					}
					else
					{
						result.TrySetException(op.Exception);
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
		/// <exception cref="NotSupportedException">Thrown if the target <see cref="IAsyncOperation"/> does not implement <see cref="IAsyncContinuationController"/>.</exception>
		public static Task<T> ToTask<T>(this IAsyncOperation<T> op)
		{
			if (op is IAsyncContinuationController c)
			{
				var result = new TaskCompletionSource<T>();

				c.AddCompletionCallback(() =>
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
