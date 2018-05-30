// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;

namespace UnityFx.Async
{
	partial class AsyncResult
	{
		#region data

		private static readonly object _continuationCompletionSentinel = new object();

		private volatile object _continuation;

		#endregion

		#region internals

		/// <summary>
		/// Adds a completion callback for <c>await</c> implementation.
		/// </summary>
		internal void SetContinuationForAwait(Action continuation, SynchronizationContext syncContext)
		{
			ThrowIfDisposed();

			if (!TryAddContinuationInternal(continuation, syncContext))
			{
				continuation();
			}
		}

		#endregion

		#region IAsyncOperationEvents

		/// <inheritdoc/>
		public event ProgressChangedEventHandler ProgressChanged
		{
			add
			{
				ThrowIfDisposed();

				if (value == null)
				{
					throw new ArgumentNullException(nameof(value));
				}

				var syncContext = SynchronizationContext.Current;

				if (!TryAddProgressCallbackInternal(value, syncContext))
				{
					InvokeProgressChanged(value, syncContext);
				}
			}
			remove
			{
				if (value != null)
				{
					TryRemoveContinuationInternal(value);
				}
			}
		}

		/// <inheritdoc/>
		public event AsyncCompletedEventHandler Completed
		{
			add
			{
				ThrowIfDisposed();

				if (value == null)
				{
					throw new ArgumentNullException(nameof(value));
				}

				var syncContext = SynchronizationContext.Current;

				if (!TryAddContinuationInternal(value, syncContext))
				{
					InvokeContinuation(value, syncContext);
				}
			}
			remove
			{
				if (value != null)
				{
					TryRemoveContinuationInternal(value);
				}
			}
		}

		/// <inheritdoc/>
		public void AddContinuation(AsyncOperationCallback action)
		{
			if (!TryAddContinuation(action))
			{
				InvokeContinuation(action, SynchronizationContext.Current);
			}
		}

		/// <inheritdoc/>
		public bool TryAddContinuation(AsyncOperationCallback action)
		{
			ThrowIfDisposed();

			if (action == null)
			{
				throw new ArgumentNullException(nameof(action));
			}

			return TryAddContinuationInternal(action, SynchronizationContext.Current);
		}

		/// <inheritdoc/>
		public void AddContinuation(AsyncOperationCallback action, SynchronizationContext syncContext)
		{
			if (!TryAddContinuation(action, syncContext))
			{
				InvokeContinuation(action, syncContext);
			}
		}

		/// <inheritdoc/>
		public bool TryAddContinuation(AsyncOperationCallback action, SynchronizationContext syncContext)
		{
			ThrowIfDisposed();

			if (action == null)
			{
				throw new ArgumentNullException(nameof(action));
			}

			return TryAddContinuationInternal(action, syncContext);
		}

		/// <inheritdoc/>
		public bool RemoveContinuation(AsyncOperationCallback action)
		{
			if (action != null)
			{
				return TryRemoveContinuationInternal(action);
			}

			return false;
		}

		/// <inheritdoc/>
		public void AddContinuation(IAsyncContinuation continuation)
		{
			if (!TryAddContinuation(continuation))
			{
				InvokeContinuation(continuation, SynchronizationContext.Current);
			}
		}

		/// <inheritdoc/>
		public bool TryAddContinuation(IAsyncContinuation continuation)
		{
			ThrowIfDisposed();

			if (continuation == null)
			{
				throw new ArgumentNullException(nameof(continuation));
			}

			return TryAddContinuationInternal(continuation, SynchronizationContext.Current);
		}

		/// <inheritdoc/>
		public void AddContinuation(IAsyncContinuation continuation, SynchronizationContext syncContext)
		{
			if (!TryAddContinuation(continuation, syncContext))
			{
				InvokeContinuation(continuation, syncContext);
			}
		}

		/// <inheritdoc/>
		public bool TryAddContinuation(IAsyncContinuation continuation, SynchronizationContext syncContext)
		{
			ThrowIfDisposed();

			if (continuation == null)
			{
				throw new ArgumentNullException(nameof(continuation));
			}

			return TryAddContinuationInternal(continuation, syncContext);
		}

