// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Threading;

namespace UnityFx.Async
{
	partial class AsyncExtensions
	{
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
#if UNITYFX_NOT_THREAD_SAFE

			SpinUntilCompleted(op);

#else

			if (!op.IsCompleted)
			{
				op.AsyncWaitHandle.WaitOne();
			}

#endif

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
#if UNITYFX_NOT_THREAD_SAFE

			var result = SpinUntilCompleted(op, millisecondsTimeout);

#else

			var result = true;

			if (!op.IsCompleted)
			{
				result = op.AsyncWaitHandle.WaitOne(millisecondsTimeout);
			}

#endif

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
#if UNITYFX_NOT_THREAD_SAFE

			var result = SpinUntilCompleted(op, timeout);

#else

			var result = true;

			if (!op.IsCompleted)
			{
				result = op.AsyncWaitHandle.WaitOne(timeout);
			}

#endif

			if (result)
			{
				ThrowIfNonSuccess(op, true);
			}

			return result;
		}

#if !NET35

		/// <summary>
		/// Waits for the <see cref="IAsyncOperation"/> to complete execution. The wait terminates if a cancellation token is canceled before the operation completes.
		/// </summary>
		/// <param name="op">The operation to wait for.</param>
		/// <param name="cancellationToken">A cancellation token to observe while waiting for the operation to complete.</param>
		/// <exception cref="AggregateException">Thrown if the operation was canceled or faulted.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation is disposed.</exception>
		/// <exception cref="OperationCanceledException">The <paramref name="cancellationToken"/> was canceled.</exception>
		/// <seealso cref="Wait(IAsyncOperation)"/>
		public static void Wait(this IAsyncOperation op, CancellationToken cancellationToken)
		{
			WaitInternal(op, cancellationToken);
			ThrowIfNonSuccess(op, true);
		}

		/// <summary>
		/// Waits for the <see cref="IAsyncOperation"/> to complete execution within a specified number of milliseconds. The wait terminates
		/// if a timeout interval elapses or a cancellation token is canceled before the operation completes.
		/// </summary>
		/// <param name="op">The operation to wait for.</param>
		/// <param name="millisecondsTimeout">The number of milliseconds to wait, or <see cref="Timeout.Infinite"/> (-1) to wait indefinitely.</param>
		/// <param name="cancellationToken">A cancellation token to observe while waiting for the operation to complete.</param>
		/// <returns><see langword="true"/> if the operation completed execution within the allotted time; otherwise, <see langword="false"/>.</returns>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="millisecondsTimeout"/> is a negative number other than -1.</exception>
		/// <exception cref="AggregateException">Thrown if the operation was canceled or faulted.</exception>
		/// <exception cref="OperationCanceledException">The <paramref name="cancellationToken"/> was canceled.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation is disposed.</exception>
		/// <seealso cref="Wait(IAsyncOperation, int)"/>
		public static bool Wait(this IAsyncOperation op, int millisecondsTimeout, CancellationToken cancellationToken)
		{
			if (WaitInternal(op, millisecondsTimeout, cancellationToken))
			{
				ThrowIfNonSuccess(op, true);
				return true;
			}

			return false;
		}

