using System.Net;
using FileShareClientTestsCommon.Helpers;
using UKHO.FileShareAdminClient;
using UKHO.FileShareAdminClient.Models;
using UKHO.FileShareClient.Models;

namespace FileShareAdminClientTests
{
    public class CreateBatchTests
    {
        private object _nextResponse;
        private FileShareApiAdminClient _fileShareApiAdminClient;
        private HttpStatusCode _nextResponseStatusCode;
        private Uri? _lastRequestUri;
        private List<string?> _lastRequestBodies;
        private FakeFssHttpClientFactory _fakeFssHttpClientFactory;
        private const string DUMMY_ACCESS_TOKEN = "ACarefullyEncodedSecretAccessToken";

        [SetUp]
        public void Setup()
        {
            _fakeFssHttpClientFactory = new FakeFssHttpClientFactory(request =>
            {
                _lastRequestUri = request.RequestUri;

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
            _lastRequestUri = null;
            _lastRequestBodies = [];
            _fileShareApiAdminClient = new FileShareApiAdminClient(_fakeFssHttpClientFactory, @"https://fss-tests.net", DUMMY_ACCESS_TOKEN);
        }

        [TearDown]
        public void TearDown()
        {
            _fakeFssHttpClientFactory.Dispose();
        }

        [Test]
        public async Task TestCreateNewBatch()
        {
            var expectedBatchId = "newBatchId";
            _nextResponse = new CreateBatchResponseModel { BatchId = expectedBatchId };

            var batchHandle = await _fileShareApiAdminClient.CreateBatchAsync(new BatchModel { BusinessUnit = "TestUnit" });

            Assert.That(batchHandle, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(batchHandle.BatchId, Is.EqualTo(expectedBatchId));
                Assert.That(_lastRequestUri?.AbsolutePath, Is.EqualTo("/batch"));
            });
        }

        [Test]
        public async Task TestCreateNewBatchWithCancellationToken()
        {
            var expectedBatchId = "newBatchId";
            _nextResponse = new BatchHandle(expectedBatchId);

            var createBatchResult = await _fileShareApiAdminClient.CreateBatchAsync(new BatchModel { BusinessUnit = "TestUnit" }, CancellationToken.None);

            Assert.That(createBatchResult, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(createBatchResult.Data.BatchId, Is.EqualTo(expectedBatchId));
                Assert.That(_lastRequestUri?.AbsolutePath, Is.EqualTo("/batch"));
            });
        }

        [Test]
        public async Task TestCreateNewBatchWithCancellationTokenWithInvalidBusinessUnit()
        {
            _nextResponse = new Result<BatchHandle> { IsSuccess = false, StatusCode = 400, Errors = [new Error { Description = "Business Unit is invalid", Source = "BusinessUnit" }], Data = new BatchHandle(null) };
            _nextResponseStatusCode = HttpStatusCode.BadRequest;

            var createBatchResult = await _fileShareApiAdminClient.CreateBatchAsync(new BatchModel { BusinessUnit = "TestUnit" }, CancellationToken.None);

            Assert.That(createBatchResult, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(createBatchResult.Data, Is.Null);
                Assert.That(createBatchResult.StatusCode, Is.EqualTo((int)_nextResponseStatusCode));
                Assert.That(_lastRequestUri?.AbsolutePath, Is.EqualTo("/batch"));
                Assert.That(createBatchResult.Errors[0].Description, Is.EqualTo("Business Unit is invalid"));
            });
        }

