using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using UKHO.FileShareClient;
using UKHO.FileShareClientTests.Helpers;

namespace UKHO.FileShareClientTests
{
    public class DownloadFilesTests
    {
        private object nextResponse = null;
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
        public async Task TestBasicDownloadFile()
        {
            var batchId = Guid.NewGuid().ToString();
            var expectedBytes = Encoding.UTF8.GetBytes("Contents of a file.");
            nextResponse = new MemoryStream(expectedBytes);

            var batchStatusResponse = await fileShareApiClient.DownloadFileAsync(batchId, "AFilename.txt");
            Assert.AreEqual(expectedBytes, ((MemoryStream) batchStatusResponse).ToArray());
            Assert.AreEqual($"/basePath/batch/{batchId}/files/AFilename.txt", lastRequestUri.AbsolutePath);
        }

        [Test]
        public async Task TestDownloadFilesForABatchThatDoesNotExist()
        {
            var batchId = Guid.NewGuid().ToString();

            nextResponseStatusCode = HttpStatusCode.BadRequest;

            try
            {
                await fileShareApiClient.DownloadFileAsync(batchId, "AFilename.txt");
                Assert.Fail("Expected to throw an exception");
            }
            catch (Exception e)
            {
                Assert.IsInstanceOf<HttpRequestException>(e);
            }

            Assert.AreEqual($"/basePath/batch/{batchId}/files/AFilename.txt", lastRequestUri.AbsolutePath);
        }

        [Test]
        public async Task TestDownloadFilesForABatchWithAFileThatDoesNotExist()
        {
            var batchId = Guid.NewGuid().ToString();

            nextResponseStatusCode = HttpStatusCode.NotFound;

            try
            {
                await fileShareApiClient.DownloadFileAsync(batchId, "AFilename.txt");
                Assert.Fail("Expected to throw an exception");
            }
            catch (Exception e)
            {
                Assert.IsInstanceOf<HttpRequestException>(e);
            }

            Assert.AreEqual($"/basePath/batch/{batchId}/files/AFilename.txt", lastRequestUri.AbsolutePath);
        }

        [Test]
        public async Task TestGetBatchStatusForABatchThatHasBeenDeleted()
        {
            var batchId = Guid.NewGuid().ToString();

            nextResponseStatusCode = HttpStatusCode.Gone;

            try
            {
                await fileShareApiClient.DownloadFileAsync(batchId, "AFile.txt");
                Assert.Fail("Expected to throw an exception");
            }
            catch (Exception e)
            {
                Assert.IsInstanceOf<HttpRequestException>(e);
            }

            Assert.AreEqual($"/basePath/batch/{batchId}/files/AFile.txt", lastRequestUri.AbsolutePath);
        }
    }
}