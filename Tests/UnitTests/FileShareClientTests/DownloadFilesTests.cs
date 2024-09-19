using System.Net;
using System.Text;
using FileShareClientTestsCommon.Helpers;
using UKHO.FileShareClient;

namespace FileShareClientTests
{
    public class DownloadFilesTests
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
        public async Task TestBasicDownloadFile()
        {
            var batchId = Guid.NewGuid().ToString();
            var expectedBytes = Encoding.UTF8.GetBytes("Contents of a file.");
            _nextResponse = new MemoryStream(expectedBytes);

            var batchStatusResponse = await _fileShareApiClient.DownloadFileAsync(batchId, "AFilename.txt");

            Assert.Multiple(() =>
            {
                Assert.That(((MemoryStream)batchStatusResponse).ToArray(), Is.EqualTo(expectedBytes));
                Assert.That(_lastRequestUri?.AbsolutePath, Is.EqualTo($"/basePath/batch/{batchId}/files/AFilename.txt"));
            });
        }

        [Test]
        public async Task TestDownloadFilesForABatchThatDoesNotExist()
        {
            var batchId = Guid.NewGuid().ToString();
            _nextResponseStatusCode = HttpStatusCode.BadRequest;

            try
            {
                await _fileShareApiClient.DownloadFileAsync(batchId, "AFilename.txt");

                Assert.Fail("Expected to throw an exception");
            }
            catch (Exception e)
            {
                Assert.That(e, Is.InstanceOf<HttpRequestException>());
            }

            Assert.That(_lastRequestUri?.AbsolutePath, Is.EqualTo($"/basePath/batch/{batchId}/files/AFilename.txt"));
        }

        [Test]
        public async Task TestDownloadFilesForABatchWithAFileThatDoesNotExist()
        {
            var batchId = Guid.NewGuid().ToString();
            _nextResponseStatusCode = HttpStatusCode.NotFound;

            try
            {
                await _fileShareApiClient.DownloadFileAsync(batchId, "AFilename.txt");

                Assert.Fail("Expected to throw an exception");
            }
            catch (Exception e)
            {
                Assert.That(e, Is.InstanceOf<HttpRequestException>());
            }

            Assert.That(_lastRequestUri?.AbsolutePath, Is.EqualTo($"/basePath/batch/{batchId}/files/AFilename.txt"));
        }

        [Test]
        public async Task TestGetBatchStatusForABatchThatHasBeenDeleted()
        {
            var batchId = Guid.NewGuid().ToString();
            _nextResponseStatusCode = HttpStatusCode.Gone;

            try
            {
                await _fileShareApiClient.DownloadFileAsync(batchId, "AFile.txt");

                Assert.Fail("Expected to throw an exception");
            }
            catch (Exception e)
            {
                Assert.That(e, Is.InstanceOf<HttpRequestException>());
            }

            Assert.That(_lastRequestUri?.AbsolutePath, Is.EqualTo($"/basePath/batch/{batchId}/files/AFile.txt"));
        }

