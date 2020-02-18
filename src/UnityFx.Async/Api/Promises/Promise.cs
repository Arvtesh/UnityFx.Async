// Copyright (c) 2018-2020 Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;

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
		/// Creates a promise that's already been rejected.
		/// </summary>
		/// <seealso cref="Rejected(string)"/>
		/// <seealso cref="Rejected(Exception)"/>
		/// <seealso cref="Resolved"/>
		public static IAsyncOperation Rejected()
		{
			return AsyncResult.CanceledOperation;
		}

		/// <summary>
		/// Creates a promise that's already been rejected with the specified error <paramref name="message"/>.
		/// </summary>
		/// <seealso cref="Rejected(Exception)"/>
		/// <seealso cref="Rejected()"/>
		/// <seealso cref="Resolved"/>
		public static IAsyncOperation Rejected(string message)
		{
			return AsyncResult.FromException(message);
		}

		/// <summary>
		/// Creates a promise that's already been rejected with the specified <see cref="Exception"/>.
		/// </summary>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="e"/> is <see langword="null"/>.</exception>
		/// <seealso cref="Rejected(string)"/>
		/// <seealso cref="Rejected()"/>
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
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="ops"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException">Thrown if the <paramref name="ops"/> collection contained a <see langword="null"/> operation.</exception>
		/// <seealso cref="All(IEnumerable{IAsyncOperation})"/>
		public static IAsyncOperation All(params IAsyncOperation[] ops)
		{
			return AsyncResult.WhenAll(ops);
		}

		/// <summary>
		/// Returns a promise that resolves when all of the promises in the enumerable argument have resolved.
		/// </summary>
		/// <param name="ops">Operations to wait for.</param>
		/// <returns>An operation that completes when all specified promises are resolved.</returns>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="ops"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException">Thrown if the <paramref name="ops"/> collection contained a <see langword="null"/> operation.</exception>
		/// <seealso cref="All(IAsyncOperation[])"/>
		public static IAsyncOperation All(IEnumerable<IAsyncOperation> ops)
		{
			return AsyncResult.WhenAll(ops);
		}

		/// <summary>
		/// Returns a promise that resolves when any of the promises in the enumerable argument have resolved.
		/// </summary>
		/// <param name="ops">Operations to wait for.</param>
		/// <returns>An operation that completes when any of the specified promises is resolved.</returns>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="ops"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException">Thrown if the <paramref name="ops"/> collection contained a <see langword="null"/> operation.</exception>
		/// <seealso cref="Race(IEnumerable{IAsyncOperation})"/>
		public static IAsyncOperation Race(params IAsyncOperation[] ops)
		{
			return AsyncResult.WhenAny(ops);
		}

		/// <summary>
		/// Returns a promise that resolves when any of the promises in the enumerable argument have resolved.
		/// </summary>
		/// <param name="ops">Operations to wait for.</param>
		/// <returns>An operation that completes when any of the specified promises is resolved.</returns>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="ops"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException">Thrown if the <paramref name="ops"/> collection contained a <see langword="null"/> operation.</exception>
		/// <seealso cref="Race(IAsyncOperation[])"/>
		public static IAsyncOperation Race(IEnumerable<IAsyncOperation> ops)
		{
			return AsyncResult.WhenAny(ops);
		}

		/// <summary>
		/// Chain a number of operations using promises.
		/// </summary>
		/// <param name="ops">Functions each of which starts an async operation and yields a promise.</param>
		/// <returns>An operation that completes when all promises in the sequence are resolved.</returns>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="ops"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException">Thrown if the <paramref name="ops"/> collection contained a <see langword="null"/> operation.</exception>
		/// <seealso cref="Sequence(IEnumerable{Func{IAsyncOperation}})"/>
		public static IAsyncOperation Sequence(params Func<IAsyncOperation>[] ops)
		{
			if (ops == null)
			{
				throw new ArgumentNullException(nameof(ops));
			}

			if (ops.Length == 0)
			{
				return AsyncResult.CompletedOperation;
			}

			for (var i = 0; i < ops.Length; i++)
			{
				if (ops[i] == null)
				{
					throw new ArgumentException(Messages.FormatError_ListElementIsNull(), nameof(ops));
				}
			}

			return new SequenceResult(ops);
		}

		/// <summary>
		/// Chain a number of operations using promises.
		/// </summary>
		/// <param name="ops">Functions each of which starts an async operation and yields a promise.</param>
		/// <returns>An operation that completes when all promises in the sequence are resolved.</returns>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="ops"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException">Thrown if the <paramref name="ops"/> collection contained a <see langword="null"/> operation.</exception>
		/// <seealso cref="Sequence(Func{IAsyncOperation}[])"/>
		public static IAsyncOperation Sequence(IEnumerable<Func<IAsyncOperation>> ops)
		{
			if (ops == null)
			{
				throw new ArgumentNullException(nameof(ops));
			}

			var opList = new List<Func<IAsyncOperation>>();

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
				return AsyncResult.CompletedOperation;
			}

			return new SequenceResult(opList);
		}

		internal static void PropagateUnhandledException(object sender, Exception e)
		{
			UnhandledException?.Invoke(sender, new ExceptionEventArgs(e));
		}

		#endregion
	}
}
