using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using FileShareClientTestsCommon.Helpers;
using NUnit.Framework;
using UKHO.FileShareClient;
using UKHO.FileShareClient.Models;

namespace FileShareClientTests
{
    public class SearchTests
    {
        private object _nextResponse;
        private FileShareApiClient _fileShareApiClient;
        private HttpStatusCode _nextResponseStatusCode;
        private Uri _lastRequestUri;
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

        private static void CheckResponseMatchesExpectedResponse(BatchSearchResponse expectedResponse, BatchSearchResponse response)
        {
            Assert.That(response.Count, Is.EqualTo(expectedResponse.Count));
            Assert.Multiple(() =>
            {
                Assert.That(response.Total, Is.EqualTo(expectedResponse.Total));
                Assert.That(response.Links, Is.EqualTo(expectedResponse.Links));
                Assert.That(response.Entries, Is.EqualTo(expectedResponse.Entries));
            });
        }

        [Test]
        public async Task TestEmptySearchQuery()
        {
            var expectedResponse = new BatchSearchResponse
            {
                Count = 2,
                Total = 2,
                Entries = new List<BatchDetails> { new BatchDetails("batch1"), new BatchDetails("batch2") },
                Links = new Links(new Link("self"))
            };
            _nextResponse = expectedResponse;

            var response = await _fileShareApiClient.SearchAsync("");

            Assert.Multiple(() =>
            {
                Assert.That(_lastRequestUri?.AbsolutePath, Is.EqualTo("/basePath/batch"));
                Assert.That(_lastRequestUri?.Query, Is.EqualTo(""), "Should be no query query string for an empty search");
            });

            CheckResponseMatchesExpectedResponse(expectedResponse, response);
        }

        [Test]
        public async Task TestSimpleSearchString()
        {
            var expectedResponse = new BatchSearchResponse
            {
                Count = 2,
                Total = 2,
                Entries = new List<BatchDetails> { new BatchDetails("batch1"), new BatchDetails("batch2") },
                Links = new Links(new Link("self"))
            };
            _nextResponse = expectedResponse;

            var response = await _fileShareApiClient.SearchAsync("$batch(key) eq 'value'");

            Assert.Multiple(() =>
            {
                Assert.That(_lastRequestUri?.AbsolutePath, Is.EqualTo("/basePath/batch"));
                Assert.That(_lastRequestUri?.Query, Is.EqualTo("?$filter=$batch(key)%20eq%20%27value%27"));
            });

            CheckResponseMatchesExpectedResponse(expectedResponse, response);
        }

        [Test]
        public async Task TestSimpleSearchWithDifferentPageSize()
        {
            var expectedResponse = new BatchSearchResponse
            {
                Count = 2,
                Total = 2,
                Entries = new List<BatchDetails> { new BatchDetails("batch1"), new BatchDetails("batch2") },
                Links = new Links(new Link("self"))
            };
            _nextResponse = expectedResponse;

            var response = await _fileShareApiClient.SearchAsync("$batch(key) eq 'value'", 50);

            Assert.Multiple(() =>
            {
                Assert.That(_lastRequestUri?.AbsolutePath, Is.EqualTo("/basePath/batch"));
                Assert.That(_lastRequestUri?.Query, Is.EqualTo("?$filter=$batch(key)%20eq%20%27value%27&limit=50"));
            });

            CheckResponseMatchesExpectedResponse(expectedResponse, response);
        }

        [Test]
        public async Task TestSimpleSearchStartingOnNextPage()
        {
            var expectedResponse = new BatchSearchResponse
            {
                Count = 2,
                Total = 2,
                Entries = new List<BatchDetails> { new BatchDetails("batch1"), new BatchDetails("batch2") },
                Links = new Links(new Link("self"))
            };
            _nextResponse = expectedResponse;

            var response = await _fileShareApiClient.SearchAsync("$batch(key) eq 'value'", null, 20);

            Assert.Multiple(() =>
            {
                Assert.That(_lastRequestUri?.AbsolutePath, Is.EqualTo("/basePath/batch"));
                Assert.That(_lastRequestUri?.Query, Is.EqualTo("?$filter=$batch(key)%20eq%20%27value%27&start=20"));
            });

            CheckResponseMatchesExpectedResponse(expectedResponse, response);
        }

