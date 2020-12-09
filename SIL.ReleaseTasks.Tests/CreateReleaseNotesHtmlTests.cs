// Copyright (c) 2018 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System.IO;
using NUnit.Framework;

namespace SIL.ReleaseTasks.Tests
{
	[TestFixture]
	public class CreateReleaseNotesHtmlTests
	{
		[Test]
		public void MissingMarkdownReturnsFalse()
		{
			var mockEngine = new MockEngine();
			var testMarkdown = new CreateReleaseNotesHtml();
			testMarkdown.ChangelogFile = Path.GetRandomFileName();
			testMarkdown.BuildEngine = mockEngine;
			Assert.That(testMarkdown.Execute(), Is.False);
			Assert.That(mockEngine.LoggedMessages[0], Does.StartWith("The given markdown file (").And.EndsWith(") does not exist."));
		}

		[Test]
		public void SimpleMdResultsInSimpleHtml()
		{
			var testMarkdown = new CreateReleaseNotesHtml();
			using(
				var filesForTest = new TwoTempFilesForTest(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()+".Test.md"),
					Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()+".Test.htm")))
			{
				File.WriteAllLines(filesForTest.FirstFile,
					new[]
					{"## 2.3.9", "* with some random content", "* does some things", "## 2.3.7", "* more", "## 2.2.2", "* things"});
				testMarkdown.ChangelogFile = filesForTest.FirstFile;
				testMarkdown.HtmlFile = filesForTest.SecondFile;
				Assert.That(testMarkdown.Execute(), Is.True);
			}
		}

		[Test]
		public void RemovesKACHead()
		{
			var testMarkdown = new CreateReleaseNotesHtml();
			using(
				var filesForTest = new TwoTempFilesForTest(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()+".Test.md"),
					Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()+".Test.htm")))
			{
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

## [2.3.4] - 2020-12-09

### Changed
- This to that.

## [1.2.3] - 2020-12-08

### Added
- New features.

### Fixed
- All bugs.";

				string expectedHtml =
@"<html><head></head><body><div class='releasenotes'>
<h2>[2.3.4] - 2020-12-09</h2>
<h3>Changed</h3>
<ul>
<li>This to that.</li>
</ul>
<h2>[1.2.3] - 2020-12-08</h2>
<h3>Added</h3>
<ul>
<li>New features.</li>
</ul>
<h3>Fixed</h3>
<ul>
<li>All bugs.</li>
</ul>
</div></body></html>";

				File.WriteAllText(filesForTest.FirstFile, changelogContent);
				testMarkdown.ChangelogFile = filesForTest.FirstFile;
				testMarkdown.HtmlFile = filesForTest.SecondFile;
				// SUT
				Assert.That(testMarkdown.Execute(), Is.True);
				string actualHtml = File.ReadAllText(filesForTest.SecondFile);
				Assert.That(actualHtml, Is.EqualTo(expectedHtml));
			}
		}

		[Test]
		public void HtmlWithNoReleaseNotesElementIsCompletelyReplaced()
		{
			var testMarkdown = new CreateReleaseNotesHtml();
			using(
				var filesForTest = new TwoTempFilesForTest(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()+".Test.md"),
					Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()+".Test.htm")))
			{
				var ChangelogFile = filesForTest.FirstFile;
				var htmlFile = filesForTest.SecondFile;
				File.WriteAllLines(ChangelogFile,
					new[]
					{"## 2.3.9", "* with some random content", "* does some things", "## 2.3.7", "* more", "## 2.2.2", "* things"});
				File.WriteAllLines(htmlFile,
					new[] {"<html>", "<body>", "<div class='notmarkdown'/>", "</body>", "</html>"});
				testMarkdown.ChangelogFile = ChangelogFile;
				testMarkdown.HtmlFile = htmlFile;
				Assert.That(testMarkdown.Execute(), Is.True);
				AssertThatXmlIn.File(htmlFile).HasNoMatchForXpath("//div[@notmarkdown]");
			}
		}

		[Test]
		public void HtmlWithReleaseNotesElementHasOnlyReleaseNoteElementChanged()
		{
			var testMarkdown = new CreateReleaseNotesHtml();
			using(
				var filesForTest = new TwoTempFilesForTest(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()+".Test.md"),
					Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()+".Test.htm")))
			{
				var ChangelogFile = filesForTest.FirstFile;
				var htmlFile = filesForTest.SecondFile;
				File.WriteAllLines(ChangelogFile,
					new[]
					{"## 2.3.9", "* with some random content", "* does some things", "## 2.3.7", "* more", "## 2.2.2", "* things"});
				File.WriteAllLines(htmlFile,
					new[] {"<html>", "<body>", "<div class='notmarkdown'/>", "<div class='releasenotes'/>", "</body>", "</html>"});
				testMarkdown.ChangelogFile = ChangelogFile;
				testMarkdown.HtmlFile = htmlFile;
				Assert.That(testMarkdown.Execute(), Is.True);
				AssertThatXmlIn.File(htmlFile).HasSpecifiedNumberOfMatchesForXpath("//*[@class='notmarkdown']", 1);
				AssertThatXmlIn.File(htmlFile).HasSpecifiedNumberOfMatchesForXpath("//*[@class='releasenotes']", 1);
				AssertThatXmlIn.File(htmlFile).HasSpecifiedNumberOfMatchesForXpath("//*[@class='releasenotes']//*[text()[contains(., 'does some things')]]", 1);
			}
		}

		[Test]
		public void HtmlWithReleaseNotesElementWithContentsIsChanged()
		{
			var testMarkdown = new CreateReleaseNotesHtml();
			using(
				var filesForTest = new TwoTempFilesForTest(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()+".Test.md"),
					Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()+".Test.htm")))
			{
				var ChangelogFile = filesForTest.FirstFile;
				var htmlFile = filesForTest.SecondFile;
				File.WriteAllLines(ChangelogFile,
					new[]
					{"## 2.3.9", "* with some random content", "* does some things", "## 2.3.7", "* more", "## 2.2.2", "* things"});
				File.WriteAllLines(htmlFile,
					new[]
					{
						"<html>", "<body>", "<div class='releasenotes'>", "<span class='note'/>", "</div>",
						"</body>", "</html>"
					});
				testMarkdown.ChangelogFile = ChangelogFile;
				testMarkdown.HtmlFile = htmlFile;
				Assert.That(testMarkdown.Execute(), Is.True);
				AssertThatXmlIn.File(htmlFile).HasNoMatchForXpath("//span[@class='note']");
				AssertThatXmlIn.File(htmlFile).HasSpecifiedNumberOfMatchesForXpath("//*[@class='releasenotes']", 1);
			}
		}

	}
}
