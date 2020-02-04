// Copyright (c) 2018-2020 Alexander Bogarsukov.
// Licensed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;

namespace System.Runtime.CompilerServices
{
#if !NET35

	/// <summary>
	/// Indicates the type of the async method builder that should be used by a language compiler to
	/// build the attributed type when used as the return type of an async method.
	/// </summary>
	/// <seealso href="https://blogs.msdn.microsoft.com/seteplia/2018/01/11/extending-the-async-methods-in-c/"/>
	public sealed class AsyncMethodBuilderAttribute : Attribute
	{
		/// <summary>
		/// Gets the <see cref="Type"/> of the associated builder.
		/// </summary>
		public Type BuilderType { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="AsyncMethodBuilderAttribute"/> class.
		/// </summary>
		public AsyncMethodBuilderAttribute(Type builderType)
		{
			BuilderType = builderType;
		}
	}

#endif
}
