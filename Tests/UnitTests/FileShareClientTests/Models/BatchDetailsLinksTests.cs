using NUnit.Framework;
using UKHO.FileShareClient.Models;

namespace UKHO.FileShareClientTests.Models
{
    public class BatchDetailsLinksTests
    {
        [Test]
        public void TestEquals()
        {
            var emptyBatchDetailsLinks = new BatchDetailsLinks();
            var batchDetailsLinks1 = new BatchDetailsLinks(new Link("Link1"));
            var batchDetailsLinks1B = new BatchDetailsLinks(new Link("Link1"));
            var batchDetailsLinks1C = new BatchDetailsLinks(batchDetailsLinks1.Get);
            var batchDetailsLinks2 = new BatchDetailsLinks(new Link("Link2"));

            Assert.IsTrue(emptyBatchDetailsLinks.Equals(emptyBatchDetailsLinks));
            Assert.IsFalse(emptyBatchDetailsLinks.Equals(batchDetailsLinks1));
            Assert.IsFalse(emptyBatchDetailsLinks.Equals(batchDetailsLinks2));

            Assert.IsTrue(batchDetailsLinks1.Equals(batchDetailsLinks1B));
            Assert.IsTrue(batchDetailsLinks1.Equals(batchDetailsLinks1C));
            Assert.IsFalse(batchDetailsLinks1.Equals(batchDetailsLinks2));
        }

        [Test]
        public void TestGetHashcode()
        {
            var emptyBatchDetailsLinks = new BatchDetailsLinks();
            var batchDetailsLinks1 = new BatchDetailsLinks(new Link("Link1"));
            var batchDetailsLinks1B = new BatchDetailsLinks(new Link("Link1"));

            Assert.NotZero(emptyBatchDetailsLinks.GetHashCode());
            Assert.NotZero(batchDetailsLinks1.GetHashCode());
            Assert.AreEqual(emptyBatchDetailsLinks.GetHashCode(), emptyBatchDetailsLinks.GetHashCode());
            Assert.AreEqual(batchDetailsLinks1.GetHashCode(), batchDetailsLinks1.GetHashCode());
            Assert.AreEqual(batchDetailsLinks1.GetHashCode(), batchDetailsLinks1B.GetHashCode());
        }

        [Test]
        public void TestToJson()
        {
            var json = new BatchDetailsLinks(new Link("Link1")).ToJson();
            Assert.AreEqual("{\"get\":{\"href\":\"Link1\"}}", json);
        }
    }
}