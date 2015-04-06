﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using Microsoft.Framework.FunctionalTestUtils;
using Microsoft.Framework.PackageManager;
using Microsoft.Framework.Runtime;
using Xunit;

namespace Bootstrapper.FunctionalTests
{
    public class BootstrapperTests
    {
        public static IEnumerable<object[]> RuntimeComponents
        {
            get
            {
                return TestUtils.GetRuntimeComponentsCombinations();
            }
        }

        public static IEnumerable<object[]> ClrRuntimeComponents
        {
            get
            {
                return TestUtils.GetClrRuntimeComponents();
            }
        }

        public static IEnumerable<object[]> CoreClrRuntimeComponents
        {
            get
            {
                return TestUtils.GetCoreClrRuntimeComponents();
            }
        }

        [Theory]
        [MemberData("RuntimeComponents")]
        public void BootstrapperReturnsNonZeroExitCodeWhenNoArgumentWasGiven(string flavor, string os, string architecture)
        {
            using (var runtimeHomeDir = TestUtils.GetRuntimeHomeDir(flavor, os, architecture))
            {
                string stdOut, stdErr;
                var exitCode = BootstrapperTestUtils.ExecBootstrapper(
                    runtimeHomeDir,
                    arguments: string.Empty,
                    stdOut: out stdOut,
                    stdErr: out stdErr);

                Assert.NotEqual(0, exitCode);
            }
        }

        [Theory]
        [MemberData("RuntimeComponents")]
        public void BootstrapperReturnsZeroExitCodeWhenHelpOptionWasGiven(string flavor, string os, string architecture)
        {
            using (var runtimeHomeDir = TestUtils.GetRuntimeHomeDir(flavor, os, architecture))
            {
                string stdOut, stdErr;
                var exitCode = BootstrapperTestUtils.ExecBootstrapper(
                    runtimeHomeDir,
                    arguments: "--help",
                    stdOut: out stdOut,
                    stdErr: out stdErr);

                Assert.Equal(0, exitCode);
            }
        }

        [Theory]
        [MemberData("RuntimeComponents")]
        public void BootstrapperShowsVersionAndReturnsZeroExitCodeWhenVersionOptionWasGiven(string flavor, string os, string architecture)
        {
            using (var runtimeHomeDir = TestUtils.GetRuntimeHomeDir(flavor, os, architecture))
            {
                string stdOut, stdErr;
                var exitCode = BootstrapperTestUtils.ExecBootstrapper(
                    runtimeHomeDir,
                    arguments: "--version",
                    stdOut: out stdOut,
                    stdErr: out stdErr);

                Assert.Equal(0, exitCode);
                Assert.Contains(TestUtils.GetRuntimeVersion(), stdOut);
            }
        }

        [Theory]
        [MemberData("RuntimeComponents")]
        public void BootstrapperInvokesApplicationHostWithInferredAppBase_ProjectDirAsArgument(string flavor, string os, string architecture)
        {
            using (var runtimeHomeDir = TestUtils.GetRuntimeHomeDir(flavor, os, architecture))
            {
                var sampleAppRoot = Path.Combine(TestUtils.GetSamplesFolder(), "HelloWorld");
                string stdOut, stdErr;
                var exitCode = BootstrapperTestUtils.ExecBootstrapper(
                    runtimeHomeDir,
                    arguments: string.Format("{0} run", sampleAppRoot),
                    stdOut: out stdOut,
                    stdErr: out stdErr,
                    environment: new Dictionary<string, string> { { EnvironmentNames.Trace, null } });

                Assert.Equal(0, exitCode);
                Assert.Equal(@"Hello World!
Hello, code!
I
can
customize
the
default
command
", stdOut);
            }
        }

        [Theory]
        [MemberData("RuntimeComponents")]
        public void BootstrapperInvokesApplicationHostWithInferredAppBase_ProjectFileAsArgument(string flavor, string os, string architecture)
        {
            using (var runtimeHomeDir = TestUtils.GetRuntimeHomeDir(flavor, os, architecture))
            {
                var sampleAppProjectFile = Path.Combine(TestUtils.GetSamplesFolder(), "HelloWorld", Project.ProjectFileName);
                string stdOut, stdErr;
                var exitCode = BootstrapperTestUtils.ExecBootstrapper(
                    runtimeHomeDir,
                    arguments: string.Format("{0} run", sampleAppProjectFile),
                    stdOut: out stdOut,
                    stdErr: out stdErr,
                    environment: new Dictionary<string, string> { { EnvironmentNames.Trace, null } });

                Assert.Equal(0, exitCode);
                Assert.Equal(@"Hello World!
Hello, code!
I
can
customize
the
default
command
", stdOut);
            }
        }

        [Theory]
        [MemberData("RuntimeComponents")]
        public void BootstrapperInvokesAssemblyWithInferredAppBaseAndLibPathOnClr(string flavor, string os, string architecture)
        {
            var outputFolder = flavor == "coreclr" ? "dnxcore50" : "dnx451";

            using (var runtimeHomeDir = TestUtils.GetRuntimeHomeDir(flavor, os, architecture))
            using (var tempDir = TestUtils.CreateTempDir())
            {
                var samplesPath = TestUtils.GetSamplesFolder();
                var sampleAppRoot = Path.Combine(samplesPath, "HelloWorld");

                string stdOut, stdError;
                var exitCode = DnuTestUtils.ExecDnu(
                    runtimeHomeDir,
                    subcommand: "build",
                    arguments: string.Format("{0} --configuration=Release --out {1}", sampleAppRoot, tempDir.DirPath),
                    stdOut: out stdOut,
                    stdError: out stdError);

                Assert.Empty(stdError);
                Assert.Equal(0, exitCode);

                exitCode = BootstrapperTestUtils.ExecBootstrapper(
                    runtimeHomeDir,
                    arguments: Path.Combine(tempDir, "Release", outputFolder, "HelloWorld.dll"),
                    stdOut: out stdOut,
                    stdErr: out stdError,
                    environment: new Dictionary<string, string> { { EnvironmentNames.Trace, null } });

                Assert.Equal(0, exitCode);
                Assert.Equal(@"Hello World!
Hello, code!
", stdOut);
            }
        }
    }
}
