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

		private static readonly object _callbackCompletionSentinel = new object();
		private static SynchronizationContext _sharedContext;

		private volatile object _callback;

		#endregion

		#region interface

		/// <summary>
		/// Gets or sets shared <see cref="SynchronizationContext"/> instance used for continuations.
		/// </summary>
		public static SynchronizationContext SharedSynchronizationContext
		{
			get
			{
				return _sharedContext;
			}
			set
			{
				_sharedContext = value;
			}
		}

		/// <summary>
		/// Adds a completion callback for <c>await</c> implementation.
		/// </summary>
		internal void SetContinuationForAwait(Action continuation, SynchronizationContext syncContext)
		{
			ThrowIfDisposed();

			if (!TryAddCallback(continuation, syncContext, true))
			{
				continuation();
			}
		}

		#endregion

		#region IAsyncOperationEvents

		/// <summary>
		/// Raised when the operation progress is changed.
		/// </summary>
		/// <remarks>
		/// The event handler is invoked on a thread that registered it (if it has a <see cref="SynchronizationContext"/> attached).
		/// If the operation is already completed the event handler is called synchronously. Throwing an exception from the event handler
		/// might cause unspecified behaviour.
		/// </remarks>
		/// <exception cref="ArgumentNullException">Thrown if the delegate being registered is <see langword="null"/>.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation has been disposed.</exception>
		/// <seealso cref="Completed"/>
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

				if (!TryAddCallback(value, syncContext, false))
				{
					InvokeProgressCallback(value, syncContext);
				}
			}
			remove
			{
				TryRemoveCallback(value);
			}
		}

		/// <summary>
		/// Raised when the operation is completed.
		/// </summary>
		/// <remarks>
		/// The event handler is invoked on a thread that registered it (if it has a <see cref="SynchronizationContext"/> attached).
		/// If the operation is already completed the event handler is called synchronously. Throwing an exception from the event handler
		/// might cause unspecified behaviour.
		/// </remarks>
		/// <exception cref="ArgumentNullException">Thrown if the delegate being registered is <see langword="null"/>.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation has been disposed.</exception>
		/// <seealso cref="ProgressChanged"/>
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

				if (!TryAddCallback(value, syncContext, true))
				{
					InvokeCompletionCallback(value, syncContext);
				}
			}
			remove
			{
				TryRemoveCallback(value);
			}
		}

		/// <summary>
		/// Adds a completion callback to be executed after the operation has completed. If the operation is already completed the <paramref name="action"/> is called synchronously.
		/// </summary>
		/// <remarks>
		/// The <paramref name="action"/> is invoked on a thread that registered the continuation (if it has a <see cref="SynchronizationContext"/> attached).
		/// Throwing an exception from the callback might cause unspecified behaviour.
		/// </remarks>
		/// <param name="action">The callback to be executed when the operation has completed.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="action"/> is <see langword="null"/>.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation has been disposed.</exception>
		public void AddCompletionCallback(Action<IAsyncOperation> action)
		{
			AddCompletionCallback(action, SynchronizationContext.Current);
		}

		/// <summary>
		/// Attempts to add a completion callback to be executed after the operation has finished. If the operation is already completed
		/// the method does nothing and just returns <see langword="false"/>.
		/// </summary>
		/// <remarks>
		/// The <paramref name="action"/> is invoked on a thread that registered the continuation (if it has a <see cref="SynchronizationContext"/> attached).
		/// Throwing an exception from the callback might cause unspecified behaviour.
		/// </remarks>
		/// <param name="action">The callback to be executed when the operation has completed.</param>
		/// <returns>Returns <see langword="true"/> if the callback was added; <see langword="false"/> otherwise (the operation is completed).</returns>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="action"/> is <see langword="null"/>.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation has been disposed.</exception>
		public bool TryAddCompletionCallback(Action<IAsyncOperation> action)
		{
			return TryAddCompletionCallback(action, SynchronizationContext.Current);
		}

		/// <summary>
		/// Adds a completion callback to be executed after the operation has completed. If the operation is completed <paramref name="action"/> is invoked
		/// on the <paramref name="syncContext"/> specified.
		/// </summary>
		/// <remarks>
		/// The <paramref name="action"/> is invoked on a <see cref="SynchronizationContext"/> specified. Throwing an exception from the callback might cause unspecified behaviour.
		/// </remarks>
		/// <param name="action">The callback to be executed when the operation has completed.</param>
		/// <param name="syncContext">If not <see langword="null"/> method attempts to marshal the continuation to the synchronization context.
		/// Otherwise the callback is invoked on a thread that initiated the operation completion.
		/// </param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="action"/> is <see langword="null"/>.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation has been disposed.</exception>
		public void AddCompletionCallback(Action<IAsyncOperation> action, SynchronizationContext syncContext)
		{
			if (!TryAddCompletionCallback(action, syncContext))
			{
				InvokeCompletionCallback(action, syncContext);
			}
		}

		/// <summary>
		/// Attempts to add a completion callback to be executed after the operation has finished. If the operation is already completed
		/// the method does nothing and just returns <see langword="false"/>.
		/// </summary>
		/// <remarks>
		/// The <paramref name="action"/> is invoked on a <see cref="SynchronizationContext"/> specified. Throwing an exception from the callback might cause unspecified behaviour.
		/// </remarks>
		/// <param name="action">The callback to be executed when the operation has completed.</param>
		/// <param name="syncContext">If not <see langword="null"/> method attempts to marshal the continuation to the synchronization context.
		/// Otherwise the callback is invoked on a thread that initiated the operation completion.
		/// </param>
		/// <returns>Returns <see langword="true"/> if the callback was added; <see langword="false"/> otherwise (the operation is completed).</returns>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="action"/> is <see langword="null"/>.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation has been disposed.</exception>
		public bool TryAddCompletionCallback(Action<IAsyncOperation> action, SynchronizationContext syncContext)
		{
			ThrowIfDisposed();

			if (action == null)
			{
				throw new ArgumentNullException(nameof(action));
			}

			return TryAddCallback(action, syncContext, true);
		}

		/// <summary>
		/// Removes an existing completion callback.
		/// </summary>
		/// <param name="action">The callback to remove. Can be <see langword="null"/>.</param>
		/// <returns>Returns <see langword="true"/> if <paramref name="action"/> was removed; <see langword="false"/> otherwise.</returns>
		public bool RemoveCompletionCallback(Action<IAsyncOperation> action)
		{
			return TryRemoveCallback(action);
		}

		/// <summary>
		/// Adds a continuation to be executed after the operation has completed. If the operation is completed <paramref name="continuation"/> is invoked synchronously.
		/// </summary>
		/// <remarks>
		/// The <paramref name="continuation"/> is invoked on a thread that registered the continuation (if it has a <see cref="SynchronizationContext"/> attached).
		/// Throwing an exception from the callback might cause unspecified behaviour.
		/// </remarks>
		/// <param name="continuation">The callback to be executed when the operation has completed.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="continuation"/> is <see langword="null"/>.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation has been disposed.</exception>
		public void AddCompletionCallback(IAsyncContinuation continuation)
		{
			AddCompletionCallback(continuation, SynchronizationContext.Current);
		}

		/// <summary>
		/// Attempts to add a continuation to be executed after the operation has finished. If the operation is already completed
		/// the method does nothing and just returns <see langword="false"/>.
		/// </summary>
		/// <remarks>
		/// The <paramref name="continuation"/> is invoked on a thread that registered the continuation (if it has a <see cref="SynchronizationContext"/> attached).
		/// Throwing an exception from the callback might cause unspecified behaviour.
		/// </remarks>
		/// <param name="continuation">The cotinuation to be executed when the operation has completed.</param>
		/// <returns>Returns <see langword="true"/> if the callback was added; <see langword="false"/> otherwise (the operation is completed).</returns>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="continuation"/> is <see langword="null"/>.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation has been disposed.</exception>
		public bool TryAddCompletionCallback(IAsyncContinuation continuation)
		{
			return TryAddCompletionCallback(continuation, SynchronizationContext.Current);
		}

		/// <summary>
		/// Adds a continuation to be executed after the operation has completed. If the operation is completed <paramref name="continuation"/>
		/// is invoked on the <paramref name="syncContext"/> specified.
		/// </summary>
		/// <remarks>
		/// The <paramref name="continuation"/> is invoked on a <see cref="SynchronizationContext"/> specified. Throwing an exception from the callback might cause unspecified behaviour.
		/// </remarks>
		/// <param name="continuation">The callback to be executed when the operation has completed.</param>
		/// <param name="syncContext">If not <see langword="null"/> method attempts to marshal the continuation to the synchronization context.
		/// Otherwise the callback is invoked on a thread that initiated the operation completion.
		/// </param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="continuation"/> is <see langword="null"/>.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation has been disposed.</exception>
		public void AddCompletionCallback(IAsyncContinuation continuation, SynchronizationContext syncContext)
		{
			if (!TryAddCompletionCallback(continuation, syncContext))
			{
				InvokeCompletionCallback(continuation, syncContext);
			}
		}

		/// <summary>
		/// Attempts to add a continuation to be executed after the operation has finished. If the operation is already completed
		/// the method does nothing and just returns <see langword="false"/>.
		/// </summary>
		/// <remarks>
		/// The <paramref name="continuation"/> is invoked on a <see cref="SynchronizationContext"/> specified. Throwing an exception from the callback might cause unspecified behaviour.
		/// </remarks>
		/// <param name="continuation">The cotinuation to be executed when the operation has completed.</param>
		/// <param name="syncContext">If not <see langword="null"/> method attempts to marshal the continuation to the synchronization context.
		/// Otherwise the callback is invoked on a thread that initiated the operation completion.
		/// </param>
		/// <returns>Returns <see langword="true"/> if the callback was added; <see langword="false"/> otherwise (the operation is completed).</returns>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="continuation"/> is <see langword="null"/>.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation has been disposed.</exception>
		public bool TryAddCompletionCallback(IAsyncContinuation continuation, SynchronizationContext syncContext)
		{
			ThrowIfDisposed();

			if (continuation == null)
			{
				throw new ArgumentNullException(nameof(continuation));
			}

			return TryAddCallback(continuation, syncContext, true);
		}

		/// <summary>
		/// Removes an existing continuation.
		/// </summary>
		/// <param name="continuation">The continuation to remove. Can be <see langword="null"/>.</param>
		/// <returns>Returns <see langword="true"/> if <paramref name="continuation"/> was removed; <see langword="false"/> otherwise.</returns>
		public bool RemoveCompletionCallback(IAsyncContinuation continuation)
		{
			return TryRemoveCallback(continuation);
		}

		/// <summary>
		/// Adds a callback to be executed when the operation progress has changed. If the operation is already completed the <paramref name="action"/> is called synchronously.
		/// </summary>
		/// <remarks>
		/// The <paramref name="action"/> is invoked on a thread that registered the callback (if it has a <see cref="SynchronizationContext"/> attached).
		/// Throwing an exception from the callback might cause unspecified behaviour.
		/// </remarks>
		/// <param name="action">The callback to be executed when the operation progress has changed.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="action"/> is <see langword="null"/>.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation has been disposed.</exception>
		public void AddProgressCallback(Action<float> action)
		{
			AddProgressCallback(action, SynchronizationContext.Current);
		}

		/// <summary>
		/// Attempts to add a callback to be executed when the operation progress has changed. If the operation is already completed
		/// the method does nothing and just returns <see langword="false"/>.
		/// </summary>
		/// <remarks>
		/// The <paramref name="action"/> is invoked on a thread that registered the callback (if it has a <see cref="SynchronizationContext"/> attached).
		/// Throwing an exception from the callback might cause unspecified behaviour.
		/// </remarks>
		/// <param name="action">The callback to be executed when the operation progress has changed.</param>
		/// <returns>Returns <see langword="true"/> if the callback was added; <see langword="false"/> otherwise (the operation is completed).</returns>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="action"/> is <see langword="null"/>.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation has been disposed.</exception>
		public bool TryAddProgressCallback(Action<float> action)
		{
			return TryAddProgressCallback(action, SynchronizationContext.Current);
		}

		/// <summary>
		/// Adds a callback to be executed when the operation progress has changed. If the operation is completed <paramref name="action"/> is invoked
		/// on the <paramref name="syncContext"/> specified.
		/// </summary>
		/// <remarks>
		/// The <paramref name="action"/> is invoked on a <see cref="SynchronizationContext"/> specified. Throwing an exception from the callback might cause unspecified behaviour.
		/// </remarks>
		/// <param name="action">The callback to be executed when the operation progress has changed.</param>
		/// <param name="syncContext">If not <see langword="null"/> method attempts to marshal the continuation to the synchronization context.
		/// Otherwise the callback is invoked on a thread that initiated the operation.
		/// </param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="action"/> is <see langword="null"/>.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation has been disposed.</exception>
		public void AddProgressCallback(Action<float> action, SynchronizationContext syncContext)
		{
			if (!TryAddProgressCallback(action, syncContext))
			{
				InvokeProgressCallback(action, syncContext);
			}
		}

		/// <summary>
		/// Attempts to add a callback to be executed when the operation progress has changed. If the operation is already completed
		/// the method does nothing and just returns <see langword="false"/>.
		/// </summary>
		/// <remarks>
		/// The <paramref name="action"/> is invoked on a <see cref="SynchronizationContext"/> specified. Throwing an exception from the callback might cause unspecified behaviour.
		/// </remarks>
		/// <param name="action">The callback to be executed when the operation progress has changed.</param>
		/// <param name="syncContext">If not <see langword="null"/> method attempts to marshal the callback to the synchronization context.
		/// Otherwise the callback is invoked on a thread that initiated the operation.
		/// </param>
		/// <returns>Returns <see langword="true"/> if the callback was added; <see langword="false"/> otherwise (the operation is completed).</returns>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="action"/> is <see langword="null"/>.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation has been disposed.</exception>
		public bool TryAddProgressCallback(Action<float> action, SynchronizationContext syncContext)
		{
			ThrowIfDisposed();

			if (action == null)
			{
				throw new ArgumentNullException(nameof(action));
			}

			return TryAddCallback(action, syncContext, false);
		}

		/// <summary>
		/// Removes an existing progress callback.
		/// </summary>
		/// <param name="action">The callback to remove. Can be <see langword="null"/>.</param>
		/// <returns>Returns <see langword="true"/> if <paramref name="action"/> was removed; <see langword="false"/> otherwise.</returns>
		public bool RemoveProgressCallback(Action<float> action)
		{
			return TryRemoveCallback(action);
		}

