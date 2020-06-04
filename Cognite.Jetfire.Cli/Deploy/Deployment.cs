// Copyright 2020 Cognite AS
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.IO;
using System.Linq;
using System.Threading.Tasks;
using Cognite.Jetfire.Api;
using Cognite.Jetfire.Api.Model;
using Cognite.Jetfire.Cli.Deploy.Manifest;

namespace Cognite.Jetfire.Cli.Deploy
{
    public class Deployment
    {
        IJetfireClient client;
        List<TransformConfigRead> existingTransforms;
        IConsole console;

        public Deployment(
            IConsole console,
            IJetfireClient client,
            List<TransformConfigRead> existingTransforms
        )
        {
            this.console = console;
            this.client = client;
            this.existingTransforms = existingTransforms;
        }

        public async Task Deploy(ResolvedManifest resolvedManifest)
        {
            var externalId = resolvedManifest.Transformation.ExternalId;
            console.Out.WriteLine($"Deploying transformation '{externalId}'");
            var existingTransformToUpdate =
                existingTransforms.Find(x => x.ExternalId == resolvedManifest.Transformation.ExternalId);

            int transformId;

            if (existingTransformToUpdate == null)
            {
                console.Out.WriteLine($"Transformation '{externalId}' does not exist, creating...");
                var response = await Create(resolvedManifest);
                transformId = response.Id;
            }
            else
            {
                transformId = existingTransformToUpdate.Id;
                console.Out.WriteLine($"Transformation '{externalId}' already exists, updating...");
                await Update(existingTransformToUpdate.Id, resolvedManifest);
            }

            console.Out.WriteLine($"Updating schedule for transformation '{externalId}'");

            await UpdateSchedule(transformId, resolvedManifest);

            console.Out.WriteLine($"Transformation '{externalId}' was deployed successfully.");
        }

        Task<TransformConfigId> Create(ResolvedManifest manifest)
        {
            return client.TransformConfigCreate(new TransformConfigCreate
            {
                ExternalId = manifest.Transformation.ExternalId,
                IsPublic = manifest.Transformation.Shared,
                Name = manifest.Transformation.Name,
                Query = manifest.Query,
                Destination = ToDataSource(manifest.Transformation.Destination),
                ConflictMode = ToConflictModeString(manifest.Transformation.Action),
                SourceApiKey = manifest.ReadApiKey,
                DestinationApiKey = manifest.WriteApiKey,
            });
        }

        async Task Update(int id, ResolvedManifest manifest)
        {
            await client.TransformConfigUpdate(id, new TransformConfigUpdate
            {
                Name = manifest.Transformation.Name,
                Query = manifest.Query,
                Destination = ToDataSource(manifest.Transformation.Destination),
                ConflictMode = ToConflictModeString(manifest.Transformation.Action),
            });

            await client.TransformConfigUpdateSourceApiKey(id, manifest.ReadApiKey);
            await client.TransformConfigUpdateDestinationApiKey(id, manifest.WriteApiKey);
            await client.TransformConfigSetPublished(id, manifest.Transformation.Shared);
        }

        async Task UpdateSchedule(int id, ResolvedManifest manifest)
        {
            if (string.IsNullOrWhiteSpace(manifest.Transformation.Schedule))
            {
                await client.ScheduleDelete(id);
            }
            else
            {
                await client.ScheduleCreateOrUpdate(id, new ScheduleParams
                {
                    Interval = manifest.Transformation.Schedule,
                    IsPaused = false,
                });
            }
        }

        static DataSource ToDataSource(Destination destination)
        {
            return destination.Type switch
            {
                DestinationType.Raw => ToRawTableDataSource(destination),
                DestinationType.Assets => new DataSource("assets"),
                DestinationType.AssetHierarchy => new DataSource("asset_hierarchy"),
                DestinationType.Events => new DataSource("events"),
                DestinationType.Timeseries => new DataSource("timeseries"),
                DestinationType.Datapoints => new DataSource("datapoints"),
                DestinationType.StringDatapoints => new DataSource("string_datapoints"),
                _ => throw new ArgumentException($"Unknown data source type '{destination.Type}'", nameof(destination)),
            };
        }

        static DataSource ToRawTableDataSource(Destination destination)
        {
            return new RawDataSource
            {
                Type = "raw_table",
                RawType = "plain_raw",
                Database = destination.RawDatabase,
                Table = destination.RawTable,
            };
        }

        static string ToConflictModeString(ConflictMode mode)
        {
            return mode switch
            {
                ConflictMode.Upsert => "upsert",
                ConflictMode.Update => "update",
                ConflictMode.Create => "create",
                ConflictMode.Delete => "delete",
                _ => throw new ArgumentException($"Unknown conflict mode '{mode}'", nameof(mode))
            };
        }
    }
}
