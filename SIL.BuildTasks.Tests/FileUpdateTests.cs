// Copyright (c) 2025 SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using NUnit.Framework;
using SIL.Providers;
using SIL.TestUtilities.Providers;

// Sadly, Resharper wants to change Is.EqualTo to NUnit.Framework.Is.EqualTo
// ReSharper disable AccessToStaticMemberViaDerivedType

namespace SIL.BuildTasks.Tests
{
	[TestFixture]
	public class FileUpdateTests
	{
		[SetUp]
		public void Setup()
		{
			DateTimeProvider.SetProvider(
				new ReproducibleDateTimeProvider(new DateTime(2026, 4, 16)));
		}
		
		[TearDown]
		public void TearDown()
		{
			DateTimeProvider.ResetToDefault();
		}

		[TestCase("This is the story of the frog prince.", "frog", "monkey",
			ExpectedResult = "This is the story of the monkey prince.")]
		[TestCase("This is the story of the frog prince.", "f[^ ]+g", "toad",
			ExpectedResult = "This is the story of the toad prince.")]
		[TestCase("This is the story of the frog prince.", "(f[^ ]+g) prince", "$1 soup",
			ExpectedResult = "This is the story of the frog soup.")]
		[TestCase("There were 35 frog princes.", "\\d+", "14",
			ExpectedResult = "There were 14 frog princes.")]
		[TestCase("There were 35 frog princes.", "\\d+", "approximately $0",
			ExpectedResult = "There were approximately 35 frog princes.")]
		[TestCase("There were 35 frog princes.", "(?<number>\\d+)", "approximately ${number}",
			ExpectedResult = "There were approximately 35 frog princes.")]
		[TestCase("</TargetFrameworkProfile>\r\n    <Version>0.0.0.0</Version> <!-- Replaced by FileUpdate in build/build.proj -->    <Authors>SIL</Authors>", "<Version>0\\.0\\.0\\.0</Version>", "<Version>3.3.0</Version>",
			ExpectedResult = "</TargetFrameworkProfile>\r\n    <Version>3.3.0</Version> <!-- Replaced by FileUpdate in build/build.proj -->    <Authors>SIL</Authors>")]
		public string GetModifiedContents_RegexTextMatched_Succeeds(string origContents, string regex, string replacement)
		{
			var updater = new FileUpdate
			{
				Regex = regex,
				ReplacementText = replacement
			};

			return updater.GetModifiedContents(origContents);
		}

		[TestCase("</TargetFrameworkProfile>\r\n    <Version>3.3.0</Version> <!-- Previously replaced by FileUpdate in build/build.proj -->    <Authors>SIL</Authors>", "<Version>0\\.0\\.0\\.0</Version>", "<Version>3.3.0</Version>")]
		public void GetModifiedContents_RegexTextNotMatchedButReplacementTextFound_NoChangeOrError(string origContents, string regex, string replacement)
		{
			var updater = new FileUpdate
			{
				Regex = regex,
				ReplacementText = replacement
			};

			Assert.That(updater.GetModifiedContents(origContents), Is.EqualTo(origContents));
		}

		[TestCase("This is the story of the frog prince.", "princess", "soup")]
		[TestCase("This is the story of the frog prince.", "\\d+", "14")]
		public void GetModifiedContents_RegexTextNotMatched_Throws(string origContents, string regex, string replacement)
		{
			var updater = new FileUpdate
			{
				Regex = regex,
				ReplacementText = replacement
			};

			var ex = Assert.Throws<Exception>(() => updater.GetModifiedContents(origContents));
			Assert.That(ex.Message, Is.EqualTo($"No replacements made. Regex: '{regex}'; ReplacementText: '{replacement}'"));
		}

