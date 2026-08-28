/*
 * Class to represent metadata (just GUIDs at the moment) about files.
 *
 * Originally from John Hall <john.hall@xjtag.com>. It was named "Metadata.cs"
 * Hatton says: This is used to keep the same GUID for each item,
 * even though we recreate the wix file. I've cleaned it up some, but haven't
 * looked at everything it is doing.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using Microsoft.Build.Framework;

namespace SIL.BuildTasks.MakeWixForDirTree
{
	internal class IdToGuidDatabase
	{
		private readonly ILogger _logger;
		private readonly string _filename;
		private readonly Dictionary<string, string> _guids = new Dictionary<string, string>();
		// Where each GUID came from. Usually _filename, but entries merged in by
		// ImportMissingFrom keep pointing at the file that actually supplied them.
		private readonly Dictionary<string, string> _origins = new Dictionary<string, string>();


		#region Construction

		private IdToGuidDatabase(string filename, ILogger logger)
		{
			_filename = filename;
			_logger = logger;
		}

		public static IdToGuidDatabase Create(string filename, ILogger owner)
		{
			if (!File.Exists(filename))
				return new IdToGuidDatabase(filename, owner);

			var settings = new XmlReaderSettings {
				IgnoreComments = true,
				IgnoreWhitespace = true
			};
			using (var rdr = XmlReader.Create(filename, settings))
			{
				var m = new IdToGuidDatabase(filename, owner);

				// skip XML declaration
				do
				{
					if (!rdr.Read())
						throw new XmlException("Unexpected EOF");
				} while (rdr.NodeType != XmlNodeType.Element);

				if (rdr.Name != "InstallerMetadata")
					return m;

				while (rdr.Read())
				{
					if (rdr.NodeType == XmlNodeType.Element && rdr.Name == "File")
					{
						var id = rdr.GetAttribute("Id");
						var guid = rdr.GetAttribute("Guid");
						if (id == null || guid == null)
							throw new XmlException("Unexpected format");

						m.Set(id, guid, filename);
					}
					else if (rdr.NodeType == XmlNodeType.EndElement)
					{
						break;
					}
					else
					{
						throw new XmlException("Unexpected format");
					}
				}

				return m;
			}
		}
		#endregion

		private string this[string id]
		{
			get
			{
				string ret;
				return _guids.TryGetValue(id, out ret) ? ret : null;
			}
		}

		private void Set(string id, string guid, string origin)
		{
			_guids[id] = guid;
			_origins[id] = origin;
		}

		private string OriginOf(string id)
		{
			string origin;
			return _origins.TryGetValue(id, out origin) ? origin : _filename;
		}


		#region Methods

		public string GetGuid(string id, bool justCheckDontCreate)
		{
			var guid = this[id];

			if (guid != null)
				return guid.ToUpper();

			if (justCheckDontCreate)
			{
				_logger.LogError("No GUID for " + id + " in " + _filename);
				// on an error we do not save the generated GUID
			}
			else
			{
				_logger.LogMessage(MessageImportance.Low, "No GUID for " + id + " in " + _filename);
				guid = Guid.NewGuid().ToString();
				Set(id, guid, _filename);
				Write();
			}

			return guid?.ToUpper();
		}

		/// <summary>
		/// Copies in every entry this database does not already have, and saves if
		/// anything was added. Used to consolidate the per-directory files into a
		/// single one: File Ids encode the whole relative path (for example
		/// "mercurial.lib.dulwich._pack.pyd"), so they are unique across the tree
		/// and the existing GUIDs can be merged without renaming anything.
		/// With justCheckDontCreate the entries are still merged in memory, so that
		/// GetGuid finds them, but nothing is written to disk.
		/// </summary>
		public void ImportMissingFrom(IdToGuidDatabase other, bool justCheckDontCreate)
		{
			if (other == null || ReferenceEquals(other, this))
				return;

			var added = false;
			foreach (var pair in other._guids)
			{
				var existing = this[pair.Key];
				if (existing == null)
				{
					Set(pair.Key, pair.Value, other._filename);
					added = true;
				}
				else if (!string.Equals(existing, pair.Value, StringComparison.OrdinalIgnoreCase))
				{
					// Cannot happen while Ids stay path-derived, but silently preferring
					// one GUID over another would be a nasty way to find out otherwise.
					_logger.LogError(string.Format(
						"Conflicting GUIDs for {0}: {1} (from {2}) and {3} (from {4}). Keeping the first.",
						pair.Key, existing, OriginOf(pair.Key), pair.Value, other._filename));
				}
			}

			if (added && !justCheckDontCreate) Write();
		}

		/// <summary>
		/// Logs an error for every entry this database holds only in memory, because it
		/// was merged in from another file rather than read from its own. Deleting those
		/// files without a run that writes this one would lose the GUIDs.
		///
		/// Only meaningful after a run that has not written: once Write() has run, the
		/// entries are in the file even though _origins still records where they began.
		/// </summary>
		public void ReportEntriesMissingFromFile()
		{
			var sources = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
			var count = 0;
			foreach (var pair in _origins)
			{
				if (string.Equals(pair.Value, _filename, StringComparison.OrdinalIgnoreCase))
					continue;

				sources.Add(pair.Value);
				count++;
			}

			if (count == 0)
				return;

			_logger.LogError(string.Format(
				"{0} is missing {1} GUID(s) still only held in {2}. Run without CheckOnly to write them, and commit the result, before deleting those files.",
				_filename, count, string.Join(", ", sources)));
		}

		private void Write()
		{
			var settings = new XmlWriterSettings {
				Indent = true,
				IndentChars = "  ",
				Encoding = Encoding.UTF8
			};

			using (var writer = XmlWriter.Create(_filename, settings))
			{
				writer.WriteComment("This file is generated and then updated by an MSBuild task.  It preserves the automatically-generated guids assigned files that will be installed on user machines. So it should be held in source control.");
				writer.WriteStartElement("InstallerMetadata");
				foreach (var id in _guids.Keys)
				{
					writer.WriteStartElement("File");
					writer.WriteAttributeString("Id", id);
					writer.WriteAttributeString("Guid", _guids[id]);
					writer.WriteEndElement();
				}
				writer.WriteEndElement(); // end InstallerMetadata
			}
		}

		#endregion
	}

	public interface ILogger
	{
		void LogError(string s);
		void LogMessage( MessageImportance messageImportance,string s);
	}
}