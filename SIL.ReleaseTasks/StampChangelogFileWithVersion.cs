// Copyright (c) 2018 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using System.Xml.XPath;
using MarkdownDeep;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

[assembly: InternalsVisibleTo("SIL.ReleaseTasks.Tests")]

namespace SIL.ReleaseTasks
{
	/// <inheritdoc />
	/// <summary>
	/// Replaces the first line in a markdown-style Changelog/Release file with the version and date
	/// (Assumes that a temporary line is currently at the top: e.g. ## DEV_VERSION_NUMBER: DEV_RELEASE_DATE )
	/// </summary>
	[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
	public class StampChangelogFileWithVersion : Task
	{
		[Required]
		public string ChangelogFile { get; set; }

		[Required]
		public string VersionNumber { get; set; }

		public string DateTimeFormat { get; set; }

		public override bool Execute()
		{
			if (string.IsNullOrEmpty(DateTimeFormat))
				DateTimeFormat = "yyyy-MM-dd";
			var markdownLines = File.ReadAllLines(ChangelogFile);
			markdownLines[0] = $"## {VersionNumber} {DateTime.Today.ToString(DateTimeFormat)}";
			File.WriteAllLines(ChangelogFile, markdownLines);
			return true;
		}
	}
}
