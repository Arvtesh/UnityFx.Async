// Copyright (c) 2018-2020 Alexander Bogarsukov.
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

		private List<object> _tmpList = new List<object>();
		private List<Action<float>> _updateCallbacks;
		private List<IAsyncUpdatable> _updatables;

#if !NET35

		private List<IObserver<float>> _observers;

#endif

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
			}

			_updateCallbacks.Add(updateCallback);
		}

		/// <inheritdoc/>
		public void RemoveListener(Action<float> updateCallback)
		{
			if (_updateCallbacks != null)
			{
				_updateCallbacks.Remove(updateCallback);
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
			}

			_updatables.Add(updatable);
		}

		/// <inheritdoc/>
		public void RemoveListener(IAsyncUpdatable updatable)
		{
			if (_updatables != null)
			{
				_updatables.Remove(updatable);
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
				_observers.Remove(observer);
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

			if (_updateCallbacks != null && _updateCallbacks.Count > 0)
			{
				_tmpList.Clear();

				foreach (var item in _updateCallbacks)
				{
					_tmpList.Add(item);
				}

				foreach (var callback in _tmpList)
				{
					((Action<float>)callback).Invoke(frameTime);
				}
			}

			if (_updatables != null && _updatables.Count > 0)
			{
				_tmpList.Clear();

				foreach (var item in _updatables)
				{
					_tmpList.Add(item);
				}

				foreach (var item in _tmpList)
				{
					((IAsyncUpdatable)item).Update(frameTime);
				}
			}

#if !NET35

			if (_observers != null && _observers.Count > 0)
			{
				_tmpList.Clear();

				foreach (var item in _observers)
				{
					_tmpList.Add(item);
				}

				foreach (var item in _tmpList)
				{
					((IObserver<float>)item).OnNext(frameTime);
				}
			}

#endif
		}

		/// <inheritdoc/>
		public void OnCompleted()
		{
			ThrowIfDisposed();

			_updateCallbacks?.Clear();
			_updatables?.Clear();

#if !NET35

			if (_observers != null && _observers.Count > 0)
			{
				try
				{
					_tmpList.Clear();

					foreach (var item in _observers)
					{
						_tmpList.Add(item);
					}

					foreach (var item in _tmpList)
					{
						((IObserver<float>)item).OnCompleted();
					}
				}
				finally
				{
					_observers.Clear();
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

			if (_observers != null && _observers.Count > 0)
			{
				try
				{
					_tmpList.Clear();

					foreach (var item in _observers)
					{
						_tmpList.Add(item);
					}

					foreach (var item in _tmpList)
					{
						((IObserver<float>)item).OnError(e);
					}
				}
				finally
				{
					_observers.Clear();
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

				if (_observers != null && _observers.Count > 0)
				{
					try
					{
						_tmpList.Clear();

						foreach (var item in _observers)
						{
							_tmpList.Add(item);
						}

						foreach (var item in _tmpList)
						{
							((IObserver<float>)item).OnCompleted();
						}
					}
					finally
					{
						_observers = null;
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
