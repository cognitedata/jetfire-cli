// Copyright 2020 Cognite AS
// SPDX-License-Identifier: Apache-2.0

using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

using Cognite.Jetfire.Api;
using Cognite.Jetfire.Api.Model;

namespace Cognite.Jetfire.Cli.Query
{
    public class QueryCommand : IJetfireSubCommand
    {
        public Command Command { get; }

        public QueryCommand(ISecretsProvider secrets)
        {
            this.secrets = secrets;

            Command = new Command("query", "Make a SQL query and retrieve up to 1000 rows")
            {
                new Argument<string>("query")
                {
                    Arity = ArgumentArity.ExactlyOne,
                    Description = "The SQL query to run",
                },
                new Option<long>(
                    alias: "--source-limit",
                    description: "Maximum number of rows to read from each data source. " +
                                 "Values lower than 1 will disable this limit. Default: 100",
                    getDefaultValue: () => 100
                ),
                new Option<long>(
                    alias: "--infer-schema-limit",
                    description: "Maximum number of rows to read for inferring schema from RAW tables. " +
                                 "Values lower than 1 will disable this limit. Default: 100",
                    getDefaultValue: () => 100
                ),
            };
            Command.Handler = CommandHandler.Create<string, string, long, long>(Handle);
        }

        ISecretsProvider secrets;

        async Task<int> Handle(string cluster, string query, long sourceLimit, long inferSchemaLimit)
        {
            // Treat e.g. `--source-limit 0` as no limit
            long? sourceLimitOrNull = NullIfNotPositive(sourceLimit);
            long? inferSchemaLimitOrNull = NullIfNotPositive(inferSchemaLimit);

            using (var client = JetfireClientFactory.CreateClient(secrets, cluster))
            {
                var results = await client.Query(
                    query: query,
                    resultsLimit: sourceLimitOrNull,
                    inferSchemaLimit: inferSchemaLimitOrNull
                );
                using (var stdout = Console.OpenStandardOutput())
                {
                    await JsonSerializer.SerializeAsync(stdout, results, JsonConfig.SerializerOptions);
                }
            }
            return 0;
        }

        static long? NullIfNotPositive(long value)
        {
            if (value <= 0)
            {
                return null;
            }
            return value;
        }
    }
}
