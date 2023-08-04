// Copyright (c) 2018 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace SIL.BuildTasks
{
	[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
	[SuppressMessage("ReSharper", "UnusedMember.Global")]
	[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
	public class FileUpdate : Task
	{
		private string _dateFormat;

		[Required]
		public string File { get; set; }

		[Required]
		public string Regex { get; set; }

		[Required]
		public string ReplacementText { get; set; }

		/// <summary>
		/// The string pattern to replace with the current date (UTC, dd/MMM/yyyy)
		/// </summary>
		public string DatePlaceholder { get; set; }

		/// <summary>
		/// The date format to output (default is dd/MMM/yyyy)
		/// </summary>
		public string DateFormat
		{
			get => _dateFormat ?? "dd/MMM/yyyy";
			set => _dateFormat = value;
		}

		public override bool Execute()
		{
			var content = System.IO.File.ReadAllText(File);
			var newContents = GetModifiedContents(content, out var result);
			if (result)
				System.IO.File.WriteAllText(File, newContents);
			return result;
		}

		internal string GetModifiedContents(string content, out bool success)
		{
			string newContents = null;
			try
			{
				Regex regex = new Regex(Regex);
				if (!regex.IsMatch(content))
				{
					SafeLogError("No replacements made. Regex: '{0}'; ReplacementText: '{1}'", Regex, ReplacementText);
					success = false;
				}
				else
				{
					newContents = regex.Replace(content, ReplacementText);

					if (!string.IsNullOrEmpty(DatePlaceholder))
						newContents = newContents.Replace(DatePlaceholder, DateTime.UtcNow.Date.ToString(DateFormat));

					success = true;
				}
			}
			catch (Exception e)
			{
				if (e is ArgumentException)
					SafeLogError("Invalid regular expression: " + e.Message);
				else
					SafeLogError(e.Message);
				success = false;
			}
			return success ? newContents : content;
		}

		protected virtual void SafeLogError(string msg, params object[] args)
		{
			try
			{
				Debug.WriteLine(msg, args);
				Log.LogError(msg, args);
			}
			catch (Exception)
			{
				//swallow... logging fails in the unit test environment, where the log isn't really set up
			}
		}
	}
}