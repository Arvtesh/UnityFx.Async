# UnityFx.Async changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/); this project adheres to [Semantic Versioning](http://semver.org/).

-----------------------
## [Unreleased]

### Added
- New `AsyncResult` constructors added.
- New overloads for `AsyncResult.FromResult`, `AsyncResult.FromCanceled` and `AsyncResult.FromException` methods added.

### Changed
- Made optimizations to `ToTask` extensions implementation for cases when target operation is completed.
- Marked all assembly classes CLS-compilant.

### Fixed
- `ToTask` extensions now throw inner exception instead of the `AggregateException` in case of an error.
- Fixed `AsyncResult.Delay` throwing exception when infinite delay (-1) specified.

-----------------------
## [0.8.1] - 2018-03-19

### Added
- Added `AsyncResultQueue.Empty` event.
- Added `Unity`-specific tools for Asset Store.

-----------------------
## [0.8.0] - 2018-03-10

### Added
- Added `IAsyncCompletionSource.Operation` property to match `TaskCompletionSource` interface.
- Added new constructor to `AsyncResult`.
- Added `AsyncResult.Start` method to match `Task` interface.
- Added `AsyncResult.OnStarted` virtual method.
- Added `WhenAll`/`WhenAny` static helpers for `AsyncResult`.
- Added `ConfigureAwait` extensions for `IAsyncOperation`.
- Added `Task` extension methods to that convert it to an `AsyncResult` instance.
- Added `AsyncResult.Retry` methods.
- Added `Wait` overloads to match `Task` interface.

### Removed
- Removed `AsyncResult.TryCreateAsyncWaitHandle` helper.

### Changed
- Modified `AsyncResultAwaiter` implementation to throw if the operation was canceled or faulted (to match `TaskAwaiter` behaviour).
- Implemented `AsyncCompletionSource` as a sealed analog of `TaskCompletionSource`.
- Removed public completion methods from `AsyncResult` (moved them to `AsyncCompletionSource`).
- Made `SpinUntilCompleted` an extension method (was `AsyncResult` instance method).
- Changed `IAsyncOperation.Exception` type to `AggregateException` to match `Task`.
- Changed `IAsyncOperationEvents.Completed` event signature & behaviour to always execute handler (event if it was registered after the comperation has copleted).
- Removed `AsyncResult` constructors that accepted exceptions.
- Changed `AsyncResult.Result` property to throw `AggregateException` when faulted or canceled to mathch `Task` behaviour.

### Fixed
- `AsyncResultQueue` now does not remove uncompleted operations from the queue.
- `AsyncResult.Exception` now only returns non-null value when the operation is faulted.

-----------------------
## [0.7.1] - 2018-02-14

### Added
- Added possibility to store multiple exceptions in `AsyncResult`.
- Added `AggregateException` class for `net35` target.
- Added `IsEmpty` property to `AsyncResultQueue`.

### Changed
- `AsyncResult` implemenation is changed to prevent returning null operation result when the operation is completed in some cases.

-----------------------
## [0.7.0] - 2018-02-10

### Added
- Added project documentation site (`docs` folder).
- Added unit tests project.
- Added continuations support.
- Added [AppVeyor CI](https://ci.appveyor.com/project/Arvtesh/unityfx-async) support.
- Added [NuGet](https://www.nuget.org/packages/UnityFx.Async/) deployment.
- Added [GitVersion](https://gitversion.readthedocs.io/en/latest/) support.

### Changed
- Renamed `AsyncOperationStatus` values to match [TastStatus](https://msdn.microsoft.com/ru-ru/library/system.threading.tasks.taskstatus(v=vs.110).aspx).

-----------------------
## [0.3.1] - 2017-08-01

### Added
- Initial release.

