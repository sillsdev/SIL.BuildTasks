// Copyright (c) 2018 SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
/*
 * Original code from https://code.google.com/archive/p/snowcode/
 * License: MIT (http://www.opensource.org/licenses/mit-license.php)
 *
 * The code has been modified (mostly in the authentication area)
 * and greatly simplified (extracting only the bits we are using).
 */

using System;
using System.Diagnostics.CodeAnalysis;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace SIL.BuildTasks.AWS
{
	[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
	[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
	public abstract class AwsTaskBase : Task
	{
		/// <summary>
		/// The profile name in the credential profile store
		/// </summary>
		[Required]
		public string CredentialStoreProfileName { get; set; }

		/// <summary>
		/// Get AWS credentials from the credential profile store
		/// </summary>
		/// <returns></returns>
		protected AWSCredentials GetAwsCredentials()
		{
			AWSCredentials awsCredentials;
			if (!new CredentialProfileStoreChain().TryGetAWSCredentials(CredentialStoreProfileName, out awsCredentials))
				throw new ApplicationException("Unable to get AWS credentials from the credential profile store");

			Log.LogMessage(MessageImportance.Normal, "Connecting to AWS using AwsAccessKeyId: {0}",
				awsCredentials.GetCredentials().AccessKey);
			return awsCredentials;

		}

		protected static string Join(string[] values)
		{
			return string.Join(";", values);
		}
	}
}
