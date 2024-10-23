using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using FileShareClientTestsCommon.Helpers;
using NUnit.Framework;
using UKHO.FileShareAdminClient;
using UKHO.FileShareAdminClient.Models;

namespace FileShareAdminClientTests
{
    internal class AddFilesToBatchTests
    {
        private object _nextResponse;
        private FileShareApiAdminClient _fileShareApiAdminClient;
        private HttpStatusCode _nextResponseStatusCode;
        private List<(HttpMethod HttpMethod, Uri Uri)> _lastRequestUris;
        private List<string> _lastRequestBodies;
        private const int MaxBlockSize = 32;
        private FakeFssHttpClientFactory _fakeFssHttpClientFactory;
        private const string DUMMY_ACCESS_TOKEN = "ACarefullyEncodedSecretAccessToken";

        [SetUp]
        public void Setup()
        {
            _fakeFssHttpClientFactory = new FakeFssHttpClientFactory(request =>
            {
                _lastRequestUris.Add((request.Method, request.RequestUri));

                if (request.Content is StringContent content && request.Content.Headers.ContentLength.HasValue)
                {
                    _lastRequestBodies.Add(content.ReadAsStringAsync().Result);
                }
                else
                {
                    _lastRequestBodies.Add(null);
                }

                return (_nextResponseStatusCode, _nextResponse);
            });

            _nextResponse = new object();
            _nextResponseStatusCode = HttpStatusCode.Created;
            _lastRequestUris = new List<(HttpMethod HttpMethod, Uri Uri)>();
            _lastRequestBodies = new List<string>();
            _fileShareApiAdminClient = new FileShareApiAdminClient(_fakeFssHttpClientFactory, @"https://fss-tests.net", DUMMY_ACCESS_TOKEN, MaxBlockSize);
        }

        [TearDown]
        public void TearDown()
        {
            _fakeFssHttpClientFactory.Dispose();
        }

        [Test]
        public async Task TestUnseekableStreamThrowsException()
        {
            var expectedBatchId = Guid.NewGuid().ToString();
            _nextResponse = new CreateBatchResponseModel { BatchId = expectedBatchId };
            var batchHandle = await _fileShareApiAdminClient.CreateBatchAsync(new BatchModel { BusinessUnit = "TestUnit" });
            Assert.That(batchHandle.BatchId, Is.EqualTo(expectedBatchId));

            var stream1 = A.Fake<Stream>();
            A.CallTo(() => stream1.CanSeek).Returns(false);
            var filename1 = "File1.bin";
            var mimeType1 = "application/octet-stream";

            try
            {
                await _fileShareApiAdminClient.AddFileToBatchAsync(batchHandle, stream1, filename1, mimeType1, CancellationToken.None);
                Assert.Fail("Expected an exception");
            }
            catch (ArgumentException ex)
            {
                Assert.Multiple(() =>
                {
                    Assert.That(ex.ParamName, Is.EqualTo("stream"));
#if NET48
                    Assert.That(ex.Message, Is.EqualTo("The stream must be seekable.\r\nParameter name: stream"));
#elif NET8_0
                    Assert.That(ex.Message, Is.EqualTo("The stream must be seekable. (Parameter 'stream')"));
#else
                    Assert.Fail("Framework not catered for.");                    
#endif
                });
            }
        }

        [Test]
        public async Task TestUnseekableStreamThrowsExceptionWithCancellationToken()
        {
            var expectedBatchId = Guid.NewGuid().ToString();
            _nextResponse = new CreateBatchResponseModel { BatchId = expectedBatchId };
            var batchHandle = await _fileShareApiAdminClient.CreateBatchAsync(new BatchModel { BusinessUnit = "TestUnit" }, CancellationToken.None);
            Assert.That(batchHandle.Data.BatchId, Is.EqualTo(expectedBatchId));

            var stream1 = A.Fake<Stream>();
            A.CallTo(() => stream1.CanSeek).Returns(false);
            var filename1 = "File1.bin";
            var mimeType1 = "application/octet-stream";

            try
            {
                await _fileShareApiAdminClient.AddFileToBatchAsync(batchHandle.Data, stream1, filename1, mimeType1, CancellationToken.None);
                Assert.Fail("Expected an exception");
            }
            catch (ArgumentException ex)
            {
                Assert.Multiple(() =>
                {
                    Assert.That(ex.ParamName, Is.EqualTo("stream"));
#if NET48
                    Assert.That(ex.Message, Is.EqualTo("The stream must be seekable.\r\nParameter name: stream"));
#elif NET8_0
                    Assert.That(ex.Message, Is.EqualTo("The stream must be seekable. (Parameter 'stream')"));
#else
                    Assert.Fail("Framework not catered for.");                    
#endif
                });
            }
        }

