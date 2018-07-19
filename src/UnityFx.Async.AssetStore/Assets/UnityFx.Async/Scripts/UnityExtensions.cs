// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
#if UNITY_5_4_OR_NEWER
using UnityEngine.Networking;
#endif
#if NET_4_6 || NET_STANDARD_2_0
using System.Threading.Tasks;
#endif

namespace UnityFx.Async
{
	/// <summary>
	/// Extensions for Unity API.
	/// </summary>
	public static class UnityExtensions
	{
		#region AsyncOperation

		/// <summary>
		/// Creates an <see cref="IAsyncOperation"/> wrapper for the Unity <see cref="AsyncOperation"/>.
		/// </summary>
		/// <param name="op">The source operation.</param>
		public static IAsyncOperation ToAsync(this AsyncOperation op)
		{
			if (op.isDone)
			{
				return AsyncResult.CompletedOperation;
			}
			else
			{
				var result = new AsyncOperationResult(op);
				result.Start();
				return result;
			}
		}

		/// <summary>
		/// Creates an <see cref="IAsyncOperation{TResult}"/> wrapper for the Unity <see cref="ResourceRequest"/>.
		/// </summary>
		/// <param name="op">The source operation.</param>
		public static IAsyncOperation<T> ToAsync<T>(this ResourceRequest op) where T : UnityEngine.Object
		{
			var result = new ResourceRequestResult<T>(op);
			result.Start();
			return result;
		}

		/// <summary>
		/// Creates an <see cref="IAsyncOperation{TResult}"/> wrapper for the Unity <see cref="AssetBundleRequest"/>.
		/// </summary>
		/// <param name="op">The source operation.</param>
		public static IAsyncOperation<T> ToAsync<T>(this AssetBundleRequest op) where T : UnityEngine.Object
		{
			var result = new AssetBundleRequestResult<T>(op);
			result.Start();
			return result;
		}

#if NET_4_6 || NET_STANDARD_2_0

		/// <summary>
		/// Creates an <see cref="Task"/> wrapper for the Unity <see cref="AsyncOperation"/>.
		/// </summary>
		/// <param name="op">The source operation.</param>
		public static Task ToTask(this AsyncOperation op)
		{
			if (op.isDone)
			{
				return Task.CompletedTask;
			}
			else
			{
				var result = new TaskCompletionSource<object>(op);
				AsyncUtility.AddCompletionCallback(op, () => result.TrySetResult(null));
				return result.Task;
			}
		}

		/// <summary>
		/// Creates an <see cref="Task{TResult}"/> wrapper for the Unity <see cref="ResourceRequest"/>.
		/// </summary>
		/// <param name="op">The source operation.</param>
		public static Task<T> ToTask<T>(this ResourceRequest op) where T : UnityEngine.Object
		{
			var result = new TaskCompletionSource<T>(op);
			AsyncUtility.AddCompletionCallback(op, () => result.TrySetResult(op.asset as T));
			return result.Task;
		}

		/// <summary>
		/// Creates an <see cref="Task{TResult}"/> wrapper for the Unity <see cref="AssetBundleRequest"/>.
		/// </summary>
		/// <param name="op">The source operation.</param>
		public static Task<T> ToTask<T>(this AssetBundleRequest op) where T : UnityEngine.Object
		{
			var result = new TaskCompletionSource<T>(op);
			AsyncUtility.AddCompletionCallback(op, () => result.TrySetResult(op.asset as T));
			return result.Task;
		}

		/// <summary>
		/// Provides an object that waits for the completion of an <see cref="AsyncOperation"/>. This type and its members are intended for compiler use only.
		/// </summary>
		public struct AsyncOperationAwaiter : INotifyCompletion
		{
			private readonly AsyncOperation _op;

			/// <summary>
			/// Initializes a new instance of the <see cref="AsyncOperationAwaiter"/> struct.
			/// </summary>
			public AsyncOperationAwaiter(AsyncOperation op)
			{
				_op = op;
			}

