using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using UKHO.FileShareAdminClient;
using UKHO.FileShareAdminClient.Models;
using UKHO.FileShareClientTests.Helpers;

namespace UKHO.FileShareAdminClientTests
{
    internal class NewBatchCommitAndRollbackTests
    {
        private object nextResponse = null;
        private IFileShareApiAdminClient fileShareApiClient;
        private HttpStatusCode nextResponseStatusCode;
        private List<(HttpMethod, Uri)> lastRequestUris;
        private List<string> lastRequestBodies;
        private const int MaxBlockSize = 32;
        private FakeFssHttpClientFactory fakeHttpClientFactory;
        private const string DUMMY_ACCESS_TOKEN = "ACarefullyEncodedSecretAccessToken";

        [SetUp]
        public void Setup()
        {
            fakeHttpClientFactory = new FakeFssHttpClientFactory(request =>
            {
                lastRequestUris.Add((request.Method, request.RequestUri));
                if (request.Content is StringContent content && request.Content.Headers.ContentLength.HasValue)
                    lastRequestBodies.Add(content.ReadAsStringAsync().Result);
                else
                    lastRequestBodies.Add(null);

                return (nextResponseStatusCode, nextResponse);
            });
            nextResponse = null;
            nextResponseStatusCode = HttpStatusCode.Created;
            lastRequestUris = new List<(HttpMethod, Uri)>();
            lastRequestBodies = new List<string>();

            var config = new
            {
                BaseAddress = @"https://fss-tests.net",
                AccessToken = DUMMY_ACCESS_TOKEN
            };

            fileShareApiClient =
                new FileShareApiAdminClient(fakeHttpClientFactory, config.BaseAddress, config.AccessToken,
                    MaxBlockSize);
        }

        [Test]
        public async Task TestCommitNewBatch()
        {
            var expectedBatchId = Guid.NewGuid().ToString();
            nextResponse = new CreateBatchResponseModel {BatchId = expectedBatchId};
            var batchHandle = await fileShareApiClient.CreateBatchAsync(new BatchModel {BusinessUnit = "TestUnit"});
            Assert.AreEqual(expectedBatchId, batchHandle.BatchId);

            nextResponse = null;
            Stream stream1 = new MemoryStream(new byte[MaxBlockSize]);
            Stream stream2 = new MemoryStream(new byte[MaxBlockSize * 4]);
            var filename1 = "File1.bin";
            var filename2 = "File2.bin";
            var mimeType1 = "application/octet-stream";

            await fileShareApiClient.AddFileToBatch(batchHandle, stream1, filename1, mimeType1);
            await fileShareApiClient.AddFileToBatch(batchHandle, stream2, filename2, mimeType1);

            await fileShareApiClient.CommitBatch(batchHandle);

            CollectionAssert.AreEqual(new[]
            {
                "POST:/batch",

                $"POST:/batch/{expectedBatchId}/files/{filename1}",
                $"PUT:/batch/{expectedBatchId}/files/{filename1}/00001",
                $"PUT:/batch/{expectedBatchId}/files/{filename1}",

                $"POST:/batch/{expectedBatchId}/files/{filename2}",
                $"PUT:/batch/{expectedBatchId}/files/{filename2}/00001",
                $"PUT:/batch/{expectedBatchId}/files/{filename2}/00002",
                $"PUT:/batch/{expectedBatchId}/files/{filename2}/00003",
                $"PUT:/batch/{expectedBatchId}/files/{filename2}/00004",
                $"PUT:/batch/{expectedBatchId}/files/{filename2}",
                $"PUT:/batch/{expectedBatchId}"
            }, lastRequestUris.Select(uri => $"{uri.Item1}:{uri.Item2.AbsolutePath}"));

            var batchCommitModel = lastRequestBodies.Last().DeserialiseJson<List<FileDetail>>();
            CollectionAssert.AreEqual(new object[] {filename1, filename2}, batchCommitModel.Select(f => f.FileName));
        }

