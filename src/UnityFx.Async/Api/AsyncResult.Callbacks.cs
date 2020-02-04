// Copyright (c) 2018-2020 Alexander Bogarsukov.
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
		#endregion

		#region interface
		#endregion

		#region IAsyncOperationCallbacks

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

				var syncContext = SynchronizationContext.Current ?? _defaultContext;

				if (!TryAddCallback(value, syncContext, false))
				{
					CallbackUtility.InvokeProgressCallback(this, value, syncContext);
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

				var syncContext = SynchronizationContext.Current ?? _defaultContext;

				if (!TryAddCallback(value, syncContext, true))
				{
					var invokeAsync = (_flags & _flagRunContinuationsAsynchronously) != 0;
					CallbackUtility.InvokeCompletionCallback(this, value, syncContext, invokeAsync);
				}
			}
			remove
			{
				TryRemoveCallback(value);
			}
		}

		/// <summary>
		/// Adds a completion callback to be executed after the operation has completed. If the operation is completed <paramref name="action"/> is invoked
		/// on the <paramref name="syncContext"/> specified.
		/// </summary>
		/// <remarks>
		/// The <paramref name="action"/> is invoked on a <see cref="SynchronizationContext"/> specified. Throwing an exception from the callback might cause unspecified behaviour.
		/// </remarks>
		/// <param name="action">The callback to be executed when the operation has completed. Can be one of <see cref="Action"/>, <see cref="Action{T}"/>
		/// (with <see cref="IAsyncOperation"/> argument type), <see cref="AsyncCallback"/>, <see cref="IAsyncContinuation"/> or <see cref="AsyncCompletedEventHandler"/>.</param>
		/// <param name="syncContext">If not <see langword="null"/> method attempts to marshal the continuation to the synchronization context.
		/// Otherwise the callback is invoked on a thread that initiated the operation completion.
		/// </param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="action"/> is <see langword="null"/>.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation has been disposed.</exception>
		/// <seealso cref="AddProgressCallback(object, SynchronizationContext)"/>
		/// <seealso cref="RemoveCallback(object)"/>
		public void AddCompletionCallback(object action, SynchronizationContext syncContext)
		{
			ThrowIfDisposed();

			if (action == null)
			{
				throw new ArgumentNullException(nameof(action));
			}

			if (!TryAddCallback(action, syncContext, true))
			{
				var invokeAsync = (_flags & _flagRunContinuationsAsynchronously) != 0;
				CallbackUtility.InvokeCompletionCallback(this, action, syncContext, invokeAsync);
			}
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
		/// <seealso cref="AddCompletionCallback(object, SynchronizationContext)"/>
		/// <seealso cref="RemoveCallback(object)"/>
		public void AddProgressCallback(object action, SynchronizationContext syncContext)
		{
			ThrowIfDisposed();

			if (action == null)
			{
				throw new ArgumentNullException(nameof(action));
			}

			if (!TryAddCallback(action, syncContext, false))
			{
				CallbackUtility.InvokeProgressCallback(this, action, syncContext);
			}
		}

		/// <summary>
		/// Removes an existing completion/progress callback.
		/// </summary>
		/// <param name="action">The callback to remove. Can be <see langword="null"/>.</param>
		/// <returns>Returns <see langword="true"/> if <paramref name="action"/> was removed; <see langword="false"/> otherwise.</returns>
		/// <seealso cref="AddCompletionCallback(object, SynchronizationContext)"/>
		/// <seealso cref="AddProgressCallback(object, SynchronizationContext)"/>
		public bool RemoveCallback(object action)
		{
			return TryRemoveCallback(action);
		}

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
							// An optimization for single-threaded applications (Unity as an instance):
							// in most cases we expect continuations to run on _defaultContext, and if it
							// is the case we just set a flag instead of allocating callback callection.
							if (syncContext == _defaultContext)
							{
								TrySetFlag(_flagContinueOnDefaultContext);
							}
							else
							{
								var newList = CreateCallbackCollection();
								newList.AddCompletionCallback(callbackToAdd, syncContext);
								newValue = newList;
							}
						}
					}
					else
					{
						// Always create a collection instance for progress callbacks.
						var newList = CreateCallbackCollection();
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
					var newList = CreateCallbackCollection();
					newList.AddCompletionCallback(oldValue, null);
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
						if (Interlocked.CompareExchange(ref _callback, CreateCallbackCollection(), callbackToRemove) == callbackToRemove)
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

			if (value is IAsyncCallbackCollection callbackList)
			{
				lock (callbackList)
				{
					callbackList.InvokeProgressCallbacks();
				}
			}
		}

		private void InvokeCallbacks()
		{
			var value = Interlocked.Exchange(ref _callback, _callbackCompletionSentinel);

			if (value != null)
			{
				var invokeAsync = (_flags & _flagRunContinuationsAsynchronously) != 0;
				var continueOnDefaultContext = (_flags & _flagContinueOnDefaultContext) != 0;

				if (value is IAsyncCallbackCollection callbackList)
				{
					lock (callbackList)
					{
						callbackList.Invoke(invokeAsync);
					}
				}
				else
				{
					var syncContext = continueOnDefaultContext ? _defaultContext : SynchronizationContext.Current;
					CallbackUtility.InvokeCompletionCallback(this, value, syncContext, invokeAsync);
				}
			}
		}

		private IAsyncCallbackCollection CreateCallbackCollection()
		{
			return new MultiContextCallbackCollection(this);
		}

		#endregion
	}
}
