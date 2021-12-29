using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using UKHO.FileShareAdminClient;
using UKHO.FileShareAdminClient.Models;
using UKHO.FileShareAdminClient.Models.Response;
using UKHO.FileShareClient.Models;
using UKHO.FileShareClientTests.Helpers;

namespace UKHO.FileShareAdminClientTests
{
    public class CreateBatchTests
    {
        private object nextResponse;
        private IFileShareApiAdminClient fileShareApiClient;
        private HttpStatusCode nextResponseStatusCode;
        private Uri lastRequestUri;
        private List<string> lastRequestBodies;
        private FakeFssHttpClientFactory fakeHttpClientFactory;
        private const string DUMMY_ACCESS_TOKEN = "ACarefullyEncodedSecretAccessToken";

        [SetUp]
        public void Setup()
        {
            fakeHttpClientFactory = new FakeFssHttpClientFactory(request =>
            {
                lastRequestUri = request.RequestUri;
                if (request.Content is StringContent content && request.Content.Headers.ContentLength.HasValue)
                    lastRequestBodies.Add(content.ReadAsStringAsync().Result);
                else
                    lastRequestBodies.Add(null);
                return (nextResponseStatusCode, nextResponse);
            });
            nextResponse = null;
            nextResponseStatusCode = HttpStatusCode.Created;
            lastRequestUri = null;
            lastRequestBodies = new List<string>();

            var config = new
            {
                BaseAddress = @"https://fss-tests.net",
                AccessToken = DUMMY_ACCESS_TOKEN
            };

            fileShareApiClient =
                new FileShareApiAdminClient(fakeHttpClientFactory, config.BaseAddress, config.AccessToken);
        }

        [Test]
        public async Task TestCreateNewBatch()
        {
            var expectedBatchId = "newBatchId";
            nextResponse = new CreateBatchResponseModel { BatchId = expectedBatchId };
            var batchHandle = await fileShareApiClient.CreateBatchAsync(new BatchModel { BusinessUnit = "TestUnit" });
            Assert.IsNotNull(batchHandle);
            Assert.AreEqual(expectedBatchId, batchHandle.BatchId);

            Assert.AreEqual("/batch", lastRequestUri.AbsolutePath);
        }

        [Test]
        public async Task TestCreateNewBatchWithCancellationToken()
        {
            var expectedBatchId = "newBatchId";
            nextResponse = new BatchHandle(expectedBatchId);
            var createBatchResult = await fileShareApiClient.CreateBatchAsync(new BatchModel { BusinessUnit = "TestUnit" }, CancellationToken.None);
            Assert.IsNotNull(createBatchResult);
            Assert.AreEqual(expectedBatchId, createBatchResult.Data.BatchId);
            Assert.AreEqual("/batch", lastRequestUri.AbsolutePath);
        }

        [Test]
        public async Task TestCreateNewBatchWithCancellationTokenWithInvalidBusinessUnit()
        {            
            nextResponse = new Result<BatchHandle>() { IsSuccess = false, StatusCode = 400, Errors = new List<Error>() { new Error() { Description = "Business Unit is invalid", Source = "BusinessUnit" } }, Data = new BatchHandle(null) };
            nextResponseStatusCode = HttpStatusCode.BadRequest;
            var createBatchResult = await fileShareApiClient.CreateBatchAsync(new BatchModel { BusinessUnit = "TestUnit" }, CancellationToken.None);
            Assert.IsNull(createBatchResult.Data);
            Assert.AreEqual(createBatchResult.StatusCode, (int)nextResponseStatusCode);
            Assert.AreEqual("/batch", lastRequestUri.AbsolutePath);
        }

