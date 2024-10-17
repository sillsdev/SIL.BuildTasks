// Copyright (c) 2018 SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System.Collections;
using Microsoft.Build.Framework;

namespace SIL.BuildTasks.Tests
{
	public class MockTaskItem : ITaskItem
	{

		public MockTaskItem(string itemSpec)
		{
			ItemSpec = itemSpec;
		}

		public string GetMetadata(string metadataName)
		{
			return "";
		}

		public void SetMetadata(string metadataName, string metadataValue)
		{
		}

		public void RemoveMetadata(string metadataName)
		{
		}

		public void CopyMetadataTo(ITaskItem destinationItem)
		{
		}

		public IDictionary CloneCustomMetadata()
		{
			// ReSharper disable once AssignNullToNotNullAttribute
			return null;
		}

		public string ItemSpec { get; set; }

		// ReSharper disable once AssignNullToNotNullAttribute
		public ICollection MetadataNames => null;

		public int MetadataCount => 0;
	}
}