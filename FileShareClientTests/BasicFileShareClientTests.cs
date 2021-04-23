using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
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

        [SetUp]
        public void Setup()
        {
            var fakeHttpClientFactory = new FakeFSSHttpClientFactory(request => (nextResponseStatusCode, nextResponse));
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

            var status = await fileShareApiClient.GetBatchStatusAsync(batchId);
            Assert.AreEqual(expectedBatchStatus, status.Status);
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
                //Assert.AreEqual(200, ((System.Net.Http.HttpRequestException)e ).StatusCode);
            }

        }
    }

    public class FakeFSSHttpClientFactory : DelegatingHandler, IHttpClientFactory
    {
        private readonly Func<HttpRequestMessage, (HttpStatusCode, object)> httpMessageHandler;

        public FakeFSSHttpClientFactory(Func<HttpRequestMessage, (HttpStatusCode, object)> httpMessageHandler)
        {
            this.httpMessageHandler = httpMessageHandler;
        }

        public HttpClient CreateClient(string name)
        {
            return new HttpClient(this);
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var responseData = httpMessageHandler(request);
            var response = new HttpResponseMessage()
            {
                StatusCode = responseData.Item1,
            };
            if (responseData.Item2 != null)
                response.Content = new StringContent(JsonConvert.SerializeObject(responseData.Item2), Encoding.UTF8,
                    "application/json");

            return Task.FromResult(response);
        }
    }
}