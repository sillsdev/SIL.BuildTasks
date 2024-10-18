// Copyright (c) 2024 SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using NUnit.Framework;
using SIL.BuildTasks.UnitTestTasks;
// Sadly, Resharper wants to change Is.EqualTo to NUnit.Framework.Is.EqualTo
// ReSharper disable AccessToStaticMemberViaDerivedType

namespace SIL.BuildTasks.Tests.UnitTestTasks
{
	[TestFixture]
	public class NUnit3Tests
	{
		[TestCase("", "", "")]
		[TestCase(" ", "", "")]
		[TestCase(null, "", "")]
		[TestCase("CategoryToInclude", "", " --where \"(cat=CategoryToInclude)\"")]
		[TestCase("CategoryToInclude,OtherCatToInclude", "", " --where \"(cat=CategoryToInclude or cat=OtherCatToInclude)\"")]
		[TestCase("CategoryToInclude, OtherCatToInclude", "", " --where \"(cat=CategoryToInclude or cat=OtherCatToInclude)\"")]
		[TestCase("CategoryToInclude, OtherCatToInclude, ", "", " --where \"(cat=CategoryToInclude or cat=OtherCatToInclude)\"")]
		[TestCase("CategoryToInclude, OtherCatToInclude, ThirdCatToInclude", "", " --where \"(cat=CategoryToInclude or cat=OtherCatToInclude or cat=ThirdCatToInclude)\"")]
		[TestCase("", "CategoryToExclude", " --where \"(cat!=CategoryToExclude)\"")]
		[TestCase("", "CategoryToExclude, OtherCatToExclude", " --where \"(cat!=CategoryToExclude and cat!=OtherCatToExclude)\"")]
		[TestCase("CategoryToInclude,OtherCatToInclude", "CategoryToExclude,OtherCatToExclude", " --where \"(cat=CategoryToInclude or cat=OtherCatToInclude) and (cat!=CategoryToExclude and cat!=OtherCatToExclude)\"")]
		public void AddIncludeAndExcludeArguments_BuildsProperString(string include, string exclude, string result)
		{
			var nUnit3 = new NUnit3
			{
				IncludeCategory = include,
				ExcludeCategory = exclude
			};

			Assert.That(nUnit3.AddIncludeAndExcludeArguments(), Is.EqualTo(result));
		}

		[TestCase(true)]
		[TestCase(false)]
		public void FailTaskIfAnyTestsFail_NotSetByUser_SetByTeamCityProperty(bool teamcity)
		{
			var nUnit3 = new NUnit3
			{
				TeamCity = teamcity
			};

			Assert.That(nUnit3.FailTaskIfAnyTestsFail, Is.EqualTo(teamcity));
		}

		[TestCase(true)]
		[TestCase(false)]
		public void FailTaskIfAnyTestsFail_SetByUser_NotAffectedByTeamCityProperty(bool failTaskIfAnyTestsFail)
		{
			var nUnit3 = new NUnit3 {
				FailTaskIfAnyTestsFail = failTaskIfAnyTestsFail,
				TeamCity = true
			};

			Assert.That(nUnit3.FailTaskIfAnyTestsFail, Is.EqualTo(failTaskIfAnyTestsFail));
			nUnit3.TeamCity = false;
			Assert.That(nUnit3.FailTaskIfAnyTestsFail, Is.EqualTo(failTaskIfAnyTestsFail));
		}
	}
}
