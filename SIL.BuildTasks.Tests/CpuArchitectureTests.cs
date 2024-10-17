// Copyright (c) 2018 SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using NUnit.Framework;

namespace SIL.BuildTasks.Tests
{
	[TestFixture]
	public class CpuArchitectureTests
	{
		[Test]
		[Platform(Exclude = "Win")]
		public void CpuArchitecture_Linux()
		{
			var task = new CpuArchitecture();
			Assert.That(task.Execute(), Is.True);
			Assert.That(task.Value, Is.EqualTo(Environment.Is64BitOperatingSystem ? "x86_64" : "i686"));
		}

		[Test]
		[Platform(Include = "Win")]
		public void CpuArchitecture_Windows()
		{
			var task = new CpuArchitecture();
			Assert.That(task.Execute(), Is.True);
			Assert.That(task.Value, Is.EqualTo(Environment.Is64BitOperatingSystem ? "x64" : "x86"));
		}
	}
}
