// Copyright (c) 2018 SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace SIL.BuildTasks
{
	/// <summary>
	/// Return the CPU architecture of the current system.
	/// </summary>
	[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
	public class CpuArchitecture : Task
	{
		public override bool Execute()
		{
			if (Environment.OSVersion.Platform == PlatformID.Unix)
			{
				var proc = new Process {
					StartInfo = {
						UseShellExecute = false,
						RedirectStandardOutput = true,
						FileName = "/usr/bin/arch"
					}
				};
				proc.Start();
				Value = proc.StandardOutput.ReadToEnd().TrimEnd();
				proc.WaitForExit();
			}
			else
			{
				Value = Environment.Is64BitOperatingSystem ? "x64" : "x86";
			}
			return true;
		}

		[Output]
		public string Value { get; set; }
	}
}
