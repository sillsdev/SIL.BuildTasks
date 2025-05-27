// Copyright (c) 2018-2025 SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.IO;
using System.Xml.Linq;
using System.Xml.XPath;
using JetBrains.Annotations;
using Markdig;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace SIL.ReleaseTasks
{
	/// <inheritdoc />
	/// <summary>
	/// Given a markdown-style changelog file, this class will generate a release notes HTML file.
	/// If the HTML file already exists, the task will look for a section with
	/// `class="<see cref="kReleaseNotesClassName"/>>"` and replace it with the current release
	/// notes.
	/// The developer-oriented and [Unreleased] beginning will be removed from a Keep a Changelog
	/// style changelog.
	/// </summary>
	[PublicAPI]
	public class CreateReleaseNotesHtml : Task
	{
		public const string kReleaseNotesClassName = "releasenotes";

		[Required]
		public string HtmlFile { get; set; }

		[Required]
		public string ChangelogFile { get; set; }

		public override bool Execute()
		{
			if(!File.Exists(ChangelogFile))
			{
				Log.LogError($"The given markdown file ({ChangelogFile}) does not exist.");
				return false;
			}

			try
			{
				string inputMarkdown = File.ReadAllText(ChangelogFile);
				RemoveKeepAChangelogHeadIfPresent(ref inputMarkdown);
				// MarkDig appears to use \n for newlines. Rather than mix those with platform
				// line-endings, just convert them to platform line-endings if needed.
				var markdownHtml = Markdown.ToHtml(inputMarkdown).Replace("\n", Environment.NewLine);
				XElement releaseNotesElement = null;
				XDocument htmlDoc = null;
				if (File.Exists(HtmlFile))
				{
					htmlDoc = XDocument.Load(HtmlFile);
					var releaseNotesElementXpath = $"//*[@class='{kReleaseNotesClassName}']";
					releaseNotesElement = htmlDoc.XPathSelectElement(releaseNotesElementXpath);
				}
				if (releaseNotesElement == null)
					WriteBasicHtmlFromMarkdown(markdownHtml);
				else
				{
					releaseNotesElement.RemoveNodes();
					var mdDocument = XDocument.Parse($"<div>{markdownHtml}</div>");
					// ReSharper disable once PossibleNullReferenceException - Will either throw or work
					releaseNotesElement.Add(mdDocument.Root.Elements());
					htmlDoc.Save(HtmlFile);
				}
				return true;
			}
			catch(Exception e)
			{
				Log.LogErrorFromException(e, true);
				return false;
			}
		}

		private void WriteBasicHtmlFromMarkdown(string markdownHtml)
		{
			File.WriteAllText(HtmlFile, "<html><head><meta charset=\"UTF-8\"/></head><body>" +
				$"<div class='{kReleaseNotesClassName}'>" +
				$"{Environment.NewLine}{markdownHtml}</div></body></html>");
		}

		/// <summary>
		/// Remove a bunch of lines from the top of a Keep a Changelog file, so users can just see the
		/// release notes. Returns true if input is Keep A Changelog style, or false.
		/// </summary>
		public static bool RemoveKeepAChangelogHeadIfPresent(ref string md)
		{
			if (!md.Contains("[Unreleased]"))
				return false;
			string unreleasedHeader = $"## [Unreleased]{Environment.NewLine}{Environment.NewLine}";
			int unreleasedHeaderLocation = md.IndexOf(unreleasedHeader);
			md = md.Substring(unreleasedHeaderLocation + unreleasedHeader.Length);
			return true;
		}
	}
}
