# What is this?

*UnityFx.Async* is a set of of classes and interfaces that extend [Unity3d](https://unity3d.com) asynchronous operations and can be used very much like [TPL](https://msdn.microsoft.com/ru-ru/library/dd460717(v=vs.110).aspx) in .NET.

Quick example:
```csharp
var op = AsyncResult.FromWebRequest(UnityWebRequest.Get("https://www.google.com"));
yield return op;

if (op.IsCompletedSuccessfully)
{
	// TODO
}
```

# Why do I need this?

While Unity3d is a great engine, there are quite a few places where its API is not ideal. Asynchronous operations and coroutines management are the examples. While the concept of coroutines itself is great for frame-based applications, current Unity implementation is not consistent at least:
- There is no single base class/interface for yieldable entities. For example [Coroutine](https://docs.unity3d.com/ScriptReference/Coroutine.html) and [AsyncOperation](https://docs.unity3d.com/ScriptReference/AsyncOperation.html) both inherit [YieldInstruction](https://docs.unity3d.com/ScriptReference/YieldInstruction.html), while [CustomYieldInstruction](https://docs.unity3d.com/ScriptReference/CustomYieldInstruction.html) and [WWW](https://docs.unity3d.com/ScriptReference/WWW.html) do not. *UnityFx.Async* uses .NET [IAsyncResult](https://msdn.microsoft.com/en-us/library/system.iasyncresult(v=vs.110).aspx) interface as a base for all its asynchronous operations and provides wrappers for native Unity3d operations.
- Running a coroutine requires [MonoBehaviour](https://docs.unity3d.com/ScriptReference/MonoBehaviour.html) instance which is not always convenient. *UnityFx.Async* provides possibility to start coroutines without specifying a `MonoBehaviour`. It also provides a mechanism of defining custom coroutine runners via `AsyncScheduler` abstract class.
- Unity3d built-in asynchronous operations provide very little control after they have been started, [Coroutine](https://docs.unity3d.com/ScriptReference/Coroutine.html) for example doesn't even provide a way to determine if it is completed. *UnityFx.Async* defines `IAsyncOperation` interface to give users extended control options (it mimics .NET [Task](https://msdn.microsoft.com/ru-ru/library/system.threading.tasks.task(v=vs.110).aspx) as much as possible).
- There is no standard way to return a coroutine result value. While some of the [AsyncOperation](https://docs.unity3d.com/ScriptReference/AsyncOperation.html)-derived classes define operation results, [WWW](https://docs.unity3d.com/ScriptReference/WWW.html) uses completely inconsistent way of doing this. There is a generic version of `IAsyncOperation` interface with result values in *UnityFx.Async* (again very similar to [Task<T>](https://msdn.microsoft.com/ru-ru/library/dd321424(v=vs.110).aspx)).
- There is no easy way of chaining coroutines, waiting for completion of a coroutine group etc. *UnityFx.Async* implements a set of extension methods for both `IAsyncResult` and `IAsyncOperation` interfaces that provide the above mentioned functionality.
- Coroutines can't handle exceptions, because `yield return` statements cannot be surrounded with a try-catch block. *UnityFx.Async* finishes the corresponding operation with an error if any exceptions are being thrown inside its coroutine update loop.
- *UnityFx.Async* provides default yieldable/awaitable implementation for `IAsyncOperation` and `IAsyncResult` to allow easy library extension.

