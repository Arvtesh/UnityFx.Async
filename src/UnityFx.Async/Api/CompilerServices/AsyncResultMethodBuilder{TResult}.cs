// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace UnityFx.Async.CompilerServices
{
#if !NET35

	/// <summary>
	/// Provides a builder for asynchronous methods that return <see cref="AsyncResult{TResult}"/>. This type is intended for compiler use only.
	/// </summary>
	/// <remarks>
	/// <see cref="AsyncResultMethodBuilder{TResult}"/> is a value type, and thus it is copied by value. Prior to being copied,
	/// one of its <see cref="Task"/>, <see cref="SetResult"/>, or <see cref="SetException"/> members must be accessed,
	/// or else the copies may end up building distinct <see cref="AsyncResult{TResult}"/> instances.
	/// </remarks>
	/// <seealso cref="AsyncResultMethodBuilder"/>
	/// <seealso cref="AsyncResultVoidMethodBuilder"/>
	public struct AsyncResultMethodBuilder<TResult>
	{
		private AsyncResult<TResult> _op;

		/// <summary>
		/// Initializes a new <see cref="AsyncResultMethodBuilder{TResult}"/>.
		/// </summary>
		/// <returns>The initialized <see cref="AsyncResultMethodBuilder{TResult}"/>.</returns>
		[DebuggerHidden]
		public static AsyncResultMethodBuilder<TResult> Create()
		{
			return default(AsyncResultMethodBuilder<TResult>);
		}

		/// <summary>
		/// Gets the <see cref="AsyncResult{TResult}"/> for this builder.
		/// </summary>
		/// <value>The <see cref="AsyncResult{TResult}"/> representing the builder's asynchronous operation.</value>
		/// <exception cref="InvalidOperationException">The builder is not initialized.</exception>
		[DebuggerHidden]
		public AsyncResult<TResult> Task
		{
			get
			{
				return _op;
			}
		}

		/// <summary>
		/// Completes the <see cref="AsyncResult{TResult}"/> in the <see cref="AsyncOperationStatus.RanToCompletion"/> state.
		/// </summary>
		/// <param name="result">The result to use to complete the operation.</param>
		/// <exception cref="InvalidOperationException">The builder is not initialized.</exception>
		/// <exception cref="InvalidOperationException">The operation has already completed.</exception>
		[DebuggerHidden]
		public void SetResult(TResult result)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Completes the <see cref="AsyncResult{TResult}"/> in the <see cref="AsyncOperationStatus.Faulted"/> state with the specified <paramref name="exception"/>.
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

		private AsyncResult<TResult> GetTaskForResult(TResult result)
		{
			throw new NotImplementedException();
		}
	}

#endif
}
