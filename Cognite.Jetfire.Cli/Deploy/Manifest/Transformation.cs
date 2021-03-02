// Copyright 2020 Cognite AS
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Cognite.Jetfire.Cli.Deploy.Manifest
{
    public class Transformation
    {
        public string Name { get; set; }

        public string ExternalId { get; set; }

        public bool Shared { get; set; }

        public string Schedule { get; set; }

        public Destination Destination { get; set; }

        public ConflictMode Action { get; set; }

        public ApiKeys ApiKey { get; set; }

        public ReadWriteOidcCredentials Authentication { get; set; }

        public string Query { get; set; }

        public string[] Notifications { get; set; }
    }
}
