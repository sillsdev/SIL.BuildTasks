// Copyright (c) 2024 SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using Microsoft.Build.Framework;
using NUnit.Framework;
// Sadly, Resharper wants to change Is.EqualTo to NUnit.Framework.Is.EqualTo
// ReSharper disable AccessToStaticMemberViaDerivedType

namespace SIL.BuildTasks.Tests
{
	[TestFixture]
	public class ArchiveTests
	{
		private class EnvironmentForTest : IDisposable
		{
			public static ITaskItem[] TwoItemsWithBasePath(string a, string b)
			{
				ITaskItem[] result = {
					new MockTaskItem(BasePath + a),
					new MockTaskItem(BasePath + b)
				};
				return result;
			}

			public static string BasePath => "/trim/path/";

			public void Dispose()
			{
			}
		}

		[Test]
		public void ExecutableName_ForTar_Tar()
		{
			var task = new Archive.Archive { Command = "Tar" };
			Assert.That(task.ExecutableName(), Is.EqualTo("tar"));
		}

		[Test]
		public void ExecutableName_ForUnknown_EmptyString()
		{
			var task = new Archive.Archive { Command = "Unknown" };
			Assert.That(task.ExecutableName(), Is.EqualTo(string.Empty));
		}

		[Test]
		public void Arguments_ForTar_CorrectAndIncludeFileName()
		{
			var task = new Archive.Archive {
				Command = "Tar",
				OutputFileName = "MyOutputFile.tar.gz"
			};
			Assert.That(task.Arguments(), Is.EqualTo("-cvzf MyOutputFile.tar.gz"));
		}

		[Test]
		public void Arguments_ForUnknown_EmptyString()
		{
			var task = new Archive.Archive {
				Command = "Unknown",
				OutputFileName = "MyOutputFile.tar.gz"
			};
			Assert.That(task.Arguments(), Is.EqualTo(string.Empty));
		}

		[Test]
		public void TrimBaseFromFilePath_WithBase_ExcludesBasePath()
		{
			var task = new Archive.Archive { BasePath = "/trim/this/path/" };
			var result = task.TrimBaseFromFilePath("/trim/this/path/myproject/here");
			Assert.That(result, Is.EqualTo("myproject/here"));
		}

		[Test]
		public void TrimBaseFromFilePath_WithBaseNoTrailingSlash_ExcludesBasePath()
		{
			var task = new Archive.Archive { BasePath = "/trim/this/path" };
			var result = task.TrimBaseFromFilePath("/trim/this/path/myproject/here");
			Assert.That(result, Is.EqualTo("myproject/here"));
		}

		[Test]
		public void FlattenFilePaths_TwoItems_TrimsAndStringCorrect()
		{
			using (new EnvironmentForTest())
			{
				var task = new Archive.Archive {
					BasePath = EnvironmentForTest.BasePath,
					InputFilePaths = EnvironmentForTest.TwoItemsWithBasePath("a.cs", "b.cs")
				};
				var result = task.FlattenFilePaths(task.InputFilePaths, ' ', false);
				Assert.That(result, Is.EqualTo("a.cs b.cs"));
			}
		}

		[Test]
		public void FlattenFilePaths_TwoItemsOneWithSpace_TrimsAndQuotesStringCorrect()
		{
			using (new EnvironmentForTest())
			{
				var task = new Archive.Archive {
					BasePath = EnvironmentForTest.BasePath,
					InputFilePaths = EnvironmentForTest.TwoItemsWithBasePath("a space.cs", "b.cs")
				};
				var result = task.FlattenFilePaths(task.InputFilePaths, ' ', false);
				Assert.That(result, Is.EqualTo("\"a space.cs\" b.cs"));
			}
		}

		[Test]
		public void FlattenFilePaths_TwoItemsForceQuote_TrimsAndQuotesStringCorrect()
		{
			using (new EnvironmentForTest())
			{
				var task = new Archive.Archive {
					BasePath = EnvironmentForTest.BasePath,
					InputFilePaths = EnvironmentForTest.TwoItemsWithBasePath("a.cs", "b.cs")
				};
				var result = task.FlattenFilePaths(task.InputFilePaths, ' ', true);
				Assert.That(result, Is.EqualTo("\"a.cs\" \"b.cs\""));
			}
		}
	}
}
