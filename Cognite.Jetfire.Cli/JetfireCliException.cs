using System;
using Cognite.Jetfire.Api;

namespace Cognite.Jetfire.Cli
{
    public class JetfireCliException : JetfireBaseException
    {
        public JetfireCliException(string message) : base(message)
        { }
    }
}
