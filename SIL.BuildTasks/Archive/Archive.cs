// Copyright (c) 2018-2025 SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using JetBrains.Annotations;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace SIL.BuildTasks.Archive
{
	[PublicAPI]
	public class Archive : Task
	{
		[Required]
		public ITaskItem[] InputFilePaths { get; set; }

		[Required]
		public string Command { get; set; }

		[Required]
		public string OutputFileName { get; set; }

		public string BasePath { get; set; }

		public string WorkingDir { get; set; }

		public override bool Execute()
		{
			var filePathString = FlattenFilePaths(InputFilePaths, ' ', false);

			var startInfo = new ProcessStartInfo(ExecutableName()) {
				Arguments = Arguments() + " " + filePathString,
				WorkingDirectory = string.IsNullOrEmpty(WorkingDir) ? BasePath : WorkingDir
			};
			var process = Process.Start(startInfo);
			process?.WaitForExit();
			return true;
		}

		internal string ExecutableName()
		{
			return Command == "Tar" ? "tar" : string.Empty;
		}

		internal string Arguments()
		{
			if (Command == "Tar")
				return "-cvzf " + OutputFileName;

			return string.Empty;
		}

		internal string TrimBaseFromFilePath(string filePath)
		{
			var result = filePath;
			if (!result.StartsWith(BasePath))
				return result;

			result = filePath.Substring(BasePath.Length);
			if (result.StartsWith("/") || result.StartsWith("\\"))
				result = result.TrimStart('/', '\\');
			return result;
		}

		internal string FlattenFilePaths(IEnumerable<ITaskItem> items, char delimeter, bool withQuotes)
		{
			var sb = new StringBuilder();
			var haveStarted = false;
			foreach (var item in items)
			{
				if (haveStarted)
				{
					sb.Append(delimeter);
				}
				var filePath = TrimBaseFromFilePath(item.ItemSpec);
				if (filePath.Contains(" ") || withQuotes)
				{
					sb.Append('"');
					sb.Append(filePath);
					sb.Append('"');
				}
				else
				{
					sb.Append(filePath);
				}
				haveStarted = true;
			}
			return sb.ToString();
		}
	}
}