        [Test]
        public async Task TestCreateNewBatchWithAttributesExpiryDateAndAcl()
        {
            var expectedBatchId = "newBatchId";
            _nextResponse = new CreateBatchResponseModel { BatchId = expectedBatchId };
            var batchModel = new BatchModel
            {
                BusinessUnit = "TestUnit",
                Acl = new Acl
                {
                    ReadGroups = new List<string> { "public" },
                    ReadUsers = new List<string> { "userA", "userB" }
                },
                Attributes = new Dictionary<string, string> { { "Product", "AVCS" }, { "Week", "23" } }.ToList(),
                ExpiryDate = DateTime.UtcNow
            };

            var batchHandle = await _fileShareApiAdminClient.CreateBatchAsync(batchModel);

            Assert.That(batchHandle, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(batchHandle.BatchId, Is.EqualTo(expectedBatchId));
                Assert.That(_lastRequestUri?.AbsolutePath, Is.EqualTo("/batch"));
                Assert.That(_lastRequestBodies, Has.Count.EqualTo(1));
            });
            var actualRequest = _lastRequestBodies[0]?.DeserialiseJson<BatchModel>();
            Assert.That(actualRequest, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(actualRequest.BusinessUnit, Is.EqualTo(batchModel.BusinessUnit));
                Assert.That(actualRequest.Attributes, Is.EqualTo(batchModel.Attributes));
                Assert.That(actualRequest.Acl.ReadGroups, Is.EqualTo(batchModel.Acl.ReadGroups));
                Assert.That(actualRequest.Acl.ReadUsers, Is.EqualTo(batchModel.Acl.ReadUsers));
                Assert.That(actualRequest.ExpiryDate, Is.EqualTo(batchModel.ExpiryDate.Value.TruncateToMilliseconds()));
            });
        }

        [Test]
        public async Task TestGetStatusOfNewBatch()
        {
            var expectedBatchId = Guid.NewGuid().ToString();
            _nextResponse = new CreateBatchResponseModel
            {
                BatchId = expectedBatchId
            };

            var batchHandle = await _fileShareApiAdminClient.CreateBatchAsync(new BatchModel { BusinessUnit = "TestUnit" });

            Assert.That(batchHandle, Is.Not.Null);
            Assert.That(batchHandle.BatchId, Is.EqualTo(expectedBatchId));

            _nextResponse = new BatchStatusResponse
            {
                BatchId = expectedBatchId,
                Status = BatchStatusResponse.StatusEnum.Incomplete
            };
            _nextResponseStatusCode = HttpStatusCode.OK;
            _lastRequestUri = null;

            var batchStatusResponse = await _fileShareApiAdminClient.GetBatchStatusAsync(batchHandle);

            Assert.Multiple(() =>
            {
                Assert.That(batchStatusResponse.Status, Is.EqualTo(BatchStatusResponse.StatusEnum.Incomplete));
                Assert.That(batchStatusResponse.BatchId, Is.EqualTo(expectedBatchId));
                Assert.That(_lastRequestUri?.AbsolutePath, Is.EqualTo($"/batch/{expectedBatchId}/status"));
            });
        }

        [Test]
        public async Task TestGetStatusOfNewBatchWithCancellationToken()
        {
            var expectedBatchId = Guid.NewGuid().ToString();
            _nextResponse = new CreateBatchResponseModel
            {
                BatchId = expectedBatchId
            };

            var batchHandle = await _fileShareApiAdminClient.CreateBatchAsync(new BatchModel { BusinessUnit = "TestUnit" }, CancellationToken.None);

            Assert.That(batchHandle, Is.Not.Null);
            Assert.That(batchHandle.Data.BatchId, Is.EqualTo(expectedBatchId));

            _nextResponse = new BatchStatusResponse
            {
                BatchId = expectedBatchId,
                Status = BatchStatusResponse.StatusEnum.Incomplete
            };
            _nextResponseStatusCode = HttpStatusCode.OK;
            _lastRequestUri = null;

            var batchStatusResponse = await _fileShareApiAdminClient.GetBatchStatusAsync(batchHandle.Data);

            Assert.Multiple(() =>
            {
                Assert.That(batchStatusResponse.Status, Is.EqualTo(BatchStatusResponse.StatusEnum.Incomplete));
                Assert.That(batchStatusResponse.BatchId, Is.EqualTo(expectedBatchId));
                Assert.That(_lastRequestUri?.AbsolutePath, Is.EqualTo($"/batch/{expectedBatchId}/status"));
            });
        }

        [Test]
        public async Task TestCreateNewBatchSetsAuthorizationHeader()
        {
            _nextResponse = new CreateBatchResponseModel { BatchId = "newBatchId" };

            await _fileShareApiAdminClient.CreateBatchAsync(new BatchModel { BusinessUnit = "TestUnit" }, CancellationToken.None);

            Assert.That(_fakeFssHttpClientFactory.HttpClient.DefaultRequestHeaders.Authorization, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(_fakeFssHttpClientFactory.HttpClient.DefaultRequestHeaders.Authorization.Scheme, Is.EqualTo("bearer"));
                Assert.That(_fakeFssHttpClientFactory.HttpClient.DefaultRequestHeaders.Authorization.Parameter, Is.EqualTo(DUMMY_ACCESS_TOKEN));
            });
        }
    }
}
