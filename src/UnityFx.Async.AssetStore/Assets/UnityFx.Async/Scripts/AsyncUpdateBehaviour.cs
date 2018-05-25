// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityFx.Async
{
	/// <summary>
	/// A default implementation of <see cref="IAsyncUpdateSource"/>.
	/// </summary>
	public class AsyncUpdateBehaviour : MonoBehaviour, IAsyncUpdateSource
	{
		#region data

		private List<Action> _updateCallbacks;
		private List<Action> _updateCallbacksToRemove;

		private List<Action<float>> _updateCallbacks2;
		private List<Action<float>> _updateCallbacksToRemove2;

		private List<IAsyncUpdatable> _updatables;
		private List<IAsyncUpdatable> _updatablesToRemove;

#if !NET35 && !NET_2_0 && !NET_2_0_SUBSET

		private List<IObserver<float>> _observers;
		private List<IObserver<float>> _observersToRemove;

#endif

		private bool _updating;

		#endregion

		#region interface
		#endregion

		#region MonoBehavoiur

		private void Update()
		{
			try
			{
				_updating = true;

				if (_updateCallbacks != null)
				{
					foreach (var callback in _updateCallbacks)
					{
						callback();
					}

					foreach (var callback in _updateCallbacksToRemove)
					{
						_updateCallbacksToRemove.Remove(callback);
					}

					_updateCallbacksToRemove.Clear();
				}

				if (_updateCallbacks2 != null)
				{
					var frameTime = Time.deltaTime;

					foreach (var callback in _updateCallbacks2)
					{
						callback(frameTime);
					}

					foreach (var callback in _updateCallbacksToRemove2)
					{
						_updateCallbacksToRemove2.Remove(callback);
					}

					_updateCallbacksToRemove2.Clear();
				}

				if (_updatables != null)
				{
					var frameTime = Time.deltaTime;

					foreach (var item in _updatables)
					{
						item.Update(frameTime);
					}

					foreach (var item in _updatablesToRemove)
					{
						_updatablesToRemove.Remove(item);
					}

					_updatablesToRemove.Clear();
				}

#if !NET35 && !NET_2_0 && !NET_2_0_SUBSET

				if (_observers != null)
				{
					var frameTime = Time.deltaTime;

					foreach (var item in _observers)
					{
						item.OnNext(frameTime);
					}

					foreach (var item in _observersToRemove)
					{
						_observersToRemove.Remove(item);
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

		private void OnDestroy()
		{
#if !NET35 && !NET_2_0 && !NET_2_0_SUBSET

			if (_observers != null)
			{
				foreach (var item in _observers)
				{
					item.OnCompleted();
				}

				_observers.Clear();
			}

#endif
		}

		#endregion

		#region IAsyncUpdateSource

		/// <inheritdoc/>
		public void AddListener(Action updateCallback)
		{
			if (updateCallback == null)
			{
				throw new ArgumentNullException("updateCallback");
			}

			if (_updateCallbacks == null)
			{
				_updateCallbacks = new List<Action>();
				_updateCallbacksToRemove = new List<Action>();
			}

			_updateCallbacks.Add(updateCallback);
		}

		/// <inheritdoc/>
		public void RemoveListener(Action updateCallback)
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

		/// <inheritdoc/>
		public void AddListener(Action<float> updateCallback)
		{
			if (updateCallback == null)
			{
				throw new ArgumentNullException("updateCallback");
			}

			if (_updateCallbacks2 == null)
			{
				_updateCallbacks2 = new List<Action<float>>();
				_updateCallbacksToRemove2 = new List<Action<float>>();
			}

			_updateCallbacks2.Add(updateCallback);
		}

		/// <inheritdoc/>
		public void RemoveListener(Action<float> updateCallback)
		{
			if (_updating)
			{
				if (updateCallback != null)
				{
					_updateCallbacksToRemove2.Add(updateCallback);
				}
			}
			else
			{
				_updateCallbacks2.Remove(updateCallback);
			}
		}

		/// <inheritdoc/>
		public void AddListener(IAsyncUpdatable updatable)
		{
			if (updatable == null)
			{
				throw new ArgumentNullException("updatable");
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

		#endregion

		#region IObservable

#if !NET35 && !NET_2_0 && !NET_2_0_SUBSET

		private class ObservableSubscription : IDisposable
		{
			private readonly IObserver<float> _observer;
			private readonly AsyncUpdateBehaviour _parent;

			public ObservableSubscription(AsyncUpdateBehaviour parent, IObserver<float> observer)
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
			if (_updating)
			{
				_observersToRemove.Add(observer);
			}
			else
			{
				_observers.Remove(observer);
			}
		}

		/// <inheritdoc/>
		public IDisposable Subscribe(IObserver<float> observer)
		{
			if (observer == null)
			{
				throw new ArgumentNullException("observer");
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
	}
}
