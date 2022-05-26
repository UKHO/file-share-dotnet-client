using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using UKHO.FileShareClient.Internal;
using UKHO.FileShareClient.Models;

namespace UKHO.FileShareClient
{
    public interface IFileShareApiClient
    {
        Task<BatchStatusResponse> GetBatchStatusAsync(string batchId);
        Task<BatchSearchResponse> Search(string searchQuery, int? pageSize = null, int? start = null);
        Task<IResult<BatchSearchResponse>> Search(string searchQuery , int? pageSize, int? start , CancellationToken cancellationToken);
        Task<IResult<BatchAttributesSearchResponse>> BatchAttributeSearch(string searchQuery, CancellationToken cancellationToken);
        Task<Stream> DownloadFileAsync(string batchId, string filename);
        Task<IResult<DownloadFileResponse>> DownloadFileAsync(string batchId, string fileName, Stream destinationStream, long fileSizeInBytes = 0, CancellationToken cancellationToken = default);

        Task<IEnumerable<string>> GetUserAttributesAsync();
        Task<Stream> DownloadZipFileAsync(string batchId, CancellationToken cancellationToken = default);
    }

    public class FileShareApiClient : IFileShareApiClient
    {
        protected readonly IHttpClientFactory httpClientFactory;
        protected readonly IAuthTokenProvider authTokenProvider;
        
        private int maxDownloadBytes = 10485760;

        public FileShareApiClient(IHttpClientFactory httpClientFactory, string baseAddress, IAuthTokenProvider authTokenProvider)
        {
            this.httpClientFactory = new SetBaseAddressHttpClientFactory(httpClientFactory, new Uri(baseAddress));
            this.authTokenProvider = authTokenProvider;
        }

        public FileShareApiClient(IHttpClientFactory httpClientFactory, string baseAddress, string accessToken)
            :this(httpClientFactory, baseAddress, new DefaultAuthTokenProvider(accessToken))
        {
        }

        [ExcludeFromCodeCoverage] //This constructor is intended for external use with a real HTTP Client.
        public FileShareApiClient(string baseAddress, string accessToken) :
            this(new DefaultHttpClientFactory(), baseAddress, accessToken)
        {
        }
        protected async Task<HttpClient> GetAuthenticationHeaderSetClient()
        {
            var httpClient = httpClientFactory.CreateClient();
            await httpClient.SetAuthenticationHeader(authTokenProvider);
            return httpClient;
        }