        [Test]
        public async Task TestCreateNewBatchWithAttributesExpiryDateAndAcl()
        {
            var expectedBatchId = "newBatchId";
            nextResponse = new CreateBatchResponseModel { BatchId = expectedBatchId };
            var batchModel = new BatchModel
            {
                BusinessUnit = "TestUnit",
                Acl = new Acl()
                {
                    ReadGroups = new List<string> { "public" },
                    ReadUsers = new List<string> { "userA", "userB" }
                },
                Attributes = new Dictionary<string, string> { { "Product", "AVCS" }, { "Week", "23" } }.ToList(),
                ExpiryDate = DateTime.UtcNow
            };

            var batchHandle = await fileShareApiClient.CreateBatchAsync(batchModel);

            Assert.IsNotNull(batchHandle);
            Assert.AreEqual(expectedBatchId, batchHandle.BatchId);

            Assert.AreEqual("/batch", lastRequestUri.AbsolutePath);
            var actualRequest = lastRequestBodies.Single().DeserialiseJson<BatchModel>();

            Assert.AreEqual(batchModel.BusinessUnit, actualRequest.BusinessUnit);
            CollectionAssert.AreEqual(batchModel.Attributes, actualRequest.Attributes);
            CollectionAssert.AreEqual(batchModel.Acl.ReadGroups, actualRequest.Acl.ReadGroups);
            CollectionAssert.AreEqual(batchModel.Acl.ReadUsers, actualRequest.Acl.ReadUsers);
            Assert.AreEqual(batchModel.ExpiryDate.Value.TruncateToMilliseconds(), actualRequest.ExpiryDate);
        }

        [Test]
        public async Task TestGetStatusOfNewBatch()
        {
            var expectedBatchId = Guid.NewGuid().ToString();
            nextResponse = new CreateBatchResponseModel
            {
                BatchId = expectedBatchId
            };

            var batchHandle =
                await fileShareApiClient.CreateBatchAsync(new BatchModel { BusinessUnit = "TestUnit" });

            Assert.IsNotNull(batchHandle);
            Assert.AreEqual(expectedBatchId, batchHandle.BatchId);
            nextResponse = new BatchStatusResponse
            {
                BatchId = expectedBatchId,
                Status = BatchStatusResponse.StatusEnum.Incomplete
            };
            nextResponseStatusCode = HttpStatusCode.OK;
            lastRequestUri = null;

            var batchStatusResponse = await fileShareApiClient.GetBatchStatusAsync(batchHandle);
            Assert.AreEqual(BatchStatusResponse.StatusEnum.Incomplete, batchStatusResponse.Status);
            Assert.AreEqual(expectedBatchId, batchStatusResponse.BatchId);

            // ReSharper disable once PossibleNullReferenceException - Will have been set during fileShareApiClient.GetBatchStatusAsync
            Assert.AreEqual($"/batch/{expectedBatchId}/status", lastRequestUri.AbsolutePath);
        }

        [Test]
        public async Task TestGetStatusOfNewBatchWithCancellationToken()
        {
            var expectedBatchId = Guid.NewGuid().ToString();
            nextResponse = new CreateBatchResponseModel
            {
                BatchId = expectedBatchId
            };

            var batchHandle =
                await fileShareApiClient.CreateBatchAsync(new BatchModel { BusinessUnit = "TestUnit" }, CancellationToken.None);

            Assert.IsNotNull(batchHandle);
            Assert.AreEqual(expectedBatchId, batchHandle.Data.BatchId);
            nextResponse = new BatchStatusResponse
            {
                BatchId = expectedBatchId,
                Status = BatchStatusResponse.StatusEnum.Incomplete
            };
            nextResponseStatusCode = HttpStatusCode.OK;
            lastRequestUri = null;

            var batchStatusResponse = await fileShareApiClient.GetBatchStatusAsync(batchHandle.Data);
            Assert.AreEqual(BatchStatusResponse.StatusEnum.Incomplete, batchStatusResponse.Status);
            Assert.AreEqual(expectedBatchId, batchStatusResponse.BatchId);

            // ReSharper disable once PossibleNullReferenceException - Will have been set during fileShareApiClient.GetBatchStatusAsync
            Assert.AreEqual($"/batch/{expectedBatchId}/status", lastRequestUri.AbsolutePath);
        }

        [Test]
        public async Task TestCreateNewBatchSetsAuthorizationHeader()
        {
            nextResponse = new CreateBatchResponseModel { BatchId = "newBatchId" };
            await fileShareApiClient.CreateBatchAsync(new BatchModel { BusinessUnit = "TestUnit" }, CancellationToken.None);

            Assert.NotNull(fakeHttpClientFactory.HttpClient.DefaultRequestHeaders.Authorization);
            Assert.AreEqual("bearer", fakeHttpClientFactory.HttpClient.DefaultRequestHeaders.Authorization.Scheme);
            Assert.AreEqual(DUMMY_ACCESS_TOKEN, fakeHttpClientFactory.HttpClient.DefaultRequestHeaders.Authorization.Parameter);
        }
    }
}