// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using UnityEngine;

namespace UnityFx.Async
{
	/// <summary>
	/// A wrapper for <see cref="AsyncOperation"/> with result value.
	/// </summary>
	public class AsyncOperationResult : AsyncResult
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
		/// Initializes a new instance of the <see cref="AsyncOperationResult"/> class.
		/// </summary>
		/// <param name="op">Source web request.</param>
		public AsyncOperationResult(AsyncOperation op)
		{
			_op = op;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AsyncOperationResult"/> class.
		/// </summary>
		/// <param name="op">Source web request.</param>
		/// <param name="userState">User-defined data.</param>
		public AsyncOperationResult(AsyncOperation op, object userState)
			: base(null, userState)
		{
			_op = op;
		}

		#endregion

		#region AsyncResult

		/// <inheritdoc/>
		protected override void OnStarted()
		{
			if (_op.isDone)
			{
				TrySetCompleted(true);
			}
			else
			{
#if UNITY_2017_2_OR_NEWER || UNITY_2018

				// Starting with Unity 2017.2 there is AsyncOperation.completed event
				_op.completed += o => TrySetCompleted();

#else

				AsyncUtility.AddCompletionCallback(_op, () => TrySetCompleted());

#endif
			}
		}

		/// <inheritdoc/>
		protected override float GetProgress()
		{
			return _op.progress;
		}

		#endregion
	}
}
