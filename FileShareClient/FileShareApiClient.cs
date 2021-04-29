using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
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

        public async Task<BatchSearchResponse> Search(string searchQuery)
        {
            var uri = "batch";
            if (!string.IsNullOrEmpty(searchQuery)) uri+= "?$filter=" + searchQuery;

            using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, uri))
            {
                var response = await httpClientFactory.CreateClient()
                    .SendAsync(httpRequestMessage, CancellationToken.None);
                response.EnsureSuccessStatusCode();
                var searchResponse = await response.ReadAsTypeAsync<BatchSearchResponse>();
                return searchResponse;
            }
        }
    }
}