// Copyright (c) 2018-2020 Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
#if !NET35
using System.Threading.Tasks;
#endif

namespace UnityFx.Async
{
	partial class AsyncResult
	{
		#region data

		private static AsyncResult _completedOperation;
		private static AsyncResult _faultedOperation;
		private static AsyncResult _canceledOperation;

		#endregion

		#region interface

		/// <summary>
		/// Gets an operation that's already been completed successfully.
		/// </summary>
		/// <remarks>
		/// Note that <see cref="Dispose()"/> call have no effect on operations returned with the property. May not always return the same instance.
		/// </remarks>
		/// <value>Completed <see cref="IAsyncOperation"/> instance.</value>
		public static AsyncResult CompletedOperation
		{
			get
			{
				if (_completedOperation == null)
				{
					_completedOperation = new AsyncResult(_flagDoNotDispose | _flagCompletedSynchronously | StatusRanToCompletion);
				}

				return _completedOperation;
			}
		}

		/// <summary>
		/// Gets a faulted operation.
		/// </summary>
		/// <remarks>
		/// Note that <see cref="Dispose()"/> call have no effect on operations returned with the property. May not always return the same instance.
		/// </remarks>
		/// <value>Faulted <see cref="IAsyncOperation"/> instance.</value>
		public static AsyncResult FaultedOperation
		{
			get
			{
				if (_faultedOperation == null)
				{
					_faultedOperation = new AsyncResult(_flagDoNotDispose | _flagCompletedSynchronously | StatusFaulted);
				}

				return _faultedOperation;
			}
		}

		/// <summary>
		/// Gets an operation that's already been canceled.
		/// </summary>
		/// <remarks>
		/// Note that <see cref="Dispose()"/> call have no effect on operations returned with the property. May not always return the same instance.
		/// </remarks>
		/// <value>Canceled <see cref="IAsyncOperation"/> instance.</value>
		public static AsyncResult CanceledOperation
		{
			get
			{
				if (_canceledOperation == null)
				{
					_canceledOperation = new AsyncResult(_flagDoNotDispose | _flagCompletedSynchronously | StatusCanceled);
				}

				return _canceledOperation;
			}
		}

#if !NET35

		/// <summary>
		/// Creates an awaitable that asynchronously yields back to the current context when awaited (works the same as <see cref="Task.Yield"/>).
		/// </summary>
		/// <returns>
		/// A context that, when awaited, will asynchronously transition back into the current context at the
		/// time of the await. If the current <see cref="SynchronizationContext"/> is non-<see langword="null"/>,
		/// that is treated as the current context. Otherwise, the continuation is executed on the <see cref="ThreadPool"/>.
		/// </returns>
		public static YieldAwaitable Yield()
		{
			return new YieldAwaitable();
		}

#endif

		#region From*

		/// <summary>
		/// Creates a <see cref="IAsyncOperation"/> that is canceled.
		/// </summary>
		/// <returns>A canceled operation.</returns>
		/// <seealso cref="FromCanceled(object)"/>
		/// <seealso cref="FromException(Exception)"/>
		/// <seealso cref="FromResult{T}(T)"/>
		public static AsyncResult FromCanceled()
		{
			return new AsyncResult(AsyncOperationStatus.Canceled);
		}

		/// <summary>
		/// Creates a <see cref="IAsyncOperation"/> that is canceled.
		/// </summary>
		/// <param name="asyncState">User-defined data returned by <see cref="AsyncState"/>.</param>
		/// <returns>A canceled operation.</returns>
		/// <seealso cref="FromCanceled()"/>
		/// <seealso cref="FromException(Exception)"/>
		/// <seealso cref="FromResult{T}(T, object)"/>
		public static AsyncResult FromCanceled(object asyncState)
		{
			return new AsyncResult(AsyncOperationStatus.Canceled, asyncState);
		}

		/// <summary>
		/// Creates a <see cref="IAsyncOperation{TResult}"/> that is canceled.
		/// </summary>
		/// <returns>A canceled operation.</returns>
		/// <seealso cref="FromCanceled{T}(object)"/>
		/// <seealso cref="FromException{T}(Exception)"/>
		/// <seealso cref="FromResult{T}(T)"/>
		public static AsyncResult<T> FromCanceled<T>()
		{
			return new AsyncResult<T>(AsyncOperationStatus.Canceled);
		}

		/// <summary>
		/// Creates a <see cref="IAsyncOperation{TResult}"/> that is canceled.
		/// </summary>
		/// <param name="asyncState">User-defined data returned by <see cref="AsyncState"/>.</param>
		/// <returns>A canceled operation.</returns>
		/// <seealso cref="FromCanceled{T}()"/>
		/// <seealso cref="FromException{T}(Exception)"/>
		/// <seealso cref="FromResult{T}(T, object)"/>
		public static AsyncResult<T> FromCanceled<T>(object asyncState)
		{
			return new AsyncResult<T>(AsyncOperationStatus.Canceled, asyncState);
		}

		/// <summary>
		/// Creates a <see cref="IAsyncOperation"/> that has completed with the specified error message.
		/// </summary>
		/// <param name="message">An exception message.</param>
		/// <returns>A faulted operation.</returns>
		/// <seealso cref="FromException(System.Exception)"/>
		/// <seealso cref="FromCanceled()"/>
		/// <seealso cref="FromResult{T}(T)"/>
		public static AsyncResult FromException(string message)
		{
			return new AsyncResult(new Exception(message), null);
		}

		/// <summary>
		/// Creates a <see cref="IAsyncOperation"/> that has completed with a specified error message.
		/// </summary>
		/// <param name="message">An exception message.</param>
		/// <param name="asyncState">User-defined data returned by <see cref="AsyncState"/>.</param>
		/// <returns>A faulted operation.</returns>
		/// <seealso cref="FromException(System.Exception, object)"/>
		/// <seealso cref="FromCanceled(object)"/>
		/// <seealso cref="FromResult{T}(T, object)"/>
		public static AsyncResult FromException(string message, object asyncState)
		{
			return new AsyncResult(new Exception(message), asyncState);
		}

		/// <summary>
		/// Creates a <see cref="IAsyncOperation"/> that has completed with a specified exception.
		/// </summary>
		/// <param name="exception">The exception to complete the operation with.</param>
		/// <returns>A faulted operation.</returns>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="exception"/> is <see langword="null"/>.</exception>
		/// <seealso cref="FromException(System.Exception, object)"/>
		/// <seealso cref="FromException(string)"/>
		/// <seealso cref="FromCanceled()"/>
		/// <seealso cref="FromResult{T}(T)"/>
		public static AsyncResult FromException(Exception exception)
		{
			return new AsyncResult(exception, null);
		}

