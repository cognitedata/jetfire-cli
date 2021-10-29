// Copyright 2020 Cognite AS
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Cognite.Extractor.Common;
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
        static readonly string TokenAudienceEnvironmentVariable = "JETFIRE_TOKEN_AUDIENCE";

        static readonly string ProjectEnvironmentVariable = "JETFIRE_PROJECT";
        static readonly string ClusterEnvironmentVariable = "JETFIRE_CLUSTER";

        public static IJetfireClient CreateClient(ISecretsProvider secrets, string cluster)
        {
            var baseUri = GetBaseUriFromCluster(secrets, cluster);
            var credentials = GetCredentials(secrets);
            return new JetfireClient(baseUri, credentials);
        }

        static ICredentials GetCredentials(ISecretsProvider secrets)
        {
            var apiKey = secrets.GetNamedSecret(ApiKeyEnvironmentVariable).TrimToNull();

            var tokenUrl = secrets.GetNamedSecret(TokenUrlEnvironmentVariable).TrimToNull();
            var clientId = secrets.GetNamedSecret(ClientIdEnvironmentVariable).TrimToNull();
            var clientSecret = secrets.GetNamedSecret(ClientSecretEnvironmentVariable).TrimToNull();
            var tokenScopes = secrets.GetNamedSecret(TokenScopesEnvironmentVariable).TrimToNull();
            var tokenAudience = secrets.GetNamedSecret(TokenAudienceEnvironmentVariable).TrimToNull();
            var project = secrets.GetNamedSecret(ProjectEnvironmentVariable).TrimToNull();

            if (apiKey != null)
            {
                return new ApiKeyCredentials(apiKey);
            }
            else if (tokenUrl != null && clientId != null && clientSecret != null && project != null)
            {
                var scopeList = new List<string>();
                if (tokenScopes != null && tokenScopes != "")
                {
                    scopeList = new List<string>(tokenScopes.Split(","));
                }

                var authConfig = new AuthenticatorConfig
                {
                    Implementation = AuthenticatorConfig.AuthenticatorImplementation.Basic,
                    ClientId = clientId,
                    Secret = clientSecret,
                    TokenUrl = tokenUrl,
                    Scopes = scopeList,
                    Audience = tokenAudience
                };
                return new TokenCredentials(authConfig, project);
            }
            else
            {
                throw new JetfireCliException($"Either the {ApiKeyEnvironmentVariable} environment variable, or the {TokenUrlEnvironmentVariable}, {ClientIdEnvironmentVariable}, {ClientSecretEnvironmentVariable}, {TokenScopesEnvironmentVariable} and {ProjectEnvironmentVariable} environment variables must be set");
            }

        }

        static Uri GetBaseUriFromCluster(ISecretsProvider secrets, string cluster)
        {
            if (cluster == "localhost")
            {
                return new Uri("http://localhost:8084");
            }
            if (string.IsNullOrWhiteSpace(cluster))
            {
                cluster = secrets.GetNamedSecret(ClusterEnvironmentVariable) ?? "europe-west1-1";
            }
            return new Uri($"https://jetfire.{cluster}.cogniteapp.com");
        }
    }
}
