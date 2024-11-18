using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FileShareClientIntegrationTests.Helpers;
using NUnit.Framework;
using UKHO.FileShareAdminClient;
using UKHO.FileShareAdminClient.Models;

namespace FileShareClientIntegrationTests
{
    public class FileShareAdminClientTests
    {
        private FileShareApiAdminClient _fileShareApiAdminClient;
        private BatchModel _batchModel;
        private DateTime _expiryDate;
        private IBatchHandle _batchHandle;

        [SetUp]
        public async Task SetUp()
        {
            _fileShareApiAdminClient = new FileShareApiAdminClient(Configuration.HttpClientFactory, Configuration.FssUrl, Configuration.AuthTokenProvider);
            _expiryDate = DateTime.Now.AddMinutes(15);
            _batchModel = new BatchModel
            {
                BusinessUnit = Configuration.FssTestBusinessUnit,
                Acl = new Acl
                {
                    ReadUsers = new List<string>(),
                    ReadGroups = new List<string>()
                },
                Attributes = new List<KeyValuePair<string, string>>(),
                ExpiryDate = _expiryDate
            };
            _batchHandle = await _fileShareApiAdminClient.CreateBatchAsync(_batchModel);
            Assert.That(_batchHandle, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(_batchHandle.BatchId, Is.Not.Null);
                Assert.That(Guid.TryParse(_batchHandle.BatchId, out _), Is.True);
            });
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
        }

        [Test]
        public async Task AppendAclAsync()
        {
            var acl = new Acl
            {
                ReadUsers = new List<string>(),
                ReadGroups = new List<string> { "FileShareAdminClientTests AppendAclAsync" }
            };

            var result = await _fileShareApiAdminClient.AppendAclAsync(_batchHandle.BatchId, acl);

            Assert.That(result, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.True);
                Assert.That(result.StatusCode, Is.EqualTo(204));
            });

            await RollBackBatchAsync(_batchHandle);
        }

        [Test]
        public async Task GetBatchStatusAsync()
        {
            var result = await _fileShareApiAdminClient.GetBatchStatusAsync(_batchHandle.BatchId);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Status, Is.Not.Null);

            await RollBackBatchAsync(_batchHandle);
        }

        [Test]
        public async Task AddFileToBatchAsync()
        {
            var stream = new MemoryStream(Encoding.UTF8.GetBytes("FileShareAdminClientTests - AddFileToBatchAsync"));

            var result = await _fileShareApiAdminClient.AddFileToBatchAsync(_batchHandle, stream, "test.txt", "text/plain", CancellationToken.None);

            Assert.That(result, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.True);
                Assert.That(result.StatusCode, Is.EqualTo(204));
            });

            await CommitBatchAsync(_batchHandle);
        }

        [Test]
        public async Task ReplaceAclAsync()
        {
            var acl = new Acl
            {
                ReadUsers = new List<string>(),
                ReadGroups = new List<string> { "FileShareAdminClientTests ReplaceAclAsync" }
            };

            var result = await _fileShareApiAdminClient.ReplaceAclAsync(_batchHandle.BatchId, acl);

            Assert.That(result, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.True);
                Assert.That(result.StatusCode, Is.EqualTo(204));
            });

            await RollBackBatchAsync(_batchHandle);
        }

        [Test]
        public async Task SetExpiryDateAsync()
        {
            var batchExpiryModel = new BatchExpiryModel
            {
                ExpiryDate = _expiryDate.AddMinutes(1)
            };

            var result = await _fileShareApiAdminClient.SetExpiryDateAsync(_batchHandle.BatchId, batchExpiryModel);

            Assert.That(result, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.True);
                Assert.That(result.StatusCode, Is.EqualTo(204));
            });

            await RollBackBatchAsync(_batchHandle);
        }
    }
}
