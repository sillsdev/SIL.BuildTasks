// Copyright (c) 2018 SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using System.Text;
using NUnit.Framework;
// ReSharper disable UnusedVariable

namespace SIL.BuildTasks.Tests
{
	[TestFixture]
	public class UpdateBuildTypeFileTests
	{
		[Test]
		public void GetVersionTypes_AlphaBetaRcProduction_GetsAllFourTypes()
		{
			var buildTypeFileMaker = new UpdateBuildTypeFile.UpdateBuildTypeFile();

			var types = UpdateBuildTypeFile.UpdateBuildTypeFile.GetVersionTypes(GetFileContentsForType("Alpha"));
			Assert.AreEqual(4, types.Count);
			Assert.IsTrue(types.Contains("Alpha"));
			Assert.IsTrue(types.Contains("Beta"));
			Assert.IsTrue(types.Contains("ReleaseCandidate"));
			Assert.IsTrue(types.Contains("Production"));
		}

		[Test]
		public void GetVersionTypes_Custom_GetsAllCustomTypes()
		{
			var buildTypeFileMaker = new UpdateBuildTypeFile.UpdateBuildTypeFile();

			var types = UpdateBuildTypeFile.UpdateBuildTypeFile.GetVersionTypes(GetFileContents("Fred", "Wilma", "BamBam", "Fred", "Barney", "Pebbles"));
			Assert.AreEqual(5, types.Count);
			Assert.IsTrue(types.Contains("Wilma"));
			Assert.IsTrue(types.Contains("BamBam"));
			Assert.IsTrue(types.Contains("Fred"));
			Assert.IsTrue(types.Contains("Barney"));
			Assert.IsTrue(types.Contains("Pebbles"));
		}

		[Test]
		public void GetFileContents_UpdateAlphaToReleaseCandidate()
		{
			var buildTypeFileMaker = new UpdateBuildTypeFile.UpdateBuildTypeFile();

			var contents = UpdateBuildTypeFile.UpdateBuildTypeFile.GetUpdatedFileContents(GetFileContentsForType("Alpha"), "ReleaseCandidate");
			Assert.AreEqual(GetFileContentsForType("ReleaseCandidate"), contents);
		}

		[Test]
		public void GetFileContents_UpdateReleaseCandidateToProduction()
		{
			var buildTypeFileMaker = new UpdateBuildTypeFile.UpdateBuildTypeFile();

			var contents = UpdateBuildTypeFile.UpdateBuildTypeFile.GetUpdatedFileContents(GetFileContentsForType("ReleaseCandidate"), "Production");
			Assert.AreEqual(GetFileContentsForType("Production"), contents);
		}

		[Test]
		public void GetFileContents_UpdateBogusToBeta_NoReplacement()
		{
			var buildTypeFileMaker = new UpdateBuildTypeFile.UpdateBuildTypeFile();

			var contents = UpdateBuildTypeFile.UpdateBuildTypeFile.GetUpdatedFileContents(GetFileContentsForType("Bogus"), "Beta");
			Assert.AreEqual(GetFileContentsForType("Bogus"), contents);
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