#if !NET35

		/// <summary>
		/// Adds a callback to be executed each time progress value changes. If the operation is completed <paramref name="callback"/> is invoked synchronously.
		/// </summary>
		/// <remarks>
		/// The <paramref name="callback"/> is invoked on a thread that registered it (if it has a <see cref="SynchronizationContext"/> attached).
		/// Throwing an exception from the callback might cause unspecified behaviour.
		/// </remarks>
		/// <param name="callback">The callback to be executed when the operation progress value has changed.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="callback"/> is <see langword="null"/>.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation has been disposed.</exception>
		public void AddProgressCallback(IProgress<float> callback)
		{
			AddProgressCallback(callback, SynchronizationContext.Current);
		}

		/// <summary>
		/// Attempts to add a progress callback to be executed each time progress value changes. If the operation is already completed
		/// the method does nothing and just returns <see langword="false"/>.
		/// </summary>
		/// <remarks>
		/// The <paramref name="callback"/> is invoked on a thread that registered the continuation (if it has a <see cref="SynchronizationContext"/> attached).
		/// Throwing an exception from the callback might cause unspecified behaviour.
		/// </remarks>
		/// <param name="callback">The callback to be executed when the operation progress value has changed.</param>
		/// <returns>Returns <see langword="true"/> if the callback was added; <see langword="false"/> otherwise (the operation is completed).</returns>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="callback"/> is <see langword="null"/>.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation has been disposed.</exception>
		public bool TryAddProgressCallback(IProgress<float> callback)
		{
			return TryAddProgressCallback(callback, SynchronizationContext.Current);
		}

		/// <summary>
		/// Adds a callback to be executed each time progress value changes. If the operation is completed <paramref name="callback"/>
		/// is invoked on the <paramref name="syncContext"/> specified.
		/// </summary>
		/// <remarks>
		/// The <paramref name="callback"/> is invoked on a <see cref="SynchronizationContext"/> specified. Throwing an exception from the callback might cause unspecified behaviour.
		/// </remarks>
		/// <param name="callback">The callback to be executed when the operation progress value has changed.</param>
		/// <param name="syncContext">If not <see langword="null"/> method attempts to marshal the continuation to the synchronization context.
		/// Otherwise the callback is invoked on a thread that initiated the operation completion.
		/// </param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="callback"/> is <see langword="null"/>.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation has been disposed.</exception>
		public void AddProgressCallback(IProgress<float> callback, SynchronizationContext syncContext)
		{
			if (!TryAddProgressCallback(callback, syncContext))
			{
				InvokeProgressCallback(callback, syncContext);
			}
		}

		/// <summary>
		/// Attempts to add a progress callback to be executed each time progress value changes. If the operation is already completed
		/// the method does nothing and just returns <see langword="false"/>.
		/// </summary>
		/// <remarks>
		/// The <paramref name="callback"/> is invoked on a <see cref="SynchronizationContext"/> specified. Throwing an exception from the callback might cause unspecified behaviour.
		/// </remarks>
		/// <param name="callback">The callback to be executed when the operation progress value has changed.</param>
		/// <param name="syncContext">If not <see langword="null"/> method attempts to marshal the callback to the synchronization context.
		/// Otherwise the callback is invoked on a thread that initiated the operation completion.
		/// </param>
		/// <returns>Returns <see langword="true"/> if the callback was added; <see langword="false"/> otherwise (the operation is completed).</returns>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="callback"/> is <see langword="null"/>.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation has been disposed.</exception>
		public bool TryAddProgressCallback(IProgress<float> callback, SynchronizationContext syncContext)
		{
			ThrowIfDisposed();

			if (callback == null)
			{
				throw new ArgumentNullException(nameof(callback));
			}

			return TryAddCallback(callback, syncContext, false);
		}

		/// <summary>
		/// Removes an existing progress callback.
		/// </summary>
		/// <param name="callback">The callback to remove. Can be <see langword="null"/>.</param>
		/// <returns>Returns <see langword="true"/> if <paramref name="callback"/> was removed; <see langword="false"/> otherwise.</returns>
		public bool RemoveProgressCallback(IProgress<float> callback)
		{
			return TryRemoveCallback(callback);
		}