		/// <summary>
		/// Creates a <see cref="IAsyncOperation"/> that has completed with a specified exception.
		/// </summary>
		/// <param name="exception">The exception to complete the operation with.</param>
		/// <param name="asyncState">User-defined data returned by <see cref="AsyncState"/>.</param>
		/// <returns>A faulted operation.</returns>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="exception"/> is <see langword="null"/>.</exception>
		/// <seealso cref="FromException(System.Exception)"/>
		/// <seealso cref="FromException(string, object)"/>
		/// <seealso cref="FromCanceled(object)"/>
		/// <seealso cref="FromResult{T}(T, object)"/>
		public static AsyncResult FromException(Exception exception, object asyncState)
		{
			return new AsyncResult(exception, asyncState);
		}

		/// <summary>
		/// Creates a <see cref="IAsyncOperation{TResult}"/> that has completed with a specified error message.
		/// </summary>
		/// <param name="message">An exception message.</param>
		/// <returns>A faulted operation.</returns>
		/// <seealso cref="FromException{T}(System.Exception)"/>
		/// <seealso cref="FromCanceled{T}()"/>
		/// <seealso cref="FromResult{T}(T)"/>
		public static AsyncResult<T> FromException<T>(string message)
		{
			return new AsyncResult<T>(new Exception(message), null);
		}

		/// <summary>
		/// Creates a <see cref="IAsyncOperation{TResult}"/> that has completed with a specified error message.
		/// </summary>
		/// <param name="message">An exception message.</param>
		/// <param name="asyncState">User-defined data returned by <see cref="AsyncState"/>.</param>
		/// <returns>A faulted operation.</returns>
		/// <seealso cref="FromException{T}(System.Exception, object)"/>
		/// <seealso cref="FromCanceled{T}(object)"/>
		/// <seealso cref="FromResult{T}(T, object)"/>
		public static AsyncResult<T> FromException<T>(string message, object asyncState)
		{
			return new AsyncResult<T>(new Exception(message), asyncState);
		}

		/// <summary>
		/// Creates a <see cref="IAsyncOperation{TResult}"/> that has completed with a specified exception.
		/// </summary>
		/// <param name="exception">The exception to complete the operation with.</param>
		/// <returns>A faulted operation.</returns>
		/// <seealso cref="FromException(string)"/>
		/// <seealso cref="FromException{T}(System.Exception, object)"/>
		/// <seealso cref="FromCanceled{T}()"/>
		/// <seealso cref="FromResult{T}(T)"/>
		public static AsyncResult<T> FromException<T>(Exception exception)
		{
			return new AsyncResult<T>(exception, null);
		}

		/// <summary>
		/// Creates a <see cref="IAsyncOperation{TResult}"/> that has completed with a specified exception.
		/// </summary>
		/// <param name="exception">The exception to complete the operation with.</param>
		/// <param name="asyncState">User-defined data returned by <see cref="AsyncState"/>.</param>
		/// <returns>A faulted operation.</returns>
		/// <seealso cref="FromException(string, object)"/>
		/// <seealso cref="FromException{T}(System.Exception)"/>
		/// <seealso cref="FromCanceled{T}(object)"/>
		/// <seealso cref="FromResult{T}(T, object)"/>
		public static AsyncResult<T> FromException<T>(Exception exception, object asyncState)
		{
			return new AsyncResult<T>(exception, asyncState);
		}

		/// <summary>
		/// Creates a <see cref="IAsyncOperation{TResult}"/> that has completed with a specified result.
		/// </summary>
		/// <param name="result">The result value with which to complete the operation.</param>
		/// <returns>A completed operation with the specified result value.</returns>
		/// <seealso cref="FromResult{T}(T, object)"/>
		/// <seealso cref="FromCanceled{T}()"/>
		/// <seealso cref="FromException{T}(Exception)"/>
		public static AsyncResult<T> FromResult<T>(T result)
		{
			return new AsyncResult<T>(result, null);
		}

		/// <summary>
		/// Creates a <see cref="IAsyncOperation{TResult}"/> that has completed with a specified result.
		/// </summary>
		/// <param name="result">The result value with which to complete the operation.</param>
		/// <param name="asyncState">User-defined data returned by <see cref="AsyncState"/>.</param>
		/// <returns>A completed operation with the specified result value.</returns>
		/// <seealso cref="FromResult{T}(T)"/>
		/// <seealso cref="FromCanceled{T}(object)"/>
		/// <seealso cref="FromException{T}(Exception)"/>
		public static AsyncResult<T> FromResult<T>(T result, object asyncState)
		{
			return new AsyncResult<T>(result, asyncState);
		}

		/// <summary>
		/// Creates a completed <see cref="IAsyncOperation"/> that represents result of the <paramref name="action"/> specified.
		/// </summary>
		/// <param name="action">The delegate to execute.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="action"/> is <see langword="null"/>.</exception>
		/// <returns>A completed operation that represents <paramref name="action"/> result.</returns>
		/// <seealso cref="FromAction{T}(Action{T}, T)"/>
		/// <seealso cref="FromAction(SendOrPostCallback, object)"/>
		/// <seealso cref="FromAction{TResult}(Func{TResult})"/>
		public static AsyncResult FromAction(Action action)
		{
			if (action == null)
			{
				throw new ArgumentNullException(nameof(action));
			}

			try
			{
				action();
				return CompletedOperation;
			}
			catch (Exception e)
			{
				return new AsyncResult(e, null);
			}
		}

		/// <summary>
		/// Creates a completed <see cref="IAsyncOperation"/> that represents result of the <paramref name="action"/> specified.
		/// </summary>
		/// <param name="action">The delegate to execute.</param>
		/// <param name="state">User-defained state to pass to the <paramref name="action"/>.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="action"/> is <see langword="null"/>.</exception>
		/// <returns>A completed operation that represents <paramref name="action"/> result.</returns>
		/// <seealso cref="FromAction(Action)"/>
		/// <seealso cref="FromAction(SendOrPostCallback, object)"/>
		/// <seealso cref="FromAction{TResult}(Func{TResult})"/>
		public static AsyncResult FromAction<T>(Action<T> action, T state)
		{
			if (action == null)
			{
				throw new ArgumentNullException(nameof(action));
			}

			try
			{
				action(state);
				return CompletedOperation;
			}
			catch (Exception e)
			{
				return new AsyncResult(e, null);
			}
		}

