// Copyright (c) 2018 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
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

						m[id] = guid;
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
			set
			{
				_guids[id] = value;
			}
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
				this[id] = guid;
				Write();
			}

			return guid?.ToUpper();
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