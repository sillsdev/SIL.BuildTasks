// Copyright (c) 2018-2025 SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using JetBrains.Annotations;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace SIL.ReleaseTasks
{
	/// <inheritdoc />
	/// <summary>
	/// Given a Changelog file, this task will add an entry to the debian changelog.
	/// </summary>
	[PublicAPI]
	public class CreateChangelogEntry: Task
	{
		[Required]
		public string ChangelogFile { get; set; }

		[Required]
		public string VersionNumber { get; set; }

		[Required]
		public string PackageName { get; set; }

		[Required]
		public string DebianChangelog { get; set; }

		public string Distribution { get; set; } = "UNRELEASED";
		public string Urgency { get; set; } = "low";
		public DateTime EntryDate { get; set; } = DateTime.Now;

		/// <summary>
		/// Name and e-mail string
		/// </summary>
		public string MaintainerInfo { get; set; }

		[PublicAPI]
		public override bool Execute()
		{
			var oldChangeLog = Path.ChangeExtension(DebianChangelog, ".old");
			File.Delete(oldChangeLog);
			File.Move(DebianChangelog, oldChangeLog);
			WriteMostRecentMarkdownEntryToChangelog();
			File.AppendAllLines(DebianChangelog, File.ReadAllLines(oldChangeLog));
			return true;
		}

		private void WriteMostRecentMarkdownEntryToChangelog()
		{
			if(string.IsNullOrEmpty(MaintainerInfo))
			{
				MaintainerInfo = "Anonymous <anonymous@example.com>";
			}
			string[] markdownLines = File.ReadAllLines(ChangelogFile);
			List<string> newChangelogEntry = GenerateNewDebianChangelogEntry(markdownLines);
			File.AppendAllLines(DebianChangelog, newChangelogEntry);
		}

		public List<string> GenerateNewDebianChangelogEntry(string[] markdownLines)
		{
			string mdText = string.Join(Environment.NewLine, markdownLines);
			bool isKeepAChangelogFormat = CreateReleaseNotesHtml.RemoveKeepAChangelogHeadIfPresent(ref mdText);
			markdownLines = mdText.Split(new [] { Environment.NewLine }, StringSplitOptions.None);

			if (isKeepAChangelogFormat)
			{
				markdownLines = ConvertKACSectionsToBullets(markdownLines);
			}

			var newEntryLines = new List<string>
			{
				// Write out the first line as the changelog version line
				$"{PackageName} ({VersionNumber}) {Distribution}; urgency={Urgency}",
				string.Empty
			};
			// Skip a beginning blank line after the version header, if present.
			// (Not to be confused with a Keep a Changelog heading, that would have been removed earlier.)
			int contentStartingLineAfterVersionHeader = 1;
			if (string.IsNullOrWhiteSpace(markdownLines[1]))
			{
				contentStartingLineAfterVersionHeader = 2;
			}
			for (var i = contentStartingLineAfterVersionHeader; i < markdownLines.Length; ++i)
			{
				if (markdownLines[i].StartsWith("##"))
				{
					break;
				}
				ConvertMarkdownLineToChangelogLine(markdownLines[i], newEntryLines);
			}
			if (newEntryLines[newEntryLines.Count - 1] != string.Empty)
			{
				// End body with a blank line, if not already.
				newEntryLines.Add(string.Empty);
			}
			// The debian changelog needs RFC 2822 format (Thu, 15 Oct 2015 08:25:16 -0500), which is not quite what .NET can provide
			var debianDate = CreateChangelogEntry.DebianDate(EntryDate);
			newEntryLines.Add($" -- {MaintainerInfo}  {debianDate}");
			newEntryLines.Add(string.Empty);
			return newEntryLines;
		}

		/// <summary>
		/// Format `when` suitable for debian/changelog files.
		/// </summary>
		public static string DebianDate(DateTime when)
		{
			return $"{when:ddd, dd MMM yyyy HH'|'mm'|'ss zzz}".Replace(":", "").Replace('|', ':');
		}

		private static void ConvertMarkdownLineToChangelogLine(string markdownLine, List<string> newEntryLines)
		{
			if (string.IsNullOrWhiteSpace(markdownLine))
			{
				newEntryLines.Add(string.Empty);
				return;
			}
			// ReSharper disable once SwitchStatementMissingSomeCases
			switch(markdownLine[0])
			{
				case '*':
				case '-':
				case '+':
				case '1':
				case '2':
				case '3':
				case '4':
				case '5':
				case '6':
				case '7':
				case '8':
				case '9':
				case '0':
					// Treat all unordered and ordered list items the same in the changelog
					newEntryLines.Add($"  *{markdownLine.Substring(1)}");
					break;
				case ' ':
					// Handle lists within lists, only second level items are handled, any further indentation is
					// currently ignored.
					// TrimStart('.') is used to remove the period from "1.".
					newEntryLines.Add($"    *{markdownLine.Trim().Substring(1).TrimStart('.')}");
					break;
			}
		}

		/// <summary>
		/// Keep a Changelog style changelogs have sections like "Fixed" and "Added". Change these to bullets.
		/// </summary>
		private string[] ConvertKACSectionsToBullets(string[] mdLines)
		{
			return (new List<string>(mdLines))
				// Indent everything, except headers and empty lines, so the section bullets are at the least level of indentation.
				.Select((string line) => {
					if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
					{
						return line;
					}
					return line.Insert(0, "  ");
				})
				// Change ### section headers to first-level bullets.
				.Select((string line) => line.Replace("### ", "- "))
				.ToArray();
		}
	}
}
