using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
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
        Task<IResult<IBatchHandle>> CreateBatchAsync(BatchModel batchModel, CancellationToken cancellationToken);
        Task<BatchStatusResponse> GetBatchStatusAsync(IBatchHandle batchHandle);
        Task AddFileToBatchAsync(IBatchHandle batchHandle, Stream stream, string fileName, string mimeType,
            params KeyValuePair<string, string>[] fileAttributes);
        Task<IResult<AddFileToBatchResponse>> AddFileToBatchAsync(IBatchHandle batchHandle, Stream stream, string fileName, string mimeType,
            CancellationToken cancellationToken, params KeyValuePair<string, string>[] fileAttributes);
        Task AddFileToBatchAsync(IBatchHandle batchHandle, Stream stream, string fileName, string mimeType,
            Action<(int blocksComplete, int totalBlockCount)> progressUpdate, params KeyValuePair<string, string>[] fileAttributes);
        Task<IResult<AddFileToBatchResponse>> AddFileToBatchAsync(IBatchHandle batchHandle, Stream stream, string fileName, string mimeType,
            Action<(int blocksComplete, int totalBlockCount)> progressUpdate, CancellationToken cancellationToken,
            params KeyValuePair<string, string>[] fileAttributes);
        Task CommitBatchAsync(IBatchHandle batchHandle);
        Task<IResult<CommitBatchResponse>> CommitBatchAsync(IBatchHandle batchHandle, CancellationToken cancellationToken);
        Task<IResult<ReplaceAclResponse>> ReplaceAclAsync(string batchId, Acl acl, CancellationToken cancellationToken = default);
        Task RollBackBatchAsync(IBatchHandle batchHandle);
        Task<IResult<RollBackBatchResponse>> RollBackBatchAsync(IBatchHandle batchHandle, CancellationToken cancellationToken);
        Task<IResult<SetExpiryDateResponse>> SetExpiryDateAsync(string batchId, BatchExpiryModel batchExpiry, CancellationToken cancellationToken = default);

        #region backwards compatible obsolete versions of methods that have been renamed.

        [Obsolete("Please use AddFileToBatchAsync")]
        Task AddFileToBatch(IBatchHandle batchHandle, Stream stream, string fileName, string mimeType,
            params KeyValuePair<string, string>[] fileAttributes);

        [Obsolete("Please use AddFileToBatchAsync")]
        Task<IResult<AddFileToBatchResponse>> AddFileToBatch(IBatchHandle batchHandle, Stream stream, string fileName,
            string mimeType, CancellationToken cancellationToken, params KeyValuePair<string, string>[] fileAttributes);

        [Obsolete("Please use AddFileToBatchAsync")]
        Task AddFileToBatch(IBatchHandle batchHandle, Stream stream, string fileName, string mimeType,
            Action<(int blocksComplete, int totalBlockCount)> progressUpdate,
            params KeyValuePair<string, string>[] fileAttributes);

        [Obsolete("Please use AddFileToBatchAsync")]
        Task<IResult<AddFileToBatchResponse>> AddFileToBatch(IBatchHandle batchHandle, Stream stream, string fileName,
            string mimeType, Action<(int blocksComplete, int totalBlockCount)> progressUpdate,
            CancellationToken cancellationToken, params KeyValuePair<string, string>[] fileAttributes);

        [Obsolete("Please use CommitBatchAsync")]
        Task CommitBatch(IBatchHandle batchHandle);

        [Obsolete("Please use CommitBatchAsync")]
        Task<IResult<CommitBatchResponse>> CommitBatch(IBatchHandle batchHandle, CancellationToken cancellationToken);

        #endregion
    }

    public class FileShareApiAdminClient : FileShareApiClient, IFileShareApiAdminClient
    {
        private const int DefaultMaxFileBlockSize = 4194304;
        private readonly int _maxFileBlockSize;

        public FileShareApiAdminClient(string baseAddress, string accessToken) : base(baseAddress, accessToken)
        {
            _maxFileBlockSize = DefaultMaxFileBlockSize;
        }

        public FileShareApiAdminClient(string baseAddress, string accessToken, int maxFileBlockSize) : base(baseAddress, accessToken)
        {
            _maxFileBlockSize = maxFileBlockSize;
        }

        public FileShareApiAdminClient(IHttpClientFactory httpClientFactory, string baseAddress, string accessToken) : base(httpClientFactory, baseAddress, accessToken)
        {
            _maxFileBlockSize = DefaultMaxFileBlockSize;
        }

        public FileShareApiAdminClient(IHttpClientFactory httpClientFactory, string baseAddress, string accessToken, int maxFileBlockSize) : base(httpClientFactory, baseAddress, accessToken)
        {
            _maxFileBlockSize = maxFileBlockSize;
        }

        public FileShareApiAdminClient(IHttpClientFactory httpClientFactory, string baseAddress, IAuthTokenProvider authTokenProvider) : base(httpClientFactory, baseAddress, authTokenProvider)
        {
            _maxFileBlockSize = DefaultMaxFileBlockSize;
        }

        public FileShareApiAdminClient(IHttpClientFactory httpClientFactory, string baseAddress, IAuthTokenProvider authTokenProvider, int maxFileBlockSize) : base(httpClientFactory, baseAddress, authTokenProvider)
        {
            _maxFileBlockSize = maxFileBlockSize;
        }

        public async Task<IResult<AppendAclResponse>> AppendAclAsync(string batchId, Acl acl, CancellationToken cancellationToken = default)
            => await SendResult<Acl, AppendAclResponse>($"batch/{batchId}/acl", HttpMethod.Post, acl, cancellationToken);

        public async Task<IBatchHandle> CreateBatchAsync(BatchModel batchModel)
        {
            const string uri = "batch";
            var payloadJson = JsonConvert.SerializeObject(batchModel,
                new IsoDateTimeConverter { DateTimeFormat = "yyyy-MM-ddTHH:mm:ss.fffK" });

            using (var httpClient = await GetAuthenticationHeaderSetClient())
            using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, uri)
            {
                Content = new StringContent(payloadJson, Encoding.UTF8, "application/json")
            })
            {
                var response = await httpClient.SendAsync(httpRequestMessage);
                response.EnsureSuccessStatusCode();

                var data = await response.ReadAsTypeAsync<CreateBatchResponseModel>();
                var batchId = data.BatchId;

                return new BatchHandle(batchId);
            }
        }

        public async Task<IResult<IBatchHandle>> CreateBatchAsync(BatchModel batchModel, CancellationToken cancellationToken)
        {
            var result = await SendResult<BatchModel, BatchHandle>($"batch", HttpMethod.Post, batchModel, cancellationToken);
            var mappedResult = new Result<IBatchHandle>
            {
                Data = result.Data,
                Errors = result.Errors,
                IsSuccess = result.IsSuccess,
                StatusCode = result.StatusCode
            };
            return mappedResult;
        }

        public Task<BatchStatusResponse> GetBatchStatusAsync(IBatchHandle batchHandle)
        {
            return GetBatchStatusAsync(batchHandle.BatchId);
        }

        public Task AddFileToBatchAsync(IBatchHandle batchHandle, Stream stream, string fileName, string mimeType,
            params KeyValuePair<string, string>[] fileAttributes)
        {
            return AddFileToBatchAsync(batchHandle, stream, fileName, mimeType, _ => { }, CancellationToken.None, fileAttributes);
        }

        public Task<IResult<AddFileToBatchResponse>> AddFileToBatchAsync(IBatchHandle batchHandle, Stream stream, string fileName, string mimeType, CancellationToken cancellationToken,
            params KeyValuePair<string, string>[] fileAttributes)
        {
            return AddFileToBatchAsync(batchHandle, stream, fileName, mimeType, _ => { }, cancellationToken, fileAttributes);
        }

        public async Task AddFileToBatchAsync(IBatchHandle batchHandle, Stream stream, string fileName, string mimeType,
            Action<(int blocksComplete, int totalBlockCount)> progressUpdate,
            params KeyValuePair<string, string>[] fileAttributes)
        {
            await AddFileAsync(batchHandle, stream, fileName, mimeType, progressUpdate, CancellationToken.None, fileAttributes);
        }

        public async Task<IResult<AddFileToBatchResponse>> AddFileToBatchAsync(IBatchHandle batchHandle, Stream stream, string fileName, string mimeType,
            Action<(int blocksComplete, int totalBlockCount)> progressUpdate, CancellationToken cancellationToken,
            params KeyValuePair<string, string>[] fileAttributes)
        {
            return await AddFiles(batchHandle, stream, fileName, mimeType, progressUpdate, cancellationToken, fileAttributes);
        }

        public async Task CommitBatchAsync(IBatchHandle batchHandle)
        {
            var uri = $"/batch/{batchHandle.BatchId}";
            var batchCommitModel = new BatchCommitModel
            {
                FileDetails = ((BatchHandle)batchHandle).FileDetails
            };

            var payloadJson = JsonConvert.SerializeObject(batchCommitModel.FileDetails);

            using (var httpClient = await GetAuthenticationHeaderSetClient())
            using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Put, uri) { Content = new StringContent(payloadJson, Encoding.UTF8, "application/json") })
            {
                var response = await httpClient.SendAsync(httpRequestMessage, CancellationToken.None);
                response.EnsureSuccessStatusCode();
            }
        }

        public async Task<IResult<CommitBatchResponse>> CommitBatchAsync(IBatchHandle batchHandle, CancellationToken cancellationToken)
        {
            var uri = $"/batch/{batchHandle.BatchId}";
            var batchCommitModel = new BatchCommitModel
            {
                FileDetails = ((BatchHandle)batchHandle).FileDetails
            };
            return await SendResult<List<FileDetail>, CommitBatchResponse>(uri, HttpMethod.Put, batchCommitModel.FileDetails, cancellationToken);
        }

        public async Task<IResult<ReplaceAclResponse>> ReplaceAclAsync(string batchId, Acl acl, CancellationToken cancellationToken = default)
            => await SendResult<Acl, ReplaceAclResponse>($"batch/{batchId}/acl", HttpMethod.Put, acl, cancellationToken);

        public async Task RollBackBatchAsync(IBatchHandle batchHandle)
        {
            var uri = $"batch/{batchHandle.BatchId}";

            using (var httpClient = await GetAuthenticationHeaderSetClient())
            using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Delete, uri))
            {
                var response = await httpClient.SendAsync(httpRequestMessage, CancellationToken.None);
                response.EnsureSuccessStatusCode();
            }
        }

        public async Task<IResult<RollBackBatchResponse>> RollBackBatchAsync(IBatchHandle batchHandle, CancellationToken cancellationToken)
            => await SendResult<IBatchHandle, RollBackBatchResponse>($"batch/{batchHandle.BatchId}", HttpMethod.Delete, null, cancellationToken);

        public async Task<IResult<SetExpiryDateResponse>> SetExpiryDateAsync(string batchId, BatchExpiryModel batchExpiry, CancellationToken cancellationToken = default)
            => await SendResult<BatchExpiryModel, SetExpiryDateResponse>($"batch/{batchId}/expiry", HttpMethod.Put, batchExpiry, cancellationToken);

        #region backwards compatible obsolete versions of methods that have been renamed.

        [Obsolete("Please use AddFileToBatchAsync")]
        public Task AddFileToBatch(IBatchHandle batchHandle, Stream stream, string fileName, string mimeType, params KeyValuePair<string, string>[] fileAttributes)
        {
            return AddFileToBatchAsync(batchHandle, stream, fileName, mimeType, fileAttributes);
        }

        [Obsolete("Please use AddFileToBatchAsync")]

        public Task<IResult<AddFileToBatchResponse>> AddFileToBatch(IBatchHandle batchHandle, Stream stream, string fileName, string mimeType, CancellationToken cancellationToken, params KeyValuePair<string, string>[] fileAttributes)
        {
            return AddFileToBatchAsync(batchHandle, stream, fileName, mimeType, cancellationToken, fileAttributes);
        }

        [Obsolete("Please use AddFileToBatchAsync")]
        public Task AddFileToBatch(IBatchHandle batchHandle, Stream stream, string fileName, string mimeType, Action<(int blocksComplete, int totalBlockCount)> progressUpdate, params KeyValuePair<string, string>[] fileAttributes)
        {
            return AddFileToBatchAsync(batchHandle, stream, fileName, mimeType, progressUpdate, fileAttributes);
        }

        [Obsolete("Please use AddFileToBatchAsync")]
        public Task<IResult<AddFileToBatchResponse>> AddFileToBatch(IBatchHandle batchHandle, Stream stream, string fileName, string mimeType, Action<(int blocksComplete, int totalBlockCount)> progressUpdate, CancellationToken cancellationToken, params KeyValuePair<string, string>[] fileAttributes)
        {
            return AddFileToBatchAsync(batchHandle, stream, fileName, mimeType, progressUpdate, cancellationToken, fileAttributes);
        }

        [Obsolete("Please use CommitBatchAsync")]
        public Task CommitBatch(IBatchHandle batchHandle)
        {
            return CommitBatchAsync(batchHandle);
        }

        [Obsolete("Please use CommitBatchAsync")]
        public Task<IResult<CommitBatchResponse>> CommitBatch(IBatchHandle batchHandle, CancellationToken cancellationToken)
        {
            return CommitBatchAsync(batchHandle, cancellationToken);
        }

        #endregion

        #region Private methods

        private async Task AddFileAsync(IBatchHandle batchHandle, Stream stream, string fileName, string mimeType,
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

                using (var httpClient = await GetAuthenticationHeaderSetClient())
                using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, fileUri) { Content = new StringContent(payloadJson, Encoding.UTF8, "application/json") })
                {
                    httpRequestMessage.Headers.Add("X-Content-Size", "" + stream.Length);

                    if (!string.IsNullOrEmpty(mimeType)) httpRequestMessage.Headers.Add("X-MIME-Type", mimeType);

                    var createFileRecordResponse = await httpClient.SendAsync(httpRequestMessage, cancellationToken);
                    createFileRecordResponse.EnsureSuccessStatusCode();
                }
            }

            var fileBlocks = new List<string>();
            var fileBlockId = 0;
            var expectedTotalBlockCount = (int)Math.Ceiling(stream.Length / (double)_maxFileBlockSize);
            progressUpdate((0, expectedTotalBlockCount));

            var buffer = new byte[_maxFileBlockSize];

            using (var md5 = MD5.Create())
            using (var cryptoStream = new CryptoStream(stream, md5, CryptoStreamMode.Read, true))
            {
                while (true)
                {
                    fileBlockId++;
                    var ms = new MemoryStream();

                    var read = cryptoStream.Read(buffer, 0, _maxFileBlockSize);
                    if (read <= 0) break;
                    ms.Write(buffer, 0, read);

                    var fileBlockIdAsString = fileBlockId.ToString("D5");
                    var putFileUri = $"batch/{batchHandle.BatchId}/files/{fileName}/{fileBlockIdAsString}";
                    fileBlocks.Add(fileBlockIdAsString);
                    ms.Seek(0, SeekOrigin.Begin);

                    var blockMD5 = ms.CalculateMD5();

                    using (var httpClient = await GetAuthenticationHeaderSetClient())
                    using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Put, putFileUri) { Content = new StreamContent(ms) })
                    {
                        httpRequestMessage.Content.Headers.ContentType =
                            new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");

                        httpRequestMessage.Content.Headers.ContentMD5 = blockMD5;

                        var putFileResponse = await httpClient.SendAsync(httpRequestMessage, cancellationToken);
                        putFileResponse.EnsureSuccessStatusCode();

                        progressUpdate((fileBlockId, expectedTotalBlockCount));
                    }
                }

                {
                    var writeBlockFileModel = new WriteBlockFileModel { BlockIds = fileBlocks };
                    var payloadJson = JsonConvert.SerializeObject(writeBlockFileModel);

                    using (var httpClient = await GetAuthenticationHeaderSetClient())
                    using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Put, fileUri) { Content = new StringContent(payloadJson, Encoding.UTF8, "application/json") })
                    {
                        var writeFileResponse = await httpClient.SendAsync(httpRequestMessage, cancellationToken);
                        writeFileResponse.EnsureSuccessStatusCode();
                    }
                }

                ((BatchHandle)batchHandle).AddFile(fileName, Convert.ToBase64String(md5.Hash));
            }
        }

        private async Task<IResult<AddFileToBatchResponse>> AddFiles(IBatchHandle batchHandle, Stream stream, string fileName, string mimeType,
            Action<(int blocksComplete, int totalBlockCount)> progressUpdate, CancellationToken cancellationToken,
            params KeyValuePair<string, string>[] fileAttributes)
        {
            var mappedResult = new Result<AddFileToBatchResponse>();
            if (!stream.CanSeek)
                throw new ArgumentException("The stream must be seekable.", nameof(stream));
            stream.Seek(0, SeekOrigin.Begin);

            var fileUri = $"batch/{batchHandle.BatchId}/files/{fileName}";
            {
                var fileModel = new FileModel { Attributes = fileAttributes ?? Enumerable.Empty<KeyValuePair<string, string>>() };

                var requestHeaders = new Dictionary<string, string>
                {
                    { "X-Content-Size", "" + stream.Length }
                };

                if (!string.IsNullOrEmpty(mimeType)) requestHeaders.Add("X-MIME-Type", mimeType);

                var result = await SendResult<FileModel, AddFileToBatchResponse>(fileUri, HttpMethod.Post, fileModel, cancellationToken, requestHeaders);

                if (result.Errors != null && result.Errors.Any())
                {
                    mappedResult = (Result<AddFileToBatchResponse>)result;
                }
                else
                {
                    var fileBlocks = new List<string>();
                    var fileBlockId = 0;
                    var expectedTotalBlockCount = (int)Math.Ceiling(stream.Length / (double)_maxFileBlockSize);
                    progressUpdate((0, expectedTotalBlockCount));

                    var buffer = new byte[_maxFileBlockSize];

                    using (var md5 = MD5.Create())
                    using (var cryptoStream = new CryptoStream(stream, md5, CryptoStreamMode.Read, true))
                    {
                        while (true)
                        {
                            fileBlockId++;
                            var ms = new MemoryStream();

                            var read = cryptoStream.Read(buffer, 0, _maxFileBlockSize);
                            if (read <= 0) break;
                            ms.Write(buffer, 0, read);

                            var fileBlockIdAsString = fileBlockId.ToString("D5");
                            var putFileUri = $"batch/{batchHandle.BatchId}/files/{fileName}/{fileBlockIdAsString}";
                            fileBlocks.Add(fileBlockIdAsString);
                            ms.Seek(0, SeekOrigin.Begin);

                            var blockMD5 = ms.CalculateMD5();

                            using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Put, putFileUri) { Content = new StreamContent(ms) })
                            {
                                httpRequestMessage.Content.Headers.ContentType =
                                    new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");

                                httpRequestMessage.Content.Headers.ContentMD5 = blockMD5;
                                progressUpdate((fileBlockId, expectedTotalBlockCount));

                                result = await SendMessageResult<AddFileToBatchResponse>(httpRequestMessage, cancellationToken);
                                if (result.Errors != null && result.Errors.Any())
                                {
                                    mappedResult = (Result<AddFileToBatchResponse>)result;
                                    break;
                                }
                            }
                        }

                        if (!(mappedResult.Errors != null && mappedResult.Errors.Any()))
                        {
                            var writeBlockFileModel = new WriteBlockFileModel { BlockIds = fileBlocks };
                            result = await SendResult<WriteBlockFileModel, AddFileToBatchResponse>(fileUri, HttpMethod.Put, writeBlockFileModel, cancellationToken);

                            if (result.Errors != null && result.Errors.Any())
                            {
                                mappedResult = (Result<AddFileToBatchResponse>)result;
                            }
                            else
                            {
                                ((BatchHandle)batchHandle).AddFile(fileName, Convert.ToBase64String(md5.Hash));
                                return result;
                            }
                        }
                    }
                }
            }

            return mappedResult;
        }

        private async Task<IResult<TResponse>> SendResult<TRequest, TResponse>(string uri, HttpMethod httpMethod, TRequest request, CancellationToken cancellationToken, Dictionary<string, string> requestHeaders = default)
            => await SendObjectResult<TResponse>(uri, httpMethod, request, cancellationToken, requestHeaders);

        private async Task<IResult<TResponse>> SendObjectResult<TResponse>(string uri, HttpMethod httpMethod, object request, CancellationToken cancellationToken, Dictionary<string, string> requestHeaders = default)
        {
            var payloadJson = JsonConvert.SerializeObject(request, new IsoDateTimeConverter { DateTimeFormat = "yyyy-MM-ddTHH:mm:ss.fffK" });
            var httpContent = new StringContent(payloadJson, Encoding.UTF8, "application/json");

            using (var httpRequestMessage = new HttpRequestMessage(httpMethod, uri) { Content = httpContent })
            {
                foreach (var requestHeader in requestHeaders ?? new Dictionary<string, string>())
                {
                    httpRequestMessage.Headers.Add(requestHeader.Key, requestHeader.Value);
                }

                return await SendMessageResult<TResponse>(httpRequestMessage, cancellationToken);
            }
        }

        private async Task<IResult<TResponse>> SendMessageResult<TResponse>(HttpRequestMessage messageToSend, CancellationToken cancellationToken)
        {
            using (var httpClient = await GetAuthenticationHeaderSetClient())
            {
                var response = await httpClient.SendAsync(messageToSend, cancellationToken);
                var result = new Result<TResponse>();
                return await Result.WithObjectData<TResponse>(response);
            }
        }

        #endregion
    }
}
