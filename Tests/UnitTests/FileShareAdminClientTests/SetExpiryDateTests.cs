using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FileShareClientTestsCommon.Helpers;
using NUnit.Framework;
using UKHO.FileShareAdminClient;
using UKHO.FileShareAdminClient.Models;

namespace FileShareAdminClientTests
{
    internal class SetExpiryDateTests
    {
        private FileShareApiAdminClient _fileShareApiAdminClient;
        private HttpStatusCode _nextResponseStatusCode;
        private List<(HttpMethod HttpMethod, Uri Uri)> _lastRequestUris;
        private List<string> _lastRequestBodies;
        private const int MaxBlockSize = 32;
        private FakeFssHttpClientFactory _fakeFssHttpClientFactory;
        private const string DUMMY_ACCESS_TOKEN = "ACarefullyEncodedSecretAccessToken";

        [SetUp]
        public void Setup()
        {
            _fakeFssHttpClientFactory = new FakeFssHttpClientFactory(request =>
            {
                _lastRequestUris.Add((request.Method, request.RequestUri));

                if (request.Content is StringContent content && request.Content.Headers.ContentLength.HasValue)
                {
                    _lastRequestBodies.Add(content.ReadAsStringAsync().Result);
                }
                else
                {
                    _lastRequestBodies.Add(null);
                }

                return (_nextResponseStatusCode, new object());
            });

            _nextResponseStatusCode = HttpStatusCode.NoContent;
            _lastRequestUris = new List<(HttpMethod HttpMethod, Uri Uri)>();
            _lastRequestBodies = new List<string>();
            _fileShareApiAdminClient = new FileShareApiAdminClient(_fakeFssHttpClientFactory, @"https://fss-tests.net", DUMMY_ACCESS_TOKEN, MaxBlockSize);
        }

        [TearDown]
        public void TearDown()
        {
            _fakeFssHttpClientFactory.Dispose();
        }

        [Test]
        public async Task TestSetExpiryDate()
        {
            var dateTime = DateTime.UtcNow.AddDays(15);
            dateTime = Convert.ToDateTime(dateTime.ToString("yyyy-MM-ddTHH:mm:ss.fffK", CultureInfo.InvariantCulture));
            var batchId = Guid.NewGuid().ToString();

            await _fileShareApiAdminClient.SetExpiryDateAsync(batchId, new BatchExpiryModel { ExpiryDate = dateTime }, CancellationToken.None);

            var expectedRequests = new[]
            {
                $"PUT:/batch/{batchId}/expiry"
            };
            var actualRequests = _lastRequestUris.Select(x => $"{x.HttpMethod}:{x.Uri?.AbsolutePath}");
            var expiryDate = _lastRequestBodies.First()?.DeserialiseJson<BatchExpiryModel>();
            Assert.Multiple(() =>
            {
                Assert.That(actualRequests, Is.EqualTo(expectedRequests));
                Assert.That(expiryDate?.ExpiryDate, Is.EqualTo(dateTime));
            });
        }
    }
}
