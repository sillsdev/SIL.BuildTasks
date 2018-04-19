// Copyright (c) 2018 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Xml.Linq;
using System.Xml.XPath;
using MarkdownDeep;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace SIL.ReleaseTasks
{
	/// <inheritdoc />
	/// <summary>
	/// Given a markdown-style changelog file, this class will generate a release notes HTML file.
	/// If the HTML file already exists, the task will look for a section with `class="releasenotes"`
	/// and replace it with the current release notes.
	/// </summary>
	[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
	public class CreateReleaseNotesHtml : Task
	{
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

			var markDownTransformer = new Markdown();
			try
			{
				var markdownHtml = markDownTransformer.Transform(File.ReadAllText(ChangelogFile));
				if(File.Exists(HtmlFile))
				{
					var htmlDoc = XDocument.Load(HtmlFile);
					var releaseNotesElement = htmlDoc.XPathSelectElement("//*[@class='releasenotes']");
					if (releaseNotesElement == null)
						return true;

					releaseNotesElement.RemoveNodes();
					var mdDocument = XDocument.Parse($"<div>{markdownHtml}</div>");
					// ReSharper disable once PossibleNullReferenceException - Will either throw or work
					releaseNotesElement.Add(mdDocument.Root.Elements());
					htmlDoc.Save(HtmlFile);
				}
				else
				{
					WriteBasicHtmlFromMarkdown(markdownHtml);
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
			File.WriteAllText(HtmlFile, $"<html><div class='releasenotes'>{markdownHtml}</div></html>");
		}
	}
}
