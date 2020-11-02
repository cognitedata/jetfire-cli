// Copyright 2020 Cognite AS
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Cognite.Jetfire.Api.Model
{
    public class TransformJobId
    {
        public string JobId { get; set; }
    }

    public class TransformJob
    {
        public string Uuid { get; set; }

        public string SourceProject { get; set; }

        public string DestinationProject { get; set; }

        public string DestinationType { get; set; }

        public string DestinationDatabase { get; set; }

        public string DestinationTable { get; set; }

        public string ConflictMode { get; set; }

        public string RawQuery { get; set; }

        public long? CreatedTime { get; set; }

        public long? StartedTime { get; set; }

        public long? FinishedTime { get; set; }

        public long? LastSeenTime { get; set; }

        public string Error { get; set; }

        public string Status()
        {
            if (FinishedTime == null)
            {
                return "running";
            }
            else if (Error == null)
            {
                return "success";
            }
            else
            {
                return "failed";
            }
        }
    }
}
