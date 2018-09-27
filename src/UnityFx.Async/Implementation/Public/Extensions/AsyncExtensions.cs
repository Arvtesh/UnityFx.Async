// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;

namespace UnityFx.Async
{
	/// <summary>
	/// Extension methods for <see cref="IAsyncOperation"/>.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	public static partial class AsyncExtensions
	{
		#region data

		private static SendOrPostCallback _actionCallback;

#if !NET35

		private static Action<object> _cancelHandler;

#endif

		#endregion

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

		#region SynchronizationContext

		/// <summary>
		/// Dispatches an synchronous message to a synchronization context.
		/// </summary>
		/// <param name="context">The target context.</param>
		/// <param name="action">The delegate to invoke.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="action"/> is <see langword="null"/>.</exception>
		public static void Send(this SynchronizationContext context, Action action)
		{
			if (action == null)
			{
				throw new ArgumentNullException(nameof(action));
			}

			if (_actionCallback == null)
			{
				_actionCallback = ActionCallback;
			}

			context.Post(_actionCallback, action);
		}

		/// <summary>
		/// Dispatches an asynchronous message to a synchronization context.
		/// </summary>
		/// <param name="context">The target context.</param>
		/// <param name="action">The delegate to invoke.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="action"/> is <see langword="null"/>.</exception>
		public static void Post(this SynchronizationContext context, Action action)
		{
			if (action == null)
			{
				throw new ArgumentNullException(nameof(action));
			}

			if (_actionCallback == null)
			{
				_actionCallback = ActionCallback;
			}

			context.Post(_actionCallback, action);
		}

		/// <summary>
		/// Dispatches a message to a synchronization context.
		/// </summary>
		/// <param name="context">The target context.</param>
		/// <param name="action">The delegate to invoke.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="action"/> is <see langword="null"/>.</exception>
		public static void Invoke(this SynchronizationContext context, Action action)
		{
			if (context == SynchronizationContext.Current)
			{
				if (action == null)
				{
					throw new ArgumentNullException(nameof(action));
				}

				action.Invoke();
			}
			else
			{
				context.Post(_actionCallback, action);
			}
		}

		/// <summary>
		/// Dispatches a message to a synchronization context.
		/// </summary>
		/// <param name="context">The target context.</param>
		/// <param name="d">The delegate to invoke.</param>
		/// <param name="state">User-defined state.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="d"/> is <see langword="null"/>.</exception>
		public static void Invoke(this SynchronizationContext context, SendOrPostCallback d, object state)
		{
			if (context == SynchronizationContext.Current)
			{
				if (d == null)
				{
					throw new ArgumentNullException(nameof(d));
				}

				d.Invoke(state);
			}
			else
			{
				context.Post(d, state);
			}
		}

		#endregion

		#region IAsyncOperationEvents

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
		public static void AddCompletionCallback(this IAsyncOperationEvents op, Action<IAsyncOperation> callback)
		{
			op.AddCompletionCallback(callback, SynchronizationContext.Current);
		}

		/// <summary>
		/// Adds a completion callback to be executed after the operation has completed. If the operation is already completed
		/// the <paramref name="callback"/> is invoked on a context specified via <paramref name="continuationContext"/>.
		/// </summary>
		/// <remarks>
		/// The <paramref name="callback"/> is invoked on a <see cref="SynchronizationContext"/> specified.
		/// Throwing an exception from the callback might cause unspecified behaviour.
		/// </remarks>
		/// <param name="op">The operation to schedule continuation for.</param>
		/// <param name="callback">The callback to be executed when the operation has completed.</param>
		/// <param name="continuationContext">Identifier of a <see cref="SynchronizationContext"/> to schedule callback on.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="callback"/> is <see langword="null"/>.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation has been disposed.</exception>
		public static void AddCompletionCallback(this IAsyncOperationEvents op, Action<IAsyncOperation> callback, AsyncContinuationContext continuationContext)
		{
			op.AddCompletionCallback(callback, AsyncResult.GetSynchronizationContext(continuationContext));
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
		public static void AddCompletionCallback(this IAsyncOperationEvents op, IAsyncContinuation callback)
		{
			op.AddCompletionCallback(callback, SynchronizationContext.Current);
		}

		/// <summary>
		/// Adds a completion callback to be executed after the operation has completed. If the operation is already completed
		/// the <paramref name="callback"/> is invoked on a context specified via <paramref name="continuationContext"/>.
		/// </summary>
		/// <remarks>
		/// The <paramref name="callback"/> is invoked on a <see cref="SynchronizationContext"/> specified.
		/// Throwing an exception from the callback might cause unspecified behaviour.
		/// </remarks>
		/// <param name="op">The operation to schedule continuation for.</param>
		/// <param name="callback">The callback to be executed when the operation has completed.</param>
		/// <param name="continuationContext">Identifier of a <see cref="SynchronizationContext"/> to schedule callback on.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="callback"/> is <see langword="null"/>.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation has been disposed.</exception>
		public static void AddCompletionCallback(this IAsyncOperationEvents op, IAsyncContinuation callback, AsyncContinuationContext continuationContext)
		{
			op.AddCompletionCallback(callback, AsyncResult.GetSynchronizationContext(continuationContext));
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
		public static void AddProgressCallback(this IAsyncOperationEvents op, Action<float> callback)
		{
			op.AddProgressCallback(callback, SynchronizationContext.Current);
		}

		/// <summary>
		/// Adds a callback to be executed when the operation progress has changed. If the operation is already completed
		/// the <paramref name="callback"/> is invoked on a context specified via <paramref name="continuationContext"/>.
		/// </summary>
		/// <remarks>
		/// The <paramref name="callback"/> is invoked on a <see cref="SynchronizationContext"/> specified.
		/// Throwing an exception from the callback might cause unspecified behaviour.
		/// </remarks>
		/// <param name="op">The operation to schedule continuation for.</param>
		/// <param name="callback">The callback to be executed when the operation progress has changed.</param>
		/// <param name="continuationContext">Identifier of a <see cref="SynchronizationContext"/> to schedule callback on.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="callback"/> is <see langword="null"/>.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation has been disposed.</exception>
		public static void AddProgressCallback(this IAsyncOperationEvents op, Action<float> callback, AsyncContinuationContext continuationContext)
		{
			op.AddProgressCallback(callback, AsyncResult.GetSynchronizationContext(continuationContext));
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
		public static void AddProgressCallback(this IAsyncOperationEvents op, IProgress<float> callback)
		{
			op.AddProgressCallback(callback, SynchronizationContext.Current);
		}

		/// <summary>
		/// Adds a callback to be executed when the operation progress has changed. If the operation is already completed
		/// the <paramref name="callback"/> is invoked on a context specified via <paramref name="continuationContext"/>.
		/// </summary>
		/// <remarks>
		/// The <paramref name="callback"/> is invoked on a <see cref="SynchronizationContext"/> specified.
		/// Throwing an exception from the callback might cause unspecified behaviour.
		/// </remarks>
		/// <param name="op">The operation to schedule continuation for.</param>
		/// <param name="callback">The callback to be executed when the operation progress has changed.</param>
		/// <param name="continuationContext">Identifier of a <see cref="SynchronizationContext"/> to schedule callback on.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="callback"/> is <see langword="null"/>.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation has been disposed.</exception>
		public static void AddProgressCallback(this IAsyncOperationEvents op, IProgress<float> callback, AsyncContinuationContext continuationContext)
		{
			op.AddProgressCallback(callback, AsyncResult.GetSynchronizationContext(continuationContext));
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
		/// <seealso cref="SetExceptions(IAsyncCompletionSource, IEnumerable{Exception})"/>
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
		/// <seealso cref="SetExceptions{TResult}(IAsyncCompletionSource{TResult}, IEnumerable{Exception})"/>
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
		/// <seealso cref="SetExceptions(IAsyncCompletionSource, IEnumerable{Exception})"/>
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
				throw new InvalidOperationException(Messages.FormatError_OperationStateCannotBeChanged());
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
		/// <seealso cref="SetExceptions(IAsyncCompletionSource, IEnumerable{Exception})"/>
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
		/// <seealso cref="SetExceptions{TResult}(IAsyncCompletionSource{TResult}, IEnumerable{Exception})"/>
		public static void SetResult<TResult>(this IAsyncCompletionSource<TResult> completionSource, TResult result)
		{
			if (!completionSource.TrySetResult(result))
			{
				throw new InvalidOperationException(Messages.FormatError_OperationStateCannotBeChanged());
			}
		}

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

		private static void ActionCallback(object args)
		{
			((Action)args).Invoke();
		}

		#endregion
	}
}
