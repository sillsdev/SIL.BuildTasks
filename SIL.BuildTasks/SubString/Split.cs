// Copyright (c) 2018 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System.Diagnostics.CodeAnalysis;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace SIL.BuildTasks.SubString
{
	[SuppressMessage("ReSharper", "UnusedMember.Global")]
	[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
	[SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global")]
	[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
	public class Split : Task
	{
		public Split()
		{
			Input = "";
			Delimiter = ":";
			OutputSubString = 0;
			MaxSplit = 999;

			ReturnValue = "";

		}

		[Required]
		public string Input { get; set; }

		public string Delimiter { get; set; }

		public int OutputSubString { get; set; }

		public int MaxSplit { get; set; }

		[Output]
		public string ReturnValue { get; private set; }

		public override bool Execute()
		{
			var result = Input.Split(Delimiter.ToCharArray(), MaxSplit);
			if (OutputSubString >= result.Length)
				return false;

			ReturnValue = result[OutputSubString];
			return true;
		}

	}
}