// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections;
using System.Collections.Generic;

namespace UnityFx.Async
{
	/// <summary>
	/// Implementation of <see cref="IAsyncUpdateSource"/>.
	/// </summary>
#if NET35
	public class AsyncUpdateSource : IAsyncUpdateSource, IDisposable
#else
	public class AsyncUpdateSource : IAsyncUpdateSource, IObserver<float>, IDisposable
#endif
	{
		#region data

		private List<Action<float>> _updateCallbacks;
		private List<Action<float>> _updateCallbacksToRemove;

		private List<IAsyncUpdatable> _updatables;
		private List<IAsyncUpdatable> _updatablesToRemove;

#if !NET35

		private List<IObserver<float>> _observers;
		private List<IObserver<float>> _observersToRemove;

#endif

		private bool _updating;
		private bool _disposed;

		#endregion

		#region interface
		#endregion

		#region IAsyncUpdateSource

		/// <inheritdoc/>
		public void AddListener(Action<float> updateCallback)
		{
			ThrowIfDisposed();

			if (updateCallback == null)
			{
				throw new ArgumentNullException(nameof(updateCallback));
			}

			if (_updateCallbacks == null)
			{
				_updateCallbacks = new List<Action<float>>();
				_updateCallbacksToRemove = new List<Action<float>>();
			}

			_updateCallbacks.Add(updateCallback);
		}

		/// <inheritdoc/>
		public void RemoveListener(Action<float> updateCallback)
		{
			if (_updateCallbacks != null)
			{
				if (_updating)
				{
					if (updateCallback != null)
					{
						_updateCallbacksToRemove.Add(updateCallback);
					}
				}
				else
				{
					_updateCallbacks.Remove(updateCallback);
				}
			}
		}

		/// <inheritdoc/>
		public void AddListener(IAsyncUpdatable updatable)
		{
			ThrowIfDisposed();

			if (updatable == null)
			{
				throw new ArgumentNullException(nameof(updatable));
			}

			if (_updatables == null)
			{
				_updatables = new List<IAsyncUpdatable>();
				_updatablesToRemove = new List<IAsyncUpdatable>();
			}

			_updatables.Add(updatable);
		}

		/// <inheritdoc/>
		public void RemoveListener(IAsyncUpdatable updatable)
		{
			if (_updatables != null)
			{
				if (_updating)
				{
					if (updatable != null)
					{
						_updatablesToRemove.Add(updatable);
					}
				}
				else
				{
					_updatables.Remove(updatable);
				}
			}
		}

		#endregion

		#region IObservable

#if !NET35

		private class ObservableSubscription : IDisposable
		{
			private readonly IObserver<float> _observer;
			private readonly AsyncUpdateSource _parent;

			public ObservableSubscription(AsyncUpdateSource parent, IObserver<float> observer)
			{
				_parent = parent;
				_observer = observer;
				_parent._observers.Add(observer);
			}

			public void Dispose()
			{
				_parent.RemoveListener(_observer);
			}
		}

		private void RemoveListener(IObserver<float> observer)
		{
			if (_observers != null)
			{
				if (_updating)
				{
					_observersToRemove.Add(observer);
				}
				else
				{
					_observers.Remove(observer);
				}
			}
		}

		/// <inheritdoc/>
		public IDisposable Subscribe(IObserver<float> observer)
		{
			ThrowIfDisposed();

			if (observer == null)
			{
				throw new ArgumentNullException(nameof(observer));
			}

			if (_observers == null)
			{
				_observers = new List<IObserver<float>>();
				_observersToRemove = new List<IObserver<float>>();
			}

			return new ObservableSubscription(this, observer);
		}

#endif

		#endregion

		#region IObserver

		/// <inheritdoc/>
		public void OnNext(float frameTime)
		{
			ThrowIfDisposed();

			try
			{
				_updating = true;

				if (_updateCallbacks != null)
				{
					foreach (var callback in _updateCallbacks)
					{
						callback(frameTime);
					}

					foreach (var callback in _updateCallbacksToRemove)
					{
						_updateCallbacks.Remove(callback);
					}

					_updateCallbacksToRemove.Clear();
				}

				if (_updatables != null)
				{
					foreach (var item in _updatables)
					{
						item.Update(frameTime);
					}

					foreach (var item in _updatablesToRemove)
					{
						_updatables.Remove(item);
					}

					_updatablesToRemove.Clear();
				}

#if !NET35

				if (_observers != null)
				{
					foreach (var item in _observers)
					{
						item.OnNext(frameTime);
					}

					foreach (var item in _observersToRemove)
					{
						_observers.Remove(item);
					}

					_observersToRemove.Clear();
				}

#endif
			}
			finally
			{
				_updating = false;
			}
		}

		/// <inheritdoc/>
		public void OnCompleted()
		{
			ThrowIfDisposed();

			_updateCallbacks?.Clear();
			_updatables?.Clear();

#if !NET35

			if (_observers != null)
			{
				_updating = true;

				try
				{
					foreach (var item in _observers)
					{
						item.OnCompleted();
					}
				}
				finally
				{
					_observersToRemove.Clear();
					_observers.Clear();
					_updating = false;
				}
			}

#endif
		}

		/// <inheritdoc/>
		public void OnError(Exception e)
		{
			ThrowIfDisposed();

			_updateCallbacks?.Clear();
			_updatables?.Clear();

#if !NET35

			if (_observers != null)
			{
				_updating = true;

				try
				{
					foreach (var item in _observers)
					{
						item.OnError(e);
					}
				}
				finally
				{
					_observersToRemove.Clear();
					_observers.Clear();
					_updating = false;
				}
			}

#endif
		}

		#endregion

		#region IDisposable

		/// <inheritdoc/>
		public void Dispose()
		{
			if (!_disposed)
			{
				_disposed = true;

				_updateCallbacks = null;
				_updatables = null;

#if !NET35

				if (_observers != null)
				{
					_updating = true;

					try
					{
						foreach (var item in _observers)
						{
							item.OnCompleted();
						}
					}
					finally
					{
						_observersToRemove = null;
						_observers = null;
						_updating = false;
					}
				}

#endif
			}
		}

		#endregion

		#region implementation

		private void ThrowIfDisposed()
		{
			if (_disposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
		}

		#endregion
	}
}
