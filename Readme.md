# Readme

Several useful msbuild tasks.

## Current Tasks

### [`SIL.ReleaseTasks` package](Documentation/SIL.ReleaseTasks.md)

**Task**                      | **Description**
------------------------------|----------------------------------------------------------------
[CreateChangelogEntry](Documentation/SIL.ReleaseTasks.md#createchangelogentry-task) | Given a Changelog file, this task will add an entry to the debian changelog.
[CreateReleaseNotesHtml](Documentation/SIL.ReleaseTasks.md#createreleasenoteshtml-task) | Given a markdown-style changelog file, this class will generate a release notes HTML file.
[StampChangelogFileWithVersion](Documentation/SIL.ReleaseTasks.md#stampchangelogfilewithversion-task) | Replaces the first line in a markdown-style Changelog/Release file with the version and date.

### [`SIL.BuildTasks` package](Documentation/SIL.BuildTasks.md)

**Task**                      | **Description**
------------------------------|----------------------------------------------------------------
[Archive](Documentation/SIL.BuildTasks.md#archive-task) |
[CpuArchitecture](Documentation/SIL.BuildTasks.md#cpuarchitecture-task) | Return the CPU architecture of the current system.
[DownloadFile](Documentation/SIL.BuildTasks.md#downloadfile-task) | Download a file from a web address.
[FileUpdate](Documentation/SIL.BuildTasks.md#fileupdate-task) |
[MakePot](Documentation/SIL.BuildTasks.md#makepot-task) |
[MakeWixForDirTree](Documentation/SIL.BuildTasks.md#makewixfordirtree-task) |
[NUnit](Documentation/SIL.BuildTasks.md#nunit-task) | Run NUnit (v2) on a test assembly.
[NUnit3](Documentation/SIL.BuildTasks.md#nunit3-task) | Run NUnit3 on a test assembly.
[Split](Documentation/SIL.BuildTasks.md#split-task) |
[StampAssemblies](Documentation/SIL.BuildTasks.md#stampassemblies-task) |
[UnixName](Documentation/SIL.BuildTasks.md#unixname-task) | Determine the Unix Name of the operating system executing the build.
[UpdateBuildTypeFile](Documentation/SIL.BuildTasks.md#updatebuildtypefile-task) |