        [Test]
        public async Task TestAddSmallFilesToBatch()
        {
            var expectedBatchId = Guid.NewGuid().ToString();
            _nextResponse = new CreateBatchResponseModel { BatchId = expectedBatchId };
            var batchHandle = await _fileShareApiAdminClient.CreateBatchAsync(new BatchModel { BusinessUnit = "TestUnit" });
            Assert.That(batchHandle.BatchId, Is.EqualTo(expectedBatchId));

            Stream stream1 = new MemoryStream(new byte[] { 1, 2, 3, 4, 5 });
            Stream stream2 = new MemoryStream(new byte[] { 2, 3, 4, 5, 6, 7, 8 });
            var filename1 = "File1.bin";
            var filename2 = "File2.bin";
            var mimeType1 = "application/octet-stream";
            var mimeType2 = "application/octet-stream";

            await _fileShareApiAdminClient.AddFileToBatchAsync(batchHandle, stream1, filename1, mimeType1);
            await _fileShareApiAdminClient.AddFileToBatchAsync(batchHandle, stream2, filename2, mimeType2);

            var expectedRequests = new[]
            {
                "POST:/batch",
                $"POST:/batch/{expectedBatchId}/files/{filename1}",
                $"PUT:/batch/{expectedBatchId}/files/{filename1}/00001",
                $"PUT:/batch/{expectedBatchId}/files/{filename1}",
                $"POST:/batch/{expectedBatchId}/files/{filename2}",
                $"PUT:/batch/{expectedBatchId}/files/{filename2}/00001",
                $"PUT:/batch/{expectedBatchId}/files/{filename2}"
            };
            var actualRequests = _lastRequestUris.Select(x => $"{x.HttpMethod}:{x.Uri?.AbsolutePath}");
            Assert.That(actualRequests, Is.EqualTo(expectedRequests));
        }

        [Test]
        public async Task TestAddSmallFilesToBatchWithCancellationToken()
        {
            var expectedBatchId = Guid.NewGuid().ToString();
            _nextResponse = new CreateBatchResponseModel { BatchId = expectedBatchId };
            var batchHandle = await _fileShareApiAdminClient.CreateBatchAsync(new BatchModel { BusinessUnit = "TestUnit" }, CancellationToken.None);
            Assert.That(batchHandle.Data.BatchId, Is.EqualTo(expectedBatchId));

            Stream stream1 = new MemoryStream(new byte[] { 1, 2, 3, 4, 5 });
            Stream stream2 = new MemoryStream(new byte[] { 2, 3, 4, 5, 6, 7, 8 });
            var filename1 = "File1.bin";
            var filename2 = "File2.bin";
            var mimeType1 = "application/octet-stream";
            var mimeType2 = "application/octet-stream";

            await _fileShareApiAdminClient.AddFileToBatchAsync(batchHandle.Data, stream1, filename1, mimeType1, CancellationToken.None);
            await _fileShareApiAdminClient.AddFileToBatchAsync(batchHandle.Data, stream2, filename2, mimeType2, CancellationToken.None);

            var expectedRequests = new[]
            {
                "POST:/batch",
                $"POST:/batch/{expectedBatchId}/files/{filename1}",
                $"PUT:/batch/{expectedBatchId}/files/{filename1}/00001",
                $"PUT:/batch/{expectedBatchId}/files/{filename1}",
                $"POST:/batch/{expectedBatchId}/files/{filename2}",
                $"PUT:/batch/{expectedBatchId}/files/{filename2}/00001",
                $"PUT:/batch/{expectedBatchId}/files/{filename2}"
            };
            var actualRequests = _lastRequestUris.Select(x => $"{x.HttpMethod}:{x.Uri?.AbsolutePath}");
            Assert.That(actualRequests, Is.EqualTo(expectedRequests));
        }

