// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections;
using System.Diagnostics;
using System.Threading;

namespace UnityFx.Async
{
	/// <summary>
	/// Implementation of <see cref="IAsyncOperation{T}"/> that wraps another <see cref="IAsyncOperation{T}"/> instance and transforma its result.
	/// </summary>
	/// <typeparam name="T">Type of the operation result.</typeparam>
	/// <typeparam name="U">Result type of the wrapped operation.</typeparam>
	[DebuggerDisplay("Status = {Status}, Progress={Progress}, StatusText = {StatusText}")]
	internal sealed class AsyncResultTransformer<T, U> : IAsyncOperation<T>, IEnumerator
	{
		#region data

		private readonly IAsyncOperation<U> _op;
		private readonly Func<U, T> _transformer;

		#endregion

		#region interface

		public AsyncResultTransformer(IAsyncOperation<U> op, Func<U, T> transformer)
		{
			_op = op;
			_transformer = transformer;
		}

		#endregion

		#region IAsyncOperation

		public T Result => _transformer(_op.Result);

		public float Progress => _op.Progress;

		public AsyncOperationStatus Status => _op.Status;

		public Exception Exception => _op.Exception;

		public bool IsCompletedSuccessfully => _op.IsCompletedSuccessfully;

		public bool IsFaulted => _op.IsFaulted;

		public bool IsCanceled => _op.IsCanceled;

		#endregion

		#region IAsyncResult

		public object AsyncState => _op.AsyncState;

		public WaitHandle AsyncWaitHandle => _op.AsyncWaitHandle;

		public bool CompletedSynchronously => _op.CompletedSynchronously;

		public bool IsCompleted => _op.IsCompleted;

		#endregion

		#region IObservable

		public IDisposable Subscribe(IObserver<T> observer) => throw new NotImplementedException();

		#endregion

		#region IEnumerator

		public object Current => _op is IEnumerator op ? op.Current : null;

		public bool MoveNext() => _op is IEnumerator op ? op.MoveNext() : _op.IsCompleted;

		public void Reset() => throw new NotSupportedException();

		#endregion

		#region IDisposable

		public void Dispose() => _op.Dispose();

		#endregion

		#region Object

		public override string ToString() => _op.ToString();

		#endregion
	}
}
