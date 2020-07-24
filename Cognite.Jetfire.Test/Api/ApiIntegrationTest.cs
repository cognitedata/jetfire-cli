// Copyright 2020 Cognite AS
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

using Cognite.Jetfire.Api.Model;

namespace Cognite.Jetfire.Api
{
    public class ApiIntegrationTest
    {
        JetfireClient client = TestEnvironment.Jetfiretest1.Client;

        [Fact]
        public async Task TransformConfigListTest()
        {
            var listResponse = await client.TransformConfigList();
            Assert.True(listResponse.Count > 0);
        }

        [Fact]
        public async Task CreateTest()
        {
            var createRequest = new TransformConfigCreate
            {
                ExternalId = "jetfire-cli-create-test",
                Name = "Test: Create from C# API",
                Query = "Foo",
            };

            var configId = await client.TransformConfigCreate(createRequest);

            try
            {
                Assert.True(configId.Id > 0);

                var list = await client.TransformConfigList();
                var config = Assert.Single(list.Where(x => x.Id == configId.Id));

                Assert.Equal(createRequest.Name, config.Name);
                Assert.Equal(createRequest.Query, config.Query);
            }
            finally
            {
                await client.TransformConfigDelete(configId.Id);
            }
        }

        [Fact]
        public async Task FailedAuthShouldThrowJetfireApiException()
        {
            var invalidCredentials = new ApiKeyCredentials("Not a valid API key");
            var invalidAuthClient = new JetfireClient(TestEnvironment.Jetfiretest1.BaseUri, invalidCredentials);
            var exception = await Assert.ThrowsAsync<JetfireApiException>(() => invalidAuthClient.TransformConfigList());
            Assert.Equal(HttpMethod.Get, exception.Method);
            Assert.Equal("/api/transform/config?includePublic", exception.Uri.PathAndQuery);
            Assert.Equal(HttpStatusCode.Unauthorized, exception.StatusCode);
            Assert.Equal("auth", exception.Error.Type);
            Assert.NotNull(exception.Error.Message);
        }

        [Fact]
        public async Task BodyIsNullIfNotPresent()
        {
            var exception = await Assert.ThrowsAsync<JetfireApiException>(
                () => client.SendAsync(HttpMethod.Get, "/nowhere"));

            Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
            Assert.Null(exception.Error);
        }

        [Fact]
        public async Task QueryTest()
        {
            var result = await client.Query(new QueryRequest
            {
                Query = "select 123 as firstColumn, 'test' as secondColumn"
            });
            Assert.Collection(
                result.Schema,
                col => {
                    Assert.Equal("INT", col.SqlType);
                    Assert.Equal("firstColumn", col.Name);
                },
                col => {
                    Assert.Equal("STRING", col.SqlType);
                    Assert.Equal("secondColumn", col.Name);
                }
            );

            var row = Assert.Single(result.Results);
            Assert.Equal("123", row["firstColumn"]);
            Assert.Equal("test", row["secondColumn"]);
        }

        [Fact]
        public async Task QueryLimitTest()
        {
            var request = new QueryRequest { Query = "select * from testdb.futureEvents" };
            var result = await client.Query(request, resultsLimit: 1);
            Assert.Single(result.Results);
        }

        [Fact]
        public async Task ExternalIdTest()
        {
            var createRequest = new TransformConfigCreate
            {
                ExternalId = "Foo",
                Name = "Test: Create from C# API",
                Query = "Foo",
            };

            var configId = await client.TransformConfigCreate(createRequest);

            try
            {
                Assert.True(configId.Id > 0);

                var list = await client.TransformConfigList();
                var config = Assert.Single(list.Where(x => x.Id == configId.Id));

                Assert.Equal(createRequest.Name, config.Name);
                Assert.Equal(createRequest.Query, config.Query);

                var duplicateExtIdException = await Assert.ThrowsAsync<JetfireApiException>(
                    () => client.TransformConfigCreate(createRequest));
                Assert.Equal(HttpMethod.Post, duplicateExtIdException.Method);
                Assert.Equal(HttpStatusCode.BadRequest, duplicateExtIdException.StatusCode);
                Assert.Equal("Duplicate externalId is not allowed", duplicateExtIdException.Error.Message);

                var invalidExternalIdConfig = new TransformConfigCreate{ ExternalId = "  foo  bar ", Name = "Test: Create from C# API", Query = "Foo" };
                var invalidExternalId = await Assert.ThrowsAsync<JetfireApiException>(
                    () => client.TransformConfigCreate(invalidExternalIdConfig));
                Assert.Equal(HttpMethod.Post, invalidExternalId.Method);
                Assert.Equal(HttpStatusCode.BadRequest, invalidExternalId.StatusCode);
                Assert.Equal("ExternalId is invalid", invalidExternalId.Error.Message);

                invalidExternalIdConfig.ExternalId = "foo?bar";
                invalidExternalId = await Assert.ThrowsAsync<JetfireApiException>(
                    () => client.TransformConfigCreate(invalidExternalIdConfig));
                Assert.Equal(HttpMethod.Post, invalidExternalId.Method);
                Assert.Equal(HttpStatusCode.BadRequest, invalidExternalId.StatusCode);
                Assert.Equal("ExternalId is invalid", invalidExternalId.Error.Message);
            }
            finally
            {
                await client.TransformConfigDelete(configId.Id);
            }
        }
    }
}
