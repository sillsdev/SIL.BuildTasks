#!groovy
// Copyright (c) 2018-2020 SIL International
// This software is licensed under the MIT license (http://opensource.org/licenses/MIT)

@Library('lsdev-pipeline-library') _

xplatformBuildAndRunTests {
	winNodeSpec = 'windows && supported && vs2019 && netcore3.1'
	linuxNodeSpec = 'linux64 && !packager && ubuntu && mono6'
	winTool = 'msbuild16'
	linuxTool = 'mono-msbuild15'
	configuration = 'Release'
	uploadNuGet = true
	restorePackages = true
	clean = true
}
