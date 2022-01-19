using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using UKHO.FileShareClient;
using UKHO.FileShareClient.Models;
using UKHO.FileShareClientTests.Helpers;

namespace UKHO.FileShareClientTests
{
    public class SearchTests
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
            var expectedResponse = new BatchSearchResponse
            {
                Count = 2,
                Total = 2,
                Entries = new List<BatchDetails>
                {
                    new BatchDetails("batch1"), new BatchDetails("batch2")
                },
                Links = new Links(new Link("self"))
            };
            nextResponse = expectedResponse;
            var response = await fileShareApiClient.Search("");
            Assert.AreEqual("/basePath/batch", lastRequestUri.AbsolutePath);
            Assert.AreEqual("", lastRequestUri.Query, "Should be no query query string for an empty search");

            CheckResponseMatchesExpectedResponse(expectedResponse, response);
        }


        [Test]
        public async Task TestSimpleSearchString()
        {
            var expectedResponse = new BatchSearchResponse
            {
                Count = 2,
                Total = 2,
                Entries = new List<BatchDetails>
                {
                    new BatchDetails("batch1"), new BatchDetails("batch2")
                },
                Links = new Links(new Link("self"))
            };
            nextResponse = expectedResponse;

            var response = await fileShareApiClient.Search("$batch(key) eq 'value'");
            Assert.AreEqual("/basePath/batch", lastRequestUri.AbsolutePath);
            Assert.AreEqual("?$filter=$batch(key)%20eq%20%27value%27", lastRequestUri.Query);

            CheckResponseMatchesExpectedResponse(expectedResponse, response);
        }

        [Test]
        public async Task TestSimpleSearchWithDifferentPageSize()
        {
            var expectedResponse = new BatchSearchResponse
            {
                Count = 2,
                Total = 2,
                Entries = new List<BatchDetails>
                {
                    new BatchDetails("batch1"), new BatchDetails("batch2")
                },
                Links = new Links(new Link("self"))
            };
            nextResponse = expectedResponse;

            var response = await fileShareApiClient.Search("$batch(key) eq 'value'", 50);
            Assert.AreEqual("/basePath/batch", lastRequestUri.AbsolutePath);
            Assert.AreEqual("?$filter=$batch(key)%20eq%20%27value%27&limit=50", lastRequestUri.Query);

            CheckResponseMatchesExpectedResponse(expectedResponse, response);
        }

        [Test]
        public async Task TestSimpleSearchStartingOnNextPage()
        {
            var expectedResponse = new BatchSearchResponse
            {
                Count = 2,
                Total = 2,
                Entries = new List<BatchDetails>
                {
                    new BatchDetails("batch1"), new BatchDetails("batch2")
                },
                Links = new Links(new Link("self"))
            };
            nextResponse = expectedResponse;

            var response = await fileShareApiClient.Search("$batch(key) eq 'value'", null, 20);
            Assert.AreEqual("/basePath/batch", lastRequestUri.AbsolutePath);
            Assert.AreEqual("?$filter=$batch(key)%20eq%20%27value%27&start=20", lastRequestUri.Query);

            CheckResponseMatchesExpectedResponse(expectedResponse, response);
        }

        [Test]
        public async Task TestSimpleSearchWithPageSizeAndStartingOnNextPage()
        {
            var expectedResponse = new BatchSearchResponse
            {
                Count = 2,
                Total = 2,
                Entries = new List<BatchDetails>
                {
                    new BatchDetails("batch1"), new BatchDetails("batch2")
                },
                Links = new Links(new Link("self"))
            };
            nextResponse = expectedResponse;

            var response = await fileShareApiClient.Search("$batch(key) eq 'value'", 10, 20);
            Assert.AreEqual("/basePath/batch", lastRequestUri.AbsolutePath);
            Assert.AreEqual("?$filter=$batch(key)%20eq%20%27value%27&limit=10&start=20", lastRequestUri.Query);

            CheckResponseMatchesExpectedResponse(expectedResponse, response);
        }

        [TestCase(-10)]
        [TestCase(0)]
        public void TestSearchWithInvalidPageSizeThrowsArgumentException(int pageSize)
        {
            var exception = Assert.ThrowsAsync<ArgumentException>(async () =>
                await fileShareApiClient.Search("$batch(key) eq 'value'", pageSize, 20));

            Assert.AreEqual("pageSize", exception!.ParamName);
        }

        [Test]
        public void TestSearchWithInvalidPageStartThrowsArgumentException()
        {
            var exception = Assert.ThrowsAsync<ArgumentException>(async () =>
                await fileShareApiClient.Search("$batch(key) eq 'value'", -10, 20));

            Assert.AreEqual("pageSize", exception!.ParamName);
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
            nextResponse = expectedResponse;

            var response = await fileShareApiClient.Search("$batch(key) eq 'value'");
            Assert.AreEqual("/basePath/batch", lastRequestUri.AbsolutePath);
            Assert.AreEqual("?$filter=$batch(key)%20eq%20%27value%27", lastRequestUri.Query);

            CheckResponseMatchesExpectedResponse(expectedResponse, response);
        }

        [Test]
        public async Task SearchQuerySetsAuthorizationHeader()
        {
            nextResponse = new BatchSearchResponse
            {
                Count = 0,
                Total = 0,
                Entries = new List<BatchDetails>(),
                Links = new Links(new Link("self"))
            };
            
            await fileShareApiClient.Search("");

            Assert.NotNull(fakeHttpClientFactory.HttpClient.DefaultRequestHeaders.Authorization);
            Assert.AreEqual("bearer", fakeHttpClientFactory.HttpClient.DefaultRequestHeaders.Authorization.Scheme);
            Assert.AreEqual(DUMMY_ACCESS_TOKEN, fakeHttpClientFactory.HttpClient.DefaultRequestHeaders.Authorization.Parameter);
        }

        private void CheckResponseMatchesExpectedResponse(BatchSearchResponse expectedResponse,
            BatchSearchResponse response)
        {
            Assert.AreEqual(expectedResponse.Count, response.Count);
            Assert.AreEqual(expectedResponse.Total, response.Total);
            Assert.AreEqual(expectedResponse.Links, response.Links);
            CollectionAssert.AreEqual(expectedResponse.Entries, response.Entries);
        }


        [Test]
        public async Task TestEmptySearchQueryWithCancellation()
        {
            var expectedResponse = new BatchSearchResponse
            {                              
                    Count = 2,
                    Total = 2,
                    Entries = new List<BatchDetails>
                    {
                        new BatchDetails("batch1"), new BatchDetails("batch2")
                    },
                    Links = new Links(new Link("self"))
               
            };
            nextResponse = expectedResponse;
            var response = await fileShareApiClient.Search("" ,cancellationToken: CancellationToken.None,null,null);
            Assert.AreEqual("/basePath/batch", lastRequestUri.AbsolutePath);
            Assert.AreEqual("", lastRequestUri.Query, "Should be no query query string for an empty search");
            Assert.AreEqual((int)nextResponseStatusCode, response.StatusCode);
            Assert.IsTrue(response.IsSuccess);
     
            CheckResponseMatchesExpectedResponse(expectedResponse, response.Data);
        }

        [Test]
        public async Task TestSimpleSearchStringWithCancellation()
        {
            var expectedResponse = new BatchSearchResponse
            {
                Count = 2,
                Total = 2,
                Entries = new List<BatchDetails>
                {
                    new BatchDetails("batch1"), new BatchDetails("batch2")
                },
                Links = new Links(new Link("self"))
            };
            nextResponse = expectedResponse;

            var response = await fileShareApiClient.Search("$batch(key) eq 'value'",cancellationToken:CancellationToken.None,null,null);
            Assert.AreEqual("/basePath/batch", lastRequestUri.AbsolutePath);
            Assert.AreEqual("?$filter=$batch(key)%20eq%20%27value%27", lastRequestUri.Query);
            Assert.AreEqual((int)nextResponseStatusCode, response.StatusCode);
            Assert.IsTrue(response.IsSuccess);
           
            CheckResponseMatchesExpectedResponse(expectedResponse, response.Data);
        }

        [Test]
        public async Task TestSimpleSearchWithDifferentPageSizeAndCancellation()
        {
            var expectedResponse = new BatchSearchResponse
            {
                Count = 2,
                Total = 2,
                Entries = new List<BatchDetails>
                {
                    new BatchDetails("batch1"), new BatchDetails("batch2")
                },
                Links = new Links(new Link("self"))
            };
            nextResponse = expectedResponse;

            var response = await fileShareApiClient.Search("$batch(key) eq 'value'", cancellationToken: CancellationToken.None, 50, null);
            Assert.AreEqual("/basePath/batch", lastRequestUri.AbsolutePath);
            Assert.AreEqual("?$filter=$batch(key)%20eq%20%27value%27&limit=50", lastRequestUri.Query);
            Assert.AreEqual((int)nextResponseStatusCode, response.StatusCode);
            Assert.IsTrue(response.IsSuccess);

            CheckResponseMatchesExpectedResponse(expectedResponse, response.Data);
        }

        [Test]
        public async Task TestSimpleSearchStartingOnNextPageWithCancellation()
        {
            var expectedResponse = new BatchSearchResponse
            {
                Count = 2,
                Total = 2,
                Entries = new List<BatchDetails>
                {
                    new BatchDetails("batch1"), new BatchDetails("batch2")
                },
                Links = new Links(new Link("self"))
            };
            nextResponse = expectedResponse;

            var response = await fileShareApiClient.Search("$batch(key) eq 'value'", cancellationToken: CancellationToken.None, null, 20);
            Assert.AreEqual("/basePath/batch", lastRequestUri.AbsolutePath);
            Assert.AreEqual("?$filter=$batch(key)%20eq%20%27value%27&start=20", lastRequestUri.Query);
            Assert.AreEqual((int)nextResponseStatusCode, response.StatusCode);
            Assert.IsTrue(response.IsSuccess);

            CheckResponseMatchesExpectedResponse(expectedResponse, response.Data);
        }

        [TestCase(-10)]
        [TestCase(0)]
        public void TestSearchWithInvalidPageSizeThrowsArgumentExceptionAndCancellationn(int pageSize)
        {
            var exception = Assert.ThrowsAsync<ArgumentException>(async () =>
                await fileShareApiClient.Search("$batch(key) eq 'value'", cancellationToken: CancellationToken.None, pageSize, 20));

            Assert.AreEqual("pageSize", exception!.ParamName);
        }

        [Test]
        public void TestSearchWithInvalidPageStartThrowsArgumentExceptionAndCancellation()
        {
            var exception = Assert.ThrowsAsync<ArgumentException>(async () =>
                await fileShareApiClient.Search("$batch(key) eq 'value'",cancellationToken:CancellationToken.None, -10, 20));

            Assert.AreEqual("pageSize", exception!.ParamName);
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
            nextResponse = expectedResponse;

            var response = await fileShareApiClient.Search("$batch(key) eq 'value'",cancellationToken: CancellationToken.None,null,null);
            Assert.AreEqual("/basePath/batch", lastRequestUri.AbsolutePath);
            Assert.AreEqual("?$filter=$batch(key)%20eq%20%27value%27", lastRequestUri.Query);
            Assert.AreEqual((int)nextResponseStatusCode, response.StatusCode);
            Assert.IsTrue(response.IsSuccess);

            CheckResponseMatchesExpectedResponse(expectedResponse, response.Data);
        }

        [Test]
        public async Task SearchQuerySetsAuthorizationHeaderWithCancellation()
        {
            nextResponse = new BatchSearchResponse
            {
                Count = 0,
                Total = 0,
                Entries = new List<BatchDetails>(),
                Links = new Links(new Link("self"))
            };

            await fileShareApiClient.Search("",cancellationToken:CancellationToken.None,null,null);

            Assert.NotNull(fakeHttpClientFactory.HttpClient.DefaultRequestHeaders.Authorization);
            Assert.AreEqual("bearer", fakeHttpClientFactory.HttpClient.DefaultRequestHeaders.Authorization.Scheme);
            Assert.AreEqual(DUMMY_ACCESS_TOKEN, fakeHttpClientFactory.HttpClient.DefaultRequestHeaders.Authorization.Parameter);
        }

        [Test]
        public async Task TestSimpleSearchQueryForBadRequestWithCancellation()
        {
            nextResponseStatusCode = HttpStatusCode.BadRequest;
         
            var response = await fileShareApiClient.Search("$batch(key) eq 'value'", cancellationToken: CancellationToken.None,null,null);
            Assert.AreEqual("/basePath/batch", lastRequestUri.AbsolutePath);
            Assert.AreEqual("?$filter=$batch(key)%20eq%20%27value%27", lastRequestUri.Query);
            Assert.AreEqual((int)nextResponseStatusCode, response.StatusCode);
            Assert.IsFalse(response.IsSuccess);
        }

        [Test]
        public async Task TestSimpleSearchQueryForInternalServerErrorWithCancellation()
        {
            nextResponseStatusCode = HttpStatusCode.InternalServerError;

            var response = await fileShareApiClient.Search("$batch(key) eq 'value'", cancellationToken: CancellationToken.None,null,null);
            Assert.AreEqual("/basePath/batch", lastRequestUri.AbsolutePath);
            Assert.AreEqual("?$filter=$batch(key)%20eq%20%27value%27", lastRequestUri.Query);
            Assert.AreEqual((int)nextResponseStatusCode, response.StatusCode);
            Assert.IsFalse(response.IsSuccess);
        }
    }
}