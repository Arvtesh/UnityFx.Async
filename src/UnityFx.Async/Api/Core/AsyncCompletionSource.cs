// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;

namespace UnityFx.Async
{
	/// <summary>
	/// Represents an asynchronous operation with external completion control.
	/// </summary>
	/// <seealso cref="AsyncCompletionSource{T}"/>
	public sealed class AsyncCompletionSource : AsyncResult, IAsyncCompletionSource
	{
		#region data

		private float _progress;

		#endregion

		#region interface

		/// <summary>
		/// Initializes a new instance of the <see cref="AsyncCompletionSource"/> class.
		/// </summary>
		public AsyncCompletionSource()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AsyncCompletionSource"/> class.
		/// </summary>
		/// <param name="options">The <see cref="AsyncCreationOptions"/> used to customize the operation's behavior.</param>
		public AsyncCompletionSource(AsyncCreationOptions options)
			: base(options)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AsyncCompletionSource"/> class.
		/// </summary>
		/// <param name="asyncState">User-defined data returned by <see cref="IAsyncResult.AsyncState"/>.</param>
		public AsyncCompletionSource(object asyncState)
			: base(default(AsyncCallback), asyncState)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AsyncCompletionSource"/> class.
		/// </summary>
		/// <param name="options">The <see cref="AsyncCreationOptions"/> used to customize the operation's behavior.</param>
		/// <param name="asyncState">User-defined data returned by <see cref="IAsyncResult.AsyncState"/>.</param>
		public AsyncCompletionSource(AsyncCreationOptions options, object asyncState)
			: base(options, null, asyncState)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AsyncCompletionSource"/> class.
		/// </summary>
		/// <param name="asyncCallback">User-defined completion callback.</param>
		/// <param name="asyncState">User-defined data returned by <see cref="IAsyncResult.AsyncState"/>.</param>
		public AsyncCompletionSource(AsyncCallback asyncCallback, object asyncState)
			: base(asyncCallback, asyncState)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AsyncCompletionSource"/> class.
		/// </summary>
		/// <param name="options">The <see cref="AsyncCreationOptions"/> used to customize the operation's behavior.</param>
		/// <param name="asyncCallback">User-defined completion callback.</param>
		/// <param name="asyncState">User-defined data returned by <see cref="IAsyncResult.AsyncState"/>.</param>
		public AsyncCompletionSource(AsyncCreationOptions options, AsyncCallback asyncCallback, object asyncState)
			: base(options, asyncCallback, asyncState)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AsyncCompletionSource"/> class.
		/// </summary>
		/// <param name="status">Initial value of the <see cref="AsyncResult.Status"/> property.</param>
		public AsyncCompletionSource(AsyncOperationStatus status)
			: base(status)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AsyncCompletionSource"/> class.
		/// </summary>
		/// <param name="status">Initial value of the <see cref="AsyncResult.Status"/> property.</param>
		/// <param name="options">The <see cref="AsyncCreationOptions"/> used to customize the operation's behavior.</param>
		public AsyncCompletionSource(AsyncOperationStatus status, AsyncCreationOptions options)
			: base(status, options)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AsyncCompletionSource"/> class.
		/// </summary>
		/// <param name="status">Initial value of the <see cref="AsyncResult.Status"/> property.</param>
		/// <param name="asyncState">User-defined data returned by <see cref="IAsyncResult.AsyncState"/>.</param>
		public AsyncCompletionSource(AsyncOperationStatus status, object asyncState)
			: base(status, asyncState)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AsyncCompletionSource"/> class.
		/// </summary>
		/// <param name="status">Initial value of the <see cref="AsyncResult.Status"/> property.</param>
		/// <param name="options">The <see cref="AsyncCreationOptions"/> used to customize the operation's behavior.</param>
		/// <param name="asyncState">User-defined data returned by <see cref="IAsyncResult.AsyncState"/>.</param>
		public AsyncCompletionSource(AsyncOperationStatus status, AsyncCreationOptions options, object asyncState)
			: base(status, options, asyncState)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AsyncCompletionSource"/> class.
		/// </summary>
		/// <param name="status">Initial value of the <see cref="AsyncResult.Status"/> property.</param>
		/// <param name="asyncCallback">User-defined completion callback.</param>
		/// <param name="asyncState">User-defined data returned by <see cref="IAsyncResult.AsyncState"/>.</param>
		public AsyncCompletionSource(AsyncOperationStatus status, AsyncCallback asyncCallback, object asyncState)
			: base(status, asyncCallback, asyncState)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AsyncCompletionSource"/> class.
		/// </summary>
		/// <param name="status">Initial value of the <see cref="AsyncResult.Status"/> property.</param>
		/// <param name="options">The <see cref="AsyncCreationOptions"/> used to customize the operation's behavior.</param>
		/// <param name="asyncCallback">User-defined completion callback.</param>
		/// <param name="asyncState">User-defined data returned by <see cref="IAsyncResult.AsyncState"/>.</param>
		public AsyncCompletionSource(AsyncOperationStatus status, AsyncCreationOptions options, AsyncCallback asyncCallback, object asyncState)
			: base(status, options, asyncCallback, asyncState)
		{
		}

