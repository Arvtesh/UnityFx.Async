// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;

namespace UnityFx.Async
{
	internal class ContinuationResult<T> : ContinuationResultBase<T>, IAsyncContinuation
	{
		#region data

		private readonly object _continuation;
		private readonly object _userState;

		#endregion

		#region interface

		internal ContinuationResult(IAsyncOperation op, AsyncContinuationOptions options, object continuation, object userState)
			: base(options)
		{
			_continuation = continuation;
			_userState = userState;

			// NOTE: Cannot move this to base class because this call might trigger _continuation (and it would be uninitialized in base ctor)
			if (!op.TryAddContinuation(this))
			{
				InvokeOnSyncContext(op, true);
			}
		}

		#endregion

		#region AsyncContinuation

		protected override T OnInvoke(IAsyncOperation op)
		{
			var result = default(T);

			switch (_continuation)
			{
				case Action<IAsyncOperation> a:
					a.Invoke(op);
					break;

				case Func<IAsyncOperation, T> f:
					result = f.Invoke(op);
					break;

				case Action<IAsyncOperation, object> ao:
					ao.Invoke(op, _userState);
					break;

				case Func<IAsyncOperation, object, T> fo:
					result = fo.Invoke(op, _userState);
					break;

				default:
					// Should not get here.
					throw new InvalidOperationException();
			}

			return result;
		}

		#endregion

		#region IAsyncContinuation

		public void Invoke(IAsyncOperation op)
		{
			InvokeOnSyncContext(op, false);
		}

		#endregion
	}
}
