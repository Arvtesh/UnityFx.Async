// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;

namespace UnityFx.Async
{
	internal class ContinueWithResult<T> : ContinuationResult<T>, IAsyncContinuation
	{
		#region data

		private readonly AsyncContinuationOptions _options;
		private readonly object _continuation;
		private readonly object _userState;

		#endregion

		#region interface

		internal ContinueWithResult(IAsyncOperation op, AsyncContinuationOptions options, object continuation, object userState)
			: base((options & AsyncContinuationOptions.ExecuteSynchronously) == 0)
		{
			_options = options;
			_continuation = continuation;
			_userState = userState;

			// NOTE: Cannot move this to base class because this call might trigger _continuation (and it would be uninitialized in base ctor)
			if (!op.TryAddContinuation(this))
			{
				InvokeInternal(op, true);
			}
		}

		#endregion

		#region PromiseResult

		protected override void InvokeUnsafe(IAsyncOperation op, bool completedSynchronously)
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
					TrySetCanceled(completedSynchronously);
					return;
			}

			TrySetResult(result, completedSynchronously);
		}

		#endregion

		#region IAsyncContinuation

		public void Invoke(IAsyncOperation op)
		{
			InvokeInternal(op, false);
		}

		#endregion

		#region implementation

		private void InvokeInternal(IAsyncOperation op, bool completedSynchronously)
		{
			if (AsyncContinuation.CanInvoke(op, _options))
			{
				InvokeOnSyncContext(op, completedSynchronously);
			}
			else
			{
				TrySetCanceled(completedSynchronously);
			}
		}

		#endregion
	}
}
