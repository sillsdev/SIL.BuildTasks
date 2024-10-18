// Copyright (c) 2018 SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace SIL.BuildTasks.UpdateBuildTypeFile
{
	[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
	[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
	public class UpdateBuildTypeFile : Task
	{
		[Required]
		public ITaskItem[] BuildTypePaths { get; set; }

		public string BuildType { get; set; }

		public override bool Execute()
		{
			if (string.IsNullOrEmpty(BuildType))
				return true;

			var buildTypeFile = BuildTypePaths.Single();
			var path = buildTypeFile.ItemSpec;
			var contents = File.ReadAllText(path);

			SafeLog("UpdateBuildTypeFile: Updating {0}", buildTypeFile);

			File.WriteAllText(path, GetUpdatedFileContents(contents, BuildType));
			return true;
		}

		public static string GetUpdatedFileContents(string contents, string newType)
		{
			var bldr = new StringBuilder(@"VersionType\.(");
			var first = true;
			foreach (var versionType in GetVersionTypes(contents))
			{
				if (!first)
					bldr.Append("|");
				bldr.Append("(");
				bldr.Append(versionType);
				bldr.Append(")");
				first = false;
			}
			bldr.Append(")");
			var regex = new Regex(bldr.ToString(), RegexOptions.Compiled);
			return regex.Replace(contents, "VersionType." + newType);
		}

		public static List<string> GetVersionTypes(string contents)
		{
			var i = contents.IndexOf("public enum VersionType", StringComparison.Ordinal);
			if (i < 0)
				throw new Exception("File does not contain a public definition for an enum named VersionType!");
			var iStart = contents.IndexOf("{", i, StringComparison.Ordinal) + 1;
			var iEnd = contents.IndexOf("}", iStart, StringComparison.Ordinal);
			var versionTypeEnumBody = contents.Substring(iStart, iEnd - iStart);
			var regex = new Regex(@"(?:((?!\d)\w+(?:\.(?!\d)\w+)*)\.)?((?!\d)\w+)", RegexOptions.Compiled);
			return (from object type in regex.Matches(versionTypeEnumBody) select type.ToString()).ToList();
		}

		private void SafeLog(string msg, params object[] args)
		{
			try
			{
				Debug.WriteLine(msg, args);
				Log.LogMessage(msg, args);
			}
			catch (Exception)
			{
				//swallow... logging fails in the unit test environment, where the log isn't really set up
			}
		}
	}
}