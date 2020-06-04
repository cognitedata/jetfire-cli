// Copyright 2020 Cognite AS
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Cognite.Jetfire.Cli.Deploy.Manifest
{
    public enum DestinationType
    {
        Raw,
        Assets,
        AssetHierarchy,
        Events,
        Timeseries,
        Datapoints,
        StringDatapoints,
    }

    public class Destination
    {
        public DestinationType Type { get; set; }
        public string RawDatabase { get; set; }
        public string RawTable { get; set; }
    }
}
