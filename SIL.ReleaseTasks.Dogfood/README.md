# SIL.ReleaseTasks.Dogfood

This is basically the same as SIL.ReleaseTasks with the exception that it only contains the
`SetReleaseNotesProperty` task. The project creates a nuget package that then can be included
in `SIL.ReleaseTasks`. This nuget package isn't intended to be used in general - use
`SIL.ReleaseTasks` instead. It won't be uploaded to nuget.org regularly. The only reason it
exists is to be able to use the `SetReleaseNotesProperty` task in `SIL.ReleaseTasks`
because nuget doesn't allow a project to include a nuget package it creates - at least that's
the behaviour with Rider.
