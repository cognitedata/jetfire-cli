// Copyright 2020 Cognite AS
// SPDX-License-Identifier: Apache-2.0

using System;
using System.CommandLine;
using System.Threading.Tasks;
using Cognite.Jetfire.Cli.Deploy;
using Cognite.Jetfire.Cli.Query;
using Cognite.Jetfire.Cli.Run;

namespace Cognite.Jetfire.Cli
{
    static class Program
    {
        static async Task<int> Main(string[] args)
        {
            var secrets = new EnvironmentSecretsProvider();

            var rootCommand = new JetfireRootCommand(
                new DeployCommand(secrets),
                new QueryCommand(secrets),
                new RunCommand(secrets)
            );

            return await rootCommand.Command.InvokeAsync(args);
        }
    }
}
