using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UKHO.FileShareClient.Models;

namespace UKHO.FileShareClientTests.Models
{
    public class BatchDetailsFilesTests
    {
        [Test]
        public void TestEquals()
        {
            var emptyBatchDetailsFiles = new BatchDetailsFiles();
            var batchDetailsFiles1 = new BatchDetailsFiles
            {
                Attributes = new List<BatchDetailsAttributes>(2)
                    {new BatchDetailsAttributes("key1", "value1"), new BatchDetailsAttributes("key2", "value2")},
                Links = new BatchDetailsLinks(new Link("getFileLink")),
                Filename = "File123.png",
                FileSize = 123456,
                Hash = "XEsVGA3neuA4B3aR9AWUZw=="
            };
            var batchDetailsFiles1A = new BatchDetailsFiles
            {
                Attributes = new List<BatchDetailsAttributes>(2)
                    {new BatchDetailsAttributes("key1", "value1"), new BatchDetailsAttributes("key2", "value2")},
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
                Attributes = new List<BatchDetailsAttributes>(2)
                    {new BatchDetailsAttributes("key1", "value2"), new BatchDetailsAttributes("key2", "value2")},
                Links = new BatchDetailsLinks(new Link("getFileLink")),
                Filename = "File123.png",
                FileSize = 123456,
                Hash = "XEsVGA3neuA4B3aR9AWUZw=="
            };
            var batchDetailsFiles3 = new BatchDetailsFiles
            {
                Attributes = new List<BatchDetailsAttributes>(2)
                    {new BatchDetailsAttributes("key1", "value1"), new BatchDetailsAttributes("key2", "value2")},
                Links = new BatchDetailsLinks(new Link("getFileLink3")),
                Filename = "File123.png",
                FileSize = 123456,
                Hash = "XEsVGA3neuA4B3aR9AWUZw=="
            };
            var batchDetailsFiles4 = new BatchDetailsFiles
            {
                Attributes = new List<BatchDetailsAttributes>(2)
                    {new BatchDetailsAttributes("key1", "value1"), new BatchDetailsAttributes("key2", "value2")},
                Links = new BatchDetailsLinks(new Link("getFileLink")),
                Filename = "File1234.png",
                FileSize = 123456,
                Hash = "XEsVGA3neuA4B3aR9AWUZw=="
            };
            var batchDetailsFiles5 = new BatchDetailsFiles
            {
                Attributes = new List<BatchDetailsAttributes>(2)
                    {new BatchDetailsAttributes("key1", "value1"), new BatchDetailsAttributes("key2", "value2")},
                Links = new BatchDetailsLinks(new Link("getFileLink")),
                Filename = "File123.png",
                FileSize = 12345,
                Hash = "XEsVGA3neuA4B3aR9AWUZw=="
            };
            var batchDetailsFiles6 = new BatchDetailsFiles
            {
                Attributes = new List<BatchDetailsAttributes>(2)
                    {new BatchDetailsAttributes("key1", "value1"), new BatchDetailsAttributes("key2", "value2")},
                Links = new BatchDetailsLinks(new Link("getFileLink")),
                Filename = "File123.png",
                FileSize = 123456,
                Hash = "6EsVGA3neuA4B3aR9AWUZw=="
            };
            Assert.IsTrue(emptyBatchDetailsFiles.Equals(emptyBatchDetailsFiles));
            Assert.IsFalse(emptyBatchDetailsFiles.Equals(batchDetailsFiles1));

            Assert.IsTrue(batchDetailsFiles1.Equals(batchDetailsFiles1));
            Assert.IsTrue(batchDetailsFiles1.Equals(batchDetailsFiles1A));
            Assert.IsTrue(batchDetailsFiles1.Equals(batchDetailsFiles1B));
            Assert.IsFalse(batchDetailsFiles1.Equals(emptyBatchDetailsFiles));
            Assert.IsFalse(batchDetailsFiles1.Equals(batchDetailsFiles2));
            Assert.IsFalse(batchDetailsFiles1.Equals(batchDetailsFiles3));
            Assert.IsFalse(batchDetailsFiles1.Equals(batchDetailsFiles4));
            Assert.IsFalse(batchDetailsFiles1.Equals(batchDetailsFiles5));
            Assert.IsFalse(batchDetailsFiles1.Equals(batchDetailsFiles6));
        }

        [Test]
        public void TestGetHashcode()
        {
            var emptyBatchDetailsFiles = new BatchDetailsFiles();
            var batchDetailsFiles1 = new BatchDetailsFiles
            {
                Attributes = new List<BatchDetailsAttributes>(2)
                    {new BatchDetailsAttributes("key1", "value1"), new BatchDetailsAttributes("key2", "value2")},
                Links = new BatchDetailsLinks(new Link("getFileLink")),
                Filename = "File123.png",
                FileSize = 123456,
                Hash = "XEsVGA3neuA4B3aR9AWUZw=="
            };
            var batchDetailsFiles1A = new BatchDetailsFiles
            {
                Attributes = new List<BatchDetailsAttributes>(2)
                    {new BatchDetailsAttributes("key1", "value1"), new BatchDetailsAttributes("key2", "value2")},
                Links = new BatchDetailsLinks(new Link("getFileLink")),
                Filename = "File123.png",
                FileSize = 123456,
                Hash = "XEsVGA3neuA4B3aR9AWUZw=="
            };

            Assert.NotZero(emptyBatchDetailsFiles.GetHashCode());
            Assert.AreEqual(emptyBatchDetailsFiles.GetHashCode(), emptyBatchDetailsFiles.GetHashCode());

            Assert.NotZero(batchDetailsFiles1.GetHashCode());
            Assert.AreEqual(batchDetailsFiles1.GetHashCode(), batchDetailsFiles1.GetHashCode());
            Assert.IsTrue(batchDetailsFiles1.Equals(batchDetailsFiles1A));
            Assert.AreEqual(batchDetailsFiles1.GetHashCode(), batchDetailsFiles1A.GetHashCode());
        }

        [Test]
        public void TestToJson()
        {
            var batchDetailsFiles = new BatchDetailsFiles
            {
                Attributes = new List<BatchDetailsAttributes>(2)
                    {new BatchDetailsAttributes("key1", "value1"), new BatchDetailsAttributes("key2", "value2")},
                Links = new BatchDetailsLinks(new Link("getFileLink")),
                Filename = "File123.png",
                FileSize = 123456,
                Hash = "XEsVGA3neuA4B3aR9AWUZw=="
            };
            var json = batchDetailsFiles.ToJson();
            Assert.AreEqual(
                $"{{\"filename\":\"{batchDetailsFiles.Filename}\",\"fileSize\":{batchDetailsFiles.FileSize},\"hash\":\"{batchDetailsFiles.Hash}\",\"attributes\":[{string.Join(',', batchDetailsFiles.Attributes.Select(a => a.ToJson()))}],\"links\":{batchDetailsFiles.Links.ToJson()}}}",
                json);
        }
    }
}