        [Test]
        public async Task TestAddSmallFilesToBatchWithFileAttributes()
        {
            var expectedBatchId = Guid.NewGuid().ToString();
            _nextResponse = new CreateBatchResponseModel { BatchId = expectedBatchId };
            var batchHandle = await _fileShareApiAdminClient.CreateBatchAsync(new BatchModel { BusinessUnit = "TestUnit" });
            Assert.That(batchHandle.BatchId, Is.EqualTo(expectedBatchId));

            Stream stream1 = new MemoryStream(new byte[] { 1, 2, 3, 4, 5 });
            Stream stream2 = new MemoryStream(new byte[] { 2, 3, 4, 5, 6, 7, 8 });
            var filename1 = "File1.bin";
            var filename2 = "File2.bin";
            var mimeType1 = "application/octet-stream";
            var mimeType2 = "application/octet-stream";

            await _fileShareApiAdminClient.AddFileToBatchAsync(batchHandle, stream1, filename1, mimeType1, new KeyValuePair<string, string>("fileAttributeKey1", "fileAttributeValue1"));
            await _fileShareApiAdminClient.AddFileToBatchAsync(batchHandle, stream2, filename2, mimeType2, new KeyValuePair<string, string>("fileAttributeKey2", "fileAttributeValue2"));

            var expectedRequests = new[]
            {
                "POST:/batch",
                $"POST:/batch/{expectedBatchId}/files/{filename1}",
                $"PUT:/batch/{expectedBatchId}/files/{filename1}/00001",
                $"PUT:/batch/{expectedBatchId}/files/{filename1}",
                $"POST:/batch/{expectedBatchId}/files/{filename2}",
                $"PUT:/batch/{expectedBatchId}/files/{filename2}/00001",
                $"PUT:/batch/{expectedBatchId}/files/{filename2}"
            };
            var actualRequests = _lastRequestUris.Select(x => $"{x.HttpMethod}:{x.Uri?.AbsolutePath}");
            var addFile1Request = _lastRequestBodies[1];
            var addFile2Request = _lastRequestBodies[4];
            Assert.Multiple(() =>
            {
                Assert.That(actualRequests, Is.EqualTo(expectedRequests));
                Assert.That(addFile1Request, Does.Contain("\"Key\":\"fileAttributeKey1\",\"Value\":\"fileAttributeValue1\""));
                Assert.That(addFile2Request, Does.Contain("\"Key\":\"fileAttributeKey2\",\"Value\":\"fileAttributeValue2\""));
            });
        }

        [Test]
        public async Task TestAddSmallFilesToBatchWithFileAttributesWithCancellationToken()
        {
            var expectedBatchId = Guid.NewGuid().ToString();
            _nextResponse = new CreateBatchResponseModel { BatchId = expectedBatchId };
            var batchHandle = await _fileShareApiAdminClient.CreateBatchAsync(new BatchModel { BusinessUnit = "TestUnit" }, CancellationToken.None);
            Assert.That(batchHandle.Data.BatchId, Is.EqualTo(expectedBatchId));

            Stream stream1 = new MemoryStream(new byte[] { 1, 2, 3, 4, 5 });
            Stream stream2 = new MemoryStream(new byte[] { 2, 3, 4, 5, 6, 7, 8 });
            var filename1 = "File1.bin";
            var filename2 = "File2.bin";
            var mimeType1 = "application/octet-stream";
            var mimeType2 = "application/octet-stream";

            await _fileShareApiAdminClient.AddFileToBatchAsync(batchHandle.Data, stream1, filename1, mimeType1, CancellationToken.None, new KeyValuePair<string, string>("fileAttributeKey1", "fileAttributeValue1"));
            await _fileShareApiAdminClient.AddFileToBatchAsync(batchHandle.Data, stream2, filename2, mimeType2, CancellationToken.None, new KeyValuePair<string, string>("fileAttributeKey2", "fileAttributeValue2"));

            var expectedRequests = new[]
            {
                "POST:/batch",
                $"POST:/batch/{expectedBatchId}/files/{filename1}",
                $"PUT:/batch/{expectedBatchId}/files/{filename1}/00001",
                $"PUT:/batch/{expectedBatchId}/files/{filename1}",
                $"POST:/batch/{expectedBatchId}/files/{filename2}",
                $"PUT:/batch/{expectedBatchId}/files/{filename2}/00001",
                $"PUT:/batch/{expectedBatchId}/files/{filename2}"
            };
            var actualRequests = _lastRequestUris.Select(x => $"{x.HttpMethod}:{x.Uri?.AbsolutePath}");
            var addFile1Request = _lastRequestBodies[1];
            var addFile2Request = _lastRequestBodies[4];
            Assert.Multiple(() =>
            {
                Assert.That(actualRequests, Is.EqualTo(expectedRequests));
                Assert.That(addFile1Request, Does.Contain("\"Key\":\"fileAttributeKey1\",\"Value\":\"fileAttributeValue1\""));
                Assert.That(addFile2Request, Does.Contain("\"Key\":\"fileAttributeKey2\",\"Value\":\"fileAttributeValue2\""));
            });
        }

