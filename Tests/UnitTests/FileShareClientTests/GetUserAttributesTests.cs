using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using NUnit.Framework;
using UKHO.FileShareClient;
using UKHO.FileShareClientTests.Helpers;

namespace UKHO.FileShareClientTests
{
    public class GetUserAttributesTests
    {
        private object nextResponse;
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
        public async Task TestSimpleGetAttributes()
        {
            nextResponse = new List<string> {"One", "Two"};
            var attributes = await fileShareApiClient.GetUserAttributesAsync();
            Assert.AreEqual("/basePath/attributes", lastRequestUri.AbsolutePath);
            Assert.AreEqual("", lastRequestUri.Query, "Should be no query query string for an empty search");
            CollectionAssert.AreEqual((List<string>) nextResponse, attributes);
        }

        [Test]
        public async Task TestEmptyGetAttributes()
        {
            nextResponse = new List<string>();
            var attributes = await fileShareApiClient.GetUserAttributesAsync();
            Assert.AreEqual("/basePath/attributes", lastRequestUri.AbsolutePath);
            Assert.AreEqual("", lastRequestUri.Query, "Should be no query query string for an empty search");
            CollectionAssert.AreEqual((List<string>) nextResponse, attributes);
        }

        [Test]
        public void TestGetAttributesWhenServerReturnsError()
        {
            nextResponseStatusCode = HttpStatusCode.ServiceUnavailable;
            var exception =
                Assert.ThrowsAsync<HttpRequestException>(async () => await fileShareApiClient.GetUserAttributesAsync());
            Assert.AreEqual("/basePath/attributes", lastRequestUri.AbsolutePath);
            Assert.AreEqual("", lastRequestUri.Query, "Should be no query query string for an empty search");
            Assert.AreEqual("Response status code does not indicate success: 503 (Service Unavailable).",
                exception.Message);
        }
    }
}