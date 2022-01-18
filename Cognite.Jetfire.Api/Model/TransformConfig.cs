// Copyright 2020 Cognite AS
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Net.Http;

namespace Cognite.Jetfire.Api.Model
{
    public class TransformConfigRead
    {
        public int Id { get; set; }

        public string ExternalId { get; set; }

        public string Name { get; set; }

        public string Query { get; set; }

        public DataSource Destination { get; set; }

        public string ConflictMode { get; set; }

        public ScheduleParams Schedule { get; set; }

        public bool IsPublic { get; set; }
        public bool ignoreNullFields { get; set; }

    }

    public class TransformConfigCreate
    {
        public string ExternalId { get; set; }

        public string Name { get; set; }

        public string Query { get; set; }

        public DataSource Destination { get; set; }

        public string ConflictMode { get; set; }

        public bool IsPublic { get; set; }

        public bool IgnoreNullFields { get; set; }

        public string SourceApiKey { get; set; }

        public string DestinationApiKey { get; set; }

        public FlatOidcCredentials SourceOidcCredentials { get; set; }

        public FlatOidcCredentials DestinationOidcCredentials { get; set; }
    }

    public class FlatOidcCredentials
    {
        public string ClientId { get; set; }

        public string ClientSecret { get; set; }

        public string Scopes { get; set; }
        public string Audience { get; set; }

        public string TokenUri { get; set; }

        public string CdfProjectName { get; set; }
    }

    public class TransformConfigPublishOptions
    {
        public bool IsPublic { get; set; }
    }

    public class TransformConfigIgnoreNullFieldsOptions
    {
        public bool IgnoreNullFields { get; set; }
    }

    public class TransformConfigApiKeyUpdate
    {
        public string ApiKey { get; set; }
    }

    public class TransformConfigUpdate
    {
        public string Name { get; set; }

        public string Query { get; set; }

        public string ConflictMode { get; set; }

        public DataSource Destination { get; set; }
    }

    public class TransformConfigId
    {
        public int Id { get; set; }
    }

    public class DataSource
    {
        public DataSource() { }

        public DataSource(string type)
        {
            Type = type;
        }

        public string Type { get; set; }

        // TODO: Implement proper polymorphic (de)serialization here

        public static DataSource Raw(string database, string table)
        {
            return new DataSource("raw")
            {
                Database = database,
                Table = table
            };
        }

        public string Database { get; set; }

        public string Table { get; set; }
    }
}
