// Copyright (c) 2018 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using NUnit.Framework;
using SIL.BuildTasks.Tests;
using Is = SIL.BuildTasks.Tests.Is;

namespace SIL.BuildTasks
{
	[TestFixture]
	public class ConstrainStringByLineTests
	{
		private static string[] TestValues = { "One", "One\n", "One\nTwo", "One\nTwo\n", "One\nTwo\nFour", "One\r\nTwo" };
		private static string[] TestValuesWithNull = { null, "", "One", "One\n", "One\nTwo", "One\nTwo\n", "One\nTwo\nFour", "One\r\nTwo" };

		[Test]
		public void ApplyTo_Success([ValueSource(nameof(TestValues))] string line)
		{
			Assert.That(() => Assert.That(line, new ConstrainStringByLine(line)),
				Throws.Nothing);
		}

		[Test]
		[Combinatorial]
		public void ApplyTo_FailureActual(
			[ValueSource(nameof(TestValuesWithNull))] string actual,
			[ValueSource(nameof(TestValuesWithNull))] string expected)
		{
			// Filter out combinations that produce identical values
			if (actual?.Replace("\n", "").Replace("\r", "").TrimEnd('\n', '\r') ==
				expected?.Replace("\n", "").Replace("\r", "").TrimEnd('\n', '\r'))
				return;

			Assert.That(() => Assert.That(actual, new ConstrainStringByLine(expected)),
				Throws.TypeOf<AssertionException>());
		}

		[Test]
		public void Message_ActualToShort()
		{
			Assert.That(() => Assert.That("one", new ConstrainStringByLine("one\ntwo")),
				Throws.TypeOf<AssertionException>().With.Message.EqualTo($"  Expected: \"two\"{Environment.NewLine}  But was:  null{Environment.NewLine}"));
		}

		[Test]
		public void Message_ActualToLong()
		{
			Assert.That(() => Assert.That("one\ntwo", new ConstrainStringByLine("one")),
				Throws.TypeOf<AssertionException>().With.Message.EqualTo($"  Expected: end of string (null){Environment.NewLine}  But was:  \"two\"{Environment.NewLine}"));
		}

		[Test]
		public void Usage()
		{
			Assert.That(() => Assert.That("one\ntwo", Is.MultilineString("one\ntwo")),
				Throws.Nothing);
		}

		[Test]
		public void UsageNegate()
		{
			Assert.That(() => Assert.That("one\ntwo", Is.Not.MultilineString("one\nthree")),
				Throws.Nothing);
		}
	}
}
