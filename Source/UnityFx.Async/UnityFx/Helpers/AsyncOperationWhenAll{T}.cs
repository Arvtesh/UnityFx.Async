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

		private readonly IAsyncOperation<T>[] _ops;
		private readonly AsyncContinuationOptions _options;
		private readonly int _opsCount;

		#endregion

		#region interface

		public AsyncOperationWhenAll(IAsyncOperation<T>[] ops, CancellationToken cancellationToken, AsyncContinuationOptions options)
			: base(null, cancellationToken)
		{
			_ops = new IAsyncOperation<T>[ops.Length];
			_options = options;

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
						if (IsCompletedWithOptions(op, _options))
						{
							progress += opLength;
						}
						else
						{
							throw new Exception("Async continuation failed");
						}
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
		#endregion
	}
}
