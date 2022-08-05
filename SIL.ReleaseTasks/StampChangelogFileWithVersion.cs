// Copyright (c) 2018 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

[assembly: InternalsVisibleTo("SIL.ReleaseTasks.Tests")]

namespace SIL.ReleaseTasks
{
	/// <inheritdoc />
	/// <summary>
	/// Replaces the first line in a markdown-style Changelog/Release file with the version and date
	/// (Assumes that a temporary line is currently at the top: e.g. ## DEV_VERSION_NUMBER: DEV_RELEASE_DATE
	/// or that the file contains a `## [Unreleased]` line.)
	/// </summary>
	[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
	public class StampChangelogFileWithVersion : Task
	{
		[Required]
		public string ChangelogFile { get; set; }

		[Required]
		public string VersionNumber { get; set; }

		public string DateTimeFormat { get; set; } = "yyyy-MM-dd";

		public override bool Execute()
		{
			List<string> markdownLines = new List<string>(File.ReadAllLines(ChangelogFile));
			bool isKeepAChangelogFormat = markdownLines.Contains("## [Unreleased]");
			if (isKeepAChangelogFormat)
			{
				AddNewVersionToKAC(markdownLines);
			}
			else
			{
				markdownLines[0] = $"## {VersionNumber} {DateTime.Today.ToString(DateTimeFormat)}";
			}
			File.WriteAllLines(ChangelogFile, markdownLines);
			return true;
		}

		/// <summary>
		/// For a Keep a Changelog file, with an `## [Unreleased]` line, insert a new version heading right
		/// under the Unreleased heading, and leave the Unreleased heading.
		/// </summary>
		private void AddNewVersionToKAC(List<string> lines)
		{
			int unreleasedTagLocation = lines.FindIndex((string line) => line == "## [Unreleased]");
			string newHeading = $"## [{VersionNumber}] - {DateTime.Today.ToString(DateTimeFormat)}";
			lines.InsertRange(unreleasedTagLocation + 1, new string[] {"", newHeading});
		}
	}
}
