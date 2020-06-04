// Copyright 2020 Cognite AS
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Cognite.Jetfire.Api.Model
{
    public class ScheduleParams
    {
        public bool? IsPaused { get; set; }

        public string Interval { get; set; }
    }
}
