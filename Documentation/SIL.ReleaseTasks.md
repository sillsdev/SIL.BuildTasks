# SIL.ReleaseTasks package

Tasks in the `SIL.ReleaseTasks` nuget package:

## SetReleaseNotesProperty task

This new feature adds a filter option to the SetReleaseNotesProperty task which allows to filter out any
entries that do not belong to a specific nuget package. To achieve this we add a new property
FilterEntries that controls whether or not the new feature is enabled for a project.

## CreateChangelogEntry task

Given a Changelog file, this task will add an entry to the debian changelog (`debian/changelog`).
The changelog can be a markdown file and can follow the
[Keep a Changelog](https://keepachangelog.com) conventions.

### Properties

- `ChangelogFile`: The name (and path) of the markdown-style changelog file (required)

- `VersionNumber`: The version number to put in the debian changelog (required)

- `PackageName`: The name of the product/package (required)

- `DebianChangelog`: The path and name of the debian changelog file (required)

- `Distribution`: The name of the distribution to put in the debian changelog entry. Defaults to
  `UNRELEASED`

- `Urgency`: The value to put in the urgency field in the debian changelog. Defaults to `low`

- `MaintainerInfo`: Name and e-mail of the maintainer. Defaults to
  `Anonymous <anonymous@example.com>`

### Example

```xml
<UsingTask TaskName="CreateChangelogEntry" AssemblyFile="SIL.ReleaseTasks.dll" />

<Target Name="Test">
  <CreateChangelogEntry ChangelogFile="$(RootDir)/CHANGELOG.md" VersionNumber="3.0.1"
    PackageName="myfavoriteapp" DebianChangelog="$(RootDir)/debian/changelog"
    Distribution="stable" Urgency="high" MaintainerInfo="John Doe &lt;john_doe@example.com&gt;" />
</Target>
```

This uses the markdown file `CHANGELOG.md` and VersionNumber to generate a changelog entry
in the DebianChangeLog file giving the author credit to John Doe.

## CreateReleaseNotesHtml task

Given a markdown-style changelog file, this class will generate a release notes HTML file. The
changelog can be a markdown file and can follow the [Keep a Changelog](https://keepachangelog.com)
conventions.

If the HTML file already exists, the task will look for a section with `class="releasenotes"`
and replace it with the current release notes.

### Properties

- `ChangelogFile`: The name (and path) of the markdown-style changelog file (required)

- `HtmlFile`: The name (and path) of the output HTML file (required)

### Example

```xml
<UsingTask TaskName="CreateReleaseNotesHtml" AssemblyFile="SIL.ReleaseTasks.dll" />

<Target Name="Test">
  <CreateReleaseNotesHtml ChangelogFile="$(RootDir)/CHANGELOG.md"
    HtmlFile="$(OutputDir)/ReleaseNotes.html" />
</Target>
```

This generates a `ReleaseNotes.html` file by creating a new file or by replacing the
`<div class='releasenotes'>` in an existing .htm with a generated one.

## StampChangelogFileWithVersion task

Replaces the first line in a markdown-style Changelog/Release file with the version and date. The
changelog can be a markdown file and can follow the [Keep a Changelog](https://keepachangelog.com)
conventions.

This assumes that a temporary line is currently at the top: e.g.
`## DEV_VERSION_NUMBER: DEV_RELEASE_DATE`

### Properties

- `ChangelogFile`: The name (and path) of the markdown-style changelog file (required)

- `VersionNumber`: The version number to put in the changelog file (required)

- `DateTimeFormat`: The format string used to output the date. Default: `yyyy-MM-dd`

### Example

```xml
<UsingTask TaskName="StampChangelogFileWithVersion" AssemblyFile="SIL.ReleaseTasks.dll" />

<Target Name="Test">
  <StampChangelogFileWithVersion ChangelogFile="$(RootDir)/CHANGELOG.md"
    VersionNumber="1.0.3" DateTimeFormat="dd/MMM/yyyy" />
</Target>
```

This stamps the `CHANGELOG.md` file with the version numbers (replacing the first line with
`'## VERSION_NUMBER DATE'`).

## SetReleaseNotesProperty task

Sets a property to the changes mentioned in the topmost release in a `CHANGELOG.md` file.
This is a markdown file that follows the [Keep a Changelog](https://keepachangelog.com)
conventions.

### Properties

- `ChangelogFile`: The name (and path) of the markdown-style changelog file. Defaults to
  `../CHANGELOG.md`.

- `VersionRegex`: Regular expression to extract the version number from the subheadings in the
  changelog file. Default: `#+ \[([^]]+)\]`

- `AppendToReleaseNotesProperty`: Text that gets added to the end of the property

- `Value` (output parameter): The name of the property that will be set

### Example

```xml
<UsingTask TaskName="SetReleaseNotesProperty" AssemblyFile="SIL.ReleaseTasks.dll" />

<Target Name="Test">
  <PropertyGroup>
    <TextToAdd><![CDATA[
See full changelog at https://github.com/sillsdev/SIL.BuildTasks/blob/master/CHANGELOG.md]]>
    </TextToAdd>
  </PropertyGroup>
  <SetReleaseNotesProperty ChangelogFile="$(RootDir)/CHANGELOG.md"
    AppendToReleaseNotesProperty="$(TextToAdd)">
    <Output TaskParameter="Value" PropertyName="ReleaseNotes" />
  </SetReleaseNotesProperty>
</Target>
```

### Automatically create release notes

By adding the `SIL.ReleaseTasks` nuget package to a project, the `PackageReleaseNotes`
property will be automatically set when creating a nuget package of the .csproj project.
This works with the new .csproj format that comes with VS 2017 and that defines the
nuget package in the .csproj file.

If you don't want to automatically set the `PackageReleaseNotes` property, you can set the
`IgnoreSetReleaseNotesProp` property to `true`.

By default the changelog file is expected in `../CHANGELOG.md`. The name and path can be
changed by setting the `ChangelogFile` property.

[Keep a Changelog](https://keepachangelog.com) doesn't make any recommendations in what form
versions should be put in the changelog. The default for the `SetReleaseNotesProperty` task
follows the example given on the [Keep a Changelog](https://keepachangelog.com) website, which
puts the version number in square brackets, e.g. `## [1.0.0] - 2017-06-20`. However, it's
possible to set the `VersionRegex` property to allow parsing different formats.

__NOTE:__ Due to [msbuild bug #3468](https://github.com/Microsoft/msbuild/issues/3468) the escape
character is @ instead of \\ (backslash)! To insert @ in the regular expression, double it: @@.