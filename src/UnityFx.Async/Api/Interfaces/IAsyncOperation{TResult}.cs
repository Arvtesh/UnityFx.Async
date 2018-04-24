// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;

namespace UnityFx.Async
{
	/// <summary>
	/// Represents the consumer side of an asynchronous operation. Extends an <see cref="IAsyncOperation"/>
	/// interface with a result value.
	/// </summary>
	/// <typeparam name="TResult">Type of th operation result value.</typeparam>
	/// <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.task-1">Task</seealso>
	/// <seealso cref="IAsyncOperation"/>
	/// <seealso cref="IAsyncResult"/>
	public interface IAsyncOperation<out TResult> : IAsyncOperation
	{
		/// <summary>
		/// Gets the operation result value.
		/// </summary>
		/// <remarks>
		/// Once the result of an operation is available, it is stored and is returned immediately on subsequent calls to the <see cref="Result"/> property.
		/// Note that, if an exception occurred during the operation, or if the operation has been cancelled, the <see cref="Result"/> property does not return a value.
		/// Instead, attempting to access the property value throws an exception.
		/// </remarks>
		/// <value>Result of the operation.</value>
		/// <exception cref="InvalidOperationException">Thrown either if the property is accessed before operation is completed.</exception>
		/// <exception cref="AggregateException">Thrown if the operation is in <see cref="AsyncOperationStatus.Faulted"/> or <see cref="AsyncOperationStatus.Canceled"/> state.</exception>
		TResult Result { get; }
	}
}
