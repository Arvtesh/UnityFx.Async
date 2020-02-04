// Copyright (c) 2018-2020 Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;

namespace UnityFx.Async
{
	/// <summary>
	/// Represents the consumer side of an asynchronous operation (promise). Extends <see cref="IAsyncOperation"/>
	/// interface with a result value.
	/// </summary>
	/// <typeparam name="TResult">Type of the operation result value.</typeparam>
	/// <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1">Task</seealso>
	/// <seealso cref="IAsyncCompletionSource{TResult}"/>
	/// <seealso cref="IAsyncOperation"/>
	/// <seealso cref="AsyncResult{TResult}"/>
#if NET35
	public interface IAsyncOperation<out TResult> : IAsyncOperation
#else
	public interface IAsyncOperation<out TResult> : IAsyncOperation, IObservable<TResult>
#endif
	{
		/// <summary>
		/// Gets the operation result value.
		/// </summary>
		/// <remarks>
		/// Once the result of an operation is available, it is stored and is returned immediately on subsequent calls to the <see cref="Result"/> property.
		/// Unlike Tasks accessing the property does not block the calling thread (<see cref="InvalidOperationException"/> is throws instead).
		/// Note that, if an exception occurred during the operation, or if the operation has been cancelled, the <see cref="Result"/> property does not return a value.
		/// Instead, attempting to access the property value throws an exception.
		/// </remarks>
		/// <value>Result of the operation.</value>
		/// <exception cref="InvalidOperationException">Thrown if the property is accessed before operation is completed.</exception>
		TResult Result { get; }
	}
}