		/// <summary>
		/// Creates a completed <see cref="IAsyncOperation"/> that represents result of the <paramref name="callback"/> specified.
		/// </summary>
		/// <param name="callback">The delegate to execute.</param>
		/// <param name="state">User-defained state to pass to the <paramref name="callback"/>.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="callback"/> is <see langword="null"/>.</exception>
		/// <returns>A completed operation that represents <paramref name="callback"/> result.</returns>
		/// <seealso cref="FromAction(Action)"/>
		/// <seealso cref="FromAction{T}(Action{T}, T)"/>
		/// <seealso cref="FromAction{TResult}(Func{TResult})"/>
		public static AsyncResult FromAction(SendOrPostCallback callback, object state)
		{
			if (callback == null)
			{
				throw new ArgumentNullException(nameof(callback));
			}

			try
			{
				callback(state);
				return CompletedOperation;
			}
			catch (Exception e)
			{
				return new AsyncResult(e, null);
			}
		}

		/// <summary>
		/// Creates a completed <see cref="IAsyncOperation"/> that represents result of the <paramref name="callback"/> specified.
		/// </summary>
		/// <param name="callback">The delegate to execute.</param>
		/// <param name="args">Arguments of the <paramref name="callback"/>.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="callback"/> is <see langword="null"/>.</exception>
		/// <returns>A completed operation that represents the <paramref name="callback"/> result.</returns>
		/// <seealso cref="FromAction(Action)"/>
		/// <seealso cref="FromAction(SendOrPostCallback, object)"/>
		public static AsyncResult<object> FromAction(Delegate callback, object[] args)
		{
			if (callback == null)
			{
				throw new ArgumentNullException(nameof(callback));
			}

			try
			{
				var result = callback.DynamicInvoke(args);
				return new AsyncResult<object>(result, null);
			}
			catch (Exception e)
			{
				return new AsyncResult<object>(e, null);
			}
		}

		/// <summary>
		/// Creates a completed <see cref="IAsyncOperation{TResult}"/> that represents result of the <paramref name="action"/> specified.
		/// </summary>
		/// <param name="action">The delegate to execute.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="action"/> is <see langword="null"/>.</exception>
		/// <returns>A completed operation that represents <paramref name="action"/> result.</returns>
		/// <seealso cref="FromAction(Action)"/>
		/// <seealso cref="FromAction{T, TResult}(Func{T, TResult}, T)"/>
		public static AsyncResult<TResult> FromAction<TResult>(Func<TResult> action)
		{
			if (action == null)
			{
				throw new ArgumentNullException(nameof(action));
			}

			try
			{
				var result = action();
				return new AsyncResult<TResult>(result, null);
			}
			catch (Exception e)
			{
				return new AsyncResult<TResult>(e, null);
			}
		}

		/// <summary>
		/// Creates a completed <see cref="IAsyncOperation"/> that represents result of the <paramref name="action"/> specified.
		/// </summary>
		/// <param name="action">The delegate to execute.</param>
		/// <param name="state">User-defained state to pass to the <paramref name="action"/>.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="action"/> is <see langword="null"/>.</exception>
		/// <returns>A completed operation that represents <paramref name="action"/> result.</returns>
		/// <seealso cref="FromAction{T}(Action{T}, T)"/>
		/// <seealso cref="FromAction{TResult}(Func{TResult})"/>
		public static AsyncResult<TResult> FromAction<T, TResult>(Func<T, TResult> action, T state)
		{
			if (action == null)
			{
				throw new ArgumentNullException(nameof(action));
			}

			try
			{
				var result = action(state);
				return new AsyncResult<TResult>(result, null);
			}
			catch (Exception e)
			{
				return new AsyncResult<TResult>(e, null);
			}
		}

#if !NET35

		/// <summary>
		/// Creates an <see cref="IAsyncOperation"/> instance that completes when the specified <paramref name="task"/> completes.
		/// </summary>
		/// <param name="task">The source <see cref="Task"/> instance.</param>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="task"/> reference is <see langword="null"/>.</exception>
		/// <returns>An <see cref="IAsyncOperation"/> that represents the source <paramref name="task"/>.</returns>
		/// <seealso cref="FromTask{T}(Task{T})"/>
		public static AsyncResult FromTask(Task task)
		{
			if (task == null)
			{
				throw new ArgumentNullException(nameof(task));
			}

			var result = new AsyncResult(AsyncOperationStatus.Running);

			task.ContinueWith(
				t =>
				{
					if (t.IsFaulted)
					{
						result.TrySetException(t.Exception);
					}
					else if (t.IsCanceled)
					{
						result.TrySetCanceled();
					}
					else
					{
						result.TrySetCompleted();
					}
				},
				TaskContinuationOptions.ExecuteSynchronously);

			return result;
		}

		/// <summary>
		/// Creates an <see cref="IAsyncOperation{TResult}"/> instance that completes when the specified <paramref name="task"/> completes.
		/// </summary>
		/// <param name="task">The source <see cref="Task{TResult}"/> instance.</param>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="task"/> reference is <see langword="null"/>.</exception>
		/// <returns>An <see cref="IAsyncOperation"/> that represents the source <paramref name="task"/>.</returns>
		/// <seealso cref="FromTask(Task)"/>
		public static AsyncResult<T> FromTask<T>(Task<T> task)
		{
			if (task == null)
			{
				throw new ArgumentNullException(nameof(task));
			}

			var result = new AsyncResult<T>(AsyncOperationStatus.Running);

			task.ContinueWith(
				t =>
				{
					if (t.IsFaulted)
					{
						result.TrySetException(t.Exception);
					}
					else if (t.IsCanceled)
					{
						result.TrySetCanceled();
					}
					else
					{
						result.TrySetResult(t.Result);
					}
				},
				TaskContinuationOptions.ExecuteSynchronously);

			return result;
		}

		/// <summary>
		/// Creates a completed <see cref="IAsyncOperation"/> that represents result of the <paramref name="action"/> specified.
		/// </summary>
		/// <param name="action">The delegate to execute.</param>
		/// <param name="cancellationToken">A cancellation token to check before executing the <paramref name="action"/>.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="action"/> is <see langword="null"/>.</exception>
		/// <returns>A completed operation that represents <paramref name="action"/> result.</returns>
		/// <seealso cref="FromAction(Action)"/>
		public static AsyncResult FromAction(Action action, CancellationToken cancellationToken)
		{
			if (cancellationToken.IsCancellationRequested)
			{
				return CanceledOperation;
			}

			return FromAction(action);
		}

