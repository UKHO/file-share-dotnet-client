using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FileShareClient.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace FileShareClient
{
    public class FileShareApiClient
    {
        private readonly int maxFileBlockSize;
        private readonly HttpClient httpClient = new HttpClient(new LoggingHandler(new HttpClientHandler()));

        public FileShareApiClient(string baseAddress, string accessToken, int maxFileBlockSize = 4194304)
        {
            this.maxFileBlockSize = maxFileBlockSize;
            httpClient.BaseAddress = new Uri(baseAddress);

            httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("bearer", accessToken);
        }

        public async System.Threading.Tasks.Task<IBatchHandle> CreateBatchAsync(Models.BatchModel batchModel)
        {
            var uri = "batch";
            var payloadJson = JsonConvert.SerializeObject(batchModel,
                new IsoDateTimeConverter() {DateTimeFormat = "yyyy-MM-ddTHH:mm:ss.fffK"});

            using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, uri)
            {
                Content = new StringContent(payloadJson, Encoding.UTF8, "application/json")
            })
            {
                var response = await httpClient.SendAsync(httpRequestMessage, CancellationToken.None);
                response.EnsureSuccessStatusCode();
                var data = await response.ReadAsTypeAsync<Models.ResponseCreateBatchModel>();
                var batchId = data.BatchId;
                return new BatchHandle(batchId);
            }
        }

        public Task<Models.BatchStatusResponse> GetBatchStatusAsync(IBatchHandle batchHandle)
        {
            return GetBatchStatusAsync(batchHandle.BatchId);
        }

        public async Task<Models.BatchStatusResponse> GetBatchStatusAsync(string batchId)
        {
            string uri = $"batch/{batchId}/status";

            using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, uri))
            {
                var response = await httpClient.SendAsync(httpRequestMessage, CancellationToken.None);
                response.EnsureSuccessStatusCode();
                var status = await response.ReadAsTypeAsync<Models.BatchStatusResponse>();
                return status;
            }
        }

        public async Task AddFileToBatch(IBatchHandle batchHandle, Stream stream, string fileName, string mimeType)
        {
            if (!stream.CanSeek)
                throw new Exception("The stream must be seekable.");
            stream.Seek(0, SeekOrigin.Begin);

            var fileUri = $"batch/{batchHandle.BatchId}/files/{fileName}";


            {
                var fileModel = new FileModel();

                var payloadJson = JsonConvert.SerializeObject(fileModel);

                using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, fileUri)
                    {Content = new StringContent(payloadJson, Encoding.UTF8, "application/json")})
                {
                    httpRequestMessage.Headers.Add("X-Content-Size", "" + stream.Length);

                    if (!string.IsNullOrEmpty(mimeType))
                    {
                        httpRequestMessage.Headers.Add("X-MIME-Type", mimeType);
                    }


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
                var writeblockfileModel = new WriteBlockFileModel() {BlockIds = fileBlocks};
                var payloadJson = JsonConvert.SerializeObject(writeblockfileModel);

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
                var response = await httpClient.SendAsync(httpRequestMessage, CancellationToken.None);
                response.EnsureSuccessStatusCode();
            }
        }

        public async Task RollBackBatchAsync(IBatchHandle batchHandle)
        {
            var uri = $"batch/{batchHandle.BatchId}";

            using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Delete, uri))
            {
                var response = await httpClient.SendAsync(httpRequestMessage, CancellationToken.None);
                response.EnsureSuccessStatusCode();
            }
        }
    }


    public class LoggingHandler : DelegatingHandler
    {
        public LoggingHandler(HttpMessageHandler innerHandler)
            : base(innerHandler)
        {
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Console.WriteLine("Request:");
            Console.WriteLine(request.ToString());
            if (request.Content != null)
            {
                var readAsString = (await request.Content.ReadAsStringAsync());
                Console.WriteLine(readAsString.Substring(0,Math.Min(1000, readAsString.Length)));
            }

            Console.WriteLine();

            HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

            Console.WriteLine("Response:");
            Console.WriteLine(response.ToString());
            if (response.Content != null)
            {
                Console.WriteLine(await response.Content.ReadAsStringAsync());
            }

            Console.WriteLine();

            return response;
        }
    }
}