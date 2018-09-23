// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections;
using System.Collections.Generic;
#if NET_4_6 || NET_STANDARD_2_0
using System.Collections.Concurrent;
#endif
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;
#if UNITY_5_4_OR_NEWER
using UnityEngine.Networking;
#endif

namespace UnityFx.Async
{
	/// <summary>
	/// Utility classes.
	/// </summary>
	public static class AsyncUtility
	{
		#region data

		private static SynchronizationContext _mainThreadContext;
#if NET_4_6 || NET_STANDARD_2_0
		private static ConcurrentQueue<InvokeResult> _actionQueue;
#else
		private static Queue<InvokeResult> _actionQueue;
#endif
		private static GameObject _go;
		private static AsyncRootBehaviour _rootBehaviour;

		#endregion

		#region interface

		/// <summary>
		/// Name of a <see cref="GameObject"/> used by the library tools.
		/// </summary>
		public const string RootGoName = "UnityFx.Async";

		/// <summary>
		/// Returns a <see cref="GameObject"/> used by the library tools.
		/// </summary>
		public static GameObject GetRootGo()
		{
			if (ReferenceEquals(_go, null))
			{
				_go = new GameObject(RootGoName);
				GameObject.DontDestroyOnLoad(_go);
			}

			return _go;
		}

		/// <summary>
		/// Returns an instance of an <see cref="IAsyncUpdateSource"/> for Update.
		/// </summary>
		/// <seealso cref="GetLateUpdateSource"/>
		/// <seealso cref="GetFixedUpdateSource"/>
		/// <seealso cref="GetEndOfFrameUpdateSource"/>
		public static IAsyncUpdateSource GetUpdateSource()
		{
			return GetRootBehaviour().UpdateSource;
		}

		/// <summary>
		/// Returns an instance of an <see cref="IAsyncUpdateSource"/> for LateUpdate.
		/// </summary>
		/// <seealso cref="GetUpdateSource"/>
		/// <seealso cref="GetFixedUpdateSource"/>
		/// <seealso cref="GetEndOfFrameUpdateSource"/>
		public static IAsyncUpdateSource GetLateUpdateSource()
		{
			return GetRootBehaviour().LateUpdateSource;
		}

		/// <summary>
		/// Returns an instance of an <see cref="IAsyncUpdateSource"/> for FixedUpdate.
		/// </summary>
		/// <seealso cref="GetUpdateSource"/>
		/// <seealso cref="GetLateUpdateSource"/>
		/// <seealso cref="GetEndOfFrameUpdateSource"/>
		public static IAsyncUpdateSource GetFixedUpdateSource()
		{
			return GetRootBehaviour().FixedUpdateSource;
		}

		/// <summary>
		/// Returns an instance of an <see cref="IAsyncUpdateSource"/> for end of frame.
		/// </summary>
		/// <seealso cref="GetUpdateSource"/>
		/// <seealso cref="GetLateUpdateSource"/>
		/// <seealso cref="GetFixedUpdateSource"/>
		public static IAsyncUpdateSource GetEndOfFrameUpdateSource()
		{
			return GetRootBehaviour().EofUpdateSource;
		}

		/// <summary>
		/// Dispatches a synchronous message to the main thread.
		/// </summary>
		/// <param name="d">The delegate to invoke.</param>
		/// <param name="state">The object passed to the delegate.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="d"/> is <see langword="null"/>.</exception>
		/// <seealso cref="PostToMainThread(SendOrPostCallback, object)"/>
		/// <seealso cref="InvokeOnMainThread(SendOrPostCallback, object)"/>
		public static void SendToMainThread(SendOrPostCallback d, object state)
		{
			if (d == null)
			{
				throw new ArgumentNullException("d");
			}

			if (_mainThreadContext == SynchronizationContext.Current)
			{
				d.Invoke(state);
			}
			else
			{
				using (var asyncResult = new InvokeResult(d, state))
				{
#if NET_2_0 || NET_2_0_SUBSET

					lock (_actionQueue)
					{
						_actionQueue.Enqueue(asyncResult);
					}

#else

					_actionQueue.Enqueue(asyncResult);

#endif

					asyncResult.Wait();
				}
			}
		}