		[TestCase("_DATE_ _VERSION_\r\nStuff", "_DATE_", "M/yyyy", "4/2026 3.2.1\r\nStuff")]
		[TestCase("_DATE_ _VERSION_\r\nStuff done before _DATE_", "_DATE_", "M/yyyy", "4/2026 3.2.1\r\nStuff done before 4/2026")]
		[TestCase("&DATE; _VERSION_\r\n- point #1", "&DATE;", "dd-MM-yy", "16-04-26 3.2.1\r\n- point #1")]
		[TestCase("DATE _VERSION_", "DATE", "dd MMMM, yyyy", "16 April, 2026 3.2.1")]
		[TestCase("DATE _VERSION_", "DATE", null, "16/Apr/2026 3.2.1")] // Uses default date format
		public void GetModifiedContents_DateLiteral_InsertsDateWithSpecifiedDateFormat(
			string origContents, string datePlaceholder, string dateFormat,
			string expectedResult)
		{
			var updater = new FileUpdate
			{
				Regex = "_VERSION_",
				ReplacementText = "3.2.1",
				DatePlaceholder = datePlaceholder,
				DateFormat = dateFormat
			};

			var result = updater.GetModifiedContents(origContents);
			Assert.That(result, Is.EqualTo(expectedResult));
		}

		[TestCase("_DATE_ _VERSION_\r\nStuff", "M/yyyy", "4/2026 3.2.1\r\nStuff")]
		[TestCase("_DATE_ _VERSION_\r\nStuff done before _DATE_", "dd-MM-yy", "16-04-26 3.2.1\r\nStuff done before 16-04-26")]
		public void GetModifiedContents_SpecialDatePlaceholderButFileDoesNotSpecifyFormat_InsertsDateWithSpecifiedDateFormat(
			string origContents, string dateFormat, string expectedResult)
		{
			var updater = new FileUpdate
			{
				Regex = "_VERSION_",
				ReplacementText = "3.2.1",
				DatePlaceholder = "_DATE(*)_",
				DateFormat = dateFormat
			};
			
			var result = updater.GetModifiedContents(origContents);
			Assert.That(result, Is.EqualTo(expectedResult));
		}

		[TestCase("MM-yy")]
		[TestCase("dd MMMM")]
		public void GetModifiedContents_SpecialDatePlaceholderWithFileSpecifyingFormat_InsertsDateWithFormatFromFile(
			string format)
		{
			var origContents = $"_DATE({format})_\r\nStuff";
					
			var updater = new FileUpdate
			{
				Regex = "(.*)",
				ReplacementText = "$1",
				DatePlaceholder = "_DATE(*)_",
			};

			var currentDate = DateTimeProvider.Current.UtcNow.ToString(format);

			var result = updater.GetModifiedContents(origContents);
			Assert.That(result, Is.EqualTo($"{currentDate}\r\nStuff"));
		}

		[TestCase("MM-yyyy", "d MMMM yy")]
		[TestCase("dd MMMM", "MM/dd/yyyy")]
		public void GetModifiedContents_SpecialDatePlaceholderWithFileSpecifyingMultipleFormats_InsertsDateWithFormatsFromFile(
			string format1, string format2)
		{
			var origContents = $"First _DATE({format1})_\r\nSecond _DATE_\r\nLast _DATE({format2})_";

			var updater = new FileUpdate
			{
				Regex = "(.*)",
				ReplacementText = "$1",
				DatePlaceholder = "_DATE(*)_",
			};

			var currentDate = DateTimeProvider.Current.UtcNow;
			var currentDate1 = currentDate.ToString(format1);
			var currentDate2 = currentDate.ToString(format2);

			var result = updater.GetModifiedContents(origContents);
			Assert.That(result, Is.EqualTo($"First {currentDate1}\r\nSecond 16/Apr/2026\r\nLast {currentDate2}"));
		}

		[TestCase("es", "abril")]
		[TestCase("fr", "avril")]
		public void GetModifiedContents_SpecialDatePlaceholderWithLocalizedFileSpecifyingFormat_InsertsLocaleSpecificDateWithFormatFromFile(
			string locale, string localizedMonthName)
		{
			var origContents = "_DATE(d MMMM yyyy)_\r\nStuff";

			var updater = new FileUpdate
			{
				File = $"ReleaseNotes.{locale}.md",
				FileLocalePattern = @"\.(?<locale>[a-z]{2}(-\w+)?)\.md$",
				Regex = "(.*)",
				ReplacementText = "$1",
				DatePlaceholder = "_DATE(*)_",
			};

			var result = updater.GetModifiedContents(origContents);
			Assert.That(result, Is.EqualTo($"16 {localizedMonthName} 2026\r\nStuff"));
		}

