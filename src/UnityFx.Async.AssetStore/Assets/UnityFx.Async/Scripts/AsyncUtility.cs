// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_5_4_OR_NEWER || UNITY_2017 || UNITY_2018
using UnityEngine.Networking;
#elif UNITY_5_2_OR_NEWER
using UnityEngine.Experimental.Networking;
#endif

namespace UnityFx.Async
{
	/// <summary>
	/// Utility classes.
	/// </summary>
	public static class AsyncUtility
	{
		#region data

		private static GameObject _go;

		#endregion

		#region interface

		/// <summary>
		/// Returns a <see cref="GameObject"/> used by the library tools.
		/// </summary>
		public static GameObject GetRootGo()
		{
			if (!_go)
			{
				_go = GameObject.Find("UnityFx.Async");

				if (!_go)
				{
					_go = new GameObject("UnityFx.Async");
					GameObject.DontDestroyOnLoad(_go);
				}
			}

			return _go;
		}

		/// <summary>
		/// Returns an instance of an <see cref="IAsyncUpdateSource"/>.
		/// </summary>
		public static IAsyncUpdateSource GetDefaultUpdateSource()
		{
			return GetCoroutineRunner();
		}

		/// <summary>
		/// Starts a coroutine.
		/// </summary>
		/// <param name="enumerator">The coroutine to run.</param>
		public static Coroutine StartCoroutine(IEnumerator enumerator)
		{
			if (enumerator == null)
			{
				throw new ArgumentNullException("enumerator");
			}

			return GetCoroutineRunner().StartCoroutine(enumerator);
		}

		/// <summary>
		/// Stops the specified coroutine.
		/// </summary>
		/// <param name="coroutine">The coroutine to run.</param>
		public static void StopCoroutine(Coroutine coroutine)
		{
			if (coroutine != null)
			{
				var go = GetRootGo();
				var runner = go.GetComponent<CoroutineRunner>();

				if (runner)
				{
					runner.StopCoroutine(coroutine);
				}
			}
		}

		/// <summary>
		/// Stops the specified coroutine.
		/// </summary>
		/// <param name="enumerator">The coroutine to run.</param>
		public static void StopCoroutine(IEnumerator enumerator)
		{
			if (enumerator != null)
			{
				var go = GetRootGo();
				var runner = go.GetComponent<CoroutineRunner>();

				if (runner)
				{
					runner.StopCoroutine(enumerator);
				}
			}
		}

		/// <summary>
		/// Stops all coroutines.
		/// </summary>
		public static void StopAllCoroutines()
		{
			var go = GetRootGo();
			var runner = go.GetComponent<CoroutineRunner>();

			if (runner)
			{
				runner.StopAllCoroutines();
			}
		}

		/// <summary>
		/// Adds a new delegate that is called once per update cycle.
		/// </summary>
		/// <param name="updateCallback">The update callback to add.</param>
		public static void AddUpdateCallback(Action updateCallback)
		{
			GetCoroutineRunner().AddListener(updateCallback);
		}

		/// <summary>
		/// Removes an existing update callback.
		/// </summary>
		/// <param name="updateCallback">The update callback to remove.</param>
		public static void RemoveUpdateCallback(Action updateCallback)
		{
			if (updateCallback != null)
			{
				var go = GetRootGo();
				var runner = go.GetComponent<CoroutineRunner>();

				if (runner)
				{
					runner.RemoveListener(updateCallback);
				}
			}
		}

		/// <summary>
		/// Adds a new delegate that is called once per update cycle.
		/// </summary>
		/// <param name="updateCallback">The update callback to add.</param>
		public static void AddUpdateCallback(Action<float> updateCallback)
		{
			GetCoroutineRunner().AddListener(updateCallback);
		}

		/// <summary>
		/// Removes an existing update callback.
		/// </summary>
		/// <param name="updateCallback">The update callback to remove.</param>
		public static void RemoveUpdateCallback(Action<float> updateCallback)
		{
			if (updateCallback != null)
			{
				var go = GetRootGo();
				var runner = go.GetComponent<CoroutineRunner>();

				if (runner)
				{
					runner.RemoveListener(updateCallback);
				}
			}
		}

