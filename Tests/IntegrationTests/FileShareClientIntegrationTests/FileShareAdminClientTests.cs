using System.Text;
using FileShareClientIntegrationTests.Helpers;
using UKHO.FileShareAdminClient;
using UKHO.FileShareAdminClient.Models;

namespace FileShareClientIntegrationTests
{
    public class FileShareAdminClientTests
    {
        private FileShareApiAdminClient _fileShareApiAdminClient;
        private BatchModel _batchModel;
        private DateTime _expiryDate;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            _fileShareApiAdminClient = new FileShareApiAdminClient(Configuration.HttpClientFactory, Configuration.FssUrl, Configuration.AuthTokenProvider);
            _expiryDate = DateTime.Now.AddMinutes(15);
            _batchModel = new BatchModel
            {
                BusinessUnit = "TEST",
                Acl = new Acl
                {
                    ReadUsers = [],
                    ReadGroups = []
                },
                Attributes = [],
                ExpiryDate = _expiryDate
            };
        }

        private async Task<IBatchHandle> CreateBatchAsync()
        {
            var batchHandle = await _fileShareApiAdminClient.CreateBatchAsync(_batchModel);
            Assert.That(batchHandle, Is.Not.Null);
            TestContext.Progress.WriteLine($"BatchId: {batchHandle.BatchId}");
            return batchHandle;
        }

        private async Task RollBackBatchAsync(IBatchHandle batchHandle)
        {
            var result = await _fileShareApiAdminClient.RollBackBatchAsync(batchHandle, CancellationToken.None);

            Assert.That(result, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.True);
                Assert.That(result.StatusCode, Is.EqualTo(204));
            });
            TestContext.Progress.WriteLine("Batch rolled back");
        }

        private async Task CommitBatchAsync(IBatchHandle batchHandle)
        {
            var result = await _fileShareApiAdminClient.CommitBatchAsync(batchHandle, CancellationToken.None);

            Assert.That(result, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.True);
                Assert.That(result.StatusCode, Is.EqualTo(202));
            });
            TestContext.Progress.WriteLine("Batch committed");
        }

        [Test]
        public async Task AppendAclAsync()
        {
            var batchHandle = await CreateBatchAsync();
            var acl = new Acl
            {
                ReadUsers = [],
                ReadGroups = ["FileShareAdminClientTests AppendAclAsync"]
            };

            var result = await _fileShareApiAdminClient.AppendAclAsync(batchHandle.BatchId, acl);

            Assert.That(result, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.True);
                Assert.That(result.StatusCode, Is.EqualTo(204));
            });

            await RollBackBatchAsync(batchHandle);
        }

        [Test]
        public async Task GetBatchStatusAsync()
        {
            var batchHandle = await CreateBatchAsync();

            var result = await _fileShareApiAdminClient.GetBatchStatusAsync(batchHandle.BatchId);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Status, Is.Not.Null);

            await RollBackBatchAsync(batchHandle);
        }

        [Test]
        public async Task AddFileToBatchAsync()
        {
            var batchHandle = await CreateBatchAsync();
            var stream = new MemoryStream(Encoding.UTF8.GetBytes("FileShareAdminClientTests - AddFileToBatchAsync"));

            var result = await _fileShareApiAdminClient.AddFileToBatchAsync(batchHandle, stream, "test.txt", "text/plain", CancellationToken.None);

            Assert.That(result, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.True);
                Assert.That(result.StatusCode, Is.EqualTo(204));
            });

            await CommitBatchAsync(batchHandle);
        }

        [Test]
        public async Task ReplaceAclAsync()
        {
            var batchHandle = await CreateBatchAsync();
            var acl = new Acl
            {
                ReadUsers = [],
                ReadGroups = ["FileShareAdminClientTests ReplaceAclAsync"]
            };

            var result = await _fileShareApiAdminClient.ReplaceAclAsync(batchHandle.BatchId, acl);

            Assert.That(result, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.True);
                Assert.That(result.StatusCode, Is.EqualTo(204));
            });

            await RollBackBatchAsync(batchHandle);
        }

        [Test]
        public async Task SetExpiryDateAsync()
        {
            var batchHandle = await CreateBatchAsync();
            var batchExpiryModel = new BatchExpiryModel
            {
                ExpiryDate = _expiryDate.AddMinutes(1)
            };

            var result = await _fileShareApiAdminClient.SetExpiryDateAsync(batchHandle.BatchId, batchExpiryModel);

            Assert.That(result, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.True);
                Assert.That(result.StatusCode, Is.EqualTo(204));
            });

            await RollBackBatchAsync(batchHandle);
        }
    }
}
