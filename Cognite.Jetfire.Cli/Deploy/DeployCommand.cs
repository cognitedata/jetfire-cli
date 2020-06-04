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

namespace Cognite.Jetfire.Cli.Deploy
{
    public class DeployCommand
    {
        public DeployCommand(ISecretsProvider secrets)
        {
            this.secrets = secrets;

            Command = new Command("deploy", "Deploy a set of transformations from a directory")
            {
                new Argument<string>("path", Directory.GetCurrentDirectory)
                {
                    Arity = ArgumentArity.ZeroOrOne,
                    Description = "A directory to search for transformation manifests. If omitted, the current directory is used.",
                },
            };
            Command.Handler = CommandHandler.Create<IConsole, string, string>(Handle);
        }

        ISecretsProvider secrets;

        ManifestParser parser = new ManifestParser();

        public Command Command { get; }

        static readonly string[] manifestFileExtensions = new[] { ".yml", ".yaml" };

        async Task<int> Handle(IConsole console, string cluster, string path)
        {
            var client = JetfireClientFactory.CreateClient(secrets, cluster);

            var absolutePath = Path.GetFullPath(path);

            var manifests =
                Directory.EnumerateFiles(absolutePath, "*.*", SearchOption.AllDirectories)
                .Where(path => manifestFileExtensions.Any(ext => path.EndsWith(ext)))
                .SelectMany(path =>
                {
                    var yaml = File.ReadAllText(path);
                    var manifests = parser.ParseManifests(yaml);
                    return manifests.Select(manifest => (path, manifest));
                })
                .ToList();

            var duplicateExternalIds =
                manifests
                .GroupBy(x => x.manifest.ExternalId)
                .Where(group => group.Count() > 1);

            if (duplicateExternalIds.Any())
            {
                console.Error.WriteLine($"Error: Manifests contain duplicate externalIds:");
                foreach (var group in duplicateExternalIds)
                {
                    console.Error.WriteLine($"ExternalId '{group.Key}' is defined in the following files:");
                    foreach (var (duplicatedPath, duplicatedTransform) in group)
                    {
                        var relativeDuplicatedPath = Path.GetRelativePath(duplicatedPath, absolutePath);
                        console.Error.WriteLine($"- '{relativeDuplicatedPath}' (name: {duplicatedTransform.Name})");
                    }
                    console.Error.WriteLine("");
                }

                return 1;
            }

            var resolver = new ManifestResolver(secrets);

            var resolveResults = manifests.Select(t => resolver.Resolve(t.manifest, t.path));
            var errorResults = resolveResults.Where(x => x.IsError).ToArray();
            if (errorResults.Any())
            {
                console.Error.WriteLine("Error: Found invalid transformation manifests:");
                console.Error.WriteLine();
                foreach (var result in errorResults)
                {
                    var formattedExternalId = result.ResolvedManifest?.Transformation?.ExternalId ?? "null";
                    console.Error.WriteLine($"In '{result.ManifestPath}' (externalId: {formattedExternalId}):");

                    foreach (var error in result.Errors)
                    {
                        console.Error.WriteLine($"    - {error}");
                    }

                    console.Error.WriteLine();
                }

                return 1;
            }

            var existingTransforms = await client.TransformConfigList();
            var deployment = new Deployment(console, client, existingTransforms);

            foreach (var result in resolveResults)
            {
                var externalId = result.ResolvedManifest.Transformation.ExternalId;
                try
                {
                    await deployment.Deploy(result.ResolvedManifest);
                }
                catch (Exception e)
                {
                    console.Error.WriteLine($"Failed to deploy transformation '{externalId}':");
                    console.Error.WriteLine(e.ToString());
                    console.Error.WriteLine();
                    return 1;
                }
            }

            return 0;
        }
    }
}
