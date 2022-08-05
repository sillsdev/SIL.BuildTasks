// Copyright (c) 2016-2022 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.Build.Framework;
using Moq;
using NUnit.Framework;

namespace SIL.BuildTasks.Tests.UnitTestTasks
{
	// see https://docs.microsoft.com/en-us/visualstudio/msbuild/tutorial-test-custom-task
	[TestFixture]
	public class NUnitTests
	{
		internal static string OutputDirectoryOfHelper => Path.Combine(
			OutputDirectoryOfBuildTasks, "..", "..", "..", "SIL.BuildTasks.Tests",
			"SIL.BuildTasks.Tests.Helper", "bin", "net472");

		internal static string OutputDirectoryOfBuildTasks =>
			Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);

		private         Mock<IBuildEngine>        _buildEngine;
		private         List<BuildErrorEventArgs> _errors;
		private static string                     _nunitDir;

		internal static string NUnitDir
		{
			get
			{
				if (string.IsNullOrEmpty(_nunitDir))
				{
					CopyNUnit();
				}

				return _nunitDir;
			}
			set => _nunitDir = value;
		}

		internal static void CopyNUnit()
		{
			_nunitDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			Directory.CreateDirectory(_nunitDir);
			var sourceDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
				".nuget", "packages", "nunit.runners.net4", "2.6.4", "tools");
			foreach (var file in Directory.EnumerateFiles(sourceDir, "*.*",
				SearchOption.AllDirectories))
			{
				var dir = Path.GetDirectoryName(file);
				if (dir != null && dir.EndsWith("addins"))
				{
					// skip addins. This is required for TeamCity where we might use the
					// TC addin to report the progress of the tests. However, for
					// the unit tests we run as a subprocess we don't want the addin
					// so that we don't get the intentionally failing tests of the
					// subprocess reported as failed.
					continue;
				}

				var relativeDir = dir?.Substring(sourceDir.Length).TrimStart('\\', '/');
				if (relativeDir == null)
					continue;

				var targetDir = Path.Combine(_nunitDir, relativeDir);
				Directory.CreateDirectory(targetDir);
				// ReSharper disable once AssignNullToNotNullAttribute
				File.Copy(file, Path.Combine(targetDir, Path.GetFileName(file)));
			}
		}

		private BuildTasks.UnitTestTasks.NUnit GetNUnitTask(string testCategory)
		{
			var testAssembly = new Mock<ITaskItem>();
			testAssembly.Setup(x => x.ItemSpec).Returns(
				Path.Combine(OutputDirectoryOfHelper, "SIL.BuildTasks.Tests.Helper.dll"));
			var sut = new SIL.BuildTasks.UnitTestTasks.NUnit {
				Assemblies = new[] { testAssembly.Object },
				ToolPath = NUnitDir,
				TestInNewThread = false,
				Force32Bit = !Environment.Is64BitProcess,
				IncludeCategory = testCategory,
				Verbose = true,
				BuildEngine = _buildEngine.Object
			};
			return sut;
		}

		[OneTimeTearDown]
		public void FixtureTearDown()
		{
			if (string.IsNullOrEmpty(_nunitDir))
				return;

			Directory.Delete(_nunitDir, true);
			_nunitDir = null;
		}

		[SetUp]
		public void Setup()
		{
			_buildEngine = new Mock<IBuildEngine>();
			_errors = new List<BuildErrorEventArgs>();
			_buildEngine.Setup(x => x.LogErrorEvent(It.IsAny<BuildErrorEventArgs>()))
				.Callback<BuildErrorEventArgs>(e => _errors.Add(e));
		}

		[Test]
		public void Success_DoesntFailBuild()
		{
			// Setup
			var sut = GetNUnitTask("Success");

			// Execute
			var result = sut.Execute();

			// Verify
			Assert.That(result, Is.True, "Passing tests shouldn't fail the build");
			Assert.That(_errors.Count, Is.EqualTo(0));
		}

		[Test]
		public void FailingTests_DoesntFailBuild()
		{
			// Setup
			var sut = GetNUnitTask("Failing");

			// Execute
			var result = sut.Execute();

			// Verify
			Assert.That(result, Is.True, "Failing tests shouldn't fail the build");
			Assert.That(_errors.Count, Is.EqualTo(0));
		}

		[Test]
		public void Exception_FailsTestButNotBuild()
		{
			// Setup
			var sut = GetNUnitTask("Exception");

			// Execute
			var result = sut.Execute();

			// Verify
			Assert.That(result, Is.True, "Exception in test shouldn't fail the build");
			Assert.That(_errors.Count, Is.EqualTo(0));
		}

		[Test]
		public void OutputOnStderr_DoesntFailBuild()
		{
			// Setup
			var sut = GetNUnitTask("Stderr");

			// Execute
			var result = sut.Execute();

			// Verify
			Assert.That(result, Is.True, "Output on Stderr shouldn't fail the build");
			Assert.That(_errors.Count, Is.EqualTo(0));
		}

		[Test]
		public void ErrorOnStderr_FailsBuild()
		{
			// Setup
			var sut = GetNUnitTask("ErrorOnStdErr");

			// Execute
			var result = sut.Execute();

			// Verify
			Assert.That(result, Is.False, "Errors on Stderr should fail the build");
			Assert.That(_errors.Count, Is.EqualTo(1));
		}

		[Test]
		public void WarningOnStderr_DoesntFailBuild()
		{
			// Setup
			var sut = GetNUnitTask("WarningOnStdErr");

			// Execute
			var result = sut.Execute();

			// Verify
			Assert.That(result, Is.True, "Warnings on Stderr shouldn't fail the build");
			Assert.That(_errors.Count, Is.EqualTo(0));
		}
	}
}
