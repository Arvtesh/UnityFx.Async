// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Threading;

namespace UnityFx.Async
{
	partial class AsyncExtensions
	{
		#region ContinueWith

		/// <summary>
		/// Creates a continuation that executes when the target <see cref="IAsyncOperation"/> completes.
		/// </summary>
		/// <param name="op">The operation to continue.</param>
		/// <param name="action">An action to run when the <paramref name="op"/> completes.</param>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="action"/> is <see langword="null"/>.</exception>
		/// <returns>An operation that is executed after <paramref name="op"/> completes.</returns>
		/// <seealso cref="ContinueWith(IAsyncOperation, Action{IAsyncOperation}, AsyncContinuationOptions)"/>
		public static IAsyncOperation ContinueWith(this IAsyncOperation op, Action<IAsyncOperation> action)
		{
			return ContinueWith(op, action, AsyncContinuationOptions.None);
		}

		/// <summary>
		/// Creates a continuation that executes when the target <see cref="IAsyncOperation"/> completes.
		/// </summary>
		/// <param name="op">The operation to continue.</param>
		/// <param name="action">An action to run when the <paramref name="op"/> completes.</param>
		/// <param name="options">Options for when the <paramref name="action"/> is executed.</param>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="action"/> is <see langword="null"/>.</exception>
		/// <returns>An operation that is executed after <paramref name="op"/> completes.</returns>
		/// <seealso cref="ContinueWith(IAsyncOperation, Action{IAsyncOperation})"/>
		public static IAsyncOperation ContinueWith(this IAsyncOperation op, Action<IAsyncOperation> action, AsyncContinuationOptions options)
		{
			if (action == null)
			{
				throw new ArgumentNullException(nameof(action));
			}

			return new ContinueWithResult<VoidResult, VoidResult>(op, options, action, null);
		}

		/// <summary>
		/// Creates a continuation that executes when the target <see cref="IAsyncOperation"/> completes.
		/// </summary>
		/// <param name="op">The operation to continue.</param>
		/// <param name="action">An action to run when the <paramref name="op"/> completes.</param>
		/// <param name="userState">A user-defined state object that is passed as second argument to <paramref name="action"/>.</param>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="action"/> is <see langword="null"/>.</exception>
		/// <returns>An operation that is executed after <paramref name="op"/> completes.</returns>
		/// <seealso cref="ContinueWith(IAsyncOperation, Action{IAsyncOperation, object}, object, AsyncContinuationOptions)"/>
		public static IAsyncOperation ContinueWith(this IAsyncOperation op, Action<IAsyncOperation, object> action, object userState)
		{
			return ContinueWith(op, action, userState, AsyncContinuationOptions.None);
		}

		/// <summary>
		/// Creates a continuation that executes when the target <see cref="IAsyncOperation"/> completes.
		/// </summary>
		/// <param name="op">The operation to continue.</param>
		/// <param name="action">An action to run when the <paramref name="op"/> completes.</param>
		/// <param name="userState">A user-defined state object that is passed as second argument to <paramref name="action"/>.</param>
		/// <param name="options">Options for when the <paramref name="action"/> is executed.</param>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="action"/> is <see langword="null"/>.</exception>
		/// <returns>An operation that is executed after <paramref name="op"/> completes.</returns>
		/// <seealso cref="ContinueWith(IAsyncOperation, Action{IAsyncOperation, object}, object)"/>
		public static IAsyncOperation ContinueWith(this IAsyncOperation op, Action<IAsyncOperation, object> action, object userState, AsyncContinuationOptions options)
		{
			if (action == null)
			{
				throw new ArgumentNullException(nameof(action));
			}

			return new ContinueWithResult<VoidResult, VoidResult>(op, options, action, userState);
		}

		/// <summary>
		/// Creates a continuation that executes when the target <see cref="IAsyncOperation"/> completes.
		/// </summary>
		/// <param name="op">The operation to continue.</param>
		/// <param name="action">An action to run when the <paramref name="op"/> completes.</param>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="action"/> is <see langword="null"/>.</exception>
		/// <returns>An operation that is executed after <paramref name="op"/> completes.</returns>
		/// <seealso cref="ContinueWith{TResult}(IAsyncOperation, Func{IAsyncOperation, TResult}, AsyncContinuationOptions)"/>
		public static IAsyncOperation<TResult> ContinueWith<TResult>(this IAsyncOperation op, Func<IAsyncOperation, TResult> action)
		{
			return ContinueWith(op, action, AsyncContinuationOptions.None);
		}

