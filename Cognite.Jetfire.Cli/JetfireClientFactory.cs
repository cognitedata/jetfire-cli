// Copyright 2020 Cognite AS
// SPDX-License-Identifier: Apache-2.0

using System;
using Cognite.Extractor.Configuration;
using Cognite.Extractor.Utils;
using Cognite.Jetfire.Api;

namespace Cognite.Jetfire.Cli
{
    public static class JetfireClientFactory
    {
        static readonly string ApiKeyEnvironmentVariable = "JETFIRE_API_KEY";
        static readonly string CredentialsEnvironmentVariable = "JETFIRE_IDP_CREDENTIALS";

        public static IJetfireClient CreateClient(ISecretsProvider secrets, string cluster)
        {
            var baseUri = GetBaseUriFromCluster(cluster);
            var credentials = GetCredentials(secrets);
            return new JetfireClient(baseUri, credentials);
        }

        static ICredentials GetCredentials(ISecretsProvider secrets)
        {
            var apiKey = secrets.GetNamedSecret(ApiKeyEnvironmentVariable);
            var credentialsPath = secrets.GetNamedSecret(CredentialsEnvironmentVariable);
            if (!string.IsNullOrWhiteSpace(apiKey))
            {
                return new ApiKeyCredentials(apiKey);
            }
            else if (!string.IsNullOrEmpty(credentialsPath))
            {
                var config = ConfigurationUtils.Read<AuthenticatorConfig>(credentialsPath);
                return new TokenCredentials(config);
            }
            else
            {
                throw new JetfireCliException($"Either the {ApiKeyEnvironmentVariable} or {CredentialsEnvironmentVariable} environment variable must be set");
            }

        }

        static Uri GetBaseUriFromCluster(string cluster)
        {
            cluster = cluster ?? "europe-west1-1";
            return new Uri($"https://jetfire.{cluster}.cogniteapp.com");
        }
    }
}
