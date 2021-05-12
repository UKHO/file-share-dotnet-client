using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UKHO.FileShareClient.Models;

namespace UKHO.FileShareClientTests.Models
{
    public class BatchSearchResponseTests
    {
        [Test]
        public void TestEquals()
        {
            var emptyBatchSearchResponse = new BatchSearchResponse();
            var batchSearchResponse1 = new BatchSearchResponse
            {
                Count = 2,
                Entries = new List<BatchDetails>(2) {new BatchDetails("batch1"), new BatchDetails("batch2")},
                Links = new Links(new Link("selfLink1")),
                Total = 2
            };
            var batchSearchResponse1a = new BatchSearchResponse
            {
                Count = 2,
                Entries = new List<BatchDetails>(2) {new BatchDetails("batch1"), new BatchDetails("batch2")},
                Links = new Links(new Link("selfLink1")),
                Total = 2
            };
            var batchSearchResponse1b = new BatchSearchResponse
            {
                Count = 2,
                Entries = batchSearchResponse1.Entries,
                Links = batchSearchResponse1.Links,
                Total = 2
            };

            var batchSearchResponse2 = new BatchSearchResponse
            {
                Count = 1,
                Entries = new List<BatchDetails>(1) {new BatchDetails("batch1")},
                Links = new Links(new Link("selfLink1"), new Link("first"), new Link("previous")),
                Total = 2
            };

            var batchSearchResponse3 = new BatchSearchResponse
            {
                Count = 2,
                Entries = new List<BatchDetails>(2) {new BatchDetails("batch3"), new BatchDetails("batch2")},
                Links = new Links(new Link("selfLink1")),
                Total = 2
            };

            Assert.IsTrue(emptyBatchSearchResponse.Equals(emptyBatchSearchResponse));
            Assert.IsFalse(emptyBatchSearchResponse.Equals(batchSearchResponse1));

            Assert.IsTrue(batchSearchResponse1.Equals(batchSearchResponse1));
            Assert.IsTrue(batchSearchResponse1.Equals(batchSearchResponse1a));
            Assert.IsTrue(batchSearchResponse1.Equals(batchSearchResponse1b));
            Assert.IsFalse(batchSearchResponse1.Equals(emptyBatchSearchResponse));
            Assert.IsFalse(batchSearchResponse1.Equals(batchSearchResponse2));
            Assert.IsFalse(batchSearchResponse1.Equals(batchSearchResponse3));
        }

        [Test]
        public void TestGetHashcode()
        {
            var emptyBatchSearchResponse = new BatchSearchResponse();
            var batchSearchResponse1 = new BatchSearchResponse
            {
                Count = 2,
                Entries = new List<BatchDetails>(2) {new BatchDetails("batch1"), new BatchDetails("batch2")},
                Links = new Links(new Link("selfLink1")),
                Total = 2
            };

            Assert.NotZero(emptyBatchSearchResponse.GetHashCode());
            Assert.AreEqual(emptyBatchSearchResponse.GetHashCode(), emptyBatchSearchResponse.GetHashCode());
            Assert.NotZero(batchSearchResponse1.GetHashCode());
            Assert.AreEqual(batchSearchResponse1.GetHashCode(), batchSearchResponse1.GetHashCode());
        }

        [Test]
        public void TestToJson()
        {
            var batchDetailsList = new List<BatchDetails>(2) {new BatchDetails("batch1"), new BatchDetails("batch2")};
            var links = new Links(new Link("selfLink1"), new Link("first"), new Link("previous"), new Link("next"));
            var json = new BatchSearchResponse
            {
                Count = 2,
                Entries = batchDetailsList,
                Links = links,
                Total = 9
            }.ToJson();
            Assert.AreEqual(
                $"{{\"count\":2,\"total\":9,\"entries\":[{string.Join(',', batchDetailsList.Select(e => e.ToJson()))}],\"_links\":{links.ToJson()}}}",
                json);
        }
    }
}