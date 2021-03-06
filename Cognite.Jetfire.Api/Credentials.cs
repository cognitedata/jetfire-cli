// Copyright 2020 Cognite AS
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Cognite.Extractor.Utils;
using Cognite.Jetfire.Api.Model;
using Microsoft.Extensions.Logging.Abstractions;

namespace Cognite.Jetfire.Api
{
    public interface ICredentials
    {
        Task ApplyAsync(HttpRequestMessage request);
    }

    public sealed class ApiKeyCredentials : ICredentials
    {
        public string ApiKey { get; set; }

        public ApiKeyCredentials(string apiKey)
        {
            ApiKey = apiKey;
        }

#pragma warning disable CS1998 // Need async to comply with interface
        public async Task ApplyAsync(HttpRequestMessage request)
        {
            request.Headers.Add("api-key", ApiKey);
        }
#pragma warning restore CS1998
    }

    public sealed class TokenCredentials : ICredentials
    {
        private string Project;
        private IAuthenticator Authenticator;

        public TokenCredentials(AuthenticatorConfig authenticatorConfig, string project)
        {
            Authenticator = new Authenticator(authenticatorConfig, new HttpClient(), NullLogger<IAuthenticator>.Instance);
            Project = project;
        }

        public async Task ApplyAsync(HttpRequestMessage request)
        {
            var token = await Authenticator.GetToken();
            request.Headers.Add("Authorization", $"Bearer {token}");
            request.Headers.Add("project", Project);
        }
    }
}
