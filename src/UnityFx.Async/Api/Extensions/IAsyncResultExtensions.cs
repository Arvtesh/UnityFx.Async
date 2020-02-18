// Copyright (c) 2018-2020 Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections;
using System.ComponentModel;
using System.Threading;

namespace UnityFx.Async.Extensions
{
	/// <summary>
	/// Extension methods for <see cref="IAsyncResult"/>.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	public static class IAsyncResultExtensions
	{
		#region interface

		/// <summary>
		/// Creates an <see cref="IEnumerator"/> that completes when the specified operation completes.
		/// </summary>
		/// <param name="op">The operation to convert to enumerator.</param>
		/// <returns>An enumerator that represents the operation.</returns>
		public static IEnumerator ToEnum(this IAsyncResult op)
		{
			if (op is IEnumerator e)
			{
				return e;
			}

			return new TaskEnumerator(op);
		}

		/// <summary>
		/// Spins until the operation has completed.
		/// </summary>
		/// <param name="op">The operation to wait for.</param>
		public static void SpinUntilCompleted(this IAsyncResult op)
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
		/// Spins until the operation has completed within a specified timeout.
		/// </summary>
		/// <param name="op">The operation to wait for.</param>
		/// <param name="millisecondsTimeout">The number of milliseconds to wait, or <see cref="Timeout.Infinite"/> (-1) to wait indefinitely.</param>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="millisecondsTimeout"/> is a negative number other than -1.</exception>
		/// <returns>Returns <see langword="true"/> if the operation was completed within the specified time interfval; <see langword="false"/> otherwise.</returns>
		public static bool SpinUntilCompleted(this IAsyncResult op, int millisecondsTimeout)
		{
			if (millisecondsTimeout < -1)
			{
				throw new ArgumentOutOfRangeException(nameof(millisecondsTimeout), millisecondsTimeout, Messages.FormatError_InvalidTimeout());
			}

			if (millisecondsTimeout == Timeout.Infinite)
			{
				SpinUntilCompleted(op);
				return true;
			}

			return SpinUntilCompletedInternal(op, TimeSpan.FromMilliseconds(millisecondsTimeout));
		}

		/// <summary>
		/// Spins until the operation has completed within a specified timeout.
		/// </summary>
		/// <param name="op">The operation to wait for.</param>
		/// <param name="timeout">A <see cref="TimeSpan"/> that represents the number of milliseconds to wait, or a <see cref="TimeSpan"/> that represents -1 milliseconds to wait indefinitely.</param>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="timeout"/> is a negative number other than -1 milliseconds, or <paramref name="timeout"/> is greater than <see cref="int.MaxValue"/>.</exception>
		/// <returns>Returns <see langword="true"/> if the operation was completed within the specified time interfval; <see langword="false"/> otherwise.</returns>
		public static bool SpinUntilCompleted(this IAsyncResult op, TimeSpan timeout)
		{
			var totalMilliseconds = (long)timeout.TotalMilliseconds;

			if (totalMilliseconds < -1 || totalMilliseconds > int.MaxValue)
			{
				throw new ArgumentOutOfRangeException(nameof(timeout), timeout, Messages.FormatError_InvalidTimeout());
			}

			if (totalMilliseconds == Timeout.Infinite)
			{
				SpinUntilCompleted(op);
				return true;
			}

			return SpinUntilCompletedInternal(op, timeout);
		}

#if !NET35

		/// <summary>
		/// Spins until the operation has completed or until canceled.
		/// </summary>
		/// <param name="op">The operation to wait for.</param>
		/// <param name="cancellationToken">A cancellation token that can be used to cancel wait operation.</param>
		/// <exception cref="OperationCanceledException">The <paramref name="cancellationToken"/> was canceled.</exception>
		/// <seealso cref="SpinUntilCompleted(IAsyncResult)"/>
		public static void SpinUntilCompleted(this IAsyncResult op, CancellationToken cancellationToken)
		{
			var sw = new SpinWait();

			while (!op.IsCompleted)
			{
				cancellationToken.ThrowIfCancellationRequested();
				sw.SpinOnce();
			}
		}

		/// <summary>
		/// Spins until the operation has completed within a specified timeout or until canceled.
		/// </summary>
		/// <param name="op">The operation to wait for.</param>
		/// <param name="millisecondsTimeout">The number of milliseconds to wait, or <see cref="Timeout.Infinite"/> (-1) to wait indefinitely.</param>
		/// <param name="cancellationToken">A cancellation token that can be used to cancel wait operation.</param>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="millisecondsTimeout"/> is a negative number other than -1.</exception>
		/// <exception cref="OperationCanceledException">The <paramref name="cancellationToken"/> was canceled.</exception>
		/// <returns>Returns <see langword="true"/> if the operation was completed within the specified time interfval; <see langword="false"/> otherwise.</returns>
		public static bool SpinUntilCompleted(this IAsyncResult op, int millisecondsTimeout, CancellationToken cancellationToken)
		{
			if (millisecondsTimeout < -1)
			{
				throw new ArgumentOutOfRangeException(nameof(millisecondsTimeout), millisecondsTimeout, Messages.FormatError_InvalidTimeout());
			}

			if (millisecondsTimeout == Timeout.Infinite)
			{
				SpinUntilCompleted(op, cancellationToken);
				return true;
			}

			return SpinUntilCompletedInternal(op, TimeSpan.FromMilliseconds(millisecondsTimeout), cancellationToken);
		}

		/// <summary>
		/// Spins until the operation has completed within a specified timeout or until canceled.
		/// </summary>
		/// <param name="op">The operation to wait for.</param>
		/// <param name="timeout">A <see cref="TimeSpan"/> that represents the number of milliseconds to wait, or a <see cref="TimeSpan"/> that represents -1 milliseconds to wait indefinitely.</param>
		/// <param name="cancellationToken">A cancellation token that can be used to cancel wait operation.</param>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="timeout"/> is a negative number other than -1 milliseconds, or <paramref name="timeout"/> is greater than <see cref="int.MaxValue"/>.</exception>
		/// <exception cref="OperationCanceledException">The <paramref name="cancellationToken"/> was canceled.</exception>
		/// <returns>Returns <see langword="true"/> if the operation was completed within the specified time interfval; <see langword="false"/> otherwise.</returns>
		public static bool SpinUntilCompleted(this IAsyncResult op, TimeSpan timeout, CancellationToken cancellationToken)
		{
			var totalMilliseconds = (long)timeout.TotalMilliseconds;

			if (totalMilliseconds < -1 || totalMilliseconds > int.MaxValue)
			{
				throw new ArgumentOutOfRangeException(nameof(timeout), timeout, Messages.FormatError_InvalidTimeout());
			}

			if (totalMilliseconds == Timeout.Infinite)
			{
				SpinUntilCompleted(op, cancellationToken);
				return true;
			}

			return SpinUntilCompletedInternal(op, timeout, cancellationToken);
		}

#endif

		#endregion

		#region implementation

		private class TaskEnumerator : IEnumerator
		{
			private readonly IAsyncResult _op;

			public TaskEnumerator(IAsyncResult task) => _op = task;
			public object Current => null;
			public bool MoveNext() => !_op.IsCompleted;
			public void Reset() => throw new NotSupportedException();
		}

#if !NET35

		private static bool SpinUntilCompletedInternal(IAsyncResult op, TimeSpan timeout, CancellationToken cancellationToken)
		{
			var endTime = DateTime.Now + timeout;
			var sw = new SpinWait();

			while (!op.IsCompleted)
			{
				if (DateTime.Now > endTime)
				{
					return false;
				}

				cancellationToken.ThrowIfCancellationRequested();
				sw.SpinOnce();
			}

			return true;
		}

#endif

		private static bool SpinUntilCompletedInternal(IAsyncResult op, TimeSpan timeout)
		{
			var endTime = DateTime.Now + timeout;

#if NET35

			while (!op.IsCompleted)
			{
				if (DateTime.Now > endTime)
				{
					return false;
				}

				Thread.SpinWait(1);
			}

#else

			var sw = new SpinWait();

			while (!op.IsCompleted)
			{
				if (DateTime.Now > endTime)
				{
					return false;
				}

				sw.SpinOnce();
			}

#endif

			return true;
		}

		#endregion
	}
}
