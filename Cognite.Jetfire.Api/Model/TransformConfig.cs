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
    }

    public class TransformConfigCreate
    {
        public string ExternalId { get; set; }

        public string Name { get; set; }

        public string Query { get; set; }

        public DataSource Destination { get; set; }

        public string ConflictMode { get; set; }

        public bool IsPublic { get; set; }

        public string SourceApiKey { get; set; }

        public string DestinationApiKey { get; set; }
    }

    public class TransformConfigPublishOptions
    {
        public bool IsPublic { get; set; }
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
        public DataSource() {}

        public DataSource(string type)
        {
            Type = type;
        }

        public string Type { get; set; }

        // TODO: Implement proper polymorphic (de)serialization here

        public static DataSource Raw(string database, string table, string rawType = "plain_raw")
        {
            return new DataSource("raw_table")
            {
                RawType = rawType,
                Database = database,
                Table = table
            };
        }

        public string RawType { get; set; }

        public string Database { get; set; }

        public string Table { get; set; }
    }
}
