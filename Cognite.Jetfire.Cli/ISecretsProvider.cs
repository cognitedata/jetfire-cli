// Copyright 2020 Cognite AS
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Cognite.Jetfire.Cli
{
    public interface ISecretsProvider
    {
        string GetNamedSecret(string name);
    }
}
