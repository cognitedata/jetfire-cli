// Copyright 2020 Cognite AS
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Cognite.Extractor.Configuration;
using Cognite.Extractor.Utils;
using Cognite.Jetfire.Api;

namespace Cognite.Jetfire.Cli
{
    public static class JetfireClientFactory
    {
        static readonly string ApiKeyEnvironmentVariable = "JETFIRE_API_KEY";
        static readonly string TokenUrlEnvironmentVariable = "JETFIRE_TOKEN_URL";
        static readonly string ClientIdEnvironmentVariable = "JETFIRE_CLIENT_ID";
        static readonly string ClientSecretEnvironmentVariable = "JETFIRE_CLIENT_SECRET";
        static readonly string TokenScopesEnvironmentVariable = "JETFIRE_TOKEN_SCOPES";

        public static IJetfireClient CreateClient(ISecretsProvider secrets, string cluster)
        {
            var baseUri = GetBaseUriFromCluster(cluster);
            var credentials = GetCredentials(secrets);
            return new JetfireClient(baseUri, credentials);
        }

        static ICredentials GetCredentials(ISecretsProvider secrets)
        {
            var apiKey = secrets.GetNamedSecret(ApiKeyEnvironmentVariable);

            var tokenUrl = secrets.GetNamedSecret(TokenUrlEnvironmentVariable);
            var clientId = secrets.GetNamedSecret(ClientIdEnvironmentVariable);
            var clientSecret = secrets.GetNamedSecret(ClientSecretEnvironmentVariable);
            var tokenScopes = secrets.GetNamedSecret(TokenScopesEnvironmentVariable);

            if (!string.IsNullOrWhiteSpace(apiKey))
            {
                return new ApiKeyCredentials(apiKey);
            }
            else if (!string.IsNullOrEmpty(tokenUrl) && !string.IsNullOrEmpty(clientId) && !string.IsNullOrEmpty(clientSecret) && !string.IsNullOrEmpty(tokenScopes))
            {
                var authConfig = new AuthenticatorConfig
                {
                    Implementation = AuthenticatorConfig.AuthenticatorImplementation.Basic,
                    ClientId = clientId,
                    Secret = clientSecret,
                    TokenUrl = tokenUrl,
                    Scopes = new List<string>(tokenScopes.Split(","))
                };
                return new TokenCredentials(authConfig);
            }
            else
            {
                throw new JetfireCliException($"Either the {ApiKeyEnvironmentVariable} environment variable, or the {TokenUrlEnvironmentVariable}, {ClientIdEnvironmentVariable}, {ClientSecretEnvironmentVariable} and {TokenScopesEnvironmentVariable} environment variables must be set");
            }

        }

        static Uri GetBaseUriFromCluster(string cluster)
        {
            cluster = cluster ?? "europe-west1-1";
            return new Uri($"https://jetfire.{cluster}.cogniteapp.com");
        }
    }
}
