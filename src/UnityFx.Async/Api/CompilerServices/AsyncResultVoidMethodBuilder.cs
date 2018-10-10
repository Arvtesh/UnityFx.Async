// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace UnityFx.Async.CompilerServices
{
#if !NET35

	/// <summary>
	/// Provides a builder for asynchronous methods that return <see langword="void"/>. This type is intended for compiler use only.
	/// </summary>
	/// <seealso cref="AsyncResultMethodBuilder"/>
	/// <seealso cref="AsyncResultMethodBuilder{TResult}"/>
	public struct AsyncResultVoidMethodBuilder
	{
		/// <summary>
		/// Initializes a new <see cref="AsyncResultVoidMethodBuilder"/>.
		/// </summary>
		/// <returns>The initialized <see cref="AsyncResultVoidMethodBuilder"/>.</returns>
		[DebuggerHidden]
		public static AsyncResultMethodBuilder Create()
		{
			return default(AsyncResultMethodBuilder);
		}

		/// <summary>
		/// Completes the method builder successfully.
		/// </summary>
		[DebuggerHidden]
		public void SetResult()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Faults the method builder with an exception.
		/// </summary>
		/// <param name="exception">The <see cref="Exception"/> that is the cause of this fault.</param>
		/// <exception cref="ArgumentNullException">The <paramref name="exception"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException">The builder is not initialized.</exception>
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
