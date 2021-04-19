// Copyright 2020 Cognite AS
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading.Tasks;

namespace Cognite.Jetfire.Cli.ListTransforms
{
    public class ListCommand : IJetfireSubCommand
    {
        public Command Command { get; }
        private ISecretsProvider Secrets;

        public ListCommand(ISecretsProvider secrets)
        {
            Secrets = secrets;

            Command = new Command("list", "List transformations");

            Command.Handler = CommandHandler.Create<string>(Handle);
        }

        async Task Handle(string cluster)
        {
            using (var client = JetfireClientFactory.CreateClient(Secrets, cluster))
            {
                var list = await client.TransformConfigList(new System.Threading.CancellationToken());

                // Sort on ID descending (and thus created at time)
                list.Sort((x, y) => x.Id.CompareTo(y.Id));

                // Width of columns (initialized with length of col title)
                var IdLength = 2;
                var ExternalIdLength = 11;
                var NameLength = 4;
                var ScheduleLength = 8;

                // Calculate col width
                List<Tuple<string, string, string, string>> rows = new List<Tuple<string, string, string, string>>();
                foreach (var transformConfig in list)
                {
                    var id = transformConfig.Id.ToString();
                    var extid = transformConfig.ExternalId ?? "";
                    var name = transformConfig.Name;
                    var sched = transformConfig?.Schedule?.ToString() ?? "";

                    IdLength = Math.Max(IdLength, id.Length);
                    ExternalIdLength = Math.Max(ExternalIdLength, extid.Length);
                    NameLength = Math.Max(NameLength, name.Length);
                    ScheduleLength = Math.Max(ScheduleLength, sched.Length);

                    rows.Add(Tuple.Create(id, extid, name, sched));
                }

                // Print table
                Console.WriteLine($"{"ID".PadRight(IdLength)}  {"EXTERNAL ID".PadRight(ExternalIdLength)}  {"NAME".PadRight(NameLength)}  {"SCHEDULE".PadRight(ScheduleLength)}");
                foreach (var row in rows)
                {
                    Console.WriteLine($"{row.Item1.PadRight(IdLength)}  {row.Item2.PadRight(ExternalIdLength)}  {row.Item3.PadRight(NameLength)}  {row.Item4.PadRight(ScheduleLength)}");
                }
            }
        }
    }
}
