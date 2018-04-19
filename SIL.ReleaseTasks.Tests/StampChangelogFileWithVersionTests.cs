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
		public void StampMarkdownWorks()
		{
			var testMarkdown = new StampChangelogFileWithVersion();
			using(var tempFiles = new TwoTempFilesForTest(Path.Combine(Path.GetTempPath(), "Test.md"), null))
			{
				File.WriteAllLines(tempFiles.FirstFile,
					new[] {"## DEV_VERSION_NUMBER: DEV_RELEASE_DATE", "*with some random content", "*does some things"});
				testMarkdown.ChangelogFile = tempFiles.FirstFile;
				testMarkdown.VersionNumber = "2.3.10";
				var day = $"{DateTime.Now:dd/MMM/yyyy}";
				Assert.That(testMarkdown.Execute(), Is.True);
				var newContents = File.ReadAllLines(tempFiles.FirstFile);
				Assert.That(newContents.Length == 3);
				Assert.That(newContents[0], Is.EqualTo("## 2.3.10 " + day));
			}
		}
	}
}