        [Test]
        public async Task TestAddLargerFileToBatch()
        {
            var expectedBatchId = Guid.NewGuid().ToString();
            _nextResponse = new CreateBatchResponseModel { BatchId = expectedBatchId };
            var batchHandle = await _fileShareApiAdminClient.CreateBatchAsync(new BatchModel { BusinessUnit = "TestUnit" });
            Assert.That(batchHandle.BatchId, Is.EqualTo(expectedBatchId));

            Stream stream1 = new MemoryStream(new byte[MaxBlockSize * 3]);
            var filename1 = "File1.bin";
            var mimeType1 = "application/octet-stream";

            await _fileShareApiAdminClient.AddFileToBatchAsync(batchHandle, stream1, filename1, mimeType1);

            var expectedRequests = new[]
            {
                "POST:/batch",
                $"POST:/batch/{expectedBatchId}/files/{filename1}",
                $"PUT:/batch/{expectedBatchId}/files/{filename1}/00001",
                $"PUT:/batch/{expectedBatchId}/files/{filename1}/00002",
                $"PUT:/batch/{expectedBatchId}/files/{filename1}/00003",
                $"PUT:/batch/{expectedBatchId}/files/{filename1}"
            };
            var actualRequests = _lastRequestUris.Select(x => $"{x.HttpMethod}:{x.Uri?.AbsolutePath}");
            var writeBlockFileModel = _lastRequestBodies.Last()?.DeserialiseJson<WriteBlockFileModel>();
            var expectedBlockIds = new[] { "00001", "00002", "00003" };
            Assert.Multiple(() =>
            {
                Assert.That(actualRequests, Is.EqualTo(expectedRequests));
                Assert.That(writeBlockFileModel?.BlockIds, Is.EqualTo(expectedBlockIds));
            });
        }

        [Test]
        public async Task TestAddLargerFileToBatchWithCancellationToken()
        {
            var expectedBatchId = Guid.NewGuid().ToString();
            _nextResponse = new CreateBatchResponseModel { BatchId = expectedBatchId };
            var batchHandle = await _fileShareApiAdminClient.CreateBatchAsync(new BatchModel { BusinessUnit = "TestUnit" }, CancellationToken.None);
            Assert.That(batchHandle.Data.BatchId, Is.EqualTo(expectedBatchId));

            Stream stream1 = new MemoryStream(new byte[MaxBlockSize * 3]);
            var filename1 = "File1.bin";
            var mimeType1 = "application/octet-stream";

            await _fileShareApiAdminClient.AddFileToBatchAsync(batchHandle.Data, stream1, filename1, mimeType1, CancellationToken.None);

            var expectedRequests = new[]
            {
                "POST:/batch",
                $"POST:/batch/{expectedBatchId}/files/{filename1}",
                $"PUT:/batch/{expectedBatchId}/files/{filename1}/00001",
                $"PUT:/batch/{expectedBatchId}/files/{filename1}/00002",
                $"PUT:/batch/{expectedBatchId}/files/{filename1}/00003",
                $"PUT:/batch/{expectedBatchId}/files/{filename1}"
            };
            var actualRequests = _lastRequestUris.Select(x => $"{x.HttpMethod}:{x.Uri?.AbsolutePath}");
            var writeBlockFileModel = _lastRequestBodies.Last()?.DeserialiseJson<WriteBlockFileModel>();
            var expectedBlockIds = new[] { "00001", "00002", "00003" };
            Assert.Multiple(() =>
            {
                Assert.That(actualRequests, Is.EqualTo(expectedRequests));
                Assert.That(writeBlockFileModel?.BlockIds, Is.EqualTo(expectedBlockIds));
            });
        }

