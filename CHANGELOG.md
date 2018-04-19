# Change Log

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/)
and this project adheres to [Semantic Versioning](http://semver.org/).

<!-- Available types of changes:
### Added
### Changed
### Fixed
### Deprecated
### Removed
### Security
-->

## [Unreleased]

### Changed

- Automatically add tasks by providing a `.props` file

### Added

- `SIL.ReleaseTasks` nuget package with three tasks: `CreateChangelogEntry`, `CreateReleaseNotesHtml`,
  and `StampChangelogFileWithVersion`

### Removed

- The `GenerateReleaseArtifacts` task got removed from `SIL.BuildTasks` and split into three
  separate tasks (see above). See [document](Documentation/Migration.md#upgrade-to-version-2) for help
  migrating existing build scripts.

## [1.0.2] - 2018-06-08

### Changed

- Include all dependencies. This makes it easier to consume the tasks in msbuild scripts.

## [1.0.1] - 2018-04-20

### Added

- added documentation for `SIL.BuildTasks`

### Changed

- [nunit] Check existence of nunit executable
- [nunit] Allow to set `FailTaskIfAnyTestsFail` property in msbuild script

### Fixed

- [nunit] Don't crash on Linux if no toolpath is specified

## [1.0.0] - 2018-04-16

### Added

- First release as NuGet package