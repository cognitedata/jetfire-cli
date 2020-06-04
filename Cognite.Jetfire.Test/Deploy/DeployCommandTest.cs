// Copyright 2020 Cognite AS
// SPDX-License-Identifier: Apache-2.0

using System;
using Xunit;
using System.CommandLine;
using System.CommandLine.IO;
using System.CommandLine.Invocation;
using System.Threading.Tasks;
using System.Linq;
using Xunit.Abstractions;

namespace Cognite.Jetfire.Cli.Deploy
{
    public class DeployCommandTest
    {
        ITestOutputHelper testOutput;
        public DeployCommandTest(ITestOutputHelper testOutput)
        {
            this.testOutput = testOutput;
        }

        [Fact]
        public void ErrorOnDuplicateExternalIds()
        {
            var console = new TestConsole();
            var secrets = new TestSecretsProvider
            {
                ["JETFIRE_API_KEY"] = TestEnvironment.Jetfiretest1.ApiKey
            };

            var cmd = new DeployCommand(secrets).Command;
            cmd.Invoke("./TestData/duplicate-externalid", console);
            Assert.StartsWith("Error: Manifests contain duplicate externalIds", console.Error.ToString());
        }

        [Fact]
        public async Task BasicDeploymentIntegrationTest()
        {
            var externalId = "jetfire-cli-test-basic-deployment";

            var env = TestEnvironment.Jetfiretest1;

            var list = await env.Client.TransformConfigList();
            var existing = list.Find(x => x.ExternalId == externalId);
            if (existing != null)
            {
                try { await env.Client.ScheduleDelete(existing.Id); } catch {}
                await env.Client.TransformConfigDelete(existing.Id);
            }

            var console = new TestConsole();
            var secrets = new TestSecretsProvider
            {
                ["JETFIRE_API_KEY"] = env.ApiKey,
                ["TEST_API_KEY"] = env.ApiKey,
            };

            var cmd = new DeployCommand(secrets).Command;
            try
            {
                var exitCode = await cmd.InvokeAsync("./TestData/basic-deployment", console);
                Assert.Equal(0, exitCode);
            }
            finally
            {
                testOutput.WriteLine("=== Stdout ===");
                testOutput.WriteLine(console.Out.ToString());
                testOutput.WriteLine("=== Stderr ===");
                testOutput.WriteLine(console.Error.ToString());
            }

            var listAfterDeploy = await env.Client.TransformConfigList();
            var deployedConfig = listAfterDeploy.Find(x => x.ExternalId == externalId);
            Assert.NotNull(deployedConfig);
            try
            {
                Assert.Equal("update", deployedConfig.ConflictMode);
                Assert.Equal("datapoints", deployedConfig.Destination.Type);
                Assert.NotNull(deployedConfig.Schedule);
                Assert.Equal("1 1 1 1 1", deployedConfig.Schedule.Interval);
                Assert.False(deployedConfig.Schedule.IsPaused);
                Assert.Equal("select 0 limit 0", deployedConfig.Query.Trim());
            }
            finally
            {
                await env.Client.ScheduleDelete(deployedConfig.Id);
                await env.Client.TransformConfigDelete(deployedConfig.Id);
            }
        }
    }
}
