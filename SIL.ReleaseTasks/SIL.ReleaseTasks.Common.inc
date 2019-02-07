﻿<Project xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
  <!-- This file is shared between SIL.ReleaseTasks.csproj and SIL.ReleaseTasks.Dogfood.csproj -->
  <Target Name="CopyTargetsAndProps" AfterTargets="Build">
    <ItemGroup>
      <PropTargets Include="$(MSBuildThisFileDirectory)/SIL.ReleaseTasks.targets;$(MSBuildThisFileDirectory)/../$(PackageId)/$(PackageId).props" />
    </ItemGroup>
    <ReplaceFileText InputFilename="%(PropTargets.Identity)" OutputFilename="$(OutputPath)/build/$(TargetFramework)/$(PackageId)%(PropTargets.Extension)" MatchExpression="ASSEMBLYDIR" ReplacementText="../../tools/$(TargetFramework)" />
    <ReplaceFileText InputFilename="%(PropTargets.Identity)" OutputFilename="$(OutputPath)/buildMultiTargeting/$(PackageId)%(PropTargets.Extension)" MatchExpression="ASSEMBLYDIR" ReplacementText="../tools/$(TargetFramework)" />
  </Target>
  <!-- Collect all dependencies and include them in the package itself, next to the Task assembly. -->
  <Target Name="CollectRuntimeOutputs" BeforeTargets="_GetPackageFiles">
    <ItemGroup>
      <None Include="$(OutputPath)/**/*.dll;$(OutputPath)/**/*.exe;$(OutputPath)/**/*.dll.config" Exclude="$(OutputPath)/**/$(AssemblyTitle).dll">
        <Pack>true</Pack>
        <PackagePath>$(BuildOutputTargetFolder)</PackagePath>
      </None>
      <None Include="$(OutputPath)/net461/build/**/*.*;$(OutputPath)/netstandard2.0/build/**/*.*">
        <Pack>true</Pack>
        <PackagePath>build/</PackagePath>
      </None>
      <None Include="$(OutputPath)/net461/buildMultiTargeting/*.*">
        <Pack>true</Pack>
        <PackagePath>buildMultiTargeting/</PackagePath>
      </None>
    </ItemGroup>
  </Target>
  <UsingTask TaskName="ReplaceFileText" TaskFactory="CodeTaskFactory" AssemblyFile="$(MSBuildToolsPath)\Microsoft.Build.Tasks.v4.0.dll">
    <ParameterGroup>
      <InputFilename ParameterType="System.String" Required="true" />
      <OutputFilename ParameterType="System.String" Required="true" />
      <MatchExpression ParameterType="System.String" Required="true" />
      <ReplacementText ParameterType="System.String" Required="true" />
    </ParameterGroup>
    <Task>
      <Reference Include="System.Core" />
      <Using Namespace="System" />
      <Using Namespace="System.IO" />
      <Using Namespace="System.Text.RegularExpressions" />
      <Code Type="Fragment" Language="cs"><![CDATA[
        Directory.CreateDirectory(Path.GetDirectoryName(OutputFilename));
        File.WriteAllText(
          OutputFilename,
          Regex.Replace(File.ReadAllText(InputFilename), MatchExpression, ReplacementText)
          );
      ]]></Code>
    </Task>
  </UsingTask>
</Project>