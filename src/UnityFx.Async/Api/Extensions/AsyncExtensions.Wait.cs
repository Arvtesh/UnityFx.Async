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
	}
}
