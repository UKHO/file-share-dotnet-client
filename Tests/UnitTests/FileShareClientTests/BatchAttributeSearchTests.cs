using System.Net;
using FileShareClientTestsCommon.Helpers;
using UKHO.FileShareClient;
using UKHO.FileShareClient.Models;

namespace FileShareClientTests
{
    public class BatchAttributeSearchTests
    {
        private object _nextResponse;
        private FileShareApiClient _fileShareApiClient;
        private HttpStatusCode _nextResponseStatusCode;
        private Uri? _lastRequestUri;
        private FakeFssHttpClientFactory _fakeFssHttpClientFactory;
        private const string DUMMY_ACCESS_TOKEN = "ACarefullyEncodedSecretAccessToken";

        [SetUp]
        public void Setup()
        {
            _fakeFssHttpClientFactory = new FakeFssHttpClientFactory(request =>
            {
                _lastRequestUri = request.RequestUri;
                return (_nextResponseStatusCode, _nextResponse);
            });

            _nextResponse = new object();
            _nextResponseStatusCode = HttpStatusCode.OK;
            _fileShareApiClient = new FileShareApiClient(_fakeFssHttpClientFactory, @"https://fss-tests.net/basePath/", DUMMY_ACCESS_TOKEN);
        }

        [TearDown]
        public void TearDown()
        {
            _fakeFssHttpClientFactory.Dispose();
        }

        [Test]
        public async Task TestEmptySearchQuery()
        {
            var firstAttributesList = new List<string> { "string1", "string2" };
            var secondAttributesList = new List<string> { "string3", "string4" };
            var expectedResponse = new BatchAttributesSearchResponse
            {
                SearchBatchCount = 2,
                BatchAttributes = [new BatchAttributesSearchAttribute("Attribute1", firstAttributesList), new BatchAttributesSearchAttribute("Attribute2", secondAttributesList)]
            };
            _nextResponse = expectedResponse;

            var response = await _fileShareApiClient.BatchAttributeSearchAsync("", cancellationToken: CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(_lastRequestUri?.AbsolutePath, Is.EqualTo("/basePath/attributes/search"));
                Assert.That(_lastRequestUri?.Query, Is.EqualTo(""), "Should be no query string for an empty search");
            });

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
                BatchAttributes = [new BatchAttributesSearchAttribute("Attribute1", firstAttributesList), new BatchAttributesSearchAttribute("Attribute2", secondAttributesList)]
            };
            _nextResponse = expectedResponse;

