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
using System.Collections.Generic;
using System.IO;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;

namespace SIL.BuildTasks.AWS.S3
{
	/// <summary>
	/// Helper class to connect to Amazon aws S3 and store files.
	/// </summary>
	public sealed class S3Helper : IDisposable
	{
		private bool _disposed;

		#region Constructors
		public S3Helper(AWSCredentials credentials)
		{
			var config = new AmazonS3Config
			{
				ForcePathStyle = true,
				RegionEndpoint = Amazon.RegionEndpoint.USEast1 // todo: this won't work for all clients!
			};
			Client = new AmazonS3Client(credentials, config);
		}

		~S3Helper()
		{
			Dispose(false);
		}

		#endregion

		#region Properties

		private IAmazonS3 Client
		{
			get;
		}

		#endregion

		#region Public methods

		/// <summary>
		/// Publish a file to a S3 bucket, in the folder specified, optionally making it publically readable.
		/// </summary>
		public void Publish(IEnumerable<string> files, string bucketName, string folder, bool isPublicRead, string contentType = null, string contentEncoding = null)
		{
			var destinationFolder = GetDestinationFolder(folder);

			StoreFiles(files, bucketName, destinationFolder, isPublicRead, contentType, contentEncoding);
		}

		/// <summary>
		/// Publish a directory to a S3 bucket, in the folder specified, optionally making it publically readable.
		/// </summary>
		public void PublishDirectory(string sourceDirectory, string bucketName, string destinationFolder, bool isPublicRead)
		{
			destinationFolder = GetDestinationFolder(destinationFolder);

			var directoryTransferUtility = new TransferUtility(Client);
			directoryTransferUtility.UploadDirectory(new TransferUtilityUploadDirectoryRequest
			{
				Directory = sourceDirectory,
				BucketName = bucketName,
				SearchOption = SearchOption.AllDirectories,
				KeyPrefix = destinationFolder,
				CannedACL = isPublicRead ? S3CannedACL.PublicRead : S3CannedACL.Private
			});
		}

		#endregion

		#region Private Methods

		private void StoreFiles(IEnumerable<string> files, string bucketName, string destinationFolder, bool isPublicRead,
			string contentType = null, string contentEncoding = null)
		{
			foreach (var file in files)
			{
				// Use the filename as the key (aws filename).
				var key = Path.GetFileName(file);
				StoreFile(file, destinationFolder + key, bucketName, isPublicRead, contentType, contentEncoding);
			}
		}

		private static string GetDestinationFolder(string folder)
		{
			var destinationFolder = folder ?? string.Empty;

			// Append a folder seperator if a folder has been specified without one.
			if (!string.IsNullOrEmpty(destinationFolder) && !destinationFolder.EndsWith("/"))
			{
				destinationFolder += "/";
			}

			return destinationFolder;
		}

		private void StoreFile(string file, string key, string bucketName, bool isPublicRead, string contentType = null, string contentEncoding = null)
		{
			var acl = isPublicRead ? S3CannedACL.PublicRead : S3CannedACL.Private;

			var request = new PutObjectRequest { CannedACL = acl, FilePath = file, BucketName = bucketName, Key = key };
			if (contentType != null) // probably harmless to just set to null, but feels safer not to set at all if not specified.
				request.ContentType = contentType;
			if (contentEncoding != null)
				request.Headers.ContentEncoding = contentEncoding;

			Client.PutObject(request);
		}

		#endregion

		#region Implementation of IDisposable

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			if (!disposing)
				return;

			try
			{
				Client?.Dispose();
			}
			finally
			{
				_disposed = true;
			}
		}

		#endregion

	}
}
