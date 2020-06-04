// Copyright 2020 Cognite AS
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace Cognite.Jetfire.Cli
{
    public class TestSecretsProvider : ISecretsProvider
    {
        IDictionary<string, string> secrets;

        public TestSecretsProvider(IDictionary<string, string> secrets)
        {
            this.secrets = secrets;
        }

        public TestSecretsProvider() : this(new Dictionary<string, string>()) {}

        public void Add(string name, string secret)
        {
            secrets.Add(name, secret);
        }

        public string this[string name]
        {
            get { return secrets[name]; }
            set { secrets[name] = value; }
        }

        public string GetNamedSecret(string name)
        {
            this.secrets.TryGetValue(name, out var secret);
            return secret;
        }
    }
}