		/// <summary>
		/// Dispatches an asynchronous message to the main thread.
		/// </summary>
		/// <param name="d">The delegate to invoke.</param>
		/// <param name="state">The object passed to the delegate.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="d"/> is <see langword="null"/>.</exception>
		/// <seealso cref="SendToMainThread(SendOrPostCallback, object)"/>
		/// <seealso cref="InvokeOnMainThread(SendOrPostCallback, object)"/>
		public static IAsyncOperation PostToMainThread(SendOrPostCallback d, object state)
		{
			if (d == null)
			{
				throw new ArgumentNullException("d");
			}

			var asyncResult = new InvokeResult(d, state);

#if NET_2_0 || NET_2_0_SUBSET

			lock (_actionQueue)
			{
				_actionQueue.Enqueue(asyncResult);
			}

#else

			_actionQueue.Enqueue(asyncResult);

#endif

			return asyncResult;
		}

		/// <summary>
		/// Dispatches the specified delegate on the main thread.
		/// </summary>
		/// <param name="d">The delegate to invoke.</param>
		/// <param name="state">The object passed to the delegate.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="d"/> is <see langword="null"/>.</exception>
		/// <seealso cref="SendToMainThread(SendOrPostCallback, object)"/>
		/// <seealso cref="PostToMainThread(SendOrPostCallback, object)"/>
		public static IAsyncOperation InvokeOnMainThread(SendOrPostCallback d, object state)
		{
			if (_mainThreadContext == SynchronizationContext.Current)
			{
				return AsyncResult.FromAction(d, state);
			}
			else
			{
				return PostToMainThread(d, state);
			}
		}

		/// <summary>
		/// Checks whether current thread is the main Unity thread.
		/// </summary>
		/// <returns>Returns <see langword="true"/> if current thread is Unity main thread; <see langword="false"/> otherwise.</returns>
		public static bool IsMainThread()
		{
			return _mainThreadContext == SynchronizationContext.Current;
		}

		/// <summary>
		/// Creates an operation that completes after a time delay.
		/// </summary>
		/// <param name="millisecondsDelay">The number of milliseconds to wait before completing the returned operation, or -1 to wait indefinitely.</param>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if the <paramref name="millisecondsDelay"/> is less than -1.</exception>
		/// <returns>An operation that represents the time delay.</returns>
		/// <seealso cref="Delay(float)"/>
		/// <seealso cref="Delay(TimeSpan)"/>
		public static IAsyncOperation Delay(int millisecondsDelay)
		{
			return AsyncResult.Delay(millisecondsDelay, GetRootBehaviour().UpdateSource);
		}

		/// <summary>
		/// Creates an operation that completes after a time delay.
		/// </summary>
		/// <param name="secondsDelay">The number of seconds to wait before completing the returned operation, or -1 to wait indefinitely.</param>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if the <paramref name="secondsDelay"/> is less than -1.</exception>
		/// <returns>An operation that represents the time delay.</returns>
		/// <seealso cref="Delay(int)"/>
		/// <seealso cref="Delay(TimeSpan)"/>
		public static IAsyncOperation Delay(float secondsDelay)
		{
			return AsyncResult.Delay(secondsDelay, GetRootBehaviour().UpdateSource);
		}

		/// <summary>
		/// Creates an operation that completes after a time delay.
		/// </summary>
		/// <param name="delay">The time span to wait before completing the returned operation, or <c>TimeSpan.FromMilliseconds(-1)</c> to wait indefinitely.</param>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if the <paramref name="delay"/> represents a negative time interval other than <c>TimeSpan.FromMillseconds(-1)</c>.</exception>
		/// <returns>An operation that represents the time delay.</returns>
		/// <seealso cref="Delay(int)"/>
		/// <seealso cref="Delay(float)"/>
		public static IAsyncOperation Delay(TimeSpan delay)
		{
			return AsyncResult.Delay(delay, GetRootBehaviour().UpdateSource);
		}

