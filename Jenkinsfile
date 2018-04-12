#!groovy
// Copyright (c) 2018 SIL International
// This software is licensed under the MIT license (http://opensource.org/licenses/MIT)

@Library('lsdev-pipeline-library') _

xplatformBuildAndRunTests {
	winNodeSpec = 'windows && supported && vs2017'
	winTool = 'msbuild15'
	linuxNodeSpec = 'linux64 && !packager && ubuntu && mono5'
	linuxTool = 'mono-msbuild15'
	configuration = 'Release'
	uploadNuGet = false
}
