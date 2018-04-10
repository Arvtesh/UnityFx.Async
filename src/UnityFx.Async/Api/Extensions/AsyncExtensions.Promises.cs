// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;

namespace UnityFx.Async
{
	partial class AsyncExtensions
	{
		#region Then

		/// <summary>
		/// Schedules a callback to be executed after the operation has been resolved.
		/// </summary>
		/// <param name="op">An operation to be continued.</param>
		/// <param name="successCallback">The callback to be executed when the operation has completed.</param>
		/// <returns>Returns a continuation operation that completes after both source operation and the callback has completed.</returns>
		/// <seealso cref="Then{TResult}(IAsyncOperation{TResult}, Action{TResult})"/>
		/// <seealso href="https://promisesaplus.com/"/>
		public static IAsyncOperation Then(this IAsyncOperation op, Action successCallback)
		{
			if (successCallback == null)
			{
				throw new ArgumentNullException(nameof(successCallback));
			}

			return new ThenResult<VoidResult, VoidResult>(op, successCallback, null);
		}

		/// <summary>
		/// Schedules a callback to be executed after the operation has been resolved.
		/// </summary>
		/// <param name="op">An operation to be continued.</param>
		/// <param name="successCallback">The callback to be executed when the operation has completed.</param>
		/// <returns>Returns a continuation operation that completes after both source operation and the callback has completed.</returns>
		/// <seealso cref="Then(IAsyncOperation, Action)"/>
		/// <seealso href="https://promisesaplus.com/"/>
		public static IAsyncOperation Then<TResult>(this IAsyncOperation<TResult> op, Action<TResult> successCallback)
		{
			if (successCallback == null)
			{
				throw new ArgumentNullException(nameof(successCallback));
			}

			return new ThenResult<TResult, VoidResult>(op, successCallback, null);
		}

		/// <summary>
		/// Schedules a callback to be executed after the operation has been resolved.
		/// </summary>
		/// <param name="op">An operation to be continued.</param>
		/// <param name="successCallback">The callback to be executed when the operation has completed.</param>
		/// <returns>Returns a continuation operation that completes after both source operation and the operation returned by <paramref name="successCallback"/> has completed.</returns>
		/// <seealso cref="Then{TResult}(IAsyncOperation, Func{IAsyncOperation{TResult}})"/>
		/// <seealso cref="Then{TResult}(IAsyncOperation{TResult}, Func{TResult, IAsyncOperation})"/>
		/// <seealso href="https://promisesaplus.com/"/>
		public static IAsyncOperation Then(this IAsyncOperation op, Func<IAsyncOperation> successCallback)
		{
			if (successCallback == null)
			{
				throw new ArgumentNullException(nameof(successCallback));
			}

			return new ThenResult<VoidResult, VoidResult>(op, successCallback, null);
		}

		/// <summary>
		/// Schedules a callback to be executed after the operation has been resolved.
		/// </summary>
		/// <param name="op">An operation to be continued.</param>
		/// <param name="successCallback">The callback to be executed when the operation has completed.</param>
		/// <returns>Returns a continuation operation that completes after both source operation and the operation returned by <paramref name="successCallback"/> has completed.</returns>
		/// <seealso cref="Then(IAsyncOperation, Func{IAsyncOperation})"/>
		/// <seealso cref="Then{TResult}(IAsyncOperation, Func{IAsyncOperation{TResult}})"/>
		/// <seealso href="https://promisesaplus.com/"/>
		public static IAsyncOperation Then<TResult>(this IAsyncOperation<TResult> op, Func<TResult, IAsyncOperation> successCallback)
		{
			if (successCallback == null)
			{
				throw new ArgumentNullException(nameof(successCallback));
			}

			return new ThenResult<TResult, VoidResult>(op, successCallback, null);
		}

		/// <summary>
		/// Schedules a callback to be executed after the operation has been resolved.
		/// </summary>
		/// <param name="op">An operation to be continued.</param>
		/// <param name="successCallback">The callback to be executed when the operation has completed.</param>
		/// <returns>Returns a continuation operation that completes after both source operation and the operation returned by <paramref name="successCallback"/> has completed.</returns>
		/// <seealso cref="Then(IAsyncOperation, Func{IAsyncOperation})"/>
		/// <seealso cref="Then{TResult}(IAsyncOperation{TResult}, Func{TResult, IAsyncOperation})"/>
		/// <seealso href="https://promisesaplus.com/"/>
		public static IAsyncOperation<TResult> Then<TResult>(this IAsyncOperation op, Func<IAsyncOperation<TResult>> successCallback)
		{
			if (successCallback == null)
			{
				throw new ArgumentNullException(nameof(successCallback));
			}

			return new ThenResult<VoidResult, TResult>(op, successCallback, null);
		}

