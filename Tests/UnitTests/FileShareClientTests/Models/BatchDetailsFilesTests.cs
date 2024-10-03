using System.Diagnostics.CodeAnalysis;
using UKHO.FileShareClient.Models;

namespace FileShareClientTests.Models
{
    public class BatchDetailsFilesTests
    {
        [Test]
        [SuppressMessage("Assertion", "NUnit2010:Use EqualConstraint for better assertion messages in case of failure", Justification = "Test overridden Equals method")]
        public void TestEquals()
        {
            var emptyBatchDetailsFiles = new BatchDetailsFiles();
            var batchDetailsFiles1 = new BatchDetailsFiles
            {
                Attributes = [new BatchDetailsAttributes("key1", "value1"), new BatchDetailsAttributes("key2", "value2")],
                Links = new BatchDetailsLinks(new Link("getFileLink")),
                Filename = "File123.png",
                FileSize = 123456,
                Hash = "XEsVGA3neuA4B3aR9AWUZw=="
            };
            var batchDetailsFiles1A = new BatchDetailsFiles
            {
                Attributes = [new BatchDetailsAttributes("key1", "value1"), new BatchDetailsAttributes("key2", "value2")],
                Links = new BatchDetailsLinks(new Link("getFileLink")),
                Filename = "File123.png",
                FileSize = 123456,
                Hash = "XEsVGA3neuA4B3aR9AWUZw=="
            };
            var batchDetailsFiles1B = new BatchDetailsFiles
            {
                Attributes = batchDetailsFiles1.Attributes,
                Links = batchDetailsFiles1.Links,
                Filename = "File123.png",
                FileSize = 123456,
                Hash = "XEsVGA3neuA4B3aR9AWUZw=="
            };
            var batchDetailsFiles2 = new BatchDetailsFiles
            {
                Attributes = [new BatchDetailsAttributes("key1", "value2"), new BatchDetailsAttributes("key2", "value2")],
                Links = new BatchDetailsLinks(new Link("getFileLink")),
                Filename = "File123.png",
                FileSize = 123456,
                Hash = "XEsVGA3neuA4B3aR9AWUZw=="
            };
            var batchDetailsFiles3 = new BatchDetailsFiles
            {
                Attributes = [new BatchDetailsAttributes("key1", "value1"), new BatchDetailsAttributes("key2", "value2")],
                Links = new BatchDetailsLinks(new Link("getFileLink3")),
                Filename = "File123.png",
                FileSize = 123456,
                Hash = "XEsVGA3neuA4B3aR9AWUZw=="
            };
            var batchDetailsFiles4 = new BatchDetailsFiles
            {
                Attributes = [new BatchDetailsAttributes("key1", "value1"), new BatchDetailsAttributes("key2", "value2")],
                Links = new BatchDetailsLinks(new Link("getFileLink")),
                Filename = "File1234.png",
                FileSize = 123456,
                Hash = "XEsVGA3neuA4B3aR9AWUZw=="
            };
            var batchDetailsFiles5 = new BatchDetailsFiles
            {
                Attributes = [new BatchDetailsAttributes("key1", "value1"), new BatchDetailsAttributes("key2", "value2")],
                Links = new BatchDetailsLinks(new Link("getFileLink")),
                Filename = "File123.png",
                FileSize = 12345,
                Hash = "XEsVGA3neuA4B3aR9AWUZw=="
            };
            var batchDetailsFiles6 = new BatchDetailsFiles
            {
                Attributes = [new BatchDetailsAttributes("key1", "value1"), new BatchDetailsAttributes("key2", "value2")],
                Links = new BatchDetailsLinks(new Link("getFileLink")),
                Filename = "File123.png",
                FileSize = 123456,
                Hash = "6EsVGA3neuA4B3aR9AWUZw=="
            };
            Assert.Multiple(() =>
            {
                Assert.That(emptyBatchDetailsFiles.Equals(emptyBatchDetailsFiles), Is.True);
                Assert.That(emptyBatchDetailsFiles.Equals(batchDetailsFiles1), Is.False);

                Assert.That(batchDetailsFiles1.Equals(batchDetailsFiles1), Is.True);
                Assert.That(batchDetailsFiles1.Equals(batchDetailsFiles1A), Is.True);
                Assert.That(batchDetailsFiles1.Equals(batchDetailsFiles1B), Is.True);
                Assert.That(batchDetailsFiles1.Equals(emptyBatchDetailsFiles), Is.False);
                Assert.That(batchDetailsFiles1.Equals(batchDetailsFiles2), Is.False);
                Assert.That(batchDetailsFiles1.Equals(batchDetailsFiles3), Is.False);
                Assert.That(batchDetailsFiles1.Equals(batchDetailsFiles4), Is.False);
                Assert.That(batchDetailsFiles1.Equals(batchDetailsFiles5), Is.False);
                Assert.That(batchDetailsFiles1.Equals(batchDetailsFiles6), Is.False);
            });
        }

