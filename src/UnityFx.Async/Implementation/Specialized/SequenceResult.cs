// Copyright (c) 2018-2020 Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;

namespace UnityFx.Async
{
	internal sealed class SequenceResult : AsyncResult
	{
		#region data

		// NOTE: Cannot use IReadOnlyList for net35 compatibilty.
		private readonly IList<Func<IAsyncOperation>> _sequence;

		private IAsyncOperation _op;
		private int _opIndex;

		#endregion

		#region interface

		public SequenceResult(IList<Func<IAsyncOperation>> ops)
			: base(ops.Count > 0 ? AsyncOperationStatus.Running : AsyncOperationStatus.RanToCompletion)
		{
			_sequence = ops;
			MoveNext();
		}

		#endregion

		#region AsyncResult

		protected override float GetProgress()
		{
			if (_op != null)
			{
				return (_opIndex + _op.Progress) / _sequence.Count;
			}

			return (float)_opIndex / _sequence.Count;
		}

		protected override void OnCancel()
		{
			_op?.Cancel();
		}

		#endregion

		#region IAsyncContinuation

		public override void Invoke(IAsyncOperation asyncOp)
		{
			if (IsCompleted)
			{
				return;
			}

			++_opIndex;

			if (asyncOp.IsCompletedSuccessfully)
			{
				MoveNext();
			}
			else
			{
				TrySetException(asyncOp.Exception, false);
			}
		}

		#endregion

		#region implementation

		private bool MoveNext()
		{
			try
			{
				if (_opIndex < _sequence.Count)
				{
					_op = _sequence[_opIndex]();
					_op.AddCompletionCallback(this, null);
					ReportProgress();
					return true;
				}
				else
				{
					TrySetCompleted();
				}
			}
			catch (Exception e)
			{
				TrySetException(e);
			}

			return false;
		}

		#endregion
	}
}
