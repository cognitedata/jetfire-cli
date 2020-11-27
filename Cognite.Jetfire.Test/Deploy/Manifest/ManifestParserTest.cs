// Copyright 2020 Cognite AS
// SPDX-License-Identifier: Apache-2.0

using System;
using Xunit;

namespace Cognite.Jetfire.Cli.Deploy.Manifest
{
    public class ManifestParserTest
    {
        ManifestParser parser = new ManifestParser();

        [Fact]
        public void ParseSingleManifest()
        {
            var manifests = parser.ParseManifests(ConcatLines(
                "name: My Transformation",
                "externalId: my-transformation",
                "destination:",
                "  type: datapoints",
                "action: delete",
                "query: my-transformation.sql",
                "schedule: '* * * * *'",
                "shared: true",
                "apiKey:",
                "  read: API_KEY_READ",
                "  write: API_KEY_WRITE"
            ));

            var manifest = Assert.Single(manifests);
            Assert.Equal("My Transformation", manifest.Name);
            Assert.Equal("my-transformation", manifest.ExternalId);
            Assert.Equal(DestinationType.Datapoints, manifest.Destination.Type);
            Assert.Equal(ConflictMode.Delete, manifest.Action);
            Assert.Equal("my-transformation.sql", manifest.Query);
            Assert.Equal("* * * * *", manifest.Schedule);
            Assert.True(manifest.Shared);
            Assert.Equal("API_KEY_READ", manifest.ApiKey.Read);
            Assert.Equal("API_KEY_WRITE", manifest.ApiKey.Write);
        }

        [Fact]
        public void ParseMultipleManifests()
        {
            var manifests = parser.ParseManifests(ConcatLines(
                "name: first",
                "externalId: first",
                "---",
                "name: second",
                "externalId: second"
            ));
            Assert.Collection(
                manifests,
                first => {
                    Assert.Equal("first", first.Name);
                    Assert.Equal("first", first.ExternalId);
                },
                second => {
                    Assert.Equal("second", second.Name);
                    Assert.Equal("second", second.ExternalId);
                }
            );
        }

        [Fact]
        public void NotSharedByDefault()
        {
            var manifests = parser.ParseManifests("name: foo");
            var manifest = Assert.Single(manifests);
            Assert.False(manifest.Shared);
        }

        [Theory]
        [InlineData(DestinationType.AssetHierarchy, "assetHierarchy")]
        [InlineData(DestinationType.AssetHierarchy, "assethierarchy")]
        [InlineData(DestinationType.Events, "events")]
        [InlineData(DestinationType.Timeseries, "timeseries")]
        [InlineData(DestinationType.Datapoints, "datapoints")]
        [InlineData(DestinationType.StringDatapoints, "stringdatapoints")]
        [InlineData(DestinationType.StringDatapoints, "stringDatapoints")]
        [InlineData(DestinationType.Files, "files")]
        [InlineData(DestinationType.Sequences, "sequences")]
        [InlineData(DestinationType.Assets, "assets")]
        public void ParseShorthandDestination(DestinationType destinationType, string destinationTypeName)
        {
            var manifests = parser.ParseManifests($"destination: {destinationTypeName}");
            var manifest = Assert.Single(manifests);
            Assert.Equal(destinationType, manifest.Destination.Type);
        }

        [Fact]
        public void SingleApiKey()
        {
            var manifest = Assert.Single(parser.ParseManifests(ConcatLines(
                "apiKey: API_KEY"
            )));

            Assert.Equal("API_KEY", manifest.ApiKey.Read);
            Assert.Equal("API_KEY", manifest.ApiKey.Write);
        }

        [Fact]
        public void SeparateApiKeys()
        {
            var manifest = Assert.Single(parser.ParseManifests(ConcatLines(
                "apiKey:",
                "  read: API_KEY_READ",
                "  write: API_KEY_WRITE"
            )));

            Assert.Equal("API_KEY_READ", manifest.ApiKey.Read);
            Assert.Equal("API_KEY_WRITE", manifest.ApiKey.Write);
        }

        static string ConcatLines(params string[] lines) =>
            string.Join(Environment.NewLine, lines);
    }
}