		/// <summary>
		/// Creates a completed <see cref="IAsyncOperation{TResult}"/> that represents result of the <paramref name="action"/> specified.
		/// </summary>
		/// <param name="action">The delegate to execute.</param>
		/// <param name="cancellationToken">A cancellation token to check before executing the <paramref name="action"/>.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="action"/> is <see langword="null"/>.</exception>
		/// <returns>A completed operation that represents <paramref name="action"/> result.</returns>
		/// <seealso cref="FromAction{TResult}(Func{TResult})"/>
		public static AsyncResult<TResult> FromAction<TResult>(Func<TResult> action, CancellationToken cancellationToken)
		{
			if (cancellationToken.IsCancellationRequested)
			{
				return new AsyncResult<TResult>(AsyncOperationStatus.Canceled);
			}

			return FromAction(action);
		}

		/// <summary>
		/// Creates a <see cref="IAsyncOperation{T}"/> instance that can be used to track the source observable.
		/// </summary>
		/// <typeparam name="T">Type of the operation result.</typeparam>
		/// <param name="observable">The source observable.</param>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="observable"/> reference is <see langword="null"/>.</exception>
		/// <returns>Returns an <see cref="IAsyncOperation{T}"/> instance that can be used to track the observable.</returns>
		public static AsyncResult<T> FromObservable<T>(IObservable<T> observable)
		{
			if (observable == null)
			{
				throw new ArgumentNullException(nameof(observable));
			}

			return new FromObservableResult<T>(observable);
		}

#endif

		#endregion

		#region Delay

		/// <summary>
		/// Creates an operation that completes after a time delay.
		/// </summary>
		/// <param name="millisecondsDelay">The number of milliseconds to wait before completing the returned operation, or <see cref="Timeout.Infinite"/> (-1) to wait indefinitely.</param>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if the <paramref name="millisecondsDelay"/> is less than -1.</exception>
		/// <returns>An operation that represents the time delay.</returns>
		/// <seealso cref="Delay(int, IAsyncUpdateSource)"/>
		/// <seealso cref="Delay(float)"/>
		/// <seealso cref="Delay(TimeSpan)"/>
		public static AsyncResult Delay(int millisecondsDelay)
		{
			if (millisecondsDelay < Timeout.Infinite)
			{
				throw new ArgumentOutOfRangeException(nameof(millisecondsDelay), millisecondsDelay, Messages.FormatError_ValueIsLessThanZero());
			}

			if (millisecondsDelay == 0)
			{
				return CompletedOperation;
			}

			if (millisecondsDelay == Timeout.Infinite)
			{
				return new AsyncResult(AsyncOperationStatus.Running);
			}

			var result = new TimerDelayResult(millisecondsDelay);
			result.Start();
			return result;
		}

		/// <summary>
		/// Creates an operation that completes after a time delay. This method creates a more effecient operation
		/// than <see cref="Delay(int)"/> but requires a specialized update source.
		/// </summary>
		/// <param name="millisecondsDelay">The number of milliseconds to wait before completing the returned operation, or <see cref="Timeout.Infinite"/> (-1) to wait indefinitely.</param>
		/// <param name="updateSource">Update notifications provider.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="updateSource"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if the <paramref name="millisecondsDelay"/> is less than -1.</exception>
		/// <returns>An operation that represents the time delay.</returns>
		/// <seealso cref="Delay(int)"/>
		/// <seealso cref="Delay(float, IAsyncUpdateSource)"/>
		/// <seealso cref="Delay(TimeSpan, IAsyncUpdateSource)"/>
		public static AsyncResult Delay(int millisecondsDelay, IAsyncUpdateSource updateSource)
		{
			if (updateSource == null)
			{
				throw new ArgumentNullException(nameof(updateSource));
			}

			if (millisecondsDelay < Timeout.Infinite)
			{
				throw new ArgumentOutOfRangeException(nameof(millisecondsDelay), millisecondsDelay, Messages.FormatError_ValueIsLessThanZero());
			}

			if (millisecondsDelay == 0)
			{
				return CompletedOperation;
			}

			if (millisecondsDelay == Timeout.Infinite)
			{
				return new AsyncResult(AsyncOperationStatus.Running);
			}

			var result = new UpdatableDelayResult(millisecondsDelay / 1000f, updateSource);
			result.Start();
			return result;
		}

		/// <summary>
		/// Creates an operation that completes after a specified time interval.
		/// </summary>
		/// <param name="secondsDelay">The number of seconds to wait before completing the returned operation, or <see cref="Timeout.Infinite"/> (-1) to wait indefinitely.</param>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if the <paramref name="secondsDelay"/> represents a negative time interval.</exception>
		/// <returns>An operation that represents the time delay.</returns>
		/// <seealso cref="Delay(float, IAsyncUpdateSource)"/>
		/// <seealso cref="Delay(int)"/>
		/// <seealso cref="Delay(TimeSpan)"/>
		public static AsyncResult Delay(float secondsDelay)
		{
			var millisecondsDelay = (long)((double)secondsDelay * 1000);

			if (millisecondsDelay > int.MaxValue)
			{
				throw new ArgumentOutOfRangeException(nameof(secondsDelay));
			}

			return Delay((int)millisecondsDelay);
		}

		/// <summary>
		/// Creates an operation that completes after a specified time interval. This method creates a more effecient operation
		/// than <see cref="Delay(float)"/> but requires a specialized update source.
		/// </summary>
		/// <param name="secondsDelay">The number of seconds to wait before completing the returned operation, or <see cref="Timeout.Infinite"/> (-1) to wait indefinitely.</param>
		/// <param name="updateSource">Update notifications provider.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="updateSource"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if the <paramref name="secondsDelay"/> represents a negative time interval other than <c>TimeSpan.FromMillseconds(-1)</c>.</exception>
		/// <returns>An operation that represents the time delay.</returns>
		/// <seealso cref="Delay(float)"/>
		/// <seealso cref="Delay(int, IAsyncUpdateSource)"/>
		/// <seealso cref="Delay(TimeSpan, IAsyncUpdateSource)"/>
		public static AsyncResult Delay(float secondsDelay, IAsyncUpdateSource updateSource)
		{
			if (updateSource == null)
			{
				throw new ArgumentNullException(nameof(updateSource));
			}

			if (secondsDelay < Timeout.Infinite)
			{
				throw new ArgumentOutOfRangeException(nameof(secondsDelay), secondsDelay, Messages.FormatError_ValueIsLessThanZero());
			}

			if (secondsDelay == 0)
			{
				return CompletedOperation;
			}

			if (secondsDelay == Timeout.Infinite)
			{
				return new AsyncResult(AsyncOperationStatus.Running);
			}

			var result = new UpdatableDelayResult(secondsDelay, updateSource);
			result.Start();
			return result;
		}