		/// <summary>
		/// Schedules a callback to be executed after the operation has been resolved.
		/// </summary>
		/// <param name="op">An operation to be continued.</param>
		/// <param name="successCallback">The callback to be executed when the operation has completed.</param>
		/// <returns>Returns a continuation operation that completes after both source operation and the operation returned by <paramref name="successCallback"/> has completed.</returns>
		/// <seealso cref="Then(IAsyncOperation, Func{IAsyncOperation})"/>
		/// <seealso cref="Then{TResult}(IAsyncOperation, Func{IAsyncOperation{TResult}})"/>
		/// <seealso cref="Then{TResult}(IAsyncOperation{TResult}, Func{TResult, IAsyncOperation})"/>
		/// <seealso href="https://promisesaplus.com/"/>
		public static IAsyncOperation<TNewResult> Then<TResult, TNewResult>(this IAsyncOperation<TResult> op, Func<TResult, IAsyncOperation<TNewResult>> successCallback)
		{
			if (successCallback == null)
			{
				throw new ArgumentNullException(nameof(successCallback));
			}

			return new ThenResult<TResult, TNewResult>(op, successCallback, null);
		}

		/// <summary>
		/// Schedules a callbacks to be executed after the operation has been resolved.
		/// </summary>
		/// <param name="op">An operation to be continued.</param>
		/// <param name="successCallback">The callback to be executed when the operation has succeeded.</param>
		/// <param name="errorCallback">The callback to be executed when the operation has faulted/was canceled.</param>
		/// <returns>Returns a continuation operation that completes after both source operation and the callback has completed.</returns>
		/// <seealso cref="Then{TResult}(IAsyncOperation{TResult}, Action{TResult}, Action{Exception})"/>
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

			return new ThenResult<VoidResult, VoidResult>(op, successCallback, errorCallback);
		}

		/// <summary>
		/// Schedules a callbacks to be executed after the operation has been resolved.
		/// </summary>
		/// <param name="op">An operation to be continued.</param>
		/// <param name="successCallback">The callback to be executed when the operation has succeeded.</param>
		/// <param name="errorCallback">The callback to be executed when the operation has faulted/was canceled.</param>
		/// <returns>Returns a continuation operation that completes after both source operation and the callback has completed.</returns>
		/// <seealso cref="Then(IAsyncOperation, Action, Action{Exception})"/>
		/// <seealso href="https://promisesaplus.com/"/>
		public static IAsyncOperation Then<TResult>(this IAsyncOperation<TResult> op, Action<TResult> successCallback, Action<Exception> errorCallback)
		{
			if (successCallback == null)
			{
				throw new ArgumentNullException(nameof(successCallback));
			}

			if (errorCallback == null)
			{
				throw new ArgumentNullException(nameof(errorCallback));
			}

			return new ThenResult<TResult, VoidResult>(op, successCallback, errorCallback);
		}

		/// <summary>
		/// Adds a completion callback to be executed after the operation has been resolved.
		/// </summary>
		/// <param name="op">An operation to be continued.</param>
		/// <param name="successCallback">The callback to be executed when the operation has succeeded.</param>
		/// <param name="errorCallback">The callback to be executed when the operation has faulted/was canceled.</param>
		/// <returns>Returns a continuation operation that completes after both source operation and the operation returned by <paramref name="successCallback"/> has completed.</returns>
		/// <seealso cref="Then{TResult}(IAsyncOperation{TResult}, Func{TResult, IAsyncOperation})"/>
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

