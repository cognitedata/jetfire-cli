// Copyright 2020 Cognite AS
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Net.Http;
using Cognite.Jetfire.Api.Model;
using System.Text.Json;
using System.Threading.Tasks;
using System.Threading;

namespace Cognite.Jetfire.Api
{
    public interface IJetfireClient : IDisposable
    {
        Task<List<TransformConfigRead>> TransformConfigList(CancellationToken ct = default);

        Task<TransformConfigId> TransformConfigCreate(TransformConfigCreate request, CancellationToken ct = default);

        Task<TransformConfigRead> TransformConfigById(int id, CancellationToken ct = default);

        Task<TransformConfigRead> TransformConfigByExternalId(string externalId, CancellationToken ct = default);

        Task TransformConfigDelete(int id, CancellationToken ct = default);

        Task<QueryResponse> Query(
            QueryRequest query,
            long? resultsLimit = null,
            long? inferSchemaLimit = null,
            CancellationToken ct = default
        );

        Task TransformConfigUpdate(
            int id,
            TransformConfigUpdate request,
            CancellationToken ct = default
        );

        Task TransformConfigUpdateSourceApiKey(
            int id,
            TransformConfigApiKeyUpdate request,
            CancellationToken ct = default
        );

        Task TransformConfigUpdateDestinationApiKey(
            int id,
            TransformConfigApiKeyUpdate request,
            CancellationToken ct = default
        );

        Task TransformConfigUpdateSourceCredentials(
            int id,
            FlatOidcCredentials request,
            CancellationToken ct = default
        );

        Task TransformConfigUpdateDestinationCredentials(
            int id,
            FlatOidcCredentials request,
            CancellationToken ct = default
        );

        Task TransformConfigSetPublished(
            int id,
            TransformConfigPublishOptions request,
            CancellationToken ct = default
        );

        Task<TransformJobId> TransformConfigStartJob(int id, CancellationToken ct = default);

        Task<TransformJob[]> TransformConfigRecentJobs(int id, int limit = 1000, CancellationToken ct = default);

        Task<MetricCounter[]> TransformJobMetrics(string jobId, CancellationToken ct = default);

        Task ScheduleCreateOrUpdate(int id, ScheduleParams request, CancellationToken ct = default);

        Task ScheduleDelete(int id, CancellationToken ct = default);

        Task NotificationCreate(int configId, string destination, CancellationToken ct = default);

        Task<NotificationRead[]> NotificationList(int configId, CancellationToken ct = default);

        Task NotificationDelete(int configId, int notificationId, CancellationToken ct = default);
    }

    public static class JetfireClientExtensions
    {
        public static Task TransformConfigUpdateSourceApiKey(
            this IJetfireClient client,
            int id,
            string newApiKey,
            CancellationToken ct = default
        ) => client.TransformConfigUpdateSourceApiKey(id, new TransformConfigApiKeyUpdate { ApiKey = newApiKey });

        public static Task TransformConfigUpdateDestinationApiKey(
            this IJetfireClient client,
            int id,
            string newApiKey,
            CancellationToken ct = default
        ) => client.TransformConfigUpdateDestinationApiKey(id, new TransformConfigApiKeyUpdate { ApiKey = newApiKey });

        public static Task TransformConfigSetPublished(
            this IJetfireClient client,
            int id,
            bool isPublic,
            CancellationToken ct = default
        ) => client.TransformConfigSetPublished(id, new TransformConfigPublishOptions { IsPublic = isPublic });

        public static Task<QueryResponse> Query(
            this IJetfireClient client,
            string query,
            long? resultsLimit = null,
            long? inferSchemaLimit = null,
            CancellationToken ct = default
        ) => client.Query(
            new QueryRequest { Query = query },
            resultsLimit,
            inferSchemaLimit
        );
    }

    public class JetfireClient : JetfireBaseClient, IJetfireClient
    {
        public JetfireClient(Uri baseUri, ICredentials credentials)
            : base(baseUri, credentials)
        {
        }

        public Task<List<TransformConfigRead>> TransformConfigList(CancellationToken ct = default)
            => SendAsync<List<TransformConfigRead>>(HttpMethod.Get, "/api/transform/config?includePublic", ct);

        public Task<TransformConfigRead> TransformConfigById(int id, CancellationToken ct = default)
            => SendAsync<TransformConfigRead>(HttpMethod.Get, $"/api/transform/config/{id}", ct);

