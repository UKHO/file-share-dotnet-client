using System.Net;
using FileShareClientTestsCommon.Helpers;
using UKHO.FileShareAdminClient;
using UKHO.FileShareAdminClient.Models;

namespace FileShareAdminClientTests
{
    internal class NewBatchCommitAndRollbackTests
    {
        private object _nextResponse;
        private FileShareApiAdminClient _fileShareApiAdminClient;
        private HttpStatusCode _nextResponseStatusCode;
        private List<(HttpMethod HttpMethod, Uri? Uri)> _lastRequestUris;
        private List<string?> _lastRequestBodies;
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
            _lastRequestUris = [];
            _lastRequestBodies = [];
            _fileShareApiAdminClient = new FileShareApiAdminClient(_fakeFssHttpClientFactory, @"https://fss-tests.net", DUMMY_ACCESS_TOKEN, MaxBlockSize);
        }

        [TearDown]
        public void TearDown()
        {
            _fakeFssHttpClientFactory.Dispose();
        }

        [Test]
        public async Task TestCommitNewBatch()
        {
            var expectedBatchId = Guid.NewGuid().ToString();
            _nextResponse = new CreateBatchResponseModel { BatchId = expectedBatchId };
            var batchHandle = await _fileShareApiAdminClient.CreateBatchAsync(new BatchModel { BusinessUnit = "TestUnit" });
            Assert.That(batchHandle.BatchId, Is.EqualTo(expectedBatchId));

            _nextResponse = new object();
            Stream stream1 = new MemoryStream(new byte[MaxBlockSize]);
            Stream stream2 = new MemoryStream(new byte[MaxBlockSize * 4]);
            var filename1 = "File1.bin";
            var filename2 = "File2.bin";
            var mimeType1 = "application/octet-stream";

            await _fileShareApiAdminClient.AddFileToBatch(batchHandle, stream1, filename1, mimeType1);
            await _fileShareApiAdminClient.AddFileToBatch(batchHandle, stream2, filename2, mimeType1);

            await _fileShareApiAdminClient.CommitBatch(batchHandle);

            var expectedRequests = new[]
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
            };
            var actualRequests = _lastRequestUris.Select(x => $"{x.HttpMethod}:{x.Uri?.AbsolutePath}");
            var batchCommitModel = _lastRequestBodies.Last()?.DeserialiseJson<List<FileDetail>>();
            Assert.Multiple(() =>
            {
                Assert.That(actualRequests, Is.EqualTo(expectedRequests));
                Assert.That(batchCommitModel?.Select(f => f.FileName), Is.EqualTo(new object[] { filename1, filename2 }));
            });
        }

        [Test]
        public async Task TestCommitNewBatchWithCancellationToken()
        {
            var expectedBatchId = Guid.NewGuid().ToString();
            _nextResponse = new CreateBatchResponseModel { BatchId = expectedBatchId };
            var batchHandle = await _fileShareApiAdminClient.CreateBatchAsync(new BatchModel { BusinessUnit = "TestUnit" }, CancellationToken.None);
            Assert.That(batchHandle.Data.BatchId, Is.EqualTo(expectedBatchId));

            _nextResponse = new object();
            Stream stream1 = new MemoryStream(new byte[MaxBlockSize]);
            Stream stream2 = new MemoryStream(new byte[MaxBlockSize * 4]);
            var filename1 = "File1.bin";
            var filename2 = "File2.bin";
            var mimeType1 = "application/octet-stream";

            await _fileShareApiAdminClient.AddFileToBatch(batchHandle.Data, stream1, filename1, mimeType1, CancellationToken.None);
            await _fileShareApiAdminClient.AddFileToBatch(batchHandle.Data, stream2, filename2, mimeType1, CancellationToken.None);

            await _fileShareApiAdminClient.CommitBatch(batchHandle.Data, CancellationToken.None);

            var expectedRequests = new[]
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
            };
            var actualRequests = _lastRequestUris.Select(x => $"{x.HttpMethod}:{x.Uri?.AbsolutePath}");
            var batchCommitModel = _lastRequestBodies.Last()?.DeserialiseJson<List<FileDetail>>();
            Assert.Multiple(() =>
            {
                Assert.That(actualRequests, Is.EqualTo(expectedRequests));
                Assert.That(batchCommitModel?.Select(f => f.FileName), Is.EqualTo(new object[] { filename1, filename2 }));
            });
        }

        [Test]
        public async Task TestRollback()
        {
            var expectedBatchId = Guid.NewGuid().ToString();
            _nextResponse = new CreateBatchResponseModel { BatchId = expectedBatchId };
            var batchHandle = await _fileShareApiAdminClient.CreateBatchAsync(new BatchModel { BusinessUnit = "TestUnit" });
            Assert.That(batchHandle.BatchId, Is.EqualTo(expectedBatchId));

            _nextResponseStatusCode = HttpStatusCode.NoContent;
            _nextResponse = new object();
            await _fileShareApiAdminClient.RollBackBatchAsync(batchHandle);

            var expectedRequests = new[]
            {
                "POST:/batch",
                $"DELETE:/batch/{expectedBatchId}"
            };
            var actualRequests = _lastRequestUris.Select(x => $"{x.HttpMethod}:{x.Uri?.AbsolutePath}");
            Assert.That(actualRequests, Is.EqualTo(expectedRequests));
        }

        [Test]
        public async Task TestRollbackWithCancellationToken()
        {
            var expectedBatchId = Guid.NewGuid().ToString();
            _nextResponse = new CreateBatchResponseModel { BatchId = expectedBatchId };
            var batchHandle = await _fileShareApiAdminClient.CreateBatchAsync(new BatchModel { BusinessUnit = "TestUnit" }, CancellationToken.None);
            Assert.That(batchHandle.Data.BatchId, Is.EqualTo(expectedBatchId));

            _nextResponseStatusCode = HttpStatusCode.NoContent;
            _nextResponse = new object();
            await _fileShareApiAdminClient.RollBackBatchAsync(batchHandle.Data, CancellationToken.None);

            var expectedRequests = new[]
            {
                "POST:/batch",
                $"DELETE:/batch/{expectedBatchId}"
            };
            var actualRequests = _lastRequestUris.Select(x => $"{x.HttpMethod}:{x.Uri?.AbsolutePath}");
            Assert.That(actualRequests, Is.EqualTo(expectedRequests));
        }

        [Test]
        public async Task TestCommitBatchSetsAuthorizationHeader()
        {
            var batchId = Guid.NewGuid().ToString();
            _nextResponse = new CreateBatchResponseModel { BatchId = batchId };
            var batchHandle = new BatchHandle(batchId);

            await _fileShareApiAdminClient.CommitBatch(batchHandle);

            Assert.That(_fakeFssHttpClientFactory.HttpClient.DefaultRequestHeaders.Authorization, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(_fakeFssHttpClientFactory.HttpClient.DefaultRequestHeaders.Authorization.Scheme, Is.EqualTo("bearer"));
                Assert.That(_fakeFssHttpClientFactory.HttpClient.DefaultRequestHeaders.Authorization.Parameter, Is.EqualTo(DUMMY_ACCESS_TOKEN));
            });
        }

        [Test]
        public async Task TestRollBackBatchSetsAuthorizationHeader()
        {
            var batchId = Guid.NewGuid().ToString();
            _nextResponse = new CreateBatchResponseModel { BatchId = batchId };
            var batchHandle = new BatchHandle(batchId);

            await _fileShareApiAdminClient.RollBackBatchAsync(batchHandle);

            Assert.That(_fakeFssHttpClientFactory.HttpClient.DefaultRequestHeaders.Authorization, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(_fakeFssHttpClientFactory.HttpClient.DefaultRequestHeaders.Authorization.Scheme, Is.EqualTo("bearer"));
                Assert.That(_fakeFssHttpClientFactory.HttpClient.DefaultRequestHeaders.Authorization.Parameter, Is.EqualTo(DUMMY_ACCESS_TOKEN));
            });
        }
    }
}
