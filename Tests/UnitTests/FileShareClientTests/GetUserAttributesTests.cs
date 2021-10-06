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
        private FakeFssHttpClientFactory fakeHttpClientFactory;
        private const string DUMMY_ACCESS_TOKEN = "ACarefullyEncodedSecretAccessToken";

        [SetUp]
        public void Setup()
        {
            fakeHttpClientFactory = new FakeFssHttpClientFactory(request =>
            {
                lastRequestUri = request.RequestUri;
                return (nextResponseStatusCode, nextResponse);
            });
            nextResponse = null;
            nextResponseStatusCode = HttpStatusCode.OK;

            var config = new
            {
                BaseAddress = @"https://fss-tests.net/basePath/",
                AccessToken = DUMMY_ACCESS_TOKEN
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

        [Test]
        public async Task TestGetAttributesSetsAuthorizationHeader()
        {
            nextResponse = new List<string> { "One", "Two" };
            await fileShareApiClient.GetUserAttributesAsync();

            Assert.NotNull(fakeHttpClientFactory.HttpClient.DefaultRequestHeaders.Authorization);
            Assert.AreEqual("bearer", fakeHttpClientFactory.HttpClient.DefaultRequestHeaders.Authorization.Scheme);
            Assert.AreEqual(DUMMY_ACCESS_TOKEN, fakeHttpClientFactory.HttpClient.DefaultRequestHeaders.Authorization.Parameter);
        }
    }
}