		/// <inheritdoc/>
		public bool RemoveContinuation(IAsyncContinuation continuation)
		{
			if (continuation != null)
			{
				return TryRemoveContinuationInternal(continuation);
			}

			return false;
		}

#if !NET35

		/// <inheritdoc/>
		public void AddProgressCallback(IProgress<float> callback)
		{
			if (!TryAddProgressCallback(callback))
			{
				InvokeProgressChanged(callback, SynchronizationContext.Current);
			}
		}

		/// <inheritdoc/>
		public bool TryAddProgressCallback(IProgress<float> callback)
		{
			ThrowIfDisposed();

			if (callback == null)
			{
				throw new ArgumentNullException(nameof(callback));
			}

			return TryAddProgressCallbackInternal(callback, SynchronizationContext.Current);
		}

		/// <inheritdoc/>
		public void AddProgressCallback(IProgress<float> callback, SynchronizationContext syncContext)
		{
			if (!TryAddProgressCallback(callback, syncContext))
			{
				InvokeProgressChanged(callback, syncContext);
			}
		}

		/// <inheritdoc/>
		public bool TryAddProgressCallback(IProgress<float> callback, SynchronizationContext syncContext)
		{
			ThrowIfDisposed();

			if (callback == null)
			{
				throw new ArgumentNullException(nameof(callback));
			}

			return TryAddProgressCallbackInternal(callback, syncContext);
		}

		/// <inheritdoc/>
		public bool RemoveProgressCallback(IProgress<float> callback)
		{
			if (callback != null)
			{
				return TryRemoveContinuationInternal(callback);
			}

			return false;
		}

#endif

		#endregion

		#region implementation

		private bool TryAddContinuationInternal(object continuation, SynchronizationContext syncContext)
		{
			var runContinuationsAsynchronously = (_flags & _flagRunContinuationsAsynchronously) != 0;

			if ((syncContext != null && syncContext.GetType() != typeof(SynchronizationContext)) || runContinuationsAsynchronously)
			{
				continuation = new AsyncContinuation(this, syncContext, continuation);
			}

			return TryAddContinuationInternal(continuation);
		}

		private bool TryAddProgressCallbackInternal(object callback, SynchronizationContext syncContext)
		{
			if (syncContext != null && syncContext.GetType() != typeof(SynchronizationContext))
			{
				callback = new AsyncProgress(this, syncContext, callback);
			}

			return TryAddContinuationInternal(callback);
		}

		private bool TryAddContinuationInternal(object valueToAdd)
		{
			// NOTE: The code below is adapted from https://referencesource.microsoft.com/#mscorlib/system/threading/Tasks/Task.cs.
			var oldValue = _continuation;

			// Quick return if the operation is completed.
			if (oldValue != _continuationCompletionSentinel)
			{
				// If no continuation is stored yet, try to store it as _continuation.
				if (oldValue == null)
				{
					oldValue = Interlocked.CompareExchange(ref _continuation, valueToAdd, null);

					// Quick return if exchange succeeded.
					if (oldValue == null)
					{
						return true;
					}
				}

				// Logic for the case where we were previously storing a single continuation.
				if (oldValue != _continuationCompletionSentinel && !(oldValue is IList))
				{
					var newList = new List<object>() { oldValue };

					Interlocked.CompareExchange(ref _continuation, newList, oldValue);

					// We might be racing against another thread converting the single into a list,
					// or we might be racing against operation completion, so resample "list" below.
				}

				// If list is null, it can only mean that _continuationCompletionSentinel has been exchanged
				// into _continuation. Thus, the task has completed and we should return false from this method,
				// as we will not be queuing up the continuation.
				if (_continuation is IList list)
				{
					lock (list)
					{
						// It is possible for the operation to complete right after we snap the copy of the list.
						// If so, then fall through and return false without queuing the continuation.
						if (_continuation != _continuationCompletionSentinel)
						{
							list.Add(valueToAdd);
							return true;
						}
					}
				}
			}

			return false;
		}

