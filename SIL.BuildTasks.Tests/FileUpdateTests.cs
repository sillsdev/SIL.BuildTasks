// Copyright (c) 2023 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using NUnit.Framework;

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
		public string GetModifiedContents_RegexTextMatched_Succeeds(string origContents, string regex, string replacement)
		{
			var updater = new FileUpdate
			{
				Regex = regex,
				ReplacementText = replacement
			};

			return updater.GetModifiedContents(origContents);
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
