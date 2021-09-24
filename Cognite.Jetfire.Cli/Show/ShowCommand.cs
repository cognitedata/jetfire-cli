using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cognite.Jetfire.Api;
using Cognite.Jetfire.Api.Model;

namespace Cognite.Jetfire.Cli.Show
{
    public class ShowCommand : IJetfireSubCommand
    {
        public Command Command { get; }
        private ISecretsProvider Secrets;

        public ShowCommand(ISecretsProvider secrets)
        {
            Secrets = secrets;

            Command = new Command("show", "Show detalis of a transformation")
            {
                new Option<int?>("--id")
                {
                    Description = "The id of the transformation to show. Either this or --external-id must be specified."
                },
                new Option<string>("--external-id")
                {
                    Description = "The externalId of the transformation to show. Either this or --id must be specified."
                },
                new Option<string>("--job")
                {
                    Description = "The UUID of the job to show. Include this to show job details instead of transformation details."
                },
            };

            Command.Handler = CommandHandler.Create<string, int?, string, string>(Handle);

        }

        async Task Handle(string cluster, int? id, string externalId, string job)
        {
            if (job != null)
            {
                await ShowJob(cluster, id, externalId, job);
            }
            else
            {
                await ShowTransform(cluster, id, externalId);
            }
        }

        async Task ShowTransform(string cluster, int? id, string externalId)
        {
            TransformConfigRead transform;
            NotificationRead[] notifications;

            using (var client = JetfireClientFactory.CreateClient(Secrets, cluster))
            {

                if (id == null && externalId != null)
                {
                    transform = await client.TransformConfigByExternalId(externalId, new CancellationToken());
                }
                else if (id != null && externalId == null)
                {
                    transform = await client.TransformConfigById(id.Value, new CancellationToken());
                }
                else
                {
                    throw new JetfireCliException("Either --id or --external-id must be specified");
                }

                notifications = await client.NotificationList(transform.Id);
            }


            Console.WriteLine($"Name:           {transform.Name}");
            Console.WriteLine($"ID:             {transform.Id}");
            Console.WriteLine($"External ID:    {transform.ExternalId}");

            Console.WriteLine();

            Console.WriteLine($"Destination:    {transform.Destination.Type}");
            if (transform.Destination.Type == "raw")
            {
                Console.WriteLine($"   Database:    {transform.Destination.Database}");
                Console.WriteLine($"   Table:       {transform.Destination.Table}");
            }
            Console.WriteLine($"Action:         {transform.ConflictMode}");
            Console.WriteLine($"Schedule:       {transform?.Schedule?.ToString()?.ToLower() ?? "no schedule"}");

            Console.WriteLine($"Notifications:  {(notifications.Length == 0 ? "no destinations" : "")}");
            foreach (var notification in notifications)
            {
                Console.WriteLine($"   {notification.Destination}");
            }

            Console.WriteLine();
            Console.WriteLine("".PadLeft(50, '-'));
            Console.WriteLine();

            Console.WriteLine(transform.Query);
        }

        async Task ShowJob(string cluster, int? id, string externalId, string jobId)
        {
            using (var client = JetfireClientFactory.CreateClient(Secrets, cluster))
            {
                int configId = await Utils.ResolveEitherId(id, externalId, client);
                var jobs = await client.TransformConfigRecentJobs(configId);

                var filteredJobs = jobs.Where(job => job.Uuid == jobId);
                if (filteredJobs.Count() == 0)
                {
                    throw new JetfireCliException($"Job with UUID {jobId} not found");
                }

                var job = filteredJobs.First();

                Console.WriteLine($"Job UUID:            {job.Uuid}");
                Console.WriteLine($"Transformation ID:   {configId}");

                Console.WriteLine();

                Console.WriteLine($"Destination:         {job.DestinationType}");
                if (job.DestinationType == "raw")
                {
                    Console.WriteLine($"   Database:        {job.DestinationDatabase}");
                    Console.WriteLine($"   Table:           {job.DestinationTable}");
                }
                Console.WriteLine($"Destination project: {job.DestinationProject}");
                Console.WriteLine($"Action:              {job.ConflictMode}");

                Console.WriteLine();

                Console.WriteLine($"Created at:          {Utils.FormatTimestamp(job.CreatedTime)}");
                Console.WriteLine($"Started at:          {Utils.FormatTimestamp(job.StartedTime)}");
                Console.WriteLine($"Finished at:         {Utils.FormatTimestamp(job.FinishedTime)}");
                Console.WriteLine($"Duration:            {Utils.FormatDuration(job.StartedTime, job.FinishedTime)}");
                Console.WriteLine($"Status:              {job.Status()}");
                if (job.Error != null)
                {
                    Console.WriteLine($"   Error:            {job.Error}");
                }

                Console.WriteLine();
                Console.WriteLine("Progress:");
                var metrics = await GetLatestMetrics(jobId, client);
                if (metrics.Count > 0)
                {
                    foreach (var metric in metrics)
                    {
                        Console.WriteLine($"   {metric.Key,-17} {metric.Value}");
                    }
                }
                else
                {
                    Console.WriteLine("   No progress available");
                }


                Console.WriteLine();
                Console.WriteLine("".PadLeft(50, '-'));
                Console.WriteLine();

                Console.WriteLine(job.RawQuery);
            }

            async Task<Dictionary<string, long>> GetLatestMetrics(string jobId, IJetfireClient client)
            {
                var metrics = await client.TransformJobMetrics(jobId, new CancellationToken());
                var latest = new Dictionary<string, long>();
                var latestTimestamp = new Dictionary<string, long>();

                foreach (var counter in metrics)
                {
                    if (counter.Timestamp >= latestTimestamp.GetValueOrDefault(counter.Name, 0))
                    {
                        latest[counter.Name] = counter.Count;
                        latestTimestamp[counter.Name] = counter.Timestamp;
                    }
                }

                return latest;
            }
        }
    }
}
