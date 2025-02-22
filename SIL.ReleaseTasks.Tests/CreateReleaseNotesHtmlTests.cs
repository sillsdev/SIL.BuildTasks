// Copyright (c) 2018-2025 SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using static System.IO.Path;

namespace SIL.ReleaseTasks.Tests
{
	[TestFixture]
	public class CreateReleaseNotesHtmlTests
	{
		private const string kNotReleaseNotesClassName = "notmarkdown";
		private static readonly string NotReleaseNotesDiv =
			$"<div class='{kNotReleaseNotesClassName}'/>";
		private static readonly string NotReleaseNotesDivXpath =
			$"//div[contains(@class, '{kNotReleaseNotesClassName}')]";
		private static readonly string ReleaseNotesClassAttribute =
			$"class='{CreateReleaseNotesHtml.kReleaseNotesClassName}'";

		private static string GetRandomFileEndingWith(string ending) => Combine(GetTempPath(), GetRandomFileName() + ending);

		[Test]
		public void MissingMarkdownReturnsFalse()
		{
			var mockEngine = new MockEngine();
			var sut = new CreateReleaseNotesHtml
			{
				ChangelogFile = GetRandomFileName(),
				BuildEngine = mockEngine
			};
			Assert.That(sut.Execute(), Is.False);
			Assert.That(mockEngine.LoggedMessages[0], Does.StartWith("The given markdown file (").And.EndsWith(") does not exist."));
		}

		[Test]
		public void SimpleMdResultsInSimpleHtml()
		{
			var sut = new CreateReleaseNotesHtml();
			using (var filesForTest = new TwoTempFilesForTest(GetRandomFileEndingWith(".Test.md"),
				      GetRandomFileEndingWith(".Test.htm")))
			{
				File.WriteAllLines(filesForTest.FirstFile,
					new[]
					{"## 2.3.9", "* with some random content", "* does some things", "## 2.3.7", "* more", "## 2.2.2", "* things"});
				sut.ChangelogFile = filesForTest.FirstFile;
				sut.HtmlFile = filesForTest.SecondFile;
				Assert.That(sut.Execute(), Is.True);
			}
		}

		[Test]
		public void RemovesKeepAChangelogHead()
		{
			var sut = new CreateReleaseNotesHtml();
			using (var filesForTest = new TwoTempFilesForTest(GetRandomFileEndingWith(".Test.md"),
					GetRandomFileEndingWith(".Test.htm")))
			{
				string changelogContent =
@"# Change Log

All notable changes to this project™ will be documented in this file.

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
$"<html><head><meta charset=\"UTF-8\"/></head><body><div {ReleaseNotesClassAttribute}>" +
Environment.NewLine +
@"<h2>[2.3.4] - 2020-12-09</h2>
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
				sut.ChangelogFile = filesForTest.FirstFile;
				sut.HtmlFile = filesForTest.SecondFile;
				// SUT
				Assert.That(sut.Execute(), Is.True);
				string actualHtml = File.ReadAllText(filesForTest.SecondFile);
				Assert.That(actualHtml, Is.EqualTo(expectedHtml));
			}
		}

		[TestCase(null)]
		[TestCase("UTF-8")]
		[TestCase("ISO-8859-1")]
		public void HtmlWithNoReleaseNotesElement_IsCompletelyReplaced(string existingCharset)
		{
			var sut = new CreateReleaseNotesHtml();
			using (var filesForTest = new TwoTempFilesForTest(GetRandomFileEndingWith(".Test.md"),
					GetRandomFileEndingWith(".Test.htm")))
			{
				var changelogFile = filesForTest.FirstFile;
				var htmlFile = filesForTest.SecondFile;
				File.WriteAllLines(changelogFile, new[]
					{
						"## 2.3.9", "* with some random content", "* does some things", "## 2.3.7",
						"* more", "## 2.2.2", "* things"
					});
				var htmlLines = new List<string>(new[]
					{ "<html>", "<body>", NotReleaseNotesDiv, "</body>", "</html>" });
				if (existingCharset != null)
				{
					htmlLines.Insert(1, "<head>");
					htmlLines.Insert(2, $"<meta charset = \"{existingCharset}\"/>");
					htmlLines.Insert(3, "</head>");
				}

				File.WriteAllLines(htmlFile, htmlLines);
				sut.ChangelogFile = changelogFile;
				sut.HtmlFile = htmlFile;
				Assert.That(sut.Execute(), Is.True);
				AssertThatXmlIn.File(htmlFile).HasNoMatchForXpath(NotReleaseNotesDivXpath);
				var expectedCharset = "UTF-8";
				AssertThatXmlIn.File(htmlFile).HasSpecifiedNumberOfMatchesForXpath($"/html/head/meta[@charset='{expectedCharset}']", 1);
			}
		}

