// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Threading;
using UnityEngine;

namespace UnityFx.Async
{
	/// <summary>
	/// A common continuation stuff.
	/// </summary>
	internal class AsyncContinuation<T, U, V> : AsyncResult<V>, IAsyncOperationContainer
		where T : class, IAsyncOperation
		where U : class
	{
		#region data

		private readonly Func<T, U> _continuationFactory;
		private readonly AsyncContinuationOptions _options;

		private T _op;
		private U _continuation;

		private int _opQueueLength = 1;
		private float _opMaxProgress = 0.5f;

		#endregion

		#region interface

		public AsyncContinuation(T op, Func<T, U> continuationFactory, AsyncContinuationOptions options)
			: base(null)
		{
			_continuationFactory = continuationFactory;
			_options = options;
			_op = op;

			if (op is IAsyncOperationContainer aq)
			{
				_opQueueLength = aq.Size;
				_opMaxProgress = _opQueueLength / (_opQueueLength + 1.0f);
			}
		}

#if !NET35
		public AsyncContinuation(T op, Func<T, U> continuationFactory, CancellationToken cancellationToken, AsyncContinuationOptions options)
			: base(null, cancellationToken)
		{
			_continuationFactory = continuationFactory;
			_options = options;
			_op = op;

			if (op is IAsyncOperationContainer aq)
			{
				_opQueueLength = aq.Size;
				_opMaxProgress = _opQueueLength / (_opQueueLength + 1.0f);
			}
		}
#endif

		#endregion

		#region AsyncResult

		protected override void OnUpdate()
		{
			if (_op != null)
			{
				if (_op.IsCompleted)
				{
					SetProgress(_opMaxProgress);

					try
					{
						if (IsCompletedWithOptions(_op, _options))
						{
							_continuation = _continuationFactory(_op);
						}
						else if (_op.IsCanceled)
						{
							SetCanceled();
						}
						else
						{
							SetException(_op.Exception);
						}
					}
					finally
					{
						_op = null;
					}
				}
				else
				{
					SetProgress(_op.Progress * _opMaxProgress);
				}
			}
			else if (_continuation != null)
			{
				if (_continuation is AsyncOperation)
				{
					UpdateContinuation(_continuation as AsyncOperation);
				}
				else if (_continuation is IAsyncOperation<V>)
				{
					UpdateContinuation(_continuation as IAsyncOperation<V>);
				}
				else if (_continuation is IAsyncOperation)
				{
					UpdateContinuation(_continuation as IAsyncOperation);
				}
				else if (_continuation is IAsyncResult)
				{
					UpdateContinuation(_continuation as IAsyncResult);
				}
			}
			else
			{
				SetCompleted();
			}
		}

		#endregion

		#region IAsyncQueue

		public int Size => _opQueueLength + 1;

		#endregion

		#region implementation

		private static bool IsCompletedWithOptions(IAsyncOperation op, AsyncContinuationOptions options)
		{
			if (options != AsyncContinuationOptions.None)
			{
				if (op.IsCompletedSuccessfully)
				{
					return (options & AsyncContinuationOptions.OnlyOnRanToCompletion) != 0;
				}
				else if (op.IsCanceled)
				{
					return (options & AsyncContinuationOptions.OnlyOnCanceled) != 0;
				}
				else
				{
					return (options & AsyncContinuationOptions.OnlyOnFaulted) != 0;
				}
			}

			return true;
		}

		private void UpdateContinuation(AsyncOperation continuation)
		{
			if (continuation.isDone)
			{
				try
				{
					SetResult((V)GetOperationResult(continuation));
				}
				finally
				{
					_continuation = null;
				}
			}
			else
			{
				SetContinuationProgress(continuation.progress);
			}
		}

		private void UpdateContinuation(IAsyncOperation<V> continuation)
		{
			if (continuation.IsCompleted)
			{
				try
				{
					if (continuation.IsCompletedSuccessfully)
					{
						SetResult(continuation.Result);
					}
					else if (continuation.IsCanceled)
					{
						SetCanceled();
					}
					else
					{
						SetException(continuation.Exception);
					}
				}
				finally
				{
					_continuation = null;
				}
			}
			else
			{
				SetContinuationProgress(continuation.Progress);
			}
		}

		private void UpdateContinuation(IAsyncOperation continuation)
		{
			if (continuation.IsCompleted)
			{
				try
				{
					if (continuation.IsCompletedSuccessfully)
					{
						SetCompleted();
					}
					else if (continuation.IsCanceled)
					{
						SetCanceled();
					}
					else
					{
						SetException(continuation.Exception);
					}
				}
				finally
				{
					_continuation = null;
				}
			}
			else
			{
				SetContinuationProgress(continuation.Progress);
			}
		}

		private void UpdateContinuation(IAsyncResult continuation)
		{
			if (continuation.IsCompleted)
			{
				try
				{
					SetCompleted();
				}
				finally
				{
					_continuation = null;
				}
			}
		}

		private void SetContinuationProgress(float progress)
		{
			SetProgress(_opMaxProgress + progress * (1 - _opMaxProgress));
		}

		#endregion
	}
}
