// Copyright 2020 Cognite AS
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Threading.Tasks;
using Cognite.Jetfire.Api;
using Cognite.Jetfire.Cli.Delete;
using Cognite.Jetfire.Cli.Deploy;
using Cognite.Jetfire.Cli.Jobs;
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
                    new ShowCommand(secrets),
                    new DeleteCommand(secrets),
                    new JobsCommand(secrets),
                }
            );

            try
            {
                return await rootCommand.Command.InvokeAsync(args);
            }
            catch (JetfireCliException e)
            {
                Console.Error.WriteLine($"Error: {e.Message}");
                return 1;
            }
            catch (JetfireApiException e)
            {
                Console.Error.WriteLine($"API Error: {e.Message}");
                return 1;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine($"Unhandled error: {e.Message}. ");
                Console.Error.WriteLine(e.StackTrace);
                return 1;
            }
        }
    }
}
