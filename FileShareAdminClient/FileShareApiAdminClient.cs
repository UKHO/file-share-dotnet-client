﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
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
        Task<IBatchHandle> CreateBatchAsync(BatchModel batchModel, CancellationToken? cancellationToken = null);
        Task<BatchStatusResponse> GetBatchStatusAsync(IBatchHandle batchHandle);
        Task AddFileToBatch(IBatchHandle batchHandle, Stream stream, string fileName, string mimeType,
            params KeyValuePair<string, string>[] fileAttributes);
        Task AddFileToBatch(IBatchHandle batchHandle, Stream stream, string fileName, string mimeType, CancellationToken? cancellationToken = null,
            params KeyValuePair<string, string>[] fileAttributes);
        Task AddFileToBatch(IBatchHandle batchHandle, Stream stream, string fileName, string mimeType,
            Action<(int blocksComplete, int totalBlockCount)> progressUpdate,
            params KeyValuePair<string, string>[] fileAttributes);
        Task AddFileToBatch(IBatchHandle batchHandle, Stream stream, string fileName, string mimeType,
            Action<(int blocksComplete, int totalBlockCount)> progressUpdate, CancellationToken? cancellationToken = null,
            params KeyValuePair<string, string>[] fileAttributes);
        Task CommitBatch(IBatchHandle batchHandle);
        Task RollBackBatchAsync(IBatchHandle batchHandle);
        Task AppendAclAsync(string batchId, Acl acl, CancellationToken? cancellationToken = null);
        Task ReplaceAclAsync(string batchId, Acl acl, CancellationToken? cancellationToken = null);
        Task<IResult<SetExpiryDateResponse>> SetExpiryDateAsync(string batchId, BatchExpiryModel batchExpiry, CancellationToken? cancellationToken = null);
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

        public async Task AppendAclAsync(string batchId, Acl acl, CancellationToken? cancellationToken = null)
        {
            cancellationToken = cancellationToken ?? CancellationToken.None;

            var uri = $"batch/{batchId}/acl";
            var payloadJson = JsonConvert.SerializeObject(acl);

            using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, uri)
            {
                Content = new StringContent(payloadJson, Encoding.UTF8, "application/json")
            })
            {
                var httpClient = await GetAuthenticationHeaderSetClient();
                var response = await httpClient.SendAsync(httpRequestMessage, cancellationToken.Value);
                response.EnsureSuccessStatusCode();
            }
        }

        public async Task<IBatchHandle> CreateBatchAsync(BatchModel batchModel, CancellationToken? cancellationToken = null)
        {
            cancellationToken = cancellationToken ?? CancellationToken.None;

            const string uri = "batch";
            var payloadJson = JsonConvert.SerializeObject(batchModel,
                new IsoDateTimeConverter { DateTimeFormat = "yyyy-MM-ddTHH:mm:ss.fffK" });

            using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, uri)
            {
                Content = new StringContent(payloadJson, Encoding.UTF8, "application/json")
            })
            {
                var httpClient = await GetAuthenticationHeaderSetClient();
                var response = await httpClient.SendAsync(httpRequestMessage, cancellationToken.Value);
                response.EnsureSuccessStatusCode();

                var data = await response.ReadAsTypeAsync<CreateBatchResponseModel>();
                var batchId = data.BatchId;

                return new BatchHandle(batchId);
            }
        }

        public Task<BatchStatusResponse> GetBatchStatusAsync(IBatchHandle batchHandle)
        {
            return GetBatchStatusAsync(batchHandle.BatchId);
        }

        public Task AddFileToBatch(IBatchHandle batchHandle, Stream stream, string fileName, string mimeType,
            params KeyValuePair<string, string>[] fileAttributes)
        {
            return AddFileToBatch(batchHandle, stream, fileName, mimeType, _ => { }, CancellationToken.None, fileAttributes);
        }

        public Task AddFileToBatch(IBatchHandle batchHandle, Stream stream, string fileName, string mimeType, CancellationToken? cancellationToken = null,
            params KeyValuePair<string, string>[] fileAttributes)
        {
            cancellationToken = cancellationToken ?? CancellationToken.None;

            return AddFileToBatch(batchHandle, stream, fileName, mimeType, _ => { }, cancellationToken.Value, fileAttributes);
        }

        public async Task AddFileToBatch(IBatchHandle batchHandle, Stream stream, string fileName, string mimeType,
            Action<(int blocksComplete, int totalBlockCount)> progressUpdate,
            params KeyValuePair<string, string>[] fileAttributes)
        {
            await AddFile(batchHandle, stream, fileName, mimeType, progressUpdate, CancellationToken.None, fileAttributes);
        }

        public async Task AddFileToBatch(IBatchHandle batchHandle, Stream stream, string fileName, string mimeType,
            Action<(int blocksComplete, int totalBlockCount)> progressUpdate, CancellationToken? cancellationToken = null,
            params KeyValuePair<string, string>[] fileAttributes)
        {
            cancellationToken = cancellationToken ?? CancellationToken.None;

            await AddFile(batchHandle, stream, fileName, mimeType, progressUpdate, cancellationToken.Value, fileAttributes);
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

        public async Task ReplaceAclAsync(string batchId, Acl acl, CancellationToken? cancellationToken = null)
        {
            cancellationToken = cancellationToken ?? CancellationToken.None;

            var uri = $"/batch/{batchId}/acl";
            string payloadJson = JsonConvert.SerializeObject(acl);

            using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Put, uri)
            { Content = new StringContent(payloadJson, Encoding.UTF8, "application/json") })

            {
                var httpClient = await GetAuthenticationHeaderSetClient();
                var response = await httpClient.SendAsync(httpRequestMessage, cancellationToken.Value);
                response.EnsureSuccessStatusCode();
            }
        }

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

        public async Task<IResult<SetExpiryDateResponse>> SetExpiryDateAsync(string batchId, BatchExpiryModel batchExpiry,
                    CancellationToken? cancellationToken = null)
        {
            cancellationToken = cancellationToken ?? CancellationToken.None;

            var uri = $"batch/{batchId}/expiry";

            var payloadJson = JsonConvert.SerializeObject(batchExpiry);

            using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Put, uri)
            { Content = new StringContent(payloadJson, Encoding.UTF8, "application/json") })
            {
                var httpClient = await GetAuthenticationHeaderSetClient();

                var response = await httpClient.SendAsync(httpRequestMessage, cancellationToken.Value);

                SetExpiryDateResponse setExpiryDateResponse = new SetExpiryDateResponse();

                var data = await setExpiryDateResponse.GetResponse(response);

                data.IsSuccess = response.IsSuccessStatusCode;
                data.StatusCode = (int)response.StatusCode;

                return data;
            }
        }

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
        #endregion
    }
}