        [Test]
        public async Task TestSimpleSearchWithPageSizeAndStartingOnNextPage()
        {
            var expectedResponse = new BatchSearchResponse
            {
                Count = 2,
                Total = 2,
                Entries = new List<BatchDetails> { new BatchDetails("batch1"), new BatchDetails("batch2") },
                Links = new Links(new Link("self"))
            };
            _nextResponse = expectedResponse;

            var response = await _fileShareApiClient.SearchAsync("$batch(key) eq 'value'", 10, 20);

            Assert.Multiple(() =>
            {
                Assert.That(_lastRequestUri?.AbsolutePath, Is.EqualTo("/basePath/batch"));
                Assert.That(_lastRequestUri?.Query, Is.EqualTo("?$filter=$batch(key)%20eq%20%27value%27&limit=10&start=20"));
            });

            CheckResponseMatchesExpectedResponse(expectedResponse, response);
        }

        [TestCase(-10)]
        [TestCase(0)]
        public void TestSearchWithInvalidPageSizeThrowsArgumentException(int pageSize)
        {
            var exception = Assert.ThrowsAsync<ArgumentException>(async () => await _fileShareApiClient.SearchAsync("$batch(key) eq 'value'", pageSize, 20));

            Assert.That(exception.ParamName, Is.EqualTo("pageSize"));
        }

        [Test]
        public void TestSearchWithInvalidPageStartThrowsArgumentException()
        {
            var exception = Assert.ThrowsAsync<ArgumentException>(async () => await _fileShareApiClient.SearchAsync("$batch(key) eq 'value'", -10, 20));

            Assert.That(exception.ParamName, Is.EqualTo("pageSize"));
        }

        [Test]
        public async Task TestSimpleSearchWithNoResults()
        {
            var expectedResponse = new BatchSearchResponse
            {
                Count = 0,
                Total = 0,
                Entries = new List<BatchDetails>(),
                Links = new Links(new Link("self"))
            };
            _nextResponse = expectedResponse;

            var response = await _fileShareApiClient.SearchAsync("$batch(key) eq 'value'");

            Assert.Multiple(() =>
            {
                Assert.That(_lastRequestUri?.AbsolutePath, Is.EqualTo("/basePath/batch"));
                Assert.That(_lastRequestUri?.Query, Is.EqualTo("?$filter=$batch(key)%20eq%20%27value%27"));
            });

            CheckResponseMatchesExpectedResponse(expectedResponse, response);
        }

        [Test]
        public async Task SearchQuerySetsAuthorizationHeader()
        {
            _nextResponse = new BatchSearchResponse
            {
                Count = 0,
                Total = 0,
                Entries = new List<BatchDetails>(),
                Links = new Links(new Link("self"))
            };

            await _fileShareApiClient.SearchAsync("");

            Assert.That(_fakeFssHttpClientFactory.HttpClient.DefaultRequestHeaders.Authorization, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(_fakeFssHttpClientFactory.HttpClient.DefaultRequestHeaders.Authorization.Scheme, Is.EqualTo("bearer"));
                Assert.That(_fakeFssHttpClientFactory.HttpClient.DefaultRequestHeaders.Authorization.Parameter, Is.EqualTo(DUMMY_ACCESS_TOKEN));
            });
        }

