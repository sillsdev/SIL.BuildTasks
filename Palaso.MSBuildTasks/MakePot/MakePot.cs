using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Palaso.BuildTasks.MakePot
{
	public class MakePot: Task
	{
		readonly Dictionary<string, List<string>> _entries = new Dictionary<string, List<string>>();

		public ITaskItem[] CSharpFiles { get; set; }

		public ITaskItem[] XmlFiles { get; set; }

		[Required]
		public string ProjectId { get; set; }

		public string MsdIdBugsTo { get; set; }

		[Required]
		public string OutputFile { get; set; }

		public string XpathToStrings { get; set; }

		public override bool Execute()
		{
			using (StreamWriter writer = File.CreateText(OutputFile))
			{
				if (XmlFiles != null)
				{
					foreach (ITaskItem file in XmlFiles)
					{
						ProcessXmlFile(file);
					}
				}
				if (CSharpFiles != null)
				{
					foreach (ITaskItem file in CSharpFiles)
					{
						ProcessSrcFile(file.ItemSpec);
					}
				}

				WritePotHeader(writer);

				foreach (KeyValuePair<string, List<string>> pair in _entries)
				{
					WriteEntry(pair.Key, pair.Value, writer);
				}

				this.Log.LogMessage(MessageImportance.High, "MakePot wrote " + _entries.Count + " strings to " + OutputFile);
			}
			return true;
		}

		private void WritePotHeader(StreamWriter writer)
		{
			writer.WriteLine("msgid \"\"");
			writer.WriteLine("msgstr \"\"");
			writer.WriteLine("\"Project-Id-Version: {0}\"", ProjectId);
			writer.WriteLine("\"Report-Msgid-Bugs-To: {0}\"", MsdIdBugsTo);

			writer.WriteLine("\"POT-Creation-Date: {0}\"", DateTime.UtcNow.ToString("s"));
			writer.WriteLine("\"PO-Revision-Date: {0}\"", DateTime.UtcNow.ToString("s"));
			writer.WriteLine("\"Last-Translator: \"");
			writer.WriteLine("\"Language-Team: \"");
			writer.WriteLine("\"MIME-Version: 1.0\"");
			writer.WriteLine("\"Content-Type: text/plain; charset=UTF-8\"");
			writer.WriteLine("\"Content-Transfer-Encoding: 8bit\"");
		}

		private void ProcessXmlFile(ITaskItem  fileSpec)
		{
			if (string.IsNullOrEmpty(XpathToStrings))
			{
				this.Log.LogError("You must define XPathToStrings if you include anything in XPathFiles");
				return;
			}
			this.Log.LogMessage("Processing {0}", fileSpec.ItemSpec);
			XmlDocument doc = new XmlDocument();
			doc.Load(fileSpec.ItemSpec);
			foreach (XmlNode node in doc.SelectNodes(XpathToStrings))
			{
				AddStringInstance(node.InnerText, String.Empty);
			}
		}

		private void AddStringInstance(string stringToTranslate, string commentsForTranslator)
		{
			if (!_entries.ContainsKey(stringToTranslate)) //first time we've encountered this string?
			{
				this.Log.LogMessage(MessageImportance.Low, "Found '{0}'", stringToTranslate);
				_entries.Add(stringToTranslate, new List<string>());
			}
			_entries[stringToTranslate].Add(commentsForTranslator);//add this reference
		}

		private void ProcessSrcFile(string filePath)
		{
			this.Log.LogMessage("Processing {0}", filePath);
			string contents = File.ReadAllText(filePath);
			System.Text.RegularExpressions.Regex pattern =
				new System.Text.RegularExpressions.Regex(@"""~([^""]*)""\s*(,\s*""(.*)"")?", System.Text.RegularExpressions.RegexOptions.Compiled);

			foreach (System.Text.RegularExpressions.Match match in pattern.Matches(contents))
			{
				string str = match.Groups[1].Value;
				if (!_entries.ContainsKey(str)) //first time we've encountered this string?
				{
					this.Log.LogMessage(MessageImportance.Low, "Found '{0}'", str);
					_entries.Add(str, new List<string>());
				}
				string comments = "#; " + filePath;

				//catch the second parameter from calls like this:
				//            StringCatalog.Get("~Note", "The label for the field showing a note.");

				if (match.Groups.Count >= 3 && match.Groups[3].Length > 0)
				{
					string comment = match.Groups[3].Value;
					this.Log.LogMessage(MessageImportance.Low, "  with comment '{0}'", comment);
					comments += System.Environment.NewLine + "#. " + comment;
				}
				_entries[str].Add(comments);//add this reference
			}
		}

		private static void WriteEntry(string key, List<string> comments, StreamWriter writer)
		{
			writer.WriteLine("");
			foreach (string s in comments)
			{
				writer.WriteLine(s);
			}
			key = key.Replace("\"", "\\\"");
			writer.WriteLine("msgid \"" + key + "\"");
			writer.WriteLine("msgstr \"\"");
		}
	}
}