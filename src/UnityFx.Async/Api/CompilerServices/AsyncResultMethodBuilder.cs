// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace UnityFx.Async.CompilerServices
{
#if !NET35

	/// <summary>
	/// Provides a builder for asynchronous methods that return <see cref="AsyncResult"/>. This type is intended for compiler use only.
	/// </summary>
	/// <remarks>
	/// <see cref="AsyncResultMethodBuilder"/> is a value type, and thus it is copied by value. Prior to being copied,
	/// one of its <see cref="Task"/>, <see cref="SetResult"/>, or <see cref="SetException"/> members must be accessed,
	/// or else the copies may end up building distinct <see cref="AsyncResult"/> instances.
	/// </remarks>
	/// <seealso cref="AsyncResultMethodBuilder{TResult}"/>
	/// <seealso cref="AsyncResultVoidMethodBuilder"/>
	public struct AsyncResultMethodBuilder
	{
		private AsyncResult _op;

		/// <summary>
		/// Initializes a new <see cref="AsyncResultMethodBuilder"/>.
		/// </summary>
		/// <returns>The initialized <see cref="AsyncResultMethodBuilder"/>.</returns>
		[DebuggerHidden]
		public static AsyncResultMethodBuilder Create()
		{
			return default(AsyncResultMethodBuilder);
		}

		/// <summary>
		/// Gets the <see cref="AsyncResult"/> for this builder.
		/// </summary>
		/// <value>The <see cref="AsyncResult"/> representing the builder's asynchronous operation.</value>
		/// <exception cref="InvalidOperationException">The builder is not initialized.</exception>
		[DebuggerHidden]
		public AsyncResult Task
		{
			get
			{
				if (_op == null)
				{
					_op = new AsyncResult();
				}

				return _op;
			}
		}

		/// <summary>
		/// Completes the <see cref="AsyncResult"/> in the <see cref="AsyncOperationStatus.RanToCompletion"/> state.
		/// </summary>
		/// <exception cref="InvalidOperationException">The builder is not initialized.</exception>
		/// <exception cref="InvalidOperationException">The operation has already completed.</exception>
		[DebuggerHidden]
		public void SetResult()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Completes the <see cref="AsyncResult"/> in the <see cref="AsyncOperationStatus.Faulted"/> state with the specified <paramref name="exception"/>.
		/// </summary>
		/// <param name="exception">The <see cref="Exception"/> to use to fault the operation.</param>
		/// <exception cref="ArgumentNullException">The <paramref name="exception"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException">The builder is not initialized.</exception>
		/// <exception cref="InvalidOperationException">The operation has already completed.</exception>
		[DebuggerHidden]
		public void SetException(Exception exception)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Initiates the builder's execution with the associated state machine.
		/// </summary>
		/// <typeparam name="TStateMachine">Specifies the type of the state machine.</typeparam>
		/// <param name="stateMachine">The state machine instance, passed by reference.</param>
		[DebuggerHidden]
		public void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Associates the builder with the state machine it represents.
		/// </summary>
		/// <param name="stateMachine">The heap-allocated state machine object.</param>
		[DebuggerHidden]
		public void SetStateMachine(IAsyncStateMachine stateMachine)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Schedules the specified state machine to be pushed forward when the specified awaiter completes.
		/// </summary>
		/// <typeparam name="TAwaiter">Specifies the type of the awaiter.</typeparam>
		/// <typeparam name="TStateMachine">Specifies the type of the state machine.</typeparam>
		/// <param name="awaiter">The awaiter passed by reference.</param>
		/// <param name="stateMachine">The state machine passed by reference.</param>
		[DebuggerHidden]
		public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine) where TAwaiter : INotifyCompletion where TStateMachine : IAsyncStateMachine
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Schedules the specified state machine to be pushed forward when the specified awaiter completes.
		/// </summary>
		/// <typeparam name="TAwaiter">Specifies the type of the awaiter.</typeparam>
		/// <typeparam name="TStateMachine">Specifies the type of the state machine.</typeparam>
		/// <param name="awaiter">The awaiter passed by reference.</param>
		/// <param name="stateMachine">The state machine passed by reference.</param>
		[DebuggerHidden]
		public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine) where TAwaiter : INotifyCompletion where TStateMachine : IAsyncStateMachine
		{
			throw new NotImplementedException();
		}
	}

#endif
}
