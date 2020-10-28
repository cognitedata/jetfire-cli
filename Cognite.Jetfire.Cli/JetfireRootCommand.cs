// Copyright 2020 Cognite AS
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Cognite.Jetfire.Cli
{
    public class JetfireRootCommand
    {
        public JetfireRootCommand(IEnumerable<IJetfireSubCommand> subCommands)
        {
            Command = new RootCommand
            {
                new Option<string>(
                    alias: "--cluster",
                    description: "The CDF cluster where Jetfire is hosted (e.g. greenfield, europe-west1-1)",
                    getDefaultValue: () => "europe-west1-1"
                )
            };

            foreach (var subCommand in subCommands)
            {
                Command.Add(subCommand.Command);
            }
        }

        public RootCommand Command { get; }
    }
}
