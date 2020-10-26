using System;
using System.CommandLine;

namespace Cognite.Jetfire.Cli
{
    public interface IJetfireSubCommand
    {
        Command Command { get; }
    }
}
