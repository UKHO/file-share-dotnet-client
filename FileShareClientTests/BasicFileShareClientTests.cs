using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using NUnit.Framework;
using UKHO.FileShareClient;
using UKHO.FileShareClient.Models;

namespace UKHO.FileShareClientTests
{
    public class BasicFileShareClientTests
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
            nextResponseStatusCode = HttpStatusCode.OK;

            var config = new
            {
                BaseAddress = @"https://fss-tests.net",
                AccessToken = "ACarefullyEncodedSecretAccessToken"
            };


            fileShareApiClient =
                new FileShareApiClient(fakeHttpClientFactory, config.BaseAddress, config.AccessToken);
        }

        [Test]
        public async Task TestBasicGetBatchStatus()
        {
            var batchId = "f382a514-aa1c-4709-aecd-ef06f1b963f5";
            var expectedBatchStatus = "TestBatch";
            nextResponse = new BatchStatusResponse()
            {
                BatchId = Guid.Parse(batchId),
                Status = expectedBatchStatus
            };

            var batchStatusResponse = await fileShareApiClient.GetBatchStatusAsync(batchId);
            Assert.AreEqual(expectedBatchStatus, batchStatusResponse.Status);
            Assert.AreEqual($"/batch/{batchId}/status",lastRequestUri.AbsolutePath);
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
            Assert.AreEqual($"/batch/{batchId}/status", lastRequestUri.AbsolutePath);
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
            Assert.AreEqual($"/batch/{batchId}/status", lastRequestUri.AbsolutePath);
        }
    }
}