		private bool TryRemoveContinuationInternal(object valueToRemove)
		{
			// NOTE: The code below is adapted from https://referencesource.microsoft.com/#mscorlib/system/threading/Tasks/Task.cs.
			var value = _continuation;

			if (value != _continuationCompletionSentinel)
			{
				var list = value as IList;

				if (list == null)
				{
					// This is not a list. If we have a single object (the one we want to remove) we try to replace it with an empty list.
					// Note we cannot go back to a null state, since it will mess up the TryAddContinuation logic.
					if (Interlocked.CompareExchange(ref _continuation, new List<object>(), valueToRemove) == valueToRemove)
					{
						return true;
					}
					else
					{
						// If we fail it means that either TryAddContinuation won the race condition and _continuation is now a List
						// that contains the element we want to remove. Or it set the _continuationCompletionSentinel.
						// So we should try to get a list one more time.
						list = value as IList;
					}
				}

				// If list is null it means _continuationCompletionSentinel has been set already and there is nothing else to do.
				if (list != null)
				{
					lock (list)
					{
						// There is a small chance that the operation completed since we took a local snapshot into
						// list. In that case, just return; we don't want to be manipulating the continuation list as it is being processed.
						if (_continuation != _continuationCompletionSentinel)
						{
							var index = list.IndexOf(valueToRemove);

							if (index != -1)
							{
								list.RemoveAt(index);
								return true;
							}
						}
					}
				}
			}

			return false;
		}

		private void InvokeProgressChanged()
		{
			var continuation = _continuation;

			if (continuation != null)
			{
				if (continuation is IEnumerable continuationList)
				{
					lock (continuationList)
					{
						foreach (var item in continuationList)
						{
							InvokeProgressChanged(item);
						}
					}
				}
				else
				{
					InvokeProgressChanged(continuation);
				}
			}
		}

		private void InvokeProgressChanged(object callback)
		{
			if (callback is AsyncProgress p)
			{
				p.Invoke();
			}
			else
			{
				AsyncProgress.InvokeInline(this, callback);
			}
		}

		private void InvokeProgressChanged(object callback, SynchronizationContext syncContext)
		{
			if (syncContext == null || syncContext == SynchronizationContext.Current)
			{
				AsyncProgress.InvokeInline(this, callback);
			}
			else
			{
				syncContext.Post(args => AsyncProgress.InvokeInline(this, args), callback);
			}
		}

		private void InvokeContinuations()
		{
			var continuation = Interlocked.Exchange(ref _continuation, _continuationCompletionSentinel);

			if (continuation != null)
			{
				if (continuation is IEnumerable continuationList)
				{
					lock (continuationList)
					{
						foreach (var item in continuationList)
						{
							InvokeContinuation(item);
						}
					}
				}
				else
				{
					InvokeContinuation(continuation);
				}
			}
		}

		private void InvokeContinuation(object continuation)
		{
			var runContinuationsAsynchronously = (_flags & _flagRunContinuationsAsynchronously) != 0;

			if (runContinuationsAsynchronously)
			{
				if (continuation is AsyncInvokable c)
				{
					// NOTE: This is more effective than InvokeContinuationAsync().
					c.InvokeAsync();
				}
				else
				{
					InvokeContinuationAsync(continuation, SynchronizationContext.Current, false);
				}
			}
			else
			{
				if (continuation is AsyncInvokable c)
				{
					c.Invoke();
				}
				else
				{
					InvokeContinuationInline(continuation, false);
				}
			}
		}

		private void InvokeContinuation(object continuation, SynchronizationContext syncContext)
		{
			if ((_flags & _flagRunContinuationsAsynchronously) != 0)
			{
				InvokeContinuationAsync(continuation, syncContext, true);
			}
			else if (syncContext == null || syncContext == SynchronizationContext.Current)
			{
				InvokeContinuationInline(continuation, true);
			}
			else
			{
				syncContext.Post(args => InvokeContinuationInline(args, true), continuation);
			}
		}

		private void InvokeContinuationAsync(object continuation, SynchronizationContext syncContext, bool inline)
		{
			if (syncContext != null && syncContext.GetType() != typeof(SynchronizationContext))
			{
				syncContext.Post(args => InvokeContinuationInline(args, inline), continuation);
			}
			else
			{
				ThreadPool.QueueUserWorkItem(args => InvokeContinuationInline(args, inline), continuation);
			}
		}

		private void InvokeContinuationInline(object continuation, bool inline)
		{
			AsyncContinuation.InvokeInline(this, continuation, inline);
		}

		#endregion
	}
}