        [Test]
        [SuppressMessage("Assertion", "NUnit2009:The same value has been provided as both the actual and the expected argument", Justification = "Test overridden GetHashCode method")]
        public void TestGetHashCode()
        {
            var emptyBatchDetailsFiles = new BatchDetailsFiles();
            var batchDetailsFiles1 = new BatchDetailsFiles
            {
                Attributes = [new BatchDetailsAttributes("key1", "value1"), new BatchDetailsAttributes("key2", "value2")],
                Links = new BatchDetailsLinks(new Link("getFileLink")),
                Filename = "File123.png",
                FileSize = 123456,
                Hash = "XEsVGA3neuA4B3aR9AWUZw=="
            };
            var batchDetailsFiles1A = new BatchDetailsFiles
            {
                Attributes = [new BatchDetailsAttributes("key1", "value1"), new BatchDetailsAttributes("key2", "value2")],
                Links = new BatchDetailsLinks(new Link("getFileLink")),
                Filename = "File123.png",
                FileSize = 123456,
                Hash = "XEsVGA3neuA4B3aR9AWUZw=="
            };

            Assert.Multiple(() =>
            {
                Assert.That(emptyBatchDetailsFiles.GetHashCode(), Is.Not.Zero);
                Assert.That(batchDetailsFiles1.GetHashCode(), Is.Not.Zero);
            });
            Assert.Multiple(() =>
            {
                Assert.That(emptyBatchDetailsFiles.GetHashCode(), Is.EqualTo(emptyBatchDetailsFiles.GetHashCode()));
                Assert.That(batchDetailsFiles1.GetHashCode(), Is.EqualTo(batchDetailsFiles1.GetHashCode()));
                Assert.That(batchDetailsFiles1, Is.EqualTo(batchDetailsFiles1A));
                Assert.That(batchDetailsFiles1.GetHashCode(), Is.EqualTo(batchDetailsFiles1A.GetHashCode()));
            });
        }

        [Test]
        public void TestToJson()
        {
            var batchDetailsFiles = new BatchDetailsFiles
            {
                Attributes = [new BatchDetailsAttributes("key1", "value1"), new BatchDetailsAttributes("key2", "value2")],
                Links = new BatchDetailsLinks(new Link("getFileLink")),
                Filename = "File123.png",
                FileSize = 123456,
                Hash = "XEsVGA3neuA4B3aR9AWUZw=="
            };
            var json = batchDetailsFiles.ToJson();
            Assert.That(json, Is.EqualTo($"{{\"filename\":\"{batchDetailsFiles.Filename}\",\"fileSize\":{batchDetailsFiles.FileSize},\"hash\":\"{batchDetailsFiles.Hash}\",\"attributes\":[{string.Join(',', batchDetailsFiles.Attributes.Select(a => a.ToJson()))}],\"links\":{batchDetailsFiles.Links.ToJson()}}}"));
        }
    }
}
