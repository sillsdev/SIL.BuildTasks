// Copyright (c) 2018 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System.IO;
using NUnit.Framework;

namespace SIL.ReleaseTasks.Tests
{
	[TestFixture]
	public class SetReleaseNotesPropertyTests
	{
		private string _tempFile;

		[TearDown]
		public void TearDown()
		{
			if (!string.IsNullOrEmpty(_tempFile))
				File.Delete(_tempFile);
		}

		[Test]
		public void ChangelogFileDoesntExist()
		{
			// Setup
			var sut = new SetReleaseNotesProperty();
			sut.BuildEngine = new MockEngine();
			_tempFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

			sut.ChangelogFile = _tempFile;

			// Exercise
			var result = sut.Execute();

			// Verify
			Assert.That(result, Is.False);
		}


		[Test]
		public void NoRelease()
		{
			// Setup
			var sut = new SetReleaseNotesProperty();
			sut.BuildEngine = new MockEngine();
			_tempFile = Path.GetTempFileName();

			File.WriteAllText(_tempFile, @"
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
-->");

			sut.ChangelogFile = _tempFile;

			// Exercise
			var result = sut.Execute();

			// Verify
			Assert.That(result, Is.False);
		}

		[Test]
		public void Unreleased()
		{
			// Setup
			var sut = new SetReleaseNotesProperty();
			sut.BuildEngine = new MockEngine();
			_tempFile = Path.GetTempFileName();

			File.WriteAllText(_tempFile, @"
# Change Log

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/)
and this project adheres to [Semantic Versioning](http: //semver.org/).

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

- This is a unit test");

			sut.ChangelogFile = _tempFile;

			// Exercise
			var result = sut.Execute();

			// Verify
			Assert.That(result, Is.True);
			Assert.That(sut.Value, Is.EqualTo(@"Changed:
- This is a unit test
"));
		}

		[Test]
		public void ColonAfterHeader()
		{
			// Setup
			var sut = new SetReleaseNotesProperty();
			sut.BuildEngine = new MockEngine();
			_tempFile = Path.GetTempFileName();

			File.WriteAllText(_tempFile, @"
# Change Log

## [Unreleased]

### Changed:

- This is a unit test");

			sut.ChangelogFile = _tempFile;

			// Exercise
			var result = sut.Execute();

			// Verify
			Assert.That(result, Is.True);
			Assert.That(sut.Value, Is.EqualTo(@"Changed:
- This is a unit test
"));
		}

		[Test]
		public void UnreleasedWithPreviousRelease()
		{
			// Setup
			var sut = new SetReleaseNotesProperty();
			sut.BuildEngine = new MockEngine();
			_tempFile = Path.GetTempFileName();

			File.WriteAllText(_tempFile, @"
# Change Log

## [Unreleased]

### Changed

- This is a unit test

## [1.0] - 2018-06-18

### Added

- added unit test
");

			sut.ChangelogFile = _tempFile;

			// Exercise
			var result = sut.Execute();

			// Verify
			Assert.That(result, Is.True);
			Assert.That(sut.Value, Is.EqualTo(@"Changes since version 1.0

Changed:
- This is a unit test
"));
		}

		[Test]
		public void UnreleasedWithPreviousReleaseAndCustomUnreleasedVersion()
		{
			// Setup
			var sut = new SetReleaseNotesProperty();
			sut.BuildEngine = new MockEngine();
			_tempFile = Path.GetTempFileName();

			File.WriteAllText(_tempFile, @"
# Change Log

## [Foo]

### Changed

- This is a unit test

## [1.0] - 2018-06-18

### Added

- added unit test
");

			sut.ChangelogFile = _tempFile;

			// Exercise
			var result = sut.Execute();

			// Verify
			Assert.That(result, Is.True);
			Assert.That(sut.Value, Is.EqualTo(@"Changes since version 1.0

Changed:
- This is a unit test
"));
		}

		[Test]
		public void EmptyUnreleasedWithPreviousRelease()
		{
			// Setup
			var sut = new SetReleaseNotesProperty();
			sut.BuildEngine = new MockEngine();
			_tempFile = Path.GetTempFileName();

			File.WriteAllText(_tempFile, @"
# Change Log

## [Unreleased]

## [1.0] - 2018-06-18

### Changed

- This is a unit test
");

			sut.ChangelogFile = _tempFile;

			// Exercise
			var result = sut.Execute();

			// Verify
			Assert.That(result, Is.True);
			Assert.That(sut.Value, Is.EqualTo(@"Changed:
- This is a unit test
"));
		}

		[Test]
		public void EmptyUnreleasedWithTwoPreviousReleases()
		{
			// Setup
			var sut = new SetReleaseNotesProperty();
			sut.BuildEngine = new MockEngine();
			_tempFile = Path.GetTempFileName();

			File.WriteAllText(_tempFile, @"
# Change Log

## [Unreleased]

## [1.0.5] - 2018-06-29

### Changed

- This is a unit test

## [1.0.0] - 2018-06-18

### Added

- added unit test
");

			sut.ChangelogFile = _tempFile;

			// Exercise
			var result = sut.Execute();

			// Verify
			Assert.That(result, Is.True);
			Assert.That(sut.Value, Is.EqualTo(@"Changes since version 1.0.0

Changed:
- This is a unit test
"));
		}

		[Test]
		public void ReleaseWithPreviousRelease()
		{
			// Setup
			var sut = new SetReleaseNotesProperty();
			sut.BuildEngine = new MockEngine();
			_tempFile = Path.GetTempFileName();

			File.WriteAllText(_tempFile, @"
# Change Log

## [1.0.5] - 2018-06-29

### Changed

- This is a unit test

## [1.0.0] - 2018-06-18

### Added

- added unit test
");

			sut.ChangelogFile = _tempFile;

			// Exercise
			var result = sut.Execute();

			// Verify
			Assert.That(result, Is.True);
			Assert.That(sut.Value, Is.EqualTo(@"Changes since version 1.0.0

Changed:
- This is a unit test
"));
		}

		[Test]
		public void ReleaseWithMultipleSubheadings()
		{
			// Setup
			var sut = new SetReleaseNotesProperty();
			sut.BuildEngine = new MockEngine();
			_tempFile = Path.GetTempFileName();

			File.WriteAllText(_tempFile, @"
# Change Log

## [1.0] - 2018-06-18

### Changed

- This is a unit test

### Added

- added unit test

");

			sut.ChangelogFile = _tempFile;

			// Exercise
			var result = sut.Execute();

			// Verify
			Assert.That(result, Is.True);
			Assert.That(sut.Value, Is.EqualTo(@"Changed:
- This is a unit test

Added:
- added unit test
"));
		}

	}
}