        [Test]
        public async Task TestEmptySearchQueryWithCancellation()
        {
            var expectedResponse = new BatchSearchResponse
            {
                Count = 2,
                Total = 2,
                Entries = new List<BatchDetails> { new BatchDetails("batch1"), new BatchDetails("batch2") },
                Links = new Links(new Link("self"))

            };
            _nextResponse = expectedResponse;

            var response = await _fileShareApiClient.SearchAsync("", null, null, cancellationToken: CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(_lastRequestUri?.AbsolutePath, Is.EqualTo("/basePath/batch"));
                Assert.That(_lastRequestUri?.Query, Is.EqualTo(""), "Should be no query query string for an empty search");
                Assert.That(response.StatusCode, Is.EqualTo((int)_nextResponseStatusCode));
                Assert.That(response.IsSuccess, Is.True);
            });

            CheckResponseMatchesExpectedResponse(expectedResponse, response.Data);
        }

        [Test]
        public async Task TestSimpleSearchStringWithCancellation()
        {
            var expectedResponse = new BatchSearchResponse
            {
                Count = 2,
                Total = 2,
                Entries = new List<BatchDetails> { new BatchDetails("batch1"), new BatchDetails("batch2") },
                Links = new Links(new Link("self"))
            };
            _nextResponse = expectedResponse;

            var response = await _fileShareApiClient.SearchAsync("$batch(key) eq 'value'", null, null, cancellationToken: CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(_lastRequestUri?.AbsolutePath, Is.EqualTo("/basePath/batch"));
                Assert.That(_lastRequestUri?.Query, Is.EqualTo("?$filter=$batch(key)%20eq%20%27value%27"));
                Assert.That(response.StatusCode, Is.EqualTo((int)_nextResponseStatusCode));
                Assert.That(response.IsSuccess, Is.True);
            });

            CheckResponseMatchesExpectedResponse(expectedResponse, response.Data);
        }

        [Test]
        public async Task TestSimpleSearchWithDifferentPageSizeAndCancellation()
        {
            var expectedResponse = new BatchSearchResponse
            {
                Count = 2,
                Total = 2,
                Entries = new List<BatchDetails> { new BatchDetails("batch1"), new BatchDetails("batch2") },
                Links = new Links(new Link("self"))
            };
            _nextResponse = expectedResponse;

            var response = await _fileShareApiClient.SearchAsync("$batch(key) eq 'value'", 50, null, cancellationToken: CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(_lastRequestUri?.AbsolutePath, Is.EqualTo("/basePath/batch"));
                Assert.That(_lastRequestUri?.Query, Is.EqualTo("?$filter=$batch(key)%20eq%20%27value%27&limit=50"));
                Assert.That(response.StatusCode, Is.EqualTo((int)_nextResponseStatusCode));
                Assert.That(response.IsSuccess, Is.True);
            });

            CheckResponseMatchesExpectedResponse(expectedResponse, response.Data);
        }

        [Test]
        public async Task TestSimpleSearchStartingOnNextPageWithCancellation()
        {
            var expectedResponse = new BatchSearchResponse
            {
                Count = 2,
                Total = 2,
                Entries = new List<BatchDetails> { new BatchDetails("batch1"), new BatchDetails("batch2") },
                Links = new Links(new Link("self"))
            };
            _nextResponse = expectedResponse;

            var response = await _fileShareApiClient.SearchAsync("$batch(key) eq 'value'", null, 20, cancellationToken: CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(_lastRequestUri?.AbsolutePath, Is.EqualTo("/basePath/batch"));
                Assert.That(_lastRequestUri?.Query, Is.EqualTo("?$filter=$batch(key)%20eq%20%27value%27&start=20"));
                Assert.That(response.StatusCode, Is.EqualTo((int)_nextResponseStatusCode));
                Assert.That(response.IsSuccess, Is.True);
            });

            CheckResponseMatchesExpectedResponse(expectedResponse, response.Data);
        }

        [TestCase(-10)]
        [TestCase(0)]
        public void TestSearchWithInvalidPageSizeThrowsArgumentExceptionAndCancellationn(int pageSize)
        {
            var exception = Assert.ThrowsAsync<ArgumentException>(async () => await _fileShareApiClient.SearchAsync("$batch(key) eq 'value'", pageSize, 20, cancellationToken: CancellationToken.None));

            Assert.That(exception.ParamName, Is.EqualTo("pageSize"));
        }

        [Test]
        public void TestSearchWithInvalidPageStartThrowsArgumentExceptionAndCancellation()
        {
            var exception = Assert.ThrowsAsync<ArgumentException>(async () => await _fileShareApiClient.SearchAsync("$batch(key) eq 'value'", -10, 20, cancellationToken: CancellationToken.None));

            Assert.That(exception.ParamName, Is.EqualTo("pageSize"));
        }

        [Test]
        public async Task TestSimpleSearchWithNoResultsAndCancellation()
        {
            var expectedResponse = new BatchSearchResponse
            {
                Count = 0,
                Total = 0,
                Entries = new List<BatchDetails>(),
                Links = new Links(new Link("self"))
            };
            _nextResponse = expectedResponse;

            var response = await _fileShareApiClient.SearchAsync("$batch(key) eq 'value'", null, null, cancellationToken: CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(_lastRequestUri?.AbsolutePath, Is.EqualTo("/basePath/batch"));
                Assert.That(_lastRequestUri?.Query, Is.EqualTo("?$filter=$batch(key)%20eq%20%27value%27"));
                Assert.That(response.StatusCode, Is.EqualTo((int)_nextResponseStatusCode));
                Assert.That(response.IsSuccess, Is.True);
            });

            CheckResponseMatchesExpectedResponse(expectedResponse, response.Data);
        }

        [Test]
        public async Task SearchQuerySetsAuthorizationHeaderWithCancellation()
        {
            _nextResponse = new BatchSearchResponse
            {
                Count = 0,
                Total = 0,
                Entries = new List<BatchDetails>(),
                Links = new Links(new Link("self"))
            };

            await _fileShareApiClient.SearchAsync("", null, null, cancellationToken: CancellationToken.None);

            Assert.That(_fakeFssHttpClientFactory.HttpClient.DefaultRequestHeaders.Authorization, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(_fakeFssHttpClientFactory.HttpClient.DefaultRequestHeaders.Authorization.Scheme, Is.EqualTo("bearer"));
                Assert.That(_fakeFssHttpClientFactory.HttpClient.DefaultRequestHeaders.Authorization.Parameter, Is.EqualTo(DUMMY_ACCESS_TOKEN));
            });
        }

        [Test]
        public async Task TestSimpleSearchQueryForBadRequestWithCancellation()
        {
            _nextResponseStatusCode = HttpStatusCode.BadRequest;

            var response = await _fileShareApiClient.SearchAsync("$batch(key) eq 'value'", null, null, cancellationToken: CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(_lastRequestUri?.AbsolutePath, Is.EqualTo("/basePath/batch"));
                Assert.That(_lastRequestUri?.Query, Is.EqualTo("?$filter=$batch(key)%20eq%20%27value%27"));
                Assert.That(response.StatusCode, Is.EqualTo((int)_nextResponseStatusCode));
                Assert.That(response.IsSuccess, Is.False);
            });
        }

        [Test]
        public async Task TestSimpleSearchQueryForInternalServerErrorWithCancellation()
        {
            _nextResponseStatusCode = HttpStatusCode.InternalServerError;

            var response = await _fileShareApiClient.SearchAsync("$batch(key) eq 'value'", null, null, cancellationToken: CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(_lastRequestUri?.AbsolutePath, Is.EqualTo("/basePath/batch"));
                Assert.That(_lastRequestUri?.Query, Is.EqualTo("?$filter=$batch(key)%20eq%20%27value%27"));
                Assert.That(response.StatusCode, Is.EqualTo((int)_nextResponseStatusCode));
                Assert.That(response.IsSuccess, Is.False);
            });
        }
    }
}
