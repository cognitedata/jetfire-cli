using System;
using Cognite.Jetfire.Api.Model;

namespace Cognite.Jetfire.Cli.Deploy.Manifest
{
    public class OidcCredentials
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string[] Scopes { get; set; }
        public string TokenUrl { get; set; }
        public string CdfProjectName { get; set; }
        public bool lookup { get; set; }
    }

    public class ReadWriteOidcCredentials : OidcCredentials
    {
        private OidcCredentials read;
        public OidcCredentials Read
        {
            get
            {
                if (this.read != null)
                    return this.read;
                return this;
            }
            set { this.read = value; }
        }

        private OidcCredentials write;
        public OidcCredentials Write
        {
            get
            {
                if (this.write != null)
                    return this.write;
                return this;
            }
            set { this.write = value; }
        }
    }
}
