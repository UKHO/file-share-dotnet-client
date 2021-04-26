using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FakeItEasy;
using NUnit.Framework;
using UKHO.FileShareAdminClient;
using UKHO.FileShareAdminClient.Models;
using UKHO.FileShareClientTests.Helpers;

namespace UKHO.FileShareAdminClientTests
{
    internal class AddFilesToBatchTests
    {
        private object nextResponse = null;
        private FileShareApiAdminClient fileShareApiClient;
        private HttpStatusCode nextResponseStatusCode;
        private List<(HttpMethod, Uri)> lastRequestUris;
        private List<string> lastRequestBodies;
        private const int MaxBlockSize = 32;

        [SetUp]
        public void Setup()
        {
            var fakeHttpClientFactory = new FakeFssHttpClientFactory(request =>
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
                AccessToken = "ACarefullyEncodedSecretAccessToken"
            };

            fileShareApiClient =
                new FileShareApiAdminClient(fakeHttpClientFactory, config.BaseAddress, config.AccessToken,
                    MaxBlockSize);
        }

        [Test]
        public async Task TestUnseekableStreamThrowsException()
        {
            var expectedBatchId = Guid.NewGuid().ToString();
            nextResponse = new CreateBatchResponseModel {BatchId = expectedBatchId};
            var batchHandle = await fileShareApiClient.CreateBatchAsync(new BatchModel {BusinessUnit = "TestUnit"});
            Assert.AreEqual(expectedBatchId, batchHandle.BatchId);

            var stream1 = A.Fake<Stream>();
            A.CallTo(() => stream1.CanSeek).Returns(false);
            var filename1 = "File1.bin";
            var mimeType1 = "application/octet-stream";

            try
            {
                await fileShareApiClient.AddFileToBatch(batchHandle, stream1, filename1, mimeType1);
                Assert.Fail("Expected an exception");
            }
            catch (ArgumentException e)
            {
                Assert.AreEqual("stream", e.ParamName);
                Assert.AreEqual("The stream must be seekable. (Parameter 'stream')", e.Message);
            }
        }

        [Test]
        public async Task TestAddSmallFilesToBatch()
        {
            var expectedBatchId = Guid.NewGuid().ToString();
            nextResponse = new CreateBatchResponseModel {BatchId = expectedBatchId};
            var batchHandle = await fileShareApiClient.CreateBatchAsync(new BatchModel {BusinessUnit = "TestUnit"});
            Assert.AreEqual(expectedBatchId, batchHandle.BatchId);

            Stream stream1 = new MemoryStream(new byte[] {1, 2, 3, 4, 5});
            Stream stream2 = new MemoryStream(new byte[] {2, 3, 4, 5, 6, 7, 8});
            var filename1 = "File1.bin";
            var filename2 = "File2.bin";
            var mimeType1 = "application/octet-stream";
            var mimeType2 = "application/octet-stream";

            await fileShareApiClient.AddFileToBatch(batchHandle, stream1, filename1, mimeType1);
            await fileShareApiClient.AddFileToBatch(batchHandle, stream2, filename2, mimeType2);


            CollectionAssert.AreEqual(new[]
            {
                "POST:/batch",

                $"POST:/batch/{expectedBatchId}/files/{filename1}",
                $"PUT:/batch/{expectedBatchId}/files/{filename1}/00002",
                $"PUT:/batch/{expectedBatchId}/files/{filename1}",

                $"POST:/batch/{expectedBatchId}/files/{filename2}",
                $"PUT:/batch/{expectedBatchId}/files/{filename2}/00002",
                $"PUT:/batch/{expectedBatchId}/files/{filename2}"
            }, lastRequestUris.Select(uri => $"{uri.Item1}:{uri.Item2.AbsolutePath}"));
        }

        [Test]
        public async Task TestAddLargerFileToBatch()
        {
            var expectedBatchId = Guid.NewGuid().ToString();
            nextResponse = new CreateBatchResponseModel {BatchId = expectedBatchId};
            var batchHandle = await fileShareApiClient.CreateBatchAsync(new BatchModel {BusinessUnit = "TestUnit"});
            Assert.AreEqual(expectedBatchId, batchHandle.BatchId);

            Stream stream1 = new MemoryStream(new byte[MaxBlockSize * 3]);
            var filename1 = "File1.bin";
            var mimeType1 = "application/octet-stream";

            await fileShareApiClient.AddFileToBatch(batchHandle, stream1, filename1, mimeType1);


            CollectionAssert.AreEqual(new[]
            {
                "POST:/batch",

                $"POST:/batch/{expectedBatchId}/files/{filename1}",
                $"PUT:/batch/{expectedBatchId}/files/{filename1}/00002",
                $"PUT:/batch/{expectedBatchId}/files/{filename1}/00003",
                $"PUT:/batch/{expectedBatchId}/files/{filename1}/00004",
                $"PUT:/batch/{expectedBatchId}/files/{filename1}"
            }, lastRequestUris.Select(uri => $"{uri.Item1}:{uri.Item2.AbsolutePath}"));


            var writeBlockFileModel = lastRequestBodies.Last().DeserialiseJson<WriteBlockFileModel>();
            CollectionAssert.AreEqual(new[] {"00002", "00003", "00004"}, writeBlockFileModel.BlockIds);
        }
    }
}