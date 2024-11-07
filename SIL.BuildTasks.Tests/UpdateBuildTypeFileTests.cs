// Copyright (c) 2024 SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System.Text;
using NUnit.Framework;
// Sadly, Resharper wants to change Is.EqualTo to NUnit.Framework.Is.EqualTo
// ReSharper disable AccessToStaticMemberViaDerivedType

namespace SIL.BuildTasks.Tests
{
	[TestFixture]
	public class UpdateBuildTypeFileTests
	{
		[Test]
		public void GetVersionTypes_AlphaBetaRcProduction_GetsAllFourTypes()
		{
			_ = new UpdateBuildTypeFile.UpdateBuildTypeFile();

			var types = UpdateBuildTypeFile.UpdateBuildTypeFile.GetVersionTypes(GetFileContentsForType("Alpha"));
			Assert.That(types, Is.EquivalentTo(new [] {"Alpha", "Beta", "ReleaseCandidate", "Production"}));
		}

		[Test]
		public void GetVersionTypes_Custom_GetsAllCustomTypes()
		{
			_ = new UpdateBuildTypeFile.UpdateBuildTypeFile();

			var types = UpdateBuildTypeFile.UpdateBuildTypeFile.GetVersionTypes(GetFileContents("Fred", "Wilma", "BamBam", "Fred", "Barney", "Pebbles"));
			Assert.That(types, Is.EquivalentTo(new [] {"Wilma", "BamBam", "Fred","Barney", "Pebbles"}));
		}

		[Test]
		public void GetFileContents_UpdateAlphaToReleaseCandidate()
		{
			_ = new UpdateBuildTypeFile.UpdateBuildTypeFile();

			var contents = UpdateBuildTypeFile.UpdateBuildTypeFile.GetUpdatedFileContents(GetFileContentsForType("Alpha"), "ReleaseCandidate");
			Assert.That(contents, Is.EqualTo(GetFileContentsForType("ReleaseCandidate")));
		}

		[Test]
		public void GetFileContents_UpdateReleaseCandidateToProduction()
		{
			_ = new UpdateBuildTypeFile.UpdateBuildTypeFile();

			var contents = UpdateBuildTypeFile.UpdateBuildTypeFile.GetUpdatedFileContents(GetFileContentsForType("ReleaseCandidate"), "Production");
			Assert.That(contents, Is.EqualTo(GetFileContentsForType("Production")));
		}

		[Test]
		public void GetFileContents_UpdateBogusToBeta_NoReplacement()
		{
			_ = new UpdateBuildTypeFile.UpdateBuildTypeFile();

			var contents = UpdateBuildTypeFile.UpdateBuildTypeFile.GetUpdatedFileContents(GetFileContentsForType("Bogus"), "Beta");
			Assert.That(contents, Is.EqualTo(GetFileContentsForType("Bogus")));
		}

		private static string GetFileContentsForType(string type)
		{
			return GetFileContents(type, "Alpha", "Beta", "ReleaseCandidate", "Production");
		}

		private static string GetFileContents(string existingType, params string[] types)
		{
			var bldr = new StringBuilder();
			bldr.AppendLine("namespace SIL.BuildTasks.Tests");
			bldr.AppendLine("{");
			bldr.AppendLine("    public static class MyBuildType");
			bldr.AppendLine("    {");
			bldr.AppendLine("        public enum VersionType");
			bldr.AppendLine("        {");
			foreach (var type in types)
			{
				bldr.AppendLine($"            {type},");
			}
			bldr.AppendLine("        }");
			bldr.AppendLine();
			bldr.AppendLine($"        public static VersionType BuildType {{ get {{ return VersionType.{existingType}; }} }}");
			bldr.AppendLine("    }");
			bldr.AppendLine("}");

			return bldr.ToString();
		}
	}
}
