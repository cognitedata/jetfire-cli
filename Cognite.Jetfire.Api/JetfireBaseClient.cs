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
    public class JetfireBaseClient : IDisposable
    {
        protected readonly ICredentials Credentials;

        protected readonly HttpClient Http;

        public JetfireBaseClient(Uri baseUri, ICredentials credentials)
        {
            Credentials = credentials;
            Http = new HttpClient
            {
                BaseAddress = baseUri,
                Timeout = TimeSpan.FromMinutes(6),
            };
        }

        public Task<TRes> SendAsync<TRes>(HttpMethod method, string uri, CancellationToken ct = default)
        {
            return SendAsync<TRes>(new HttpRequestMessage(method, uri), ct);
        }

        public Task<TRes> SendAsync<TReq, TRes>(HttpMethod method, string uri, TReq body, CancellationToken ct = default)
        {
            return SendAsync<TReq, TRes>(new HttpRequestMessage(method, uri), body, ct);
        }

        public Task<HttpResponseMessage> SendAsync(HttpMethod method, string uri, CancellationToken ct)
        {
            return SendAsync(method, uri, body: null, ct);
        }

        public Task<HttpResponseMessage> SendAsync(HttpMethod method, string uri, object body = null, CancellationToken ct = default)
        {
            var request = new HttpRequestMessage(method, uri);
            if (body != null)
            {
                var json = JsonSerializer.Serialize(body, JsonConfig.SerializerOptions);
                request.Content = new StringContent(json);
            }
            return SendAsync(request, ct);
        }

        public async Task<TRes> SendAsync<TReq, TRes>(HttpRequestMessage request, TReq body, CancellationToken ct = default)
        {
            var json = JsonSerializer.Serialize(body, JsonConfig.SerializerOptions);
            request.Content = new StringContent(json);
            return await SendAsync<TRes>(request, ct);
        }

        public async Task<TRes> SendAsync<TRes>(HttpRequestMessage request, CancellationToken ct = default)
        {
            var response = await SendAsync(request, ct);
            using (var responseStream = await response.Content.ReadAsStreamAsync())
            {
                return await JsonSerializer.DeserializeAsync<TRes>(responseStream, JsonConfig.SerializerOptions, ct);
            }
        }

        public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct = default)
        {
            Credentials.Apply(request);

            var response = await Http.SendAsync(request, ct);

            if (!response.IsSuccessStatusCode)
            {
                JetfireApiError error = null;
                try
                {
                    var body = await response.Content.ReadAsStringAsync();
                    error = JsonSerializer.Deserialize<JetfireApiError>(body, JsonConfig.SerializerOptions);
                }
                catch (Exception) {}

                throw new JetfireApiException(
                    response.StatusCode,
                    request.Method,
                    request.RequestUri,
                    error
                );
            }

            return response;
        }

        public void Dispose()
        {
            Http.Dispose();
        }
    }
}
