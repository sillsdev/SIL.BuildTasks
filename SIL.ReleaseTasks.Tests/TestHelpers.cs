// Copyright (c) 2018 SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Microsoft.Build.Framework;
using NUnit.Framework;

namespace SIL.ReleaseTasks.Tests
{
	public static class AssertThatXmlIn
	{
		public static AssertFile File(string path)
		{
			return new AssertFile(path);
		}
	}

	public class AssertFile
	{
		private class NullXMlNodeList : XmlNodeList
		{
			public override XmlNode Item(int index)
			{
				throw new ArgumentOutOfRangeException();
			}

			public override IEnumerator GetEnumerator()
			{
				yield return null;
			}

			public override int Count => 0;
		}

		private readonly string _path;

		public AssertFile(string path)
		{
			_path = path;
		}

		private XmlNode NodeOrDom
		{
			get
			{
				var dom = new XmlDocument();
				dom.Load(_path);
				return dom;
			}
		}

		private XmlNode GetNode(string xpath, XmlNamespaceManager nameSpaceManager)
		{
			return NodeOrDom.SelectSingleNode(xpath, nameSpaceManager);
		}

		private static XmlNamespaceManager GetNsmgr(XmlNode node, string prefix)
		{
			try
			{
				var document = node as XmlDocument;
				string namespaceUri;
				XmlNameTable nameTable;
				if (document != null)
				{
					nameTable = document.NameTable;
					namespaceUri = document.DocumentElement?.NamespaceURI;
				}
				else
				{
					nameTable = node.OwnerDocument?.NameTable;
					namespaceUri = node.NamespaceURI;
				}
				if(string.IsNullOrEmpty(namespaceUri))
				{
					return null;
				}
				// ReSharper disable once AssignNullToNotNullAttribute
				var nsmgr = new XmlNamespaceManager(nameTable);
				nsmgr.AddNamespace(prefix, namespaceUri);
				return nsmgr;

			}
			catch (Exception error)
			{
				throw new ApplicationException($"Could not create a namespace manager for the following node:{Environment.NewLine}{node.OuterXml}", error);
			}
		}

		private static string GetPrefixedPath(string xPath, string prefix)
		{
			//the code I purloined from stackoverflow didn't cope with axes and the double colon (ancestor::)
			//Rather than re-write it, I just get the axes out of the way, then put them back after we insert the prefix
			var axes = new List<string>(new[] {"ancestor","ancestor-or-self","attribute","child","descendant","descendant-or-self","following","following-sibling","namespace","parent","preceding","preceding-sibling","self" });
			foreach (var axis in axes)
			{
				xPath = xPath.Replace(axis+"::", "#"+axis);
			}

			var validLeadCharacters = "@/".ToCharArray();
			var quoteChars = "\'\"".ToCharArray();

			var pathParts = xPath.Split("/".ToCharArray()).ToList();
			var result = string.Join("/",
				pathParts.Select(x =>
					// ReSharper disable once ArrangeRedundantParentheses
					(string.IsNullOrEmpty(x) ||
					x.IndexOfAny(validLeadCharacters) == 0 ||
					x.IndexOf(':') > 0 &&
					(x.IndexOfAny(quoteChars) < 0 || x.IndexOfAny(quoteChars) > x.IndexOf(':')))
						? x
						: prefix + ":" + x).ToArray());

			foreach (var axis in axes)
			{
				if (result.Contains(axis + "-")) //don't match on, e.g., "following" if what we have is "following-sibling"
					continue;
				result = result.Replace(prefix + ":#"+axis, axis+"::" + prefix + ":");
			}

			result = result.Replace(prefix + ":text()", "text()"); //remove the pfx from the text()
			result = result.Replace(prefix + ":node()", "node()");
			return result;
		}

		private static XmlNodeList SafeSelectNodes(XmlNode node, string path)
		{
			const string prefix = "pfx";
			var nsmgr = GetNsmgr(node, prefix);
			if(nsmgr!=null) // skip this pfx business if there is no namespace anyhow (as in html5)
			{
				path = GetPrefixedPath(path, prefix);
			}
			var x= node.SelectNodes(path, nsmgr);

			return x ?? new NullXMlNodeList();
		}


		public void HasNoMatchForXpath(string xpath)
		{
			var nameSpaceManager = new XmlNamespaceManager(new NameTable());
			var node = GetNode(xpath, nameSpaceManager);
			Assert.IsNull(node, "Should not have matched: {0}", xpath);
		}

		/// <summary>
		/// Will honor default namespace
		/// </summary>
		public void HasSpecifiedNumberOfMatchesForXpath(string xpath, int count)
		{
			var nodes = SafeSelectNodes(NodeOrDom, xpath);
			if (nodes==null)
			{
				Assert.That(count, Is.EqualTo(0), $"Expected {count} but got 0 matches for {xpath}");
			}
			else if (nodes.Count != count)
			{
				Assert.That(nodes.Count, Is.EqualTo(count), $"Expected {count} but got {nodes.Count} matches for {xpath}");
			}
		}

	}

	public sealed class MockEngine : IBuildEngine
	{
		public readonly List<string> LoggedMessages = new List<string>();

		public void LogErrorEvent(BuildErrorEventArgs e)
		{
			LoggedMessages.Add(e.Message);
		}

		public void LogWarningEvent(BuildWarningEventArgs e)
		{
			LoggedMessages.Add(e.Message);
		}

		public void LogMessageEvent(BuildMessageEventArgs e)
		{
			LoggedMessages.Add(e.Message);
		}

		public void LogCustomEvent(CustomBuildEventArgs e)
		{
			LoggedMessages.Add(e.Message);
		}

		public bool BuildProjectFile(string projectFileName, string[] targetNames, IDictionary globalProperties,
			IDictionary targetOutputs)
		{
			throw new NotImplementedException();
		}

		public bool ContinueOnError => false;
		public int LineNumberOfTaskNode => 0;
		public int ColumnNumberOfTaskNode => 0;
		public string ProjectFileOfTaskNode => null;
	}

	/// <summary>
	/// This class is implemented to avoid a dependency on Palaso (which isn't strictly circular, but sure feels like it)
	/// The TempFile class that lives in SIL.IO is a more robust and generally preferred implementation.
	/// </summary>
	public sealed class TwoTempFilesForTest : IDisposable
	{
		public string FirstFile { get; }
		public string SecondFile { get; }

		public TwoTempFilesForTest(string firstFile, string secondFile)
		{
			FirstFile = firstFile;
			SecondFile = secondFile;
		}

		public void Dispose()
		{
			try
			{
				if (File.Exists(FirstFile))
				{
					File.Delete(FirstFile);
				}
				if (File.Exists(SecondFile))
				{
					File.Delete(SecondFile);
				}
			}
			catch(Exception)
			{
				// We try to clean up after ourselves, but we aren't going to fail tests if we couldn't
			}
		}
	}
}