        public async Task<BatchStatusResponse> GetBatchStatusAsync(string batchId)
        {
            var uri = $"batch/{batchId}/status";

            using (var httpClient = await GetAuthenticationHeaderSetClient())
            using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, uri))
            {
                var response = await httpClient.SendAsync(httpRequestMessage, CancellationToken.None);
                response.EnsureSuccessStatusCode();
                var status = await response.ReadAsTypeAsync<BatchStatusResponse>();
                return status;
            }
        }

        public async Task<BatchSearchResponse> Search(string searchQuery, int? pageSize = null, int? start = null)
        {
            var response = await SearchResponse(searchQuery, pageSize, start, CancellationToken.None);
            response.EnsureSuccessStatusCode();
            var searchResponse = await response.ReadAsTypeAsync<BatchSearchResponse>();
            return searchResponse;
        }

        public async Task<IResult<BatchSearchResponse>> Search(string searchQuery, int? pageSize, int? start, CancellationToken cancellationToken )
        {
            var response = await SearchResponse(searchQuery, pageSize, start, cancellationToken);
            var result = new Result<BatchSearchResponse>();
            await result.ProcessHttpResponse(HttpStatusCode.OK, response);
            return result;
        }

        private async Task<HttpResponseMessage> SearchResponse(string searchQuery, int? pageSize, int? start , CancellationToken cancellationToken)
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

            using (var httpClient = await GetAuthenticationHeaderSetClient())
            using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, uri))
            {                
                return await httpClient.SendAsync(httpRequestMessage, cancellationToken);
            }
        }


        public async Task<Stream> DownloadFileAsync(string batchId, string filename)
        {
            var uri = $"batch/{batchId}/files/{filename}";

            using (var httpClient = await GetAuthenticationHeaderSetClient())
            using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, uri))
            {
                var response = await httpClient.SendAsync(httpRequestMessage, CancellationToken.None);
                response.EnsureSuccessStatusCode();
                var downloadedFileStream = await response.Content.ReadAsStreamAsync();
                return downloadedFileStream;
            }
        }

        public async Task<IResult<DownloadFileResponse>> DownloadFileAsync(string batchId, string fileName, Stream destinationStream, long fileSizeInBytes = 0, CancellationToken cancellationToken = default)
        {
            long startByte = 0;
            long endByte = fileSizeInBytes < maxDownloadBytes ? fileSizeInBytes - 1 : maxDownloadBytes-1;
            var result = new Result<DownloadFileResponse>();
            HttpStatusCode httpStatusCode = HttpStatusCode.OK;

            while (startByte <= endByte)
            {
                string rangeHeader = $"bytes={startByte}-{endByte}";

                var uri = $"batch/{batchId}/files/{fileName}";

                using (var httpClient = await GetAuthenticationHeaderSetClient())
                using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, uri))
                {
                    if (fileSizeInBytes != 0 && rangeHeader != null)
                    {
                        httpRequestMessage.Headers.Add("Range", rangeHeader);
                        httpStatusCode = HttpStatusCode.PartialContent;
                    }

                    var response = await httpClient.SendAsync(httpRequestMessage, cancellationToken);

                    await result.ProcessHttpResponse(httpStatusCode, response, true);
                    if (!result.IsSuccess) return result;

                    using (var contentStream = await response.Content.ReadAsStreamAsync())
                    {
                        contentStream.CopyTo(destinationStream);
                    }
                }
                startByte = endByte + 1;
                endByte += maxDownloadBytes-1;

                if (endByte > fileSizeInBytes - 1)
                {
                    endByte = fileSizeInBytes - 1;
                }

            }
            
            return result;
        }


        public async Task<IEnumerable<string>> GetUserAttributesAsync()
        {
            var uri = "attributes";

            using (var httpClient = await GetAuthenticationHeaderSetClient())
            using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, uri))
            {
                var response = await httpClient.SendAsync(httpRequestMessage, CancellationToken.None);
                response.EnsureSuccessStatusCode();
                var attributes = await response.ReadAsTypeAsync<List<string>>();
                return attributes;
            }
        }

        public async Task<IResult<BatchAttributesSearchResponse>> BatchAttributeSearch(string searchQuery, CancellationToken cancellationToken)
        {
            var uri = "attributes/search";
            var result = new Result<BatchAttributesSearchResponse>();
            HttpStatusCode httpStatusCode = HttpStatusCode.OK;

            var query = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(searchQuery))
                query["$filter"] = searchQuery;

            uri = AddQueryString(uri, query);

            using (var httpClient = await GetAuthenticationHeaderSetClient())
            using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, uri))
            {
                var response = await httpClient.SendAsync(httpRequestMessage, cancellationToken);
                await result.ProcessHttpResponse(httpStatusCode, response);
            }
            return result;
        }

        public async Task<Stream> DownloadZipFileAsync(string batchId, CancellationToken cancellationToken)
        {
            var uri = $"batch/{batchId}/files";

            using (var httpClient = await GetAuthenticationHeaderSetClient())
            using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, uri))
            {
                var response = await httpClient.SendAsync(httpRequestMessage, CancellationToken.None);
                response.EnsureSuccessStatusCode();
                var downloadedFileStream = await response.Content.ReadAsStreamAsync();
                return downloadedFileStream;
            }
        }

        #region private methods
        private static string AddQueryString(string uri, IEnumerable<KeyValuePair<string, string>> queryString)
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
        #endregion

    }
}