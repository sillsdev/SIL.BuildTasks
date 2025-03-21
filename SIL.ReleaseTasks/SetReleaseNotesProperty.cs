// Copyright (c) 2018 SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
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

		public string VersionRegex { get; set; }

		public string AppendToReleaseNotesProperty { get; set; }

		public bool FilterEntries { get; set; }

		public string PackageId { get; set; }

		private string[] _markdownLines;
		private int      _currentIndex;
		private Regex    _versionRegex;
		private Regex _urlRegex;
		private Regex _filterRegex;

		public override bool Execute()
		{
			string urlRegex = @"\[[^]]+\]: (http|https|ftp|)://.+";
			_urlRegex = new Regex(urlRegex);

			string filterRegex = @"\- \[([^]]+)\]";
			_filterRegex = new Regex(filterRegex);
			

			if (!File.Exists(ChangelogFile))
			{
				Log.LogError($"The given changelog file ({ChangelogFile}) does not exist.");
				return false;
			}

			string versionRegexString;
			if (string.IsNullOrEmpty(VersionRegex))
			{
				versionRegexString = @"#+ \[([^]]+)\]";
				
			}
			else
			{
				// we can't use backslash as escape character in the property
				// (https://github.com/Microsoft/msbuild/issues/3468), so we use @ instead and
				// convert the @ to \ here.
				versionRegexString = Regex.Replace(VersionRegex, "(^|[^@])(@)([^@])",
					match => $@"{match.Groups[1].Value}\{match.Groups[3].Value}").Replace("@@", "@");
			}
			_versionRegex = new Regex(versionRegexString);

			_markdownLines = File.ReadAllLines(ChangelogFile);

			var bldr = new StringBuilder();
			bldr.Append(ConvertLatestChangelog(1, 2));

			if (!string.IsNullOrEmpty(AppendToReleaseNotesProperty))
				bldr.AppendLine(AppendToReleaseNotesProperty);

			Value = bldr.ToString();

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
									var headerText = currentLine.Substring(levelHeader.Length);
									if (!headerText.EndsWith(":"))
										headerText += ":";
									bool printHeader = SkipHeader(_currentIndex);
									if (printHeader)
									{
										if (bldr.Length > 0)
											bldr.AppendLine();
										bldr.AppendLine(headerText);
									}

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
						if (FilterEntries)
						{
							int m = _currentIndex;
								if (_filterRegex.IsMatch(currentLine))
							{
								if (currentLine.StartsWith($"- [{PackageId}]"))
								{
									string newLine = currentLine.Replace($" [{PackageId}]", "");
									bldr.AppendLine(newLine);
									m++;
									while (m < _markdownLines.Length && !_markdownLines[m].StartsWith("- [") && !string.IsNullOrEmpty(_markdownLines[m]))
									{
										bldr.AppendLine(_markdownLines[m]);
										_currentIndex = m;
										m++;
									}
									
								}
								else
								{
									while (m < _markdownLines.Length && !_markdownLines[m].StartsWith("- [") && string.IsNullOrEmpty(_markdownLines[m]))
									{
										_currentIndex = m;
										m++;
									}
								}
							}
							else if (currentLine.StartsWith("-"))
							{
								while (m < _markdownLines.Length && !_markdownLines[m].StartsWith("- [") && !string.IsNullOrEmpty(_markdownLines[m]))
								{
									if (!currentLine.StartsWith("-->"))
										bldr.AppendLine(_markdownLines[m]);
									_currentIndex = m;
									m++;
								}
							}
						}
						else if (_urlRegex.IsMatch(currentLine))
							_currentIndex = _markdownLines.Length;
						else if (level > skipUntilLevel)
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

		private bool SkipHeader(int index)
		{
			//exclude unnecessary headers from changelog file
			for (int i = index + 1; i < _markdownLines.Length; i++)
			{
				var line = _markdownLines[i];
				if (string.IsNullOrEmpty(line) || line.StartsWith("  "))
					continue;
				if (line.StartsWith("-") && FilterEntries)
				{
					if (_filterRegex.IsMatch(line))
					{
						if (line.StartsWith($"- [{PackageId}]"))
						{
							return true;
						}
					}
					else
					{
						return true;
					}
				}
				else if (!_versionRegex.IsMatch(line))
					break;
			}
			return !FilterEntries;
		}
	}
}