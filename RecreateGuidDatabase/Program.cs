// Copyright (c) 2020 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;

namespace RecreateGuidDatabase
{
	public static class Program
	{
		public static void Main(string[] args)
		{
			if (args == null || args.Length != 1)
			{
				Console.WriteLine(@"
This tool re-creates the .guidsForInstaller files by extracting the GUIDs from an existing generated .wxs file.

Usage:
RecreateGuidDatabase <wxsfile>
");
				return;
			}

			var wxsFile = args[0];
			new WxsFileProcessor(wxsFile).ProcessWxsFile();
		}

	}
}