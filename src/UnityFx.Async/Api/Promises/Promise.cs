// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;

namespace UnityFx.Async.Promises
{
	/// <summary>
	/// Promise-related helpers.
	/// </summary>
	/// <seealso href="https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Promise"/>
	public static class Promise
	{
		#region interface

		/// <summary>
		/// Event raised for unhandled exceptions. For this to work you have to complete your promises with a call to Done().
		/// </summary>
		public static event EventHandler<ExceptionEventArgs> UnhandledException;

		/// <summary>
		/// Creates a promise that's already been resolved.
		/// </summary>
		/// <seealso cref="Rejected(Exception)"/>
		/// <seealso cref="Resolved{TResult}(TResult)"/>
		public static IAsyncOperation Resolved()
		{
			return AsyncResult.CompletedOperation;
		}

		/// <summary>
		/// Creates a promise that's already been resolved with the specified value.
		/// </summary>
		/// <seealso cref="Rejected(Exception)"/>
		/// <seealso cref="Resolved"/>
		public static IAsyncOperation<TResult> Resolved<TResult>(TResult result)
		{
			return AsyncResult.FromResult(result);
		}

		/// <summary>
		/// Creates a promise that's already been rejected with the specified error <paramref name="message"/>.
		/// </summary>
		/// <seealso cref="Rejected(Exception)"/>
		/// <seealso cref="Resolved"/>
		public static IAsyncOperation Rejected(string message)
		{
			return AsyncResult.FromException(message);
		}

		/// <summary>
		/// Creates a promise that's already been rejected with the specified <see cref="Exception"/>.
		/// </summary>
		/// <seealso cref="Rejected(string)"/>
		/// <seealso cref="Resolved"/>
		public static IAsyncOperation Rejected(Exception e)
		{
			return AsyncResult.FromException(e);
		}

		/// <summary>
		/// Returns a promise that resolves when all of the promises in the enumerable argument have resolved.
		/// </summary>
		/// <param name="ops">Operations to wait for.</param>
		/// <returns>An operation that completes when all specified promises are resolved.</returns>
		/// <seealso cref="All(IEnumerable{IAsyncOperation})"/>
		public static IAsyncResult All(params IAsyncOperation[] ops)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Returns a promise that resolves when all of the promises in the enumerable argument have resolved.
		/// </summary>
		/// <param name="ops">Operations to wait for.</param>
		/// <returns>An operation that completes when all specified promises are resolved.</returns>
		/// <seealso cref="All(IAsyncOperation[])"/>
		public static IAsyncResult All(IEnumerable<IAsyncOperation> ops)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Returns a promise that resolves when any of the promises in the enumerable argument have resolved.
		/// </summary>
		/// <param name="ops">Operations to wait for.</param>
		/// <returns>An operation that completes when any of the specified promises is resolved.</returns>
		/// <seealso cref="Race(IEnumerable{IAsyncOperation})"/>
		public static IAsyncResult Race(params IAsyncOperation[] ops)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Returns a promise that resolves when any of the promises in the enumerable argument have resolved.
		/// </summary>
		/// <param name="ops">Operations to wait for.</param>
		/// <returns>An operation that completes when any of the specified promises is resolved.</returns>
		/// <seealso cref="Race(IAsyncOperation[])"/>
		public static IAsyncResult Race(IEnumerable<IAsyncOperation> ops)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Chain a number of operations using promises.
		/// </summary>
		/// <param name="opFuncs">Functions each of which starts an async operation and yields a promise.</param>
		/// <returns>An operation that completes when all promises in the sequence are resolved.</returns>
		/// <seealso cref="Sequence(IEnumerable{Func{IAsyncOperation}})"/>
		public static IAsyncResult Sequence(params Func<IAsyncOperation>[] opFuncs)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Chain a number of operations using promises.
		/// </summary>
		/// <param name="opFuncs">Functions each of which starts an async operation and yields a promise.</param>
		/// <returns>An operation that completes when all promises in the sequence are resolved.</returns>
		/// <seealso cref="Sequence(Func{IAsyncOperation}[])"/>
		public static IAsyncResult Sequence(IEnumerable<Func<IAsyncOperation>> opFuncs)
		{
			throw new NotImplementedException();
		}

		internal static void PropagateUnhandledException(object sender, Exception e)
		{
			UnhandledException?.Invoke(sender, new ExceptionEventArgs(e));
		}

		#endregion
	}
}
