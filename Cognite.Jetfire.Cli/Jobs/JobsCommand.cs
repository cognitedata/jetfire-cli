using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading;
using System.Threading.Tasks;

namespace Cognite.Jetfire.Cli.Jobs
{
    public class JobsCommand : IJetfireSubCommand
    {
        public Command Command { get; }
        private ISecretsProvider Secrets;

        public JobsCommand(ISecretsProvider secrets)
        {
            Secrets = secrets;
            Command = new Command("jobs", "Show latest jobs for a given transformation")
            {
                new Option<int?>("--id")
                {
                    Description = "The id of the transformation. Either this or --external-id must be specified."
                },
                new Option<string>("--external-id")
                {
                    Description = "The externalId of the transformation. Either this or --id must be specified."
                },
            };

            Command.Handler = CommandHandler.Create<string, int?, string>(Handle);
        }

        async Task Handle(string cluster, int? id, string externalId)
        {
            using (var client = JetfireClientFactory.CreateClient(Secrets, cluster))
            {

                int configId = await Utils.ResolveEitherId(id, externalId, client);
                var jobs = await client.TransformConfigRecentJobs(configId);

                int idLength = 36;
                int dateLength = 19;
                int durationLength = 8;
                int statusLength = 7;

                Console.WriteLine($"{"ID".PadRight(idLength)}  {"STARTED TIME".PadRight(dateLength)}  {"DURATION".PadRight(durationLength)}  {"STATUS".PadRight(statusLength)}");
                foreach (var job in jobs)
                {
                    Console.WriteLine($"{job.Uuid.PadRight(idLength)}  {Utils.FormatTimestamp(job.StartedTime).PadRight(dateLength)}  {Utils.FormatDuration(job.StartedTime, job.FinishedTime).PadRight(durationLength)}  {job.Status().PadRight(statusLength)}");
                }
            }
        }
    }
}
