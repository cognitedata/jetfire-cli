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

        public async Task ApplyAsync(HttpRequestMessage request)
        {
            request.Headers.Add("api-key", ApiKey);
        }
    }

    public sealed class TokenCredentials : ICredentials
    {
        public string ClientId { get; set; }
        public string Secret { get; set; }
        public List<string> Scopes { get; set; }
        public string Authority { get; set; }

        private IAuthenticator Authenticator;

        public TokenCredentials(AuthenticatorConfig authenticatorConfig)
        {
            Authenticator = new Authenticator(authenticatorConfig, new HttpClient(), NullLogger<IAuthenticator>.Instance);
        }

        public async Task ApplyAsync(HttpRequestMessage request)
        {
            var token = await Authenticator.GetToken();
            request.Headers.Add("Authorization", $"Bearer ${token}");
        }
    }
}
