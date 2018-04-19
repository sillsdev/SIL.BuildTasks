# SIL.BuildTasks package

Tasks in the `SIL.BuildTasks` nuget package:

## Archive task

### Properties

### Example

## CpuArchitecture task

Returns the CPU architecture of the current system (_x64/x86_ on Windows, _X86_64/i386_ on Linux).

### Properties

- `Value` (output parameter)

### Example

``` xml
<UsingTask TaskName="CpuArchitecture" AssemblyFile="SIL.BuildTasks.dll" />

<Target Name="Test">
	<CpuArchitecture>
		<Output TaskParameter="Value" PropertyName="arch" />
	</CpuArchitecture>
</Target>
```

## DownloadFile task

Downloads a file from a web address. Params specify the web address, the local path for the file,
and optionally a user and password. The user/password feature has not been tested.
If using an important password, make sure the address is https, since I think otherwise the password
may be sent in clear.

### Properties

- `Address`: HTTP address to download from (required)

- `LocalFilename`: Local file to which the downloaded file will be saved (required)

- `Username`: Username credential for HTTP authentication

- `Password`: Password credential for HTTP authentication

### Example

``` xml
<UsingTask TaskName="DownloadFile" AssemblyFile="SIL.BuildTasks.dll" />

<Target Name="Test">
	<DownloadFile
		Address="http://stackoverflow.com/questions/1089452/how-can-i-use-msbuild-to-download-a-file"
		LocalFilename="answer.html" />
</Target>
```

## FileUpdate task

### Properties

### Example

## MakePot task

### Properties

### Example

## MakeWixForDirTree task

### Properties

### Example

## NUnit task

Runs NUnit (v2) on a test assembly.

### Properties

- `Timeout`: The maximum amount of time the test is allowed to execute, expressed in milliseconds.
  The default is essentially no time-out

- `FudgeFactor`: Factor the timeout will be multiplied by

- `Verbose`: If _true_ print the output of NUnit immediately, otherwise print it after NUnit finishes

- `FailedSuites`: The names of failed test suites (output parameter)

- `AbandondedSuites`: The names of test suites that got a timeout or that crashed (output parameter)

- `Assemblies`: The full path to the NUnit assemblies (test DLLs). [Required]

- `IncludeCategory`: The categories to include. Multiple values are separated by a comma (`,`)

- `ExcludeCategory`: The categories to exclude. Multiple values are separated by a comma (`,`)

- `Fixture`: The test fixture

- `XsltTransformFile`: The XSLT transform file

- `OutputXmlFile`: The output XML file

- `ErrorOutputFile`: The file to receive test error details

- `WorkingDirectory`: The working directory

- `DisableShadowCopy`: Determines whether assemblies are copied to a shadow folder during testing

- `ProjectConfiguration`: The project configuration to run

- `FailTaskIfAnyTestsFail`: Whether or not to fail the build if any tests fail

- `TestInNewThread`: Allows tests to be run in a new thread, allowing you to take advantage of
  ApartmentState and ThreadPriority settings in the config file

- `Force32Bit`: Determines whether the tests are run in a 32bit process on a 64bit OS

- `Framework`: Determines the framework to run against

- `ToolPath`: Gets or sets the path to the NUnit executable assembly

- `Apartment`: Apartment for running tests: MTA (Default), STA

### Example

``` xml
<UsingTask TaskName="NUnit" AssemblyFile="SIL.BuildTasks.dll" />

<Target Name="Test">
	<ItemGroup>
		<TestAssemblies Include="$(OutputDir)/*.Tests.dll"/>
	</ItemGroup>

	<NUnit Assemblies="@(TestAssemblies)"
		ToolPath="$(NuGetPackageDir)/nunit.runners.net4/2.6.4/tools"
		TestInNewThread="false"
		ExcludeCategory="KnownMonoIssue"
		WorkingDirectory="$(OutputDir)"
		Force32Bit="true"
		Verbose="true"
		FailTaskIfAnyTestsFail="true"
		OutputXmlFile="$(OutputDir)/TestResults.xml"/>
</Target>
```

## NUnit3 task

Runs NUnit3 on a test assembly.

### Properties

See properties for NUnit task. The following additional properties are defined:

- `UseNUnit3Xml`: Whether to use the NUnit3 or NUnit2 XML format

- `NoColor`: Determines the use of colors in the output

- `TeamCity`: Should be set to true if the tests are running on a TeamCity server.
  Adds `--teamcity` when calling nunit which _"Turns on use of TeamCity service messages."_

### Example

See NUnit task.

## Split task

### Properties

### Example

## StampAssemblies task

### Properties

### Example

## UnixName task

Determines the Unix Name of the operating system executing the build.

This is useful when determining Mac vs Linux during a build. On Mac, the output Value will be "Darwin".
On Linux, the output Value will be "Linux".

### Properties

- `Value` (output parameter)

### Example

This can be used to set DefineConstants during the PreBuild Target.
Here is an example `build/platform.targets` file that can be included
in a CSPROJ file. `SYSTEM_MAC` or `SYSTEM_LINUX` will be defined and
can be used in the C# code for #if conditional compilation.

``` xml
<Project xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
  <UsingTask TaskName="UnixName" AssemblyFile="SIL.BuildTasks.dll" />
  <Target Name="BeforeBuild">
    <UnixName>
      <Output TaskParameter="Value" PropertyName="UNIX_NAME" />
    </UnixName>
    <PropertyGroup>
      <DefineConstants Condition="'$(OS)' == 'Unix'">$(DefineConstants);SYSTEM_UNIX</DefineConstants>
      <DefineConstants Condition="'$(UNIX_NAME)' == 'Darwin'">$(DefineConstants);SYSTEM_MAC</DefineConstants>
      <DefineConstants Condition="'$(UNIX_NAME)' == 'Linux'">$(DefineConstants);SYSTEM_LINUX</DefineConstants>
    </PropertyGroup>
  </Target>
</Project>
```

## UpdateBuildTypeFile task

### Properties

### Example
