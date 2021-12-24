﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UKHO.FileShareAdminClient.Models;
using UKHO.FileShareAdminClient.Models.Response;
using UKHO.FileShareClient;
using UKHO.FileShareClient.Internal;
using UKHO.FileShareClient.Models;

namespace UKHO.FileShareAdminClient
{
    public interface IFileShareApiAdminClient : IFileShareApiClient
    {
        Task<IResult<AppendAclResponse>> AppendAclAsync(string batchId, Acl acl, CancellationToken cancellationToken = default);
        Task<IBatchHandle> CreateBatchAsync(BatchModel batchModel);
        Task<IResult<CreateBatchResponse>> CreateBatchAsync(BatchModel batchModel, CancellationToken cancellationToken = default);
        Task<BatchStatusResponse> GetBatchStatusAsync(IBatchHandle batchHandle);
        Task AddFileToBatch(IBatchHandle batchHandle, Stream stream, string fileName, string mimeType,
            params KeyValuePair<string, string>[] fileAttributes);
        Task<IResult<AddFileToBatchResponse>> AddFileToBatch(IBatchHandle batchHandle, Stream stream, string fileName, string mimeType, CancellationToken cancellationToken = default,
            params KeyValuePair<string, string>[] fileAttributes);
        Task AddFileToBatch(IBatchHandle batchHandle, Stream stream, string fileName, string mimeType,
            Action<(int blocksComplete, int totalBlockCount)> progressUpdate,
            params KeyValuePair<string, string>[] fileAttributes);
        Task<IResult<AddFileToBatchResponse>> AddFileToBatch(IBatchHandle batchHandle, Stream stream, string fileName, string mimeType,
            Action<(int blocksComplete, int totalBlockCount)> progressUpdate, CancellationToken cancellationToken = default,
            params KeyValuePair<string, string>[] fileAttributes);
        Task CommitBatch(IBatchHandle batchHandle);
        Task<IResult<CommitBatchResponse>> CommitBatch(IBatchHandle batchHandle, CancellationToken cancellationToken = default);
        Task<IResult<ReplaceAclResponse>> ReplaceAclAsync(string batchId, Acl acl, CancellationToken cancellationToken = default);
        Task RollBackBatchAsync(IBatchHandle batchHandle);
        Task<IResult<RollBackBatchResponse>> RollBackBatchAsync(IBatchHandle batchHandle, CancellationToken cancellationToken = default);
        Task<IResult<SetExpiryDateResponse>> SetExpiryDateAsync(string batchId, BatchExpiryModel batchExpiry, CancellationToken cancellationToken = default);
    }

    public class FileShareApiAdminClient : FileShareApiClient, IFileShareApiAdminClient
    {
        private readonly int maxFileBlockSize;

        public FileShareApiAdminClient(IHttpClientFactory httpClientFactory, string baseAddress, string accessToken,
            int maxFileBlockSize = 4194304) : base(httpClientFactory, baseAddress, accessToken)
        {
            this.maxFileBlockSize = maxFileBlockSize;
        }

        public FileShareApiAdminClient(string baseAddress, string accessToken, int maxFileBlockSize = 4194304) : base(
            baseAddress, accessToken)
        {
            this.maxFileBlockSize = maxFileBlockSize;
        }

        public FileShareApiAdminClient(IHttpClientFactory httpClientFactory, string baseAddress, IAuthTokenProvider authTokenProvider,
            int maxFileBlockSize = 4194304) : base(httpClientFactory, baseAddress, authTokenProvider)
        {
            this.maxFileBlockSize = maxFileBlockSize;
        }

        public async Task<IResult<AppendAclResponse>> AppendAclAsync(string batchId, Acl acl,
            CancellationToken cancellationToken = default)
                => await SendResult<Acl, AppendAclResponse>($"batch/{batchId}/acl", HttpMethod.Post, acl, cancellationToken, HttpStatusCode.NoContent);

