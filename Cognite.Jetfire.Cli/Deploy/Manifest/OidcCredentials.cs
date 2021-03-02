using System;
using Cognite.Jetfire.Api.Model;

namespace Cognite.Jetfire.Cli.Deploy.Manifest
{
    public class OidcCredentials
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string[] Scopes { get; set; }
        public string TokenUri { get; set; }
        public string CdfProjectName { get; set; }
    }

    public class ReadWriteOidcCredentials : OidcCredentials
    {
        public OidcCredentials Read
        {
            get
            {
                if (Read == null)
                    return this;
                return Read;
            }
            set { Read = value; }
        }
        public OidcCredentials Write
        {
            get
            {
                if (Write == null)
                    return this;
                return Write;
            }
            set { Write = value; }
        }
    }
}
