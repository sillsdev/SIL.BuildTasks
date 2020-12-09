// Copyright (c) 2018 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.IO;
using NUnit.Framework;

namespace SIL.ReleaseTasks.Tests
{
	[TestFixture]
	public class StampChangelogFileWithVersionTests
	{
		[Test]
		public void StampMarkdownWorksWithDefault()
		{
			var testMarkdown = new StampChangelogFileWithVersion();
			using(var tempFiles = new TwoTempFilesForTest(Path.Combine(Path.GetTempPath(), "Test.md"), null))
			{
				File.WriteAllLines(tempFiles.FirstFile,
					new[] {"## DEV_VERSION_NUMBER: DEV_RELEASE_DATE", "*with some random content", "*does some things"});
				testMarkdown.ChangelogFile = tempFiles.FirstFile;
				testMarkdown.VersionNumber = "2.3.10";
				Assert.That(testMarkdown.Execute(), Is.True);
				var newContents = File.ReadAllLines(tempFiles.FirstFile);
				Assert.That(newContents.Length == 3);
				Assert.That(newContents[0], Is.EqualTo($"## 2.3.10 {DateTime.Now:yyyy-MM-dd}"));
			}
		}

		[Test]
		public void StampMarkdownCustomFormat()
		{
			var testMarkdown = new StampChangelogFileWithVersion();
			using(var tempFiles = new TwoTempFilesForTest(Path.Combine(Path.GetTempPath(), "Test.md"), null))
			{
				File.WriteAllLines(tempFiles.FirstFile,
					new[] {"## DEV_VERSION_NUMBER: DEV_RELEASE_DATE", "*with some random content", "*does some things"});
				testMarkdown.ChangelogFile = tempFiles.FirstFile;
				testMarkdown.VersionNumber = "2.3.10";
				testMarkdown.DateTimeFormat = "dd/MMM/yyyy";
				Assert.That(testMarkdown.Execute(), Is.True);
				var newContents = File.ReadAllLines(tempFiles.FirstFile);
				Assert.That(newContents.Length == 3);
				Assert.That(newContents[0], Is.EqualTo($"## 2.3.10 {DateTime.Now:dd/MMM/yyyy}"));
			}
		}

		[Test]
		public void ProcessesKeepAChangelogFormat()
		{
			var testMarkdown = new StampChangelogFileWithVersion();
			using(var tempFiles = new TwoTempFilesForTest(Path.Combine(Path.GetTempPath(), "CHANGELOG.md"), null))
			{
				string changelogContent =
@"All notable changes to this project will be documented in this file.

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
- This to that.

## [1.2.3] - 2020-12-08

### Added
- New features.

### Fixed
- All bugs.";

				string[] expectedNewChangelogContent =
@"All notable changes to this project will be documented in this file.

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

## [2.3.10] - DATE_HERE

### Changed
- This to that.

## [1.2.3] - 2020-12-08

### Added
- New features.

### Fixed
- All bugs.".Split('\n');
				int newExpectedHeadingLine = 17 - 1;
				expectedNewChangelogContent[newExpectedHeadingLine] = $"## [2.3.10] - {DateTime.Today.ToString("yyyy-MM-dd")}";
				File.WriteAllText(tempFiles.FirstFile, changelogContent);
				testMarkdown.ChangelogFile = tempFiles.FirstFile;
				testMarkdown.VersionNumber = "2.3.10";
				// SUT
				Assert.That(testMarkdown.Execute(), Is.True);
				var newContents = File.ReadAllLines(tempFiles.FirstFile);
				Assert.That(newContents, Is.EqualTo(expectedNewChangelogContent));
			}
		}
	}
}
