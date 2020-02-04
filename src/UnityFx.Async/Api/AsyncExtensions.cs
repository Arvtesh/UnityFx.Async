// Copyright (c) 2018-2020 Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
#if !NET35
using System.Threading.Tasks;
#endif

namespace UnityFx.Async
{
	/// <summary>
	/// Extension methods for <see cref="IAsyncOperation"/> and related classes.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	public static class AsyncExtensions
	{
		#region data

#if !NET35

		private static Action<object> _cancelHandler;

#endif

		#endregion

		#region IAsyncOperation

		#region Common

		/// <summary>
		/// Throws if the specified operation is faulted/canceled.
		/// </summary>
		public static void ThrowIfNonSuccess(this IAsyncOperation op)
		{
			var status = op.Status;

			if (status == AsyncOperationStatus.Faulted)
			{
				if (!AsyncResult.TryThrowException(op.Exception))
				{
					// Should never get here. Exception should never be null in faulted state.
					throw new Exception();
				}
			}
			else if (status == AsyncOperationStatus.Canceled)
			{
				if (!AsyncResult.TryThrowException(op.Exception))
				{
					throw new OperationCanceledException();
				}
			}
		}

#if !NET35

		/// <summary>
		/// Registers a <see cref="CancellationToken"/> that can be used to cancel the specified operation.
		/// </summary>
		/// <param name="op">An operation to register <paramref name="cancellationToken"/> for.</param>
		/// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
		/// <exception cref="NotSupportedException">Thrown if the target operation does not support cancellation.</exception>
		/// <returns>Returns the target operation.</returns>
		public static IAsyncOperation WithCancellation(this IAsyncOperation op, CancellationToken cancellationToken)
		{
			if (cancellationToken.CanBeCanceled && !op.IsCompleted)
			{
				if (cancellationToken.IsCancellationRequested)
				{
					op.Cancel();
				}
				else
				{
					if (_cancelHandler == null)
					{
						_cancelHandler = args => (args as IAsyncCancellable).Cancel();
					}

					cancellationToken.Register(_cancelHandler, op, false);
				}
			}

			return op;
		}

#endif

		#endregion

		#region Wait

		/// <summary>
		/// Waits for the <see cref="IAsyncOperation"/> to complete execution. After that rethrows the operation exception (if any).
		/// </summary>
		/// <param name="op">The operation to wait for.</param>
		/// <exception cref="ObjectDisposedException">Thrown is the operation is disposed.</exception>
		/// <seealso cref="Wait(IAsyncOperation, int)"/>
		/// <seealso cref="Wait(IAsyncOperation, TimeSpan)"/>
		public static void Wait(this IAsyncOperation op)
		{
			if (!op.IsCompleted)
			{
				op.AsyncWaitHandle.WaitOne();
			}

			ThrowIfNonSuccess(op);
		}

		/// <summary>
		/// Waits for the <see cref="IAsyncOperation"/> to complete execution within a specified number of milliseconds. After that rethrows the operation exception (if any).
		/// </summary>
		/// <param name="op">The operation to wait for.</param>
		/// <param name="millisecondsTimeout">The number of milliseconds to wait, or <see cref="Timeout.Infinite"/> (-1) to wait indefinitely.</param>
		/// <returns><see langword="true"/> if the operation completed execution within the allotted time; otherwise, <see langword="false"/>.</returns>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="millisecondsTimeout"/> is a negative number other than -1.</exception>
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
				ThrowIfNonSuccess(op);
			}

			return result;
		}

		/// <summary>
		/// Waits for the <see cref="IAsyncOperation"/> to complete execution within a specified time interval. After that rethrows the operation exception (if any).
		/// </summary>
		/// <param name="op">The operation to wait for.</param>
		/// <param name="timeout">A <see cref="TimeSpan"/> that represents the number of milliseconds to wait, or a <see cref="TimeSpan"/> that represents -1 milliseconds to wait indefinitely.</param>
		/// <returns><see langword="true"/> if the operation completed execution within the allotted time; otherwise, <see langword="false"/>.</returns>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="timeout"/> is a negative number other than -1 milliseconds, or <paramref name="timeout"/> is greater than <see cref="int.MaxValue"/>.</exception>
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
				ThrowIfNonSuccess(op);
			}

			return result;
		}

#if !NET35

		/// <summary>
		/// Waits for the <see cref="IAsyncOperation"/> to complete execution. After that rethrows the operation exception (if any).
		/// The wait terminates if a cancellation token is canceled before the operation completes.
		/// </summary>
		/// <param name="op">The operation to wait for.</param>
		/// <param name="cancellationToken">A cancellation token to observe while waiting for the operation to complete.</param>
		/// <exception cref="ObjectDisposedException">Thrown is the operation is disposed.</exception>
		/// <exception cref="OperationCanceledException">The <paramref name="cancellationToken"/> was canceled.</exception>
		/// <seealso cref="Wait(IAsyncOperation)"/>
		public static void Wait(this IAsyncOperation op, CancellationToken cancellationToken)
		{
			WaitInternal(op, cancellationToken);
			ThrowIfNonSuccess(op);
		}

		/// <summary>
		/// Waits for the <see cref="IAsyncOperation"/> to complete execution within a specified number of milliseconds. After that
		/// rethrows the operation exception (if any). The wait terminates if a timeout interval elapses or a cancellation token is
		/// canceled before the operation completes.
		/// </summary>
		/// <param name="op">The operation to wait for.</param>
		/// <param name="millisecondsTimeout">The number of milliseconds to wait, or <see cref="Timeout.Infinite"/> (-1) to wait indefinitely.</param>
		/// <param name="cancellationToken">A cancellation token to observe while waiting for the operation to complete.</param>
		/// <returns><see langword="true"/> if the operation completed execution within the allotted time; otherwise, <see langword="false"/>.</returns>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="millisecondsTimeout"/> is a negative number other than -1.</exception>
		/// <exception cref="OperationCanceledException">The <paramref name="cancellationToken"/> was canceled.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation is disposed.</exception>
		/// <seealso cref="Wait(IAsyncOperation, int)"/>
		public static bool Wait(this IAsyncOperation op, int millisecondsTimeout, CancellationToken cancellationToken)
		{
			if (WaitInternal(op, millisecondsTimeout, cancellationToken))
			{
				ThrowIfNonSuccess(op);
				return true;
			}

			return false;
		}

		/// <summary>
		/// Waits for the <see cref="IAsyncOperation"/> to complete execution within a specified time interval. After that rethrows
		/// the operation exception (if any). The wait terminates if a timeout interval elapses or a cancellation token is canceled
		/// before the operation completes.
		/// </summary>
		/// <param name="op">The operation to wait for.</param>
		/// <param name="timeout">A <see cref="TimeSpan"/> that represents the number of milliseconds to wait, or a <see cref="TimeSpan"/> that represents -1 milliseconds to wait indefinitely.</param>
		/// <param name="cancellationToken">A cancellation token to observe while waiting for the operation to complete.</param>
		/// <returns><see langword="true"/> if the operation completed execution within the allotted time; otherwise, <see langword="false"/>.</returns>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="timeout"/> is a negative number other than -1 milliseconds, or <paramref name="timeout"/> is greater than <see cref="int.MaxValue"/>.</exception>
		/// <exception cref="OperationCanceledException">The <paramref name="cancellationToken"/> was canceled.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation is disposed.</exception>
		/// <seealso cref="Wait(IAsyncOperation, TimeSpan)"/>
		public static bool Wait(this IAsyncOperation op, TimeSpan timeout, CancellationToken cancellationToken)
		{
			if (WaitInternal(op, timeout, cancellationToken))
			{
				ThrowIfNonSuccess(op);
				return true;
			}

			return false;
		}

