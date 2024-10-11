using FileShareClientIntegrationTests.Helpers;
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
            var result = await _fileShareApiClient.SearchAsync("$batch(Product Type) eq 'ADP' and contains($batch(Content), 'Software')");

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.GreaterThan(0));
        }
    }
}