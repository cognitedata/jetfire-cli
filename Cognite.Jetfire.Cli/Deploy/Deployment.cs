// Copyright 2020 Cognite AS
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Net;
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
                await Update(existingTransformToUpdate, resolvedManifest);
            }

            console.Out.WriteLine($"Updating schedule for transformation '{externalId}'");

            await UpdateSchedule(transformId, resolvedManifest);

            console.Out.WriteLine($"Updating notification rules for transformation '{externalId}'");

            await UpdateNotifications(transformId, resolvedManifest);

            console.Out.WriteLine($"Transformation '{externalId}' was deployed successfully.");
        }

        Task<TransformConfigId> Create(ResolvedManifest manifest)
        {
            return client.TransformConfigCreate(new TransformConfigCreate
            {
                ExternalId = manifest.Transformation.ExternalId,
                IsPublic = manifest.Transformation.Shared,
                IgnoreNullFields = manifest.Transformation.IgnoreNullFields,
                Name = manifest.Transformation.Name,
                Query = manifest.Query,
                Destination = ToDataSource(manifest.Transformation.Destination),
                ConflictMode = ToConflictModeString(manifest.Transformation.Action),
                SourceApiKey = manifest.ReadApiKey,
                DestinationApiKey = manifest.WriteApiKey,
                SourceOidcCredentials = manifest.ReadCredentials,
                DestinationOidcCredentials = manifest.WriteCredentials
            });
        }

        async Task Update(TransformConfigRead transformToUpdate, ResolvedManifest manifest)
        {
            var id = transformToUpdate.Id;
            await client.TransformConfigUpdate(id, new TransformConfigUpdate
            {
                Name = manifest.Transformation.Name,
                Query = manifest.Query,
                Destination = ToDataSource(manifest.Transformation.Destination),
                ConflictMode = ToConflictModeString(manifest.Transformation.Action),
            });

            if (manifest.ReadApiKey != null)
                await client.TransformConfigUpdateSourceApiKey(id, manifest.ReadApiKey);
            if (manifest.WriteApiKey != null)
                await client.TransformConfigUpdateDestinationApiKey(id, manifest.WriteApiKey);
            if (manifest.ReadCredentials != null)
                await client.TransformConfigUpdateSourceCredentials(id, manifest.ReadCredentials);
            if (manifest.WriteCredentials != null)
                await client.TransformConfigUpdateDestinationCredentials(id, manifest.WriteCredentials);

            if (transformToUpdate.IsPublic != manifest.Transformation.Shared)
            {
                await client.TransformConfigSetPublished(id, manifest.Transformation.Shared);
            }

            if (transformToUpdate.ignoreNullFields != manifest.Transformation.IgnoreNullFields)
            {
                await client.TransformConfigSetIgnoreNullFields(id, manifest.Transformation.IgnoreNullFields);
            }
        }

        async Task UpdateSchedule(int id, ResolvedManifest manifest)
        {
            if (string.IsNullOrWhiteSpace(manifest.Transformation.Schedule))
            {
                try
                {
                    await client.ScheduleDelete(id);
                }
                catch (JetfireApiException e)
                when ((int)e.StatusCode < 500)
                { }
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

        async Task UpdateNotifications(int id, ResolvedManifest manifest)
        {
            var existingDestinations = new List<NotificationRead>(await client.NotificationList(id));

            var toCreate = new List<string>();
            var toDelete = new List<NotificationRead>();

            if (manifest.Transformation.Notifications != null)
            {
                foreach (var destination in manifest.Transformation.Notifications)
                {
                    // Remove requested destinations from existing list to end up with delete delta
                    if (existingDestinations.RemoveAll(n => (n.Destination == destination)) == 0)
                    {
                        // Remove returns 0 -> doesn't exist, so create it
                        toCreate.Add(destination);
                    }

                }
            }

            // The remaining entries in existing is now only those not included in the new manifest, delete them
            foreach (var destination in existingDestinations)
            {
                toDelete.Add(destination);
            }

            Console.WriteLine($"Creating {toCreate.Count} and removing {toDelete.Count} notification destinations");

            foreach (var destination in toCreate)
            {
                await client.NotificationCreate(id, destination);
            }
            foreach (var destination in toDelete)
            {
                await client.NotificationDelete(id, destination.Id);
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
                DestinationType.Files => new DataSource("files"),
                DestinationType.Sequences => new DataSource("sequences"),
                DestinationType.Labels => new DataSource("labels"),
                DestinationType.Relationships => new DataSource("relationships"),
                _ => throw new ArgumentException($"Unknown data source type '{destination.Type}'", nameof(destination)),
            };
        }

        static DataSource ToRawTableDataSource(Destination destination)
        {
            return DataSource.Raw(
                database: destination.RawDatabase,
                table: destination.RawTable
            );
        }

        static string ToConflictModeString(ConflictMode mode)
        {
            return mode switch
            {
                ConflictMode.Upsert => "upsert",
                ConflictMode.Update => "update",
                ConflictMode.Create => "abort",
                ConflictMode.Abort => "abort",
                ConflictMode.Delete => "delete",
                _ => throw new ArgumentException($"Unknown conflict mode '{mode}'", nameof(mode))
            };
        }
    }
}
