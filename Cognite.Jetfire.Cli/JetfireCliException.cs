using System;
namespace Cognite.Jetfire.Cli
{
    public class JetfireCliException : Exception
    {
        public JetfireCliException(string message) : base(message)
        { }
    }
}
