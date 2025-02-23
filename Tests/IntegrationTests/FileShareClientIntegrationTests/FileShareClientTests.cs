using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FileShareClientIntegrationTests.Helpers;
using NUnit.Framework;
using UKHO.FileShareClient;

namespace FileShareClientIntegrationTests
{
    public class FileShareClientTests
    {
        private FileShareApiClient _fileShareApiClient;

        [SetUp]
        public void Setup()
        {
            _fileShareApiClient = new FileShareApiClient(Configuration.HttpClientFactory, Configuration.FssUrl, Configuration.AuthTokenProvider);
        }

        [Test]
        public async Task GetBatchStatusAsync()
        {
            var result = await _fileShareApiClient.GetBatchStatusAsync(Configuration.GetBatchStatusAsync.BatchId);

            Assert.That(result, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(result.BatchId, Is.EqualTo(Configuration.GetBatchStatusAsync.BatchId));
                Assert.That(result.Status, Is.Not.Null);
            });
        }

        [Test]
        public async Task GetBatchStatusAsync_InvalidBatchId()
        {
            try
            {
                var result = await _fileShareApiClient.GetBatchStatusAsync(Guid.NewGuid().ToString());

                Assert.Fail("Expected HttpRequestException not thrown");
            }
            catch (HttpRequestException ex)
            {
                Assert.That(ex, Is.Not.Null);
                Assert.That(ex.Message, Is.EqualTo("Response status code does not indicate success: 400 (Bad Request)."));
            }
            catch (Exception)
            {
                throw;
            }
        }

        [Test]
        public async Task SearchAsync()
        {
            var result = await _fileShareApiClient.SearchAsync(Configuration.SearchAsync.SearchQuery);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Total, Is.GreaterThan(Configuration.SearchAsync.PageSize));

            result = await _fileShareApiClient.SearchAsync(Configuration.SearchAsync.SearchQuery, Configuration.SearchAsync.PageSize);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(Configuration.SearchAsync.PageSize));

            result = await _fileShareApiClient.SearchAsync(Configuration.SearchAsync.SearchQuery, Configuration.SearchAsync.PageSize, Configuration.SearchAsync.Start);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.LessThanOrEqualTo(Configuration.SearchAsync.PageSize));
        }

        [Test]
        public async Task DownloadFileAsync()
        {
            var result1 = await _fileShareApiClient.DownloadFileAsync(Configuration.DownloadFileAsync.BatchId, Configuration.DownloadFileAsync.FileName);

            Assert.That(result1, Is.Not.Null);
            Assert.That(result1.Length, Is.GreaterThan(0));

            var stream = new MemoryStream();

            var result2 = await _fileShareApiClient.DownloadFileAsync(Configuration.DownloadFileAsync.BatchId, Configuration.DownloadFileAsync.FileName, stream, result1.Length, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result2, Is.Not.Null);
                Assert.That(stream.Length, Is.EqualTo(result1.Length));
            });
        }

        [Test]
        public async Task GetUserAttributesAsync()
        {
            var result = await _fileShareApiClient.GetUserAttributesAsync();

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count(), Is.GreaterThan(0));
        }

        [Test]
        public async Task BatchAttributeSearchAsync()
        {
            var result1 = await _fileShareApiClient.BatchAttributeSearchAsync(Configuration.BatchAttributeSearchAsync.SearchQuery, CancellationToken.None);

            Assert.That(result1, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(result1.IsSuccess, Is.True);
                Assert.That(result1.StatusCode, Is.EqualTo(200));
                Assert.That(result1.Data, Is.Not.Null);
            });
            Assert.That(result1.Data.BatchAttributes.Any(x => x.Values.Count > Configuration.BatchAttributeSearchAsync.MaxAttributeValueCount + 1), Is.True);

            var result2 = await _fileShareApiClient.BatchAttributeSearchAsync(Configuration.BatchAttributeSearchAsync.SearchQuery, Configuration.BatchAttributeSearchAsync.MaxAttributeValueCount, CancellationToken.None);

            Assert.That(result2, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(result2.IsSuccess, Is.True);
                Assert.That(result2.StatusCode, Is.EqualTo(200));
                Assert.That(result2.Data, Is.Not.Null);
            });
            Assert.That(result2.Data.BatchAttributes.Any(x => x.Values.Count > Configuration.BatchAttributeSearchAsync.MaxAttributeValueCount + 1), Is.False);
        }

        [Test]
        public async Task DownloadZipFileAsync()
        {
            var result = await _fileShareApiClient.DownloadZipFileAsync(Configuration.DownloadZipFileAsync.BatchId, CancellationToken.None);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Data, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(result.Data.Length, Is.GreaterThan(0));
                Assert.That(result.Errors?.Count, Is.EqualTo(0));
                Assert.That(result.IsSuccess, Is.True);
                Assert.That(result.StatusCode, Is.EqualTo(200));
            });
        }
    }
}