			/// <summary>
			/// Gets a value indicating whether the underlying operation is completed.
			/// </summary>
			/// <value>The operation completion flag.</value>
			public bool IsCompleted => _op.isDone;

			/// <summary>
			/// Returns the source result value.
			/// </summary>
			public void GetResult()
			{
			}

			/// <inheritdoc/>
			public void OnCompleted(Action continuation)
			{
				AsyncUtility.AddCompletionCallback(_op, continuation);
			}
		}

		/// <summary>
		/// Returns the operation awaiter. This method is intended for compiler rather than use directly in code.
		/// </summary>
		/// <param name="op">The operation to await.</param>
		public static AsyncOperationAwaiter GetAwaiter(this AsyncOperation op)
		{
			return new AsyncOperationAwaiter(op);
		}

#endif

		#endregion

		#region UnityWebRequest

#if UNITY_5_4_OR_NEWER

		/// <summary>
		/// Creates an <see cref="IAsyncOperation"/> wrapper for the specified <see cref="UnityWebRequest"/>.
		/// </summary>
		/// <param name="request">The source web request.</param>
		public static IAsyncOperation ToAsync(this UnityWebRequest request)
		{
			var result = new WebRequestResult<object>(request);
			result.Start();
			return result;
		}

		/// <summary>
		/// Creates an <see cref="IAsyncOperation{TResult}"/> wrapper for the specified <see cref="UnityWebRequest"/>.
		/// </summary>
		/// <param name="request">The source web request.</param>
		public static IAsyncOperation<T> ToAsync<T>(this UnityWebRequest request) where T : class
		{
			var result = new WebRequestResult<T>(request);
			result.Start();
			return result;
		}

#if NET_4_6 || NET_STANDARD_2_0

		/// <summary>
		/// Creates an <see cref="Task"/> wrapper for the specified <see cref="UnityWebRequest"/>.
		/// </summary>
		/// <param name="request">The source web request.</param>
		public static Task ToTask(this UnityWebRequest request)
		{
			var result = new TaskCompletionSource<object>(request);
			AsyncUtility.AddCompletionCallback(request, () => OnTaskCompleted(result, request));
			return result.Task;
		}

		/// <summary>
		/// Creates an <see cref="Task{TResult}"/> wrapper for the specified <see cref="UnityWebRequest"/>.
		/// </summary>
		/// <param name="request">The source web request.</param>
		public static Task<T> ToTask<T>(this UnityWebRequest request) where T : class
		{
			var result = new TaskCompletionSource<T>(request);
			AsyncUtility.AddCompletionCallback(request, () => OnTaskCompleted(result, request));
			return result.Task;
		}

		/// <summary>
		/// Provides an object that waits for the completion of an <see cref="UnityWebRequest"/>. This type and its members are intended for compiler use only.
		/// </summary>
		public struct UnityWebRequestAwaiter : INotifyCompletion
		{
			private readonly UnityWebRequest _op;

			/// <summary>
			/// Initializes a new instance of the <see cref="UnityWebRequestAwaiter"/> struct.
			/// </summary>
			public UnityWebRequestAwaiter(UnityWebRequest op)
			{
				_op = op;
			}

			/// <summary>
			/// Gets a value indicating whether the underlying operation is completed.
			/// </summary>
			/// <value>The operation completion flag.</value>
			public bool IsCompleted => _op.isDone;

			/// <summary>
			/// Returns the source result value.
			/// </summary>
			public void GetResult()
			{
			}

			/// <inheritdoc/>
			public void OnCompleted(Action continuation)
			{
				AsyncUtility.AddCompletionCallback(_op, continuation);
			}
		}

		/// <summary>
		/// Returns the operation awaiter. This method is intended for compiler rather than use directly in code.
		/// </summary>
		/// <param name="op">The operation to await.</param>
		public static UnityWebRequestAwaiter GetAwaiter(this UnityWebRequest op)
		{
			return new UnityWebRequestAwaiter(op);
		}

#endif

#endif

		#endregion

		#region WWW

