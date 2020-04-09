using System;
using System.Collections;
using Microsoft.Build.Framework;

namespace SIL.BuildTasks.Tests
{
	class MockBuildEngine : IBuildEngine
	{
		public void LogErrorEvent(BuildErrorEventArgs e)
		{
		}

		public void LogWarningEvent(BuildWarningEventArgs e)
		{
		}

		public void LogMessageEvent(BuildMessageEventArgs e)
		{
		}

		public void LogCustomEvent(CustomBuildEventArgs e)
		{
		}

		public bool BuildProjectFile(string projectFileName, string[] targetNames, IDictionary globalProperties,
			IDictionary targetOutputs)
		{
			throw new NotImplementedException();
		}

		public bool ContinueOnError => false;
		public int LineNumberOfTaskNode => 0;
		public int ColumnNumberOfTaskNode => 0;
		public string ProjectFileOfTaskNode => throw new NotImplementedException();
	}
}
