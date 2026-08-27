// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Runtime.InteropServices;
using NUnit.Framework;

namespace SIL.BuildTasks.Tests.Helper
{
	/// <summary>
	/// This class defines some tests used for testing the NUnitTask class. The tests are not
	/// intended to be run directly, only by the NUnitTests.
	/// </summary>
	[TestFixture]
	public class Tests
	{
		[Test]
		[Category("Success")]
		public void Success()
		{
			Assert.Pass("This test always passes");
		}

		[Test]
		[Category("Failing")]
		public void Failing()
		{
			Assert.Fail("This test intentionally fails");
		}

		[Test]
		[Category("Exception")]
		public void Exception()
		{
			throw new ApplicationException("This test throws an exception");
		}

		[Test]
		[Category("Crash")]
		public void Crash()
		{
			// Force the process to crash with an access violation (mimics a native
			// crash) by writing through a null pointer. This is not a regular,
			// .NET exception that can be caught- it terminates the process.
			Marshal.WriteInt32(IntPtr.Zero, 42);
			Assert.Fail("Should have crashed");
		}

		[Test]
		[Category("Stderr")]
		public void Stderr()
		{
			Console.Error.Write("Just testing");
		}

		[Test]
		[Category("ErrorOnStdErr")]
		public void ErrorOnStdErr()
		{
			Console.Error.WriteLine("Error testing");
		}

		[Test]
		[Category("WarningOnStdErr")]
		public void WarningOnStdErr()
		{
			Console.Error.WriteLine("Just some warning");
		}
	}
}
