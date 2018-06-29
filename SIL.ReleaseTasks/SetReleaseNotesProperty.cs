// Copyright (c) 2018 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace SIL.ReleaseTasks
{
	[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
	public class SetReleaseNotesProperty : Task
	{
		[Required]
		public string ChangelogFile { get; set; }

		[Output]
		public string Value { get; set; }

		public string VersionRegex { get; set; } = @"#+ \[([^]]+)\]";

		private string[] _markdownLines;
		private int      _currentIndex;
		private Regex    _versionRegex;

		public override bool Execute()
		{
			if (!File.Exists(ChangelogFile))
			{
				Log.LogError($"The given changelog file ({ChangelogFile}) does not exist.");
				return false;
			}

			_versionRegex = new Regex(VersionRegex);

			_markdownLines = File.ReadAllLines(ChangelogFile);

			Value = ConvertLatestChangelog(1, 2);

			if (!string.IsNullOrEmpty(Value))
				return true;

			Log.LogError($"Can't find release in {ChangelogFile}");
			return false;
		}

		private string ConvertLatestChangelog(int level, int skipUntilLevel = -1)
		{
			var bldr = new StringBuilder();
			if (_currentIndex >= _markdownLines.Length)
				return bldr.ToString();

			var levelHeader = new string('#', level) + " ";
			var parentLevelHeader = new string('#', level - 1) + " ";

			for (; _currentIndex < _markdownLines.Length; _currentIndex++)
			{
				var currentLine = _markdownLines[_currentIndex];
				if (string.IsNullOrEmpty(currentLine))
					continue;

				switch (currentLine[0])
				{
					case '#':
						if (currentLine.StartsWith(parentLevelHeader))
						{
							_currentIndex--;
							return bldr.ToString();
						}

						if (currentLine.StartsWith(levelHeader))
						{
							if (level >= skipUntilLevel)
							{
								if (_versionRegex.IsMatch(currentLine))
								{
									var version =
										ExtractPreviousVersionFromChangelog(levelHeader);
									if (!string.IsNullOrEmpty(version))
									{
										bldr.AppendLine($"Changes since version {version}");
										bldr.AppendLine();
									}
								}
								else
								{
									if (bldr.Length > 0)
										bldr.AppendLine();

									var headerText = currentLine.Substring(levelHeader.Length);
									if (!headerText.EndsWith(":"))
										headerText += ":";
									bldr.AppendLine(headerText);
								}
								skipUntilLevel = -1;
							}

							_currentIndex++;
							bldr.Append(ConvertLatestChangelog(level + 1, skipUntilLevel));
							break;
						}

						// other level
						if (level > skipUntilLevel)
							_currentIndex = _markdownLines.Length;
						break;
					default:
						if (level > skipUntilLevel)
							bldr.AppendLine(currentLine);
						break;
				}
			}

			return bldr.ToString();
		}

		private string ExtractPreviousVersionFromChangelog(string currentLevel)
		{
			var nonEmptyLines = 0;
			for (var i = _currentIndex + 1; i < _markdownLines.Length; i++)
			{
				var currentLine = _markdownLines[i];

				if (string.IsNullOrEmpty(currentLine))
					continue;

				nonEmptyLines++;
				if (!currentLine.StartsWith(currentLevel))
					continue;

				if (!_versionRegex.IsMatch(currentLine))
					continue;

				if (nonEmptyLines <= 1)
				{
					// if we have something like
					// ## [Unreleased]
					// ## [1.5]
					// we're currently at version 1.5 but we want to return the previous version.
					// Skip this heading
					_currentIndex = i;
					continue;
				}

				var match = _versionRegex.Match(currentLine);
				return match.Groups[1].Value;
			}

			return string.Empty;
		}
	}
}