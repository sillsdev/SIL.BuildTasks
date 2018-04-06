// Copyright (c) 2018 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
/*
 * A custom task that walks a directory tree and creates a WiX fragment containing
 * components to recreate it in an installer.
 *
 * From John Hall <john.hall@xjtag.com>, originally named "PackageTree" and posted on the wix-users mailing list
 *
 * John Hatton modified a bit to make it more general and started cleaning it up.
 *
 * Places a "".guidsForInstaller.xml" in each directory.  THIS SHOULD BE CHECKED INTO VERSION CONTROL.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;


namespace SIL.BuildTasks.MakeWixForDirTree
{
	[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
	[SuppressMessage("ReSharper", "UnusedMember.Global")]
	[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
	public class MakeWixForDirTree : Task, ILogger
	{
		private const string FileNameOfGuidDatabase = ".guidsForInstaller.xml";

		#region Private data

		private Regex _fileMatchPattern = new Regex(@".*");
		private Regex _ignoreFilePattern = new Regex(@"IGNOREME");

		//todo: this should just be a list
		private readonly Dictionary<string, string> _exclude = new Dictionary<string, string>();

		private readonly List<string> _components = new List<string>();
		private readonly Dictionary<string, int> _suffixes = new Dictionary<string, int>();
		private readonly DateTime _refDate = DateTime.MinValue;
		private bool _filesChanged;

		private const string Xmlns = "http://schemas.microsoft.com/wix/2006/wi";

		#endregion


		#region Public members


		[Required]
		public string RootDirectory { get; set; }

		/// <summary>
		/// Subfolders and files to exclude. Kinda wonky. Using Ignore makes more sense.
		/// </summary>
		public string[] Exclude { get; set; }

		/// <summary>
		/// Allow normal non-administrators to write and delete the files
		/// </summary>
		public bool GiveAllPermissions { get; set; }

		/*
		 * Regex pattern to match files. Defaults to .*
		 */
		public string MatchRegExPattern
		{
			get { return _fileMatchPattern.ToString(); }
			set { _fileMatchPattern = new Regex(value, RegexOptions.IgnoreCase); }
		}

		/// <summary>
		/// Will exclude if either the filename or the full path matches the expression.
		/// </summary>
		public string IgnoreRegExPattern
		{
			get { return _ignoreFilePattern.ToString(); }
			set { _ignoreFilePattern = new Regex(value, RegexOptions.IgnoreCase); }
		}

		/// <summary>
		/// Whether to just check that all the metadata is uptodate or not. If this is true then no file is output.
		/// </summary>
		public bool CheckOnly { get; set; }

		/// <summary>
		/// Directory where the installer source (.wixproj) is located.
		/// If provided, is used to determine relative path of the components
		/// </summary>
		public string InstallerSourceDirectory { get; set; }

		[Output, Required]
		public string OutputFilePath { get; set; }

		public override bool Execute()
		{

			if (!Directory.Exists(RootDirectory))
			{
				LogError("Directory not found: " + RootDirectory);
				return false;
			}

			LogMessage(MessageImportance.High, "Creating Wix fragment for " + RootDirectory);
			//make it an absolute path
			OutputFilePath = Path.GetFullPath(OutputFilePath);
			if (!string.IsNullOrEmpty(InstallerSourceDirectory))
				InstallerSourceDirectory = Path.GetFullPath(InstallerSourceDirectory);

			/* hatton removed this... it would leave deleted files referenced in the wxs file
			 if (File.Exists(_outputFilePath))
			{
				DateTime curFileDate = File.GetLastWriteTime(_outputFilePath);
				m_refDate = curFileDate;

				// if this assembly has been modified since the existing file was created then
				// force the output to be updated
				Assembly thisAssembly = Assembly.GetExecutingAssembly();
				DateTime assemblyTime = File.GetLastWriteTime(thisAssembly.Location);
				if (assemblyTime > curFileDate)
					m_filesChanged = true;
			}
			*/
			//instead, start afresh every time.

			if(File.Exists(OutputFilePath))
			{
				File.Delete(OutputFilePath);
			}

			SetupExclusions();

			try
			{
				var doc = new XmlDocument();
				var elemWix = doc.CreateElement("Wix", Xmlns);
				doc.AppendChild(elemWix);

				var elemFrag = doc.CreateElement("Fragment", Xmlns);
				elemWix.AppendChild(elemFrag);

				var elemDirRef = doc.CreateElement("DirectoryRef", Xmlns);
				elemDirRef.SetAttribute("Id", DirectoryReferenceId);
				elemFrag.AppendChild(elemDirRef);

				// recurse through the tree add elements
				ProcessDir(elemDirRef, Path.GetFullPath(RootDirectory), DirectoryReferenceId);

				// write out components into a group
				var elemGroup = doc.CreateElement("ComponentGroup", Xmlns);
				elemGroup.SetAttribute("Id", ComponentGroupId);

				elemFrag.AppendChild(elemGroup);

				AddComponentRefsToDom(doc, elemGroup);

				WriteDomToFile(doc);
			}
			catch (IOException e)
			{
				LogErrorFromException(e);
				return false;
			}

			return !HasLoggedErrors;
		}

		/// <summary>
		/// Though the guid-tracking *should* get stuff uninstalled, sometimes it does. So as an added precaution, delete the files on install and uninstall.
		/// Note that the *.* here should uninstall even files that were in the previous install but not this one.
		/// </summary>
		/// <param name="elemFrag"></param>
		private static void InsertFileDeletionInstruction(XmlNode elemFrag)
		{
			//awkwardly, wix only allows this in <component>, not <directory>. Further, the directory deletion equivalent (RemoveFolder) can only delete completely empty folders.
			var node = elemFrag.OwnerDocument?.CreateElement("RemoveFile", Xmlns);
			if (node == null)
				return;

			node.SetAttribute("Id", "_" + Guid.NewGuid().ToString().Replace("-", ""));
			node.SetAttribute("On", "both"); //both = install time and uninstall time "uninstall");
			node.SetAttribute("Name", "*.*");
			elemFrag.AppendChild(node);
		}

		private void WriteDomToFile(XmlNode doc)
		{
			// write the XML out onlystringles have been modified
			if (CheckOnly || !_filesChanged)
				return;

			var settings = new XmlWriterSettings {
				Indent = true,
				IndentChars = "    ",
				Encoding = Encoding.UTF8
			};
			using (var xmlWriter = XmlWriter.Create(OutputFilePath, settings))
			{
				doc.WriteTo(xmlWriter);
			}
		}

		private void AddComponentRefsToDom(XmlDocument doc, XmlNode elemGroup)
		{
			foreach (var c in _components)
			{
				var elem = doc.CreateElement("ComponentRef", Xmlns);
				elem.SetAttribute("Id", c);
				elemGroup.AppendChild(elem);
			}
		}

		private void SetupExclusions()
		{
			if (Exclude == null)
				return;

			foreach (var s in Exclude)
			{
				var key = Path.IsPathRooted(s)
					? s.ToLower()
					: Path.GetFullPath(Path.Combine(RootDirectory, s)).ToLower();
				_exclude.Add(key, s);
			}
		}

		public bool HasLoggedErrors { get; private set; }

		/// <summary>
		///   will show up as: DirectoryRef Id="this property"
		/// </summary>
		public string DirectoryReferenceId { get; set; }

		public string ComponentGroupId { get; set; }

		public void LogErrorFromException(Exception e)
		{
			HasLoggedErrors = true;
			Log.LogErrorFromException(e);
		}


		public void LogError(string s)
		{
			HasLoggedErrors = true;
			Log.LogError(s);
		}


		public void LogWarning(string s)
		{
			Log.LogWarning(s);
		}

		private void ProcessDir(XmlNode parent, string dirPath, string outerDirectoryId)
		{
			LogMessage(MessageImportance.Low, "Processing dir {0}", dirPath);

			var doc = parent.OwnerDocument;
			var files = new List<string>();

			var guidDatabase = IdToGuidDatabase.Create(Path.Combine(dirPath, FileNameOfGuidDatabase), this);

			SetupDirectoryPermissions(parent, outerDirectoryId, doc, guidDatabase);

			// Build a list of the files in this directory removing any that have been exluded
			// ReSharper disable once LoopCanBeConvertedToQuery
			foreach (var f in Directory.GetFiles(dirPath))
			{
				if (_fileMatchPattern.IsMatch(f) && !_ignoreFilePattern.IsMatch(f) && !_ignoreFilePattern.IsMatch(Path.GetFileName(f)) && !_exclude.ContainsKey(f.ToLower())
					&& !f.Contains(FileNameOfGuidDatabase) )
					files.Add(f);
			}

			// Process all files
			var isFirst = true;
			foreach (var path in files)
			{
				ProcessFile(parent, path, doc, guidDatabase, isFirst, outerDirectoryId);
				isFirst = false;
			}

			// Recursively process any subdirectories
			foreach (var d in Directory.GetDirectories(dirPath))
			{
				var shortName = Path.GetFileName(d);
				if (_exclude.ContainsKey(d.ToLower()) || shortName == ".svn" || shortName == "CVS")
					continue;

				var id = GetSafeDirectoryId(d, outerDirectoryId);

				var elemDir = doc?.CreateElement("Directory", Xmlns);
				if (elemDir == null)
					continue;

				elemDir.SetAttribute("Id", id);
				elemDir.SetAttribute("Name", shortName);
				parent.AppendChild(elemDir);

				ProcessDir(elemDir, d, id);

				if (elemDir.ChildNodes.Count == 0)
					parent.RemoveChild(elemDir);
			}
		}

		private void SetupDirectoryPermissions(XmlNode parent, string parentDirectoryId,
			XmlDocument doc, IdToGuidDatabase guidDatabase)
		{
			if (!GiveAllPermissions)
				return;

			/*	Need to add one of these in order to set the permissions on the directory
					 * <Component Id="biatahCacheDir" Guid="492F2725-9DF9-46B1-9ACE-E84E70AFEE99">
							<CreateFolder Directory="biatahCacheDir">
								<Permission GenericAll="yes" User="Everyone" />
							</CreateFolder>
						</Component>
					 */

			var id = GetSafeDirectoryId(string.Empty, parentDirectoryId);

			var componentElement = doc.CreateElement("Component", Xmlns);
			componentElement.SetAttribute("Id", id);
			componentElement.SetAttribute("Guid", guidDatabase.GetGuid(id, CheckOnly));

			var createFolderElement = doc.CreateElement("CreateFolder", Xmlns);
			createFolderElement.SetAttribute("Directory", id);
			AddPermissionElement(doc, createFolderElement);

			componentElement.AppendChild(createFolderElement);
			parent.AppendChild(componentElement);

			_components.Add(id);
		}

		private string GetSafeDirectoryId(string directoryPath, string parentDirectoryId)
		{
			var id = parentDirectoryId;
			//bit of a hack... we don't want our id to have this prefix.dir form fo the top level,
			//where it is going to be referenced by other wix files, that will just be expecting the id
			//the msbuild target gave for the id of this directory

			//I don't have it quite right, though. See the test file, where you get
			// <Component Id="common.bin.bin" (the last bin is undesirable)

			if (Path.GetFullPath(RootDirectory) != directoryPath)
			{
				id+="." + Path.GetFileName(directoryPath);
				id = id.TrimEnd('.'); //for the case where directoryPath is intentionally empty
			}
			id = Regex.Replace(id, @"[^\p{Lu}\p{Ll}\p{Nd}._]", "_");
			return id;
		}

		private void ProcessFile(XmlNode parent, string path, XmlDocument doc, IdToGuidDatabase guidDatabase, bool isFirst, string directoryId)
		{
			var name = Path.GetFileName(path);
			var id = directoryId+"."+name; //includ the parent directory id so that files with the same name (e.g. "index.html") found twice in the system will get different ids.

			const int kMaxLength = 50; //I have so far not found out what the max really is
			if (id.Length > kMaxLength)
			{
				id = id.Substring(id.Length - kMaxLength, kMaxLength); //get the last chunk of it
			}
			if (!char.IsLetter(id[0]) && id[0] != '_')//probably not needed now that we're prepending the parent directory id, accept maybe at the root?
				id = '_' + id;
			id = Regex.Replace(id, @"[^\p{Lu}\p{Ll}\p{Nd}._]", "_");

			LogMessage(MessageImportance.Normal, "Adding file {0} with id {1}", path, id);
			var key = id.ToLower();
			if (_suffixes.ContainsKey(key))
			{
				var suffix = _suffixes[key] + 1;
				_suffixes[key] = suffix;
				id += suffix.ToString();
			}
			else
			{
				_suffixes[key] = 0;
			}

			// Create <Component> and <File> for this file
			var elemComp = doc.CreateElement("Component", Xmlns);
			elemComp.SetAttribute("Id", id);
			var guid = guidDatabase.GetGuid(id,CheckOnly);
			if (guid == null)
				_filesChanged = true;        // this file is new
			else
				elemComp.SetAttribute("Guid", guid.ToUpper());
			parent.AppendChild(elemComp);

			var elemFile = doc.CreateElement("File", Xmlns);
			elemFile.SetAttribute("Id", id);
			elemFile.SetAttribute("Name", name);
			if (isFirst)
			{
				elemFile.SetAttribute("KeyPath", "yes");
			}

			var relativePath = PathUtil.RelativePathTo(string.IsNullOrEmpty(InstallerSourceDirectory) ?
				Path.GetDirectoryName(OutputFilePath) : InstallerSourceDirectory, path);
			elemFile.SetAttribute("Source", relativePath);

			if (GiveAllPermissions)
			{
				AddPermissionElement(doc, elemFile);
			}

			elemComp.AppendChild(elemFile);
			InsertFileDeletionInstruction(elemComp);
			_components.Add(id);

			// check whether the file is newer
			if (File.GetLastWriteTime(path) > _refDate)
				_filesChanged = true;
		}

		private static void AddPermissionElement(XmlDocument doc, XmlNode elementToAddPermissionTo)
		{
			var persmission = doc.CreateElement("Permission", Xmlns);
			persmission.SetAttribute("GenericAll", "yes");
			persmission.SetAttribute("User", "Everyone");
			elementToAddPermissionTo.AppendChild(persmission);
		}

		public void LogMessage(MessageImportance importance, string message)
		{
			try
			{
				Log.LogMessage(importance.ToString(), message);
			}
			catch (InvalidOperationException)
			{
				// Swallow exceptions for testing
			}
		}

		private void LogMessage(MessageImportance importance, string message, params object[] args)
		{
			try
			{
				Log.LogMessage(importance.ToString(), message, args);
			}
			catch (InvalidOperationException)
			{
				// Swallow exceptions for testing
			}
		}
		#endregion
	}
}