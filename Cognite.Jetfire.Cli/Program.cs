// Copyright 2020 Cognite AS
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Threading.Tasks;
using Cognite.Jetfire.Cli.Deploy;
using Cognite.Jetfire.Cli.ListTransforms;
using Cognite.Jetfire.Cli.Query;
using Cognite.Jetfire.Cli.Run;
using Cognite.Jetfire.Cli.Show;

namespace Cognite.Jetfire.Cli
{
    static class Program
    {
        static async Task<int> Main(string[] args)
        {
            var secrets = new EnvironmentSecretsProvider();

            var rootCommand = new JetfireRootCommand(
                new List<IJetfireSubCommand>() {
                    new DeployCommand(secrets),
                    new QueryCommand(secrets),
                    new RunCommand(secrets),
                    new ListCommand(secrets),
                    new ShowCommand(secrets)
                }
            );

            return await rootCommand.Command.InvokeAsync(args);
        }
    }
}