		/// <summary>
		/// Starts a coroutine.
		/// </summary>
		/// <param name="enumerator">The coroutine to run.</param>
		/// <returns>Returns the coroutine handle.</returns>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="enumerator"/> is <see langword="null"/>.</exception>
		/// <seealso cref="StopCoroutine(Coroutine)"/>
		/// <seealso cref="StopCoroutine(IEnumerator)"/>
		/// <seealso cref="StopAllCoroutines"/>
		public static Coroutine StartCoroutine(IEnumerator enumerator)
		{
			if (enumerator == null)
			{
				throw new ArgumentNullException("enumerator");
			}

			return GetRootBehaviour().StartCoroutine(enumerator);
		}

		/// <summary>
		/// Stops the specified coroutine.
		/// </summary>
		/// <param name="coroutine">The coroutine to stop.</param>
		/// <seealso cref="StartCoroutine(IEnumerator)"/>
		/// <seealso cref="StopCoroutine(IEnumerator)"/>
		/// <seealso cref="StopAllCoroutines"/>
		public static void StopCoroutine(Coroutine coroutine)
		{
			if (coroutine != null && _rootBehaviour)
			{
				_rootBehaviour.StopCoroutine(coroutine);
			}
		}

		/// <summary>
		/// Stops the specified coroutine.
		/// </summary>
		/// <param name="enumerator">The coroutine to stop.</param>
		/// <seealso cref="StartCoroutine(IEnumerator)"/>
		/// <seealso cref="StopCoroutine(Coroutine)"/>
		/// <seealso cref="StopAllCoroutines"/>
		public static void StopCoroutine(IEnumerator enumerator)
		{
			if (enumerator != null && _rootBehaviour)
			{
				_rootBehaviour.StopCoroutine(enumerator);
			}
		}

		/// <summary>
		/// Stops all coroutines.
		/// </summary>
		/// <seealso cref="StartCoroutine(IEnumerator)"/>
		/// <seealso cref="StopCoroutine(Coroutine)"/>
		/// <seealso cref="StopCoroutine(IEnumerator)"/>
		public static void StopAllCoroutines()
		{
			if (_rootBehaviour)
			{
				_rootBehaviour.StopAllCoroutines();
			}
		}

		/// <summary>
		/// Register a completion callback for the specified <see cref="AsyncOperation"/> instance. If operation is completed
		/// the <paramref name="completionCallback"/> is executed synchronously.
		/// </summary>
		/// <param name="op">The request to register completion callback for.</param>
		/// <param name="completionCallback">A delegate to be called when the <paramref name="op"/> has completed.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="op"/> or <paramref name="completionCallback"/> is <see langword="null"/>.</exception>
		/// <seealso cref="AddCompletionCallback(WWW, Action)"/>
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

			if (op.isDone)
			{
				completionCallback();
			}
			else
			{
#if UNITY_2017_2_OR_NEWER

				// Starting with Unity 2017.2 there is AsyncOperation.completed event.
				op.completed += o => completionCallback();

#else

				GetRootBehaviour().AddCompletionCallback(op, completionCallback);

#endif
			}
		}

#if UNITY_5_4_OR_NEWER

		/// <summary>
		/// Register a completion callback for the specified <see cref="UnityWebRequest"/> instance.
		/// </summary>
		/// <param name="request">The request to register completion callback for.</param>
		/// <param name="completionCallback">A delegate to be called when the <paramref name="request"/> has completed.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="request"/> or <paramref name="completionCallback"/> is <see langword="null"/>.</exception>
		/// <seealso cref="AddCompletionCallback(AsyncOperation, Action)"/>
		/// <seealso cref="AddCompletionCallback(WWW, Action)"/>
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

