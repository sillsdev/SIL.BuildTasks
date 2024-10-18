// Copyright (c) 2018 SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace SIL.ReleaseTasks.Tests
{
	[TestFixture]
	public class CreateChangelogEntryTests
	{
		[Test]
		public void UpdateDebianChangelogWorks()
		{
			// Setup
			var sut = new CreateChangelogEntry();
			using(var tempFiles = new TwoTempFilesForTest(Path.Combine(Path.GetTempPath(), "Test.md"),
				Path.Combine(Path.GetTempPath(), "changelog")))
			{
				var mdFile = tempFiles.FirstFile;
				var changeLogFile = tempFiles.SecondFile;
				File.WriteAllLines(mdFile, new[] {"## 2.3.10: 4/Sep/2014", "* with some random content", "* does some things"});
				File.WriteAllLines(changeLogFile, new[]
				{
					"myfavoriteapp (2.1.0~alpha1) unstable; urgency=low", "", "  * Initial Release for Linux.", "",
					" -- Stephen McConnel <stephen_mcconnel@example.com>  Fri, 12 Jul 2013 14:57:59 -0500", ""
				});
				sut.ChangelogFile = mdFile;
				sut.VersionNumber = "2.3.11";
				sut.PackageName = "myfavoriteapp";
				sut.MaintainerInfo = "Steve McConnel <stephen_mcconnel@example.com>";
				sut.DebianChangelog = changeLogFile;
				sut.Distribution = "unstable";
				string expectedDebianChangelog =
@"myfavoriteapp (2.3.11) unstable; urgency=low

  * with some random content
  * does some things

 -- Steve McConnel <stephen_mcconnel@example.com>  DATE_NOW

myfavoriteapp (2.1.0~alpha1) unstable; urgency=low

  * Initial Release for Linux.

 -- Stephen McConnel <stephen_mcconnel@example.com>  Fri, 12 Jul 2013 14:57:59 -0500
";
				expectedDebianChangelog = expectedDebianChangelog.Replace("DATE_NOW", CreateChangelogEntry.DebianDate(sut.EntryDate));

				// Execute
				Assert.That(sut.Execute(), Is.True);

				// Verify
				var newContents = File.ReadAllLines(changeLogFile);
				string newContentsText = string.Join(Environment.NewLine, newContents);
				Assert.That(newContentsText, Is.EqualTo(expectedDebianChangelog));
				// Make sure that the author line matches debian standards for time offset and spacing around author name
				Assert.That(newContents[5], Does.Match(" -- " + sut.MaintainerInfo + "  .*[+-]\\d\\d\\d\\d"));
			}
		}

		[Test]
		public void GenerateNewDebianChangelogEntry_InterpretsKAC()
		{
			// Setup
			var sut = new CreateChangelogEntry();

			string changelogContent =
@"# Change Log

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

## [2.3.11] - 2020-12-05

### Changed
- This to that.

### Fixed
- Unplanned bugs.

## [1.2.3] - 2020-12-01

### Added
- New features.

### Fixed
- All bugs.";

			string expectedNewChangelogEntry = @"myfavoriteapp (2.3.11) unstable; urgency=low

  * Changed
    * This to that.

  * Fixed
    * Unplanned bugs.

 -- Steve McConnel <stephen_mcconnel@example.com>  DATE_NOW
";
			expectedNewChangelogEntry = expectedNewChangelogEntry.Replace("DATE_NOW", CreateChangelogEntry.DebianDate(sut.EntryDate));

			sut.VersionNumber = "2.3.11";
			sut.PackageName = "myfavoriteapp";
			sut.MaintainerInfo = "Steve McConnel <stephen_mcconnel@example.com>";
			sut.Distribution = "unstable";

			// Execute
			List<string> output = sut.GenerateNewDebianChangelogEntry(changelogContent.Split(new [] { Environment.NewLine }, StringSplitOptions.None));
			string actualEntry = string.Join(Environment.NewLine, output);

			// Verify
			Assert.That(actualEntry, Is.EqualTo(expectedNewChangelogEntry));
		}

		[Test]
		public void UpdateDebianChangelogAllMdListItemsWork()
		{
			// Setup
			var sut = new CreateChangelogEntry();
			using(var tempFiles = new TwoTempFilesForTest(Path.Combine(Path.GetTempPath(), "Test.md"),
				Path.Combine(Path.GetTempPath(), "changelog")))
			{
				var ChangelogFile = tempFiles.FirstFile;
				File.WriteAllLines(ChangelogFile, new[]
				{
					"## 3.0.97 Beta",
					"- Update French UI Translation",
					"+ When importing, Bloom no longer",
					"  1. makes images transparent when importing.",
					"  4. compresses images transparent when importing.",
					"  9. saves copyright/license back to the original files",
					"    * extra indented list",
					"* Fix insertion of unwanted space before bolded, underlined, and italicized portions of words",
				});
				var debianChangelog = tempFiles.SecondFile;
				File.WriteAllLines(debianChangelog, new[]
				{
					"Bloom (3.0.82 Beta) unstable; urgency=low", "", "  * Older release", "",
					" -- Stephen McConnel <stephen_mcconnel@example.com>  Fri, 12 Jul 2014 14:57:59 -0500", ""
				});
				sut.ChangelogFile = ChangelogFile;
				sut.VersionNumber = "3.0.97 Beta";
				sut.PackageName = "myfavoriteapp";
				sut.MaintainerInfo = "John Hatton <john_hatton@example.com>";
				sut.DebianChangelog = debianChangelog;

				// Execute
				Assert.That(sut.Execute(), Is.True);

				// Verify
				var newContents = File.ReadAllLines(debianChangelog);
				Assert.That(newContents[0], Does.Contain("3.0.97 Beta"));
				Assert.That(newContents[2], Does.StartWith("  *"));
				Assert.That(newContents[3], Does.StartWith("  *"));
				Assert.That(newContents[4], Does.StartWith("    *"));
				Assert.That(newContents[5], Does.StartWith("    *"));
				Assert.That(newContents[6], Does.StartWith("    *"));
				Assert.That(newContents[7], Does.StartWith("    *")); // The 3rd (and further) level indentation isn't currently supported
				Assert.That(newContents[8], Does.StartWith("  *"));
			}
		}
	}
}
