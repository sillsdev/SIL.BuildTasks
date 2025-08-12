// Copyright (c) 2014-2025 SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
// Parts based on code by MJ Hutchinson http://mjhutchinson.com/journal/2010/01/25/integrating_gtk_application_mac

using System;
using System.Runtime.InteropServices;
using JetBrains.Annotations;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace SIL.BuildTasks
{
	/// <summary>
	/// Determines the Unix Name of the operating system executing the build.
	/// This is useful when determining Mac vs Linux during a build.
	/// On Mac, the output Value will be "Darwin".
	/// On Linux, the output Value will be "Linux".
	/// </summary>

	// This can be used to set DefineConstants during the PreBuild Target.
	// here is an example build/platform.targets file that can be included
	// in a CSPROJ file.  SYSTEM_MAC or SYSTEM_LINUX will be defined and
	// can be used in the C# code for #if conditional compilation.
	// <?xml version="1.0" encoding="utf-8" ?>
	// <Project xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
	//   <UsingTask TaskName="UnixName" AssemblyFile="SIL.BuildTasks.dll" />
	//   <Target Name="BeforeBuild">
	//     <UnixName>
	//       <Output TaskParameter="Value" PropertyName="UNIX_NAME" />
	//     </UnixName>
	//     <PropertyGroup>
	//       <DefineConstants Condition="'$(OS)' == 'Unix'">$(DefineConstants);SYSTEM_UNIX</DefineConstants>
	//       <DefineConstants Condition="'$(UNIX_NAME)' == 'Darwin'">$(DefineConstants);SYSTEM_MAC</DefineConstants>
	//       <DefineConstants Condition="'$(UNIX_NAME)' == 'Linux'">$(DefineConstants);SYSTEM_LINUX</DefineConstants>
	//     </PropertyGroup>
	//   </Target>
	// </Project>
	[PublicAPI]
	public class UnixName : Task
	{
		public override bool Execute()
		{
			Value = string.Empty;
			if (Environment.OSVersion.Platform != PlatformID.Unix)
				return !Log.HasLoggedErrors;

			var buf = IntPtr.Zero;
			try
			{
				buf = Marshal.AllocHGlobal(8192);
				// This is a hacktastic way of getting sysname from uname ()
				if (uname(buf) == 0)
					Value = Marshal.PtrToStringAnsi(buf);
				else
					Log.LogError("uname failed");
			}
			catch (Exception ex)
			{
				Log.LogError("Error calling uname: " + ex.Message);
			}
			finally
			{
				if (buf != IntPtr.Zero)
					Marshal.FreeHGlobal(buf);
			}

			return !Log.HasLoggedErrors;
		}

		[Output]
		public string Value { get; set; }

		[DllImport("libc")]
		private static extern int uname(IntPtr buf);
	}
}
