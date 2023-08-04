// Copyright (c) 2023 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace SIL.BuildTasks.Tests
{
	internal class FileUpdateWithTestErrorHandler : FileUpdate
	{
		internal readonly List<string> LoggedErrors = new List<string>();

		protected override void SafeLogError(string msg, params object[] args)
		{
			LoggedErrors.Add(string.Format(msg, args));
		}
	}

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
			var updater = new FileUpdateWithTestErrorHandler
			{
				Regex = regex,
				ReplacementText = replacement
			};

			var result = updater.GetModifiedContents(origContents, out var success);
			Assert.True(success);
			Assert.That(updater.LoggedErrors, Is.Empty);
			return result;
		}

		[TestCase("This is the story of the frog prince.", "princess", "soup")]
		[TestCase("This is the story of the frog prince.", "\\d+", "14")]
		public void GetModifiedContents_RegexTextNotMatched_ReportsErrorAndReturnsOrigContents(string origContents, string regex, string replacement)
		{
			var updater = new FileUpdateWithTestErrorHandler
			{
				Regex = regex,
				ReplacementText = replacement
			};

			var result = updater.GetModifiedContents(origContents, out var success);
			Assert.False(success);
			Assert.That(updater.LoggedErrors.Single(), Is.EqualTo($"No replacements made. Regex: '{regex}'; ReplacementText: '{replacement}'"));
			Assert.That(result, Is.EqualTo(origContents));
		}

		[Test]
		public void GetModifiedContents_InvalidRegex_ReportsErrorAndReturnsOrigContents()
		{
			var updater = new FileUpdateWithTestErrorHandler
			{
				Regex = "(.*",
				ReplacementText = "oops"
			};

			var result = updater.GetModifiedContents("Whatever", out var success);
			Assert.False(success);
			Assert.That(updater.LoggedErrors.Single(),
				Is.EqualTo($"Invalid regular expression: parsing \"{updater.Regex}\" - Not enough )'s."));
			Assert.That(result, Is.EqualTo("Whatever"));
		}
	}
}
