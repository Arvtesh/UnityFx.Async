// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Threading;

namespace UnityFx.Async
{
	/// <summary>
	/// An operation that finishes when all child operation finish.
	/// </summary>
	internal sealed class AsyncOperationWhenAll<T> : AsyncResult<T[]>, IAsyncOperationContainer
	{
		#region data

		private IAsyncOperation<T>[] _ops;
		private int _opsCount;

		#endregion

		#region interface

		public AsyncOperationWhenAll(IAsyncOperation<T>[] ops)
			: base(null)
		{
			Initialize(ops);
		}

#if NET46
		public AsyncOperationWhenAll(IAsyncOperation<T>[] ops, CancellationToken cancellationToken)
			: base(null, cancellationToken)
		{
			Initialize(ops);
		}
#endif

		#endregion

		#region AsyncResult

		protected override void OnUpdate()
		{
			var allComplete = true;
			var progress = 0f;

			for (var i = 0; i < _ops.Length; ++i)
			{
				var op = _ops[i];

				if (op != null)
				{
					var opLength = op is IAsyncOperationContainer c ? c.Size : 1;

					if (op.IsCompleted)
					{
						progress += opLength;
					}
					else
					{
						if (op is IAsyncOperation asyncOp)
						{
							progress += asyncOp.Progress * opLength;
						}

						allComplete = false;
					}
				}
			}

			if (allComplete)
			{
				var result = new T[_ops.Length];

				for (var i = 0; i < _ops.Length; ++i)
				{
					var op = _ops[i];

					if (op != null && op.IsCompletedSuccessfully)
					{
						result[i] = op.Result;
					}
				}

				SetResult(result);
			}
			else
			{
				SetProgress(progress / _opsCount);
			}
		}

		#endregion

		#region IAsyncOperationContainer

		public int Size => _opsCount;

		#endregion

		#region implementation

		private void Initialize(IAsyncOperation<T>[] ops)
		{
			_ops = new IAsyncOperation<T>[ops.Length];

			for (var i = 0; i < ops.Length; ++i)
			{
				var op = ops[i];

				if (op != null)
				{
					if (op is IAsyncOperationContainer c)
					{
						_opsCount += c.Size;
					}
					else
					{
						_opsCount += 1;
					}

					_ops[i] = op;
				}
			}
		}

		#endregion
	}
}
