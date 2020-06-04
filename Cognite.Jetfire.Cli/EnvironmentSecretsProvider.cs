// Copyright 2020 Cognite AS
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Cognite.Jetfire.Cli
{
    public sealed class EnvironmentSecretsProvider : ISecretsProvider
    {
        public string GetNamedSecret(string name)
        {
            return Environment.GetEnvironmentVariable(name);
        }
    }
}
