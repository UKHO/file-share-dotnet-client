﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using UKHO.FileShareClient.Internal;
using UKHO.FileShareClient.Models;

namespace UKHO.FileShareClient
{
    public class FileShareApiClient
    {
        protected readonly IHttpClientFactory httpClientFactory;

        public FileShareApiClient(IHttpClientFactory httpClientFactory, string baseAddress, string accessToken)
        {
            this.httpClientFactory = new AddAuthenticationHeaderHttpClientFactory(
                new SetBaseAddressHttpClientFactory(httpClientFactory, new Uri(baseAddress)),
                new System.Net.Http.Headers.AuthenticationHeaderValue("bearer", accessToken));
        }

        [ExcludeFromCodeCoverage] //This constructor is intended for external use with a real HTTP Client.
        public FileShareApiClient(string baseAddress, string accessToken) :
            this(new DefaultHttpClientFactory(), baseAddress, accessToken)
        {
        }


        public async Task<BatchStatusResponse> GetBatchStatusAsync(string batchId)
        {
            var uri = $"batch/{batchId}/status";

            using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, uri))
            {
                var response = await httpClientFactory.CreateClient()
                    .SendAsync(httpRequestMessage, CancellationToken.None);
                response.EnsureSuccessStatusCode();
                var status = await response.ReadAsTypeAsync<BatchStatusResponse>();
                return status;
            }
        }

        public async Task<BatchSearchResponse> Search(string searchQuery, int? pageSize = null, int? start = null)
        {
            var uri = "batch";

            var query = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(searchQuery))
                query["$filter"] = searchQuery;
            if (pageSize.HasValue)
            {
                if (pageSize <= 0)
                    throw new ArgumentException("Page size must be greater than zero.", nameof(pageSize));
                query["limit"] = pageSize.Value + "";
            }

            if (start.HasValue)
            {
                if (start < 0)
                    throw new ArgumentException("Start cannot be less than zero.", nameof(start));
                query["start"] = start.Value + "";
            }

            uri = AddQueryString(uri, query);

            using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, uri))
            {
                var response = await httpClientFactory.CreateClient()
                    .SendAsync(httpRequestMessage, CancellationToken.None);
                response.EnsureSuccessStatusCode();
                var searchResponse = await response.ReadAsTypeAsync<BatchSearchResponse>();
                return searchResponse;
            }
        }

        private static string AddQueryString(
            string uri,
            IEnumerable<KeyValuePair<string, string>> queryString)
        {
            var uriToBeAppended = uri;

            var queryIndex = uriToBeAppended.IndexOf('?');
            var hasQuery = queryIndex != -1;

            var sb = new StringBuilder();
            sb.Append(uriToBeAppended);
            foreach (var parameter in queryString)
            {
                sb.Append(hasQuery ? '&' : '?');
                sb.Append(UrlEncoder.Default.Encode(parameter.Key));
                sb.Append('=');
                sb.Append(UrlEncoder.Default.Encode(parameter.Value));
                hasQuery = true;
            }

            return sb.ToString();
        }
    }
}