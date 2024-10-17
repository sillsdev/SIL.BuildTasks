// Copyright (c) 2018 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Build.Framework;
using NUnit.Framework;

namespace SIL.BuildTasks.Tests
{
	[TestFixture]
	public class MakePotTests
	{

		private class EnvironmentForTest : IDisposable
		{
			private string TempDir { get; }

			public EnvironmentForTest()
			{
				TempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
				Directory.CreateDirectory(TempDir);
			}

			#region Disposable related

			~EnvironmentForTest()
			{
				Dispose(false);
			}

			public void Dispose()
			{
				Dispose(true);
				// ReSharper disable once GCSuppressFinalizeForTypeWithoutDestructor
				GC.SuppressFinalize(true);
			}

			private void Dispose(bool disposing)
			{
				if (disposing)
					Directory.Delete(TempDir, true);
			}

			#endregion

			private static ITaskItem[] CreateTaskItemsForFilePath(string filePath)
			{
				var items = new ITaskItem[1];
				items[0] = new MockTaskItem(filePath);
				return items;
			}

			public string MakePotFile(string input)
			{
				var csharpFilePath = Path.Combine(TempDir, "csharp.cs");
				File.WriteAllText(csharpFilePath, input);

				var pot = new MakePot.MakePot {
					OutputFile = Path.Combine(TempDir, "output.pot"),
					CSharpFiles = CreateTaskItemsForFilePath(csharpFilePath),
					ProjectId = "Testing",
					MsdIdBugsTo = "bugs@example.com"
				};
				pot.Execute();

				return File.ReadAllText(pot.OutputFile);
			}
		}

		[Test]
		public void MatchesInCSharpString_StringWithTilde_HasMatch()
		{
			var contents = @"
somevar.MyLocalizableFunction('~MyLocalizableString');
".Replace("'", "\"");

			var pot = new MakePot.MakePot();
			var matches = pot.MatchesInCSharpString(contents);
			Assert.That(matches.Count, NUnit.Framework.Is.EqualTo(1));
			foreach (Match match in matches)
			{
				Assert.That(match.Groups.Count, NUnit.Framework.Is.EqualTo(3));
				Assert.That(match.Groups["key"].Value, NUnit.Framework.Is.EqualTo("MyLocalizableString"));
			}
		}

		[Test]
		public void MatchesInCSharpString_StringWithTildeAndNotes_HasMatchAndNotes()
		{
			var contents = @"
somevar.MyLocalizableFunction('~MyLocalizableString', 'MyTranslationNotes');
".Replace("'", "\"");

			var pot = new MakePot.MakePot();
			var matches = pot.MatchesInCSharpString(contents);
			Assert.That(matches.Count, NUnit.Framework.Is.EqualTo(1));
			foreach (Match match in matches)
			{
				Assert.That(match.Groups.Count, NUnit.Framework.Is.EqualTo(3));
				Assert.That(match.Groups["key"].Value, NUnit.Framework.Is.EqualTo("MyLocalizableString"));
				Assert.That(match.Groups["note"].Value, NUnit.Framework.Is.EqualTo("MyTranslationNotes"));

			}
		}

		[Test]
		public void MatchesInCSharpString_StringWithTwoMatches_DoesNotContainTildeInResult()
		{
			var contents = @"
somevar.MyLocalizableFunction(StringCatalog.Get('~MyLocalizableString', 'MyTranslationNotes'));
".Replace("'", "\"");

			var pot = new MakePot.MakePot();
			var matches = pot.MatchesInCSharpString(contents);
			Assert.That(matches.Count, NUnit.Framework.Is.EqualTo(1));
			foreach (Match match in matches)
			{
				Assert.That(match.Groups.Count, NUnit.Framework.Is.EqualTo(3));
				Assert.That(match.Groups["key"].Value, NUnit.Framework.Is.EqualTo("MyLocalizableString"));
				Assert.That(match.Groups["note"].Value, NUnit.Framework.Is.EqualTo("MyTranslationNotes"));

			}
		}

