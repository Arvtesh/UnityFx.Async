// Copyright (c) 2018-2020 Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.ComponentModel;
using System.Threading;

namespace UnityFx.Async.Extensions
{
	/// <summary>
	/// Extension methods for <see cref="SynchronizationContext"/>.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	public static class SynchronizationContextExtensions
	{
		#region data

		private static SendOrPostCallback _actionCallback;

		#endregion

		#region interface

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
		/// Dispatches an synchronous message to a synchronization context.
		/// </summary>
		/// <param name="context">The target context.</param>
		/// <param name="action">The delegate to invoke.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="action"/> is <see langword="null"/>.</exception>
		/// <returns>Returns result of the <paramref name="action"/> call.</returns>
		public static T Send<T>(this SynchronizationContext context, Func<T> action)
		{
			if (action == null)
			{
				throw new ArgumentNullException(nameof(action));
			}

			var result = default(T);

			context.Send(
				state =>
				{
					result = ((Func<T>)state)();
				},
				action);

			return result;
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
		/// Dispatches an asynchronous message to a synchronization context.
		/// </summary>
		/// <param name="context">The target context.</param>
		/// <param name="action">The delegate to invoke.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="action"/> is <see langword="null"/>.</exception>
		/// <returns>An <see cref="IAsyncOperation"/> that can be used to track the operation status.</returns>
		public static IAsyncOperation PostAsync(this SynchronizationContext context, Action action)
		{
			if (action == null)
			{
				throw new ArgumentNullException(nameof(action));
			}

			var op = new ActionResult(action);

			context.Post(
				state =>
				{
					((ActionResult)state).Start();
				},
				op);

			return op;
		}

		/// <summary>
		/// Dispatches an asynchronous message to a synchronization context.
		/// </summary>
		/// <param name="context">The target context.</param>
		/// <param name="d">The delegate to invoke.</param>
		/// <param name="state">User-defined state.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="d"/> is <see langword="null"/>.</exception>
		/// <returns>An <see cref="IAsyncOperation"/> that can be used to track the operation status.</returns>
		public static IAsyncOperation PostAsync(this SynchronizationContext context, SendOrPostCallback d, object state)
		{
			if (d == null)
			{
				throw new ArgumentNullException(nameof(d));
			}

			var op = new ActionResult(d, state);

			context.Post(
				s =>
				{
					((ActionResult)s).Start();
				},
				op);

			return op;
		}

		/// <summary>
		/// Dispatches an asynchronous message to a synchronization context.
		/// </summary>
		/// <param name="context">The target context.</param>
		/// <param name="action">The delegate to invoke.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="action"/> is <see langword="null"/>.</exception>
		/// <returns>An <see cref="IAsyncOperation{TResult}"/> that can be used to track the operation status.</returns>
		public static IAsyncOperation<T> PostAsync<T>(this SynchronizationContext context, Func<T> action)
		{
			if (action == null)
			{
				throw new ArgumentNullException(nameof(action));
			}

			var op = new ActionResult<T>(action);

			context.Post(
				state =>
				{
					((ActionResult<T>)state).Start();
				},
				op);

			return op;
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
		/// <param name="action">The delegate to invoke.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="action"/> is <see langword="null"/>.</exception>
		/// <returns>An <see cref="IAsyncOperation"/> that can be used to track the operation status.</returns>
		public static IAsyncOperation InvokeAsync(this SynchronizationContext context, Action action)
		{
			if (context == SynchronizationContext.Current)
			{
				return AsyncResult.FromAction(action);
			}
			else
			{
				return PostAsync(context, action);
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

		/// <summary>
		/// Dispatches a message to a synchronization context.
		/// </summary>
		/// <param name="context">The target context.</param>
		/// <param name="d">The delegate to invoke.</param>
		/// <param name="state">User-defined state.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="d"/> is <see langword="null"/>.</exception>
		/// <returns>An <see cref="IAsyncOperation"/> that can be used to track the operation status.</returns>
		public static IAsyncOperation InvokeAsync(this SynchronizationContext context, SendOrPostCallback d, object state)
		{
			if (context == SynchronizationContext.Current)
			{
				return AsyncResult.FromAction(d, state);
			}
			else
			{
				return PostAsync(context, d, state);
			}
		}

		/// <summary>
		/// Dispatches a message to a synchronization context.
		/// </summary>
		/// <param name="context">The target context.</param>
		/// <param name="action">The delegate to invoke.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="action"/> is <see langword="null"/>.</exception>
		/// <returns>An <see cref="IAsyncOperation{TResult}"/> that can be used to track the operation status.</returns>
		public static IAsyncOperation<T> InvokeAsync<T>(this SynchronizationContext context, Func<T> action)
		{
			if (context == SynchronizationContext.Current)
			{
				return AsyncResult.FromAction(action);
			}
			else
			{
				return PostAsync(context, action);
			}
		}

		#endregion

		#region implementation

		private static void ActionCallback(object args)
		{
			((Action)args).Invoke();
		}

		#endregion
	}
}