        [Test]
        public async Task TestDownloadFileSetsAuthorizationHeader()
        {
            var batchId = Guid.NewGuid().ToString();
            var expectedBytes = Encoding.UTF8.GetBytes("Contents of a file.");
            _nextResponse = new MemoryStream(expectedBytes);

            await _fileShareApiClient.DownloadFileAsync(batchId, "AFilename.txt");

            Assert.That(_fakeFssHttpClientFactory.HttpClient.DefaultRequestHeaders.Authorization, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(_fakeFssHttpClientFactory.HttpClient.DefaultRequestHeaders.Authorization.Scheme, Is.EqualTo("bearer"));
                Assert.That(_fakeFssHttpClientFactory.HttpClient.DefaultRequestHeaders.Authorization.Parameter, Is.EqualTo(DUMMY_ACCESS_TOKEN));
            });
        }

        [Test]
        public async Task TestBasicDownloadFileWithCancellationToken()
        {
            var batchId = Guid.NewGuid().ToString();
            var expectedBytes = Encoding.UTF8.GetBytes("Contents of a file.");
            _nextResponse = new MemoryStream(expectedBytes);
            var destStream = new MemoryStream();

            var result = await _fileShareApiClient.DownloadFileAsync(batchId, "AFilename.txt", destStream, expectedBytes.Length, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.True);
                Assert.That(_lastRequestUri?.AbsolutePath, Is.EqualTo($"/basePath/batch/{batchId}/files/AFilename.txt"));
            });
        }

        [Test]
        public async Task TestDownloadFileSetsAuthorizationHeaderWithCancellationToken()
        {
            var batchId = Guid.NewGuid().ToString();
            var expectedBytes = Encoding.UTF8.GetBytes("Contents of a file.");
            _nextResponse = new MemoryStream(expectedBytes);
            var destStream = new MemoryStream();

            var result = await _fileShareApiClient.DownloadFileAsync(batchId, "AFilename.txt", destStream, expectedBytes.Length, CancellationToken.None);

            Assert.That(_fakeFssHttpClientFactory.HttpClient.DefaultRequestHeaders.Authorization, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(_fakeFssHttpClientFactory.HttpClient.DefaultRequestHeaders.Authorization.Scheme, Is.EqualTo("bearer"));
                Assert.That(_fakeFssHttpClientFactory.HttpClient.DefaultRequestHeaders.Authorization.Parameter, Is.EqualTo(DUMMY_ACCESS_TOKEN));
            });
        }

        [Test]
        public async Task TestDownloadFilesForABatchThatDoesNotExistWithCancellationToken()
        {
            var batchId = Guid.NewGuid().ToString();
            var expectedBytes = Encoding.UTF8.GetBytes("Contents of a file.");
            _nextResponse = new MemoryStream(expectedBytes);
            var destStream = new MemoryStream();

            var result = await _fileShareApiClient.DownloadFileAsync(batchId, "AFilename.txt", destStream, expectedBytes.Length, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result.StatusCode, Is.EqualTo((int)_nextResponseStatusCode));
                Assert.That(_lastRequestUri?.AbsolutePath, Is.EqualTo($"/basePath/batch/{batchId}/files/AFilename.txt"));
            });
        }

        [Test]
        public async Task TestDownloadFilesForABatchWithAFileThatDoesNotExistWithCancellationToken()
        {
            var batchId = Guid.NewGuid().ToString();
            var expectedBytes = Encoding.UTF8.GetBytes("Contents of a file.");
            _nextResponse = new MemoryStream(expectedBytes);
            var destStream = new MemoryStream();

            var result = await _fileShareApiClient.DownloadFileAsync(batchId, "AFilename.txt", destStream, expectedBytes.Length, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result.StatusCode, Is.EqualTo((int)_nextResponseStatusCode));
                Assert.That(_lastRequestUri?.AbsolutePath, Is.EqualTo($"/basePath/batch/{batchId}/files/AFilename.txt"));
            });
        }

        [Test]
        public async Task TestGetBatchStatusForABatchThatHasBeenDeletedWithCancellationToken()
        {
            var batchId = Guid.NewGuid().ToString();
            var expectedBytes = Encoding.UTF8.GetBytes("Contents of a file.");
            _nextResponse = new MemoryStream(expectedBytes);
            var destStream = new MemoryStream();

            var result = await _fileShareApiClient.DownloadFileAsync(batchId, "AFilename.txt", destStream, expectedBytes.Length, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result.StatusCode, Is.EqualTo((int)_nextResponseStatusCode));
                Assert.That(_lastRequestUri?.AbsolutePath, Is.EqualTo($"/basePath/batch/{batchId}/files/AFilename.txt"));
            });
        }

        [Test]
        public async Task TestForDownloadFilesWhenFileSizeIsGreaterThanMaxDownlodBytes()
        {
            var batchId = Guid.NewGuid().ToString();
            _nextResponseStatusCode = HttpStatusCode.PartialContent;
            var expectedBytes = new byte[10585760];
            _nextResponse = new MemoryStream(expectedBytes);
            var destStream = new MemoryStream();

            var result = await _fileShareApiClient.DownloadFileAsync(batchId, "AFilename.txt", destStream, expectedBytes.Length, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result.StatusCode, Is.EqualTo((int)_nextResponseStatusCode));
                Assert.That(destStream.Length, Is.EqualTo(expectedBytes.Length));
                Assert.That(_lastRequestUri?.AbsolutePath, Is.EqualTo($"/basePath/batch/{batchId}/files/AFilename.txt"));
            });
        }

        [Test]
        public async Task TestForDownloadedFilesbytesIsEqualToExpectedFileBytes()
        {
            var batchId = Guid.NewGuid().ToString();
            var expectedBytes = Encoding.UTF8.GetBytes("Contents of a file.");
            _nextResponse = new MemoryStream(expectedBytes);
            var destStream = new MemoryStream();

            var result = await _fileShareApiClient.DownloadFileAsync(batchId, "AFilename.txt", destStream, expectedBytes.Length, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result.StatusCode, Is.EqualTo((int)_nextResponseStatusCode));
                Assert.That(destStream.Length, Is.EqualTo(expectedBytes.Length));
            });
        }

        [Test]
        public async Task TestBasicDownloadZipFile()
        {
            var batchId = Guid.NewGuid().ToString();

            var expectedBytes = new MemoryStream(Encoding.UTF8.GetBytes("Contents of a file."));
            _nextResponse = expectedBytes;

            var response = await _fileShareApiClient.DownloadZipFileAsync(batchId, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(response.Data, Is.EqualTo(expectedBytes));
                Assert.That(response.StatusCode, Is.EqualTo((int)_nextResponseStatusCode));
                Assert.That(response.IsSuccess, Is.True);
                Assert.That(_lastRequestUri?.AbsolutePath, Is.EqualTo($"/basePath/batch/{batchId}/files"));
            });
        }

        [Test]
        public async Task TestDownloadZipFileForABatchThatDoesNotExist()
        {
            var batchId = Guid.NewGuid().ToString();
            _nextResponseStatusCode = HttpStatusCode.BadRequest;

            var response = await _fileShareApiClient.DownloadZipFileAsync(batchId, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(response.Data, Is.Null);
                Assert.That(response.StatusCode, Is.EqualTo((int)_nextResponseStatusCode));
                Assert.That(response.IsSuccess, Is.False);
                Assert.That(_lastRequestUri?.AbsolutePath, Is.EqualTo($"/basePath/batch/{batchId}/files"));
            });
        }

        [Test]
        public async Task TestDownloadZipFileForABatchWithAFileThatDoesNotExist()
        {
            var batchId = Guid.NewGuid().ToString();
            _nextResponseStatusCode = HttpStatusCode.NotFound;

            var response = await _fileShareApiClient.DownloadZipFileAsync(batchId, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(response.Data, Is.Null);
                Assert.That(response.StatusCode, Is.EqualTo((int)_nextResponseStatusCode));
                Assert.That(response.IsSuccess, Is.False);
                Assert.That(_lastRequestUri?.AbsolutePath, Is.EqualTo($"/basePath/batch/{batchId}/files"));
            });
        }

        [Test]
        public async Task TestGetBatchStatusForABatchZipFileThatHasBeenDeleted()
        {
            var batchId = Guid.NewGuid().ToString();
            _nextResponseStatusCode = HttpStatusCode.Gone;

            var response = await _fileShareApiClient.DownloadZipFileAsync(batchId, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(response.Data, Is.Null);
                Assert.That(response.StatusCode, Is.EqualTo((int)_nextResponseStatusCode));
                Assert.That(response.IsSuccess, Is.False);
                Assert.That(_lastRequestUri?.AbsolutePath, Is.EqualTo($"/basePath/batch/{batchId}/files"));
            });
        }

        [Test]
        public async Task TestDownloadZipFileSetsAuthorizationHeader()
        {
            var batchId = Guid.NewGuid().ToString();
            var expectedBytes = Encoding.UTF8.GetBytes("Contents of a file.");
            _nextResponse = new MemoryStream(expectedBytes);

            await _fileShareApiClient.DownloadZipFileAsync(batchId, CancellationToken.None);

            Assert.That(_fakeFssHttpClientFactory.HttpClient.DefaultRequestHeaders.Authorization, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(_fakeFssHttpClientFactory.HttpClient.DefaultRequestHeaders.Authorization.Scheme, Is.EqualTo("bearer"));
                Assert.That(_fakeFssHttpClientFactory.HttpClient.DefaultRequestHeaders.Authorization.Parameter, Is.EqualTo(DUMMY_ACCESS_TOKEN));
            });
        }
    }
}
