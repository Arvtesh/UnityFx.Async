# UnityFx.Async changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/); this project adheres to [Semantic Versioning](http://semver.org/).

## [0.7.0] - 2017-XX-XX

### Added
- Added project documentation site (`docs` folder).
- Added unit tests for `AsyncResult`.
- Added `AsyncContinuationOptions` support to continuations.
- Added [AppVeyor CI](https://ci.appveyor.com/project/Arvtesh/unityfx-async) support.
- Added [GitVersion](https://gitversion.readthedocs.io/en/latest/) support.

### Changed
- Renamed `AsyncOperationStatus` values to match [TastStatus](https://msdn.microsoft.com/ru-ru/library/system.threading.tasks.taskstatus(v=vs.110).aspx).
- Renamed `AsyncContinuationOptions` values to match [TastContinuationOptions](https://msdn.microsoft.com/ru-ru/library/system.threading.tasks.taskcontinuationoptions(v=vs.110).aspx).

## [0.3.1] - 2017-08-01

### Added
- Initial release.

