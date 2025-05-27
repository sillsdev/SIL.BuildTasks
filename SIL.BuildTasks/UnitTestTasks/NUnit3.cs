// Copyright (c) 2016-2025 SIL Global
// This software is licensed under the MIT license (http://opensource.org/licenses/MIT)

using System;
using System.Diagnostics;
using System.Text;
using JetBrains.Annotations;

namespace SIL.BuildTasks.UnitTestTasks
{
	/// <inheritdoc />
	/// <summary>
	/// Run NUnit3 on a test assembly.
	/// </summary>
	[PublicAPI]
	public class NUnit3 : NUnit
	{
		private bool? _useNUnit3Xml;
		private bool _teamCity;

		public bool UseNUnit3Xml
		{
			get => _useNUnit3Xml.HasValue && _useNUnit3Xml.Value;
			set => _useNUnit3Xml = value;
		}

		public bool NoColor { get; set; }

		/// <summary>
		/// Should be set to true if the tests are running on a TeamCity server.
		/// Adds --teamcity which "Turns on use of TeamCity service messages."
		/// </summary>
		public bool TeamCity
		{
			get => _teamCity;
			set
			{
				_teamCity = value;

				// According to Eberhard, we don't want this behavior by default on
				// Jenkins, so this is tied to the TeamCity property.
				// REVIEW: This should probably be true for NUnit also, but changing
				// the logic there could potentially cause unexpected results for existing
				// callers whereas the NUnit3 task is new enough, I think we are okay.
				// (And there is no TeamCity property for NUnit, anyway.)
				if (!_failTaskIfAnyTestsFail.HasValue)
					FailTaskIfAnyTestsFail = value;
			}
		}

		public string Test { get; set; }

		public string Trace { get; set; }

		public int Agents { get; set; }

		public int Workers { get; set; }

		public bool DisposeRunners { get; set; }

		/// <summary>
		/// When set to true it will offer the opportunity to attach a debugger before starting the unit tests
		/// </summary>
		public bool Debug { get; set; }

		/// <summary>
		/// PROCESS isolation for test assemblies. Values: Single, Separate, Multiple.
		/// If not specified, defaults to Separate for a single assembly or Multiple for more than one.
		/// By default, processes are run in parallel
		/// </summary>
		public string Process { get; set; }

		/// <inheritdoc />
		/// <summary>
		/// Gets the name (without path) of the NUnit executable. When running on Mono this is
		/// different from ProgramNameAndPath() which returns the executable we'll start.
		/// </summary>
		protected override string RealProgramName => "nunit3-console.exe";

		protected override string AddAdditionalProgramArguments()
		{
			var bldr = new StringBuilder();
			//bldr.Append(" --noheader");
			// We don't support TestInNewThread for now
			if (!string.IsNullOrEmpty(OutputXmlFile))
			{
				bldr.AppendFormat(" \"--result:{0};format={1}\"",
					new Uri(OutputXmlFile).LocalPath,
					UseNUnit3Xml ? "nunit3" : "nunit2");
			}

			bldr.Append(GetLabelsOption());
			if (NoColor)
				bldr.Append(" --nocolor");
			if (Force32Bit)
				bldr.Append(" --x86");
			if (TeamCity)
				bldr.Append(" --teamcity");
			if (!string.IsNullOrEmpty(Trace))
				bldr.AppendFormat(" --trace={0}", Trace);
			if (!string.IsNullOrEmpty(Test))
				bldr.AppendFormat(" --test={0}", Test);
			if (DisposeRunners)
				bldr.Append(" --dispose-runners");
			if (Debug)
				bldr.Append(" --debug");
			if (!string.IsNullOrEmpty(Process))
				bldr.AppendFormat(" --process={0}", Process);
			if (Workers > 0)
				bldr.AppendFormat(" --workers={0}", Workers);
			if (Agents > 0)
				bldr.AppendFormat(" --agents={0}", Agents);
			return bldr.ToString();
		}

		private string GetLabelsOption()
		{
			var version = ConsoleRunnerVersion;
			if (version.ProductMajorPart > 3 ||
				version.ProductMajorPart == 3 && version.ProductMinorPart >= 8)
			{
				// Starting with NUnit.ConsoleRunner 3.8 there's a --labels=Before option that
				// does the same as --labels=All. However, the latter is deprecated in later
				// versions so we use the new syntax.
				return " --labels=Before";
			}
			return " --labels=All";
		}

		internal override string AddIncludeAndExcludeArguments()
		{
			var bldr = new StringBuilder();
			string include = null;
			string exclude = null;

			if (!string.IsNullOrWhiteSpace(IncludeCategory))
				include = BuildCategoriesString(IncludeCategory, "=", " or ");

			if (!string.IsNullOrWhiteSpace(ExcludeCategory))
				exclude = BuildCategoriesString(ExcludeCategory, "!=", " and ");

			if (include == null && exclude == null)
				return string.Empty;

			bldr.Append(" --where \"");
			if (include != null && exclude != null)
				bldr.Append(include).Append(" and ").Append(exclude);
			else
				bldr.Append(include).Append(exclude);
			bldr.Append("\"");

			return bldr.ToString();
		}

		private static string BuildCategoriesString(string categoryString, string condition, string joiner)
		{
			var bldr = new StringBuilder();

			foreach (var cat in categoryString.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries))
				bldr.Append("cat" + condition + cat + joiner);

			bldr.Length = bldr.Length - joiner.Length; // remove final "or"
			bldr.Insert(0, "(").Append(")");
			return bldr.ToString();
		}

		private FileVersionInfo ConsoleRunnerVersion => FileVersionInfo.GetVersionInfo(RealProgramNameAndPath);
	}
}
