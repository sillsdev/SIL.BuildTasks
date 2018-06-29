// Copyright (c) 2018 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace SIL.ReleaseTasks
{
	[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
	public class SetReleaseNotesProperty: Task
	{
		[Required]
		public string ChangelogFile { get; set; }

		[Output]
		public string Value { get; set; }

		private string[] _markdownLines;
		private int _currentIndex;

		public override bool Execute()
		{
			if (!File.Exists(ChangelogFile))
			{
				Log.LogError($"The given changelog file ({ChangelogFile}) does not exist.");
				return false;
			}

			_markdownLines = File.ReadAllLines(ChangelogFile);

			Value = ConvertLatestChangelog(1, 3);

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
								if (bldr.Length > 0)
									bldr.AppendLine();
								var headerText = currentLine.Substring(levelHeader.Length);
								if (!headerText.EndsWith(":"))
									headerText += ":";
								bldr.AppendLine(headerText);
								skipUntilLevel = -1;
							}

							_currentIndex++;
							bldr.Append(ConvertLatestChangelog(level + 1, skipUntilLevel));
							break;
						}

						// other level
						if (level >= skipUntilLevel)
							_currentIndex = _markdownLines.Length;
						break;
					default:
						if (level >= skipUntilLevel)
							bldr.AppendLine(currentLine);
						break;
				}
			}

			return bldr.ToString();
		}

	}
}