#endif

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

			ThrowIfNonSuccess(op);
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
		/// <seealso cref="Join{TResult}(IAsyncOperation{TResult}, int)"/>
		public static void Join(this IAsyncOperation op, int millisecondsTimeout)
		{
			var result = true;

			if (!op.IsCompleted)
			{
				result = op.AsyncWaitHandle.WaitOne(millisecondsTimeout);
			}

			if (result)
			{
				ThrowIfNonSuccess(op);
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
		/// <seealso cref="Join{TResult}(IAsyncOperation{TResult}, TimeSpan)"/>
		public static void Join(this IAsyncOperation op, TimeSpan timeout)
		{
			var result = true;

			if (!op.IsCompleted)
			{
				result = op.AsyncWaitHandle.WaitOne(timeout);
			}

			if (result)
			{
				ThrowIfNonSuccess(op);
			}
			else
			{
				throw new TimeoutException();
			}
		}

		/// <summary>
		/// Waits for the <see cref="IAsyncOperation{TResult}"/> to complete execution. After that rethrows the operation exception (if any).
		/// </summary>
		/// <param name="op">The operation to join.</param>
		/// <returns>The operation result.</returns>
		/// <exception cref="ObjectDisposedException">Thrown is the operation is disposed.</exception>
		/// <seealso cref="Join{TResult}(IAsyncOperation{TResult}, int)"/>
		/// <seealso cref="Join{TResult}(IAsyncOperation{TResult}, TimeSpan)"/>
		/// <seealso cref="Join(IAsyncOperation)"/>
		public static TResult Join<TResult>(this IAsyncOperation<TResult> op)
		{
			if (!op.IsCompleted)
			{
				op.AsyncWaitHandle.WaitOne();
			}

			return op.Result;
		}

		/// <summary>
		/// Waits for the <see cref="IAsyncOperation{TResult}"/> to complete execution within a specified number of milliseconds. After that rethrows the operation exception (if any).
		/// </summary>
		/// <param name="op">The operation to wait for.</param>
		/// <param name="millisecondsTimeout">The number of milliseconds to wait, or <see cref="Timeout.Infinite"/> (-1) to wait indefinitely.</param>
		/// <returns>The operation result.</returns>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="millisecondsTimeout"/> is a negative number other than -1.</exception>
		/// <exception cref="TimeoutException">Thrown if the operation did not completed within <paramref name="millisecondsTimeout"/>.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation is disposed.</exception>
		/// <seealso cref="Join{TResult}(IAsyncOperation{TResult})"/>
		/// <seealso cref="Join{TResult}(IAsyncOperation{TResult}, TimeSpan)"/>
		/// <seealso cref="Join(IAsyncOperation, int)"/>
		public static TResult Join<TResult>(this IAsyncOperation<TResult> op, int millisecondsTimeout)
		{
			var result = true;

			if (!op.IsCompleted)
			{
				result = op.AsyncWaitHandle.WaitOne(millisecondsTimeout);
			}

			if (!result)
			{
				throw new TimeoutException();
			}

			return op.Result;
		}

		/// <summary>
		/// Waits for the <see cref="IAsyncOperation{TResult}"/> to complete execution within a specified timeout. After that rethrows the operation exception (if any).
		/// </summary>
		/// <param name="op">The operation to wait for.</param>
		/// <param name="timeout">A <see cref="TimeSpan"/> that represents the number of milliseconds to wait, or a <see cref="TimeSpan"/> that represents -1 milliseconds to wait indefinitely.</param>
		/// <returns>The operation result.</returns>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="timeout"/> is a negative number other than -1 milliseconds, or <paramref name="timeout"/> is greater than <see cref="int.MaxValue"/>.</exception>
		/// <exception cref="TimeoutException">Thrown if the operation did not completed within <paramref name="timeout"/>.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation is disposed.</exception>
		/// <seealso cref="Join{TResult}(IAsyncOperation{TResult})"/>
		/// <seealso cref="Join{TResult}(IAsyncOperation{TResult}, int)"/>
		/// <seealso cref="Join(IAsyncOperation, TimeSpan)"/>
		public static TResult Join<TResult>(this IAsyncOperation<TResult> op, TimeSpan timeout)
		{
			var result = true;

			if (!op.IsCompleted)
			{
				result = op.AsyncWaitHandle.WaitOne(timeout);
			}

			if (!result)
			{
				throw new TimeoutException();
			}

			return op.Result;
		}

#if !NET35

		/// <summary>
		/// Waits for the <see cref="IAsyncOperation"/> to complete execution. After that rethrows the operation exception (if any). The wait terminates
		/// if a cancellation token is canceled before the operation completes.
		/// </summary>
		/// <param name="op">The operation to join.</param>
		/// <param name="cancellationToken">A cancellation token to observe while waiting for the operation to complete.</param>
		/// <exception cref="OperationCanceledException">The <paramref name="cancellationToken"/> was canceled.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation is disposed.</exception>
		/// <seealso cref="Join(IAsyncOperation)"/>
		public static void Join(this IAsyncOperation op, CancellationToken cancellationToken)
		{
			WaitInternal(op, cancellationToken);
			ThrowIfNonSuccess(op);
		}

		/// <summary>
		/// Waits for the <see cref="IAsyncOperation"/> to complete execution within a specified number of milliseconds. After that rethrows the operation exception (if any).
		/// The wait terminates if a timeout interval elapses or a cancellation token is canceled before the operation completes.
		/// </summary>
		/// <param name="op">The operation to wait for.</param>
		/// <param name="millisecondsTimeout">The number of milliseconds to wait, or <see cref="Timeout.Infinite"/> (-1) to wait indefinitely.</param>
		/// <param name="cancellationToken">A cancellation token to observe while waiting for the operation to complete.</param>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="millisecondsTimeout"/> is a negative number other than -1.</exception>
		/// <exception cref="TimeoutException">Thrown if the operation did not completed within <paramref name="millisecondsTimeout"/>.</exception>
		/// <exception cref="OperationCanceledException">The <paramref name="cancellationToken"/> was canceled.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation is disposed.</exception>
		/// <seealso cref="Join(IAsyncOperation, int)"/>
		public static void Join(this IAsyncOperation op, int millisecondsTimeout, CancellationToken cancellationToken)
		{
			if (WaitInternal(op, millisecondsTimeout, cancellationToken))
			{
				ThrowIfNonSuccess(op);
			}
			else
			{
				throw new TimeoutException();
			}
		}

		/// <summary>
		/// Waits for the <see cref="IAsyncOperation"/> to complete execution within a specified timeout. After that rethrows the operation exception (if any).
		/// The wait terminates if a timeout interval elapses or a cancellation token is canceled before the operation completes.
		/// </summary>
		/// <param name="op">The operation to wait for.</param>
		/// <param name="timeout">A <see cref="TimeSpan"/> that represents the number of milliseconds to wait, or a <see cref="TimeSpan"/> that represents -1 milliseconds to wait indefinitely.</param>
		/// <param name="cancellationToken">A cancellation token to observe while waiting for the operation to complete.</param>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="timeout"/> is a negative number other than -1 milliseconds, or <paramref name="timeout"/> is greater than <see cref="int.MaxValue"/>.</exception>
		/// <exception cref="TimeoutException">Thrown if the operation did not completed within <paramref name="timeout"/>.</exception>
		/// <exception cref="OperationCanceledException">The <paramref name="cancellationToken"/> was canceled.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation is disposed.</exception>
		/// <seealso cref="Join(IAsyncOperation, TimeSpan)"/>
		public static void Join(this IAsyncOperation op, TimeSpan timeout, CancellationToken cancellationToken)
		{
			if (WaitInternal(op, timeout, cancellationToken))
			{
				ThrowIfNonSuccess(op);
			}
			else
			{
				throw new TimeoutException();
			}
		}

		/// <summary>
		/// Waits for the <see cref="IAsyncOperation{TResult}"/> to complete execution. After that rethrows the operation exception (if any).
		/// The wait terminates if a cancellation token is canceled before the operation completes.
		/// </summary>
		/// <param name="op">The operation to join.</param>
		/// <param name="cancellationToken">A cancellation token to observe while waiting for the operation to complete.</param>
		/// <returns>The operation result.</returns>
		/// <exception cref="OperationCanceledException">The <paramref name="cancellationToken"/> was canceled.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation is disposed.</exception>
		/// <seealso cref="Join{TResult}(IAsyncOperation{TResult})"/>
		public static TResult Join<TResult>(this IAsyncOperation<TResult> op, CancellationToken cancellationToken)
		{
			WaitInternal(op, cancellationToken);
			return op.Result;
		}

		/// <summary>
		/// Waits for the <see cref="IAsyncOperation{TResult}"/> to complete execution within a specified number of milliseconds. After that rethrows the operation exception (if any).
		/// The wait terminates if a timeout interval elapses or a cancellation token is canceled before the operation completes.
		/// </summary>
		/// <param name="op">The operation to wait for.</param>
		/// <param name="millisecondsTimeout">The number of milliseconds to wait, or <see cref="Timeout.Infinite"/> (-1) to wait indefinitely.</param>
		/// <param name="cancellationToken">A cancellation token to observe while waiting for the operation to complete.</param>
		/// <returns>The operation result.</returns>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="millisecondsTimeout"/> is a negative number other than -1.</exception>
		/// <exception cref="TimeoutException">Thrown if the operation did not completed within <paramref name="millisecondsTimeout"/>.</exception>
		/// <exception cref="OperationCanceledException">The <paramref name="cancellationToken"/> was canceled.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation is disposed.</exception>
		/// <seealso cref="Join{TResult}(IAsyncOperation{TResult}, int)"/>
		public static TResult Join<TResult>(this IAsyncOperation<TResult> op, int millisecondsTimeout, CancellationToken cancellationToken)
		{
			if (!WaitInternal(op, millisecondsTimeout, cancellationToken))
			{
				throw new TimeoutException();
			}

			return op.Result;
		}

		/// <summary>
		/// Waits for the <see cref="IAsyncOperation{TResult}"/> to complete execution within a specified timeout. After that rethrows the operation exception (if any).
		/// The wait terminates if a timeout interval elapses or a cancellation token is canceled before the operation completes.
		/// </summary>
		/// <param name="op">The operation to wait for.</param>
		/// <param name="timeout">A <see cref="TimeSpan"/> that represents the number of milliseconds to wait, or a <see cref="TimeSpan"/> that represents -1 milliseconds to wait indefinitely.</param>
		/// <param name="cancellationToken">A cancellation token to observe while waiting for the operation to complete.</param>
		/// <returns>The operation result.</returns>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="timeout"/> is a negative number other than -1 milliseconds, or <paramref name="timeout"/> is greater than <see cref="int.MaxValue"/>.</exception>
		/// <exception cref="TimeoutException">Thrown if the operation did not completed within <paramref name="timeout"/>.</exception>
		/// <exception cref="OperationCanceledException">The <paramref name="cancellationToken"/> was canceled.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation is disposed.</exception>
		/// <seealso cref="Join{TResult}(IAsyncOperation{TResult}, TimeSpan)"/>
		public static TResult Join<TResult>(this IAsyncOperation<TResult> op, TimeSpan timeout, CancellationToken cancellationToken)
		{
			if (!WaitInternal(op, timeout, cancellationToken))
			{
				throw new TimeoutException();
			}

			return op.Result;
		}

#endif

		#endregion

		#region ContinueWith

		/// <summary>
		/// Creates a continuation that executes when the target <see cref="IAsyncOperation"/> completes.
		/// </summary>
		/// <param name="op">The operation to continue.</param>
		/// <param name="action">An action to run when the <paramref name="op"/> completes.</param>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="action"/> is <see langword="null"/>.</exception>
		/// <returns>An operation that is executed after <paramref name="op"/> completes.</returns>
		/// <seealso cref="ContinueWith(IAsyncOperation, Action{IAsyncOperation}, AsyncContinuationOptions)"/>
		public static IAsyncOperation ContinueWith(this IAsyncOperation op, Action<IAsyncOperation> action)
		{
			return ContinueWith(op, action, AsyncContinuationOptions.None);
		}

		/// <summary>
		/// Creates a continuation that executes when the target <see cref="IAsyncOperation"/> completes.
		/// </summary>
		/// <param name="op">The operation to continue.</param>
		/// <param name="action">An action to run when the <paramref name="op"/> completes.</param>
		/// <param name="options">Options for when the <paramref name="action"/> is executed.</param>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="action"/> is <see langword="null"/>.</exception>
		/// <returns>An operation that is executed after <paramref name="op"/> completes.</returns>
		/// <seealso cref="ContinueWith(IAsyncOperation, Action{IAsyncOperation})"/>
		public static IAsyncOperation ContinueWith(this IAsyncOperation op, Action<IAsyncOperation> action, AsyncContinuationOptions options)
		{
			if (action == null)
			{
				throw new ArgumentNullException(nameof(action));
			}

			return new ContinueWithResult<VoidResult, VoidResult>(op, options, action, null);
		}

		/// <summary>
		/// Creates a continuation that executes when the target <see cref="IAsyncOperation"/> completes.
		/// </summary>
		/// <param name="op">The operation to continue.</param>
		/// <param name="action">An action to run when the <paramref name="op"/> completes.</param>
		/// <param name="userState">A user-defined state object that is passed as second argument to <paramref name="action"/>.</param>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="action"/> is <see langword="null"/>.</exception>
		/// <returns>An operation that is executed after <paramref name="op"/> completes.</returns>
		/// <seealso cref="ContinueWith(IAsyncOperation, Action{IAsyncOperation, object}, object, AsyncContinuationOptions)"/>
		public static IAsyncOperation ContinueWith(this IAsyncOperation op, Action<IAsyncOperation, object> action, object userState)
		{
			return ContinueWith(op, action, userState, AsyncContinuationOptions.None);
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
		/// <seealso cref="ContinueWith(IAsyncOperation, Action{IAsyncOperation, object}, object)"/>
		public static IAsyncOperation ContinueWith(this IAsyncOperation op, Action<IAsyncOperation, object> action, object userState, AsyncContinuationOptions options)
		{
			if (action == null)
			{
				throw new ArgumentNullException(nameof(action));
			}

			return new ContinueWithResult<VoidResult, VoidResult>(op, options, action, userState);
		}

		/// <summary>
		/// Creates a continuation that executes when the target <see cref="IAsyncOperation"/> completes.
		/// </summary>
		/// <param name="op">The operation to continue.</param>
		/// <param name="action">An action to run when the <paramref name="op"/> completes.</param>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="action"/> is <see langword="null"/>.</exception>
		/// <returns>An operation that is executed after <paramref name="op"/> completes.</returns>
		/// <seealso cref="ContinueWith{TResult}(IAsyncOperation, Func{IAsyncOperation, TResult}, AsyncContinuationOptions)"/>
		public static IAsyncOperation<TResult> ContinueWith<TResult>(this IAsyncOperation op, Func<IAsyncOperation, TResult> action)
		{
			return ContinueWith(op, action, AsyncContinuationOptions.None);
		}

		/// <summary>
		/// Creates a continuation that executes when the target <see cref="IAsyncOperation"/> completes.
		/// </summary>
		/// <param name="op">The operation to continue.</param>
		/// <param name="action">An action to run when the <paramref name="op"/> completes.</param>
		/// <param name="options">Options for when the <paramref name="action"/> is executed.</param>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="action"/> is <see langword="null"/>.</exception>
		/// <returns>An operation that is executed after <paramref name="op"/> completes.</returns>
		/// <seealso cref="ContinueWith{TResult}(IAsyncOperation, Func{IAsyncOperation, TResult})"/>
		public static IAsyncOperation<TResult> ContinueWith<TResult>(this IAsyncOperation op, Func<IAsyncOperation, TResult> action, AsyncContinuationOptions options)
		{
			if (action == null)
			{
				throw new ArgumentNullException(nameof(action));
			}

			return new ContinueWithResult<VoidResult, TResult>(op, options, action, null);
		}

		/// <summary>
		/// Creates a continuation that executes when the target <see cref="IAsyncOperation"/> completes.
		/// </summary>
		/// <param name="op">The operation to continue.</param>
		/// <param name="action">An action to run when the <paramref name="op"/> completes.</param>
		/// <param name="userState">A user-defined state object that is passed as second argument to <paramref name="action"/>.</param>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="action"/> is <see langword="null"/>.</exception>
		/// <returns>An operation that is executed after <paramref name="op"/> completes.</returns>
		/// <seealso cref="ContinueWith{TResult}(IAsyncOperation, Func{IAsyncOperation, object, TResult}, object, AsyncContinuationOptions)"/>
		public static IAsyncOperation<TResult> ContinueWith<TResult>(this IAsyncOperation op, Func<IAsyncOperation, object, TResult> action, object userState)
		{
			return ContinueWith(op, action, userState, AsyncContinuationOptions.None);
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
		/// <seealso cref="ContinueWith{TResult}(IAsyncOperation, Func{IAsyncOperation, object, TResult}, object)"/>
		public static IAsyncOperation<TResult> ContinueWith<TResult>(this IAsyncOperation op, Func<IAsyncOperation, object, TResult> action, object userState, AsyncContinuationOptions options)
		{
			if (action == null)
			{
				throw new ArgumentNullException(nameof(action));
			}

			return new ContinueWithResult<VoidResult, TResult>(op, options, action, userState);
		}

		/// <summary>
		/// Creates a continuation that executes when the target <see cref="IAsyncOperation{TResult}"/> completes.
		/// </summary>
		/// <param name="op">The operation to continue.</param>
		/// <param name="action">An action to run when the <paramref name="op"/> completes.</param>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="action"/> is <see langword="null"/>.</exception>
		/// <returns>An operation that is executed after <paramref name="op"/> completes.</returns>
		/// <seealso cref="ContinueWith{TResult}(IAsyncOperation{TResult}, Action{IAsyncOperation{TResult}}, AsyncContinuationOptions)"/>
		public static IAsyncOperation ContinueWith<TResult>(this IAsyncOperation<TResult> op, Action<IAsyncOperation<TResult>> action)
		{
			return ContinueWith(op, action, AsyncContinuationOptions.None);
		}

		/// <summary>
		/// Creates a continuation that executes when the target <see cref="IAsyncOperation{TResult}"/> completes.
		/// </summary>
		/// <param name="op">The operation to continue.</param>
		/// <param name="action">An action to run when the <paramref name="op"/> completes.</param>
		/// <param name="options">Options for when the <paramref name="action"/> is executed.</param>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="action"/> is <see langword="null"/>.</exception>
		/// <returns>An operation that is executed after <paramref name="op"/> completes.</returns>
		/// <seealso cref="ContinueWith{TResult}(IAsyncOperation{TResult}, Action{IAsyncOperation{TResult}})"/>
		public static IAsyncOperation ContinueWith<TResult>(this IAsyncOperation<TResult> op, Action<IAsyncOperation<TResult>> action, AsyncContinuationOptions options)
		{
			if (action == null)
			{
				throw new ArgumentNullException(nameof(action));
			}

			return new ContinueWithResult<TResult, VoidResult>(op, options, action, null);
		}

		/// <summary>
		/// Creates a continuation that executes when the target <see cref="IAsyncOperation{TResult}"/> completes.
		/// </summary>
		/// <param name="op">The operation to continue.</param>
		/// <param name="action">An action to run when the <paramref name="op"/> completes.</param>
		/// <param name="userState">A user-defined state object that is passed as second argument to <paramref name="action"/>.</param>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="action"/> is <see langword="null"/>.</exception>
		/// <returns>An operation that is executed after <paramref name="op"/> completes.</returns>
		/// <seealso cref="ContinueWith{TResult}(IAsyncOperation{TResult}, Action{IAsyncOperation{TResult}, object}, object, AsyncContinuationOptions)"/>
		public static IAsyncOperation ContinueWith<TResult>(this IAsyncOperation<TResult> op, Action<IAsyncOperation<TResult>, object> action, object userState)
		{
			return ContinueWith(op, action, userState, AsyncContinuationOptions.None);
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
		/// <seealso cref="ContinueWith{TResult}(IAsyncOperation{TResult}, Action{IAsyncOperation{TResult}, object}, object)"/>
		public static IAsyncOperation ContinueWith<TResult>(this IAsyncOperation<TResult> op, Action<IAsyncOperation<TResult>, object> action, object userState, AsyncContinuationOptions options)
		{
			if (action == null)
			{
				throw new ArgumentNullException(nameof(action));
			}

			return new ContinueWithResult<TResult, VoidResult>(op, options, action, userState);
		}

		/// <summary>
		/// Creates a continuation that executes when the target <see cref="IAsyncOperation"/> completes.
		/// </summary>
		/// <param name="op">The operation to continue.</param>
		/// <param name="action">An action to run when the <paramref name="op"/> completes.</param>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="action"/> is <see langword="null"/>.</exception>
		/// <returns>An operation that is executed after <paramref name="op"/> completes.</returns>
		/// <seealso cref="ContinueWith{TResult, TNewResult}(IAsyncOperation{TResult}, Func{IAsyncOperation{TResult}, TNewResult}, AsyncContinuationOptions)"/>
		public static IAsyncOperation<TNewResult> ContinueWith<TResult, TNewResult>(this IAsyncOperation<TResult> op, Func<IAsyncOperation<TResult>, TNewResult> action)
		{
			return ContinueWith(op, action, AsyncContinuationOptions.None);
		}

		/// <summary>
		/// Creates a continuation that executes when the target <see cref="IAsyncOperation"/> completes.
		/// </summary>
		/// <param name="op">The operation to continue.</param>
		/// <param name="action">An action to run when the <paramref name="op"/> completes.</param>
		/// <param name="options">Options for when the <paramref name="action"/> is executed.</param>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="action"/> is <see langword="null"/>.</exception>
		/// <returns>An operation that is executed after <paramref name="op"/> completes.</returns>
		/// <seealso cref="ContinueWith{TResult, TNewResult}(IAsyncOperation{TResult}, Func{IAsyncOperation{TResult}, TNewResult})"/>
		public static IAsyncOperation<TNewResult> ContinueWith<TResult, TNewResult>(this IAsyncOperation<TResult> op, Func<IAsyncOperation<TResult>, TNewResult> action, AsyncContinuationOptions options)
		{
			if (action == null)
			{
				throw new ArgumentNullException(nameof(action));
			}

			return new ContinueWithResult<TResult, TNewResult>(op, options, action, null);
		}

		/// <summary>
		/// Creates a continuation that executes when the target <see cref="IAsyncOperation"/> completes.
		/// </summary>
		/// <param name="op">The operation to continue.</param>
		/// <param name="action">An action to run when the <paramref name="op"/> completes.</param>
		/// <param name="userState">A user-defined state object that is passed as second argument to <paramref name="action"/>.</param>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="action"/> is <see langword="null"/>.</exception>
		/// <returns>An operation that is executed after <paramref name="op"/> completes.</returns>
		/// <seealso cref="ContinueWith{TResult, TNewResult}(IAsyncOperation{TResult}, Func{IAsyncOperation{TResult}, object, TNewResult}, object, AsyncContinuationOptions)"/>
		public static IAsyncOperation<TNewResult> ContinueWith<TResult, TNewResult>(this IAsyncOperation<TResult> op, Func<IAsyncOperation<TResult>, object, TNewResult> action, object userState)
		{
			return ContinueWith(op, action, userState, AsyncContinuationOptions.None);
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
		/// <seealso cref="ContinueWith{TResult, TNewResult}(IAsyncOperation{TResult}, Func{IAsyncOperation{TResult}, object, TNewResult}, object)"/>
		public static IAsyncOperation<TNewResult> ContinueWith<TResult, TNewResult>(this IAsyncOperation<TResult> op, Func<IAsyncOperation<TResult>, object, TNewResult> action, object userState, AsyncContinuationOptions options)
		{
			if (action == null)
			{
				throw new ArgumentNullException(nameof(action));
			}

			return new ContinueWithResult<TResult, TNewResult>(op, options, action, userState);
		}

		#endregion

		#region Unwrap

		/// <summary>
		/// Creates a proxy <see cref="IAsyncOperation"/> that represents the asynchronous operation of a <c>IAsyncOperation&lt;IAsyncOperation&gt;</c>.
		/// </summary>
		/// <param name="op">The source operation.</param>
		/// <returns>The unwrapped operation.</returns>
		/// <seealso cref="Unwrap{TResult}(IAsyncOperation{IAsyncOperation{TResult}})"/>
		public static IAsyncOperation Unwrap(this IAsyncOperation<IAsyncOperation> op)
		{
			return new UnwrapResult<VoidResult>(op);
		}

		/// <summary>
		/// Creates a proxy <see cref="IAsyncOperation{TResult}"/> that represents the asynchronous operation of a <c>IAsyncOperation&lt;IAsyncOperation&lt;TResult&gt;&gt;</c>.
		/// </summary>
		/// <param name="op">The source operation.</param>
		/// <returns>The unwrapped operation.</returns>
		/// <seealso cref="Unwrap(IAsyncOperation{IAsyncOperation})"/>
		public static IAsyncOperation<TResult> Unwrap<TResult>(this IAsyncOperation<IAsyncOperation<TResult>> op)
		{
			return new UnwrapResult<TResult>(op);
		}

		#endregion

		#region ToTask

#if !NET35

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
					new Action(() =>
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
					}),
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
					new Action(() =>
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
					}),
					null);

				return tcs.Task;
			}
		}

#endif

		#endregion

		#region GetAwaiter/ConfigureAwait

#if !NET35

		/// <summary>
		/// Returns the operation awaiter. This method is intended for compiler use only.
		/// </summary>
		/// <param name="op">The operation to await.</param>
		/// <returns>An object that can be used to await the operation.</returns>
		public static CompilerServices.AsyncAwaiter GetAwaiter(this IAsyncOperation op)
		{
			return new CompilerServices.AsyncAwaiter(op);
		}

		/// <summary>
		/// Returns the operation awaiter. This method is intended for compiler use only.
		/// </summary>
		/// <param name="op">The operation to await.</param>
		/// <returns>An object that can be used to await the operation.</returns>
		public static CompilerServices.AsyncAwaiter<TResult> GetAwaiter<TResult>(this IAsyncOperation<TResult> op)
		{
			return new CompilerServices.AsyncAwaiter<TResult>(op);
		}

		/// <summary>
		/// Configures an awaiter used to await this operation.
		/// </summary>
		/// <param name="op">The operation to await.</param>
		/// <param name="continueOnCapturedContext">If <see langword="true"/> attempts to marshal the continuation back to the original context captured.</param>
		/// <returns>An object that can be used to await the operation.</returns>
		public static CompilerServices.AsyncAwaitable ConfigureAwait(this IAsyncOperation op, bool continueOnCapturedContext)
		{
			return new CompilerServices.AsyncAwaitable(op, continueOnCapturedContext ? SynchronizationContext.Current : null);
		}

		/// <summary>
		/// Configures an awaiter used to await this operation.
		/// </summary>
		/// <param name="op">The operation to await.</param>
		/// <param name="continueOnCapturedContext">If <see langword="true"/> attempts to marshal the continuation back to the original context captured.</param>
		/// <returns>An object that can be used to await the operation.</returns>
		public static CompilerServices.AsyncAwaitable<TResult> ConfigureAwait<TResult>(this IAsyncOperation<TResult> op, bool continueOnCapturedContext)
		{
			return new CompilerServices.AsyncAwaitable<TResult>(op, continueOnCapturedContext ? SynchronizationContext.Current : null);
		}

		/// <summary>
		/// Configures an awaiter used to await this operation.
		/// </summary>
		/// <param name="op">The operation to await.</param>
		/// <param name="options">Specifies continuation options.</param>
		/// <returns>An object that can be used to await the operation.</returns>
		public static CompilerServices.AsyncAwaitable ConfigureAwait(this IAsyncOperation op, AsyncCallbackOptions options)
		{
			return new CompilerServices.AsyncAwaitable(op, AsyncResult.GetSynchronizationContext(options));
		}

		/// <summary>
		/// Configures an awaiter used to await this operation.
		/// </summary>
		/// <param name="op">The operation to await.</param>
		/// <param name="options">Specifies continuation options.</param>
		/// <returns>An object that can be used to await the operation.</returns>
		public static CompilerServices.AsyncAwaitable<TResult> ConfigureAwait<TResult>(this IAsyncOperation<TResult> op, AsyncCallbackOptions options)
		{
			return new CompilerServices.AsyncAwaitable<TResult>(op, AsyncResult.GetSynchronizationContext(options));
		}

#endif

		#endregion

		#endregion

		#region IAsyncOperationCallbacks

		/// <summary>
		/// Adds a completion callback to be executed after the operation has completed. If the operation is already completed
		/// the <paramref name="callback"/> is called synchronously.
		/// </summary>
		/// <remarks>
		/// The <paramref name="callback"/> is invoked on a thread that registered the continuation (if it has a <see cref="SynchronizationContext"/> attached).
		/// Throwing an exception from the callback might cause unspecified behaviour.
		/// </remarks>
		/// <param name="op">The operation to schedule continuation for.</param>
		/// <param name="callback">The callback to be executed when the operation has completed.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="callback"/> is <see langword="null"/>.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation has been disposed.</exception>
		public static void AddCompletionCallback(this IAsyncOperationCallbacks op, Action callback)
		{
			op.AddCompletionCallback(callback, SynchronizationContext.Current);
		}

		/// <summary>
		/// Adds a completion callback to be executed after the operation has completed. If the operation is already completed
		/// the <paramref name="callback"/> is invoked on a context specified via <paramref name="options"/>.
		/// </summary>
		/// <remarks>
		/// The <paramref name="callback"/> is invoked on a <see cref="SynchronizationContext"/> specified.
		/// Throwing an exception from the callback might cause unspecified behaviour.
		/// </remarks>
		/// <param name="op">The operation to schedule continuation for.</param>
		/// <param name="callback">The callback to be executed when the operation has completed.</param>
		/// <param name="options">Identifier of a <see cref="SynchronizationContext"/> to schedule callback on.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="callback"/> is <see langword="null"/>.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation has been disposed.</exception>
		public static void AddCompletionCallback(this IAsyncOperationCallbacks op, Action callback, AsyncCallbackOptions options)
		{
			op.AddCompletionCallback(callback, AsyncResult.GetSynchronizationContext(options));
		}

		/// <summary>
		/// Adds a completion callback to be executed after the operation has completed. If the operation is already completed
		/// the <paramref name="callback"/> is called synchronously.
		/// </summary>
		/// <remarks>
		/// The <paramref name="callback"/> is invoked on a thread that registered the continuation (if it has a <see cref="SynchronizationContext"/> attached).
		/// Throwing an exception from the callback might cause unspecified behaviour.
		/// </remarks>
		/// <param name="op">The operation to schedule continuation for.</param>
		/// <param name="callback">The callback to be executed when the operation has completed.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="callback"/> is <see langword="null"/>.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation has been disposed.</exception>
		public static void AddCompletionCallback(this IAsyncOperationCallbacks op, Action<IAsyncOperation> callback)
		{
			op.AddCompletionCallback(callback, SynchronizationContext.Current);
		}

		/// <summary>
		/// Adds a completion callback to be executed after the operation has completed. If the operation is already completed
		/// the <paramref name="callback"/> is invoked on a context specified via <paramref name="options"/>.
		/// </summary>
		/// <remarks>
		/// The <paramref name="callback"/> is invoked on a <see cref="SynchronizationContext"/> specified.
		/// Throwing an exception from the callback might cause unspecified behaviour.
		/// </remarks>
		/// <param name="op">The operation to schedule continuation for.</param>
		/// <param name="callback">The callback to be executed when the operation has completed.</param>
		/// <param name="options">Identifier of a <see cref="SynchronizationContext"/> to schedule callback on.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="callback"/> is <see langword="null"/>.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation has been disposed.</exception>
		public static void AddCompletionCallback(this IAsyncOperationCallbacks op, Action<IAsyncOperation> callback, AsyncCallbackOptions options)
		{
			op.AddCompletionCallback(callback, AsyncResult.GetSynchronizationContext(options));
		}

		/// <summary>
		/// Adds a completion callback to be executed after the operation has completed. If the operation is already completed
		/// the <paramref name="callback"/> is called synchronously.
		/// </summary>
		/// <remarks>
		/// The <paramref name="callback"/> is invoked on a thread that registered the continuation (if it has a <see cref="SynchronizationContext"/> attached).
		/// Throwing an exception from the callback might cause unspecified behaviour.
		/// </remarks>
		/// <param name="op">The operation to schedule continuation for.</param>
		/// <param name="callback">The callback to be executed when the operation has completed.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="callback"/> is <see langword="null"/>.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation has been disposed.</exception>
		public static void AddCompletionCallback(this IAsyncOperationCallbacks op, IAsyncContinuation callback)
		{
			op.AddCompletionCallback(callback, SynchronizationContext.Current);
		}

		/// <summary>
		/// Adds a completion callback to be executed after the operation has completed. If the operation is already completed
		/// the <paramref name="callback"/> is invoked on a context specified via <paramref name="options"/>.
		/// </summary>
		/// <remarks>
		/// The <paramref name="callback"/> is invoked on a <see cref="SynchronizationContext"/> specified.
		/// Throwing an exception from the callback might cause unspecified behaviour.
		/// </remarks>
		/// <param name="op">The operation to schedule continuation for.</param>
		/// <param name="callback">The callback to be executed when the operation has completed.</param>
		/// <param name="options">Identifier of a <see cref="SynchronizationContext"/> to schedule callback on.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="callback"/> is <see langword="null"/>.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation has been disposed.</exception>
		public static void AddCompletionCallback(this IAsyncOperationCallbacks op, IAsyncContinuation callback, AsyncCallbackOptions options)
		{
			op.AddCompletionCallback(callback, AsyncResult.GetSynchronizationContext(options));
		}

		/// <summary>
		/// Adds a callback to be executed when the operation progress has changed. If the operation is already completed
		/// the <paramref name="callback"/> is called synchronously.
		/// </summary>
		/// <remarks>
		/// The <paramref name="callback"/> is invoked on a thread that registered the callback (if it has a <see cref="SynchronizationContext"/> attached).
		/// Throwing an exception from the callback might cause unspecified behaviour.
		/// </remarks>
		/// <param name="op">The operation to schedule continuation for.</param>
		/// <param name="callback">The callback to be executed when the operation progress has changed.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="callback"/> is <see langword="null"/>.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation has been disposed.</exception>
		public static void AddProgressCallback(this IAsyncOperationCallbacks op, Action<float> callback)
		{
			op.AddProgressCallback(callback, SynchronizationContext.Current);
		}

		/// <summary>
		/// Adds a callback to be executed when the operation progress has changed. If the operation is already completed
		/// the <paramref name="callback"/> is invoked on a context specified via <paramref name="options"/>.
		/// </summary>
		/// <remarks>
		/// The <paramref name="callback"/> is invoked on a <see cref="SynchronizationContext"/> specified.
		/// Throwing an exception from the callback might cause unspecified behaviour.
		/// </remarks>
		/// <param name="op">The operation to schedule continuation for.</param>
		/// <param name="callback">The callback to be executed when the operation progress has changed.</param>
		/// <param name="options">Identifier of a <see cref="SynchronizationContext"/> to schedule callback on.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="callback"/> is <see langword="null"/>.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation has been disposed.</exception>
		public static void AddProgressCallback(this IAsyncOperationCallbacks op, Action<float> callback, AsyncCallbackOptions options)
		{
			op.AddProgressCallback(callback, AsyncResult.GetSynchronizationContext(options));
		}

#if !NET35

		/// <summary>
		/// Adds a callback to be executed when the operation progress has changed. If the operation is already completed
		/// the <paramref name="callback"/> is called synchronously.
		/// </summary>
		/// <remarks>
		/// The <paramref name="callback"/> is invoked on a thread that registered the callback (if it has a <see cref="SynchronizationContext"/> attached).
		/// Throwing an exception from the callback might cause unspecified behaviour.
		/// </remarks>
		/// <param name="op">The operation to schedule continuation for.</param>
		/// <param name="callback">The callback to be executed when the operation progress has changed.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="callback"/> is <see langword="null"/>.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation has been disposed.</exception>
		public static void AddProgressCallback(this IAsyncOperationCallbacks op, IProgress<float> callback)
		{
			op.AddProgressCallback(callback, SynchronizationContext.Current);
		}

		/// <summary>
		/// Adds a callback to be executed when the operation progress has changed. If the operation is already completed
		/// the <paramref name="callback"/> is invoked on a context specified via <paramref name="options"/>.
		/// </summary>
		/// <remarks>
		/// The <paramref name="callback"/> is invoked on a <see cref="SynchronizationContext"/> specified.
		/// Throwing an exception from the callback might cause unspecified behaviour.
		/// </remarks>
		/// <param name="op">The operation to schedule continuation for.</param>
		/// <param name="callback">The callback to be executed when the operation progress has changed.</param>
		/// <param name="options">Identifier of a <see cref="SynchronizationContext"/> to schedule callback on.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="callback"/> is <see langword="null"/>.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation has been disposed.</exception>
		public static void AddProgressCallback(this IAsyncOperationCallbacks op, IProgress<float> callback, AsyncCallbackOptions options)
		{
			op.AddProgressCallback(callback, AsyncResult.GetSynchronizationContext(options));
		}

#endif

		#endregion

		#region IAsyncCompletionSource

		/// <summary>
		/// Sets the operation progress value in range [0, 1].
		/// </summary>
		/// <param name="completionSource">The completion source instance.</param>
		/// <param name="progress">The operation progress in range [0, 1].</param>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="progress"/> is not in range [0, 1].</exception>
		/// <exception cref="InvalidOperationException">Thrown if the progress value cannot be set.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation is disposed.</exception>
		/// <seealso cref="SetCompleted(IAsyncCompletionSource)"/>
		public static void SetProgress(this IAsyncCompletionSource completionSource, float progress)
		{
			if (!completionSource.TrySetProgress(progress))
			{
				throw new InvalidOperationException(Messages.FormatError_OperationStateCannotBeChanged());
			}
		}

		/// <summary>
		/// Sets the operation progress value in range [0, 1].
		/// </summary>
		/// <param name="completionSource">The completion source instance.</param>
		/// <param name="progress">The operation progress in range [0, 1].</param>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="progress"/> is not in range [0, 1].</exception>
		/// <exception cref="InvalidOperationException">Thrown if the progress value cannot be set.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation is disposed.</exception>
		/// <seealso cref="SetResult{TResult}(IAsyncCompletionSource{TResult}, TResult)"/>
		public static void SetProgress<TResult>(this IAsyncCompletionSource<TResult> completionSource, float progress)
		{
			if (!completionSource.TrySetProgress(progress))
			{
				throw new InvalidOperationException(Messages.FormatError_OperationStateCannotBeChanged());
			}
		}

		/// <summary>
		/// Transitions the underlying <see cref="IAsyncOperation"/> into the <see cref="AsyncOperationStatus.Canceled"/> state.
		/// </summary>
		/// <param name="completionSource">The completion source instance.</param>
		/// <exception cref="InvalidOperationException">Thrown if the transition fails.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation is disposed.</exception>
		/// <seealso cref="SetException(IAsyncCompletionSource, Exception)"/>
		/// <seealso cref="SetCompleted(IAsyncCompletionSource)"/>
		public static void SetCanceled(this IAsyncCompletionSource completionSource)
		{
			if (!completionSource.TrySetCanceled())
			{
				throw new InvalidOperationException(Messages.FormatError_OperationStateCannotBeChanged());
			}
		}

		/// <summary>
		/// Transitions the underlying <see cref="IAsyncOperation{TResult}"/> into the <see cref="AsyncOperationStatus.Canceled"/> state.
		/// </summary>
		/// <param name="completionSource">The completion source instance.</param>
		/// <exception cref="InvalidOperationException">Thrown if the transition fails.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation is disposed.</exception>
		/// <seealso cref="SetException{TResult}(IAsyncCompletionSource{TResult}, Exception)"/>
		/// <seealso cref="SetResult{TResult}(IAsyncCompletionSource{TResult}, TResult)"/>
		public static void SetCanceled<TResult>(this IAsyncCompletionSource<TResult> completionSource)
		{
			if (!completionSource.TrySetCanceled())
			{
				throw new InvalidOperationException(Messages.FormatError_OperationStateCannotBeChanged());
			}
		}

		/// <summary>
		/// Attempts to transition the underlying <see cref="IAsyncOperation"/> into the <see cref="AsyncOperationStatus.Faulted"/> state.
		/// </summary>
		/// <param name="completionSource">The completion source instance.</param>
		/// <param name="message">An exception message.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="message"/> is <see langword="null"/>.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation is disposed.</exception>
		/// <returns>Returns <see langword="true"/> if the attemp was successfull; <see langword="false"/> otherwise.</returns>
		/// <seealso cref="SetException(IAsyncCompletionSource, Exception)"/>
		/// <seealso cref="SetCanceled(IAsyncCompletionSource)"/>
		/// <seealso cref="SetCompleted(IAsyncCompletionSource)"/>
		public static bool TrySetException(this IAsyncCompletionSource completionSource, string message)
		{
			return completionSource.TrySetException(new Exception(message));
		}

		/// <summary>
		/// Attempts to transition the underlying <see cref="IAsyncOperation{TResult}"/> into the <see cref="AsyncOperationStatus.Faulted"/> state.
		/// </summary>
		/// <param name="completionSource">The completion source instance.</param>
		/// <param name="message">An exception message.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="message"/> is <see langword="null"/>.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation is disposed.</exception>
		/// <returns>Returns <see langword="true"/> if the attemp was successfull; <see langword="false"/> otherwise.</returns>
		/// <seealso cref="SetException{TResult}(IAsyncCompletionSource{TResult}, Exception)"/>
		/// <seealso cref="SetCanceled{TResult}(IAsyncCompletionSource{TResult})"/>
		/// <seealso cref="SetResult{TResult}(IAsyncCompletionSource{TResult}, TResult)"/>
		public static bool TrySetException<TResult>(this IAsyncCompletionSource<TResult> completionSource, string message)
		{
			return completionSource.TrySetException(new Exception(message));
		}

		/// <summary>
		/// Transitions the underlying <see cref="IAsyncOperation"/> into the <see cref="AsyncOperationStatus.Faulted"/> state.
		/// </summary>
		/// <param name="completionSource">The completion source instance.</param>
		/// <param name="message">An exception message.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="message"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException">Thrown if the transition fails.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation is disposed.</exception>
		/// <seealso cref="SetException(IAsyncCompletionSource, Exception)"/>
		/// <seealso cref="SetCanceled(IAsyncCompletionSource)"/>
		/// <seealso cref="SetCompleted(IAsyncCompletionSource)"/>
		public static void SetException(this IAsyncCompletionSource completionSource, string message)
		{
			if (!completionSource.TrySetException(new Exception(message)))
			{
				throw new InvalidOperationException(Messages.FormatError_OperationStateCannotBeChanged());
			}
		}

		/// <summary>
		/// Transitions the underlying <see cref="IAsyncOperation{TResult}"/> into the <see cref="AsyncOperationStatus.Faulted"/> state.
		/// </summary>
		/// <param name="completionSource">The completion source instance.</param>
		/// <param name="message">An exception message.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="message"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException">Thrown if the transition fails.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation is disposed.</exception>
		/// <seealso cref="SetException{TResult}(IAsyncCompletionSource{TResult}, Exception)"/>
		/// <seealso cref="SetCanceled{TResult}(IAsyncCompletionSource{TResult})"/>
		/// <seealso cref="SetResult{TResult}(IAsyncCompletionSource{TResult}, TResult)"/>
		public static void SetException<TResult>(this IAsyncCompletionSource<TResult> completionSource, string message)
		{
			if (!completionSource.TrySetException(new Exception(message)))
			{
				throw new InvalidOperationException(Messages.FormatError_OperationStateCannotBeChanged());
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
		/// <seealso cref="SetCanceled(IAsyncCompletionSource)"/>
		/// <seealso cref="SetCompleted(IAsyncCompletionSource)"/>
		public static void SetException(this IAsyncCompletionSource completionSource, Exception exception)
		{
			if (!completionSource.TrySetException(exception))
			{
				throw new InvalidOperationException(Messages.FormatError_OperationStateCannotBeChanged());
			}
		}

		/// <summary>
		/// Transitions the underlying <see cref="IAsyncOperation{TResult}"/> into the <see cref="AsyncOperationStatus.Faulted"/> state.
		/// </summary>
		/// <param name="completionSource">The completion source instance.</param>
		/// <param name="exception">An exception that caused the operation to end prematurely.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="exception"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException">Thrown if the transition fails.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation is disposed.</exception>
		/// <seealso cref="SetException{TResult}(IAsyncCompletionSource{TResult}, Exception)"/>
		/// <seealso cref="SetCanceled{TResult}(IAsyncCompletionSource{TResult})"/>
		/// <seealso cref="SetResult{TResult}(IAsyncCompletionSource{TResult}, TResult)"/>
		public static void SetException<TResult>(this IAsyncCompletionSource<TResult> completionSource, Exception exception)
		{
			if (!completionSource.TrySetException(exception))
			{
				throw new InvalidOperationException(Messages.FormatError_OperationStateCannotBeChanged());
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
		public static void SetCompleted(this IAsyncCompletionSource completionSource)
		{
			if (!completionSource.TrySetCompleted())
			{
				throw new InvalidOperationException(Messages.FormatError_OperationStateCannotBeChanged());
			}
		}

		/// <summary>
		/// Transitions the underlying <see cref="IAsyncOperation{TResult}"/> into the <see cref="AsyncOperationStatus.RanToCompletion"/> state.
		/// </summary>
		/// <param name="completionSource">The completion source instance.</param>
		/// <param name="result">The operation result.</param>
		/// <exception cref="InvalidOperationException">Thrown if the transition fails.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation is disposed.</exception>
		/// <seealso cref="SetCanceled{TResult}(IAsyncCompletionSource{TResult})"/>
		/// <seealso cref="SetException{TResult}(IAsyncCompletionSource{TResult}, Exception)"/>
		public static void SetResult<TResult>(this IAsyncCompletionSource<TResult> completionSource, TResult result)
		{
			if (!completionSource.TrySetResult(result))
			{
				throw new InvalidOperationException(Messages.FormatError_OperationStateCannotBeChanged());
			}
		}

		#endregion

		#region implementation

#if !NET35

		private static void WaitInternal(IAsyncOperation op, CancellationToken cancellationToken)
		{
			if (!op.IsCompleted)
			{
				if (cancellationToken.CanBeCanceled)
				{
					cancellationToken.ThrowIfCancellationRequested();

					var index = WaitHandle.WaitAny(new WaitHandle[] { op.AsyncWaitHandle, cancellationToken.WaitHandle });

					if (index == 1)
					{
						throw new OperationCanceledException();
					}
				}
				else
				{
					op.AsyncWaitHandle.WaitOne();
				}
			}
		}

		private static bool WaitInternal(IAsyncOperation op, int millisecondsTimeout, CancellationToken cancellationToken)
		{
			var result = true;

			if (!op.IsCompleted)
			{
				if (cancellationToken.CanBeCanceled)
				{
					cancellationToken.ThrowIfCancellationRequested();

					var index = WaitHandle.WaitAny(new WaitHandle[] { op.AsyncWaitHandle, cancellationToken.WaitHandle }, millisecondsTimeout);

					if (index == WaitHandle.WaitTimeout)
					{
						result = false;
					}
					else if (index == 1)
					{
						throw new OperationCanceledException();
					}
				}
				else
				{
					result = op.AsyncWaitHandle.WaitOne(millisecondsTimeout);
				}
			}

			return result;
		}

		private static bool WaitInternal(IAsyncOperation op, TimeSpan timeout, CancellationToken cancellationToken)
		{
			var result = true;

			if (!op.IsCompleted)
			{
				if (cancellationToken.CanBeCanceled)
				{
					cancellationToken.ThrowIfCancellationRequested();

					var index = WaitHandle.WaitAny(new WaitHandle[] { op.AsyncWaitHandle, cancellationToken.WaitHandle }, timeout);

					if (index == WaitHandle.WaitTimeout)
					{
						result = false;
					}
					else if (index == 1)
					{
						throw new OperationCanceledException();
					}
				}
				else
				{
					result = op.AsyncWaitHandle.WaitOne(timeout);
				}
			}

			return result;
		}

#endif

		#endregion
	}
}
