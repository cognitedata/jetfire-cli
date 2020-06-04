// Copyright 2020 Cognite AS
// SPDX-License-Identifier: Apache-2.0

using System;
using Cognite.Jetfire.Api;

namespace Cognite.Jetfire.Cli
{
    public static class JetfireClientFactory
    {
        static readonly string ApiKeyEnvironmentVariable = "JETFIRE_API_KEY";

        public static IJetfireClient CreateClient(ISecretsProvider secrets, string cluster)
        {
            var baseUri = GetBaseUriFromCluster(cluster);
            var credentials = GetCredentials(secrets);
            return new JetfireClient(baseUri, credentials);
        }

        static ICredentials GetCredentials(ISecretsProvider secrets)
        {
            var apiKey = secrets.GetNamedSecret(ApiKeyEnvironmentVariable);
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new Exception($"The {ApiKeyEnvironmentVariable} environment variable must be set");
            }
            return new ApiKeyCredentials(apiKey);
        }

        static Uri GetBaseUriFromCluster(string cluster)
        {
            cluster = cluster ?? "europe-west1-1";
            return new Uri($"https://jetfire.{cluster}.cogniteapp.com");
        }
    }
}