		[Test]
		public void MatchesInCSharpString_UsingStringCatalogNoTilde_HasMatchAndNotes()
		{
			var contents = @"
somevar.MyLocalizableFunction(StringCatalog.Get('MyLocalizableString', 'MyTranslationNotes'));
".Replace("'", "\"");

			var pot = new MakePot.MakePot();
			var matches = pot.MatchesInCSharpString(contents);
			Assert.That(matches.Count, NUnit.Framework.Is.EqualTo(1));
			foreach (Match match in matches)
			{
				Assert.That(match.Groups.Count, NUnit.Framework.Is.EqualTo(3));
				Assert.That(match.Groups["key"].Value, NUnit.Framework.Is.EqualTo("MyLocalizableString"));
				Assert.That(match.Groups["note"].Value, NUnit.Framework.Is.EqualTo("MyTranslationNotes"));

			}
		}

		[Test]
		public void MatchesInCSharpString_UsingStringCatalogGetFormattedNoTilde_HasMatchAndNotes()
		{
			var contents = @"
somevar.MyLocalizableFunction(StringCatalog.GetFormatted('MyLocalizableString {0}', 'MyTranslationNotes', someArg));
".Replace("'", "\"");

			var pot = new MakePot.MakePot();
			var matches = pot.MatchesInCSharpString(contents);
			Assert.That(matches.Count, NUnit.Framework.Is.EqualTo(1));
			foreach (Match match in matches)
			{
				Assert.That(match.Groups.Count, NUnit.Framework.Is.EqualTo(3));
				Assert.That(match.Groups["key"].Value, NUnit.Framework.Is.EqualTo("MyLocalizableString {0}"));
				Assert.That(match.Groups["note"].Value, NUnit.Framework.Is.EqualTo("MyTranslationNotes"));

			}
		}

		[Test]
		public void MatchesInCSharpString_UsingTextEqual_HasMatchAndNotes()
		{
			var contents = @"
somevar.Text = 'MyLocalizableString';
".Replace("'", "\"");

			var pot = new MakePot.MakePot();
			var matches = pot.MatchesInCSharpString(contents);
			Assert.That(matches.Count, NUnit.Framework.Is.EqualTo(1));
			foreach (Match match in matches)
			{
				Assert.That(match.Groups.Count, NUnit.Framework.Is.EqualTo(3));
				Assert.That(match.Groups["key"].Value, NUnit.Framework.Is.EqualTo("MyLocalizableString"));

			}
		}

		[Test]
		public void MatchesInCSharpString_StringWithBackslashQuote_MatchesToEndOfString()
		{
			var contents = @"
somevar.Text = 'MyLocalizableString \'InQuote\' end';
".Replace("'", "\"");

			const string expected = "MyLocalizableString \\\"InQuote\\\" end";

			var pot = new MakePot.MakePot();
			var matches = pot.MatchesInCSharpString(contents);
			Assert.That(matches.Count, NUnit.Framework.Is.EqualTo(1));
			foreach (Match match in matches)
			{
				Assert.That(match.Groups.Count, NUnit.Framework.Is.EqualTo(3));
				Assert.That(match.Groups["key"].Value, NUnit.Framework.Is.EqualTo(expected));

			}
		}

		[Test]
		public void UnescapeString_WithBackSlash_HasNoBackslash()
		{
			const string contents = @"don\'t want backslash";
			const string expected = "don't want backslash";

			var actual = MakePot.MakePot.UnescapeString(contents);
			Assert.That(actual, NUnit.Framework.Is.EqualTo(expected));
		}