		/// <summary>
		/// Creates a continuation that executes when the target <see cref="IAsyncOperation"/> completes.
		/// </summary>
		/// <param name="op">The operation to continue.</param>
		/// <param name="action">An action to run when the <paramref name="op"/> completes.</param>
		/// <param name="options">Options for when the <paramref name="action"/> is executed.</param>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="action"/> is <see langword="null"/>.</exception>
		/// <returns>An operation that is executed after <paramref name="op"/> completes.</returns>
		/// <seealso cref="ContinueWith{TResult}(IAsyncOperation, Func{IAsyncOperation, TResult})"/>
		public static IAsyncOperation<TResult> ContinueWith<TResult>(this IAsyncOperation op, Func<IAsyncOperation, TResult> action, AsyncContinuationOptions options)
		{
			if (action == null)
			{
				throw new ArgumentNullException(nameof(action));
			}

			return new ContinueWithResult<VoidResult, TResult>(op, options, action, null);
		}

		/// <summary>
		/// Creates a continuation that executes when the target <see cref="IAsyncOperation"/> completes.
		/// </summary>
		/// <param name="op">The operation to continue.</param>
		/// <param name="action">An action to run when the <paramref name="op"/> completes.</param>
		/// <param name="userState">A user-defined state object that is passed as second argument to <paramref name="action"/>.</param>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="action"/> is <see langword="null"/>.</exception>
		/// <returns>An operation that is executed after <paramref name="op"/> completes.</returns>
		/// <seealso cref="ContinueWith{TResult}(IAsyncOperation, Func{IAsyncOperation, object, TResult}, object, AsyncContinuationOptions)"/>
		public static IAsyncOperation<TResult> ContinueWith<TResult>(this IAsyncOperation op, Func<IAsyncOperation, object, TResult> action, object userState)
		{
			return ContinueWith(op, action, userState, AsyncContinuationOptions.None);
		}

		/// <summary>
		/// Creates a continuation that executes when the target <see cref="IAsyncOperation"/> completes.
		/// </summary>
		/// <param name="op">The operation to continue.</param>
		/// <param name="action">An action to run when the <paramref name="op"/> completes.</param>
		/// <param name="userState">A user-defined state object that is passed as second argument to <paramref name="action"/>.</param>
		/// <param name="options">Options for when the <paramref name="action"/> is executed.</param>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="action"/> is <see langword="null"/>.</exception>
		/// <returns>An operation that is executed after <paramref name="op"/> completes.</returns>
		/// <seealso cref="ContinueWith{TResult}(IAsyncOperation, Func{IAsyncOperation, object, TResult}, object)"/>
		public static IAsyncOperation<TResult> ContinueWith<TResult>(this IAsyncOperation op, Func<IAsyncOperation, object, TResult> action, object userState, AsyncContinuationOptions options)
		{
			if (action == null)
			{
				throw new ArgumentNullException(nameof(action));
			}

			return new ContinueWithResult<VoidResult, TResult>(op, options, action, userState);
		}

		/// <summary>
		/// Creates a continuation that executes when the target <see cref="IAsyncOperation{TResult}"/> completes.
		/// </summary>
		/// <param name="op">The operation to continue.</param>
		/// <param name="action">An action to run when the <paramref name="op"/> completes.</param>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="action"/> is <see langword="null"/>.</exception>
		/// <returns>An operation that is executed after <paramref name="op"/> completes.</returns>
		/// <seealso cref="ContinueWith{TResult}(IAsyncOperation{TResult}, Action{IAsyncOperation{TResult}}, AsyncContinuationOptions)"/>
		public static IAsyncOperation ContinueWith<TResult>(this IAsyncOperation<TResult> op, Action<IAsyncOperation<TResult>> action)
		{
			return ContinueWith(op, action, AsyncContinuationOptions.None);
		}

