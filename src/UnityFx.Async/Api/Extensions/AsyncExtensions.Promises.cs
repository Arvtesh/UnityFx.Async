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
		/// Schedules a callback to be executed after the operation has succeeded.
		/// </summary>
		/// <param name="op">An operation to be continued.</param>
		/// <param name="successCallback">The callback to be executed when the operation has completed.</param>
		/// <returns>Returns a continuation operation that completes after both source operation and the callback has completed.</returns>
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
		/// Schedules a callback to be executed after the operation has succeeded.
		/// </summary>
		/// <param name="op">An operation to be continued.</param>
		/// <param name="successCallback">The callback to be executed when the operation has completed.</param>
		/// <returns>Returns a continuation operation that completes after both source operation and the callback has completed.</returns>
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
		/// Schedules a callback to be executed after the operation has succeeded.
		/// </summary>
		/// <param name="op">An operation to be continued.</param>
		/// <param name="successCallback">The callback to be executed when the operation has completed.</param>
		/// <returns>Returns a continuation operation that completes after both source operation and the operation returned by <paramref name="successCallback"/> has completed.</returns>
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
		/// Schedules a callback to be executed after the operation has succeeded.
		/// </summary>
		/// <param name="op">An operation to be continued.</param>
		/// <param name="successCallback">The callback to be executed when the operation has completed.</param>
		/// <returns>Returns a continuation operation that completes after both source operation and the operation returned by <paramref name="successCallback"/> has completed.</returns>
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
		/// Schedules a callback to be executed after the operation has succeeded.
		/// </summary>
		/// <param name="op">An operation to be continued.</param>
		/// <param name="successCallback">The callback to be executed when the operation has completed.</param>
		/// <returns>Returns a continuation operation that completes after both source operation and the operation returned by <paramref name="successCallback"/> has completed.</returns>
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
		/// Schedules a callback to be executed after the operation has succeeded.
		/// </summary>
		/// <param name="op">An operation to be continued.</param>
		/// <param name="successCallback">The callback to be executed when the operation has completed.</param>
		/// <returns>Returns a continuation operation that completes after both source operation and the operation returned by <paramref name="successCallback"/> has completed.</returns>
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
		/// Schedules a callbacks to be executed after the operation has completed.
		/// </summary>
		/// <param name="op">An operation to be continued.</param>
		/// <param name="successCallback">The callback to be executed when the operation has succeeded.</param>
		/// <param name="errorCallback">The callback to be executed when the operation has faulted/was canceled.</param>
		/// <returns>Returns a continuation operation that completes after both source operation and the callback has completed.</returns>
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
		/// Schedules a callbacks to be executed after the operation has completed.
		/// </summary>
		/// <param name="op">An operation to be continued.</param>
		/// <param name="successCallback">The callback to be executed when the operation has succeeded.</param>
		/// <param name="errorCallback">The callback to be executed when the operation has faulted/was canceled.</param>
		/// <returns>Returns a continuation operation that completes after both source operation and the callback has completed.</returns>
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
		/// Adds a completion callback to be executed after the operation has succeeded.
		/// </summary>
		/// <param name="op">An operation to be continued.</param>
		/// <param name="successCallback">The callback to be executed when the operation has succeeded.</param>
		/// <param name="errorCallback">The callback to be executed when the operation has faulted/was canceled.</param>
		/// <returns>Returns a continuation operation that completes after both source operation and the operation returned by <paramref name="successCallback"/> has completed.</returns>
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
		/// Adds a completion callback to be executed after the operation has succeeded.
		/// </summary>
		/// <param name="op">An operation to be continued.</param>
		/// <param name="successCallback">The callback to be executed when the operation has succeeded.</param>
		/// <param name="errorCallback">The callback to be executed when the operation has faulted/was canceled.</param>
		/// <returns>Returns a continuation operation that completes after both source operation and the operation returned by <paramref name="successCallback"/> has completed.</returns>
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
		/// Schedules a callback to be executed after the operation has succeeded.
		/// </summary>
		/// <param name="op">An operation to be continued.</param>
		/// <param name="successCallback">The callback to be executed when the operation has completed.</param>
		/// <returns>Returns a continuation operation that completes after both source operation and the callback has completed.</returns>
		public static IAsyncOperation ThenAll(this IAsyncOperation op, Func<IEnumerable<IAsyncOperation>> successCallback)
		{
			if (successCallback == null)
			{
				throw new ArgumentNullException(nameof(successCallback));
			}

			throw new NotImplementedException();
		}

		/// <summary>
		/// Schedules a callback to be executed after the operation has succeeded.
		/// </summary>
		/// <param name="op">An operation to be continued.</param>
		/// <param name="successCallback">The callback to be executed when the operation has completed.</param>
		/// <returns>Returns a continuation operation that completes after both source operation and the callback has completed.</returns>
		public static IAsyncOperation ThenAll<T>(this IAsyncOperation<T> op, Func<T, IEnumerable<IAsyncOperation>> successCallback)
		{
			if (successCallback == null)
			{
				throw new ArgumentNullException(nameof(successCallback));
			}

			throw new NotImplementedException();
		}

		/// <summary>
		/// Schedules a callback to be executed after the operation has succeeded.
		/// </summary>
		/// <param name="op">An operation to be continued.</param>
		/// <param name="successCallback">The callback to be executed when the operation has completed.</param>
		/// <returns>Returns a continuation operation that completes after both source operation and the callback has completed.</returns>
		public static IAsyncOperation<T[]> ThenAll<T>(this IAsyncOperation op, Func<IEnumerable<IAsyncOperation<T>>> successCallback)
		{
			if (successCallback == null)
			{
				throw new ArgumentNullException(nameof(successCallback));
			}

			throw new NotImplementedException();
		}

		/// <summary>
		/// Schedules a callback to be executed after the operation has succeeded.
		/// </summary>
		/// <param name="op">An operation to be continued.</param>
		/// <param name="successCallback">The callback to be executed when the operation has completed.</param>
		/// <returns>Returns a continuation operation that completes after both source operation and the callback has completed.</returns>
		public static IAsyncOperation<U[]> ThenAll<T, U>(this IAsyncOperation<T> op, Func<T, IEnumerable<IAsyncOperation<U>>> successCallback)
		{
			if (successCallback == null)
			{
				throw new ArgumentNullException(nameof(successCallback));
			}

			throw new NotImplementedException();
		}

		#endregion

		#region ThenAny

		/// <summary>
		/// Schedules a callback to be executed after the operation has succeeded.
		/// </summary>
		/// <param name="op">An operation to be continued.</param>
		/// <param name="successCallback">The callback to be executed when the operation has completed.</param>
		/// <returns>Returns a continuation operation that completes after both source operation and the callback has completed.</returns>
		public static IAsyncOperation ThenAny(this IAsyncOperation op, Func<IEnumerable<IAsyncOperation>> successCallback)
		{
			if (successCallback == null)
			{
				throw new ArgumentNullException(nameof(successCallback));
			}

			throw new NotImplementedException();
		}

		/// <summary>
		/// Schedules a callback to be executed after the operation has succeeded.
		/// </summary>
		/// <param name="op">An operation to be continued.</param>
		/// <param name="successCallback">The callback to be executed when the operation has completed.</param>
		/// <returns>Returns a continuation operation that completes after both source operation and the callback has completed.</returns>
		public static IAsyncOperation ThenAny<T>(this IAsyncOperation<T> op, Func<T, IEnumerable<IAsyncOperation>> successCallback)
		{
			if (successCallback == null)
			{
				throw new ArgumentNullException(nameof(successCallback));
			}

			throw new NotImplementedException();
		}

		/// <summary>
		/// Schedules a callback to be executed after the operation has succeeded.
		/// </summary>
		/// <param name="op">An operation to be continued.</param>
		/// <param name="successCallback">The callback to be executed when the operation has completed.</param>
		/// <returns>Returns a continuation operation that completes after both source operation and the callback has completed.</returns>
		public static IAsyncOperation<T> ThenAny<T>(this IAsyncOperation op, Func<IEnumerable<IAsyncOperation<T>>> successCallback)
		{
			if (successCallback == null)
			{
				throw new ArgumentNullException(nameof(successCallback));
			}

			throw new NotImplementedException();
		}

		/// <summary>
		/// Schedules a callback to be executed after the operation has succeeded.
		/// </summary>
		/// <param name="op">An operation to be continued.</param>
		/// <param name="successCallback">The callback to be executed when the operation has completed.</param>
		/// <returns>Returns a continuation operation that completes after both source operation and the callback has completed.</returns>
		public static IAsyncOperation<U> ThenAny<T, U>(this IAsyncOperation<T> op, Func<T, IEnumerable<IAsyncOperation<U>>> successCallback)
		{
			if (successCallback == null)
			{
				throw new ArgumentNullException(nameof(successCallback));
			}

			throw new NotImplementedException();
		}

		#endregion

		#region Rebind

		/// <summary>
		/// Schedules a callback to be executed after the operation has succeeded.
		/// </summary>
		/// <param name="op">An operation to be continued.</param>
		/// <param name="successCallback">The callback to be executed when the operation has completed.</param>
		/// <returns>Returns a continuation operation that completes after both source operation and the callback has completed.</returns>
		public static IAsyncOperation<TNewResult> Rebind<TNewResult>(this IAsyncOperation op, Func<TNewResult> successCallback)
		{
			if (successCallback == null)
			{
				throw new ArgumentNullException(nameof(successCallback));
			}

			return new RebindResult<VoidResult, TNewResult>(op, successCallback);
		}

		/// <summary>
		/// Schedules a callback to be executed after the operation has succeeded.
		/// </summary>
		/// <param name="op">An operation to be continued.</param>
		/// <param name="successCallback">The callback to be executed when the operation has completed.</param>
		/// <returns>Returns a continuation operation that completes after both source operation and the callback has completed.</returns>
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
		/// Adds a completion callback to be executed after the operation has faulted or was canceled.
		/// </summary>
		/// <param name="op">An operation to be continued.</param>
		/// <param name="errorCallback">The callback to be executed when the operation has faulted/was canceled.</param>
		/// <returns>Returns a continuation operation that completes after both source operation and the callback has completed.</returns>
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
		/// Adds a completion callback to be executed after the operation has faulted or was canceled.
		/// </summary>
		/// <param name="op">An operation to be continued.</param>
		/// <param name="errorCallback">The callback to be executed when the operation has faulted/was canceled.</param>
		/// <returns>Returns a continuation operation that completes after both source operation and the callback has completed.</returns>
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

		#region Finally

		/// <summary>
		/// Adds a completion callback to be executed after the operation has completed.
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
