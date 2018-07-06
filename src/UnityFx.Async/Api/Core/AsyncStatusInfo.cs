// Copyright (c) Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;

namespace UnityFx.Async
{
	/// <summary>
	/// A helper for initialization-like operations.
	/// </summary>
	public struct AsyncStatusInfo
	{
		#region data

		private Func<IAsyncOperation> _opFactory;
		private IAsyncOperation _op;

		#endregion

		#region interface

		/// <summary>
		/// Gets a value indicating whether the operation has started.
		/// </summary>
		public bool IsStarted => _op != null;

		/// <summary>
		/// Gets a value indicating whether the operation completed successfully (i.e. with <see cref="AsyncOperationStatus.RanToCompletion"/> status).
		/// </summary>
		public bool IsCompletedSuccessfully => _op?.IsCompletedSuccessfully ?? false;

		/// <summary>
		/// Gets a value indicating whether the operation has completed.
		/// </summary>
		public bool IsCompleted => _op?.IsCompleted ?? false;

		/// <summary>
		/// Gets a value indicating whether the operation completed due to an unhandled exception (i.e. with <see cref="AsyncOperationStatus.Faulted"/> status).
		/// </summary>
		public bool IsFaulted => _op?.IsFaulted ?? false;

		/// <summary>
		/// Gets a value indicating whether the operation completed due to being canceled (i.e. with <see cref="AsyncOperationStatus.Canceled"/> status).
		/// </summary>
		public bool IsCanceled => _op?.IsCanceled ?? false;

		/// <summary>
		/// Gets the operation (might return <see langword="null"/>).
		/// </summary>
		public IAsyncOperation Operation => _op;

		/// <summary>
		/// Gets or sets the oepration factory delegate.
		/// </summary>
		public Func<IAsyncOperation> OperationFactory
		{
			get
			{
				return _opFactory;
			}
			set
			{
				_opFactory = value ?? throw new ArgumentNullException(nameof(value));
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AsyncStatusInfo"/> struct.
		/// </summary>
		/// <param name="opFactory"></param>
		public AsyncStatusInfo(Func<IAsyncOperation> opFactory)
		{
			_opFactory = opFactory ?? throw new ArgumentNullException(nameof(opFactory));
			_op = null;
		}

		/// <summary>
		/// Starts the operation or just updates its state.
		/// </summary>
		public IAsyncOperation StartOrUpdate()
		{
			if (_op != null)
			{
				UpdateOperation();
			}
			else
			{
				if (_opFactory == null)
				{
					throw new InvalidOperationException();
				}

				_op = _opFactory();
			}

			return _op;
		}

		/// <summary>
		/// Implicit conversion to <see langword="bool"/>.
		/// </summary>
		public static implicit operator bool(AsyncStatusInfo status)
		{
			return status.IsCompletedSuccessfully;
		}

		#endregion

		#region implementation

		private void UpdateOperation()
		{
			if (_op.IsCompletedSuccessfully)
			{
				_op = AsyncResult.CompletedOperation;
				_opFactory = null;
			}
		}

		#endregion
	}
}
