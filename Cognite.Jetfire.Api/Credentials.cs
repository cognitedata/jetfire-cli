// Copyright 2020 Cognite AS
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Net.Http;
using Cognite.Jetfire.Api.Model;

namespace Cognite.Jetfire.Api
{
    public interface ICredentials
    {
        void Apply(HttpRequestMessage request);
    }

    public sealed class ApiKeyCredentials : ICredentials
    {
        public string ApiKey { get; set; }

        public ApiKeyCredentials(string apiKey)
        {
            ApiKey = apiKey;
        }

        public void Apply(HttpRequestMessage request)
        {
            request.Headers.Add("api-key", ApiKey);
        }
    }
}
