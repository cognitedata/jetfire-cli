// Copyright 2020 Cognite AS
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Net;
using System.Net.Http;

using Cognite.Jetfire.Api.Model;

namespace Cognite.Jetfire.Api
{
    public class JetfireApiException : JetfireBaseException
    {
        public HttpStatusCode StatusCode { get; }

        public HttpMethod Method { get; }

        public Uri Uri { get; }

        public JetfireApiError Error { get; }

        public JetfireApiException(
            HttpStatusCode statusCode,
            HttpMethod method,
            Uri uri,
            JetfireApiError error
        ) : base(MakeMessage(statusCode, method, uri, error))
        {
            StatusCode = statusCode;
            Method = method;
            Uri = uri;
            Error = error;
        }

        static string MakeMessage(
            HttpStatusCode status,
            HttpMethod method,
            Uri uri,
            JetfireApiError error
        )
        {
            var statusMessage = ((int)status).ToString();
            if (Enum.IsDefined(typeof(HttpStatusCode), status))
            {
                statusMessage += $" {status}";
            }
            var message = $"{method} {uri} failed with status {statusMessage}";
            if (error != null)
            {
                message += $": [{error.Type}] {error.Message}";
            }
            return message;
        }
    }
}
