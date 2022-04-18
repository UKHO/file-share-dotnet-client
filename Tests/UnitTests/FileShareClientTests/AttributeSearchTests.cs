using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UKHO.FileShareClient;
using UKHO.FileShareClient.Models;
using UKHO.FileShareClientTests.Helpers;

namespace UKHO.FileShareClientTests
{
    public class AttributeSearchTests
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
        public async Task TestEmptySearchQuery()
        {
            var expectedResponse = new BatchAttributesSearchResponse
            {
                SearchBatchCount = 2,
                BatchAttributes = new List<Attributes>
                    {
                        new Attributes("Attribute1"), new Attributes("Attribute2")
                    }

            };
            nextResponse = expectedResponse;
            var response = await fileShareApiClient.BatchAttributeSearch("", cancellationToken: CancellationToken.None);
            Assert.AreEqual("/basePath/attributes/search", lastRequestUri.AbsolutePath);
            Assert.AreEqual("", lastRequestUri.Query, "Should be no query string for an empty search");
            Assert.AreEqual((int)nextResponseStatusCode, response.StatusCode);
            Assert.IsTrue(response.IsSuccess);

            CheckResponseMatchesExpectedResponse(expectedResponse, response.Data);
        }

        [Test]
        public async Task TestSimpleSearchString()
        {
            string[] firstAttributesList = new string[] { "string1", "string2" };
            string[] secondAttributesList = new string[] { "string3", "string4" };

            var expectedResponse = new BatchAttributesSearchResponse
            {
                SearchBatchCount = 2,
                BatchAttributes = new List<Attributes>
                    {
                        new Attributes("Attribute1",firstAttributesList), new Attributes("Attribute2",secondAttributesList)
                    },

            };
            nextResponse = expectedResponse;

            var response = await fileShareApiClient.BatchAttributeSearch("$batch(key) eq 'value'", cancellationToken: CancellationToken.None);
            Assert.AreEqual("/basePath/attributes/search", lastRequestUri.AbsolutePath);
            Assert.AreEqual("?$filter=$batch(key)%20eq%20%27value%27", lastRequestUri.Query);
            Assert.AreEqual((int)nextResponseStatusCode, response.StatusCode);
            Assert.IsTrue(response.IsSuccess);

            CheckResponseMatchesExpectedResponse(expectedResponse, response.Data);
        }

        [Test]
        public async Task TestSimpleSearchWithNoResults()
        {
            var expectedResponse = new BatchAttributesSearchResponse
            {
                BatchAttributes = new List<Attributes>(),
                SearchBatchCount = 0
            };
            nextResponse = expectedResponse;

            var response = await fileShareApiClient.BatchAttributeSearch("$batch(key) eq 'value'", cancellationToken: CancellationToken.None);
            Assert.AreEqual("/basePath/attributes/search", lastRequestUri.AbsolutePath);
            Assert.AreEqual("?$filter=$batch(key)%20eq%20%27value%27", lastRequestUri.Query);
            Assert.AreEqual((int)nextResponseStatusCode, response.StatusCode);
            Assert.IsTrue(response.IsSuccess);

            CheckResponseMatchesExpectedResponse(expectedResponse, response.Data);
        }

        [Test]
        public async Task SearchQuerySetsAuthorizationHeader()
        {
            var expectedResponse = new BatchAttributesSearchResponse
            {
                SearchBatchCount = 0,
                BatchAttributes = new List<Attributes>()
            };

            await fileShareApiClient.BatchAttributeSearch("", cancellationToken: CancellationToken.None);

            Assert.NotNull(fakeHttpClientFactory.HttpClient.DefaultRequestHeaders.Authorization);
            Assert.AreEqual("bearer", fakeHttpClientFactory.HttpClient.DefaultRequestHeaders.Authorization.Scheme);
            Assert.AreEqual(DUMMY_ACCESS_TOKEN, fakeHttpClientFactory.HttpClient.DefaultRequestHeaders.Authorization.Parameter);
        }

        [Test]
        public async Task TestSimpleSearchQueryForBadRequest()
        {
            nextResponseStatusCode = HttpStatusCode.BadRequest;

            var response = await fileShareApiClient.BatchAttributeSearch("$batch(key) eq 'value'", cancellationToken: CancellationToken.None);
            Assert.AreEqual("/basePath/attributes/search", lastRequestUri.AbsolutePath);
            Assert.AreEqual("?$filter=$batch(key)%20eq%20%27value%27", lastRequestUri.Query);
            Assert.AreEqual((int)nextResponseStatusCode, response.StatusCode);
            Assert.IsFalse(response.IsSuccess);
        }

        [Test]
        public async Task TestSimpleSearchQueryForInternalServerError()
        {
            nextResponseStatusCode = HttpStatusCode.InternalServerError;

            var response = await fileShareApiClient.BatchAttributeSearch("$batch(key) eq 'value'", cancellationToken: CancellationToken.None);
            Assert.AreEqual("/basePath/attributes/search", lastRequestUri.AbsolutePath);
            Assert.AreEqual("?$filter=$batch(key)%20eq%20%27value%27", lastRequestUri.Query);
            Assert.AreEqual((int)nextResponseStatusCode, response.StatusCode);
            Assert.IsFalse(response.IsSuccess);
        }

        private void CheckResponseMatchesExpectedResponse(BatchAttributesSearchResponse expectedResponse,
            BatchAttributesSearchResponse response)
        {
            Assert.AreEqual(expectedResponse.SearchBatchCount, response.SearchBatchCount);
            CollectionAssert.AreEqual(expectedResponse.BatchAttributes, response.BatchAttributes);
        }
    }
}
