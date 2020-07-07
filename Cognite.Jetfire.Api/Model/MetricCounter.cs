// Copyright 2020 Cognite AS
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Cognite.Jetfire.Api.Model
{
    public class MetricCounter
    {
        public long Id { get; set; }

        public long Timestamp { get; set; }

        public string Name { get; set; }

        public long Count { get; set; }
    }
}