		/// <summary>
		/// Waits for the <see cref="IAsyncOperation"/> to complete execution within a specified time interval. The wait terminates
		/// if a timeout interval elapses or a cancellation token is canceled before the operation completes.
		/// </summary>
		/// <param name="op">The operation to wait for.</param>
		/// <param name="timeout">A <see cref="TimeSpan"/> that represents the number of milliseconds to wait, or a <see cref="TimeSpan"/> that represents -1 milliseconds to wait indefinitely.</param>
		/// <param name="cancellationToken">A cancellation token to observe while waiting for the operation to complete.</param>
		/// <returns><see langword="true"/> if the operation completed execution within the allotted time; otherwise, <see langword="false"/>.</returns>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="timeout"/> is a negative number other than -1 milliseconds, or <paramref name="timeout"/> is greater than <see cref="int.MaxValue"/>.</exception>
		/// <exception cref="AggregateException">Thrown if the operation was canceled or faulted.</exception>
		/// <exception cref="OperationCanceledException">The <paramref name="cancellationToken"/> was canceled.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation is disposed.</exception>
		/// <seealso cref="Wait(IAsyncOperation, TimeSpan)"/>
		public static bool Wait(this IAsyncOperation op, TimeSpan timeout, CancellationToken cancellationToken)
		{
			if (WaitInternal(op, timeout, cancellationToken))
			{
				ThrowIfNonSuccess(op, true);
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
#if UNITYFX_NOT_THREAD_SAFE

			SpinUntilCompleted(op);

#else

			if (!op.IsCompleted)
			{
				op.AsyncWaitHandle.WaitOne();
			}

#endif

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
		/// <seealso cref="Join{TResult}(IAsyncOperation{TResult}, int)"/>
		public static void Join(this IAsyncOperation op, int millisecondsTimeout)
		{
#if UNITYFX_NOT_THREAD_SAFE

			var result = SpinUntilCompleted(op, millisecondsTimeout);

#else

			var result = true;

			if (!op.IsCompleted)
			{
				result = op.AsyncWaitHandle.WaitOne(millisecondsTimeout);
			}

#endif

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
		/// <seealso cref="Join{TResult}(IAsyncOperation{TResult}, TimeSpan)"/>
		public static void Join(this IAsyncOperation op, TimeSpan timeout)
		{
#if UNITYFX_NOT_THREAD_SAFE

			var result = SpinUntilCompleted(op, timeout);

#else

			var result = true;

			if (!op.IsCompleted)
			{
				result = op.AsyncWaitHandle.WaitOne(timeout);
			}

#endif

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
#if UNITYFX_NOT_THREAD_SAFE

			SpinUntilCompleted(op);

#else

			if (!op.IsCompleted)
			{
				op.AsyncWaitHandle.WaitOne();
			}

#endif

			ThrowIfNonSuccess(op, false);
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
#if UNITYFX_NOT_THREAD_SAFE

			var result = SpinUntilCompleted(op, millisecondsTimeout);

#else

			var result = true;

			if (!op.IsCompleted)
			{
				result = op.AsyncWaitHandle.WaitOne(millisecondsTimeout);
			}

#endif

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
#if UNITYFX_NOT_THREAD_SAFE

			var result = SpinUntilCompleted(op, timeout);

#else

			var result = true;

			if (!op.IsCompleted)
			{
				result = op.AsyncWaitHandle.WaitOne(timeout);
			}

#endif

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
			ThrowIfNonSuccess(op, false);
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
				ThrowIfNonSuccess(op, false);
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
				ThrowIfNonSuccess(op, false);
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
			ThrowIfNonSuccess(op, false);
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
			if (WaitInternal(op, millisecondsTimeout, cancellationToken))
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
			if (WaitInternal(op, timeout, cancellationToken))
			{
				ThrowIfNonSuccess(op, false);
			}
			else
			{
				throw new TimeoutException();
			}

			return op.Result;
		}

#endif

		#endregion

		#region implementation

#if !NET35

		private static void WaitInternal(IAsyncOperation op, CancellationToken cancellationToken)
		{
#if UNITYFX_NOT_THREAD_SAFE

			SpinUntilCompleted(op, cancellationToken);

#else

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

#endif
		}

		private static bool WaitInternal(IAsyncOperation op, int millisecondsTimeout, CancellationToken cancellationToken)
		{
#if UNITYFX_NOT_THREAD_SAFE

			return SpinUntilCompleted(op, millisecondsTimeout, cancellationToken);

#else

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
#endif
		}

		private static bool WaitInternal(IAsyncOperation op, TimeSpan timeout, CancellationToken cancellationToken)
		{
#if UNITYFX_NOT_THREAD_SAFE

			return SpinUntilCompleted(op, timeout, cancellationToken);

#else

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
#endif
		}

#endif

		#endregion
	}
}
