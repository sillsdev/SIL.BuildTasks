// Copyright (c) 2024 SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using NUnit.Framework;
// Sadly, Resharper wants to change Is.EqualTo to NUnit.Framework.Is.EqualTo
// ReSharper disable AccessToStaticMemberViaDerivedType

namespace SIL.BuildTasks.Tests
{
	[TestFixture]
	public class FileUpdateTests
	{
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
	}
}
