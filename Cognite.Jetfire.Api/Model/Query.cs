// Copyright 2020 Cognite AS
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace Cognite.Jetfire.Api.Model
{
    public class QueryRequest
    {
        public string Query { get; set; }
    }

    public class QueryResponse
    {
        public List<ColumnType> Schema { get; set; }

        public List<IDictionary<string, string>> Results { get; set; }
    }

    public class ColumnType
    {
        public string Name { get; set; }

        public string SqlType { get; set; }

        public bool Nullable { get; set; }
    }
}
