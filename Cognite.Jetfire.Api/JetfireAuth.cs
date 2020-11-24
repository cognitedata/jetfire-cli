using System;
using System.Collections.Generic;
using System.Text.Json;
using Cognite.Extractor.Utils;
using Cognite.Jetfire.Api;

namespace Cognite.Jetfire.Api
{
    public class DualAuth
    {
        public SingleAuth ReadAuth { get; set; }
        public SingleAuth WriteAuth { get; set; }
    }

    public class SingleAuth
    {
        public string Type { get; set; }

        public string Authority { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public List<string> Scopes { get; set; }

        public string Key { get; set; }

        public SingleAuth(string authority, string clientId, string clientSecret, List<string> scopes)
        {
            Type = "client_credentials";

            Authority = authority;
            ClientId = clientId;
            ClientSecret = clientSecret;
            Scopes = scopes;
        }

        public SingleAuth(string apiKey)
        {
            Type = "api_key";

            Key = apiKey;
        }

        public ICredentials GetCredentials()
        {
            if (Type == "api_key")
            {
                return new ApiKeyCredentials(Key);
            }
            else if (Type == "client_credentials")
            {
                var config = new AuthenticatorConfig();
                config.Authority = Authority;
                config.Tenant = "";
                config.ClientId = ClientId;
                config.Secret = ClientSecret;
                config.Scopes = Scopes;

                return new TokenCredentials(config);
            }
            else
            {
                throw new JetfireBaseException($"Unknown auth method {Type}");
            }
        }
    }
}