		/// <summary>
		/// Creates a continuation that executes when the target <see cref="IAsyncOperation{TResult}"/> completes.
		/// </summary>
		/// <param name="op">The operation to continue.</param>
		/// <param name="action">An action to run when the <paramref name="op"/> completes.</param>
		/// <param name="options">Options for when the <paramref name="action"/> is executed.</param>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="action"/> is <see langword="null"/>.</exception>
		/// <returns>An operation that is executed after <paramref name="op"/> completes.</returns>
		/// <seealso cref="ContinueWith{TResult}(IAsyncOperation{TResult}, Action{IAsyncOperation{TResult}})"/>
		public static IAsyncOperation ContinueWith<TResult>(this IAsyncOperation<TResult> op, Action<IAsyncOperation<TResult>> action, AsyncContinuationOptions options)
		{
			if (action == null)
			{
				throw new ArgumentNullException(nameof(action));
			}

			return new ContinueWithResult<TResult, VoidResult>(op, options, action, null);
		}

		/// <summary>
		/// Creates a continuation that executes when the target <see cref="IAsyncOperation{TResult}"/> completes.
		/// </summary>
		/// <param name="op">The operation to continue.</param>
		/// <param name="action">An action to run when the <paramref name="op"/> completes.</param>
		/// <param name="userState">A user-defined state object that is passed as second argument to <paramref name="action"/>.</param>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="action"/> is <see langword="null"/>.</exception>
		/// <returns>An operation that is executed after <paramref name="op"/> completes.</returns>
		/// <seealso cref="ContinueWith{TResult}(IAsyncOperation{TResult}, Action{IAsyncOperation{TResult}, object}, object, AsyncContinuationOptions)"/>
		public static IAsyncOperation ContinueWith<TResult>(this IAsyncOperation<TResult> op, Action<IAsyncOperation<TResult>, object> action, object userState)
		{
			return ContinueWith(op, action, userState, AsyncContinuationOptions.None);
		}

		/// <summary>
		/// Creates a continuation that executes when the target <see cref="IAsyncOperation{TResult}"/> completes.
		/// </summary>
		/// <param name="op">The operation to continue.</param>
		/// <param name="action">An action to run when the <paramref name="op"/> completes.</param>
		/// <param name="userState">A user-defined state object that is passed as second argument to <paramref name="action"/>.</param>
		/// <param name="options">Options for when the <paramref name="action"/> is executed.</param>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="action"/> is <see langword="null"/>.</exception>
		/// <returns>An operation that is executed after <paramref name="op"/> completes.</returns>
		/// <seealso cref="ContinueWith{TResult}(IAsyncOperation{TResult}, Action{IAsyncOperation{TResult}, object}, object)"/>
		public static IAsyncOperation ContinueWith<TResult>(this IAsyncOperation<TResult> op, Action<IAsyncOperation<TResult>, object> action, object userState, AsyncContinuationOptions options)
		{
			if (action == null)
			{
				throw new ArgumentNullException(nameof(action));
			}

			return new ContinueWithResult<TResult, VoidResult>(op, options, action, userState);
		}

		/// <summary>
		/// Creates a continuation that executes when the target <see cref="IAsyncOperation"/> completes.
		/// </summary>
		/// <param name="op">The operation to continue.</param>
		/// <param name="action">An action to run when the <paramref name="op"/> completes.</param>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="action"/> is <see langword="null"/>.</exception>
		/// <returns>An operation that is executed after <paramref name="op"/> completes.</returns>
		/// <seealso cref="ContinueWith{TResult, TNewResult}(IAsyncOperation{TResult}, Func{IAsyncOperation{TResult}, TNewResult}, AsyncContinuationOptions)"/>
		public static IAsyncOperation<TNewResult> ContinueWith<TResult, TNewResult>(this IAsyncOperation<TResult> op, Func<IAsyncOperation<TResult>, TNewResult> action)
		{
			return ContinueWith(op, action, AsyncContinuationOptions.None);
		}

