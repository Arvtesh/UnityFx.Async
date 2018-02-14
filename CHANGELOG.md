# UnityFx.Async changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/); this project adheres to [Semantic Versioning](http://semver.org/).

## [0.7.1] - 2018-02-14

### Added
- Added possibility to store multiple exceptions in `AsyncResult`.
- Added `AggregateException` class for `net35` target.
- Added `IsEmpty` property to `AsyncResultQueue`.

### Changed
- `AsyncResult` implemenatino is changed to prevent returning null operation result when the operation is completed in some cases.

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

## [0.3.1] - 2017-08-01

### Added
- Initial release.

