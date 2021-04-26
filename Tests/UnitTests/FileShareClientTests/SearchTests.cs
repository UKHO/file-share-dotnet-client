using System;
using System.Collections.Generic;
using System.Net;
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
        public async Task TestEmptySearchQuery()
        {
            var expectedResponse = new BatchSearchResponse()
            {
                Count = 2,
                Total = 2,
                Entries = new List<BatchDetails>()
                {
                    new BatchDetails("batch1"), new BatchDetails("batch2")
                },
                Links = new Links(new Link("self"))
            };
            nextResponse = expectedResponse;
            var response = await fileShareApiClient.Search("");
            Assert.AreEqual("/batch", lastRequestUri.AbsolutePath);
            Assert.AreEqual("", lastRequestUri.Query, "Should be no query query string for an empty search");

            CheckResponseMatchesExpectedResponse(expectedResponse, response);
        }


        [Test]
        public async Task TestSimpleSearchString()
        {
            var expectedResponse = new BatchSearchResponse()
            {
                Count = 2,
                Total = 2,
                Entries = new List<BatchDetails>()
                {
                    new BatchDetails("batch1"), new BatchDetails("batch2")
                },
                Links = new Links(new Link("self"))
            };
            nextResponse = expectedResponse;

            var response = await fileShareApiClient.Search("$batch(key) eq 'value'");
            Assert.AreEqual("/batch", lastRequestUri.AbsolutePath);
            Assert.AreEqual("?$filter=$batch(key)%20eq%20'value'", lastRequestUri.Query);

            CheckResponseMatchesExpectedResponse(expectedResponse, response);
        }

        [Test]
        public async Task TestSimpleSearchWithNoResults()
        {
            var expectedResponse = new BatchSearchResponse()
            {
                Count = 0,
                Total = 0,
                Entries = new List<BatchDetails>(),
                Links = new Links(new Link("self"))
            };
            nextResponse = expectedResponse;

            var response = await fileShareApiClient.Search("$batch(key) eq 'value'");
            Assert.AreEqual("/batch", lastRequestUri.AbsolutePath);
            Assert.AreEqual("?$filter=$batch(key)%20eq%20'value'", lastRequestUri.Query);

            CheckResponseMatchesExpectedResponse(expectedResponse, response);
        }

        private void CheckResponseMatchesExpectedResponse(BatchSearchResponse expectedResponse,
            BatchSearchResponse response)
        {
            Assert.AreEqual(expectedResponse.Count, response.Count);
            Assert.AreEqual(expectedResponse.Total, response.Total);
            Assert.AreEqual(expectedResponse.Links, response.Links);
            CollectionAssert.AreEqual(expectedResponse.Entries, response.Entries);
        }
    }
}