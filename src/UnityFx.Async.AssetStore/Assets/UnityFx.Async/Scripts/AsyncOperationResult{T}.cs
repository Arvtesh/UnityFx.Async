// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using UnityEngine;

namespace UnityFx.Async
{
	/// <summary>
	/// A wrapper for <see cref="AsyncOperation"/> with result value.
	/// </summary>
	public abstract class AsyncOperationResult<T> : AsyncResult<T> where T : UnityEngine.Object
	{
		#region data

		private readonly AsyncOperation _op;

		#endregion

		#region interface

		/// <summary>
		/// Gets the underlying <see cref="AsyncOperation"/> instance.
		/// </summary>
		public AsyncOperation Operation
		{
			get
			{
				return _op;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AsyncOperationResult{T}"/> class.
		/// </summary>
		/// <param name="op">Source web request.</param>
		protected AsyncOperationResult(AsyncOperation op)
		{
			_op = op;
		}

		/// <summary>
		/// Called when the source <see cref="AsyncOperation"/> is completed.
		/// </summary>
		protected abstract T GetResult(AsyncOperation op);

		#endregion

		#region AsyncResult

		/// <inheritdoc/>
		protected override void OnStarted()
		{
			if (_op.isDone)
			{
				OnSetCompleted(_op);
			}
			else
			{
#if UNITY_2017_2_OR_NEWER || UNITY_2018

				// Starting with Unity 2017.2 there is AsyncOperation.completed event
				_op.completed += OnSetCompleted;

#else

				AsyncUtility.AddCompletionCallback(_op, () => OnSetCompleted(_op));

#endif
			}
		}

		/// <inheritdoc/>
		protected override float GetProgress()
		{
			return _op.progress;
		}

		#endregion

		#region implementation

		private void OnSetCompleted(AsyncOperation op)
		{
			TrySetResult(GetResult(op));
		}

		#endregion
	}
}
