// Copyright (c) 2018-2020 Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace UnityFx.Async.Promises
{
	/// <summary>
	/// Promise extensions for <see cref="IAsyncOperation"/>.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	public static class PromiseExtensions
	{
		#region Then

		/// <summary>
		/// Schedules a callback to be executed after the promise has been resolved.
		/// </summary>
		/// <param name="op">An operation to be continued.</param>
		/// <param name="successCallback">The callback to be executed when the operation has completed.</param>
		/// <returns>Returns a continuation operation that completes after both source operation and the callback has completed.</returns>
		/// <seealso cref="Then{TResult}(IAsyncOperation{TResult}, Action{TResult})"/>
		/// <seealso href="https://developer.mozilla.org/en/docs/Web/JavaScript/Reference/Global_Objects/Promise"/>
		public static IAsyncOperation Then(this IAsyncOperation op, Action successCallback)
		{
			if (successCallback == null)
			{
				throw new ArgumentNullException(nameof(successCallback));
			}

			return new ThenResult<VoidResult, VoidResult>(op, successCallback, null);
		}

		/// <summary>
		/// Schedules a callback to be executed after the promise has been resolved.
		/// </summary>
		/// <param name="op">An operation to be continued.</param>
		/// <param name="successCallback">The callback to be executed when the operation has completed.</param>
		/// <returns>Returns a continuation operation that completes after both source operation and the callback has completed.</returns>
		/// <seealso cref="Then(IAsyncOperation, Action)"/>
		/// <seealso href="https://developer.mozilla.org/en/docs/Web/JavaScript/Reference/Global_Objects/Promise"/>
		public static IAsyncOperation Then<TResult>(this IAsyncOperation<TResult> op, Action<TResult> successCallback)
		{
			if (successCallback == null)
			{
				throw new ArgumentNullException(nameof(successCallback));
			}

			return new ThenResult<TResult, VoidResult>(op, successCallback, null);
		}

		/// <summary>
		/// Schedules a callback to be executed after the promise has been resolved.
		/// </summary>
		/// <param name="op">An operation to be continued.</param>
		/// <param name="successCallback">The callback to be executed when the operation has completed.</param>
		/// <returns>Returns a continuation operation that completes after both source operation and the operation returned by <paramref name="successCallback"/> has completed.</returns>
		/// <seealso cref="Then{TResult}(IAsyncOperation, Func{IAsyncOperation{TResult}})"/>
		/// <seealso cref="Then{TResult}(IAsyncOperation{TResult}, Func{TResult, IAsyncOperation})"/>
		/// <seealso href="https://developer.mozilla.org/en/docs/Web/JavaScript/Reference/Global_Objects/Promise"/>
		public static IAsyncOperation Then(this IAsyncOperation op, Func<IAsyncOperation> successCallback)
		{
			if (successCallback == null)
			{
				throw new ArgumentNullException(nameof(successCallback));
			}

			return new ThenResult<VoidResult, VoidResult>(op, successCallback, null);
		}

		/// <summary>
		/// Schedules a callback to be executed after the promise has been resolved.
		/// </summary>
		/// <param name="op">An operation to be continued.</param>
		/// <param name="successCallback">The callback to be executed when the operation has completed.</param>
		/// <returns>Returns a continuation operation that completes after both source operation and the operation returned by <paramref name="successCallback"/> has completed.</returns>
		/// <seealso cref="Then(IAsyncOperation, Func{IAsyncOperation})"/>
		/// <seealso cref="Then{TResult}(IAsyncOperation, Func{IAsyncOperation{TResult}})"/>
		/// <seealso href="https://developer.mozilla.org/en/docs/Web/JavaScript/Reference/Global_Objects/Promise"/>
		public static IAsyncOperation Then<TResult>(this IAsyncOperation<TResult> op, Func<TResult, IAsyncOperation> successCallback)
		{
			if (successCallback == null)
			{
				throw new ArgumentNullException(nameof(successCallback));
			}

			return new ThenResult<TResult, VoidResult>(op, successCallback, null);
		}

		/// <summary>
		/// Schedules a callback to be executed after the promise has been resolved.
		/// </summary>
		/// <param name="op">An operation to be continued.</param>
		/// <param name="successCallback">The callback to be executed when the operation has completed.</param>
		/// <returns>Returns a continuation operation that completes after both source operation and the operation returned by <paramref name="successCallback"/> has completed.</returns>
		/// <seealso cref="Then(IAsyncOperation, Func{IAsyncOperation})"/>
		/// <seealso cref="Then{TResult}(IAsyncOperation{TResult}, Func{TResult, IAsyncOperation})"/>
		/// <seealso href="https://developer.mozilla.org/en/docs/Web/JavaScript/Reference/Global_Objects/Promise"/>
		public static IAsyncOperation<TResult> Then<TResult>(this IAsyncOperation op, Func<IAsyncOperation<TResult>> successCallback)
		{
			if (successCallback == null)
			{
				throw new ArgumentNullException(nameof(successCallback));
			}

			return new ThenResult<VoidResult, TResult>(op, successCallback, null);
		}

		/// <summary>
		/// Schedules a callback to be executed after the promise has been resolved.
		/// </summary>
		/// <param name="op">An operation to be continued.</param>
		/// <param name="successCallback">The callback to be executed when the operation has completed.</param>
		/// <returns>Returns a continuation operation that completes after both source operation and the operation returned by <paramref name="successCallback"/> has completed.</returns>
		/// <seealso cref="Then(IAsyncOperation, Func{IAsyncOperation})"/>
		/// <seealso cref="Then{TResult}(IAsyncOperation, Func{IAsyncOperation{TResult}})"/>
		/// <seealso cref="Then{TResult}(IAsyncOperation{TResult}, Func{TResult, IAsyncOperation})"/>
		/// <seealso href="https://developer.mozilla.org/en/docs/Web/JavaScript/Reference/Global_Objects/Promise"/>
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
		/// <seealso href="https://developer.mozilla.org/en/docs/Web/JavaScript/Reference/Global_Objects/Promise"/>
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
		/// <seealso href="https://developer.mozilla.org/en/docs/Web/JavaScript/Reference/Global_Objects/Promise"/>
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
		/// Schedules a callbacks to be executed after the operation has been resolved.
		/// </summary>
		/// <param name="op">An operation to be continued.</param>
		/// <param name="successCallback">The callback to be executed when the operation has succeeded.</param>
		/// <param name="errorCallback">The callback to be executed when the operation has faulted/was canceled.</param>
		/// <returns>Returns a continuation operation that completes after both source operation and the operation returned by <paramref name="successCallback"/> has completed.</returns>
		/// <seealso cref="Then{TResult}(IAsyncOperation{TResult}, Func{TResult, IAsyncOperation})"/>
		/// <seealso href="https://developer.mozilla.org/en/docs/Web/JavaScript/Reference/Global_Objects/Promise"/>
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
		/// Schedules a callbacks to be executed after the operation has been resolved.
		/// </summary>
		/// <param name="op">An operation to be continued.</param>
		/// <param name="successCallback">The callback to be executed when the operation has succeeded.</param>
		/// <param name="errorCallback">The callback to be executed when the operation has faulted/was canceled.</param>
		/// <returns>Returns a continuation operation that completes after both source operation and the operation returned by <paramref name="successCallback"/> has completed.</returns>
		/// <seealso cref="Then(IAsyncOperation, Func{IAsyncOperation}, Action{Exception})"/>
		/// <seealso href="https://developer.mozilla.org/en/docs/Web/JavaScript/Reference/Global_Objects/Promise"/>
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
		/// Schedules a callback to be executed after the promise has been resolved. The resulting operation will complete after all of the operations in the callback return value have completed.
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
		/// Schedules a callback to be executed after the promise has been resolved. The resulting operation will complete after all of the operations in the callback return value have completed.
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
		/// Schedules a callback to be executed after the promise has been resolved. The resulting operation will complete after all of the operations in the callback return value have completed.
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
		/// Schedules a callback to be executed after the promise has been resolved. The resulting operation will complete after all of the specified objects in an array have completed.
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
		/// Schedules a callback to be executed after the promise has been resolved. The resulting operation will complete after any of the operations in the callback return value have completed.
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
		/// Schedules a callback to be executed after the promise has been resolved. The resulting operation will complete after any of the operations in the callback return value have completed.
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
		/// Schedules a callback to be executed after the promise has been resolved. The resulting operation will complete after any of the operations in the callback return value have completed.
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
		/// Schedules a callback to be executed after the promise has been resolved. The resulting operation will complete after any of the operations in the callback return value have completed.
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

		#region ThenSequence

		/// <summary>
		/// Schedules a callback to be executed after the promise has been resolved. The resulting operation will complete after all of the operations in the callback have completed.
		/// </summary>
		/// <param name="op">An operation to be continued.</param>
		/// <param name="successCallback">The callback to be executed when the operation has completed.</param>
		/// <returns>Returns a continuation operation that completes after both source operation and the callback has completed.</returns>
		public static IAsyncOperation ThenSequence(this IAsyncOperation op, Func<IEnumerable<Func<IAsyncOperation>>> successCallback)
		{
			if (successCallback == null)
			{
				throw new ArgumentNullException(nameof(successCallback));
			}

			return new ThenSequenceResult<VoidResult>(op, successCallback, null);
		}

		/// <summary>
		/// Schedules a callback to be executed after the promise has been resolved. The resulting operation will complete after all of the operations in the callback have completed.
		/// </summary>
		/// <param name="op">An operation to be continued.</param>
		/// <param name="successCallback">The callback to be executed when the operation has completed.</param>
		/// <returns>Returns a continuation operation that completes after both source operation and the callback has completed.</returns>
		public static IAsyncOperation ThenSequence<TResult>(this IAsyncOperation<TResult> op, Func<IEnumerable<Func<IAsyncOperation>>> successCallback)
		{
			if (successCallback == null)
			{
				throw new ArgumentNullException(nameof(successCallback));
			}

			return new ThenSequenceResult<TResult>(op, successCallback, null);
		}

		/// <summary>
		/// Schedules a callback to be executed after the promise has been resolved. The resulting operation will complete after all of the operations in the callback have completed.
		/// </summary>
		/// <param name="op">An operation to be continued.</param>
		/// <param name="successCallback">The callback to be executed when the operation has completed.</param>
		/// <returns>Returns a continuation operation that completes after both source operation and the callback has completed.</returns>
		public static IAsyncOperation ThenSequence(this IAsyncOperation op, Func<Func<IAsyncOperation>[]> successCallback)
		{
			if (successCallback == null)
			{
				throw new ArgumentNullException(nameof(successCallback));
			}

			return new ThenSequenceResult<VoidResult>(op, successCallback, null);
		}

		/// <summary>
		/// Schedules a callback to be executed after the promise has been resolved. The resulting operation will complete after all of the operations in the callback have completed.
		/// </summary>
		/// <param name="op">An operation to be continued.</param>
		/// <param name="successCallback">The callback to be executed when the operation has completed.</param>
		/// <returns>Returns a continuation operation that completes after both source operation and the callback has completed.</returns>
		public static IAsyncOperation ThenSequence<TResult>(this IAsyncOperation<TResult> op, Func<Func<IAsyncOperation>[]> successCallback)
		{
			if (successCallback == null)
			{
				throw new ArgumentNullException(nameof(successCallback));
			}

			return new ThenSequenceResult<TResult>(op, successCallback, null);
		}

		#endregion

		#region Rebind

		/// <summary>
		/// Transforms the promise result to another type.
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
		/// Transforms the promise result to another type.
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
		/// Schedules a callback to be executed after the promise has been rejected.
		/// </summary>
		/// <param name="op">An operation to be continued.</param>
		/// <param name="errorCallback">The callback to be executed when the operation has faulted/was canceled.</param>
		/// <returns>Returns a continuation operation that completes after both source operation and the callback has completed.</returns>
		/// <seealso cref="Catch{TException}(IAsyncOperation, Action{TException})"/>
		/// <seealso href="https://developer.mozilla.org/en/docs/Web/JavaScript/Reference/Global_Objects/Promise"/>
		public static IAsyncOperation Catch(this IAsyncOperation op, Action<Exception> errorCallback)
		{
			if (errorCallback == null)
			{
				throw new ArgumentNullException(nameof(errorCallback));
			}

			return new CatchResult<VoidResult, Exception>(op, errorCallback);
		}

		/// <summary>
		/// Schedules a callback to be executed after the promise has been rejected.
		/// </summary>
		/// <param name="op">An operation to be continued.</param>
		/// <param name="errorCallback">The callback to be executed when the operation has faulted/was canceled.</param>
		/// <returns>Returns a continuation operation that completes after both source operation and the callback has completed.</returns>
		/// <seealso cref="Catch(IAsyncOperation, Action{Exception})"/>
		/// <seealso href="https://developer.mozilla.org/en/docs/Web/JavaScript/Reference/Global_Objects/Promise"/>
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
		/// Schedules a callback to be executed after the promise has completed.
		/// </summary>
		/// <param name="op">An operation to be continued.</param>
		/// <param name="action">The callback to be executed when the operation has completed.</param>
		/// <returns>Returns a continuation operation that completes after both source operation and the callback has completed.</returns>
		/// <seealso href="https://developer.mozilla.org/en/docs/Web/JavaScript/Reference/Global_Objects/Promise"/>
		public static IAsyncOperation Finally(this IAsyncOperation op, Action action)
		{
			if (action == null)
			{
				throw new ArgumentNullException(nameof(action));
			}

			return new FinallyResult<VoidResult>(op, action);
		}

		#endregion

		#region Done

		/// <summary>
		/// Schedules a callback to be executed after the promise chain has completed. Routes unhendled errors to <see cref="Promise.UnhandledException"/>.
		/// </summary>
		/// <param name="op">An operation to be continued.</param>
		/// <param name="successCallback">The callback to be executed when the promise has resolved.</param>
		/// <seealso cref="Done(IAsyncOperation, Action, Action{Exception})"/>
		/// <seealso cref="Promise.UnhandledException"/>
		/// <seealso href="https://developer.mozilla.org/en/docs/Web/JavaScript/Reference/Global_Objects/Promise"/>
		public static void Done(this IAsyncOperation op, Action successCallback)
		{
			if (successCallback == null)
			{
				throw new ArgumentNullException(nameof(successCallback));
			}

			op.AddCompletionCallback(new DoneResult<VoidResult>(successCallback, null));
		}

		/// <summary>
		/// Schedules a callback to be executed after the promise chain has completed. Routes unhendled errors to <see cref="Promise.UnhandledException"/>.
		/// </summary>
		/// <param name="op">An operation to be continued.</param>
		/// <param name="successCallback">The callback to be executed when the promise has resolved.</param>
		/// <param name="errorCallback">The callback to be executed when the promise was rejected.</param>
		/// <seealso cref="Done(IAsyncOperation, Action)"/>
		/// <seealso cref="Promise.UnhandledException"/>
		/// <seealso href="https://developer.mozilla.org/en/docs/Web/JavaScript/Reference/Global_Objects/Promise"/>
		public static void Done(this IAsyncOperation op, Action successCallback, Action<Exception> errorCallback)
		{
			if (successCallback == null)
			{
				throw new ArgumentNullException(nameof(successCallback));
			}

			if (errorCallback == null)
			{
				throw new ArgumentNullException(nameof(errorCallback));
			}

			op.AddCompletionCallback(new DoneResult<VoidResult>(successCallback, errorCallback));
		}

		/// <summary>
		/// Schedules a callback to be executed after the promise chain has completed. Routes unhendled errors to <see cref="Promise.UnhandledException"/>.
		/// </summary>
		/// <param name="op">An operation to be continued.</param>
		/// <param name="successCallback">The callback to be executed when the promise has resolved.</param>
		/// <seealso cref="Done{TResult}(IAsyncOperation{TResult}, Action{TResult}, Action{Exception})"/>
		/// <seealso cref="Promise.UnhandledException"/>
		/// <seealso href="https://developer.mozilla.org/en/docs/Web/JavaScript/Reference/Global_Objects/Promise"/>
		public static void Done<TResult>(this IAsyncOperation<TResult> op, Action<TResult> successCallback)
		{
			if (successCallback == null)
			{
				throw new ArgumentNullException(nameof(successCallback));
			}

			op.AddCompletionCallback(new DoneResult<TResult>(successCallback, null));
		}

		/// <summary>
		/// Schedules a callback to be executed after the promise chain has completed. Routes unhendled errors to <see cref="Promise.UnhandledException"/>.
		/// </summary>
		/// <param name="op">An operation to be continued.</param>
		/// <param name="successCallback">The callback to be executed when the promise has resolved.</param>
		/// <param name="errorCallback">The callback to be executed when the promise was rejected.</param>
		/// <seealso cref="Done{TResult}(IAsyncOperation{TResult}, Action{TResult})"/>
		/// <seealso cref="Promise.UnhandledException"/>
		/// <seealso href="https://developer.mozilla.org/en/docs/Web/JavaScript/Reference/Global_Objects/Promise"/>
		public static void Done<TResult>(this IAsyncOperation<TResult> op, Action<TResult> successCallback, Action<Exception> errorCallback)
		{
			if (successCallback == null)
			{
				throw new ArgumentNullException(nameof(successCallback));
			}

			if (errorCallback == null)
			{
				throw new ArgumentNullException(nameof(errorCallback));
			}

			op.AddCompletionCallback(new DoneResult<TResult>(successCallback, errorCallback));
		}

		/// <summary>
		/// Routes unhendled errors to <see cref="Promise.UnhandledException"/>.
		/// </summary>
		/// <param name="op">An operation to be continued.</param>
		/// <seealso cref="Promise.UnhandledException"/>
		/// <seealso href="https://developer.mozilla.org/en/docs/Web/JavaScript/Reference/Global_Objects/Promise"/>
		public static void Done(this IAsyncOperation op)
		{
			op.AddCompletionCallback(new DoneResult<VoidResult>(null, null));
		}

		#endregion
	}
}
