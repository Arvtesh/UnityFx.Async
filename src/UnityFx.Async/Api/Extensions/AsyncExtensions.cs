// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;

namespace UnityFx.Async
{
	/// <summary>
	/// Extension methods for <see cref="IAsyncOperation"/> related classes.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	public static partial class AsyncExtensions
	{
		#region Common

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
			throw new NotImplementedException();
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
			throw new NotImplementedException();
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
		/// Transitions the underlying <see cref="IAsyncOperation{TResult}"/> into the <see cref="AsyncOperationStatus.Canceled"/> state.
		/// </summary>
		/// <param name="completionSource">The completion source instance.</param>
		/// <exception cref="InvalidOperationException">Thrown if the transition fails.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation is disposed.</exception>
		/// <seealso cref="SetException{TResult}(IAsyncCompletionSource{TResult}, Exception)"/>
		/// <seealso cref="SetExceptions{TResult}(IAsyncCompletionSource{TResult}, IEnumerable{Exception})"/>
		/// <seealso cref="SetResult{TResult}(IAsyncCompletionSource{TResult}, TResult)"/>
		public static void SetCanceled<TResult>(this IAsyncCompletionSource<TResult> completionSource)
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
		/// Transitions the underlying <see cref="IAsyncOperation{TResult}"/> into the <see cref="AsyncOperationStatus.Faulted"/> state.
		/// </summary>
		/// <param name="completionSource">The completion source instance.</param>
		/// <param name="exceptions">Exceptions that caused the operation to end prematurely.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="exceptions"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException">Thrown if the transition fails.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation is disposed.</exception>
		/// <seealso cref="SetException{TResult}(IAsyncCompletionSource{TResult}, Exception)"/>
		/// <seealso cref="SetCanceled{TResult}(IAsyncCompletionSource{TResult})"/>
		/// <seealso cref="SetResult{TResult}(IAsyncCompletionSource{TResult}, TResult)"/>
		public static void SetExceptions<TResult>(this IAsyncCompletionSource<TResult> completionSource, IEnumerable<Exception> exceptions)
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
		/// Transitions the underlying <see cref="IAsyncOperation{TResult}"/> into the <see cref="AsyncOperationStatus.RanToCompletion"/> state.
		/// </summary>
		/// <param name="completionSource">The completion source instance.</param>
		/// <param name="result">The operation result.</param>
		/// <exception cref="InvalidOperationException">Thrown if the transition fails.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation is disposed.</exception>
		/// <seealso cref="SetCanceled{TResult}(IAsyncCompletionSource{TResult})"/>
		/// <seealso cref="SetException{TResult}(IAsyncCompletionSource{TResult}, Exception)"/>
		/// <seealso cref="SetExceptions{TResult}(IAsyncCompletionSource{TResult}, IEnumerable{Exception})"/>
		public static void SetResult<TResult>(this IAsyncCompletionSource<TResult> completionSource, TResult result)
		{
			if (!completionSource.TrySetResult(result))
			{
				throw new InvalidOperationException();
			}
		}

		#endregion
	}
}
