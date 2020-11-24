// Copyright 2020 Cognite AS
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;

namespace Cognite.Jetfire.Api
{
    public static class JsonConfig
    {
        public static readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            IgnoreNullValues = true
        };
    }
}