		/// <summary>
		/// Register a completion callback for the specified <see cref="AsyncOperation"/> instance.
		/// </summary>
		/// <param name="op">The request to register completion callback for.</param>
		/// <param name="completionCallback">A delegate to be called when the <paramref name="op"/> has completed.</param>
		public static void AddCompletionCallback(AsyncOperation op, Action completionCallback)
		{
			if (op == null)
			{
				throw new ArgumentNullException("op");
			}

			if (completionCallback == null)
			{
				throw new ArgumentNullException("completionCallback");
			}

			GetCoroutineRunner().AddCompletionCallback(op, completionCallback);
		}

#if UNITY_5_2_OR_NEWER || UNITY_5_3_OR_NEWER || UNITY_2017 || UNITY_2018

		/// <summary>
		/// Register a completion callback for the specified <see cref="UnityWebRequest"/> instance.
		/// </summary>
		/// <param name="request">The request to register completion callback for.</param>
		/// <param name="completionCallback">A delegate to be called when the <paramref name="request"/> has completed.</param>
		public static void AddCompletionCallback(UnityWebRequest request, Action completionCallback)
		{
			if (request == null)
			{
				throw new ArgumentNullException("request");
			}

			if (completionCallback == null)
			{
				throw new ArgumentNullException("completionCallback");
			}

			GetCoroutineRunner().AddCompletionCallback(request, completionCallback);
		}

#endif

		/// <summary>
		/// Register a completion callback for the specified <see cref="WWW"/> instance.
		/// </summary>
		/// <param name="request">The request to register completion callback for.</param>
		/// <param name="completionCallback">A delegate to be called when the <paramref name="request"/> has completed.</param>
		internal static void AddCompletionCallback(WWW request, Action completionCallback)
		{
			if (request == null)
			{
				throw new ArgumentNullException("request");
			}

			if (completionCallback == null)
			{
				throw new ArgumentNullException("completionCallback");
			}

			GetCoroutineRunner().AddCompletionCallback(request, completionCallback);
		}

		#endregion

		#region implementation

		private class CoroutineRunner : MonoBehaviour, IAsyncUpdateSource
		{
			#region data

			private Dictionary<object, Action> _ops;
			private List<object> _opsToRemove;

			private List<Action> _updateCallbacks;
			private List<Action> _updateCallbacksToRemove;

			private List<Action<float>> _updateCallbacks2;
			private List<Action<float>> _updateCallbacksToRemove2;

			private List<IAsyncUpdatable> _updatables;
			private List<IAsyncUpdatable> _updatablesToRemove;

			private bool _updating;

			#endregion

			#region interface

			public void AddCompletionCallback(object op, Action cb)
			{
				if (_ops == null)
				{
					_ops = new Dictionary<object, Action>();
					_opsToRemove = new List<object>();
				}

				_ops.Add(op, cb);
			}

			#endregion

			#region MonoBehavoiur

			private void Update()
			{
				try
				{
					_updating = true;

					if (_ops != null && _ops.Count > 0)
					{
						foreach (var item in _ops)
						{
							if (item.Key is AsyncOperation)
							{
								var asyncOp = item.Key as AsyncOperation;

								if (asyncOp.isDone)
								{
									item.Value();
									_opsToRemove.Add(asyncOp);
								}
							}
#if UNITY_5_2_OR_NEWER || UNITY_5_3_OR_NEWER || UNITY_2017 || UNITY_2018
							else if (item.Key is UnityWebRequest)
							{
								var asyncOp = item.Key as UnityWebRequest;

								if (asyncOp.isDone)
								{
									item.Value();
									_opsToRemove.Add(asyncOp);
								}
							}
#endif
							else if (item.Key is WWW)
							{
								var asyncOp = item.Key as WWW;

								if (asyncOp.isDone)
								{
									item.Value();
									_opsToRemove.Add(asyncOp);
								}
							}
						}

						foreach (var item in _opsToRemove)
						{
							_ops.Remove(item);
						}

						_opsToRemove.Clear();
					}

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

			private List<IObserver<float>> _observers;
			private List<IObserver<float>> _observersToRemove;

			private class ObservableSubscription : IDisposable
			{
				private readonly IObserver<float> _observer;
				private readonly CoroutineRunner _parent;

				public ObservableSubscription(CoroutineRunner runner, IObserver<float> observer)
				{
					_parent = runner;
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

		private static CoroutineRunner GetCoroutineRunner()
		{
			var go = GetRootGo();
			var runner = go.GetComponent<CoroutineRunner>();

			if (!runner)
			{
				runner = go.AddComponent<CoroutineRunner>();
			}

			return runner;
		}

		#endregion
	}
}