#endif

		#endregion

		#region implementation

		private bool TryAddCallback(object callbackToAdd, SynchronizationContext syncContext, bool completionCallback)
		{
			Debug.Assert(callbackToAdd != null);

			// NOTE: The code below is adapted from https://referencesource.microsoft.com/#mscorlib/system/threading/Tasks/Task.cs.
			var oldValue = _callback;

			// Quick return if the operation is completed.
			if (oldValue != _callbackCompletionSentinel)
			{
				// If no callback is stored yet, try to store it as _callback.
				if (oldValue == null)
				{
					var newValue = callbackToAdd;

					if (completionCallback)
					{
						if (syncContext != null)
						{
							newValue = CreateCallbackCollection(callbackToAdd, syncContext);
						}
					}
					else
					{
						var newList = CreateCallbackCollection(null, null);
						newList.AddProgressCallback(callbackToAdd, syncContext);
						newValue = newList;
					}

					oldValue = Interlocked.CompareExchange(ref _callback, newValue, null);

					// Quick return if exchange succeeded.
					if (oldValue == null)
					{
						return true;
					}
				}

				// Logic for the case where we were previously storing a single callback.
				if (oldValue != _callbackCompletionSentinel && !(oldValue is IAsyncCallbackCollection))
				{
					var newList = CreateCallbackCollection(oldValue, null);
					Interlocked.CompareExchange(ref _callback, newList, oldValue);

					// We might be racing against another thread converting the single into a list,
					// or we might be racing against operation completion, so resample "list" below.
				}

				// If list is null, it can only mean that _callbackCompletionSentinel has been exchanged
				// into _callback. Thus, the task has completed and we should return false from this method,
				// as we will not be queuing up the callback.
				if (_callback is IAsyncCallbackCollection list)
				{
					lock (list)
					{
						// It is possible for the operation to complete right after we snap the copy of the list.
						// If so, then fall through and return false without queuing the callback.
						if (_callback != _callbackCompletionSentinel)
						{
							if (completionCallback)
							{
								list.AddCompletionCallback(callbackToAdd, syncContext);
							}
							else
							{
								list.AddProgressCallback(callbackToAdd, syncContext);
							}

							return true;
						}
					}
				}
			}

			return false;
		}

		private bool TryRemoveCallback(object callbackToRemove)
		{
			if (callbackToRemove != null)
			{
				// NOTE: The code below is adapted from https://referencesource.microsoft.com/#mscorlib/system/threading/Tasks/Task.cs.
				var value = _callback;

				if (value != _callbackCompletionSentinel)
				{
					var list = value as IAsyncCallbackCollection;

					if (list == null)
					{
						// This is not a list. If we have a single object (the one we want to remove) we try to replace it with an empty list.
						// Note we cannot go back to a null state, since it will mess up the TryAddCallback() logic.
						if (Interlocked.CompareExchange(ref _callback, CreateCallbackCollection(null, null), callbackToRemove) == callbackToRemove)
						{
							return true;
						}
						else
						{
							// If we fail it means that either TryAddContinuation won the race condition and _callback is now a List
							// that contains the element we want to remove. Or it set the _callbackCompletionSentinel.
							// So we should try to get a list one more time.
							list = _callback as IAsyncCallbackCollection;
						}
					}

					// If list is null it means _callbackCompletionSentinel has been set already and there is nothing else to do.
					if (list != null)
					{
						lock (list)
						{
							// There is a small chance that the operation completed since we took a local snapshot into
							// list. In that case, just return; we don't want to be manipulating the callback list as it is being processed.
							if (_callback != _callbackCompletionSentinel)
							{
								return list.Remove(callbackToRemove);
							}
						}
					}
				}
			}

			return false;
		}

		private void InvokeProgressCallbacks()
		{
			var value = _callback;

			if (value != null)
			{
				if (value is IAsyncCallbackCollection callbackList)
				{
					lock (callbackList)
					{
						callbackList.InvokeProgressCallbacks();
					}
				}
				else
				{
					CallbackUtility.InvokeProgressCallback(this, value);
				}
			}
		}

		private void InvokeCallbacks()
		{
			var value = Interlocked.Exchange(ref _callback, _callbackCompletionSentinel);

			if (value != null)
			{
				var invokeAsync = (_flags & _flagRunContinuationsAsynchronously) != 0;

				if (value is IAsyncCallbackCollection callbackList)
				{
					lock (callbackList)
					{
						callbackList.Invoke(invokeAsync);
					}
				}
				else if (invokeAsync)
				{
					CallbackUtility.InvokeCompletionCallbackAsync(this, value, SynchronizationContext.Current);
				}
				else
				{
					CallbackUtility.InvokeCompletionCallback(this, value);
				}
			}
		}

		private void InvokeProgressCallback(object callback, SynchronizationContext syncContext)
		{
			Debug.Assert(callback != null);

			if (syncContext == null || syncContext == SynchronizationContext.Current)
			{
				CallbackUtility.InvokeProgressCallback(this, callback);
			}
			else
			{
				syncContext.Post(args => CallbackUtility.InvokeProgressCallback(this, args), callback);
			}
		}

		private void InvokeCompletionCallback(object continuation, SynchronizationContext syncContext)
		{
			Debug.Assert(continuation != null);

			if ((_flags & _flagRunContinuationsAsynchronously) != 0)
			{
				CallbackUtility.InvokeCompletionCallbackAsync(this, continuation, syncContext);
			}
			else if (syncContext == null || syncContext == SynchronizationContext.Current)
			{
				CallbackUtility.InvokeCompletionCallback(this, continuation);
			}
			else
			{
				syncContext.Post(args => CallbackUtility.InvokeCompletionCallback(this, args), continuation);
			}
		}

		private IAsyncCallbackCollection CreateCallbackCollection(object oldValue, SynchronizationContext syncContext)
		{
			if (oldValue != null)
			{
				return new MultiContextCallbackCollection(this, oldValue, syncContext);
			}

			return new MultiContextCallbackCollection(this);
		}

		#endregion
	}
}