		[Test]
		public void HtmlWithReleaseNotesElement_HasOnlyReleaseNoteElementChanged()
		{
			var sut = new CreateReleaseNotesHtml();
			using (var filesForTest = new TwoTempFilesForTest(GetRandomFileEndingWith(".Test.md"),
					GetRandomFileEndingWith(".Test.htm")))
			{
				var changelogFile = filesForTest.FirstFile;
				var htmlFile = filesForTest.SecondFile;
				File.WriteAllLines(changelogFile, new[]
					{
						"## 2.3.9", "* with some random content", "* does some things", "## 2.3.7",
						"* more", "## 2.2.2", "* things"
					});
				File.WriteAllLines(htmlFile, new[]
				{
					"<html>", "<body>", NotReleaseNotesDiv,
					$"<div {ReleaseNotesClassAttribute}/>", "</body>", "</html>"
				});
				sut.ChangelogFile = changelogFile;
				sut.HtmlFile = htmlFile;
				Assert.That(sut.Execute(), Is.True);
				AssertThatXmlIn.File(htmlFile).HasSpecifiedNumberOfMatchesForXpath(
					NotReleaseNotesDivXpath, 1);
				AssertThatXmlIn.File(htmlFile).HasSpecifiedNumberOfMatchesForXpath(
					$"//*[@{ReleaseNotesClassAttribute}]", 1);
				AssertThatXmlIn.File(htmlFile).HasSpecifiedNumberOfMatchesForXpath(
					$"//*[@{ReleaseNotesClassAttribute}]//*[text()[contains(., 'does some things')]]", 1);
			}
		}

		[Test]
		public void HtmlWithReleaseNotesElementWithContents_IsChanged()
		{
			var sut = new CreateReleaseNotesHtml();
			using (var filesForTest = new TwoTempFilesForTest(GetRandomFileEndingWith(".Test.md"),
					GetRandomFileEndingWith(".Test.htm")))
			{
				var changelogFile = filesForTest.FirstFile;
				var htmlFile = filesForTest.SecondFile;
				File.WriteAllLines(changelogFile, new[]
					{
						"## 2.3.9", "* with some random content", "* does some things", "## 2.3.7",
						"* more", "## 2.2.2", "* things"
					});
				File.WriteAllLines(htmlFile, new[]
					{
						"<html>", "<head>", "<meta charset='UTF-8'/>", "</head>", "<body>",
						$"<div {ReleaseNotesClassAttribute}>",
						"<span class='note'/>", "</div>", "</body>", "</html>"
					});
				sut.ChangelogFile = changelogFile;
				sut.HtmlFile = htmlFile;
				Assert.That(sut.Execute(), Is.True);
				AssertThatXmlIn.File(htmlFile).HasSpecifiedNumberOfMatchesForXpath(
					"/html/head/meta[@charset='UTF-8']", 1);
				AssertThatXmlIn.File(htmlFile).HasNoMatchForXpath("//span[@class='note']");
				AssertThatXmlIn.File(htmlFile).HasSpecifiedNumberOfMatchesForXpath(
					$"//*[@{ReleaseNotesClassAttribute}]", 1);
			}
		}
	}
}
