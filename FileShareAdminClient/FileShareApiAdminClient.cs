﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UKHO.FileShareAdminClient.Models;
using UKHO.FileShareClient;
using UKHO.FileShareClient.Internal;
using UKHO.FileShareClient.Models;

namespace UKHO.FileShareAdminClient
{
    public interface IFileShareApiAdminClient : IFileShareApiClient
    {
        Task<IBatchHandle> CreateBatchAsync(BatchModel batchModel);
        Task<BatchStatusResponse> GetBatchStatusAsync(IBatchHandle batchHandle);
        Task AddFileToBatch(IBatchHandle batchHandle, Stream stream, string fileName, string mimeType);
        Task CommitBatch(IBatchHandle batchHandle);
        Task RollBackBatchAsync(IBatchHandle batchHandle);
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

        public async Task<IBatchHandle> CreateBatchAsync(BatchModel batchModel)
        {
            const string uri = "batch";
            var payloadJson = JsonConvert.SerializeObject(batchModel,
                new IsoDateTimeConverter {DateTimeFormat = "yyyy-MM-ddTHH:mm:ss.fffK"});

            using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, uri)
            {
                Content = new StringContent(payloadJson, Encoding.UTF8, "application/json")
            })
            {
                var response = await httpClientFactory.CreateClient()
                    .SendAsync(httpRequestMessage, CancellationToken.None);
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


        public async Task AddFileToBatch(IBatchHandle batchHandle, Stream stream, string fileName, string mimeType)
        {
            if (!stream.CanSeek)
                throw new ArgumentException("The stream must be seekable.", nameof(stream));
            stream.Seek(0, SeekOrigin.Begin);

            var fileUri = $"batch/{batchHandle.BatchId}/files/{fileName}";
            var httpClient = httpClientFactory.CreateClient();

            {
                var fileModel = new FileModel();

                var payloadJson = JsonConvert.SerializeObject(fileModel);

                using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, fileUri)
                    {Content = new StringContent(payloadJson, Encoding.UTF8, "application/json")})
                {
                    httpRequestMessage.Headers.Add("X-Content-Size", "" + stream.Length);

                    if (!string.IsNullOrEmpty(mimeType)) httpRequestMessage.Headers.Add("X-MIME-Type", mimeType);


                    var createFileRecordResponse =
                        await httpClient.SendAsync(httpRequestMessage, CancellationToken.None);
                    createFileRecordResponse.EnsureSuccessStatusCode();
                }
            }


            var md5Hash = stream.CalculateMD5();
            stream.Seek(0, SeekOrigin.Begin);

            var fileBlocks = new List<string>();
            var fileBlockId = 1;

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
                    {Content = new StreamContent(ms)})
                {
                    httpRequestMessage.Content.Headers.ContentType =
                        new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");

                    httpRequestMessage.Content.Headers.ContentMD5 = blockMD5;


                    var putFileResponse = await httpClient.SendAsync(httpRequestMessage, CancellationToken.None);
                    putFileResponse.EnsureSuccessStatusCode();
                }
            }

            {
                var writeBlockFileModel = new WriteBlockFileModel {BlockIds = fileBlocks};
                var payloadJson = JsonConvert.SerializeObject(writeBlockFileModel);

                using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Put, fileUri)
                    {Content = new StringContent(payloadJson, Encoding.UTF8, "application/json")})
                {
                    var writeFileResponse = await httpClient.SendAsync(httpRequestMessage, CancellationToken.None);
                    writeFileResponse.EnsureSuccessStatusCode();
                }
            }
            ((BatchHandle) batchHandle).AddFile(fileName, Convert.ToBase64String(md5Hash));
        }

        public async Task CommitBatch(IBatchHandle batchHandle)
        {
            var uri = $"/batch/{batchHandle.BatchId}";
            var batchCommitModel = new BatchCommitModel
            {
                FileDetails = ((BatchHandle) batchHandle).FileDetails
            };


            var payloadJson = JsonConvert.SerializeObject(batchCommitModel.FileDetails);

            using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Put, uri)
                {Content = new StringContent(payloadJson, Encoding.UTF8, "application/json")})
            {
                var response = await httpClientFactory.CreateClient()
                    .SendAsync(httpRequestMessage, CancellationToken.None);
                response.EnsureSuccessStatusCode();
            }
        }

        public async Task RollBackBatchAsync(IBatchHandle batchHandle)
        {
            var uri = $"batch/{batchHandle.BatchId}";

            using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Delete, uri))
            {
                var response = await httpClientFactory.CreateClient()
                    .SendAsync(httpRequestMessage, CancellationToken.None);
                response.EnsureSuccessStatusCode();
            }
        }
    }
}