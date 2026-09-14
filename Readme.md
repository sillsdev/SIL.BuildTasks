# Readme

Several useful msbuild tasks.

[![NuGet version (SIL.BuildTasks)](https://img.shields.io/nuget/v/SIL.BuildTasks.svg?style=flat-square)](https://www.nuget.org/packages/SIL.BuildTasks/)
[![Build, Test and Pack](https://github.com/sillsdev/SIL.BuildTasks/actions/workflows/CI-CD.yml/badge.svg)](https://github.com/sillsdev/SIL.BuildTasks/actions/workflows/CI-CD.yml)

Upgrading from version 1 to version 2? See the [migration guide](Documentation/Migration.md).

## Current Tasks

### [`SIL.ReleaseTasks` package](Documentation/SIL.ReleaseTasks.md)

**Task**                      | **Description**
------------------------------|----------------------------------------------------------------
[CreateChangelogEntry](Documentation/SIL.ReleaseTasks.md#createchangelogentry-task) | Given a Changelog file, this task will add an entry to the debian changelog.
[CreateReleaseNotesHtml](Documentation/SIL.ReleaseTasks.md#createreleasenoteshtml-task) | Given a markdown-style changelog file, this class will generate a release notes HTML file.
[StampChangelogFileWithVersion](Documentation/SIL.ReleaseTasks.md#stampchangelogfilewithversion-task) | Replaces the first line in a markdown-style Changelog/Release file with the version and date.
[SetReleaseNotesProperty](Documentation/SIL.ReleaseTasks.md#setreleasenotesproperty-task) | Given a markdown-style changelog file, this class will set a property to the changes mentioned in the topmost release.

### [`SIL.BuildTasks` package](Documentation/SIL.BuildTasks.md)

**Task**                      | **Description**
------------------------------|----------------------------------------------------------------
[Archive](Documentation/SIL.BuildTasks.md#archive-task) | Create a tar archive from a set of files.
[CpuArchitecture](Documentation/SIL.BuildTasks.md#cpuarchitecture-task) | Return the CPU architecture of the current system.
[DownloadFile](Documentation/SIL.BuildTasks.md#downloadfile-task) | Download a file from a web address.
[FileUpdate](Documentation/SIL.BuildTasks.md#fileupdate-task) | Replace text in a file by regular expression, optionally substituting the current date.
[MakePot](Documentation/SIL.BuildTasks.md#makepot-task) | Generate a gettext POT template from strings in C# and XML files.
[MakeWixForDirTree](Documentation/SIL.BuildTasks.md#makewixfordirtree-task) | Generate a WiX fragment describing the files in a directory tree.
[NormalizeLocales](Documentation/SIL.BuildTasks.md#normalizelocales-task) | Drops country code from directories and filenames to help work with Crowdin
[NUnit](Documentation/SIL.BuildTasks.md#nunit-task) | Run NUnit (v2) on a test assembly.
[NUnit3](Documentation/SIL.BuildTasks.md#nunit3-task) | Run NUnit3 on a test assembly.
[Split](Documentation/SIL.BuildTasks.md#split-task) | Split a string on a delimiter and return one of the parts.
[StampAssemblies](Documentation/SIL.BuildTasks.md#stampassemblies-task) | Write version numbers into AssemblyInfo source files.
[UnixName](Documentation/SIL.BuildTasks.md#unixname-task) | Determine the Unix Name of the operating system executing the build.
[UpdateBuildTypeFile](Documentation/SIL.BuildTasks.md#updatebuildtypefile-task) | Set the build type in a source file's version-type enum.

### [`SIL.BuildTasks.AWS` package](Documentation/SIL.BuildTasks.AWS.md)

**Task**                      | **Description**
------------------------------|----------------------------------------------------------------
[S3BuildPublisher](Documentation/SIL.BuildTasks.AWS.md#s3buildpublisher-task) | Publish a set of files to an S3 bucket.

## Build

### Windows or Linux

Install the latest .NET SDK from https://dotnet.microsoft.com/download .

Build and run tests:

```bash
dotnet pack --configuration Release SIL.ReleaseTasks.Dogfood/SIL.ReleaseTasks.Dogfood.csproj
dotnet test
```

### Building a local package of SIL.BuildTasks for testing (Windows)

1. Run the Pack build command from inside Visual Studio on the SIL.ReleaseTasks.Dogfood project
2. Run the Pack build command from inside Visual Studio on the SIL.BuildTasks project
3. Install the package from the output folder into your local Nuget source e.g.`nuget.exe add output/Debug/SIL.BuildTasks.3.3.0.nupkg -Source /c/Repositories/DevelopmentPackages/`