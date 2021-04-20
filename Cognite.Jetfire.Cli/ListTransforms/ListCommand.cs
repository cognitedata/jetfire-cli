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
                var LastRunLength = 7;

                // Calculate col width
                var rows = new List<(string id, string extid, string name, string sched, string lastRun)>();
                foreach (var transformConfig in list)
                {
                    var id = transformConfig.Id.ToString();
                    var extid = transformConfig.ExternalId ?? "";
                    var name = transformConfig.Name;
                    var sched = transformConfig?.Schedule?.ToString() ?? "";
                    var fullLastRun = await client.TransformConfigRecentJobs(transformConfig.Id, 1);
                    var lastRun = fullLastRun.Length > 0 ? fullLastRun[0].Status() : "";

                    IdLength = Math.Max(IdLength, id.Length);
                    ExternalIdLength = Math.Max(ExternalIdLength, extid.Length);
                    NameLength = Math.Max(NameLength, name.Length);
                    ScheduleLength = Math.Max(ScheduleLength, sched.Length);
                    LastRunLength = Math.Max(LastRunLength, lastRun.Length);

                    rows.Add(Tuple.Create(id, extid, name, sched, lastRun));
                }

                // Print table
                Console.WriteLine($"{"ID".PadRight(IdLength)}  {"EXTERNAL ID".PadRight(ExternalIdLength)}  {"NAME".PadRight(NameLength)}  {"SCHEDULE".PadRight(ScheduleLength)}  {"LAST RUN".PadRight(LastRunLength)}");
                foreach (var row in rows)
                {
                    Console.WriteLine($"{row.id.PadRight(IdLength)}  {row.etxid.PadRight(ExternalIdLength)}  {row.name.PadRight(NameLength)}  {row.sched.PadRight(ScheduleLength)}  {row.lastRun.PadRight(LastRunLength)}");
                }
            }
        }
    }
}
