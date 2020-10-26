// Copyright 2020 Cognite AS
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.IO;
using System.Threading.Tasks;

using Cognite.Jetfire.Api;
using Cognite.Jetfire.Api.Model;
using Cognite.Jetfire.Cli.Deploy.Manifest;
using System.Threading;
using System.CommandLine.Binding;

namespace Cognite.Jetfire.Cli.Run
{
    public class RunCommand : IJetfireSubCommand
    {
        public RunCommand(ISecretsProvider secrets)
        {
            this.secrets = secrets;

            Command = new Command("run", "Start and/or watch transformation jobs")
            {
                new Option<int?>("--id")
                {
                    Description = "The id of the transformation to run. Either this or --external-id must be specified."
                },
                new Option<string>("--external-id")
                {
                    Description = "The externalId of the transformation to run. Either this or --id must be specified."
                },
                new Option<bool>("--watch")
                {
                    Description = "Wait until job has completed"
                },
                new Option<bool>("--watch-only")
                {
                    Description = "Do not start a transformation job, only watch the most recent job for completion"
                },
                new Option<TimeSpan>("--timeout", () => TimeSpan.FromHours(12))
                {
                    Description = "Maximum amount of time to wait for job to complete"
                },
                new Option<TimeSpan>("--interval", () => TimeSpan.FromSeconds(5))
                {
                    Description = "Time interval for polling job status"
                }
            };
            Command.Handler = CommandHandler.Create(
                (Func<IConsole, string, int?, string, bool, bool, TimeSpan, TimeSpan, Task<int>>)Handle
            );
        }

        ISecretsProvider secrets;

        ManifestParser parser = new ManifestParser();

        public Command Command { get; }

        async Task<int> Handle(
            IConsole console,
            string cluster,
            int? id,
            string externalId,
            bool watch,
            bool watchOnly,
            TimeSpan timeout,
            TimeSpan interval
        )
        {
            var client = JetfireClientFactory.CreateClient(secrets, cluster);

            if (id == null && externalId == null ||
                id != null && externalId != null)
            {
                throw new Exception("Either --id or --external-id must be specified");
            }

            int configId;
            if (id != null)
            {
                configId = id.Value;
            }
            else
            {
                var config = await client.TransformConfigByExternalId(externalId);
                configId = config.Id;
            }

            string jobUuid;
            if (watchOnly)
            {
                var jobs = await client.TransformConfigRecentJobs(configId);
                jobUuid = jobs.First().Uuid;
            }
            else
            {
                console.Out.Write($"Starting job for transformation {configId}");
                if (externalId != null)
                {
                    console.Out.Write($" (externalId: {externalId})");
                }
                console.Out.WriteLine();

                var startJobResult = await client.TransformConfigStartJob(configId);
                console.Out.WriteLine($"Started job {startJobResult.JobId}");
                jobUuid = startJobResult.JobId;
            }

            if (watch || watchOnly)
            {
                console.Out.WriteLine($"Watching job {jobUuid} for completion, timing out after {timeout}");

                var cancellationTokenSource = new CancellationTokenSource();
                var cancellationToken = cancellationTokenSource.Token;
                try
                {
                    cancellationTokenSource.CancelAfter(timeout);

                    while (!await IsJobCompleted(client, configId, jobUuid, cancellationToken))
                    {
                        await Task.Delay(interval, cancellationToken);
                    }
                    console.Out.WriteLine("Job completed successfully");
                }
                catch (TaskCanceledException)
                {
                    console.Error.WriteLine($"Timed out after {timeout}");
                    return 1;
                }
                catch (JobStatusException e)
                {
                    console.Error.WriteLine(e.Message);
                    return 1;
                }
                finally
                {
                    cancellationTokenSource.Dispose();
                }
            }

            return 0;
        }

        async Task<bool> IsJobCompleted(
            IJetfireClient client,
            int configId,
            string jobUuid,
            CancellationToken cancellationToken
        )
        {
            var jobs = await client.TransformConfigRecentJobs(configId, cancellationToken);
            var firstJob = jobs.First();
            var watchingJob = jobs.Single(x => x.Uuid == jobUuid);

            if (firstJob.Uuid != jobUuid && !watchingJob.FinishedTime.HasValue)
            {
                // If a new job has started since the one we're watching, but our job is still not finished,
                // the driver it was running on has crashed.
                throw new JobDeadException(watchingJob);
            }

            if (watchingJob.Error != null)
            {
                throw new JobFailedException(watchingJob);
            }

            return watchingJob.FinishedTime.HasValue;
        }
    }

    public class JobStatusException : Exception
    {
        public TransformJob Job { get; }

        public JobStatusException(TransformJob job, string message)
            : base(message)
        {
            Job = job;
        }
    }

    public class JobFailedException : JobStatusException
    {
        public JobFailedException(TransformJob job)
            : base(job, $"Job {job.Uuid} failed with the following error:\n{job.Error}")
        {
        }
    }

    public class JobDeadException : JobStatusException
    {
        public JobDeadException(TransformJob job)
            : base(job,
                $"Job {job.Uuid} is dead, and will never complete. " +
                "This likely means that it was running on a driver that has crashed.")
        {
        }
    }
}
