using System;
using System.Net;
using System.Threading.Tasks;
using NUnit.Framework;
using UKHO.FileShareClient;
using UKHO.FileShareClient.Models;

namespace UKHO.FileShareClientTests
{
    public class CreateBatchTests
    {
        private object nextResponse = null;
        private FileShareApiClient fileShareApiClient;
        private HttpStatusCode nextResponseStatusCode;
        private Uri lastRequestUri;

        [SetUp]
        public void Setup()
        {
            var fakeHttpClientFactory = new FakeFssHttpClientFactory(request =>
            {
                lastRequestUri = request.RequestUri;
                return (nextResponseStatusCode, nextResponse);
            });
            nextResponse = null;
            nextResponseStatusCode = HttpStatusCode.Created;
            lastRequestUri = null;

            var config = new
            {
                BaseAddress = @"https://fss-tests.net",
                AccessToken = "ACarefullyEncodedSecretAccessToken"
            };

            fileShareApiClient =
                new FileShareApiClient(fakeHttpClientFactory, config.BaseAddress, config.AccessToken);
        }

        [Test]
        public async Task TestCreateNewBatch()
        {
            var expectedBatchId = "newBatchId";
            nextResponse = new CreateBatchResponseModel {BatchId = expectedBatchId};
            var batchHandle = await fileShareApiClient.CreateBatchAsync(new BatchModel {BusinessUnit = "TestUnit"});
            Assert.IsNotNull(batchHandle);
            Assert.AreEqual(expectedBatchId, batchHandle.BatchId);

            Assert.AreEqual("/batch", lastRequestUri.AbsolutePath);
        }

        [Test]
        public async Task TestGetStatusOfNewBatch()
        {
            var expectedBatchId = Guid.NewGuid().ToString();
            nextResponse = new CreateBatchResponseModel {BatchId = expectedBatchId};
            var batchHandle = await fileShareApiClient.CreateBatchAsync(new BatchModel {BusinessUnit = "TestUnit"});
            Assert.IsNotNull(batchHandle);
            Assert.AreEqual(expectedBatchId, batchHandle.BatchId);


            nextResponse = new BatchStatusResponse
            {
                BatchId = Guid.Parse(expectedBatchId),
                Status = "Incomplete"
            };
            nextResponseStatusCode = HttpStatusCode.OK;
            lastRequestUri = null;

            var batchStatusResponse = await fileShareApiClient.GetBatchStatusAsync(batchHandle);
            Assert.AreEqual("Incomplete", batchStatusResponse.Status);
            Assert.AreEqual(expectedBatchId, batchStatusResponse.BatchId.ToString());
            // ReSharper disable once PossibleNullReferenceException - Will have been set during fileShareApiClient.GetBatchStatusAsync
            Assert.AreEqual($"/batch/{expectedBatchId}/status", lastRequestUri.AbsolutePath);
        }
    }
}