// Copyright (c) 2018-2025 SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace SIL.BuildTasks.StampAssemblies
{
	[PublicAPI]
	public class StampAssemblies : Task
	{
		private enum VersionFormat
		{
			File,
			Info,
			Semantic
		}

		public class VersionParts
		{
			public string[] Parts = new string[4];
			public string Prerelease { get; set; }

			public override string ToString()
			{
				var str = string.Join(".", Parts);
				if (!string.IsNullOrEmpty(Prerelease))
					str += "-" + Prerelease;
				return str;
			}
		}

		[Required]
		public ITaskItem[] InputAssemblyPaths { get; set; }

		[Required]
		public string Version { get; set; }

		public string FileVersion { get; set; }

		public string PackageVersion { get; set; }

		public override bool Execute()
		{
			foreach (var inputAssemblyPath in InputAssemblyPaths)
			{
				var path = inputAssemblyPath.ItemSpec;

				SafeLog("StampAssemblies: Reading {0}", path); //investigating mysterious TeamCity failure with "Illegal Characters in path"
				SafeLog("StampAssemblies: If you get 'Illegal Characters in path' and have a wild card in the file specification, check for paths that exceed MAX_PATH. We had this happen when we 'shrinkwrap'-ped our node dependencies. MsBuild just silently gives up when this happens.");
				var contents = File.ReadAllText(path);

				SafeLog("StampAssemblies: Stamping {0}", inputAssemblyPath);

				var isCode = Path.GetExtension(path).Equals(".cs", StringComparison.InvariantCultureIgnoreCase);
				// ENHANCE: add property for InformationalVersion
				contents = GetModifiedContents(contents, isCode, Version, FileVersion, PackageVersion);
				File.WriteAllText(path, contents);
			}
			return true;
		}

		private string ExpandTemplate(string regexTemplate, string replaceTemplate, string whichVersion,
			string contents, VersionParts incomingVersion, VersionFormat format = VersionFormat.File)
		{
			try
			{
				var regex = new Regex(string.Format(regexTemplate, whichVersion));
				var result = regex.Match(contents);
				if (result == Match.Empty)
					return contents;
				var versionTemplateInFile = ParseVersionString(result.Groups[1].Value, format);
				var newVersion = MergeTemplates(incomingVersion, versionTemplateInFile);

				SafeLog("StampAssemblies: Merging existing {0} with incoming {1} to produce {2}.",
					versionTemplateInFile.ToString(), incomingVersion.ToString(), newVersion);
				return regex.Replace(contents, string.Format(replaceTemplate, whichVersion, newVersion));
			}
			catch (Exception e)
			{
				Log.LogError("Could not parse the {0} attribute, which should be something like 0.7.*.* or 1.0.0.0",
					whichVersion);
				Log.LogErrorFromException(e);
				throw;
			}
		}

		// ReSharper disable once UnusedMember.Global
		public string GetModifiedContents(string contents, string versionStr, string fileVersionStr)
		{
			return GetModifiedContents(contents, true, versionStr, fileVersionStr, null);
		}

		internal string GetModifiedContents(string contents, bool isCode, string versionStr, string fileVersionStr,
			string packageVersionStr)
		{
			// ENHANCE: add property for InformationalVersion
			var version = ParseVersionString(versionStr);
			var fileVersion = fileVersionStr != null ? ParseVersionString(fileVersionStr) : version;
			var infoVersion = ParseVersionString(versionStr, VersionFormat.Info);
			var packageVersion = packageVersionStr != null
				? ParseVersionString(packageVersionStr, VersionFormat.Semantic)
				: version;

			return isCode ? ModifyCodeAttributes(contents, version, fileVersion, infoVersion)
				: ModifyMsBuildProps(contents, version, fileVersion, infoVersion, packageVersion);
		}

		private string ModifyCodeAttributes(string contents, VersionParts version, VersionParts fileVersion,
			VersionParts infoVersion)
		{
			const string regexTemplate = @"\[assembly\: {0}\(""(.+)""";
			const string replaceTemplate = @"[assembly: {0}(""{1}""";

			contents = ExpandTemplate(regexTemplate, replaceTemplate, "AssemblyVersion", contents, version);
			contents = ExpandTemplate(regexTemplate, replaceTemplate, "AssemblyFileVersion", contents, fileVersion);
			contents = ExpandTemplate(regexTemplate, replaceTemplate, "AssemblyInformationalVersion", contents,
				infoVersion, VersionFormat.Info);
			return contents;
		}

		private string ModifyMsBuildProps(string contents, VersionParts version, VersionParts fileVersion,
			VersionParts infoVersion, VersionParts packageVersion)
		{
			const string regexTemplate = "<{0}>(.+)</{0}>";
			const string replaceTemplate = "<{0}>{1}</{0}>";

			contents = ExpandTemplate(regexTemplate, replaceTemplate, "AssemblyVersion", contents, version);
			contents = ExpandTemplate(regexTemplate, replaceTemplate, "FileVersion", contents, fileVersion);
			contents = ExpandTemplate(regexTemplate, replaceTemplate, "InformationalVersion", contents, infoVersion,
				VersionFormat.Info);
			contents = ExpandTemplate(regexTemplate, replaceTemplate, "Version", contents, packageVersion,
				VersionFormat.Semantic);

			return contents;
		}

		private void SafeLog(string msg, params object[] args)
		{
			try
			{
				Debug.WriteLine(msg, args);
				Log.LogMessage(msg,args);
			}
			catch (Exception)
			{
				//swallow... logging fails in the unit test environment, where the log isn't really set up
			}
		}

		private static string MergeTemplates(VersionParts incoming, VersionParts existing)
		{
			var result = new VersionParts
			{
				Parts = (string[]) existing.Parts.Clone(),
				Prerelease = incoming.Prerelease ?? existing.Prerelease
			};
			for (var i = 0; i < result.Parts.Length; i++)
			{
				if (incoming.Parts[i] != "*")
					result.Parts[i] = incoming.Parts[i];
				else if (existing.Parts[i] == "*")
					result.Parts[i] = "0";
			}
			return result.ToString();
		}

		private VersionParts GetExistingAssemblyVersion(string whichAttribute, string contents)
		{
			try
			{
				var result = Regex.Match(contents, $@"\[assembly\: {whichAttribute}\(""(.+)""");
				return result == Match.Empty ? null : ParseVersionString(result.Groups[1].Value);
			}
			catch (Exception e)
			{
				Log.LogError("Could not parse the {0} attribute, which should be something like 0.7.*.* or 1.0.0.0",
					whichAttribute);
				Log.LogErrorFromException(e);
				throw;
			}
		}

		public VersionParts GetExistingAssemblyVersion(string contents)
		{
			return GetExistingAssemblyVersion("AssemblyVersion", contents);
		}

		// ReSharper disable once UnusedMember.Global
		public VersionParts GetExistingAssemblyFileVersion(string contents)
		{
			return GetExistingAssemblyVersion("AssemblyFileVersion", contents);
		}

		private static VersionParts ParseVersionString(string contents, bool allowHashAsRevision = false)
		{
			return ParseVersionString(contents, allowHashAsRevision ? VersionFormat.Info : VersionFormat.File);
		}

		private static VersionParts ParseVersionString(string contents, VersionFormat format)
		{
			VersionParts v;
			if (format == VersionFormat.Semantic)
			{
				var result = Regex.Match(contents, @"([\d\*]+)\.([\d\*]+)\.([\d\*]+)(?:\-(.*))?");

				v = new VersionParts
				{
					Parts = new[]
					{
						result.Groups[1].Value,
						result.Groups[2].Value,
						result.Groups[3].Value
					},
					Prerelease = result.Groups[4].Value
				};
			}
			else
			{
				var result = Regex.Match(contents, @"(.+)\.(.+)\.(.+)\.(.+)");
				if (!result.Success)
				{
					//handle 1.0.*  (I'm not good enough with regex to
					//overcome greediness and get a single pattern to work for both situations).
					result = Regex.Match(contents, @"(.+)\.(.+)\.(\*)");
				}
				if (!result.Success)
				{
					//handle 0.0.12
					result = Regex.Match(contents, @"(.+)\.(.+)\.(.+)");
				}

				v = new VersionParts
				{
					Parts = new[]
					{
						result.Groups[1].Value,
						result.Groups[2].Value,
						result.Groups[3].Value,
						result.Groups[4].Value
					}
				};

				if (format == VersionFormat.File && v.Parts.Length == 4
					&& v.Parts[3].IndexOfAny(new[] { 'a', 'b', 'c', 'd', 'e', 'f' }) != -1)
				{
					// zero out hash code which we can't have in numeric version numbers
					v.Parts[3] = "0";
				}
			}

			for (var i = 0; i < v.Parts.Length; i++)
			{
				if (string.IsNullOrEmpty(v.Parts[i]))
					v.Parts[i] = "*";
			}

			return v;
		}
	}
}