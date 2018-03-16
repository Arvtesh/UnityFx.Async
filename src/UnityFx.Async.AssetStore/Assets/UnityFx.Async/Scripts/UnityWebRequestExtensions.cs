// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using UnityEngine;
using UnityEngine.Networking;

namespace UnityFx.Async
{
	/// <summary>
	/// Extensions for <see cref="UnityWebRequest"/> class.
	/// </summary>
	public static class UnityWebRequestExtensions
	{
		#region interface

		/// <summary>
		/// Creates an <see cref="IAsyncOperation"/> wrapper for the specified <see cref="UnityWebRequest"/>.
		/// </summary>
		/// <param name="request">The source web request.</param>
		/// <returns>Returns a <see cref="IAsyncOperation"/> instance that will complete when the source operation have completed.</returns>
		public static AsyncResult<T> ToAsync<T>(this UnityWebRequest request)
		{
			if (request.isDone)
			{
				if (request.isHttpError || request.isNetworkError)
				{
					return AsyncResult.FromException<T>(new UnityWebRequestException(request.error, request.responseCode));
				}
				else
				{
					return AsyncResult.FromResult(GetResult<T>(request));
				}
			}
			else
			{
				throw new NotImplementedException();
			}
		}

		#endregion

		#region implementation

		private class WebRequestResult<T> : AsyncResult<T>
		{
			private readonly UnityWebRequest _request;
			private AsyncOperation _op;

			public WebRequestResult(UnityWebRequest request)
				: base(request.isModifiable ? AsyncOperationStatus.Created : AsyncOperationStatus.Running)
			{
				_request = request;
			}

			protected override void OnStarted()
			{
				base.OnStarted();
				_op = _request.Send();
			}

			protected override void OnCompleted()
			{
				_op = null;
				base.OnCompleted();
			}

			protected override void Dispose(bool disposing)
			{
				if (disposing)
				{
					_request.Dispose();
					_op = null;
				}

				base.Dispose(disposing);
			}
		}

		private static T GetResult<T>(UnityWebRequest request)
		{
			return default(T);
		}

		#endregion
	}
}
