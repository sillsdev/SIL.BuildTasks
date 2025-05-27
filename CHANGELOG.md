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

### Deprecated

- [SIL.BuildTasks] Deprecated `AbandondedSuites` in favor of correctly spelled `AbandonedSuites`.

## [3.1.1] - 2025-03-18

### Changed

- [SIL.BuildTasks] Upgraded dependency on Markdig.Signed to version 0.37.0 (to be consistent with SIL.ReleaseTasks)

## [3.1.0] - 2025-02-22

### Added

- [SIL.ReleaseTasks] Added code to `CreateReleaseNotesHtml` to set the charset
  in a metadata element in the HTML head so that special characters such
  as the TM mark do not render as garbled text (#72)

## [3.0.0] - 2024-11-07

### Added

- [SIL.ReleaseTasks] Added filter option for project-specific entries (#48)
- [SIL.BuildTasks] Additionally build for .NetStandard 2.0

### Changed

- Target .NET 4.7.2 instead of 4.6.1
- SIL International changed to SIL Global
- BREAKING DEPENDENCY CHANGE: Upgraded to NUnit 4 (see https://docs.nunit.org/articles/nunit/release-notes/breaking-changes.html)

### Fixed

- [SIL.ReleaseTasks] Exclude links from CHANGELOG.md file when extracting release notes (#46)
- [SIL.BuildTasks] Fixed logging in `MakeWixForDirTree` task (#55)
- [SIL.BuildTasks] Improved FileUpdate replacement verification and error handling (#63)

## [2.5.0] - 2021-02-24

### Added

- [SIL.ReleaseTasks] Support for Keep a Changelog style CHANGELOG.md files

## [2.4.0] - 2021-01-22

### Added

- [SIL.BuildTasks] `Nunit3` task: Add `Process`, `Workers`, `Trace`, `Test`, `Agents`, and
  `Debug` properties for passing on to the NUnit console runner for tuning and debugging

- ReadMe.md Added windows instructions for building a package for local testing

## [2.3.4] - 2020-10-05

### Fixed

- [SIL.ReleaseTasks] Fix setting of `SilReleaseTasksPath` property

## [2.3.3] - 2020-07-31

### Fixed

- [SIL.ReleaseTasks] Fix failure when running with `dotnet pack`

## [2.3.2] - 2020-05-22

### Fixed

- [SIL.BuildTasks] `MakeWixForDirTree` task: fix `Exclude` property

## [2.3.1] - 2020-05-19

### Changed

- [SIL.BuildTasks] `NUnit3` task: Deal with differing options in `NUnit.ConsoleRunner` versions

## [2.3.0] - 2020-04-15

### Added

- [SIL.BuildTasks] Add `NormalizeLocales` Task to help work with Crowdin Localized files
- Create symbol nuget packages

## [2.2.0] - 2018-12-11

### Changed

- Add new property `AppendToReleaseNotesProperty` to `ReleaseNotesProperty` task
  that allows to add text to the end of the release notes.

## [2.1.0] - 2018-07-28

### Changed

- Change default date output format for `StampChangelogFileWithVersion` to
  `yyyy-MM-dd` (instead of `dd/MMM/yyyy`).
- Add new property `DateTimeFormat` to `StampChangelogFileWithVersion` task
  to allow specification of output format.

## [2.0.2] - 2018-07-02

### Fixed

- Implement workaround for [msbuild bug #3468](https://github.com/Microsoft/msbuild/issues/3468):
  use @ as escape character instead of \\ (backslash) for `VersionRegex` property

## [2.0.1] - 2018-06-29

### Changed

- Automatically set `PackageReleaseNotes` property
- [SetReleaseNotesProperty task] Remove empty lines
- [SetReleaseNotesProperty task] Add `VersionRegex` property, extract version number and mention
  in `PackageReleaseNotes` property
- Allow to skip setting `PackageReleaseNotes` property automatically by setting the
  `IgnoreSetReleaseNotesProp` property.

## [2.0.0] - 2018-06-18

### Changed

- Automatically add tasks by providing a `.props` file

### Added

- `SIL.ReleaseTasks` nuget package with three tasks: `CreateChangelogEntry`, `CreateReleaseNotesHtml`,
  and `StampChangelogFileWithVersion`

- `SetReleaseNotesProperty` task that sets a msbuild property with the release notes from a
  `CHANGELOG.md` file. This is useful for nuget packages.

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

[Unreleased]: https://github.com/sillsdev/SIL.BuildTasks/compare/v3.1.1...master

[3.1.1]: https://github.com/sillsdev/SIL.BuildTasks/compare/v3.1.0...v3.1.1
[3.1.0]: https://github.com/sillsdev/SIL.BuildTasks/compare/v3.0.0...v3.1.0
[3.0.0]: https://github.com/sillsdev/SIL.BuildTasks/compare/v2.5.0...v3.0.0
[2.5.0]: https://github.com/sillsdev/SIL.BuildTasks/compare/v2.4.0...v2.5.0
[2.4.0]: https://github.com/sillsdev/SIL.BuildTasks/compare/v2.3.0...v2.4.0
[2.3.4]: https://github.com/sillsdev/SIL.BuildTasks/compare/v2.3.3...v2.3.4
[2.3.3]: https://github.com/sillsdev/SIL.BuildTasks/compare/v2.3.2...v2.3.3
[2.3.2]: https://github.com/sillsdev/SIL.BuildTasks/compare/v2.3.1...v2.3.2
[2.3.1]: https://github.com/sillsdev/SIL.BuildTasks/compare/v2.3.0...v2.3.1
[2.3.0]: https://github.com/sillsdev/SIL.BuildTasks/compare/v2.2.0...v2.3.0
[2.2.0]: https://github.com/sillsdev/SIL.BuildTasks/compare/v2.1.0...v2.2.0
[2.1.0]: https://github.com/sillsdev/SIL.BuildTasks/compare/v2.0.1...v2.1.0
[2.0.1]: https://github.com/sillsdev/SIL.BuildTasks/compare/v2.0.0...v2.0.1
[2.0.0]: https://github.com/sillsdev/SIL.BuildTasks/compare/v1.0.2...v2.0.0
[1.0.2]: https://github.com/sillsdev/SIL.BuildTasks/compare/v1.0.1...v1.0.2
[1.0.1]: https://github.com/sillsdev/SIL.BuildTasks/compare/v1.0.0...v1.0.1
[1.0.0]: https://github.com/sillsdev/SIL.BuildTasks/compare/...v1.0.0