		/// <summary>
		/// Creates an operation that completes after a specified time interval.
		/// </summary>
		/// <param name="delay">The time span to wait before completing the returned operation, or <c>TimeSpan.FromMilliseconds(-1)</c> to wait indefinitely.</param>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if the <paramref name="delay"/> represents a negative time interval other than <c>TimeSpan.FromMillseconds(-1)</c>.</exception>
		/// <returns>An operation that represents the time delay.</returns>
		/// <seealso cref="Delay(TimeSpan, IAsyncUpdateSource)"/>
		/// <seealso cref="Delay(int)"/>
		/// <seealso cref="Delay(float)"/>
		public static AsyncResult Delay(TimeSpan delay)
		{
			var millisecondsDelay = (long)delay.TotalMilliseconds;

			if (millisecondsDelay > int.MaxValue)
			{
				throw new ArgumentOutOfRangeException(nameof(delay));
			}

			return Delay((int)millisecondsDelay);
		}

		/// <summary>
		/// Creates an operation that completes after a specified time interval. This method creates a more effecient operation
		/// than <see cref="Delay(TimeSpan)"/> but requires a specialized update source.
		/// </summary>
		/// <param name="delay">The time span to wait before completing the returned operation, or <c>TimeSpan.FromMilliseconds(-1)</c> to wait indefinitely.</param>
		/// <param name="updateSource">Update notifications provider.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="updateSource"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if the <paramref name="delay"/> represents a negative time interval other than <c>TimeSpan.FromMillseconds(-1)</c>.</exception>
		/// <returns>An operation that represents the time delay.</returns>
		/// <seealso cref="Delay(TimeSpan)"/>
		/// <seealso cref="Delay(int, IAsyncUpdateSource)"/>
		/// <seealso cref="Delay(float, IAsyncUpdateSource)"/>
		public static AsyncResult Delay(TimeSpan delay, IAsyncUpdateSource updateSource)
		{
			var secondsDelay = delay.TotalSeconds;

			if (secondsDelay > float.MaxValue)
			{
				throw new ArgumentOutOfRangeException(nameof(delay));
			}

			return Delay((float)secondsDelay, updateSource);
		}

		#endregion

		#region Retry

		/// <summary>
		/// Creates an operation that completes when the source operation is completed successfully.
		/// </summary>
		/// <param name="opFactory">A delegate that initiates the source operation.</param>
		/// <param name="millisecondsRetryDelay">The number of milliseconds to wait after a failed try before starting a new operation.</param>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="opFactory"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="millisecondsRetryDelay"/> is less than zero.</exception>
		/// <returns>An operation that represents the retry process.</returns>
		/// <seealso cref="Retry(Func{IAsyncOperation}, TimeSpan, int)"/>
		public static AsyncResult Retry(Func<IAsyncOperation> opFactory, int millisecondsRetryDelay)
		{
			return Retry(opFactory, millisecondsRetryDelay, 0);
		}

		/// <summary>
		/// Creates an operation that completes when the source operation is completed successfully.
		/// </summary>
		/// <param name="opFactory">A delegate that initiates the source operation.</param>
		/// <param name="millisecondsRetryDelay">The number of milliseconds to wait after a failed try before starting a new operation.</param>
		/// <param name="updateSource">Update notifications provider.</param>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="opFactory"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="millisecondsRetryDelay"/> is less than zero.</exception>
		/// <returns>An operation that represents the retry process.</returns>
		/// <seealso cref="Retry(Func{IAsyncOperation}, TimeSpan, int, IAsyncUpdateSource)"/>
		public static AsyncResult Retry(Func<IAsyncOperation> opFactory, int millisecondsRetryDelay, IAsyncUpdateSource updateSource)
		{
			return Retry(opFactory, millisecondsRetryDelay, 0, updateSource);
		}

		/// <summary>
		/// Creates an operation that completes when the source operation is completed successfully or maximum number of retries exceeded.
		/// </summary>
		/// <param name="opFactory">A delegate that initiates the source operation.</param>
		/// <param name="millisecondsRetryDelay">The number of milliseconds to wait after a failed try before starting a new operation.</param>
		/// <param name="maxRetryCount">Maximum number of retries. Zero means no limits.</param>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="opFactory"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="millisecondsRetryDelay"/> or <paramref name="maxRetryCount"/> is less than zero.</exception>
		/// <returns>An operation that represents the retry process.</returns>
		/// <seealso cref="Retry(Func{IAsyncOperation}, TimeSpan, int)"/>
		public static AsyncResult Retry(Func<IAsyncOperation> opFactory, int millisecondsRetryDelay, int maxRetryCount)
		{
			if (opFactory == null)
			{
				throw new ArgumentNullException(nameof(opFactory));
			}

			if (millisecondsRetryDelay < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(millisecondsRetryDelay), millisecondsRetryDelay, Messages.FormatError_ValueIsLessThanZero());
			}

			if (maxRetryCount < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(maxRetryCount), maxRetryCount, Messages.FormatError_ValueIsLessThanZero());
			}

