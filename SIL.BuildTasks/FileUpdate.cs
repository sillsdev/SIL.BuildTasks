// Copyright (c) 2025 SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using static System.IO.File;
using static System.Text.RegularExpressions.RegexOptions;

namespace SIL.BuildTasks
{
	public class FileUpdate : Task
	{
		private string _dateFormat;
		private Regex _localeRegex;

		[Required]
		public string File { get; set; }

		[Required]
		public string Regex { get; set; }

		[Required]
		public string ReplacementText { get; set; }

		/// <summary>
		/// The string pattern to replace with the current date. If this is specified as
		/// `_DATE(*)_`, then this will be treated as a regex that will match `_DATE_` as well as
		/// any string that matches _DATE?&lt;?dateFormat?&gt;([dMy/:.,\-\s']+)_, in which case
		/// the `dateFormat` match group will be used to format the date instead of
		/// <see cref="DateFormat"/>.
		/// </summary>
		public string DatePlaceholder { get; set; }

		/// <summary>
		/// Default date format, used unless the <see cref="DatePlaceholder"/> finds a match that
		/// specifies an alternate format.
		/// </summary>
		public string DateFormat
		{
			get => _dateFormat ?? "dd/MMM/yyyy";
			set => _dateFormat = value;
		}

		/// <summary>
		/// Optional regex pattern with a named group 'locale' to extract the locale from the
		/// filename.
		/// Example: @"\.(?&lt;locale&gt;[a-z]{2}(-\w+)?)\.md$" to match "es", "fr", "zh-CN", etc.,
		/// between dots and preceding the final md (markdown) extension.
		/// If there is no named group 'locale', then the entire match will be treated
		/// as the locale.
		/// Example: @"(?&lt;=\.).es|fr|de(?=\.)"
		/// </summary>
		/// <exception cref="ArgumentException">The given pattern is not a well-formed regular expression</exception>
		/// <remarks>If the pattern matches more than once, the first match will be used.</remarks>
		public string FileLocalePattern
		{
			get => _localeRegex?.ToString();
			set
			{
				try
				{
					_localeRegex = string.IsNullOrEmpty(value) ? null : new Regex(value);
				}
				catch (ArgumentException e)
				{
					throw new ArgumentException("FileLocalePattern: Invalid regular expression: " + e.Message, e);
				}
			}
		}

		public override bool Execute()
		{
			try
			{
				var content = ReadAllText(File);
				var newContents = GetModifiedContents(content);
				WriteAllText(File, newContents);
				return true;
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				Debug.WriteLine(e);
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
				{
					var culture = GetCultureFromFileName() ?? CultureInfo.CurrentCulture;

					if (DatePlaceholder.Equals("_DATE(*)_", StringComparison.Ordinal))
					{
						var dateRegex = new Regex(
							@"_DATE(\((?<dateFormat>[dMy\/:.,\-\s'M]+)\))?_", Compiled);
						newContents = dateRegex.Replace(newContents, m =>
						{
							var format = m.Groups["dateFormat"].Success
								? m.Groups["dateFormat"].Value
								: DateFormat;
							return DateTime.UtcNow.Date.ToString(format, culture);
						});
					}
					else
					{
						var formattedDate = DateTime.UtcNow.Date.ToString(DateFormat, culture);
						newContents = newContents.Replace(DatePlaceholder, formattedDate);
					}
				}

				return newContents;
			}
			catch (ArgumentException e)
			{
				throw new Exception("Invalid regular expression: " + e.Message, e);
			}
		}

		internal CultureInfo GetCultureFromFileName()
		{
			if (_localeRegex == null)
				return null;

			var fileName = Path.GetFileName(File);

			try
			{
				var match = _localeRegex.Match(fileName);
				if (match.Success)
				{
					var locale = match.Groups["locale"].Success
						? match.Groups["locale"].Value
						: match.Value;
					return new CultureInfo(locale);
				}
			}
			catch (CultureNotFoundException)
			{
			}
			catch (Exception ex)
			{
				SafeLogError(
					$"Failed to extract locale from filename using pattern '{FileLocalePattern}': {ex.Message}");
			}

			return null;
		}

		private void SafeLogError(string msg)
		{
			try
			{
				Log.LogError(msg);
			}
			catch (Exception)
			{
				//swallow... logging fails in the unit test environment, where the log isn't set up
			}
		}
	}
}
