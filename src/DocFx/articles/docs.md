# Documentation

TODO

# Code samples

1) The simpliest way of creating asyncronous operations is using `AsyncResult` static helpers. No `MonoBehaviour` is needed.

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