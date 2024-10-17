// Copyright (c) 2023 SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace SIL.BuildTasks
{
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
			try
			{
				var content = System.IO.File.ReadAllText(File);
				var newContents = GetModifiedContents(content);
				System.IO.File.WriteAllText(File, newContents);
				return true;
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				Debug.WriteLine(e.Message);
				SafeLogError(e.Message);
				return false;
			}
			
		}

		internal string GetModifiedContents(string content)
		{
			try
			{
				var regex = new Regex(Regex);
				if (!regex.IsMatch(content))
				{
					// This check will generally only work if ReplacementText is just a literal string.
					// If it contains references to regex match groups, and GetModifiedContents is run
					// against a previously modified file with the same arguments, most likely no match
					// will be found, and the "No replacements made" error will be displayed.
					if (content.Contains(ReplacementText))
					{
						// Most likely, the replacement was already done an a previous build step.
						return content;
					}

					throw new Exception($"No replacements made. Regex: '{Regex}'; " +
						$"ReplacementText: '{ReplacementText}'");
				}

				var newContents = regex.Replace(content, ReplacementText);

				if (!string.IsNullOrEmpty(DatePlaceholder))
					newContents = newContents.Replace(DatePlaceholder, DateTime.UtcNow.Date.ToString(DateFormat));

				return newContents;
			}
			catch (ArgumentException e)
			{
				throw new Exception("Invalid regular expression: " + e.Message, e);
			}
		}

		private void SafeLogError(string msg)
		{
			try
			{
				Log.LogError(msg);
			}
			catch (Exception)
			{
				//swallow... logging fails in the unit test environment, where the log isn't really set up
			}
		}
	}
}