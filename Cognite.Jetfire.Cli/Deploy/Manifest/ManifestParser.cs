// Copyright 2020 Cognite AS
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.IO;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Cognite.Jetfire.Cli.Deploy.Manifest
{
    public class ManifestParser
    {
        IDeserializer deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .WithNodeDeserializer(new DestinationShorthandDeserializer(), x => x.OnTop())
            .WithNodeDeserializer(new ApiKeyShorthandDeserializer(), x => x.OnTop())
            .Build();

        public IEnumerable<Transformation> ParseManifests(string yaml) =>
            ParseManifests(new StringReader(yaml));

        public IEnumerable<Transformation> ParseManifests(TextReader reader)
        {
            var parser = new Parser(reader);

            parser.Consume<StreamStart>();

            while (parser.Accept<DocumentStart>(out _))
            {
                yield return deserializer.Deserialize<Transformation>(parser);
            }
        }
    }

    public class DestinationShorthandDeserializer : INodeDeserializer
    {
        static readonly Type typeofDestination = typeof(Destination);

        public bool Deserialize(
            IParser reader,
            Type expectedType,
            Func<IParser, Type, object> nestedObjectDeserializer,
            out object value)
        {
            if (typeofDestination.IsAssignableFrom(expectedType) == false)
            {
                value = null;
                return false;
            }

            if (reader.TryConsume<Scalar>(out var scalar))
            {
                if (Enum.TryParse<DestinationType>(scalar.Value, ignoreCase: true, out var destinationType))
                {
                    value = new Destination { Type = destinationType };
                    return true;
                }
            }

            value = null;
            return false;
        }
    }

    public class ApiKeyShorthandDeserializer : INodeDeserializer
    {
        static readonly Type typeofApiKeys = typeof(ApiKeys);

        public bool Deserialize(
            IParser reader,
            Type expectedType,
            Func<IParser, Type, object> nestedObjectDeserializer,
            out object value)
        {
            if (typeofApiKeys.IsAssignableFrom(expectedType) == false)
            {
                value = null;
                return false;
            }

            if (reader.TryConsume<Scalar>(out var apiKey))
            {
                value = new ApiKeys { Read = apiKey.Value, Write = apiKey.Value };
                return true;
            }

            value = null;
            return false;
        }
    }
}
