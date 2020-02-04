// Copyright (c) 2018-2020 Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;

namespace UnityFx.Async
{
	internal static class Messages
	{
		public static string FormatError_ListIsEmpty()
		{
			return "The list cannot be empty.";
		}

		public static string FormatError_ListElementIsNull()
		{
			return "The list elements cannot be null.";
		}

		public static string FormatError_ValueIsLessThanZero()
		{
			return "The valus cannot be less than zero.";
		}

		public static string FormatError_OperationIsNotCompleted()
		{
			return "The operation should be completed.";
		}

		public static string FormatError_OperationResultIsNotAvailable()
		{
			return "The operation result is not available. Result value is only available for operations that have completed successfully.";
		}

		public static string FormatError_OperationStateCannotBeChanged()
		{
			return "The operation state cannot be changed. Most likely the operation is already completed.";
		}

		public static string FormatError_InvalidTimeout()
		{
			return "The timeout value specified is not valid. The supported range is [-1, MaxInt].";
		}

		public static string FormatError_InvalidProgressValue()
		{
			return "The progress value should be in range [0, 1].";
		}

		public static string FormatError_InvalidDelegateType(Type type)
		{
			return $"Invalid delegate type: {type.Name}.";
		}

		public static string FormatError_MaxNumberOrRetriesReached(int numberOfRetries)
		{
			return "Maximum number of retries exceeded.";
		}
	}
}
