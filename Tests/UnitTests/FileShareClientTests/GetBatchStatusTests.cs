using System.Net;
using FileShareClientTestsCommon.Helpers;
using UKHO.FileShareClient;
using UKHO.FileShareClient.Models;

namespace FileShareClientTests
{
    public class GetBatchStatusTests
    {
        private object _nextResponse;
        private FileShareApiClient _fileShareApiClient;
        private HttpStatusCode _nextResponseStatusCode;
        private Uri? _lastRequestUri;
        private FakeFssHttpClientFactory _fakeFssHttpClientFactory;
        private const string DUMMY_ACCESS_TOKEN = "ACarefullyEncodedSecretAccessToken";

        [SetUp]
        public void Setup()
        {
            _fakeFssHttpClientFactory = new FakeFssHttpClientFactory(request =>
            {
                _lastRequestUri = request.RequestUri;
                return (_nextResponseStatusCode, _nextResponse);
            });

            _nextResponse = new object();
            _nextResponseStatusCode = HttpStatusCode.OK;
            _fileShareApiClient = new FileShareApiClient(_fakeFssHttpClientFactory, @"https://fss-tests.net/basePath/", DUMMY_ACCESS_TOKEN);
        }

        [TearDown]
        public void TearDown()
        {
            _fakeFssHttpClientFactory.Dispose();
        }

        [Test]
        public async Task TestBasicGetBatchStatus()
        {
            var batchId = "f382a514-aa1c-4709-aecd-ef06f1b963f5";
            var expectedBatchStatus = BatchStatusResponse.StatusEnum.Committed;
            _nextResponse = new BatchStatusResponse
            {
                BatchId = batchId,
                Status = expectedBatchStatus
            };

            var batchStatusResponse = await _fileShareApiClient.GetBatchStatusAsync(batchId);

            Assert.Multiple(() =>
            {
                Assert.That(batchStatusResponse.Status, Is.EqualTo(expectedBatchStatus));
                Assert.That(_lastRequestUri?.AbsolutePath, Is.EqualTo($"/basePath/batch/{batchId}/status"));
            });
        }

        [Test]
        public async Task TestGetBatchStatusForABatchThatDoesNotExist()
        {
            var batchId = Guid.NewGuid();
            _nextResponseStatusCode = HttpStatusCode.BadRequest;

            try
            {
                await _fileShareApiClient.GetBatchStatusAsync(batchId.ToString());

                Assert.Fail("Expected to throw an exception");
            }
            catch (Exception e)
            {
                Assert.That(e, Is.InstanceOf<HttpRequestException>());
            }

            Assert.That(_lastRequestUri?.AbsolutePath, Is.EqualTo($"/basePath/batch/{batchId}/status"));
        }

        [Test]
        public async Task TestGetBatchStatusForABatchThatHasBeenDeleted()
        {
            var batchId = Guid.NewGuid();
            _nextResponseStatusCode = HttpStatusCode.Gone;

            try
            {
                await _fileShareApiClient.GetBatchStatusAsync(batchId.ToString());

                Assert.Fail("Expected to throw an exception");
            }
            catch (Exception e)
            {
                Assert.That(e, Is.InstanceOf<HttpRequestException>());
            }

            Assert.That(_lastRequestUri?.AbsolutePath, Is.EqualTo($"/basePath/batch/{batchId}/status"));
        }

        [Test]
        public async Task TestGetBatchStatusSetsAuthorizationHeader()
        {
            var batchId = "f382a514-aa1c-4709-aecd-ef06f1b963f5";
            var expectedBatchStatus = BatchStatusResponse.StatusEnum.Committed;
            _nextResponse = new BatchStatusResponse
            {
                BatchId = batchId,
                Status = expectedBatchStatus
            };

            await _fileShareApiClient.GetBatchStatusAsync(batchId);

            Assert.That(_fakeFssHttpClientFactory.HttpClient.DefaultRequestHeaders.Authorization, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(_fakeFssHttpClientFactory.HttpClient.DefaultRequestHeaders.Authorization.Scheme, Is.EqualTo("bearer"));
                Assert.That(_fakeFssHttpClientFactory.HttpClient.DefaultRequestHeaders.Authorization.Parameter, Is.EqualTo(DUMMY_ACCESS_TOKEN));
            });
        }
    }
}
