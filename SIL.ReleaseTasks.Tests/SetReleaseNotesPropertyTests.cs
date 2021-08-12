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
			sut.ChangelogFile = _tempFile;

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
			sut.ChangelogFile = _tempFile;

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
			sut.ChangelogFile = _tempFile;

			File.WriteAllText(_tempFile, @"
# Change Log

## [Unreleased]

### Changed:

- This is a unit test");

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
			sut.ChangelogFile = _tempFile;

			File.WriteAllText(_tempFile, @"
# Change Log

## [Unreleased]

### Changed

- This is a unit test

## [1.0] - 2018-06-18

### Added

- added unit test
");

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
			sut.ChangelogFile = _tempFile;

			File.WriteAllText(_tempFile, @"
# Change Log

## [Foo]

### Changed

- This is a unit test

## [1.0] - 2018-06-18

### Added

- added unit test
");

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
			sut.ChangelogFile = _tempFile;

			File.WriteAllText(_tempFile, @"
# Change Log

## [Unreleased]

## [1.0] - 2018-06-18

### Changed

- This is a unit test
");

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
			sut.ChangelogFile = _tempFile;

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
			sut.ChangelogFile = _tempFile;

			File.WriteAllText(_tempFile, @"
# Change Log

## [1.0.5] - 2018-06-29

### Changed

- This is a unit test

## [1.0.0] - 2018-06-18

### Added

- added unit test
");

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
			sut.ChangelogFile = _tempFile;

			File.WriteAllText(_tempFile, @"
# Change Log

## [1.0] - 2018-06-18

### Changed

- This is a unit test

### Added

- added unit test

");

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

		[Test]
		public void CustomVersionRegex()
		{
			// Setup
			var sut = new SetReleaseNotesProperty();
			sut.BuildEngine = new MockEngine();
			sut.VersionRegex = @"#+ @d+-@d+-@d+ @@[a-z]+ @(([^)]+)@)|## Unreleased";
			_tempFile = Path.GetTempFileName();
			sut.ChangelogFile = _tempFile;

			File.WriteAllText(_tempFile, @"
# Change Log

## Unreleased

### Changed:

- This is a unit test

## 2018-07-02 @foo (5.0)

### Added

- added unit test");

			// Exercise
			var result = sut.Execute();

			// Verify
			Assert.That(result, Is.True);
			Assert.That(sut.Value, Is.EqualTo(@"Changes since version 5.0

Changed:
- This is a unit test
"));
		}

		[Test]
		public void AppendToReleaseNotesProperty()
		{
			// Setup
			var sut = new SetReleaseNotesProperty();
			sut.BuildEngine = new MockEngine();
			sut.AppendToReleaseNotesProperty = @"
See full changelog at github.";
			_tempFile = Path.GetTempFileName();
			sut.ChangelogFile = _tempFile;

			File.WriteAllText(_tempFile, @"
# Change Log

## [Unreleased]

### Changed:

- This is a unit test");

			// Exercise
			var result = sut.Execute();

			// Verify
			Assert.That(result, Is.True);
			Assert.That(sut.Value, Is.EqualTo(@"Changed:
- This is a unit test

See full changelog at github.
"));
		}

		[Test]
		public void UrlInChangeLog()
		{
			// Setup
			var sut = new SetReleaseNotesProperty();
			sut.BuildEngine = new MockEngine();
			_tempFile = Path.GetTempFileName();
			sut.ChangelogFile = _tempFile;

			File.WriteAllText(_tempFile, @"
# Change Log

## [Unreleased]

### Changed

- This is a unit test

[Unreleased]: https://example.com
");

			// Exercise
			var result = sut.Execute();

			// Verify
			Assert.That(result, Is.True);
			Assert.That(sut.Value, Is.EqualTo(@"Changed:
- This is a unit test
"));
		}

		[Test]
		public void FilterEntriesTrueSomeOtherProject()
		{
			// Setup
			var sut = new SetReleaseNotesProperty();
			sut.BuildEngine = new MockEngine();
			_tempFile = Path.GetTempFileName();
			sut.FilterEntries = true;
			sut.PackageId = "Some.OtherProject";
			sut.ChangelogFile = _tempFile;

			File.WriteAllText(_tempFile, @"
# Change Log

## [Unreleased]

## [2.4.0] - 2021-01-22

### Added

- [MyProject1] Add 'DoSomething()' method to 'Foo'
- 'ReadMe.md' Lorem ipsum

### Fixed

- [Some.OtherProject] Fix crash in 'Foobar'

## [2.3.0]
");
			// Exercise
			var result = sut.Execute();

			// Verify
			Assert.That(result, Is.True);
			Assert.That(sut.Value, Is.EqualTo(@"Changes since version 2.3.0

Added:
- 'ReadMe.md' Lorem ipsum

Fixed:
- Fix crash in 'Foobar'
"));

		}

		[Test]
		public void FilterEntriesFalseSomeOtherProject()
		{
			// Setup
			var sut = new SetReleaseNotesProperty();
			sut.BuildEngine = new MockEngine();
			_tempFile = Path.GetTempFileName();
			sut.FilterEntries = false;
			sut.PackageId = "Some.OtherProject";
			sut.ChangelogFile = _tempFile;

			File.WriteAllText(_tempFile, @"
# Change Log

## [Unreleased]

## [2.4.0] - 2021-01-22

### Added

- [MyProject1] Add 'DoSomething()' method to 'Foo'
- 'ReadMe.md' Lorem ipsum

### Fixed

- [Some.OtherProject] Fix crash in 'Foobar'

## [2.3.0]
");

			// Exercise
			var result = sut.Execute();

			// Verify
			Assert.That(result, Is.True);
			Assert.That(sut.Value, Is.EqualTo(@"Changes since version 2.3.0

Added:
- [MyProject1] Add 'DoSomething()' method to 'Foo'
- 'ReadMe.md' Lorem ipsum

Fixed:
- [Some.OtherProject] Fix crash in 'Foobar'
"));

		}

		[Test]
		public void FilterEntriesTrueMyProject1()
		{
			// Setup
			var sut = new SetReleaseNotesProperty();
			sut.BuildEngine = new MockEngine();
			_tempFile = Path.GetTempFileName();
			sut.FilterEntries = true;
			sut.PackageId = "MyProject1";
			sut.ChangelogFile = _tempFile;

			File.WriteAllText(_tempFile, @"
# Change Log

## [Unreleased]

## [2.4.0] - 2021-01-22

### Added

- [MyProject1] Add 'DoSomething()' method to 'Foo'
- 'ReadMe.md' Lorem ipsum

### Fixed

- [Some.OtherProject] Fix crash in 'Foobar'

## [2.3.0]
");

			// Exercise
			var result = sut.Execute();

			// Verify
			Assert.That(result, Is.True);
			Assert.That(sut.Value, Is.EqualTo(@"Changes since version 2.3.0

Added:
- Add 'DoSomething()' method to 'Foo'
- 'ReadMe.md' Lorem ipsum
"));

		}

		[Test]
		public void FilterEntriesFalseMyProject1()
		{
			// Setup
			var sut = new SetReleaseNotesProperty();
			sut.BuildEngine = new MockEngine();
			_tempFile = Path.GetTempFileName();
			sut.FilterEntries = false;
			sut.PackageId = "MyProject1";
			sut.ChangelogFile = _tempFile;

			File.WriteAllText(_tempFile, @"
# Change Log

## [Unreleased]

## [2.4.0] - 2021-01-22

### Added

- [MyProject1] Add 'DoSomething()' method to 'Foo'
- 'ReadMe.md' Lorem ipsum

### Fixed

- [Some.OtherProject] Fix crash in 'Foobar'

## [2.3.0]
");

			// Exercise
			var result = sut.Execute();

			// Verify
			Assert.That(result, Is.True);
			Assert.That(sut.Value, Is.EqualTo(@"Changes since version 2.3.0

Added:
- [MyProject1] Add 'DoSomething()' method to 'Foo'
- 'ReadMe.md' Lorem ipsum

Fixed:
- [Some.OtherProject] Fix crash in 'Foobar'
"));
		}

		[Test]
		public void FilterEntriesUnnecessaryHeaders()
		{
			// Setup
			var sut = new SetReleaseNotesProperty();
			sut.BuildEngine = new MockEngine();
			_tempFile = Path.GetTempFileName();
			sut.FilterEntries = true;
			sut.PackageId = "MyProject1";
			sut.ChangelogFile = _tempFile;

			File.WriteAllText(_tempFile, @"
# Change Log

## [Unreleased]

## [2.4.0] - 2021-01-22

### Added

- [Some.OtherProject] Add 'DoSomething()' method to 'Foo'

### Fixed

- [MyProject1] Fix crash in 'Foobar'
- 'ReadMe.md' Lorem ipsum

## [2.3.0]
");

			// Exercise
			var result = sut.Execute();

			// Verify
			Assert.That(result, Is.True);
			Assert.That(sut.Value, Is.EqualTo(@"Changes since version 2.3.0

Fixed:
- Fix crash in 'Foobar'
- 'ReadMe.md' Lorem ipsum
"));

		}

		[Test]
		public void FilterEntriesOtherUnnecessaryHeaders()
		{
			// Setup
			var sut = new SetReleaseNotesProperty();
			sut.BuildEngine = new MockEngine();
			_tempFile = Path.GetTempFileName();
			sut.FilterEntries = true;
			sut.PackageId = "AnotherProject";
			sut.ChangelogFile = _tempFile;

			File.WriteAllText(_tempFile, @"
# Change Log

## [Unreleased]

## [2.4.0] - 2021-01-22

### Added

- [Some.OtherProject] Add 'DoSomething()' method to 'Foo'

### Fixed

- [MyProject1] Fix crash in 'Foobar'
- 'ReadMe.md' Lorem ipsum

## [2.3.0]
");

			// Exercise
			var result = sut.Execute();

			// Verify
			Assert.That(result, Is.True);
			Assert.That(sut.Value, Is.EqualTo(@"Changes since version 2.3.0

Fixed:
- 'ReadMe.md' Lorem ipsum
"));

		}

		[Test]
		public void FilterEntriesTrueMyProject1_EOF()
		{
			// Setup
			_tempFile = Path.GetTempFileName();
			var sut = new SetReleaseNotesProperty
			{
				BuildEngine = new MockEngine(),
				ChangelogFile = _tempFile,
				PackageId = "MyProject1",
				FilterEntries = true

			};

			File.WriteAllText(_tempFile, @"
# Change Log

## [Unreleased]

## [2.4.0] - 2021-01-22

### Added

- [MyProject1] Add 'DoSomething()' method to 'Foo'
- 'ReadMe.md' Lorem ipsum

### Fixed

- [Some.OtherProject] Fix crash in 'Foobar'
- [MyProject1] Fixed 'Something()'
");
			// Exercise
			var result = sut.Execute();

			// Verify
			Assert.That(result, Is.True);
			Assert.That(sut.Value, Is.EqualTo(@"Added:
- Add 'DoSomething()' method to 'Foo'
- 'ReadMe.md' Lorem ipsum

Fixed:
- Fixed 'Something()'
"));
		}

		[Test]
		public void FilterEntriesTrueMyProject1_LongLines()
		{
			// Setup
			_tempFile = Path.GetTempFileName();
			var sut = new SetReleaseNotesProperty
			{
				BuildEngine = new MockEngine(),
				ChangelogFile = _tempFile,
				PackageId = "MyProject1",
				FilterEntries = true

			};

			File.WriteAllText(_tempFile, @"
# Change Log

## [Unreleased]

## [2.4.0] - 2021-01-22

### Added

- [MyProject1] Add 'DoSomething()' method to 'Foo'
  continued on second line
- 'ReadMe.md' Lorem ipsum
  also continued

### Fixed

- [Some.OtherProject] Fix crash in 'Foobar'
  other project continued
- [MyProject1] Fixed 'Something()'
  something continued

## [2.3.0]
");
			// Exercise
			var result = sut.Execute();

			// Verify
			Assert.That(result, Is.True);
			Assert.That(sut.Value, Is.EqualTo(@"Changes since version 2.3.0

Added:
- Add 'DoSomething()' method to 'Foo'
  continued on second line
- 'ReadMe.md' Lorem ipsum
  also continued

Fixed:
- Fixed 'Something()'
  something continued
"));
		}

		[Test]
		public void FilterEntriesWithoutPackage()
		{
			// Setup
			var sut = new SetReleaseNotesProperty();
			sut.BuildEngine = new MockEngine();
			_tempFile = Path.GetTempFileName();
			sut.FilterEntries = true;
			sut.PackageId = "AnotherProject";
			sut.ChangelogFile = _tempFile;

			File.WriteAllText(_tempFile, @"
# Change Log

## [Unreleased]

## [2.4.0] - 2021-01-22

### Added

- Add 'DoSomething()' method to 'Foo'

### Fixed

- Fix crash in 'Foobar'
- 'ReadMe.md' Lorem ipsum

## [2.3.0]
");

			// Exercise
			var result = sut.Execute();

			// Verify
			Assert.That(result, Is.True);
			Assert.That(sut.Value, Is.EqualTo(@"Changes since version 2.3.0

Added:
- Add 'DoSomething()' method to 'Foo'

Fixed:
- Fix crash in 'Foobar'
- 'ReadMe.md' Lorem ipsum
"));

		}

		[Test]
		public void FilterEntries_UnreleasedWithComment()
		{
			// Setup
			var sut = new SetReleaseNotesProperty();
			sut.BuildEngine = new MockEngine();
			_tempFile = Path.GetTempFileName();
			sut.FilterEntries = true;
			sut.PackageId = "MyProject1";
			sut.ChangelogFile = _tempFile;

			File.WriteAllText(_tempFile, @"
# Change Log

<!-- comment
-->

## [Unreleased]

### Added

- [MyProject1] Add 'DoSomething()' method to 'Foo'
- 'ReadMe.md' Lorem ipsum

## [2.4.0] - 2021-01-22
");

			// Exercise
			var result = sut.Execute();

			// Verify
			Assert.That(result, Is.True);
			Assert.That(sut.Value, Is.EqualTo(@"Changes since version 2.4.0

Added:
- Add 'DoSomething()' method to 'Foo'
- 'ReadMe.md' Lorem ipsum
"));
		}

		[Test]
		public void FilterEntries_ReleasedWithComment()
		{
			// Setup
			var sut = new SetReleaseNotesProperty();
			sut.BuildEngine = new MockEngine();
			_tempFile = Path.GetTempFileName();
			sut.FilterEntries = true;
			sut.PackageId = "MyProject1";
			sut.ChangelogFile = _tempFile;

			File.WriteAllText(_tempFile, @"
# Change Log

<!-- comment
-->

## [Unreleased]

## [2.4.0] - 2021-01-22

### Added

- [MyProject1] Add 'DoSomething()' method to 'Foo'
- 'ReadMe.md' Lorem ipsum

## [2.3.0] - 2021-01-21
");

			// Exercise
			var result = sut.Execute();

			// Verify
			Assert.That(result, Is.True);
			Assert.That(sut.Value, Is.EqualTo(@"Changes since version 2.3.0

Added:
- Add 'DoSomething()' method to 'Foo'
- 'ReadMe.md' Lorem ipsum
"));
		}

		[Test]
		public void FilterEntries_UnreleasedUnfilteredWithComment()
		{
			// Setup
			var sut = new SetReleaseNotesProperty();
			sut.BuildEngine = new MockEngine();
			_tempFile = Path.GetTempFileName();
			sut.FilterEntries = false;
			sut.PackageId = "MyProject1";
			sut.ChangelogFile = _tempFile;

			File.WriteAllText(_tempFile, @"
# Change Log

<!-- comment
-->

## [Unreleased]

### Added

- [MyProject1] Add 'DoSomething()' method to 'Foo'
- 'ReadMe.md' Lorem ipsum

## [2.4.0] - 2021-01-22
");

			// Exercise
			var result = sut.Execute();

			// Verify
			Assert.That(result, Is.True);
			Assert.That(sut.Value, Is.EqualTo(@"Changes since version 2.4.0

Added:
- [MyProject1] Add 'DoSomething()' method to 'Foo'
- 'ReadMe.md' Lorem ipsum
"));
		}

	}
}