        [Test]
        public async Task TestCommitNewBatchWithCancellationToken()
        {
            var expectedBatchId = Guid.NewGuid().ToString();
            nextResponse = new CreateBatchResponseModel { BatchId = expectedBatchId };
            var batchHandle = await fileShareApiClient.CreateBatchAsync(new BatchModel { BusinessUnit = "TestUnit" }, CancellationToken.None);
            Assert.AreEqual(expectedBatchId, batchHandle.Data.BatchId);

            nextResponse = null;
            Stream stream1 = new MemoryStream(new byte[MaxBlockSize]);
            Stream stream2 = new MemoryStream(new byte[MaxBlockSize * 4]);
            var filename1 = "File1.bin";
            var filename2 = "File2.bin";
            var mimeType1 = "application/octet-stream";

            await fileShareApiClient.AddFileToBatch(batchHandle.Data, stream1, filename1, mimeType1, CancellationToken.None);
            await fileShareApiClient.AddFileToBatch(batchHandle.Data, stream2, filename2, mimeType1, CancellationToken.None);

            await fileShareApiClient.CommitBatch(batchHandle.Data);

            CollectionAssert.AreEqual(new[]
            {
                "POST:/batch",

                $"POST:/batch/{expectedBatchId}/files/{filename1}",
                $"PUT:/batch/{expectedBatchId}/files/{filename1}/00001",
                $"PUT:/batch/{expectedBatchId}/files/{filename1}",

                $"POST:/batch/{expectedBatchId}/files/{filename2}",
                $"PUT:/batch/{expectedBatchId}/files/{filename2}/00001",
                $"PUT:/batch/{expectedBatchId}/files/{filename2}/00002",
                $"PUT:/batch/{expectedBatchId}/files/{filename2}/00003",
                $"PUT:/batch/{expectedBatchId}/files/{filename2}/00004",
                $"PUT:/batch/{expectedBatchId}/files/{filename2}",
                $"PUT:/batch/{expectedBatchId}"
            }, lastRequestUris.Select(uri => $"{uri.Item1}:{uri.Item2.AbsolutePath}"));

            var batchCommitModel = lastRequestBodies.Last().DeserialiseJson<List<FileDetail>>();
            CollectionAssert.AreEqual(new object[] { filename1, filename2 }, batchCommitModel.Select(f => f.FileName));
        }

        [Test]
        public async Task TestRollback()
        {
            var expectedBatchId = Guid.NewGuid().ToString();
            nextResponse = new CreateBatchResponseModel {BatchId = expectedBatchId};
            var batchHandle = await fileShareApiClient.CreateBatchAsync(new BatchModel {BusinessUnit = "TestUnit"});
            Assert.AreEqual(expectedBatchId, batchHandle.BatchId);

            nextResponseStatusCode = HttpStatusCode.NoContent;
            nextResponse = null;
            await fileShareApiClient.RollBackBatchAsync(batchHandle);

            CollectionAssert.AreEqual(new[]
            {
                "POST:/batch",
                $"DELETE:/batch/{expectedBatchId}"
            }, lastRequestUris.Select(uri => $"{uri.Item1}:{uri.Item2.AbsolutePath}"));
        }

        [Test]
        public async Task TestRollbackWithCancellationToken()
        {
            var expectedBatchId = Guid.NewGuid().ToString();
            nextResponse = new CreateBatchResponseModel { BatchId = expectedBatchId };
            var batchHandle = await fileShareApiClient.CreateBatchAsync(new BatchModel { BusinessUnit = "TestUnit" }, CancellationToken.None);
            Assert.AreEqual(expectedBatchId, batchHandle.Data.BatchId);

            nextResponseStatusCode = HttpStatusCode.NoContent;
            nextResponse = null;
            await fileShareApiClient.RollBackBatchAsync(batchHandle.Data);

            CollectionAssert.AreEqual(new[]
            {
                "POST:/batch",
                $"DELETE:/batch/{expectedBatchId}"
            }, lastRequestUris.Select(uri => $"{uri.Item1}:{uri.Item2.AbsolutePath}"));
        }

        [Test]
        public async Task TestCommitBatchSetsAuthorizationHeader()
        {
            var batchId = Guid.NewGuid().ToString();
            nextResponse = new CreateBatchResponseModel { BatchId = batchId };
            var batchHandle = new BatchHandle(batchId);

            await fileShareApiClient.CommitBatch(batchHandle);

            Assert.NotNull(fakeHttpClientFactory.HttpClient.DefaultRequestHeaders.Authorization);
            Assert.AreEqual("bearer", fakeHttpClientFactory.HttpClient.DefaultRequestHeaders.Authorization.Scheme);
            Assert.AreEqual(DUMMY_ACCESS_TOKEN, fakeHttpClientFactory.HttpClient.DefaultRequestHeaders.Authorization.Parameter);
        }

        [Test]
        public async Task TestRollBackBatchSetsAuthorizationHeader()
        {
            var batchId = Guid.NewGuid().ToString();
            nextResponse = new CreateBatchResponseModel { BatchId = batchId };
            var batchHandle = new BatchHandle(batchId);

            await fileShareApiClient.RollBackBatchAsync(batchHandle);

            Assert.NotNull(fakeHttpClientFactory.HttpClient.DefaultRequestHeaders.Authorization);
            Assert.AreEqual("bearer", fakeHttpClientFactory.HttpClient.DefaultRequestHeaders.Authorization.Scheme);
            Assert.AreEqual(DUMMY_ACCESS_TOKEN, fakeHttpClientFactory.HttpClient.DefaultRequestHeaders.Authorization.Parameter);
        }
    }
}