		/// <summary>
		/// Creates an <see cref="IAsyncOperation"/> wrapper for the specified <see cref="WWW"/>.
		/// </summary>
		/// <param name="request">The source web request.</param>
		public static IAsyncOperation ToAsync(this WWW request)
		{
			var result = new WwwResult<object>(request);
			result.Start();
			return result;
		}

		/// <summary>
		/// Creates an <see cref="IAsyncOperation{TResult}"/> wrapper for the specified <see cref="WWW"/>.
		/// </summary>
		/// <param name="request">The source web request.</param>
		public static IAsyncOperation<T> ToAsync<T>(this WWW request) where T : class
		{
			var result = new WwwResult<T>(request);
			result.Start();
			return result;
		}

#if NET_4_6 || NET_STANDARD_2_0

		/// <summary>
		/// Creates an <see cref="Task"/> wrapper for the specified <see cref="WWW"/>.
		/// </summary>
		/// <param name="request">The source web request.</param>
		public static Task ToTask(this WWW request)
		{
			var result = new TaskCompletionSource<object>(request);
			AsyncUtility.AddCompletionCallback(request, () => OnTaskCompleted(result, request));
			return result.Task;
		}

		/// <summary>
		/// Creates an <see cref="Task{TResult}"/> wrapper for the specified <see cref="WWW"/>.
		/// </summary>
		/// <param name="request">The source web request.</param>
		public static Task<T> ToTask<T>(this WWW request) where T : class
		{
			var result = new TaskCompletionSource<T>(request);
			AsyncUtility.AddCompletionCallback(request, () => OnTaskCompleted(result, request));
			return result.Task;
		}

		/// <summary>
		/// Provides an object that waits for the completion of an <see cref="WWW"/>. This type and its members are intended for compiler use only.
		/// </summary>
		public struct WwwAwaiter : INotifyCompletion
		{
			private readonly WWW _op;

			/// <summary>
			/// Initializes a new instance of the <see cref="WwwAwaiter"/> struct.
			/// </summary>
			public WwwAwaiter(WWW op)
			{
				_op = op;
			}

			/// <summary>
			/// Gets a value indicating whether the underlying operation is completed.
			/// </summary>
			/// <value>The operation completion flag.</value>
			public bool IsCompleted => _op.isDone;

			/// <summary>
			/// Returns the source result value.
			/// </summary>
			public void GetResult()
			{
			}

			/// <inheritdoc/>
			public void OnCompleted(Action continuation)
			{
				AsyncUtility.AddCompletionCallback(_op, continuation);
			}
		}

		/// <summary>
		/// Returns the operation awaiter. This method is intended for compiler rather than use directly in code.
		/// </summary>
		/// <param name="op">The operation to await.</param>
		public static WwwAwaiter GetAwaiter(this WWW op)
		{
			return new WwwAwaiter(op);
		}

#endif

		#endregion

		#region implementation

#if NET_4_6 || NET_STANDARD_2_0

#if UNITY_5_4_OR_NEWER

		private static void OnTaskCompleted<T>(TaskCompletionSource<T> tcs, UnityWebRequest request) where T : class
		{
			try
			{
#if UNITY_5
				if (request.isError)
#else
				if (request.isHttpError || request.isNetworkError)
#endif
				{
					tcs.TrySetException(new WebRequestException(request.error, request.responseCode));
				}
				else if (request.downloadHandler != null)
				{
					var result = AsyncUtility.GetResult<T>(request);
					tcs.TrySetResult(result);
				}
				else
				{
					tcs.TrySetResult(null);
				}
			}
			catch (Exception e)
			{
				tcs.TrySetException(e);
			}
		}

#endif

		private static void OnTaskCompleted<T>(TaskCompletionSource<T> tcs, WWW www) where T : class
		{
			try
			{
				if (string.IsNullOrEmpty(www.error))
				{
					var result = AsyncUtility.GetResult<T>(www);
					tcs.TrySetResult(result);
				}
				else
				{
					tcs.TrySetException(new WebRequestException(www.error));
				}
			}
			catch (Exception e)
			{
				tcs.TrySetException(e);
			}
		}

#endif

		#endregion
	}
}
