// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;

namespace UnityFx.Async
{
	/// <summary>
	/// A common continuation stuff.
	/// </summary>
	internal abstract class AsyncContinuation<T, U, V> : AsyncResult<V>, IAsyncOperationContainer
		where T : class, IAsyncOperation
		where U : class
	{
		#region data

		private readonly Func<T, U> _continuationFactory;

		private T _op;
		private U _continuation;

		private int _opQueueLength = 1;
		private float _opMaxProgress = 0.5f;

		#endregion

		#region interface

		public AsyncContinuation(T op, Func<T, U> continuationFactory)
			: base(null)
		{
			_continuationFactory = continuationFactory;
			_op = op;

			if (op is IAsyncOperationContainer aq)
			{
				_opQueueLength = aq.Size;
				_opMaxProgress = _opQueueLength / (_opQueueLength + 1.0f);
			}
		}

		protected void SetContinuationProgress(float progress)
		{
			SetProgress(_opMaxProgress + progress * (1 - _opMaxProgress));
		}

		protected abstract void OnUpdateContinuation(U continuation);

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
						_continuation = _continuationFactory(_op);
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
				OnUpdateContinuation(_continuation);
			}
		}

		#endregion

		#region IAsyncQueue

		/// <inheritdoc/>
		public int Size => _opQueueLength + 1;

		#endregion

		#region implementation
		#endregion
	}
}