        [Test]
        public async Task TestProgressFeedbackWithAddLargerFileToBatch()
        {
            var expectedBatchId = Guid.NewGuid().ToString();
            _nextResponse = new CreateBatchResponseModel { BatchId = expectedBatchId };
            var batchHandle = await _fileShareApiAdminClient.CreateBatchAsync(new BatchModel { BusinessUnit = "TestUnit" });
            Assert.That(batchHandle.BatchId, Is.EqualTo(expectedBatchId));

            var stream1 = new MemoryStream(new byte[MaxBlockSize * 3 - 1]);
            var filename1 = "File1.bin";
            var mimeType1 = "application/octet-stream";

            var progressReports = new List<(int blocksComplete, int totalBlockCount)>();
            await _fileShareApiAdminClient.AddFileToBatchAsync(batchHandle, stream1, filename1, mimeType1, progressReports.Add);

            var expectedBlocksComplete = new[] { 0, 1, 2, 3 };
            var expectedTotalBlockCount = new[] { 3, 3, 3, 3 };
            Assert.Multiple(() =>
            {
                Assert.That(progressReports, Has.Count.EqualTo(4));
                Assert.That(progressReports.Select(r => r.blocksComplete), Is.EqualTo(expectedBlocksComplete));
                Assert.That(progressReports.Select(r => r.totalBlockCount), Is.EqualTo(expectedTotalBlockCount));
            });
        }

        [Test]
        public async Task TestProgressFeedbackWithAddLargerFileToBatchWithCancellationToken()
        {
            var expectedBatchId = Guid.NewGuid().ToString();
            _nextResponse = new CreateBatchResponseModel { BatchId = expectedBatchId };
            var batchHandle = await _fileShareApiAdminClient.CreateBatchAsync(new BatchModel { BusinessUnit = "TestUnit" }, CancellationToken.None);
            Assert.That(batchHandle.Data.BatchId, Is.EqualTo(expectedBatchId));

            var stream1 = new MemoryStream(new byte[MaxBlockSize * 3 - 1]);
            var filename1 = "File1.bin";
            var mimeType1 = "application/octet-stream";

            var progressReports = new List<(int blocksComplete, int totalBlockCount)>();
            await _fileShareApiAdminClient.AddFileToBatchAsync(batchHandle.Data, stream1, filename1, mimeType1, progressReports.Add, CancellationToken.None);

            var expectedBlocksComplete = new[] { 0, 1, 2, 3 };
            var expectedTotalBlockCount = new[] { 3, 3, 3, 3 };
            Assert.Multiple(() =>
            {
                Assert.That(progressReports, Has.Count.EqualTo(4));
                Assert.That(progressReports.Select(r => r.blocksComplete), Is.EqualTo(expectedBlocksComplete));
                Assert.That(progressReports.Select(r => r.totalBlockCount), Is.EqualTo(expectedTotalBlockCount));
            });
        }

        [Test]
        public async Task TestAddFileToBatchSetsAuthorizationHeader()
        {
            var batchId = Guid.NewGuid().ToString();
            _nextResponse = new CreateBatchResponseModel { BatchId = batchId };
            var batchHandle = new BatchHandle(batchId);

            Stream stream1 = new MemoryStream(new byte[] { 1, 2, 3, 4, 5 });
            var filename1 = "File1.bin";
            var mimeType1 = "application/octet-stream";

            await _fileShareApiAdminClient.AddFileToBatchAsync(batchHandle, stream1, filename1, mimeType1, CancellationToken.None);

            Assert.That(_fakeFssHttpClientFactory.HttpClient.DefaultRequestHeaders.Authorization, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(_fakeFssHttpClientFactory.HttpClient.DefaultRequestHeaders.Authorization.Scheme, Is.EqualTo("bearer"));
                Assert.That(_fakeFssHttpClientFactory.HttpClient.DefaultRequestHeaders.Authorization.Parameter, Is.EqualTo(DUMMY_ACCESS_TOKEN));
            });
        }
    }
}