        public async Task<IBatchHandle> CreateBatchAsync(BatchModel batchModel)
        {
            const string uri = "batch";
            var payloadJson = JsonConvert.SerializeObject(batchModel,
                new IsoDateTimeConverter { DateTimeFormat = "yyyy-MM-ddTHH:mm:ss.fffK" });

            using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, uri)
            {
                Content = new StringContent(payloadJson, Encoding.UTF8, "application/json")
            })
            {
                var httpClient = await GetAuthenticationHeaderSetClient();
                var response = await httpClient.SendAsync(httpRequestMessage);
                response.EnsureSuccessStatusCode();

                var data = await response.ReadAsTypeAsync<CreateBatchResponseModel>();
                var batchId = data.BatchId;

                return new BatchHandle(batchId);
            }
        }

        public async Task<IResult<CreateBatchResponse>> CreateBatchAsync(BatchModel batchModel, CancellationToken cancellationToken = default)
            => await SendResult<BatchModel, CreateBatchResponse>($"batch", HttpMethod.Post, batchModel, cancellationToken, HttpStatusCode.Created);


        public Task<BatchStatusResponse> GetBatchStatusAsync(IBatchHandle batchHandle)
        {
            return GetBatchStatusAsync(batchHandle.BatchId);
        }

        public Task AddFileToBatch(IBatchHandle batchHandle, Stream stream, string fileName, string mimeType,
            params KeyValuePair<string, string>[] fileAttributes)
        {
            return AddFileToBatch(batchHandle, stream, fileName, mimeType, _ => { }, CancellationToken.None, fileAttributes);
        }

        public Task<IResult<AddFileToBatchResponse>> AddFileToBatch(IBatchHandle batchHandle, Stream stream, string fileName, string mimeType, CancellationToken cancellationToken = default,
            params KeyValuePair<string, string>[] fileAttributes)
        {
            return AddFileToBatch(batchHandle, stream, fileName, mimeType, _ => { }, cancellationToken, fileAttributes);
        }

        public async Task AddFileToBatch(IBatchHandle batchHandle, Stream stream, string fileName, string mimeType,
            Action<(int blocksComplete, int totalBlockCount)> progressUpdate,
            params KeyValuePair<string, string>[] fileAttributes)
        {
            await AddFile(batchHandle, stream, fileName, mimeType, progressUpdate, CancellationToken.None, fileAttributes);
        }

        public async Task<IResult<AddFileToBatchResponse>> AddFileToBatch(IBatchHandle batchHandle, Stream stream, string fileName, string mimeType,
            Action<(int blocksComplete, int totalBlockCount)> progressUpdate, CancellationToken cancellationToken = default,
            params KeyValuePair<string, string>[] fileAttributes)
        {
            return await AddFiles(batchHandle, stream, fileName, mimeType, progressUpdate, cancellationToken, fileAttributes);
        }

        public async Task CommitBatch(IBatchHandle batchHandle)
        {
            var uri = $"/batch/{batchHandle.BatchId}";
            var batchCommitModel = new BatchCommitModel
            {
                FileDetails = ((BatchHandle)batchHandle).FileDetails
            };

            var payloadJson = JsonConvert.SerializeObject(batchCommitModel.FileDetails);

            using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Put, uri)
            { Content = new StringContent(payloadJson, Encoding.UTF8, "application/json") })
            {
                var httpClient = await GetAuthenticationHeaderSetClient();
                var response = await httpClient.SendAsync(httpRequestMessage, CancellationToken.None);
                response.EnsureSuccessStatusCode();
            }
        }

        public async Task<IResult<CommitBatchResponse>> CommitBatch(IBatchHandle batchHandle, CancellationToken cancellationToken = default)
        {
            var uri = $"/batch/{batchHandle.BatchId}";
            var batchCommitModel = new BatchCommitModel
            {
                FileDetails = ((BatchHandle)batchHandle).FileDetails
            };
            return await SendResult<List<FileDetail>, CommitBatchResponse>(uri, HttpMethod.Put, batchCommitModel.FileDetails, cancellationToken,
                HttpStatusCode.Accepted);
        }

        public async Task<IResult<ReplaceAclResponse>> ReplaceAclAsync(string batchId, Acl acl,
            CancellationToken cancellationToken = default)
                => await SendResult<Acl, ReplaceAclResponse>($"batch/{batchId}/acl", HttpMethod.Put, acl, cancellationToken, HttpStatusCode.NoContent);

        public async Task RollBackBatchAsync(IBatchHandle batchHandle)
        {
            var uri = $"batch/{batchHandle.BatchId}";

            using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Delete, uri))
            {
                var httpClient = await GetAuthenticationHeaderSetClient();
                var response = await httpClient.SendAsync(httpRequestMessage, CancellationToken.None);
                response.EnsureSuccessStatusCode();
            }
        }

        public async Task<IResult<RollBackBatchResponse>> RollBackBatchAsync(IBatchHandle batchHandle, CancellationToken cancellationToken = default)
            => await SendResult<IBatchHandle, RollBackBatchResponse>($"batch/{batchHandle.BatchId}", HttpMethod.Delete, null,
                cancellationToken, HttpStatusCode.NoContent);

        public async Task<IResult<SetExpiryDateResponse>> SetExpiryDateAsync(string batchId, BatchExpiryModel batchExpiry,
             CancellationToken cancellationToken = default)
                => await SendResult<BatchExpiryModel, SetExpiryDateResponse>($"batch/{batchId}/expiry", HttpMethod.Put, batchExpiry,
                    cancellationToken, HttpStatusCode.NoContent);


        #region Private methods

        private async Task AddFile(IBatchHandle batchHandle, Stream stream, string fileName, string mimeType,
            Action<(int blocksComplete, int totalBlockCount)> progressUpdate, CancellationToken cancellationToken,
            params KeyValuePair<string, string>[] fileAttributes)
        {
            if (!stream.CanSeek)
                throw new ArgumentException("The stream must be seekable.", nameof(stream));
            stream.Seek(0, SeekOrigin.Begin);

            var fileUri = $"batch/{batchHandle.BatchId}/files/{fileName}";

            {
                var fileModel = new FileModel()
                { Attributes = fileAttributes ?? Enumerable.Empty<KeyValuePair<string, string>>() };

                var payloadJson = JsonConvert.SerializeObject(fileModel);

                using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, fileUri)
                { Content = new StringContent(payloadJson, Encoding.UTF8, "application/json") })
                {
                    httpRequestMessage.Headers.Add("X-Content-Size", "" + stream.Length);

                    if (!string.IsNullOrEmpty(mimeType)) httpRequestMessage.Headers.Add("X-MIME-Type", mimeType);

                    var httpClient = await GetAuthenticationHeaderSetClient();
                    var createFileRecordResponse = await httpClient.SendAsync(httpRequestMessage, cancellationToken);
                    createFileRecordResponse.EnsureSuccessStatusCode();
                }
            }

            var md5Hash = stream.CalculateMD5();
            stream.Seek(0, SeekOrigin.Begin);

            var fileBlocks = new List<string>();
            var fileBlockId = 0;
            var expectedTotalBlockCount = (int)Math.Ceiling(stream.Length / (double)maxFileBlockSize);
            progressUpdate((0, expectedTotalBlockCount));

            var buffer = new byte[maxFileBlockSize];
            while (true)

            {
                fileBlockId++;
                var ms = new MemoryStream();

                var read = stream.Read(buffer, 0, maxFileBlockSize);
                if (read <= 0) break;
                ms.Write(buffer, 0, read);

                var fileBlockIdAsString = fileBlockId.ToString("D5");
                var putFileUri = $"batch/{batchHandle.BatchId}/files/{fileName}/{fileBlockIdAsString}";
                fileBlocks.Add(fileBlockIdAsString);
                ms.Seek(0, SeekOrigin.Begin);

                var blockMD5 = ms.CalculateMD5();

                using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Put, putFileUri)
                { Content = new StreamContent(ms) })
                {
                    httpRequestMessage.Content.Headers.ContentType =
                        new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");

                    httpRequestMessage.Content.Headers.ContentMD5 = blockMD5;

                    var httpClient = await GetAuthenticationHeaderSetClient();
                    var putFileResponse = await httpClient.SendAsync(httpRequestMessage, cancellationToken);
                    putFileResponse.EnsureSuccessStatusCode();

                    progressUpdate((fileBlockId, expectedTotalBlockCount));
                }
            }

            {
                var writeBlockFileModel = new WriteBlockFileModel { BlockIds = fileBlocks };
                var payloadJson = JsonConvert.SerializeObject(writeBlockFileModel);

                using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Put, fileUri)
                { Content = new StringContent(payloadJson, Encoding.UTF8, "application/json") })
                {
                    var httpClient = await GetAuthenticationHeaderSetClient();
                    var writeFileResponse = await httpClient.SendAsync(httpRequestMessage, cancellationToken);
                    writeFileResponse.EnsureSuccessStatusCode();
                }
            }
            ((BatchHandle)batchHandle).AddFile(fileName, Convert.ToBase64String(md5Hash));
        }
        private async Task<IResult<AddFileToBatchResponse>> AddFiles(IBatchHandle batchHandle, Stream stream, string fileName, string mimeType,
            Action<(int blocksComplete, int totalBlockCount)> progressUpdate, CancellationToken cancellationToken,
            params KeyValuePair<string, string>[] fileAttributes)
        {
            if (!stream.CanSeek)
                throw new ArgumentException("The stream must be seekable.", nameof(stream));
            stream.Seek(0, SeekOrigin.Begin);

            var fileUri = $"batch/{batchHandle.BatchId}/files/{fileName}";
            {
                var fileModel = new FileModel()
                { Attributes = fileAttributes ?? Enumerable.Empty<KeyValuePair<string, string>>() };

                Dictionary<string, string> requestHeaders = new Dictionary<string, string>();
                requestHeaders.Add("X-Content-Size", "" + stream.Length);
                if (!string.IsNullOrEmpty(mimeType)) requestHeaders.Add("X-MIME-Type", mimeType);

                var result = await SendResult<FileModel, AddFileToBatchResponse>(fileUri, HttpMethod.Post, fileModel, cancellationToken,
                    HttpStatusCode.Created, false, requestHeaders);
                if (result.Errors != null && result.Errors.Any())
                {
                    return result;
                }
            }

            var md5Hash = stream.CalculateMD5();
            stream.Seek(0, SeekOrigin.Begin);

            var fileBlocks = new List<string>();
            var fileBlockId = 0;
            var expectedTotalBlockCount = (int)Math.Ceiling(stream.Length / (double)maxFileBlockSize);
            progressUpdate((0, expectedTotalBlockCount));

            var buffer = new byte[maxFileBlockSize];
            while (true)
            {
                fileBlockId++;
                var ms = new MemoryStream();

                var read = stream.Read(buffer, 0, maxFileBlockSize);
                if (read <= 0) break;
                ms.Write(buffer, 0, read);

                var fileBlockIdAsString = fileBlockId.ToString("D5");
                var putFileUri = $"batch/{batchHandle.BatchId}/files/{fileName}/{fileBlockIdAsString}";
                fileBlocks.Add(fileBlockIdAsString);
                ms.Seek(0, SeekOrigin.Begin);

                var blockMD5 = ms.CalculateMD5();

                using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Put, putFileUri)
                { Content = new StreamContent(ms) })
                {
                    httpRequestMessage.Content.Headers.ContentType =
                        new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");

                    httpRequestMessage.Content.Headers.ContentMD5 = blockMD5;

                    var httpClient = await GetAuthenticationHeaderSetClient();
                    var putFileResponse = await httpClient.SendAsync(httpRequestMessage, cancellationToken);
                    ////putFileResponse.EnsureSuccessStatusCode();

                    var result = new Result<AddFileToBatchResponse>();
                    await result.ProcessHttpResponse(HttpStatusCode.Created, putFileResponse);


                    //var result = await SendResult<MemoryStream, AddFileToBatchResponse>(putFileUri,
                    //                                HttpMethod.Put, ms, cancellationToken, HttpStatusCode.Created, true, null, httpRequestMessage.Content.Headers);
                    if (result.Errors != null && result.Errors.Any())
                    {
                        return result;
                    }
                    progressUpdate((fileBlockId, expectedTotalBlockCount));
                }

                {
                    var writeBlockFileModel = new WriteBlockFileModel { BlockIds = fileBlocks };
                    var result = await SendResult<WriteBlockFileModel, AddFileToBatchResponse>(fileUri, HttpMethod.Put,
                        writeBlockFileModel, cancellationToken, HttpStatusCode.NoContent);
                    if (result.Errors != null && result.Errors.Any())
                    {
                        return result;
                    }
                }
            ((BatchHandle)batchHandle).AddFile(fileName, Convert.ToBase64String(md5Hash));
            }
            return null;
        }

        private async Task<IResult<TResponse>> SendResult<TRequest, TResponse>(string uri, HttpMethod httpMethod,
            TRequest request, CancellationToken cancellationToken, HttpStatusCode successCode, bool isRequestContentStream = default, Dictionary<string, string> requestHeaders = default, HttpContentHeaders contentHeaders = default)
        {
            string payloadJson;
            var httpRequestMessage = new HttpRequestMessage();
            HttpContent httpContent = null;
            if (request != null && !isRequestContentStream)
            {
                payloadJson = JsonConvert.SerializeObject(request,
                    new IsoDateTimeConverter { DateTimeFormat = "yyyy-MM-ddTHH:mm:ss.fffK" });
                httpContent = new StringContent(payloadJson, Encoding.UTF8, "application/json");
            }
            else if (isRequestContentStream)//scenario - AddFileToBatch - when uploads a block of byte data
            {
                httpContent = new StreamContent(request as MemoryStream);
            }
            using (httpRequestMessage = new HttpRequestMessage(httpMethod, uri)
            {
                Content = httpContent
            })
            {
                if (requestHeaders != null && requestHeaders.Count() > 0)
                {
                    foreach (var requestHeader in requestHeaders)
                    {
                        httpRequestMessage.Headers.Add(requestHeader.Key, requestHeader.Value);
                    }
                }
                var httpClient = await GetAuthenticationHeaderSetClient();
                var response = await httpClient.SendAsync(httpRequestMessage, cancellationToken);
                var result = new Result<TResponse>();
                await result.ProcessHttpResponse(successCode, response);
                return result;
            }
        }
        #endregion
    }
}