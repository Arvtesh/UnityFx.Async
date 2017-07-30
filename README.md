# UnityFx.Async

## Synopsis

Unity3d extensions for asynchronous operations and coroutines management. The library provides an `IAsyncOperation` interface that can be used very much like TPL [Task](https://msdn.microsoft.com/ru-ru/library/system.threading.tasks.task(v=vs.110).aspx).

## Motivation

Unity3d provides powerful tools for creating graphical applications. Still there are quite a few places where its API is not ideal. One of the issues is asynchronous operations and coroutines management. While the concept of coroutines itself is great for frame-based applications, Unity implementation is not consistent to say the least:
- There is no single base class/interface for yieldable entities; for example [Coroutine](https://docs.unity3d.com/ScriptReference/Coroutine.html) and [AsyncOperation](https://docs.unity3d.com/ScriptReference/AsyncOperation.html) both inherit [YieldInstruction](https://docs.unity3d.com/ScriptReference/YieldInstruction.html), while [CustomYieldInstruction](https://docs.unity3d.com/ScriptReference/CustomYieldInstruction.html) and [WWW](https://docs.unity3d.com/ScriptReference/WWW.html) do not. *UnityFx* uses .NET [IAsyncResult](https://msdn.microsoft.com/en-us/library/system.iasyncresult(v=vs.110).aspx) interface as a base for any asynchronous operation and provides wrappers for native Unity3d operations.
- Running a coroutine requires [MonoBehaviour](https://docs.unity3d.com/ScriptReference/MonoBehaviour.html) instance which is not always convenient. *UnityFx* provides possibility to start coroutines without specifying a `MonoBehaviour`.
- Unity3d built-in asynchronous operations provide very little control after they have been started, [Coroutine](https://docs.unity3d.com/ScriptReference/Coroutine.html) for example doesn't even provide a way to determine if it is completed. *UnityFx* defines `IAsyncOperation` interface for extended control (it mimics .NET [Task](https://msdn.microsoft.com/ru-ru/library/system.threading.tasks.task(v=vs.110).aspx) as much as possible).
- There is no standard way to return a coroutine result value; while some of the `AsyncOperation`-derived classes define operation results, [WWW](https://docs.unity3d.com/ScriptReference/WWW.html) uses completely inconsistent way of doing this. There are generic versions of `IAsyncResult` and `IAsyncOperation` interfaces with result values in *UnityFx*.
- There is no easy way of chaining coroutines, waiting for completion of a coroutine group etc. *UnityFx* implements a set of extension methods for both `IAsyncResult` and `IAsyncOperation` interfaces that provide the above mentioned functionality.
- Coroutines can't handle exceptions, because yield return statements cannot be surrounded with a try-catch construction.
- *UnityFx* provides default yieldable implementations for both `IAsyncOperation` and `IAsyncResult` to allow easy library extension.

## Code samples

```csharp
IEnumerator Foo(Coroutine c, IEnumerator e, AsyncOperation op, IAsyncResult ar)
{
	var op1 = AsyncResult.FromCoroutine(c);
	var op2 = AsyncResult.FromEnumerator(e);
	var op3 = AsyncResult.FromAsyncOperation(op);

	// wait for the first operation to finish
	yield return op1;

	// wait for the second operation to finish and then load scene 'tt'
	yield return op2.ContinueWith((parentOp) => SceneManager.LoadSceneAsync("TestScene"));

	// wait for the rest of operations to finish; note than 'ar' may actually represent a coroutine or .NET asynchronous operation
	yield return AsyncResult.WhenAll(op3, ar);
}
```

## API Reference

TODO

## Software requirements

- [Microsoft Visual Studio 2017](https://www.visualstudio.com/vs/community/)
- [Unity3d](https://store.unity.com/)

## Contributing

- Clone repository: `git clone https://github.com/Arvtesh/UnityFx.Async.git`
- Build sources: `powershell .\build.ps1`
- Run unit-tests: `TODO`

## License

Please see the [MIT license](LICENSE.md) for details.