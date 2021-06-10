using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using NUnit.Framework;
using UKHO.FileShareClient;
using UKHO.FileShareClient.Models;
using UKHO.FileShareClientTests.Helpers;

namespace UKHO.FileShareClientTests
{
    public class GetBatchStatusTests
    {
        private object nextResponse = null;
        private IFileShareApiClient fileShareApiClient;
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
            nextResponseStatusCode = HttpStatusCode.OK;

            var config = new
            {
                BaseAddress = @"https://fss-tests.net/basePath/",
                AccessToken = "ACarefullyEncodedSecretAccessToken"
            };


            fileShareApiClient =
                new FileShareApiClient(fakeHttpClientFactory, config.BaseAddress, config.AccessToken);
        }

        [Test]
        public async Task TestBasicGetBatchStatus()
        {
            var batchId = "f382a514-aa1c-4709-aecd-ef06f1b963f5";
            var expectedBatchStatus = BatchStatusResponse.StatusEnum.Committed;
            nextResponse = new BatchStatusResponse
            {
                BatchId = batchId,
                Status = expectedBatchStatus
            };

            var batchStatusResponse = await fileShareApiClient.GetBatchStatusAsync(batchId);
            Assert.AreEqual(expectedBatchStatus, batchStatusResponse.Status);
            Assert.AreEqual($"/basePath/batch/{batchId}/status", lastRequestUri.AbsolutePath);
        }

        [Test]
        public async Task TestGetBatchStatusForABatchThatDoesNotExist()
        {
            var batchId = Guid.NewGuid();

            nextResponseStatusCode = HttpStatusCode.BadRequest;

            try
            {
                await fileShareApiClient.GetBatchStatusAsync(batchId.ToString());
                Assert.Fail("Expected to throw an exception");
            }
            catch (Exception e)
            {
                Assert.IsInstanceOf<HttpRequestException>(e);
            }

            Assert.AreEqual($"/basePath/batch/{batchId}/status", lastRequestUri.AbsolutePath);
        }

        [Test]
        public async Task TestGetBatchStatusForABatchThatHasBeenDeleted()
        {
            var batchId = Guid.NewGuid();

            nextResponseStatusCode = HttpStatusCode.Gone;

            try
            {
                await fileShareApiClient.GetBatchStatusAsync(batchId.ToString());
                Assert.Fail("Expected to throw an exception");
            }
            catch (Exception e)
            {
                Assert.IsInstanceOf<HttpRequestException>(e);
            }

            Assert.AreEqual($"/basePath/batch/{batchId}/status", lastRequestUri.AbsolutePath);
        }
    }
}