        public Task<TransformConfigRead> TransformConfigByExternalId(string externalId, CancellationToken ct = default)
            => SendAsync<TransformConfigRead>(HttpMethod.Get, $"/api/transform/configByExternalId/{externalId}", ct);

        public Task<TransformConfigId> TransformConfigCreate(TransformConfigCreate request, CancellationToken ct = default)
            => SendAsync<TransformConfigCreate, TransformConfigId>(HttpMethod.Post, "/api/transform/config", request, ct);

        public Task TransformConfigDelete(int id, CancellationToken ct = default)
            => SendAsync(HttpMethod.Delete, $"/api/transform/config/{id}", ct);

        public Task TransformConfigUpdate(int id, TransformConfigUpdate request, CancellationToken ct = default)
            => SendAsync(HttpMethod.Put, $"/api/transform/config/{id}", request, ct);

        public Task TransformConfigUpdateSourceApiKey(
            int id,
            TransformConfigApiKeyUpdate request,
            CancellationToken ct = default
        ) => SendAsync(HttpMethod.Post, $"/api/transform/config/{id}/sourceApiKey", request, ct);

        public Task TransformConfigUpdateDestinationApiKey(
            int id,
            TransformConfigApiKeyUpdate request,
            CancellationToken ct = default
        ) => SendAsync(HttpMethod.Post, $"/api/transform/config/{id}/destinationApiKey", request, ct);

        public Task TransformConfigUpdateSourceCredentials(
            int id,
            FlatOidcCredentials request,
            CancellationToken ct = default
        ) => SendAsync(HttpMethod.Post, $"/api/transform/config/{id}/sourceOidcCredentials", request, ct);

        public Task TransformConfigUpdateDestinationCredentials(
            int id,
            FlatOidcCredentials request,
            CancellationToken ct = default
        ) => SendAsync(HttpMethod.Post, $"/api/transform/config/{id}/destinationOidcCredentials", request, ct);

        public Task TransformConfigSetPublished(
            int id,
            TransformConfigPublishOptions request,
            CancellationToken ct = default
        ) => SendAsync(HttpMethod.Put, $"/api/transform/config/{id}/setPublished", request, ct);

        public Task<TransformJobId> TransformConfigStartJob(int id, CancellationToken ct = default)
            => SendAsync<TransformJobId>(HttpMethod.Post, $"/api/transform/config/run/{id}", ct);

        public Task<TransformJob[]> TransformConfigRecentJobs(int id, int limit = 1000, CancellationToken ct = default)
            => SendAsync<TransformJob[]>(HttpMethod.Get, $"/api/transform/jobDetails/{id}?={limit}", ct);

        public Task<MetricCounter[]> TransformJobMetrics(string jobId, CancellationToken ct = default)
            => SendAsync<MetricCounter[]>(HttpMethod.Get, $"/api/transform/jobs/{jobId}/metrics", ct);

        public Task ScheduleCreateOrUpdate(int id, ScheduleParams request, CancellationToken ct = default)
            => SendAsync(HttpMethod.Post, $"/api/schedule/config/{id}", request, ct);

        public Task ScheduleDelete(int id, CancellationToken ct = default)
            => SendAsync(HttpMethod.Delete, $"/api/schedule/config/{id}", ct);

        public Task NotificationCreate(int configId, string destination, CancellationToken ct = default)
            => SendAsync(HttpMethod.Post, $"/api/transform/config/{configId}/notifications", new NotificationCreate(destination), ct);

        public Task<NotificationRead[]> NotificationList(int configId, CancellationToken ct = default)
            => SendAsync<NotificationRead[]>(HttpMethod.Get, $"/api/transform/config/{configId}/notifications", ct);

        public Task NotificationDelete(int configId, int notificationId, CancellationToken ct = default)
            => SendAsync(HttpMethod.Delete, $"/api/transform/config/{configId}/notifications/{notificationId}", ct);

        public Task<QueryResponse> Query(
            QueryRequest request,
            long? resultsLimit = null,
            long? inferSchemaLimit = null,
            CancellationToken ct = default
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
                request,
                ct
            );
        }

        static string MakeQueryString(IEnumerable<KeyValuePair<string, string>> items)
        {
            using (var content = new FormUrlEncodedContent(items))
            {
                return content.ReadAsStringAsync().Result;
            }
        }
    }
}
