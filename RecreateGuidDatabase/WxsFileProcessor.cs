// Copyright (c) 2020 SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace RecreateGuidDatabase
{
	public class WxsFileProcessor
	{
		private readonly string _baseDir;
		private readonly string _wxsFile;
		private readonly Dictionary<string, Dictionary<string, string>> _guidDatabases = new Dictionary<string, Dictionary<string, string>>();

		public WxsFileProcessor(string wxsFile)
		{
			_wxsFile = wxsFile;
			_baseDir = Path.GetDirectoryName(wxsFile);
		}

		public void ProcessWxsFile()
		{
			var settings = new XmlReaderSettings {
				IgnoreComments = true,
				IgnoreWhitespace = true
			};
			using var xmlReader = XmlReader.Create(_wxsFile, settings);

			// skip XML declaration
			do
			{
				if (!xmlReader.Read())
					throw new XmlException("Unexpected EOF");
			} while (xmlReader.NodeType != XmlNodeType.Element);

			if (xmlReader.Name != "Wix")
				throw new InvalidDataException($"Invalid root element {xmlReader.Name}, expected <Wix>");

			ProcessNextLevel(xmlReader);

			WriteGuidDatabaseFiles();
		}

		private void WriteGuidDatabaseFiles()
		{
			foreach (var (directory, guidDatabase) in _guidDatabases)
			{
				var guidsForInstallerFile = Path.Combine(_baseDir, directory, ".guidsForInstaller.xml");
				Console.WriteLine($"Writing {guidsForInstallerFile}");
				WriteGuidsForInstallerFile(guidsForInstallerFile, guidDatabase);
			}
		}

		private static void WriteGuidsForInstallerFile(string filename, Dictionary<string, string> guids)
		{
			Directory.CreateDirectory(Path.GetDirectoryName(filename));

			var settings = new XmlWriterSettings {
				Indent = true,
				IndentChars = "  ",
				Encoding = Encoding.UTF8
			};
			using var writer = XmlWriter.Create(filename, settings);

			writer.WriteComment("This file is generated and then updated by an MSBuild task. " +
				"It preserves the automatically-generated GUIDs assigned files that will be installed " +
				"on user machines. So it should be held in source control.");
			writer.WriteStartElement("InstallerMetadata");

			foreach (var (id, guid) in guids)
			{
				writer.WriteStartElement("File");
				writer.WriteAttributeString("Id", id);
				writer.WriteAttributeString("Guid", guid);
				writer.WriteEndElement();
			}
			writer.WriteEndElement(); // end InstallerMetadata
		}

		private void ProcessNextLevel(XmlReader xmlReader)
		{
			while (xmlReader.Read())
			{
				switch (xmlReader.NodeType)
				{
					case XmlNodeType.Element:
						switch (xmlReader.Name)
						{
							case "Wix":
							case "Fragment":
							case "Directory":
							case "DirectoryRef":
							case "ComponentGroup":
							case "ComponentRef":
								ProcessNextLevel(xmlReader);
								break;
							case "Component":
								ProcessComponent(xmlReader);
								break;
							default:
								throw new XmlException($"Unknown element {xmlReader.Name}");
						}

						break;
					case XmlNodeType.EndElement:
						return;
					default:
						throw new XmlException("Unexpected format");
				}
			}
		}

		private void ProcessComponent(XmlReader xmlReader)
		{
			var id = xmlReader.GetAttribute("Id");
			var guid = xmlReader.GetAttribute("Guid");
			var source = ProcessFileElement(xmlReader);
			var directory = ExtractDirectory(source);

			if (!_guidDatabases.TryGetValue(directory, out var guidDatabase))
			{
				guidDatabase = new Dictionary<string, string>();
				_guidDatabases[directory] = guidDatabase;
			}

			guidDatabase[id] = guid;
			ProcessNextLevel(xmlReader); // skip everything until end element
		}

		private static string ProcessFileElement(XmlReader xmlReader)
		{
			if (!xmlReader.Read())
				throw new EndOfStreamException("Expected <File> but found EOF instead");

			if (xmlReader.NodeType != XmlNodeType.Element || xmlReader.Name != "File")
				throw new XmlException($"Unexpected XML node {xmlReader.Name}; expected <File>");

			return xmlReader.GetAttribute("Source");
		}

		private static string ExtractDirectory(string source)
		{
			return Path.GetDirectoryName(source.Replace('\\', Path.DirectorySeparatorChar));
		}
	}
}