			GetRootBehaviour().AddCompletionCallback(request, completionCallback);
		}

#endif

		/// <summary>
		/// Register a completion callback for the specified <see cref="WWW"/> instance.
		/// </summary>
		/// <param name="request">The request to register completion callback for.</param>
		/// <param name="completionCallback">A delegate to be called when the <paramref name="request"/> has completed.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="request"/> or <paramref name="completionCallback"/> is <see langword="null"/>.</exception>
		/// <seealso cref="AddCompletionCallback(AsyncOperation, Action)"/>
		public static void AddCompletionCallback(WWW request, Action completionCallback)
		{
			if (request == null)
			{
				throw new ArgumentNullException("request");
			}

			if (completionCallback == null)
			{
				throw new ArgumentNullException("completionCallback");
			}

			GetRootBehaviour().AddCompletionCallback(request, completionCallback);
		}

		#endregion

		#region implementation

		private sealed class AsyncRootBehaviour : MonoBehaviour
		{
			#region data

			private Dictionary<object, Action> _ops;
			private List<object> _opsToRemove;

			private AsyncUpdateSource _updateSource;
			private AsyncUpdateSource _lateUpdateSource;
			private AsyncUpdateSource _fixedUpdateSource;
			private AsyncUpdateSource _eofUpdateSource;
			private WaitForEndOfFrame _eof;

			#endregion

			#region interface

			public IAsyncUpdateSource UpdateSource
			{
				get
				{
					if (_updateSource == null)
					{
						_updateSource = new AsyncUpdateSource();
					}

					return _updateSource;
				}
			}

			public IAsyncUpdateSource LateUpdateSource
			{
				get
				{
					if (_lateUpdateSource == null)
					{
						_lateUpdateSource = new AsyncUpdateSource();
					}

					return _lateUpdateSource;
				}
			}

			public IAsyncUpdateSource FixedUpdateSource
			{
				get
				{
					if (_fixedUpdateSource == null)
					{
						_fixedUpdateSource = new AsyncUpdateSource();
					}

					return _fixedUpdateSource;
				}
			}

			public IAsyncUpdateSource EofUpdateSource
			{
				get
				{
					if (_eofUpdateSource == null)
					{
						_eofUpdateSource = new AsyncUpdateSource();
						_eof = new WaitForEndOfFrame();
						StartCoroutine(EofEnumerator());
					}

					return _eofUpdateSource;
				}
			}

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
				if (_ops != null && _ops.Count > 0)
				{
					try
					{
						foreach (var item in _ops)
						{
							if (item.Key is AsyncOperation)
							{
								var asyncOp = item.Key as AsyncOperation;

								if (asyncOp.isDone)
								{
									_opsToRemove.Add(asyncOp);
									item.Value();
								}
							}
#if UNITY_5_4_OR_NEWER
							else if (item.Key is UnityWebRequest)
							{
								var asyncOp = item.Key as UnityWebRequest;

								if (asyncOp.isDone)
								{
									_opsToRemove.Add(asyncOp);
									item.Value();
								}
							}
#endif
							else if (item.Key is WWW)
							{
								var asyncOp = item.Key as WWW;

								if (asyncOp.isDone)
								{
									_opsToRemove.Add(asyncOp);
									item.Value();
								}
							}
						}
					}
					catch (Exception e)
					{
						Debug.LogException(e, this);
					}

					foreach (var item in _opsToRemove)
					{
						_ops.Remove(item);
					}

					_opsToRemove.Clear();
				}

				if (_updateSource != null)
				{
					try
					{
						_updateSource.OnNext(Time.deltaTime);
					}
					catch (Exception e)
					{
						Debug.LogException(e, this);
					}
				}

#if NET_4_6 || NET_STANDARD_2_0

				InvokeResult invokeResult;

				while (_actionQueue.TryDequeue(out invokeResult))
				{
					try
					{
						invokeResult.Start();
						invokeResult.SetCompleted();
					}
					catch (Exception e)
					{
						invokeResult.SetException(e);
						Debug.LogException(e, this);
					}
				}

#else

				if (_actionQueue.Count > 0)
				{
					lock (_actionQueue)
					{
						while (_actionQueue.Count > 0)
						{
							var asyncResult = _actionQueue.Dequeue();

							try
							{
								asyncResult.Start();
								asyncResult.SetCompleted();
							}
							catch (Exception e)
							{
								asyncResult.SetException(e);
								Debug.LogException(e, this);
							}
						}
					}
				}

#endif
			}

			private void LateUpdate()
			{
				if (_lateUpdateSource != null)
				{
					_lateUpdateSource.OnNext(Time.deltaTime);
				}
			}

			private void FixedUpdate()
			{
				if (_fixedUpdateSource != null)
				{
					_fixedUpdateSource.OnNext(Time.fixedDeltaTime);
				}
			}

			private void OnDestroy()
			{
				if (_updateSource != null)
				{
					_updateSource.Dispose();
					_updateSource = null;
				}

				if (_lateUpdateSource != null)
				{
					_lateUpdateSource.Dispose();
					_lateUpdateSource = null;
				}

				if (_fixedUpdateSource != null)
				{
					_fixedUpdateSource.Dispose();
					_fixedUpdateSource = null;
				}

				if (_eofUpdateSource != null)
				{
					_eofUpdateSource.Dispose();
					_eofUpdateSource = null;
				}
			}

			#endregion

			#region implementation

			private IEnumerator EofEnumerator()
			{
				yield return _eof;

				if (_eofUpdateSource != null)
				{
					_eofUpdateSource.OnNext(Time.deltaTime);
				}
			}

			#endregion
		}

		private sealed class InvokeResult : AsyncResult
		{
			private readonly SendOrPostCallback _callback;

			public InvokeResult(SendOrPostCallback d, object asyncState)
				: base(AsyncOperationStatus.Scheduled, asyncState)
			{
				_callback = d;
			}

			public void SetCompleted()
			{
				TrySetCompleted();
			}

			public void SetException(Exception e)
			{
				TrySetException(e);
			}

			protected override void OnStarted()
			{
				_callback.Invoke(AsyncState);
			}
		}

		private sealed class MainThreadSynchronizationContext : SynchronizationContext
		{
			public override SynchronizationContext CreateCopy()
			{
				return new MainThreadSynchronizationContext();
			}

			public override void Send(SendOrPostCallback d, object state)
			{
				SendToMainThread(d, state);
			}

			public override void Post(SendOrPostCallback d, object state)
			{
				PostToMainThread(d, state);
			}
		}

#if !NET35
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		private static AsyncRootBehaviour GetRootBehaviour()
		{
			if (ReferenceEquals(_rootBehaviour, null))
			{
				InitRootBehaviour();
			}

			return _rootBehaviour;
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		private static void InitRootBehaviour()
		{
			var go = GetRootGo();

			if (go)
			{
				_rootBehaviour = go.AddComponent<AsyncRootBehaviour>();
			}
			else
			{
				throw new ObjectDisposedException(RootGoName);
			}
		}

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		private static void Initialize()
		{
			var context = SynchronizationContext.Current;

			if (context == null)
			{
				context = new MainThreadSynchronizationContext();
				SynchronizationContext.SetSynchronizationContext(context);
			}

			// Save the main thread context for future use.
			_mainThreadContext = context;

			// Initialize delayed action queue.
#if NET_4_6 || NET_STANDARD_2_0
			_actionQueue = new ConcurrentQueue<InvokeResult>();
#else
			_actionQueue = new Queue<InvokeResult>();
#endif

			// Set main thread context as default for all continuations. This saves allocations in many cases.
			AsyncResult.DefaultSynchronizationContext = context;
		}

		#endregion
	}
}
