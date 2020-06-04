// Copyright 2020 Cognite AS
// SPDX-License-Identifier: Apache-2.0

using System;
using Cognite.Jetfire.Api;

namespace Cognite.Jetfire
{
    public class TestEnvironment
    {
        public readonly Uri BaseUri;

        public readonly string ApiKey;

        public readonly JetfireClient Client;

        public TestEnvironment(Uri baseUri, string apiKey)
        {
            BaseUri = baseUri;
            ApiKey = apiKey;
            Client = new JetfireClient(baseUri, new ApiKeyCredentials(apiKey));
        }

        static string testCluster = Environment.GetEnvironmentVariable("JETFIRE_CLI_TEST_CLUSTER") ?? "europe-west1-1";

        public static readonly TestEnvironment Jetfiretest1 = new TestEnvironment(
            baseUri: new Uri($"https://jetfire.{testCluster}.cogniteapp.com"),

            apiKey: Environment.GetEnvironmentVariable("JETFIRE_CLI_TEST_API_KEY")
                ?? throw new Exception("To run tests, please set the JETFIRE_CLI_TEST_API_KEY environment variable")
        );
    }
}