		[Test]
		public void GetModifiedContents_InvalidRegex_Throws()
		{
			var updater = new FileUpdate
			{
				Regex = "(.*",
				ReplacementText = "oops"
			};

			var ex = Assert.Throws<Exception>(() => updater.GetModifiedContents("Whatever"));
			Assert.That(ex.Message, Is.EqualTo($"Invalid regular expression: parsing \"{updater.Regex}\" - Not enough )'s."));
		}

		[Test]
		public void FileLocalePattern_InvalidRegex_ThrowsArgumentException()
		{
			const string expr = @"ReleaseNotes\.(.*\.md";
			Assert.That(() =>
			{
				_ = new FileUpdate
				{
					FileLocalePattern = expr,
					ReplacementText = "oops"
				};
			}, Throws.ArgumentException.With.Message.EqualTo($"FileLocalePattern: Invalid regular expression: parsing \"{expr}\" - Not enough )'s."));
		}

		[TestCase("es")]
		[TestCase("fr")]
		[TestCase("zh-CN")]
		public void GetCultureFromFileName_MatchLocaleGroupToKnownCulture_GetsSpecifiedCulture(string localeSpecifier)
		{
			var fileUpdater = new FileUpdate
			{
				File = $"ReleaseNotes.{localeSpecifier}.md",
				FileLocalePattern = @"\.(?<locale>[a-z]{2}(-\w+)?)\.md$",
			};

			Assert.That(fileUpdater.GetCultureFromFileName().IetfLanguageTag,
				Is.EqualTo(localeSpecifier));
		}

		[TestCase("zz-Unknown")]
		[TestCase("qq-Weird")]
		public void GetCultureFromFileName_MatchLocaleGroupToUnknownCulture_ReturnsNull(string localeSpecifier)
		{
			var fileUpdater = new FileUpdate
			{
				File = $"ReleaseNotes.{localeSpecifier}.md",
				FileLocalePattern = @"\.(?<locale>[a-z]{2}(-\w+)?)\.md$",
			};

			Assert.That(fileUpdater.GetCultureFromFileName(), Is.Null);
		}

		[TestCase("es")]
		[TestCase("fr-FR")]
		[TestCase("de")]
		public void GetCultureFromFileName_EntireMatchIsKnownCulture_GetsSpecifiedCulture(string localeSpecifier)
		{
			var fileUpdater = new FileUpdate
			{
				File = $"ReleaseNotes.{localeSpecifier}.md",
				FileLocalePattern = @"(?<=\.)es|fr-FR|de(?=\.)",
			};

			Assert.That(fileUpdater.GetCultureFromFileName().IetfLanguageTag,
				Is.EqualTo(localeSpecifier));
		}

		[TestCase("My.bat.ate.your.homework.md", @"(?<=\.)[a-z]{4}(?=\.)")]
		[TestCase("ReleaseNotes.htm", ".+")]
		public void GetCultureFromFileName_EntireMatchIsUnknownCulture_ReturnsNull(string fileName, string pattern)
		{
			var fileUpdater = new FileUpdate
			{
				File = fileName,
				FileLocalePattern =  pattern,
			};

			Assert.That(fileUpdater.GetCultureFromFileName(), Is.Null);
		}

		[TestCase("My.bat.ate.your.homework.md", @"(?<=\.)[a-z]{22}(?=\.)")]
		[TestCase("ReleaseNotes.htm", @"(?<=\.)es|fr-FR|de(?=\.)")]
		[TestCase("ReleaseNotes.htm", @"\.(?<locale>[a-z]{2}(-\w+)?)\.md$")]
		public void GetCultureFromFileName_NoMatch_ReturnsNull(string fileName, string pattern)
		{
			var fileUpdater = new FileUpdate
			{
				File = fileName,
				FileLocalePattern = pattern,
			};

			Assert.That(fileUpdater.GetCultureFromFileName(), Is.Null);
		}
	}
}
