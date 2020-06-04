// Copyright 2020 Cognite AS
// SPDX-License-Identifier: Apache-2.0

using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Cognite.Jetfire.Api.Model;

namespace Cognite.Jetfire.Cli.Deploy.Manifest
{
    public class ResolvedManifest
    {
        public Transformation Transformation { get; set; }

        public string ReadApiKey { get; set; }

        public string WriteApiKey { get; set; }

        public string Query { get; set; }
    }

    public class ManifestResolverResult
    {
        public List<string> Errors { get; set; } = new List<string>();

        public string ManifestPath { get; set; }

        public ResolvedManifest ResolvedManifest { get; set; }

        public bool IsError => Errors.Any();

        public bool IsValid => !IsError;

        public void ReportError(string message) => Errors.Add(message);
    }

    /// <summary>
    /// Validates transformation manifests and resolves externally configured information like API keys and sql files.
    /// </summary>
    public class ManifestResolver
    {
        ISecretsProvider secrets;

        public ManifestResolver(ISecretsProvider secrets)
        {
            this.secrets = secrets;
        }

        public ManifestResolverResult Resolve(
            Transformation manifest,
            string manifestPath
        )
        {
            var resolvedManifest = new ResolvedManifest { Transformation = manifest };
            var result = new ManifestResolverResult()
            {
                ManifestPath = manifestPath,
                ResolvedManifest = resolvedManifest,
            };

            try
            {
                if (manifest.ApiKey == null)
                {
                    result.ReportError("API keys are not defined");
                }
                else
                {
                    resolvedManifest.ReadApiKey = GetSecret(manifest.ApiKey.Read, result);
                    resolvedManifest.WriteApiKey = GetSecret(manifest.ApiKey.Write, result);
                }

                var destination = manifest.Destination;
                if (destination == null)
                {
                    result.ReportError("Destination type is not defined");
                }
                else
                {
                    if (destination.Type == DestinationType.Raw)
                    {
                        if (string.IsNullOrWhiteSpace(destination.RawDatabase))
                        {
                            result.ReportError("RAW database is not defined");
                        }
                        if (string.IsNullOrWhiteSpace(destination.RawTable))
                        {
                            result.ReportError("RAW table is not defined");
                        }
                    }
                }

                if (string.IsNullOrWhiteSpace(manifest.ExternalId))
                {
                    result.ReportError("ExternalId is not defined");
                }

                if (string.IsNullOrWhiteSpace(manifest.Query))
                {
                    result.ReportError("Query is not defined");
                }
                else
                {
                    var manifestDirectoryPath = Path.GetDirectoryName(manifestPath);
                    var sqlFilePath = Path.GetFullPath(Path.Join(manifestDirectoryPath, manifest.Query));

                    try
                    {
                        resolvedManifest.Query = File.ReadAllText(sqlFilePath);
                    }
                    catch (FileNotFoundException)
                    {
                        result.ReportError($"SQL file was not found: {sqlFilePath}");
                    }
                    catch (Exception e)
                    {
                        result.ReportError($"Failed to read SQL file: {e}");
                    }
                }
            }
            catch (Exception e)
            {
                result.ReportError($"Exception thrown while resolving manifest: {e}");
            }

            return result;
        }

        string GetSecret(string name, ManifestResolverResult result)
        {
            var value = secrets.GetNamedSecret(name);
            if (value == null)
            {
                result.ReportError($"Could not find secret named '{name}'.");
            }
            return value;
        }
    }
}
