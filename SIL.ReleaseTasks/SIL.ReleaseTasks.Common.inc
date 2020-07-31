<Project xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
  <!-- This file is shared between SIL.ReleaseTasks.csproj and SIL.ReleaseTasks.Dogfood.csproj -->
  <Target Name="CopyTargetsAndProps" AfterTargets="Build">
    <Copy SourceFiles="$(MSBuildThisFileDirectory)/../$(PackageId)/$(PackageId).props" DestinationFiles="$(OutputPath)/build/$(PackageId).props" />
    <Copy SourceFiles="$(MSBuildThisFileDirectory)/SIL.ReleaseTasks.targets" DestinationFiles="$(OutputPath)/build/$(PackageId).targets" />
    <Copy SourceFiles="$(MSBuildThisFileDirectory)/../$(PackageId)/$(PackageId).props" DestinationFiles="$(OutputPath)/buildMultiTargeting/$(PackageId).props" />
    <Copy SourceFiles="$(MSBuildThisFileDirectory)/SIL.ReleaseTasks.targets" DestinationFiles="$(OutputPath)/buildMultiTargeting/$(PackageId).targets" />
    <MakeDir Directories="$(OutputPath)/lib/net461" />
    <MakeDir Directories="$(OutputPath)/lib/netstandard2.0" />
    <!-- Create empty files -->
    <WriteLinesToFile File="$(OutputPath)/lib/net461/_._" />
    <WriteLinesToFile File="$(OutputPath)/lib/netstandard2.0/_._" />
  </Target>

  <!-- Collect all dependencies and include them in the package itself, next to the Task assembly. -->
  <Target Name="CollectRuntimeOutputs" BeforeTargets="_GetPackageFiles">
    <ItemGroup>
      <None Include="$(OutputPath)/**/*.dll;$(OutputPath)/**/*.exe;$(OutputPath)/**/*.dll.config" Exclude="$(OutputPath)/**/$(AssemblyTitle).dll">
        <Pack>true</Pack>
        <PackagePath>$(BuildOutputTargetFolder)</PackagePath>
      </None>
      <!-- we use *.props and *.targets files from net461 - they are identical to the ones in standard2.0 -->
      <None Include="$(OutputPath)/net461/build/*.*">
        <Pack>true</Pack>
        <PackagePath>build/</PackagePath>
      </None>
      <None Include="$(OutputPath)/net461/buildMultiTargeting/*.*">
        <Pack>true</Pack>
        <PackagePath>buildMultiTargeting/</PackagePath>
      </None>
      <None Include="$(OutputPath)/lib/**/_._">
        <Pack>true</Pack>
        <PackagePath>lib/</PackagePath>
      </None>
    </ItemGroup>
  </Target>
</Project>