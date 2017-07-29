// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Threading;

namespace UnityFx.Async
{
	/// <summary>
	/// An operation that finishes when all child operation finish.
	/// </summary>
	internal sealed class AsyncOperationWhenAll : AsyncResult, IAsyncOperationContainer
	{
		#region data

		private readonly List<IAsyncResult> _ops;
		private readonly AsyncContinuationOptions _options;
		private readonly int _opsCount;

		#endregion

		#region interface

		public AsyncOperationWhenAll(IAsyncResult[] ops, CancellationToken cancellationToken, AsyncContinuationOptions options)
			: base(null, cancellationToken)
		{
			_options = options;
			_ops = new List<IAsyncResult>(ops.Length);

			foreach (var op in ops)
			{
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

					_ops.Add(op);
				}
			}
		}

		#endregion

		#region AsyncResult

		protected override void OnUpdate()
		{
			var allComplete = true;
			var progress = 0f;

			foreach (var op in _ops)
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

			if (allComplete)
			{
				SetCompleted();
			}
			else
			{
				SetProgress(progress / _opsCount);
			}
		}

		#endregion

		#region IAsyncOperationContainer

		/// <inheritdoc/>
		public int Size => _opsCount;

		#endregion

		#region implementation
		#endregion
	}
}
