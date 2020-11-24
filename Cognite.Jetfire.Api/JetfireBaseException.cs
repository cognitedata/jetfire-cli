using System;
namespace Cognite.Jetfire.Api
{
    public class JetfireBaseException : Exception
    {
        public JetfireBaseException(string message) : base(message)
        {
        }
    }
}
