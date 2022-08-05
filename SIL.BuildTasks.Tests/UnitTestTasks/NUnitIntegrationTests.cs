// Copyright (c) 2016-2022 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using NUnit.Framework;

namespace SIL.BuildTasks.Tests.UnitTestTasks
{
	// see https://docs.microsoft.com/en-us/visualstudio/msbuild/tutorial-test-custom-task?view=vs-2022#integration-tests
	[TestFixture]
	public class NUnitIntegrationTests
	{
		private        Process _buildProcess;

		private static string GetBuildFilename(string category)
		{
			var buildFile = Path.GetTempFileName();
			File.WriteAllText(buildFile, $@"
<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
	<UsingTask TaskName='NUnit' AssemblyFile='{
					Path.Combine(NUnitTests.OutputDirectoryOfBuildTasks, "SIL.BuildTasks.dll")
				}' />
	<Target Name='Test'>
		<NUnit Assemblies='{
					Path.Combine(NUnitTests.OutputDirectoryOfHelper, "SIL.BuildTasks.Tests.Helper.dll")
				}' ToolPath='{NUnitTests.NUnitDir}' TestInNewThread='false' Force32Bit='{!Environment
				.Is64BitProcess}'
			IncludeCategory='{category}' Verbose='true' />
	</Target>
</Project>");

			return buildFile;
		}

		private bool ExecuteRunTests(string testCategory)
		{
			_buildProcess.StartInfo.Arguments = $"build /t:Test {GetBuildFilename(testCategory)}";
			_buildProcess.Start();
			_buildProcess.WaitForExit();
			return _buildProcess.ExitCode == 0;
		}

		[OneTimeTearDown]
		public void FixtureTearDown()
		{
			Directory.Delete(NUnitTests.NUnitDir, true);
			NUnitTests.NUnitDir = null;
		}

		[SetUp]
		public void Setup()
		{
			var dotnet = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
				? $"{Environment.GetEnvironmentVariable("ProgramW6432")}/dotnet/dotnet.exe"
				: "dotnet";

			_buildProcess = new Process();
			_buildProcess.StartInfo = new ProcessStartInfo {
				FileName = dotnet,
				WindowStyle = ProcessWindowStyle.Hidden,
				CreateNoWindow = true,
			};
		}

		[TearDown]
		public void TearDown()
		{
			_buildProcess.Close();
		}

		[TestCase("Success", true, "Passing tests shouldn't fail the build")]
		[TestCase("Failing", true, "Failing tests shouldn't fail the build")]
		[TestCase("Exception", true, "Exception in test shouldn't fail the build")]
		[TestCase("Stderr", true, "Output on Stderr shouldn't fail the build")]
		[TestCase("ErrorOnStdErr", false, "Errors on Stderr should fail the build")]
		[TestCase("WarningOnStdErr", true, "Warnings on Stderr shouldn't fail the build")]
		public void IntegrationTests(string testCategory, bool expected, string message)
		{
			Assert.That(ExecuteRunTests(testCategory), Is.EqualTo(expected), message);
		}

	}
}
