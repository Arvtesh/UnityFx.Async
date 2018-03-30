// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
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

		#region common

		/// <summary>
		/// Spins until the operation has completed.
		/// </summary>
		public static void SpinUntilCompleted(this IAsyncOperation op)
		{
#if NET35

			while (!op.IsCompleted)
			{
				Thread.SpinWait(1);
			}

#else

			var sw = new SpinWait();

			while (!op.IsCompleted)
			{
				sw.SpinOnce();
			}

#endif
		}

		/// <summary>
		/// Throws exception if the operation has failed or canceled.
		/// </summary>
		internal static void ThrowIfNonSuccess(IAsyncOperation op, bool throwAggregate)
		{
			if (op is AsyncResult ar)
			{
				ar.ThrowIfNonSuccess(throwAggregate);
			}
			else
			{
				var status = op.Status;

				if (status == AsyncOperationStatus.Faulted)
				{
					if (throwAggregate)
					{
						throw op.Exception;
					}
					else if (!AsyncResult.TryThrowException(op.Exception))
					{
						// Should never get here. If faulted state excpetion should not be null.
						throw new Exception();
					}
				}
				else if (status == AsyncOperationStatus.Canceled)
				{
					if (throwAggregate)
					{
						throw op.Exception;
					}
					else if (!AsyncResult.TryThrowException(op.Exception))
					{
						throw new OperationCanceledException();
					}
				}
			}
		}

		#endregion

		#region Wait

		/// <summary>
		/// Waits for the <see cref="IAsyncOperation"/> to complete execution.
		/// </summary>
		/// <param name="op">The operation to wait for.</param>
		/// <exception cref="AggregateException">Thrown if the operation was canceled or faulted.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation is disposed.</exception>
		/// <seealso cref="Wait(IAsyncOperation, int)"/>
		/// <seealso cref="Wait(IAsyncOperation, TimeSpan)"/>
		public static void Wait(this IAsyncOperation op)
		{
			if (!op.IsCompleted)
			{
				op.AsyncWaitHandle.WaitOne();
			}

			ThrowIfNonSuccess(op, true);
		}

		/// <summary>
		/// Waits for the <see cref="IAsyncOperation"/> to complete execution within a specified number of milliseconds.
		/// </summary>
		/// <param name="op">The operation to wait for.</param>
		/// <param name="millisecondsTimeout">The number of milliseconds to wait, or <see cref="Timeout.Infinite"/> (-1) to wait indefinitely.</param>
		/// <returns><see langword="true"/> if the operation completed execution within the allotted time; otherwise, <see langword="false"/>.</returns>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="millisecondsTimeout"/> is a negative number other than -1.</exception>
		/// <exception cref="AggregateException">Thrown if the operation was canceled or faulted.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation is disposed.</exception>
		/// <seealso cref="Wait(IAsyncOperation)"/>
		/// <seealso cref="Wait(IAsyncOperation, TimeSpan)"/>
		public static bool Wait(this IAsyncOperation op, int millisecondsTimeout)
		{
			var result = true;

			if (!op.IsCompleted)
			{
				result = op.AsyncWaitHandle.WaitOne(millisecondsTimeout);
			}

			if (result)
			{
				ThrowIfNonSuccess(op, true);
			}

			return result;
		}

		/// <summary>
		/// Waits for the <see cref="IAsyncOperation"/> to complete execution within a specified time interval.
		/// </summary>
		/// <param name="op">The operation to wait for.</param>
		/// <param name="timeout">A <see cref="TimeSpan"/> that represents the number of milliseconds to wait, or a <see cref="TimeSpan"/> that represents -1 milliseconds to wait indefinitely.</param>
		/// <returns><see langword="true"/> if the operation completed execution within the allotted time; otherwise, <see langword="false"/>.</returns>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="timeout"/> is a negative number other than -1 milliseconds, or <paramref name="timeout"/> is greater than <see cref="int.MaxValue"/>.</exception>
		/// <exception cref="AggregateException">Thrown if the operation was canceled or faulted.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation is disposed.</exception>
		/// <seealso cref="Wait(IAsyncOperation)"/>
		/// <seealso cref="Wait(IAsyncOperation, int)"/>
		public static bool Wait(this IAsyncOperation op, TimeSpan timeout)
		{
			var result = true;

			if (!op.IsCompleted)
			{
				result = op.AsyncWaitHandle.WaitOne(timeout);
			}

			if (result)
			{
				ThrowIfNonSuccess(op, true);
			}

			return result;
		}

		#endregion

		#region Join

		/// <summary>
		/// Waits for the <see cref="IAsyncOperation"/> to complete execution. After that rethrows the operation exception (if any).
		/// </summary>
		/// <param name="op">The operation to join.</param>
		/// <exception cref="ObjectDisposedException">Thrown is the operation is disposed.</exception>
		/// <seealso cref="Join(IAsyncOperation, int)"/>
		/// <seealso cref="Join(IAsyncOperation, TimeSpan)"/>
		/// <seealso cref="Join{T}(IAsyncOperation{T})"/>
		public static void Join(this IAsyncOperation op)
		{
			if (!op.IsCompleted)
			{
				op.AsyncWaitHandle.WaitOne();
			}

			ThrowIfNonSuccess(op, false);
		}

		/// <summary>
		/// Waits for the <see cref="IAsyncOperation"/> to complete execution within a specified number of milliseconds. After that rethrows the operation exception (if any).
		/// </summary>
		/// <param name="op">The operation to wait for.</param>
		/// <param name="millisecondsTimeout">The number of milliseconds to wait, or <see cref="Timeout.Infinite"/> (-1) to wait indefinitely.</param>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="millisecondsTimeout"/> is a negative number other than -1.</exception>
		/// <exception cref="TimeoutException">Thrown if the operation did not completed within <paramref name="millisecondsTimeout"/>.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation is disposed.</exception>
		/// <seealso cref="Join(IAsyncOperation)"/>
		/// <seealso cref="Join(IAsyncOperation, TimeSpan)"/>
		/// <seealso cref="Join{T}(IAsyncOperation{T}, int)"/>
		public static void Join(this IAsyncOperation op, int millisecondsTimeout)
		{
			var result = true;

			if (!op.IsCompleted)
			{
				result = op.AsyncWaitHandle.WaitOne(millisecondsTimeout);
			}

			if (result)
			{
				ThrowIfNonSuccess(op, false);
			}
			else
			{
				throw new TimeoutException();
			}
		}

		/// <summary>
		/// Waits for the <see cref="IAsyncOperation"/> to complete execution within a specified timeout. After that rethrows the operation exception (if any).
		/// </summary>
		/// <param name="op">The operation to wait for.</param>
		/// <param name="timeout">A <see cref="TimeSpan"/> that represents the number of milliseconds to wait, or a <see cref="TimeSpan"/> that represents -1 milliseconds to wait indefinitely.</param>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="timeout"/> is a negative number other than -1 milliseconds, or <paramref name="timeout"/> is greater than <see cref="int.MaxValue"/>.</exception>
		/// <exception cref="TimeoutException">Thrown if the operation did not completed within <paramref name="timeout"/>.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation is disposed.</exception>
		/// <seealso cref="Join(IAsyncOperation)"/>
		/// <seealso cref="Join(IAsyncOperation, int)"/>
		/// <seealso cref="Join{T}(IAsyncOperation{T}, TimeSpan)"/>
		public static void Join(this IAsyncOperation op, TimeSpan timeout)
		{
			var result = true;

			if (!op.IsCompleted)
			{
				result = op.AsyncWaitHandle.WaitOne(timeout);
			}

			if (result)
			{
				ThrowIfNonSuccess(op, false);
			}
			else
			{
				throw new TimeoutException();
			}
		}

		/// <summary>
		/// Waits for the <see cref="IAsyncOperation{T}"/> to complete execution. After that rethrows the operation exception (if any).
		/// </summary>
		/// <param name="op">The operation to join.</param>
		/// <returns>The operation result.</returns>
		/// <exception cref="ObjectDisposedException">Thrown is the operation is disposed.</exception>
		/// <seealso cref="Join{T}(IAsyncOperation{T}, int)"/>
		/// <seealso cref="Join{T}(IAsyncOperation{T}, TimeSpan)"/>
		/// <seealso cref="Join(IAsyncOperation)"/>
		public static T Join<T>(this IAsyncOperation<T> op)
		{
			if (!op.IsCompleted)
			{
				op.AsyncWaitHandle.WaitOne();
			}

			ThrowIfNonSuccess(op, false);
			return op.Result;
		}

		/// <summary>
		/// Waits for the <see cref="IAsyncOperation{T}"/> to complete execution within a specified number of milliseconds. After that rethrows the operation exception (if any).
		/// </summary>
		/// <param name="op">The operation to wait for.</param>
		/// <param name="millisecondsTimeout">The number of milliseconds to wait, or <see cref="Timeout.Infinite"/> (-1) to wait indefinitely.</param>
		/// <returns>The operation result.</returns>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="millisecondsTimeout"/> is a negative number other than -1.</exception>
		/// <exception cref="TimeoutException">Thrown if the operation did not completed within <paramref name="millisecondsTimeout"/>.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation is disposed.</exception>
		/// <seealso cref="Join{T}(IAsyncOperation{T})"/>
		/// <seealso cref="Join{T}(IAsyncOperation{T}, TimeSpan)"/>
		/// <seealso cref="Join(IAsyncOperation, int)"/>
		public static T Join<T>(this IAsyncOperation<T> op, int millisecondsTimeout)
		{
			var result = true;

			if (!op.IsCompleted)
			{
				result = op.AsyncWaitHandle.WaitOne(millisecondsTimeout);
			}

			if (result)
			{
				ThrowIfNonSuccess(op, false);
			}
			else
			{
				throw new TimeoutException();
			}

			return op.Result;
		}

		/// <summary>
		/// Waits for the <see cref="IAsyncOperation{T}"/> to complete execution within a specified timeout. After that rethrows the operation exception (if any).
		/// </summary>
		/// <param name="op">The operation to wait for.</param>
		/// <param name="timeout">A <see cref="TimeSpan"/> that represents the number of milliseconds to wait, or a <see cref="TimeSpan"/> that represents -1 milliseconds to wait indefinitely.</param>
		/// <returns>The operation result.</returns>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="timeout"/> is a negative number other than -1 milliseconds, or <paramref name="timeout"/> is greater than <see cref="int.MaxValue"/>.</exception>
		/// <exception cref="TimeoutException">Thrown if the operation did not completed within <paramref name="timeout"/>.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation is disposed.</exception>
		/// <seealso cref="Join{T}(IAsyncOperation{T})"/>
		/// <seealso cref="Join{T}(IAsyncOperation{T}, int)"/>
		/// <seealso cref="Join(IAsyncOperation, TimeSpan)"/>
		public static T Join<T>(this IAsyncOperation<T> op, TimeSpan timeout)
		{
			var result = true;

			if (!op.IsCompleted)
			{
				result = op.AsyncWaitHandle.WaitOne(timeout);
			}

			if (result)
			{
				ThrowIfNonSuccess(op, false);
			}
			else
			{
				throw new TimeoutException();
			}

			return op.Result;
		}

		#endregion

		#region Then

		/// <summary>
		/// Schedules a callback to be executed after the operation has succeeded.
		/// </summary>
		/// <param name="op">The target operation.</param>
		/// <param name="successCallback">The callback to be executed when the operation has completed.</param>
		/// <returns>TODO</returns>
		/// <seealso href="https://promisesaplus.com/"/>
		public static IAsyncOperation Then(this IAsyncOperation op, Action successCallback)
		{
			if (successCallback == null)
			{
				throw new ArgumentNullException(nameof(successCallback));
			}

			var result = new AsyncCompletionSource(AsyncOperationStatus.Running);

			op.AddCompletionCallback(asyncOp =>
			{
				try
				{
					if (asyncOp.IsCompletedSuccessfully)
					{
						successCallback();
						result.TrySetCompleted();
					}
					else
					{
						result.TrySetException(asyncOp.Exception);
					}
				}
				catch (Exception e)
				{
					result.TrySetException(e);
				}
			});

			return result;
		}

		/// <summary>
		/// Schedules a callback to be executed after the operation has succeeded.
		/// </summary>
		/// <param name="op">The target operation.</param>
		/// <param name="successCallback">The callback to be executed when the operation has completed.</param>
		/// <returns>TODO</returns>
		/// <seealso href="https://promisesaplus.com/"/>
		public static IAsyncOperation Then<T>(this IAsyncOperation<T> op, Action<T> successCallback)
		{
			if (successCallback == null)
			{
				throw new ArgumentNullException(nameof(successCallback));
			}

			var result = new AsyncCompletionSource(AsyncOperationStatus.Running);

			op.AddCompletionCallback(asyncOp =>
			{
				try
				{
					if (asyncOp.IsCompletedSuccessfully)
					{
						successCallback((asyncOp as IAsyncOperation<T>).Result);
						result.TrySetCompleted();
					}
					else
					{
						result.TrySetException(asyncOp.Exception);
					}
				}
				catch (Exception e)
				{
					result.TrySetException(e);
				}
			});

			return result;
		}

		/// <summary>
		/// Schedules a callback to be executed after the operation has succeeded.
		/// </summary>
		/// <param name="op">The target operation.</param>
		/// <param name="successCallback">The callback to be executed when the operation has completed.</param>
		/// <seealso href="https://promisesaplus.com/"/>
		public static IAsyncOperation Then(this IAsyncOperation op, Func<IAsyncOperation> successCallback)
		{
			if (successCallback == null)
			{
				throw new ArgumentNullException(nameof(successCallback));
			}

			var result = new AsyncCompletionSource(AsyncOperationStatus.Running);

			op.AddCompletionCallback(asyncOp =>
			{
				try
				{
					if (asyncOp.IsCompletedSuccessfully)
					{
						successCallback().AddCompletionCallback(asyncOp2 => result.CopyCompletionState(asyncOp2, false), null);
					}
					else
					{
						result.TrySetException(asyncOp.Exception);
					}
				}
				catch (Exception e)
				{
					result.TrySetException(e);
				}
			});

			return result;
		}

		/// <summary>
		/// Schedules a callback to be executed after the operation has succeeded.
		/// </summary>
		/// <param name="op">The target operation.</param>
		/// <param name="successCallback">The callback to be executed when the operation has completed.</param>
		/// <seealso href="https://promisesaplus.com/"/>
		public static IAsyncOperation Then<T>(this IAsyncOperation<T> op, Func<T, IAsyncOperation> successCallback)
		{
			if (successCallback == null)
			{
				throw new ArgumentNullException(nameof(successCallback));
			}

			var result = new AsyncCompletionSource(AsyncOperationStatus.Running);

			op.AddCompletionCallback(asyncOp =>
			{
				try
				{
					if (asyncOp.IsCompletedSuccessfully)
					{
						successCallback((asyncOp as IAsyncOperation<T>).Result).AddCompletionCallback(asyncOp2 => result.CopyCompletionState(asyncOp2, false), null);
					}
					else
					{
						result.TrySetException(asyncOp.Exception);
					}
				}
				catch (Exception e)
				{
					result.TrySetException(e);
				}
			});

			return result;
		}

		/// <summary>
		/// Schedules a callbacks to be executed after the operation has completed.
		/// </summary>
		/// <param name="op">The target operation.</param>
		/// <param name="successCallback">The callback to be executed when the operation has succeeded.</param>
		/// <param name="errorCallback">The callback to be executed when the operation has faulted/was canceled.</param>
		/// <seealso href="https://promisesaplus.com/"/>
		public static IAsyncOperation Then(this IAsyncOperation op, Action successCallback, Action<Exception> errorCallback)
		{
			if (successCallback == null)
			{
				throw new ArgumentNullException(nameof(successCallback));
			}

			if (errorCallback == null)
			{
				throw new ArgumentNullException(nameof(errorCallback));
			}

			var result = new AsyncCompletionSource(AsyncOperationStatus.Running);

			op.AddCompletionCallback(asyncOp =>
			{
				try
				{
					if (asyncOp.IsCompletedSuccessfully)
					{
						successCallback();
						result.TrySetCompleted();
					}
					else
					{
						errorCallback(asyncOp.Exception.InnerException);
						result.TrySetException(asyncOp.Exception);
					}
				}
				catch (Exception e)
				{
					result.TrySetException(e);
				}
			});

			return result;
		}

		/// <summary>
		/// Schedules a callbacks to be executed after the operation has completed.
		/// </summary>
		/// <param name="op">The target operation.</param>
		/// <param name="successCallback">The callback to be executed when the operation has succeeded.</param>
		/// <param name="errorCallback">The callback to be executed when the operation has faulted/was canceled.</param>
		/// <seealso href="https://promisesaplus.com/"/>
		public static IAsyncOperation Then<T>(this IAsyncOperation<T> op, Action<T> successCallback, Action<Exception> errorCallback)
		{
			if (successCallback == null)
			{
				throw new ArgumentNullException(nameof(successCallback));
			}

			if (errorCallback == null)
			{
				throw new ArgumentNullException(nameof(errorCallback));
			}

			var result = new AsyncCompletionSource(AsyncOperationStatus.Running);

			op.AddCompletionCallback(asyncOp =>
			{
				try
				{
					if (asyncOp.IsCompletedSuccessfully)
					{
						successCallback((asyncOp as IAsyncOperation<T>).Result);
						result.TrySetCompleted();
					}
					else
					{
						errorCallback(asyncOp.Exception.InnerException);
						result.TrySetException(asyncOp.Exception);
					}
				}
				catch (Exception e)
				{
					result.TrySetException(e);
				}
			});

			return result;
		}

		/// <summary>
		/// Adds a completion callback to be executed after the operation has succeeded.
		/// </summary>
		/// <param name="op">The target operation.</param>
		/// <param name="successCallback">The callback to be executed when the operation has succeeded.</param>
		/// <param name="errorCallback">The callback to be executed when the operation has faulted/was canceled.</param>
		/// <seealso href="https://promisesaplus.com/"/>
		public static IAsyncOperation Then(this IAsyncOperation op, Func<IAsyncOperation> successCallback, Action<Exception> errorCallback)
		{
			if (successCallback == null)
			{
				throw new ArgumentNullException(nameof(successCallback));
			}

			if (errorCallback == null)
			{
				throw new ArgumentNullException(nameof(errorCallback));
			}

			var result = new AsyncCompletionSource(AsyncOperationStatus.Running);

			op.AddCompletionCallback(asyncOp =>
			{
				try
				{
					if (asyncOp.IsCompletedSuccessfully)
					{
						successCallback().AddCompletionCallback(asyncOp2 => result.CopyCompletionState(asyncOp2, false), null);
					}
					else
					{
						errorCallback(asyncOp.Exception.InnerException);
						result.TrySetException(asyncOp.Exception);
					}
				}
				catch (Exception e)
				{
					result.TrySetException(e);
				}
			});

			return result;
		}

		/// <summary>
		/// Adds a completion callback to be executed after the operation has succeeded.
		/// </summary>
		/// <param name="op">The target operation.</param>
		/// <param name="successCallback">The callback to be executed when the operation has succeeded.</param>
		/// <param name="errorCallback">The callback to be executed when the operation has faulted/was canceled.</param>
		/// <seealso href="https://promisesaplus.com/"/>
		public static IAsyncOperation Then<T>(this IAsyncOperation<T> op, Func<T, IAsyncOperation> successCallback, Action<Exception> errorCallback)
		{
			if (successCallback == null)
			{
				throw new ArgumentNullException(nameof(successCallback));
			}

			if (errorCallback == null)
			{
				throw new ArgumentNullException(nameof(errorCallback));
			}

			var result = new AsyncCompletionSource(AsyncOperationStatus.Running);

			op.AddCompletionCallback(asyncOp =>
			{
				try
				{
					if (asyncOp.IsCompletedSuccessfully)
					{
						successCallback((asyncOp as IAsyncOperation<T>).Result).AddCompletionCallback(asyncOp2 => result.CopyCompletionState(asyncOp2, false), null);
					}
					else
					{
						errorCallback(asyncOp.Exception.InnerException);
						result.TrySetException(asyncOp.Exception);
					}
				}
				catch (Exception e)
				{
					result.TrySetException(e);
				}
			});

			return result;
		}

		#endregion

		#region Catch

		/// <summary>
		/// Adds a completion callback to be executed after the operation has faulted or was canceled.
		/// </summary>
		/// <param name="op">The target operation.</param>
		/// <param name="errorCallback">The callback to be executed when the operation has faulted/was canceled.</param>
		/// <seealso href="https://promisesaplus.com/"/>
		public static IAsyncOperation Catch(this IAsyncOperation op, Action<Exception> errorCallback)
		{
			if (errorCallback == null)
			{
				throw new ArgumentNullException(nameof(errorCallback));
			}

			var result = new AsyncCompletionSource(AsyncOperationStatus.Running);

			op.AddCompletionCallback(
				asyncOp =>
				{
					try
					{
						if (asyncOp.IsCompletedSuccessfully)
						{
							result.TrySetCompleted();
						}
						else
						{
							errorCallback(asyncOp.Exception.InnerException);
							result.TrySetCompleted();
						}
					}
					catch (Exception e)
					{
						result.TrySetException(e);
					}
				},
				AsyncContinuationOptions.CaptureSynchronizationContext);

			return result;
		}

		#endregion

		#region Finally

		/// <summary>
		/// Adds a completion callback to be executed after the operation has completed.
		/// </summary>
		/// <param name="op">The target operation.</param>
		/// <param name="action">The callback to be executed when the operation has completed.</param>
		/// <seealso href="https://promisesaplus.com/"/>
		public static IAsyncOperation Finally(this IAsyncOperation op, Action action)
		{
			if (action == null)
			{
				throw new ArgumentNullException(nameof(action));
			}

			var result = new AsyncCompletionSource(AsyncOperationStatus.Running);

			op.AddCompletionCallback(
				asyncOp =>
				{
					try
					{
						action();
						result.TrySetCompleted();
					}
					catch (Exception e)
					{
						result.TrySetException(e);
					}
				},
				AsyncContinuationOptions.CaptureSynchronizationContext);

			return result;
		}

		#endregion

		#region AddCompletionCallback

		/// <summary>
		/// Adds a completion callback to be executed after the operation has completed. If the operation is completed the <paramref name="action"/> is invoked synchronously.
		/// </summary>
		/// <param name="op">The target operation.</param>
		/// <param name="action">The callback to be executed when the operation has completed.</param>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="action"/> is <see langword="null"/>.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation has been disposed.</exception>
		/// <seealso cref="IAsyncOperationEvents"/>
		/// <seealso cref="AddCompletionCallback(IAsyncOperation, AsyncOperationCallback, AsyncContinuationOptions)"/>
		/// <seealso cref="AddCompletionCallback(IAsyncOperation, AsyncOperationCallback, SynchronizationContext)"/>
		public static void AddCompletionCallback(this IAsyncOperation op, AsyncOperationCallback action)
		{
			if (!op.TryAddCompletionCallback(action, SynchronizationContext.Current))
			{
				action(op);
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
		/// <seealso cref="AddCompletionCallback(IAsyncOperation, Action, AsyncContinuationOptions)"/>
		public static void AddCompletionCallback(this IAsyncOperation op, Action action)
		{
			if (!op.TryAddCompletionCallback(action, AsyncContinuationOptions.CaptureSynchronizationContext))
			{
				action();
			}
		}

		/// <summary>
		/// Adds a completion callback to be executed after the operation has completed. If the operation is completed the <paramref name="action"/> is invoked synchronously.
		/// </summary>
		/// <param name="op">The target operation.</param>
		/// <param name="action">The callback to be executed when the operation has completed.</param>
		/// <param name="options">Options for when the callback is executed.</param>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="action"/> is <see langword="null"/>.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation has been disposed.</exception>
		/// <seealso cref="IAsyncOperationEvents"/>
		/// <seealso cref="AddCompletionCallback(IAsyncOperation, AsyncOperationCallback)"/>
		/// <seealso cref="AddCompletionCallback(IAsyncOperation, AsyncOperationCallback, SynchronizationContext)"/>
		public static void AddCompletionCallback(this IAsyncOperation op, AsyncOperationCallback action, AsyncContinuationOptions options)
		{
			if (!op.TryAddCompletionCallback(action, options))
			{
				if (AsyncContinuation.CanInvoke(op, options))
				{
					action(op);
				}
			}
		}

		/// <summary>
		/// Adds a completion callback to be executed after the operation has completed. If the operation is completed the <paramref name="action"/> is invoked synchronously.
		/// </summary>
		/// <param name="op">The target operation.</param>
		/// <param name="action">The callback to be executed when the operation has completed.</param>
		/// <param name="options">Options for when the callback is executed.</param>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="action"/> is <see langword="null"/>.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation has been disposed.</exception>
		/// <seealso cref="IAsyncOperationEvents"/>
		/// <seealso cref="AddCompletionCallback(IAsyncOperation, Action)"/>
		public static void AddCompletionCallback(this IAsyncOperation op, Action action, AsyncContinuationOptions options)
		{
			if (!op.TryAddCompletionCallback(action, options))
			{
				if (AsyncContinuation.CanInvoke(op, options))
				{
					action();
				}
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
		/// <seealso cref="AddCompletionCallback(IAsyncOperation, AsyncOperationCallback, AsyncContinuationOptions)"/>
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

#if UNITYFX_SUPPORT_TAP

		#region GetAwaiter/ConfigureAwait

		/// <summary>
		/// Provides an object that waits for the completion of an asynchronous operation. This type and its members are intended for compiler use only.
		/// </summary>
		/// <seealso cref="IAsyncOperation"/>
		public struct AsyncAwaiter : INotifyCompletion
		{
			private readonly IAsyncOperation _op;
			private readonly AsyncContinuationOptions _options;

			/// <summary>
			/// Initializes a new instance of the <see cref="AsyncAwaiter"/> struct.
			/// </summary>
			public AsyncAwaiter(IAsyncOperation op, AsyncContinuationOptions options)
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
					ThrowIfNonSuccess(_op, false);
				}
			}

			/// <inheritdoc/>
			public void OnCompleted(Action continuation)
			{
				if (!_op.TryAddCompletionCallback(continuation, _options))
				{
					continuation();
				}
			}
		}

		/// <summary>
		/// Provides an object that waits for the completion of an asynchronous operation. This type and its members are intended for compiler use only.
		/// </summary>
		/// <seealso cref="IAsyncOperation{T}"/>
		public struct AsyncAwaiter<T> : INotifyCompletion
		{
			private readonly IAsyncOperation<T> _op;
			private readonly AsyncContinuationOptions _options;

			/// <summary>
			/// Initializes a new instance of the <see cref="AsyncAwaiter{T}"/> struct.
			/// </summary>
			public AsyncAwaiter(IAsyncOperation<T> op, AsyncContinuationOptions options)
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
					ThrowIfNonSuccess(_op, false);
				}

				return _op.Result;
			}

			/// <inheritdoc/>
			public void OnCompleted(Action continuation)
			{
				if (!_op.TryAddCompletionCallback(continuation, _options))
				{
					continuation();
				}
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
				_awaiter = new AsyncAwaiter(op, continueOnCapturedContext ? AsyncContinuationOptions.CaptureSynchronizationContext : AsyncContinuationOptions.None);
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
				_awaiter = new AsyncAwaiter<T>(op, continueOnCapturedContext ? AsyncContinuationOptions.CaptureSynchronizationContext : AsyncContinuationOptions.None);
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
			return new AsyncAwaiter(op, AsyncContinuationOptions.CaptureSynchronizationContext);
		}

		/// <summary>
		/// Returns the operation awaiter. This method is intended for compiler rather than use directly in code.
		/// </summary>
		/// <param name="op">The operation to await.</param>
		/// <seealso cref="GetAwaiter(IAsyncOperation)"/>
		public static AsyncAwaiter<T> GetAwaiter<T>(this IAsyncOperation<T> op)
		{
			return new AsyncAwaiter<T>(op, AsyncContinuationOptions.CaptureSynchronizationContext);
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

#endif

#if !NET35

		#region ToObservable

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

		#endregion

#endif

		#endregion

		#region IAsyncCompletionSource

		/// <summary>
		/// Transitions the underlying <see cref="IAsyncOperation"/> into the <see cref="AsyncOperationStatus.Canceled"/> state.
		/// </summary>
		/// <param name="completionSource">The completion source instance.</param>
		/// <exception cref="InvalidOperationException">Thrown if the transition fails.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation is disposed.</exception>
		/// <seealso cref="SetException(IAsyncCompletionSource, Exception)"/>
		/// <seealso cref="SetExceptions(IAsyncCompletionSource, IEnumerable{Exception})"/>
		/// <seealso cref="SetCompleted(IAsyncCompletionSource)"/>
		public static void SetCanceled(this IAsyncCompletionSource completionSource)
		{
			if (!completionSource.TrySetCanceled())
			{
				throw new InvalidOperationException();
			}
		}

		/// <summary>
		/// Transitions the underlying <see cref="IAsyncOperation{T}"/> into the <see cref="AsyncOperationStatus.Canceled"/> state.
		/// </summary>
		/// <param name="completionSource">The completion source instance.</param>
		/// <exception cref="InvalidOperationException">Thrown if the transition fails.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation is disposed.</exception>
		/// <seealso cref="SetException{T}(IAsyncCompletionSource{T}, Exception)"/>
		/// <seealso cref="SetExceptions{T}(IAsyncCompletionSource{T}, IEnumerable{Exception})"/>
		/// <seealso cref="SetResult{T}(IAsyncCompletionSource{T}, T)"/>
		public static void SetCanceled<T>(this IAsyncCompletionSource<T> completionSource)
		{
			if (!completionSource.TrySetCanceled())
			{
				throw new InvalidOperationException();
			}
		}

		/// <summary>
		/// Transitions the underlying <see cref="IAsyncOperation"/> into the <see cref="AsyncOperationStatus.Faulted"/> state.
		/// </summary>
		/// <param name="completionSource">The completion source instance.</param>
		/// <param name="exception">An exception that caused the operation to end prematurely.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="exception"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException">Thrown if the transition fails.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation is disposed.</exception>
		/// <seealso cref="SetExceptions(IAsyncCompletionSource, IEnumerable{Exception})"/>
		/// <seealso cref="SetCanceled(IAsyncCompletionSource)"/>
		/// <seealso cref="SetCompleted(IAsyncCompletionSource)"/>
		public static void SetException(this IAsyncCompletionSource completionSource, Exception exception)
		{
			if (!completionSource.TrySetException(exception))
			{
				throw new InvalidOperationException();
			}
		}

		/// <summary>
		/// Transitions the underlying <see cref="IAsyncOperation{T}"/> into the <see cref="AsyncOperationStatus.Faulted"/> state.
		/// </summary>
		/// <param name="completionSource">The completion source instance.</param>
		/// <param name="exception">An exception that caused the operation to end prematurely.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="exception"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException">Thrown if the transition fails.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation is disposed.</exception>
		/// <seealso cref="SetException{T}(IAsyncCompletionSource{T}, Exception)"/>
		/// <seealso cref="SetCanceled{T}(IAsyncCompletionSource{T})"/>
		/// <seealso cref="SetResult{T}(IAsyncCompletionSource{T}, T)"/>
		public static void SetException<T>(this IAsyncCompletionSource<T> completionSource, Exception exception)
		{
			if (!completionSource.TrySetException(exception))
			{
				throw new InvalidOperationException();
			}
		}

		/// <summary>
		/// Transitions the underlying <see cref="IAsyncOperation"/> into the <see cref="AsyncOperationStatus.Faulted"/> state.
		/// </summary>
		/// <param name="completionSource">The completion source instance.</param>
		/// <param name="exceptions">Exceptions that caused the operation to end prematurely.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="exceptions"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException">Thrown if the transition fails.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation is disposed.</exception>
		/// <seealso cref="SetException(IAsyncCompletionSource, Exception)"/>
		/// <seealso cref="SetCanceled(IAsyncCompletionSource)"/>
		/// <seealso cref="SetCompleted(IAsyncCompletionSource)"/>
		public static void SetExceptions(this IAsyncCompletionSource completionSource, IEnumerable<Exception> exceptions)
		{
			if (!completionSource.TrySetExceptions(exceptions))
			{
				throw new InvalidOperationException();
			}
		}

		/// <summary>
		/// Transitions the underlying <see cref="IAsyncOperation{T}"/> into the <see cref="AsyncOperationStatus.Faulted"/> state.
		/// </summary>
		/// <param name="completionSource">The completion source instance.</param>
		/// <param name="exceptions">Exceptions that caused the operation to end prematurely.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="exceptions"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException">Thrown if the transition fails.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation is disposed.</exception>
		/// <seealso cref="SetException{T}(IAsyncCompletionSource{T}, Exception)"/>
		/// <seealso cref="SetCanceled{T}(IAsyncCompletionSource{T})"/>
		/// <seealso cref="SetResult{T}(IAsyncCompletionSource{T}, T)"/>
		public static void SetExceptions<T>(this IAsyncCompletionSource<T> completionSource, IEnumerable<Exception> exceptions)
		{
			if (!completionSource.TrySetExceptions(exceptions))
			{
				throw new InvalidOperationException();
			}
		}

		/// <summary>
		/// Transitions the underlying <see cref="IAsyncOperation"/> into the <see cref="AsyncOperationStatus.RanToCompletion"/> state.
		/// </summary>
		/// <param name="completionSource">The completion source instance.</param>
		/// <exception cref="InvalidOperationException">Thrown if the transition fails.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation is disposed.</exception>
		/// <seealso cref="SetCanceled(IAsyncCompletionSource)"/>
		/// <seealso cref="SetException(IAsyncCompletionSource, Exception)"/>
		/// <seealso cref="SetExceptions(IAsyncCompletionSource, IEnumerable{Exception})"/>
		public static void SetCompleted(this IAsyncCompletionSource completionSource)
		{
			if (!completionSource.TrySetCompleted())
			{
				throw new InvalidOperationException();
			}
		}

		/// <summary>
		/// Transitions the underlying <see cref="IAsyncOperation{T}"/> into the <see cref="AsyncOperationStatus.RanToCompletion"/> state.
		/// </summary>
		/// <param name="completionSource">The completion source instance.</param>
		/// <param name="result">The operation result.</param>
		/// <exception cref="InvalidOperationException">Thrown if the transition fails.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation is disposed.</exception>
		/// <seealso cref="SetCanceled{T}(IAsyncCompletionSource{T})"/>
		/// <seealso cref="SetException{T}(IAsyncCompletionSource{T}, Exception)"/>
		/// <seealso cref="SetExceptions{T}(IAsyncCompletionSource{T}, IEnumerable{Exception})"/>
		public static void SetResult<T>(this IAsyncCompletionSource<T> completionSource, T result)
		{
			if (!completionSource.TrySetResult(result))
			{
				throw new InvalidOperationException();
			}
		}

		#endregion

		#region Task

#if UNITYFX_SUPPORT_TAP

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

#endif

		#endregion
	}
}
