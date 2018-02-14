# Documentation

TODO

# Code samples

1) The simpliest way of creating asyncronous operations is using `AsyncResult` static helpers.

```csharp
var op1 = AsyncResult.FromEnumerator(GetEnum());
var op2 = AsyncResult.FromCoroutine(StartCoroutine(GetEnum()));
var op3 = AsyncResult.FromCoroutine(new WaitForSeconds(2));
var op4 = AsyncResult.FromAsyncOperation(SceneManager.LoadSceneAsync("TestScene"));
var op5 = AsyncResult.FromUpdateCallback<int>(c => c.SetResult(20));
```

2) There are several helpers that create completed operations:

```csharp
var op1 = AsyncResult.CompletedOperation;
var op2 = AsyncResult.FromCanceled();
var op3 = AsyncResult.FromException(new Exception());
var op5 = AsyncResult.FromResult(20);
```

3) You can use `yield` and `await` to wait for `IAsyncOperation` instances without blocking the calling thread (obviously `await` cannot be used on .NET 3.5):

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

4) Each `IAsyncOperation` maintains its status value (just like [Task](https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.task):

```csharp
var op = AsyncResult.FromEnumerator(GetEnum());
yield return op;

if (op.IsCompletedSuccessfully)
{
	// could also check op.Status
}
```

5) Operations can be chained together very much like [Tasks](https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.task) instances:

```csharp
var op1 = AsyncResult.Delay(TimeSpan.FromSeconds(10));
var op2 = op1.ContinueWith(op => AsyncResult.FromEnumerator(GetEnum()));
```