		/// <summary>
		/// Creates a continuation that executes when the target <see cref="IAsyncOperation"/> completes.
		/// </summary>
		/// <param name="op">The operation to continue.</param>
		/// <param name="action">An action to run when the <paramref name="op"/> completes.</param>
		/// <param name="options">Options for when the <paramref name="action"/> is executed.</param>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="action"/> is <see langword="null"/>.</exception>
		/// <returns>An operation that is executed after <paramref name="op"/> completes.</returns>
		/// <seealso cref="ContinueWith{TResult, TNewResult}(IAsyncOperation{TResult}, Func{IAsyncOperation{TResult}, TNewResult})"/>
		public static IAsyncOperation<TNewResult> ContinueWith<TResult, TNewResult>(this IAsyncOperation<TResult> op, Func<IAsyncOperation<TResult>, TNewResult> action, AsyncContinuationOptions options)
		{
			if (action == null)
			{
				throw new ArgumentNullException(nameof(action));
			}

			return new ContinueWithResult<TResult, TNewResult>(op, options, action, null);
		}

		/// <summary>
		/// Creates a continuation that executes when the target <see cref="IAsyncOperation"/> completes.
		/// </summary>
		/// <param name="op">The operation to continue.</param>
		/// <param name="action">An action to run when the <paramref name="op"/> completes.</param>
		/// <param name="userState">A user-defined state object that is passed as second argument to <paramref name="action"/>.</param>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="action"/> is <see langword="null"/>.</exception>
		/// <returns>An operation that is executed after <paramref name="op"/> completes.</returns>
		/// <seealso cref="ContinueWith{TResult, TNewResult}(IAsyncOperation{TResult}, Func{IAsyncOperation{TResult}, object, TNewResult}, object, AsyncContinuationOptions)"/>
		public static IAsyncOperation<TNewResult> ContinueWith<TResult, TNewResult>(this IAsyncOperation<TResult> op, Func<IAsyncOperation<TResult>, object, TNewResult> action, object userState)
		{
			return ContinueWith(op, action, userState, AsyncContinuationOptions.None);
		}

		/// <summary>
		/// Creates a continuation that executes when the target <see cref="IAsyncOperation"/> completes.
		/// </summary>
		/// <param name="op">The operation to continue.</param>
		/// <param name="action">An action to run when the <paramref name="op"/> completes.</param>
		/// <param name="userState">A user-defined state object that is passed as second argument to <paramref name="action"/>.</param>
		/// <param name="options">Options for when the <paramref name="action"/> is executed.</param>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="action"/> is <see langword="null"/>.</exception>
		/// <returns>An operation that is executed after <paramref name="op"/> completes.</returns>
		/// <seealso cref="ContinueWith{TResult, TNewResult}(IAsyncOperation{TResult}, Func{IAsyncOperation{TResult}, object, TNewResult}, object)"/>
		public static IAsyncOperation<TNewResult> ContinueWith<TResult, TNewResult>(this IAsyncOperation<TResult> op, Func<IAsyncOperation<TResult>, object, TNewResult> action, object userState, AsyncContinuationOptions options)
		{
			if (action == null)
			{
				throw new ArgumentNullException(nameof(action));
			}

			return new ContinueWithResult<TResult, TNewResult>(op, options, action, userState);
		}

		#endregion

		#region Unwrap

		/// <summary>
		/// Creates a proxy <see cref="IAsyncOperation"/> that represents the asynchronous operation of a <c>IAsyncOperation&lt;IAsyncOperation&gt;</c>.
		/// </summary>
		/// <param name="op">The source operation.</param>
		/// <returns>The unwrapped operation.</returns>
		/// <seealso cref="Unwrap{TResult}(IAsyncOperation{IAsyncOperation{TResult}})"/>
		public static IAsyncOperation Unwrap(this IAsyncOperation<IAsyncOperation> op)
		{
			return new UnwrapResult<VoidResult>(op);
		}

		/// <summary>
		/// Creates a proxy <see cref="IAsyncOperation{TResult}"/> that represents the asynchronous operation of a <c>IAsyncOperation&lt;IAsyncOperation&lt;TResult&gt;&gt;</c>.
		/// </summary>
		/// <param name="op">The source operation.</param>
		/// <returns>The unwrapped operation.</returns>
		/// <seealso cref="Unwrap(IAsyncOperation{IAsyncOperation})"/>
		public static IAsyncOperation<TResult> Unwrap<TResult>(this IAsyncOperation<IAsyncOperation<TResult>> op)
		{
			return new UnwrapResult<TResult>(op);
		}

		#endregion
	}
}