		/// <summary>
		/// Transitions the operation to <see cref="AsyncOperationStatus.Scheduled"/> state.
		/// </summary>
		/// <exception cref="InvalidOperationException">Thrown if the transition fails.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation is disposed.</exception>
		/// <seealso cref="TrySetScheduled"/>
		/// <seealso cref="SetRunning"/>
		public void SetScheduled()
		{
			if (!base.TrySetScheduled())
			{
				throw new InvalidOperationException();
			}
		}

		/// <summary>
		/// Attempts to transition the operation into the <see cref="AsyncOperationStatus.Scheduled"/> state.
		/// </summary>
		/// <exception cref="ObjectDisposedException">Thrown is the operation is disposed.</exception>
		/// <returns>Returns <see langword="true"/> if the attemp was successfull; <see langword="false"/> otherwise.</returns>
		/// <seealso cref="SetScheduled"/>
		/// <seealso cref="TrySetRunning"/>
		public new bool TrySetScheduled() => base.TrySetScheduled();

		/// <summary>
		/// Transitions the operation to <see cref="AsyncOperationStatus.Running"/> state.
		/// </summary>
		/// <exception cref="InvalidOperationException">Thrown if the transition fails.</exception>
		/// <exception cref="ObjectDisposedException">Thrown is the operation is disposed.</exception>
		/// <seealso cref="TrySetRunning"/>
		/// <seealso cref="SetScheduled"/>
		public void SetRunning()
		{
			if (!base.TrySetRunning())
			{
				throw new InvalidOperationException();
			}
		}

		/// <summary>
		/// Attempts to transition the operation into the <see cref="AsyncOperationStatus.Running"/> state.
		/// </summary>
		/// <returns>Returns <see langword="true"/> if the attemp was successfull; <see langword="false"/> otherwise.</returns>
		/// <exception cref="ObjectDisposedException">Thrown is the operation is disposed.</exception>
		/// <seealso cref="SetRunning"/>
		/// <seealso cref="TrySetScheduled"/>
		public new bool TrySetRunning() => base.TrySetRunning();

		#endregion

		#region AsyncResult

		/// <inheritdoc/>
		protected override float GetProgress()
		{
			return _progress;
		}

		/// <inheritdoc/>
		protected override void OnCancel()
		{
			TrySetCanceled(false);
		}

		#endregion

		#region IAsyncCompletionSource

		/// <inheritdoc/>
		public IAsyncOperation Operation => this;

		/// <inheritdoc/>
		public bool TrySetProgress(float progress)
		{
			if (progress < 0 || progress > 1)
			{
				throw new ArgumentOutOfRangeException(nameof(progress), progress, Constants.ErrorInvalidProgress);
			}

			ThrowIfDisposed();

			if (Status == AsyncOperationStatus.Running && _progress != progress)
			{
				_progress = progress;
				ReportProgress();
				return true;
			}

			return false;
		}

		/// <inheritdoc/>
		public new bool TrySetCanceled() => TrySetCanceled(false);

		/// <inheritdoc/>
		public new bool TrySetCompleted() => TrySetCompleted(false);

		/// <inheritdoc/>
		public new bool TrySetException(Exception exception) => TrySetException(exception, false);

		/// <inheritdoc/>
		public new bool TrySetExceptions(IEnumerable<Exception> exceptions) => TrySetExceptions(exceptions, false);

		#endregion
	}
}