			return new ThenResult<VoidResult, VoidResult>(op, successCallback, errorCallback);
		}

		/// <summary>
		/// Adds a completion callback to be executed after the operation has been resolved.
		/// </summary>
		/// <param name="op">An operation to be continued.</param>
		/// <param name="successCallback">The callback to be executed when the operation has succeeded.</param>
		/// <param name="errorCallback">The callback to be executed when the operation has faulted/was canceled.</param>
		/// <returns>Returns a continuation operation that completes after both source operation and the operation returned by <paramref name="successCallback"/> has completed.</returns>
		/// <seealso cref="Then(IAsyncOperation, Func{IAsyncOperation}, Action{Exception})"/>
		/// <seealso href="https://promisesaplus.com/"/>
		public static IAsyncOperation Then<TResult>(this IAsyncOperation<TResult> op, Func<TResult, IAsyncOperation> successCallback, Action<Exception> errorCallback)
		{
			if (successCallback == null)
			{
				throw new ArgumentNullException(nameof(successCallback));
			}

			if (errorCallback == null)
			{
				throw new ArgumentNullException(nameof(errorCallback));
			}

			return new ThenResult<TResult, VoidResult>(op, successCallback, errorCallback);
		}

		#endregion

		#region ThenAll

		/// <summary>
		/// Schedules a callback to be executed after the operation has been resolved. The resulting operation will complete after all of the operations in the callback return value have completed.
		/// </summary>
		/// <param name="op">An operation to be continued.</param>
		/// <param name="successCallback">The callback to be executed when the operation has completed.</param>
		/// <returns>Returns a continuation operation that completes after both source operation and the callback has completed.</returns>
		/// <seealso cref="ThenAll{T}(IAsyncOperation{T}, Func{T, IEnumerable{IAsyncOperation}})"/>
		public static IAsyncOperation ThenAll(this IAsyncOperation op, Func<IEnumerable<IAsyncOperation>> successCallback)
		{
			if (successCallback == null)
			{
				throw new ArgumentNullException(nameof(successCallback));
			}

			return new ThenAllResult<VoidResult, VoidResult>(op, successCallback, null);
		}

		/// <summary>
		/// Schedules a callback to be executed after the operation has been resolved. The resulting operation will complete after all of the operations in the callback return value have completed.
		/// </summary>
		/// <param name="op">An operation to be continued.</param>
		/// <param name="successCallback">The callback to be executed when the operation has completed.</param>
		/// <returns>Returns a continuation operation that completes after both source operation and the callback has completed.</returns>
		/// <seealso cref="ThenAll(IAsyncOperation, Func{IEnumerable{IAsyncOperation}})"/>
		public static IAsyncOperation ThenAll<T>(this IAsyncOperation<T> op, Func<T, IEnumerable<IAsyncOperation>> successCallback)
		{
			if (successCallback == null)
			{
				throw new ArgumentNullException(nameof(successCallback));
			}

			return new ThenAllResult<T, VoidResult>(op, successCallback, null);
		}

		/// <summary>
		/// Schedules a callback to be executed after the operation has been resolved. The resulting operation will complete after all of the operations in the callback return value have completed.
		/// </summary>
		/// <param name="op">An operation to be continued.</param>
		/// <param name="successCallback">The callback to be executed when the operation has completed.</param>
		/// <returns>Returns a continuation operation that completes after both source operation and the callback has completed.</returns>
		/// <seealso cref="ThenAll{T, U}(IAsyncOperation{T}, Func{T, IEnumerable{IAsyncOperation{U}}})"/>
		public static IAsyncOperation<T[]> ThenAll<T>(this IAsyncOperation op, Func<IEnumerable<IAsyncOperation<T>>> successCallback)
		{
			if (successCallback == null)
			{
				throw new ArgumentNullException(nameof(successCallback));
			}

			return new ThenAllResult<VoidResult, T>(op, successCallback, null);
		}

		/// <summary>
		/// Schedules a callback to be executed after the operation has been resolved. The resulting operation will complete after all of the specified objects in an array have completed.
		/// </summary>
		/// <param name="op">An operation to be continued.</param>
		/// <param name="successCallback">The callback to be executed when the operation has completed.</param>
		/// <returns>Returns a continuation operation that completes after both source operation and the callback has completed.</returns>
		/// <seealso cref="ThenAll{T}(IAsyncOperation, Func{IEnumerable{IAsyncOperation{T}}})"/>
		public static IAsyncOperation<U[]> ThenAll<T, U>(this IAsyncOperation<T> op, Func<T, IEnumerable<IAsyncOperation<U>>> successCallback)
		{
			if (successCallback == null)
			{
				throw new ArgumentNullException(nameof(successCallback));
			}

			return new ThenAllResult<T, U>(op, successCallback, null);
		}

		#endregion

		#region ThenAny

		/// <summary>
		/// Schedules a callback to be executed after the operation has been resolved. The resulting operation will complete after any of the operations in the callback return value have completed.
		/// </summary>
		/// <param name="op">An operation to be continued.</param>
		/// <param name="successCallback">The callback to be executed when the operation has completed.</param>
		/// <returns>Returns a continuation operation that completes after both source operation and the callback has completed.</returns>
		/// <seealso cref="ThenAny{TResult}(IAsyncOperation{TResult}, Func{TResult, IEnumerable{IAsyncOperation}})"/>
		public static IAsyncOperation ThenAny(this IAsyncOperation op, Func<IEnumerable<IAsyncOperation>> successCallback)
		{
			if (successCallback == null)
			{
				throw new ArgumentNullException(nameof(successCallback));
			}

			return new ThenAnyResult<VoidResult, VoidResult>(op, successCallback, null);
		}

		/// <summary>
		/// Schedules a callback to be executed after the operation has been resolved. The resulting operation will complete after any of the operations in the callback return value have completed.
		/// </summary>
		/// <param name="op">An operation to be continued.</param>
		/// <param name="successCallback">The callback to be executed when the operation has completed.</param>
		/// <returns>Returns a continuation operation that completes after both source operation and the callback has completed.</returns>
		/// <seealso cref="ThenAny(IAsyncOperation, Func{IEnumerable{IAsyncOperation}})"/>
		public static IAsyncOperation ThenAny<TResult>(this IAsyncOperation<TResult> op, Func<TResult, IEnumerable<IAsyncOperation>> successCallback)
		{
			if (successCallback == null)
			{
				throw new ArgumentNullException(nameof(successCallback));
			}

			return new ThenAnyResult<TResult, VoidResult>(op, successCallback, null);
		}

		/// <summary>
		/// Schedules a callback to be executed after the operation has been resolved. The resulting operation will complete after any of the operations in the callback return value have completed.
		/// </summary>
		/// <param name="op">An operation to be continued.</param>
		/// <param name="successCallback">The callback to be executed when the operation has completed.</param>
		/// <returns>Returns a continuation operation that completes after both source operation and the callback has completed.</returns>
		/// <seealso cref="ThenAny{T, TResult}(IAsyncOperation{T}, Func{T, IEnumerable{IAsyncOperation{TResult}}})"/>
		public static IAsyncOperation<TResult> ThenAny<TResult>(this IAsyncOperation op, Func<IEnumerable<IAsyncOperation<TResult>>> successCallback)
		{
			if (successCallback == null)
			{
				throw new ArgumentNullException(nameof(successCallback));
			}

			return new ThenAnyResult<VoidResult, TResult>(op, successCallback, null);
		}

		/// <summary>
		/// Schedules a callback to be executed after the operation has been resolved. The resulting operation will complete after any of the operations in the callback return value have completed.
		/// </summary>
		/// <param name="op">An operation to be continued.</param>
		/// <param name="successCallback">The callback to be executed when the operation has completed.</param>
		/// <returns>Returns a continuation operation that completes after both source operation and the callback has completed.</returns>
		/// <seealso cref="ThenAny{TResult}(IAsyncOperation, Func{IEnumerable{IAsyncOperation{TResult}}})"/>
		public static IAsyncOperation<TResult> ThenAny<T, TResult>(this IAsyncOperation<T> op, Func<T, IEnumerable<IAsyncOperation<TResult>>> successCallback)
		{
			if (successCallback == null)
			{
				throw new ArgumentNullException(nameof(successCallback));
			}

			return new ThenAnyResult<T, TResult>(op, successCallback, null);
		}

		#endregion

		#region Rebind

		/// <summary>
		/// Schedules a callback to be executed after the operation has been resolved.
		/// </summary>
		/// <param name="op">An operation to be continued.</param>
		/// <param name="successCallback">The callback to be executed when the operation has completed.</param>
		/// <returns>Returns a continuation operation that completes after both source operation and the callback has completed.</returns>
		/// <seealso cref="Rebind{TResult, TNewResult}(IAsyncOperation{TResult}, Func{TResult, TNewResult})"/>
		public static IAsyncOperation<TResult> Rebind<TResult>(this IAsyncOperation op, Func<TResult> successCallback)
		{
			if (successCallback == null)
			{
				throw new ArgumentNullException(nameof(successCallback));
			}

			return new RebindResult<VoidResult, TResult>(op, successCallback);
		}

		/// <summary>
		/// Schedules a callback to be executed after the operation has been resolved.
		/// </summary>
		/// <param name="op">An operation to be continued.</param>
		/// <param name="successCallback">The callback to be executed when the operation has completed.</param>
		/// <returns>Returns a continuation operation that completes after both source operation and the callback has completed.</returns>
		/// <seealso cref="Rebind{TResult}(IAsyncOperation, Func{TResult})"/>
		public static IAsyncOperation<TNewResult> Rebind<TResult, TNewResult>(this IAsyncOperation<TResult> op, Func<TResult, TNewResult> successCallback)
		{
			if (successCallback == null)
			{
				throw new ArgumentNullException(nameof(successCallback));
			}

			return new RebindResult<TResult, TNewResult>(op, successCallback);
		}

		#endregion

		#region Catch

		/// <summary>
		/// Schedules a callback to be executed after the operation has been rejected.
		/// </summary>
		/// <param name="op">An operation to be continued.</param>
		/// <param name="errorCallback">The callback to be executed when the operation has faulted/was canceled.</param>
		/// <returns>Returns a continuation operation that completes after both source operation and the callback has completed.</returns>
		/// <seealso cref="Catch{TException}(IAsyncOperation, Action{TException})"/>
		/// <seealso href="https://promisesaplus.com/"/>
		public static IAsyncOperation Catch(this IAsyncOperation op, Action<Exception> errorCallback)
		{
			if (errorCallback == null)
			{
				throw new ArgumentNullException(nameof(errorCallback));
			}

			return new CatchResult<VoidResult, Exception>(op, errorCallback);
		}

		/// <summary>
		/// Schedules a callback to be executed after the operation has been rejected.
		/// </summary>
		/// <param name="op">An operation to be continued.</param>
		/// <param name="errorCallback">The callback to be executed when the operation has faulted/was canceled.</param>
		/// <returns>Returns a continuation operation that completes after both source operation and the callback has completed.</returns>
		/// <seealso cref="Catch(IAsyncOperation, Action{Exception})"/>
		/// <seealso href="https://promisesaplus.com/"/>
		public static IAsyncOperation Catch<TException>(this IAsyncOperation op, Action<TException> errorCallback) where TException : Exception
		{
			if (errorCallback == null)
			{
				throw new ArgumentNullException(nameof(errorCallback));
			}

			return new CatchResult<VoidResult, TException>(op, errorCallback);
		}

		#endregion

		#region ContinueWith

		/// <summary>
		/// Schedules a callback to be executed after the operation has completed.
		/// </summary>
		/// <param name="op">The operation to continue.</param>
		/// <param name="action">An action to run when the <paramref name="op"/> completes.</param>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="action"/> is <see langword="null"/>.</exception>
		/// <returns>An operation that is executed after <paramref name="op"/> completes.</returns>
		/// <seealso cref="ContinueWith{TResult}(IAsyncOperation, Func{IAsyncOperation{TResult}})"/>
		public static IAsyncOperation ContinueWith(this IAsyncOperation op, Func<IAsyncOperation> action)
		{
			if (action == null)
			{
				throw new ArgumentNullException(nameof(action));
			}

			return new FinallyResult<VoidResult>(op, action);
		}

		/// <summary>
		/// Schedules a callback to be executed after the operation has completed.
		/// </summary>
		/// <param name="op">The operation to continue.</param>
		/// <param name="action">An action to run when the <paramref name="op"/> completes.</param>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="action"/> is <see langword="null"/>.</exception>
		/// <returns>An operation that is executed after <paramref name="op"/> completes.</returns>
		/// <seealso cref="ContinueWith(IAsyncOperation, Func{IAsyncOperation})"/>
		public static IAsyncOperation<TResult> ContinueWith<TResult>(this IAsyncOperation op, Func<IAsyncOperation<TResult>> action)
		{
			if (action == null)
			{
				throw new ArgumentNullException(nameof(action));
			}

			return new FinallyResult<TResult>(op, action);
		}

		#endregion

		#region Finally

		/// <summary>
		/// Schedules a callback to be executed after the operation has completed.
		/// </summary>
		/// <param name="op">An operation to be continued.</param>
		/// <param name="action">The callback to be executed when the operation has completed.</param>
		/// <returns>Returns a continuation operation that completes after both source operation and the callback has completed.</returns>
		/// <seealso href="https://promisesaplus.com/"/>
		public static IAsyncOperation Finally(this IAsyncOperation op, Action action)
		{
			if (action == null)
			{
				throw new ArgumentNullException(nameof(action));
			}

			return new FinallyResult<VoidResult>(op, action);
		}

		#endregion
	}
}