            var response = await _fileShareApiClient.BatchAttributeSearchAsync("$batch(key) eq 'value'", cancellationToken: CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(_lastRequestUri?.AbsolutePath, Is.EqualTo("/basePath/attributes/search"));
                Assert.That(_lastRequestUri?.Query, Is.EqualTo("?$filter=$batch(key)%20eq%20%27value%27"));
            });

            CheckResponseMatchesExpectedResponse(expectedResponse, response.Data);
        }

        [Test]
        public async Task TestSimpleSearchWithNoResults()
        {
            var expectedResponse = new BatchAttributesSearchResponse
            {
                BatchAttributes = [],
                SearchBatchCount = 0
            };
            _nextResponse = expectedResponse;

            var response = await _fileShareApiClient.BatchAttributeSearchAsync("$batch(key) eq 'value'", cancellationToken: CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(_lastRequestUri?.AbsolutePath, Is.EqualTo("/basePath/attributes/search"));
                Assert.That(_lastRequestUri?.Query, Is.EqualTo("?$filter=$batch(key)%20eq%20%27value%27"));
            });

            CheckResponseMatchesExpectedResponse(expectedResponse, response.Data);
        }

        [Test]
        public async Task SearchQuerySetsAuthorizationHeader()
        {
            var expectedResponse = new BatchAttributesSearchResponse
            {
                SearchBatchCount = 0,
                BatchAttributes = []
            };

            await _fileShareApiClient.BatchAttributeSearchAsync("", cancellationToken: CancellationToken.None);

            Assert.That(_fakeFssHttpClientFactory.HttpClient.DefaultRequestHeaders.Authorization, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(_fakeFssHttpClientFactory.HttpClient.DefaultRequestHeaders.Authorization.Scheme, Is.EqualTo("bearer"));
                Assert.That(_fakeFssHttpClientFactory.HttpClient.DefaultRequestHeaders.Authorization.Parameter, Is.EqualTo(DUMMY_ACCESS_TOKEN));
            });
        }

        [Test]
        public async Task TestSimpleSearchQueryForBadRequest()
        {
            _nextResponseStatusCode = HttpStatusCode.BadRequest;

            var response = await _fileShareApiClient.BatchAttributeSearchAsync("$batch(key) eq 'value'", cancellationToken: CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(_lastRequestUri?.AbsolutePath, Is.EqualTo("/basePath/attributes/search"));
                Assert.That(_lastRequestUri?.Query, Is.EqualTo("?$filter=$batch(key)%20eq%20%27value%27"));
                Assert.That(response.StatusCode, Is.EqualTo((int)_nextResponseStatusCode));
                Assert.That(response.IsSuccess, Is.False);
            });
        }

        [Test]
        public async Task TestSimpleSearchQueryForInternalServerError()
        {
            _nextResponseStatusCode = HttpStatusCode.InternalServerError;

            var response = await _fileShareApiClient.BatchAttributeSearchAsync("$batch(key) eq 'value'", cancellationToken: CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(_lastRequestUri?.AbsolutePath, Is.EqualTo("/basePath/attributes/search"));
                Assert.That(_lastRequestUri?.Query, Is.EqualTo("?$filter=$batch(key)%20eq%20%27value%27"));
                Assert.That(response.StatusCode, Is.EqualTo((int)_nextResponseStatusCode));
                Assert.That(response.IsSuccess, Is.False);
            });
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

            Assert.That(attributeValues, Is.EqualTo("class BatchAttributesSearchAttribute {\n Key: Colour\n Values: red, blue\n}\n"));
        }

        #region BatchSearch with MaxAttributeValueCount

        [TestCase(-1)]
        [TestCase(0)]
        [TestCase(2)]
        [TestCase(10)]
        [TestCase(1000)]
        public async Task DoesBatchAttributeSearchReturnsSucessWithMaxAttributeValueCountandFilter(int maxAttributeValueCount)
        {
            var response = await _fileShareApiClient.BatchAttributeSearchAsync("$batch(key) eq 'value'", maxAttributeValueCount, cancellationToken: CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(_lastRequestUri?.AbsolutePath, Is.EqualTo("/basePath/attributes/search"));
                Assert.That(_lastRequestUri?.Query, Is.EqualTo("?$filter=$batch(key)%20eq%20%27value%27&maxAttributeValueCount=" + maxAttributeValueCount));
                Assert.That(response.IsSuccess, Is.True);
            });
        }

        [Test]
        public async Task DoesBatchAttributeSearchReturnsBadRequestWithMaxAttributeValueCountZeroandFilter()
        {
            var MaxAttributeValueCount = 0;
            _nextResponseStatusCode = HttpStatusCode.BadRequest;

            var response = await _fileShareApiClient.BatchAttributeSearchAsync("$batch(key) eq 'value'", MaxAttributeValueCount, cancellationToken: CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(_lastRequestUri?.AbsolutePath, Is.EqualTo("/basePath/attributes/search"));
                Assert.That(_lastRequestUri?.Query, Is.EqualTo("?$filter=$batch(key)%20eq%20%27value%27&maxAttributeValueCount=0"));
                Assert.That(response.StatusCode, Is.EqualTo((int)_nextResponseStatusCode));
                Assert.That(response.IsSuccess, Is.False);
            });
        }

        #endregion

        #region Private method

        private static void CheckResponseMatchesExpectedResponse(BatchAttributesSearchResponse expectedResponse, BatchAttributesSearchResponse response)
        {
            Assert.That(response.SearchBatchCount, Is.EqualTo(expectedResponse.SearchBatchCount));

            for (var i = 0; i < expectedResponse.BatchAttributes.Count; i++)
            {
                var expectedBatchAttribute = expectedResponse.BatchAttributes[i];
                var actualBatchAttribute = response.BatchAttributes[i];
                Assert.That(actualBatchAttribute.Key, Is.EqualTo(expectedBatchAttribute.Key));

                for (var j = 0; j < expectedBatchAttribute.Values.Count; j++)
                {
                    Assert.That(actualBatchAttribute.Values[j], Is.EqualTo(expectedBatchAttribute.Values[j]));
                }
            }
        }

        #endregion
    }
}
