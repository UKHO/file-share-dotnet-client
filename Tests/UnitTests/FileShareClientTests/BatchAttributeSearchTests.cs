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
    public class BatchAttributeSearchTests
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
            var firstAttributesList = new List<string> { "string1", "string2" };
            var secondAttributesList = new List<string> { "string3", "string4" };
            var expectedResponse = new BatchAttributesSearchResponse
            {
                SearchBatchCount = 2,
                BatchAttributes = new List<BatchAttributesSearchAttribute>
                    {
                        new BatchAttributesSearchAttribute("Attribute1",firstAttributesList), new BatchAttributesSearchAttribute("Attribute2",secondAttributesList)
                    }

            };
            nextResponse = expectedResponse;
            var response = await fileShareApiClient.BatchAttributeSearch("", cancellationToken: CancellationToken.None);
            Assert.AreEqual("/basePath/attributes/search", lastRequestUri.AbsolutePath);
            Assert.AreEqual("", lastRequestUri.Query, "Should be no query string for an empty search");

            CheckResponseMatchesExpectedResponse(expectedResponse, response.Data);
        }

        [Test]
        public async Task TestSimpleSearchString()
        {
            var firstAttributesList = new List<string> { "string1", "string2" };
            var secondAttributesList = new List<string> { "string3", "string4" };

            var expectedResponse = new BatchAttributesSearchResponse
            {
                SearchBatchCount = 2,
                BatchAttributes = new List<BatchAttributesSearchAttribute>
                    {
                        new BatchAttributesSearchAttribute("Attribute1",firstAttributesList), new BatchAttributesSearchAttribute("Attribute2",secondAttributesList)
                    },

            };
            nextResponse = expectedResponse;

            var response = await fileShareApiClient.BatchAttributeSearch("$batch(key) eq 'value'", cancellationToken: CancellationToken.None);
            Assert.AreEqual("/basePath/attributes/search", lastRequestUri.AbsolutePath);
            Assert.AreEqual("?$filter=$batch(key)%20eq%20%27value%27", lastRequestUri.Query);

            CheckResponseMatchesExpectedResponse(expectedResponse, response.Data);
        }

        [Test]
        public async Task TestSimpleSearchWithNoResults()
        {
            var expectedResponse = new BatchAttributesSearchResponse
            {
                BatchAttributes = new List<BatchAttributesSearchAttribute>(),
                SearchBatchCount = 0
            };
            nextResponse = expectedResponse;

            var response = await fileShareApiClient.BatchAttributeSearch("$batch(key) eq 'value'", cancellationToken: CancellationToken.None);
            Assert.AreEqual("/basePath/attributes/search", lastRequestUri.AbsolutePath);
            Assert.AreEqual("?$filter=$batch(key)%20eq%20%27value%27", lastRequestUri.Query);

            CheckResponseMatchesExpectedResponse(expectedResponse, response.Data);
        }

        [Test]
        public async Task SearchQuerySetsAuthorizationHeader()
        {
            var expectedResponse = new BatchAttributesSearchResponse
            {
                SearchBatchCount = 0,
                BatchAttributes = new List<BatchAttributesSearchAttribute>()
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

        [Test]
        public void TestNameOfAndAttributeValuesinToStringMethod()
        {
            var colourList = new List<string> { "red", "blue" };

            var searchBatchAttributes = new BatchAttributesSearchAttribute
            {
                Key = "Colour",
                Values = colourList
            };
            var attributeValues = searchBatchAttributes.ToString();
            Assert.AreEqual("class BatchAttributesSearchAttribute {\n Key: Colour\n Values: red, blue\n}\n", attributeValues);
        }

        #region BatchSearch with MaxAttributeValueCount

        [TestCase(-1)]
        [TestCase(0)]
        [TestCase(2)]
        [TestCase(10)]
        [TestCase(1000)]
        public async Task DoesBatchAttributeSearchReturnsSucessWithMaxAttributeValueCountandFilter(int maxAttributeValueCount)
        {
            var response = await fileShareApiClient.BatchAttributeSearch("$batch(key) eq 'value'", maxAttributeValueCount, cancellationToken: CancellationToken.None);
            Assert.AreEqual("/basePath/attributes/search", lastRequestUri.AbsolutePath);
            Assert.AreEqual("?$filter=$batch(key)%20eq%20%27value%27&maxAttributeValueCount="+maxAttributeValueCount, lastRequestUri.Query);
            Assert.IsTrue(response.IsSuccess);

        }

        [Test]
        public async Task DoesBatchAttributeSearchReturnsBadRequestWithMaxAttributeValueCountZeroandFilter()
        {
            int MaxAttributeValueCount = 0;
            nextResponseStatusCode = HttpStatusCode.BadRequest;

            var response = await fileShareApiClient.BatchAttributeSearch("$batch(key) eq 'value'", MaxAttributeValueCount, cancellationToken: CancellationToken.None);
            Assert.AreEqual("/basePath/attributes/search", lastRequestUri.AbsolutePath);
            Assert.AreEqual("?$filter=$batch(key)%20eq%20%27value%27&maxAttributeValueCount=0", lastRequestUri.Query);
            Assert.AreEqual((int)nextResponseStatusCode, response.StatusCode);
            Assert.IsFalse(response.IsSuccess);
        }

        #endregion

        #region Private method
        private void CheckResponseMatchesExpectedResponse(BatchAttributesSearchResponse expectedResponse,
            BatchAttributesSearchResponse response)
        {
            Assert.AreEqual(expectedResponse.SearchBatchCount, response.SearchBatchCount);
            for (int i = 0; i < expectedResponse.BatchAttributes.Count; i++)
            {
                var expectedBatchAttribute = expectedResponse.BatchAttributes[i];
                var actualBatchAttribute = response.BatchAttributes[i];
                Assert.AreEqual(expectedBatchAttribute.Key, actualBatchAttribute.Key);
                for (int j = 0; j < expectedBatchAttribute.Values.Count; j++)
                {
                    Assert.AreEqual(expectedBatchAttribute.Values[j], actualBatchAttribute.Values[j]);
                }
            }
        }
        #endregion

    }
}