			var result = new TimerRetryResult<VoidResult>(opFactory, millisecondsRetryDelay, maxRetryCount);
			result.Start();
			return result;
		}

		/// <summary>
		/// Creates an operation that completes when the source operation is completed successfully or maximum number of retries exceeded.
		/// </summary>
		/// <param name="opFactory">A delegate that initiates the source operation.</param>
		/// <param name="millisecondsRetryDelay">The number of milliseconds to wait after a failed try before starting a new operation.</param>
		/// <param name="maxRetryCount">Maximum number of retries. Zero means no limits.</param>
		/// <param name="updateSource">Update notifications provider.</param>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="opFactory"/> or <paramref name="updateSource"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="millisecondsRetryDelay"/> or <paramref name="maxRetryCount"/> is less than zero.</exception>
		/// <returns>An operation that represents the retry process.</returns>
		/// <seealso cref="Retry(Func{IAsyncOperation}, TimeSpan, int, IAsyncUpdateSource)"/>
		public static AsyncResult Retry(Func<IAsyncOperation> opFactory, int millisecondsRetryDelay, int maxRetryCount, IAsyncUpdateSource updateSource)
		{
			if (opFactory == null)
			{
				throw new ArgumentNullException(nameof(opFactory));
			}

			if (updateSource == null)
			{
				throw new ArgumentNullException(nameof(updateSource));
			}

			if (millisecondsRetryDelay < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(millisecondsRetryDelay), millisecondsRetryDelay, Messages.FormatError_ValueIsLessThanZero());
			}

			if (maxRetryCount < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(maxRetryCount), maxRetryCount, Messages.FormatError_ValueIsLessThanZero());
			}

			var result = new UpdatableRetryResult<VoidResult>(opFactory, millisecondsRetryDelay, maxRetryCount, updateSource);
			result.Start();
			return result;
		}

		/// <summary>
		/// Creates an operation that completes when the source operation is completed successfully.
		/// </summary>
		/// <param name="opFactory">A delegate that initiates the source operation.</param>
		/// <param name="retryDelay">The time to wait after a failed try before starting a new operation.</param>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="opFactory"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="retryDelay"/> is less than zero.</exception>
		/// <returns>An operation that represents the retry process.</returns>
		/// <seealso cref="Retry(Func{IAsyncOperation}, int, int)"/>
		public static AsyncResult Retry(Func<IAsyncOperation> opFactory, TimeSpan retryDelay)
		{
			return Retry(opFactory, retryDelay, 0);
		}

		/// <summary>
		/// Creates an operation that completes when the source operation is completed successfully.
		/// </summary>
		/// <param name="opFactory">A delegate that initiates the source operation.</param>
		/// <param name="retryDelay">The time to wait after a failed try before starting a new operation.</param>
		/// <param name="updateSource">Update notifications provider.</param>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="opFactory"/> or <paramref name="updateSource"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="retryDelay"/> is less than zero.</exception>
		/// <returns>An operation that represents the retry process.</returns>
		/// <seealso cref="Retry(Func{IAsyncOperation}, int, int, IAsyncUpdateSource)"/>
		public static AsyncResult Retry(Func<IAsyncOperation> opFactory, TimeSpan retryDelay, IAsyncUpdateSource updateSource)
		{
			return Retry(opFactory, retryDelay, 0, updateSource);
		}

		/// <summary>
		/// Creates an operation that completes when the source operation is completed successfully or maximum number of retries exceeded.
		/// </summary>
		/// <param name="opFactory">A delegate that initiates the source operation.</param>
		/// <param name="retryDelay">The time to wait after a failed try before starting a new operation.</param>
		/// <param name="maxRetryCount">Maximum number of retries. Zero means no limits.</param>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="opFactory"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="retryDelay"/> or <paramref name="maxRetryCount"/> is less than zero.</exception>
		/// <returns>An operation that represents the retry process.</returns>
		/// <seealso cref="Retry(Func{IAsyncOperation}, int, int)"/>
		public static AsyncResult Retry(Func<IAsyncOperation> opFactory, TimeSpan retryDelay, int maxRetryCount)
		{
			var millisecondsDelay = (long)retryDelay.TotalMilliseconds;

			if (millisecondsDelay > int.MaxValue)
			{
				throw new ArgumentOutOfRangeException(nameof(retryDelay));
			}

			return Retry(opFactory, (int)millisecondsDelay, maxRetryCount);
		}

		/// <summary>
		/// Creates an operation that completes when the source operation is completed successfully or maximum number of retries exceeded.
		/// </summary>
		/// <param name="opFactory">A delegate that initiates the source operation.</param>
		/// <param name="retryDelay">The time to wait after a failed try before starting a new operation.</param>
		/// <param name="maxRetryCount">Maximum number of retries. Zero means no limits.</param>
		/// <param name="updateSource">Update notifications provider.</param>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="opFactory"/> or <paramref name="updateSource"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="retryDelay"/> or <paramref name="maxRetryCount"/> is less than zero.</exception>
		/// <returns>An operation that represents the retry process.</returns>
		/// <seealso cref="Retry(Func{IAsyncOperation}, int, int, IAsyncUpdateSource)"/>
		public static AsyncResult Retry(Func<IAsyncOperation> opFactory, TimeSpan retryDelay, int maxRetryCount, IAsyncUpdateSource updateSource)
		{
			var millisecondsDelay = (long)retryDelay.TotalMilliseconds;

			if (millisecondsDelay > int.MaxValue)
			{
				throw new ArgumentOutOfRangeException(nameof(retryDelay));
			}

			return Retry(opFactory, (int)millisecondsDelay, maxRetryCount, updateSource);
		}

		/// <summary>
		/// Creates an operation that completes when the source operation is completed successfully.
		/// </summary>
		/// <param name="opFactory">A delegate that initiates the source operation.</param>
		/// <param name="millisecondsRetryDelay">The number of milliseconds to wait after a failed try before starting a new operation.</param>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="opFactory"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="millisecondsRetryDelay"/> is less than zero.</exception>
		/// <returns>An operation that represents the retry process.</returns>
		/// <seealso cref="Retry{TResult}(Func{IAsyncOperation{TResult}}, TimeSpan, int)"/>
		public static AsyncResult<TResult> Retry<TResult>(Func<IAsyncOperation<TResult>> opFactory, int millisecondsRetryDelay)
		{
			return Retry(opFactory, millisecondsRetryDelay, 0);
		}

		/// <summary>
		/// Creates an operation that completes when the source operation is completed successfully.
		/// </summary>
		/// <param name="opFactory">A delegate that initiates the source operation.</param>
		/// <param name="millisecondsRetryDelay">The number of milliseconds to wait after a failed try before starting a new operation.</param>
		/// <param name="updateSource">Update notifications provider.</param>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="opFactory"/> or <paramref name="updateSource"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="millisecondsRetryDelay"/> is less than zero.</exception>
		/// <returns>An operation that represents the retry process.</returns>
		/// <seealso cref="Retry{TResult}(Func{IAsyncOperation{TResult}}, TimeSpan, int, IAsyncUpdateSource)"/>
		public static AsyncResult<TResult> Retry<TResult>(Func<IAsyncOperation<TResult>> opFactory, int millisecondsRetryDelay, IAsyncUpdateSource updateSource)
		{
			return Retry(opFactory, millisecondsRetryDelay, 0, updateSource);
		}

		/// <summary>
		/// Creates an operation that completes when the source operation is completed successfully or maximum number of retries exceeded.
		/// </summary>
		/// <param name="opFactory">A delegate that initiates the source operation.</param>
		/// <param name="millisecondsRetryDelay">The number of milliseconds to wait after a failed try before starting a new operation.</param>
		/// <param name="maxRetryCount">Maximum number of retries. Zero means no limits.</param>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="opFactory"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="millisecondsRetryDelay"/> or <paramref name="maxRetryCount"/> is less than zero.</exception>
		/// <returns>An operation that represents the retry process.</returns>
		/// <seealso cref="Retry{TResult}(Func{IAsyncOperation{TResult}}, TimeSpan, int)"/>
		public static AsyncResult<TResult> Retry<TResult>(Func<IAsyncOperation<TResult>> opFactory, int millisecondsRetryDelay, int maxRetryCount)
		{
			if (opFactory == null)
			{
				throw new ArgumentNullException(nameof(opFactory));
			}

			if (millisecondsRetryDelay < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(millisecondsRetryDelay), millisecondsRetryDelay, Messages.FormatError_ValueIsLessThanZero());
			}

			if (maxRetryCount < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(maxRetryCount), maxRetryCount, Messages.FormatError_ValueIsLessThanZero());
			}

			var result = new TimerRetryResult<TResult>(opFactory, millisecondsRetryDelay, maxRetryCount);
			result.Start();
			return result;
		}

		/// <summary>
		/// Creates an operation that completes when the source operation is completed successfully or maximum number of retries exceeded.
		/// </summary>
		/// <param name="opFactory">A delegate that initiates the source operation.</param>
		/// <param name="millisecondsRetryDelay">The number of milliseconds to wait after a failed try before starting a new operation.</param>
		/// <param name="maxRetryCount">Maximum number of retries. Zero means no limits.</param>
		/// <param name="updateSource">Update notifications provider.</param>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="opFactory"/> or <paramref name="updateSource"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="millisecondsRetryDelay"/> or <paramref name="maxRetryCount"/> is less than zero.</exception>
		/// <returns>An operation that represents the retry process.</returns>
		/// <seealso cref="Retry{TResult}(Func{IAsyncOperation{TResult}}, TimeSpan, int, IAsyncUpdateSource)"/>
		public static AsyncResult<TResult> Retry<TResult>(Func<IAsyncOperation<TResult>> opFactory, int millisecondsRetryDelay, int maxRetryCount, IAsyncUpdateSource updateSource)
		{
			if (opFactory == null)
			{
				throw new ArgumentNullException(nameof(opFactory));
			}

			if (updateSource == null)
			{
				throw new ArgumentNullException(nameof(updateSource));
			}

			if (millisecondsRetryDelay < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(millisecondsRetryDelay), millisecondsRetryDelay, Messages.FormatError_ValueIsLessThanZero());
			}

			if (maxRetryCount < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(maxRetryCount), maxRetryCount, Messages.FormatError_ValueIsLessThanZero());
			}

			var result = new UpdatableRetryResult<TResult>(opFactory, millisecondsRetryDelay, maxRetryCount, updateSource);
			result.Start();
			return result;
		}

		/// <summary>
		/// Creates an operation that completes when the source operation is completed successfully.
		/// </summary>
		/// <param name="opFactory">A delegate that initiates the source operation.</param>
		/// <param name="retryDelay">The time to wait after a failed try before starting a new operation.</param>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="opFactory"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="retryDelay"/> is less than zero.</exception>
		/// <returns>An operation that represents the retry process.</returns>
		/// <seealso cref="Retry{TResult}(Func{IAsyncOperation{TResult}}, int, int)"/>
		public static AsyncResult<TResult> Retry<TResult>(Func<IAsyncOperation<TResult>> opFactory, TimeSpan retryDelay)
		{
			return Retry(opFactory, retryDelay, 0);
		}

		/// <summary>
		/// Creates an operation that completes when the source operation is completed successfully.
		/// </summary>
		/// <param name="opFactory">A delegate that initiates the source operation.</param>
		/// <param name="retryDelay">The time to wait after a failed try before starting a new operation.</param>
		/// <param name="updateSource">Update notifications provider.</param>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="opFactory"/> or <paramref name="updateSource"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="retryDelay"/> is less than zero.</exception>
		/// <returns>An operation that represents the retry process.</returns>
		/// <seealso cref="Retry{TResult}(Func{IAsyncOperation{TResult}}, int, int, IAsyncUpdateSource)"/>
		public static AsyncResult<TResult> Retry<TResult>(Func<IAsyncOperation<TResult>> opFactory, TimeSpan retryDelay, IAsyncUpdateSource updateSource)
		{
			return Retry(opFactory, retryDelay, 0, updateSource);
		}

		/// <summary>
		/// Creates an operation that completes when the source operation is completed successfully or maximum number of retries exceeded.
		/// </summary>
		/// <param name="opFactory">A delegate that initiates the source operation.</param>
		/// <param name="retryDelay">The time to wait after a failed try before starting a new operation.</param>
		/// <param name="maxRetryCount">Maximum number of retries. Zero means no limits.</param>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="opFactory"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="retryDelay"/> or <paramref name="maxRetryCount"/> is less than zero.</exception>
		/// <returns>An operation that represents the retry process.</returns>
		/// <seealso cref="Retry{TResult}(Func{IAsyncOperation{TResult}}, int, int)"/>
		public static AsyncResult<TResult> Retry<TResult>(Func<IAsyncOperation<TResult>> opFactory, TimeSpan retryDelay, int maxRetryCount)
		{
			var millisecondsDelay = (long)retryDelay.TotalMilliseconds;

			if (millisecondsDelay > int.MaxValue)
			{
				throw new ArgumentOutOfRangeException(nameof(retryDelay));
			}

			return Retry(opFactory, (int)millisecondsDelay, maxRetryCount);
		}

		/// <summary>
		/// Creates an operation that completes when the source operation is completed successfully or maximum number of retries exceeded.
		/// </summary>
		/// <param name="opFactory">A delegate that initiates the source operation.</param>
		/// <param name="retryDelay">The time to wait after a failed try before starting a new operation.</param>
		/// <param name="maxRetryCount">Maximum number of retries. Zero means no limits.</param>
		/// <param name="updateSource">Update notifications provider.</param>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="opFactory"/> or <paramref name="updateSource"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="retryDelay"/> or <paramref name="maxRetryCount"/> is less than zero.</exception>
		/// <returns>An operation that represents the retry process.</returns>
		/// <seealso cref="Retry{TResult}(Func{IAsyncOperation{TResult}}, int, int, IAsyncUpdateSource)"/>
		public static AsyncResult<TResult> Retry<TResult>(Func<IAsyncOperation<TResult>> opFactory, TimeSpan retryDelay, int maxRetryCount, IAsyncUpdateSource updateSource)
		{
			var millisecondsDelay = (long)retryDelay.TotalMilliseconds;

			if (millisecondsDelay > int.MaxValue)
			{
				throw new ArgumentOutOfRangeException(nameof(retryDelay));
			}

			return Retry(opFactory, (int)millisecondsDelay, maxRetryCount, updateSource);
		}

		#endregion

		#region WhenAll

		/// <summary>
		/// Creates an operation that will complete when all of the specified objects in an enumerable collection have completed.
		/// </summary>
		/// <param name="ops">The operations to wait on for completion.</param>
		/// <returns>An operation that represents the completion of all of the supplied operations.</returns>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="ops"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException">Thrown if the <paramref name="ops"/> collection contained a <see langword="null"/> operation..</exception>
		/// <seealso cref="WhenAll{TResult}(IEnumerable{IAsyncOperation{TResult}})"/>
		/// <seealso cref="WhenAll(IAsyncOperation[])"/>
		public static AsyncResult WhenAll(IEnumerable<IAsyncOperation> ops)
		{
			if (ops == null)
			{
				throw new ArgumentNullException(nameof(ops));
			}

			var opList = new List<IAsyncOperation>();

			foreach (var op in ops)
			{
				if (op == null)
				{
					throw new ArgumentException(Messages.FormatError_ListElementIsNull(), nameof(ops));
				}

				opList.Add(op);
			}

			if (opList.Count == 0)
			{
				return CompletedOperation;
			}

			return new WhenAllResult<VoidResult>(opList);
		}

		/// <summary>
		/// Creates an operation that will complete when all of the specified objects in an enumerable collection have completed.
		/// </summary>
		/// <param name="ops">The operations to wait on for completion.</param>
		/// <returns>An operation that represents the completion of all of the supplied operations.</returns>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="ops"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException">Thrown if the <paramref name="ops"/> collection contained a <see langword="null"/> operation..</exception>
		/// <seealso cref="WhenAll(IEnumerable{IAsyncOperation})"/>
		/// <seealso cref="WhenAll{TResult}(IAsyncOperation{TResult}[])"/>
		public static AsyncResult<T[]> WhenAll<T>(IEnumerable<IAsyncOperation<T>> ops)
		{
			if (ops == null)
			{
				throw new ArgumentNullException(nameof(ops));
			}

			var opList = new List<IAsyncOperation>();

			foreach (var op in ops)
			{
				if (op == null)
				{
					throw new ArgumentException(Messages.FormatError_ListElementIsNull(), nameof(ops));
				}

				opList.Add(op);
			}

			if (opList.Count == 0)
			{
				return FromResult(new T[0]);
			}

			return new WhenAllResult<T>(opList);
		}

		/// <summary>
		/// Creates an operation that will complete when all of the specified objects in an array have completed.
		/// </summary>
		/// <param name="ops">The operations to wait on for completion.</param>
		/// <returns>An operation that represents the completion of all of the supplied operations.</returns>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="ops"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException">Thrown if the <paramref name="ops"/> collection contained a <see langword="null"/> operation..</exception>
		/// <seealso cref="WhenAll{T}(IAsyncOperation{T}[])"/>
		/// <seealso cref="WhenAll(IEnumerable{IAsyncOperation})"/>
		public static AsyncResult WhenAll(params IAsyncOperation[] ops)
		{
			if (ops == null)
			{
				throw new ArgumentNullException(nameof(ops));
			}

			if (ops.Length == 0)
			{
				return CompletedOperation;
			}

			var opArray = new IAsyncOperation[ops.Length];

			for (var i = 0; i < ops.Length; i++)
			{
				if (ops[i] == null)
				{
					throw new ArgumentException(Messages.FormatError_ListElementIsNull(), nameof(ops));
				}

				opArray[i] = ops[i];
			}

			return new WhenAllResult<VoidResult>(opArray);
		}

		/// <summary>
		/// Creates an operation that will complete when all of the specified objects in an array have completed.
		/// </summary>
		/// <param name="ops">The operations to wait on for completion.</param>
		/// <returns>An operation that represents the completion of all of the supplied operations.</returns>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="ops"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException">Thrown if the <paramref name="ops"/> collection contained a <see langword="null"/> operation.</exception>
		/// <seealso cref="WhenAll(IAsyncOperation[])"/>
		/// <seealso cref="WhenAll{T}(IEnumerable{IAsyncOperation{T}})"/>
		public static AsyncResult<T[]> WhenAll<T>(params IAsyncOperation<T>[] ops)
		{
			if (ops == null)
			{
				throw new ArgumentNullException(nameof(ops));
			}

			if (ops.Length == 0)
			{
				return FromResult(new T[0]);
			}

			var opArray = new IAsyncOperation<T>[ops.Length];

			for (var i = 0; i < ops.Length; i++)
			{
				if (ops[i] == null)
				{
					throw new ArgumentException(Messages.FormatError_ListElementIsNull(), nameof(ops));
				}

				opArray[i] = ops[i];
			}

			return new WhenAllResult<T>(opArray);
		}

		#endregion

		#region WhenAny

		/// <summary>
		/// Creates an operation that will complete when any of the specified objects in an enumerable collection have completed.
		/// </summary>
		/// <param name="ops">The operations to wait on for completion.</param>
		/// <returns>An operation that represents the completion of any of the supplied operations.</returns>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="ops"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException">Thrown if the <paramref name="ops"/> collection contained a <see langword="null"/> operation..</exception>
		/// <seealso cref="WhenAny{T}(T[])"/>
		public static AsyncResult<T> WhenAny<T>(IEnumerable<T> ops) where T : IAsyncOperation
		{
			if (ops == null)
			{
				throw new ArgumentNullException(nameof(ops));
			}

			var opList = new List<T>();

			foreach (var op in ops)
			{
				if (op == null)
				{
					throw new ArgumentException(Messages.FormatError_ListElementIsNull(), nameof(ops));
				}

				opList.Add(op);
			}

			if (opList.Count == 0)
			{
				throw new ArgumentException(Messages.FormatError_ListIsEmpty(), nameof(ops));
			}

			return new WhenAnyResult<T>(opList);
		}

		/// <summary>
		/// Creates an operation that will complete when any of the specified objects in an array have completed.
		/// </summary>
		/// <param name="ops">The operations to wait on for completion.</param>
		/// <returns>An operation that represents the completion of any of the supplied operations.</returns>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="ops"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException">Thrown if the <paramref name="ops"/> collection contained a <see langword="null"/> operation..</exception>
		/// <seealso cref="WhenAny{T}(IEnumerable{T})"/>
		public static AsyncResult<T> WhenAny<T>(params T[] ops) where T : IAsyncOperation
		{
			if (ops == null)
			{
				throw new ArgumentNullException(nameof(ops));
			}

			if (ops.Length == 0)
			{
				throw new ArgumentException(Messages.FormatError_ListIsEmpty(), nameof(ops));
			}

			var opArray = new T[ops.Length];

			for (var i = 0; i < ops.Length; i++)
			{
				if (ops[i] == null)
				{
					throw new ArgumentException(Messages.FormatError_ListElementIsNull(), nameof(ops));
				}

				opArray[i] = ops[i];
			}

			return new WhenAnyResult<T>(opArray);
		}

		#endregion

		#endregion
	}
}
