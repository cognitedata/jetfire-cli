// Copyright 2020 Cognite AS
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Net.Http;
using Cognite.Jetfire.Api.Model;
using System.Text.Json;
using System.Threading.Tasks;

namespace Cognite.Jetfire.Api
{
    public interface IJetfireClient : IDisposable
    {
        Task<List<TransformConfigRead>> TransformConfigList();

        Task<TransformConfigId> TransformConfigCreate(TransformConfigCreate request);

        Task<TransformConfigRead> TransformConfigById(int id);

        Task<TransformConfigRead> TransformConfigByExternalId(string externalId);

        Task TransformConfigDelete(int id);

        Task<QueryResponse> Query(
            QueryRequest query,
            long? resultsLimit = null,
            long? inferSchemaLimit = null
        );

        Task TransformConfigUpdate(int id, TransformConfigUpdate request);

        Task TransformConfigUpdateSourceApiKey(int id, TransformConfigApiKeyUpdate request);

        Task TransformConfigUpdateDestinationApiKey(int id, TransformConfigApiKeyUpdate request);

        Task TransformConfigSetPublished(int id, TransformConfigPublishOptions request);

        Task<TransformJobId> TransformConfigStartJob(int id);

        Task<TransformJob[]> TransformConfigRecentJobs(int id);

        Task<MetricCounter[]> TransformJobMetrics(string jobId);

        Task ScheduleCreateOrUpdate(int id, ScheduleParams request);

        Task ScheduleDelete(int id);
    }

    public static class JetfireClientExtensions
    {
        public static Task TransformConfigUpdateSourceApiKey(this IJetfireClient client, int id, string newApiKey)
            => client.TransformConfigUpdateSourceApiKey(id, new TransformConfigApiKeyUpdate { ApiKey = newApiKey });

        public static Task TransformConfigUpdateDestinationApiKey(this IJetfireClient client, int id, string newApiKey)
            => client.TransformConfigUpdateDestinationApiKey(id, new TransformConfigApiKeyUpdate { ApiKey = newApiKey });

        public static Task TransformConfigSetPublished(this IJetfireClient client, int id, bool isPublic)
            => client.TransformConfigSetPublished(id, new TransformConfigPublishOptions { IsPublic = isPublic });

        public static Task<QueryResponse> Query(
            this IJetfireClient client,
            string query,
            long? resultsLimit = null,
            long? inferSchemaLimit = null
        ) => client.Query(
            new QueryRequest { Query = query },
            resultsLimit,
            inferSchemaLimit
        );
    }

    public class JetfireClient : JetfireBaseClient, IJetfireClient
    {
        public JetfireClient(Uri baseUri, ICredentials credentials)
            : base (baseUri, credentials)
        {
        }

        public Task<List<TransformConfigRead>> TransformConfigList()
        {
            return SendAsync<List<TransformConfigRead>>(
                HttpMethod.Get, "/api/transform/config?includePublic");
        }

        public Task<TransformConfigRead> TransformConfigById(int id)
        {
            return SendAsync<TransformConfigRead>(HttpMethod.Get, $"/api/transform/config/{id}");
        }

        public Task<TransformConfigRead> TransformConfigByExternalId(string externalId)
        {
            return SendAsync<TransformConfigRead>(HttpMethod.Get, $"/api/transform/configByExternalId/{externalId}");
        }

        public Task<TransformConfigId> TransformConfigCreate(TransformConfigCreate request)
        {
            return SendAsync<TransformConfigCreate, TransformConfigId>(
                HttpMethod.Post, "/api/transform/config", request);
        }

        public Task TransformConfigDelete(int id)
        {
            return SendAsync(HttpMethod.Delete, $"/api/transform/config/{id}");
        }

        public Task TransformConfigUpdate(int id, TransformConfigUpdate request)
        {
            return SendAsync(HttpMethod.Put, $"/api/transform/config/{id}", request);
        }

        public Task TransformConfigUpdateSourceApiKey(int id, TransformConfigApiKeyUpdate request)
        {
            return SendAsync(HttpMethod.Post, $"/api/transform/config/{id}/sourceApiKey", request);
        }

        public Task TransformConfigUpdateDestinationApiKey(int id, TransformConfigApiKeyUpdate request)
        {
            return SendAsync(HttpMethod.Post, $"/api/transform/config/{id}/destinationApiKey", request);
        }

        public Task TransformConfigSetPublished(int id, TransformConfigPublishOptions request)
        {
            return SendAsync(HttpMethod.Put, $"/api/transform/config/{id}/setPublished", request);
        }

        public Task<TransformJobId> TransformConfigStartJob(int id)
        {
            return SendAsync<TransformJobId>(HttpMethod.Post, $"/api/transform/config/run/{id}");
        }

        public Task<TransformJob[]> TransformConfigRecentJobs(int id)
        {
            return SendAsync<TransformJob[]>(HttpMethod.Get, $"/api/transform/jobDetails/{id}");
        }

        public Task<MetricCounter[]> TransformJobMetrics(string jobId)
        {
            return SendAsync<MetricCounter[]>(HttpMethod.Get, $"/api/transform/jobs/{jobId}/metrics");
        }

        public Task ScheduleCreateOrUpdate(int id, ScheduleParams request)
        {
            return SendAsync(HttpMethod.Post, $"/api/schedule/config/{id}", request);
        }

        public Task ScheduleDelete(int id)
        {
            return SendAsync(HttpMethod.Delete, $"/api/schedule/config/{id}");
        }

        public Task<QueryResponse> Query(
            QueryRequest request,
            long? resultsLimit = null,
            long? inferSchemaLimit = null
        )
        {
            var query = new Dictionary<string, string>();

            if (resultsLimit.HasValue)
            {
                query["resultsLimit"] = resultsLimit.ToString();
            }
            if (inferSchemaLimit.HasValue)
            {
                query["inferSchemaLimit"] = inferSchemaLimit.ToString();
            }

            return SendAsync<QueryRequest, QueryResponse>(
                HttpMethod.Post,
                "/api/query?" + MakeQueryString(query),
                request
            );
        }

        static string MakeQueryString(IEnumerable<KeyValuePair<string, string>> items)
        {
            using(var content = new FormUrlEncodedContent(items))
            {
                return content.ReadAsStringAsync().Result;
            }
        }
    }
}
