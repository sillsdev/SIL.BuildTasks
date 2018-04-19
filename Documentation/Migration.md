# Migration

## Upgrade to version 2

Version 2 removes the `GenerateReleaseArtifacts` task from `SIL.BuildTasks` and instead splits its up
in three new tasks in `SIL.ReleaseTasks`: `CreateChangelogEntry`, `CreateReleaseNotesHtml`,
and `StampChangelogFileWithVersion`.

### First example

Version 1:

``` xml
<GenerateReleaseArtifacts MarkdownFile="$(RootDir)\src\Installer\ReleaseNotes.md" StampMarkdown="false"
  VersionNumber="4.1" ProductName="bloom" Stability="stable" Urgency="medium"
  DebianChangeLog="$(RootDir)\debian\changelog"
  ChangeLogAuthorInfo="Stephen McConnel &lt;stephen_mcconnel@sil.org&gt;" />
```

Version 2:

``` xml
<CreateChangelogEntry ChangelogFile="$(RootDir)\src\Installer\ReleaseNotes.md" VersionNumber="4.1"
  PackageName="bloom" DebianChangelog="$(RootDir)\debian\changelog"
  Distribution="stable" Urgency="medium"
  MaintainerInfo="Stephen McConnel &lt;stephen_mcconnel@sil.org&gt;" />
```

### Second example

Version 1:

``` xml
<GenerateReleaseArtifacts MarkdownFile="$(RootDir)\src\Installer\ReleaseNotes.md" StampMarkdown="true"
  HtmlFile="$(RootDir)\src\Installer\$(UploadFolder).htm" VersionNumber="$(Version)"
  ProductName="flexbridge" DebianChangeLog="$(RootDir)\debian\changelog"
  ChangeLogAuthorInfo="Jason Naylor &lt;jason_naylor@sil.org&gt;" />
```

Version 2:

``` xml
<StampChangelogFileWithVersion ChangelogFile="$(RootDir)\src\Installer\ReleaseNotes.md"
  VersionNumber="$(Version)" />
<CreateReleaseNotesHtml ChangelogFile="$(RootDir)\src\Installer\ReleaseNotes.md"
  HtmlFile="$(RootDir)\src\Installer\$(UploadFolder).htm" />
<CreateChangelogEntry ChangelogFile="$(RootDir)\src\Installer\ReleaseNotes.md"
  VersionNumber="$(Version)" PackageName="flexbridge" DebianChangelog="$(RootDir)\debian\changelog"
  MaintainerInfo="Jason Naylor &lt;jason_naylor@sil.org&gt;" />
```