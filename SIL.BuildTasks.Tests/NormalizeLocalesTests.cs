// Copyright (c) 2020 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System.Diagnostics;
using System.IO;
using System.Threading;
using NUnit.Framework;

namespace SIL.BuildTasks.Tests
{
	[TestFixture]
	public class NormalizeLocalesTests
	{
		private NormalizeLocales _task;
		private string _testDir;

		[SetUp]
		public void TestSetup()
		{
			_testDir = Path.Combine(Path.GetTempPath(), GetType().Name);
			_task = new NormalizeLocales { BuildEngine = new MockBuildEngine(), L10nsDirectory = _testDir };

			RecreateDirectory(_testDir);
		}

		[TearDown]
		public void TestTeardown()
		{
			try
			{
				Directory.Delete(_testDir, true);
			}
			catch
			{
				Debug.WriteLine("Test could not clean up data directories.");
			}
		}

		[Test]
		public void DoesntCrashWhenNoNameChange()
		{
			FileSystemSetup(new[] { "de", "en-US" });

			_task.Execute();

			// Verify that the already-normalized locale is still normalized
			VerifyLocale("de", "never-existed");
			// Verify that the locale with a region has been normalized
			VerifyLocale("en", "en-US");
		}

		[Test]
		public void Works()
		{
			FileSystemSetup(new[] { "de-DE", "en-US", "zh-CN" });

			_task.Execute();

			// Verify that normal languages have no country codes
			VerifyLocale("de", "de-DE");
			VerifyLocale("en", "en-US");

			// Verify that Chinese has the country code and that there is no regionless Chinese.
			VerifyLocale("zh-CN", "zh");
		}

		private void FileSystemSetup(string[] locales)
		{
			foreach (var locale in locales)
			{
				var localeDir = Path.Combine(_testDir, locale);
				Directory.CreateDirectory(localeDir);
				File.WriteAllText(Path.Combine(localeDir, $"strings-{locale}.xml"), "some strings");
				File.WriteAllText(Path.Combine(localeDir, $"Palaso.{locale}.xlf"), "pretend this is xliff");
				var projectDir = Path.Combine(localeDir, "someProject");
				Directory.CreateDirectory(projectDir);
				File.WriteAllText(Path.Combine(projectDir, $"SomeFile.{locale}.resx"), "contents");
			}
		}

		private void VerifyLocale(string expected, string not)
		{
			var localeDir = Path.Combine(_testDir, expected);
			AssertDirExists(localeDir);
			AssertDirDoesNotExist(Path.Combine(_testDir, not));

			AssertFileExists(Path.Combine(localeDir, $"strings-{expected}.xml"));
			AssertFileDoesNotExist(Path.Combine(localeDir, $"strings-{not}.xml"));
			AssertFileExists(Path.Combine(localeDir, $"Palaso.{expected}.xlf"));
			AssertFileDoesNotExist(Path.Combine(localeDir, $"Palaso.{not}.xlf"));
			AssertFileExists(Path.Combine(localeDir, "someProject", $"SomeFile.{expected}.resx"));
			AssertFileDoesNotExist(Path.Combine(localeDir, "someProject", $"SomeFile.{not}.resx"));
		}

		private static void AssertDirExists(string path)
		{
			Assert.That(Directory.Exists(path), $"Expected the directory {path} to exist, but it did not.");
		}

		private static void AssertDirDoesNotExist(string path)
		{
			Assert.That(!Directory.Exists(path), $"Expected the directory {path} not to exist, but it did.");
		}

		private static void AssertFileExists(string path)
		{
			Assert.That(File.Exists(path), $"Expected the file {path} to exist, but it did not.");
		}

		private static void AssertFileDoesNotExist(string path)
		{
			Assert.That(!File.Exists(path), $"Expected the file {path} not to exist, but it did.");
		}
		public static void RecreateDirectory(string path)
		{
			if (Directory.Exists(path))
			{
				try
				{
					Directory.Delete(path, true);
					Thread.Sleep(1000); // wait for the filesystem to finish deleting
				}
				catch
				{
                    Assert.Fail("Couldn't delete old test data from aborted runs");
				}
			}
			Directory.CreateDirectory(path);
		}
	}
}
