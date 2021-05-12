using NUnit.Framework;
using UKHO.FileShareClient.Models;

namespace UKHO.FileShareClientTests.Models
{
    public class BatchDetailsAttributesTests
    {
        [Test]
        public void TestEquals()
        {
            var emptyBatchDetailsLinks = new BatchDetailsAttributes();
            var batchDetailsLinks1 = new BatchDetailsAttributes("key1", "value1");
            var batchDetailsLinks1B = new BatchDetailsAttributes("key1", "value1");
            var batchDetailsLinks2 = new BatchDetailsAttributes("key1", "value2");
            var batchDetailsLinks3 = new BatchDetailsAttributes("key2", "value1");

            Assert.IsTrue(emptyBatchDetailsLinks.Equals(emptyBatchDetailsLinks));
            Assert.IsFalse(emptyBatchDetailsLinks.Equals(batchDetailsLinks1));
            Assert.IsFalse(emptyBatchDetailsLinks.Equals(batchDetailsLinks2));

            Assert.IsTrue(batchDetailsLinks1.Equals(batchDetailsLinks1B));
            Assert.IsFalse(batchDetailsLinks1.Equals(batchDetailsLinks2));
            Assert.IsFalse(batchDetailsLinks1.Equals(batchDetailsLinks3));
        }

        [Test]
        public void TestGetHashcode()
        {
            var emptyBatchDetailsLinks = new BatchDetailsAttributes();
            var batchDetailsLinks1 = new BatchDetailsAttributes("key1", "value1");
            var batchDetailsLinks1B = new BatchDetailsAttributes("key1", "value1");
            Assert.NotZero(emptyBatchDetailsLinks.GetHashCode());
            Assert.NotZero(batchDetailsLinks1.GetHashCode());
            Assert.AreEqual(emptyBatchDetailsLinks.GetHashCode(), emptyBatchDetailsLinks.GetHashCode());
            Assert.AreEqual(batchDetailsLinks1.GetHashCode(), batchDetailsLinks1.GetHashCode());
            Assert.AreEqual(batchDetailsLinks1.GetHashCode(), batchDetailsLinks1B.GetHashCode());
        }

        [Test]
        public void TestToJson()
        {
            var json = new BatchDetailsAttributes("key1", "value1").ToJson();
            Assert.AreEqual("{\"key\":\"key1\",\"value\":\"value1\"}", json);
        }
    }
}