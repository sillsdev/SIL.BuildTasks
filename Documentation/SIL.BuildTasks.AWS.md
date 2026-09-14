# SIL.BuildTasks.AWS package

**SIL.BuildTasks.AWS** provides an msbuild task to publish files to an Amazon S3 bucket.

Tasks in the `SIL.BuildTasks.AWS` nuget package:

## S3BuildPublisher task

Uploads files to an S3 bucket, either a list of individual files or the contents of a folder.
Credentials come from a named profile in the AWS credential profile store, not from task
properties.

Files made public are served from `https://s3.amazonaws.com/<bucket>/<folder>/<file>`.

### Properties

- `CredentialStoreProfileName`: The profile name in the AWS credential profile store (required)

- `DestinationBucket`: The S3 bucket to upload to (required)

- `SourceFiles`: The files to upload. Subfolders are not supported. Provide either this or
  `SourceFolder`, not both

- `SourceFolder`: The folder whose contents to upload. Provide either this or `SourceFiles`,
  not both

- `DestinationFolder`: The folder within the bucket. Must not begin with `/` or `\`, or end
  with `\`

- `IsPublicRead`: Whether the uploaded files are publicly readable. Defaults to `false`

- `ContentType`: The content type to record in the S3 metadata. Only applies to `SourceFiles`

- `ContentEncoding`: The content encoding to record in the S3 metadata, e.g. `gzip`. Only
  applies to `SourceFiles`

### Example

```xml
<UsingTask TaskName="S3BuildPublisher" AssemblyFile="SIL.BuildTasks.AWS.dll" />

<Target Name="Publish">
  <S3BuildPublisher CredentialStoreProfileName="my-profile" DestinationBucket="my-bucket"
    SourceFolder="$(RootDir)/output/release" DestinationFolder="downloads/latest"
    IsPublicRead="true" />
</Target>
```

This uploads everything in `output/release` to `my-bucket/downloads/latest`, publicly readable.