		[Test]
		public void ProcessSrcFile_AllMatches_OutputsGoodPo()
		{
			var contents = @"
somevar.Text = 'FirstLocalizableString';

somevar.MyLocalizableFunction(StringCatalog.Get('SecondLocalizableString', 'SecondNotes'));

somevar.MyLocalizableFunction('~ThirdLocalizableString', 'ThirdNotes');

".Replace("'", "\"");

			var expected =
@"msgid ''
msgstr ''
'Project-Id-Version: Testing\n'
'POT-Creation-Date: .*
'PO-Revision-Date: \n'
'Last-Translator: \n'
'Language-Team: \n'
'Plural-Forms: \n'
'MIME-Version: 1.0\n'
'Content-Type: text/plain; charset=UTF-8\n'
'Content-Transfer-Encoding: 8bit\n'

# Project-Id-Version: Testing
# Report-Msgid-Bugs-To: bugs@example.com
# POT-Creation-Date: .*
# Content-Type: text/plain; charset=UTF-8


#: .*
msgid 'FirstLocalizableString'
msgstr ''

#: .*
#. SecondNotes
msgid 'SecondLocalizableString'
msgstr ''

#: .*
#. ThirdNotes
msgid 'ThirdLocalizableString'
msgstr ''
".Replace("'", "\"");

			using (var e = new EnvironmentForTest())
			{
				Assert.That(e.MakePotFile(contents), Is.MultilineString(expected));
			}
		}

		[Test]
		public void ProcessSrcFile_BackupStringWithDots_DoesNotHaveDuplicates()
		{
			var contents = @"
somevar.Text = 'Backing Up...';
".Replace("'", "\"");

			var expected =
@"msgid ''
msgstr ''
'Project-Id-Version: Testing\n'
'POT-Creation-Date: .*
'PO-Revision-Date: \n'
'Last-Translator: \n'
'Language-Team: \n'
'Plural-Forms: \n'
'MIME-Version: 1.0\n'
'Content-Type: text/plain; charset=UTF-8\n'
'Content-Transfer-Encoding: 8bit\n'

# Project-Id-Version: Testing
# Report-Msgid-Bugs-To: bugs@example.com
# POT-Creation-Date: .*
# Content-Type: text/plain; charset=UTF-8


#: .*csharp.cs
msgid 'Backing Up...'
msgstr ''
".Replace("'", "\"");

			using (var e = new EnvironmentForTest())
			{
				Assert.That(e.MakePotFile(contents), Is.MultilineString(expected));
			}
		}

		[Test]
		public void ProcessSrcFile_BackupStringWithDuplicates_HasOnlyOneInOutput()
		{
			var contents = @"
somevar.Text = 'Backing Up...';

somevar.Text = 'Backing Up...';
".Replace("'", "\"");

			var expected =
@"msgid ''
msgstr ''
'Project-Id-Version: Testing\n'
'POT-Creation-Date: .*
'PO-Revision-Date: \n'
'Last-Translator: \n'
'Language-Team: \n'
'Plural-Forms: \n'
'MIME-Version: 1.0\n'
'Content-Type: text/plain; charset=UTF-8\n'
'Content-Transfer-Encoding: 8bit\n'

# Project-Id-Version: Testing
# Report-Msgid-Bugs-To: bugs@example.com
# POT-Creation-Date: .*
# Content-Type: text/plain; charset=UTF-8


#: .*csharp.cs
#: .*csharp.cs
msgid 'Backing Up...'
msgstr ''
".Replace("'", "\"");

			using (var e = new EnvironmentForTest())
			{
				Assert.That(e.MakePotFile(contents), Is.MultilineString(expected));
			}
		}

		[Test]
		public void ProcessSrcFile_EmptyString_NotPresentInOutput()
		{
			var contents = @"
somevar.Text = '';
".Replace("'", "\"");

			var expected =
@"msgid ''
msgstr ''
'Project-Id-Version: Testing\n'
'POT-Creation-Date: .*
'PO-Revision-Date: \n'
'Last-Translator: \n'
'Language-Team: \n'
'Plural-Forms: \n'
'MIME-Version: 1.0\n'
'Content-Type: text/plain; charset=UTF-8\n'
'Content-Transfer-Encoding: 8bit\n'

# Project-Id-Version: Testing
# Report-Msgid-Bugs-To: bugs@example.com
# POT-Creation-Date: .*
# Content-Type: text/plain; charset=UTF-8

".Replace("'", "\"");

			using (var e = new EnvironmentForTest())
			{
				Assert.That(e.MakePotFile(contents), Is.MultilineString(expected));
			}
		}

	}
}
