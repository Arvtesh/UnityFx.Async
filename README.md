# UnityFx.Async

Channel  | Branch | UnityFx.Async |
---------|--------|---------------|
AppVeyor ([home](https://ci.appveyor.com/project/Arvtesh/unityfx-async)) | master | [![Build status](https://ci.appveyor.com/api/projects/status/hfmq9vow53al7tpd/branch/master?svg=true)](https://ci.appveyor.com/project/Arvtesh/unityfx-async/branch/master)
AppVeyor | develop | [![Build status](https://ci.appveyor.com/api/projects/status/hfmq9vow53al7tpd/branch/develop?svg=true)](https://ci.appveyor.com/project/Arvtesh/unityfx-async/branch/develop)

## Synopsis

Unity3d extensions for asynchronous operations and coroutines. The library provides a set of classes and interfaces that can be used very much like [TPL](https://msdn.microsoft.com/ru-ru/library/dd460717(v=vs.110).aspx).

Please see [CHANGELOG](CHANGELOG.md) for information on recent changes.

Unity Asset Store [link](https://www.assetstore.unity3d.com/#!/content/96696).

## Motivation

Unity3d provides powerful set of tools for game development. While it is a great engine there are quite a few places where its API is not ideal. Asynchronous operations and coroutine management are examples of such weaknesses. While the concept of coroutines itself is great for frame-based applications, Unity implementation is not consistent at least:
- There is no single base class/interface for yieldable entities; for example [Coroutine](https://docs.unity3d.com/ScriptReference/Coroutine.html) and [AsyncOperation](https://docs.unity3d.com/ScriptReference/AsyncOperation.html) both inherit [YieldInstruction](https://docs.unity3d.com/ScriptReference/YieldInstruction.html), while [CustomYieldInstruction](https://docs.unity3d.com/ScriptReference/CustomYieldInstruction.html) and [WWW](https://docs.unity3d.com/ScriptReference/WWW.html) do not. *UnityFx* uses .NET [IAsyncResult](https://msdn.microsoft.com/en-us/library/system.iasyncresult(v=vs.110).aspx) interface as a base for all its asynchronous operations and provides wrappers for native Unity3d operations.
- Running a coroutine requires [MonoBehaviour](https://docs.unity3d.com/ScriptReference/MonoBehaviour.html) instance which is not always convenient. *UnityFx* provides possibility to start coroutines without specifying a `MonoBehaviour`. It also provides a mechanism of defining custom coroutine runners via `AsyncScheduler` abstract class.
- Unity3d built-in asynchronous operations provide very little control after they have been started, [Coroutine](https://docs.unity3d.com/ScriptReference/Coroutine.html) for example doesn't even provide a way to determine if it is completed. *UnityFx* defines `IAsyncOperation` interface for extended control (it mimics .NET [Task](https://msdn.microsoft.com/ru-ru/library/system.threading.tasks.task(v=vs.110).aspx) as much as possible).
- There is no standard way to return a coroutine result value; while some of the [AsyncOperation](https://docs.unity3d.com/ScriptReference/AsyncOperation.html)-derived classes define operation results, [WWW](https://docs.unity3d.com/ScriptReference/WWW.html) uses completely inconsistent way of doing this. There is a generic version of `IAsyncOperation` interface with result values in *UnityFx* (again very similar to [Task<T>](https://msdn.microsoft.com/ru-ru/library/dd321424(v=vs.110).aspx)).
- There is no easy way of chaining coroutines, waiting for completion of a coroutine group etc. *UnityFx* implements a set of extension methods for both `IAsyncResult` and `IAsyncOperation` interfaces that provide the above mentioned functionality.
- Coroutines can't handle exceptions, because yield return statements cannot be surrounded with a try-catch construction. *UnityFx* stores finishes the corresponding operation with an error if any exceptions are being thrown inside its coroutine update loop.
- *UnityFx* provides default yieldable/awaitable implementation for `IAsyncOperation` and `IAsyncResult` to allow easy library extension.

## Code samples

1) The simpliest way of creating asyncronous operations is using `AsyncResult` statis helpers. No `MonoBehaviour` is needed (still there is always a way to specify one).

```csharp
var op1 = AsyncResult.FromEnumerator(GetEnum());
var op2 = AsyncResult.FromCoroutine(StartCoroutine(GetEnum()));
var op3 = AsyncResult.FromCoroutine(new WaitForSeconds(2));
var op4 = AsyncResult.FromAsyncOperation(SceneManager.LoadSceneAsync("TestScene"));
var op5 = AsyncResult.FromUpdateCallback<int>(c => c.SetResult(20));
```

2) There are several helpers that create completed operations:

```csharp
var op1 = AsyncResult.Completed;
var op2 = AsyncResult.Canceled;
var op3 = AsyncResult.FromException(new Exception());
var op4 = AsyncResult.FromException<SomeClass>(new Exception());
var op5 = AsyncResult.FromResult<int>(20);
```

3) To execute an operation on a specific `MonoBehaviour` use `AsyncFactory`:

```csharp
var factory = new AsyncFactory(monoBehaviour);
var op1 = factory.FromEnumerator(GetEnum());
var op2 = factory.FromUpdateCallback(c => c.SetCompleted());
```

4) You can use `yield` and `await` to wait for `IAsyncOperation` instances without blocking the calling thread:

```csharp
IEnumerator TestYield()
{
	yield return AsyncResult.FromEnumerator(GetEnum());
}

async Task TestAwait()
{
	await AsyncResult.FromEnumerator(GetEnum());
}
```

5) Each `IAsyncOperation` maintains its status value (just like [Task](https://msdn.microsoft.com/ru-ru/library/system.threading.tasks.task(v=vs.110).aspx)):

```csharp
var op = AsyncResult.FromEnumerator(GetEnum());
yield return op;

if (op.IsCompletedSuccessfully)
{
	// could also check op.Status
}
```

6) Several operations (basically everything that derives [IAsyncResult](https://msdn.microsoft.com/en-us/library/system.iasyncresult(v=vs.110).aspx)) can be combited into single operation:

```csharp
var op1 = AsyncResult.FromUpdateCallback(c => c.SetCanceled());
var op2 = Task.Run(() => ThreadSleep(100));
var op3 = AsyncResult.WhenAll(op1, op2);
```

7) Operations can be chained together very much like [Tasks](https://msdn.microsoft.com/ru-ru/library/system.threading.tasks.task(v=vs.110).aspx):

```csharp
var op1 = AsyncResult.Delay(TimeSpan.FromSeconds(10));
var op2 = op1.ContinueWith(op => AsyncResult.FromEnumerator(GetEnum()));
```

## Software requirements

- [Microsoft Visual Studio 2017](https://www.visualstudio.com/vs/community/)
- [Unity3d](https://store.unity.com/)

## Contributing

Please see [contributing guide](CONTRIBUTING.md) for details.

## License

Please see the [MIT license](LICENSE.md) for details.