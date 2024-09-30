using System.Diagnostics.CodeAnalysis;
using UKHO.FileShareClient.Models;

namespace FileShareClientTests.Models
{
    public class BatchSearchResponseTests
    {
        [Test]
        [SuppressMessage("Assertion", "NUnit2010:Use EqualConstraint for better assertion messages in case of failure", Justification = "Test overridden Equals method")]
        public void TestEquals()
        {
            var emptyBatchSearchResponse = new BatchSearchResponse();
            var batchSearchResponse1 = new BatchSearchResponse
            {
                Count = 2,
                Entries = [new BatchDetails("batch1"), new BatchDetails("batch2")],
                Links = new Links(new Link("selfLink1")),
                Total = 2
            };
            var batchSearchResponse1a = new BatchSearchResponse
            {
                Count = 2,
                Entries = [new BatchDetails("batch1"), new BatchDetails("batch2")],
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
                Entries = [new BatchDetails("batch1")],
                Links = new Links(new Link("selfLink1"), new Link("first"), new Link("previous")),
                Total = 2
            };

            var batchSearchResponse3 = new BatchSearchResponse
            {
                Count = 2,
                Entries = [new BatchDetails("batch3"), new BatchDetails("batch2")],
                Links = new Links(new Link("selfLink1")),
                Total = 2
            };

            Assert.Multiple(() =>
            {
                Assert.That(emptyBatchSearchResponse.Equals(emptyBatchSearchResponse), Is.True);
                Assert.That(emptyBatchSearchResponse.Equals(batchSearchResponse1), Is.False);

                Assert.That(batchSearchResponse1.Equals(batchSearchResponse1), Is.True);
                Assert.That(batchSearchResponse1.Equals(batchSearchResponse1a), Is.True);
                Assert.That(batchSearchResponse1.Equals(batchSearchResponse1b), Is.True);
                Assert.That(batchSearchResponse1.Equals(emptyBatchSearchResponse), Is.False);
                Assert.That(batchSearchResponse1.Equals(batchSearchResponse2), Is.False);
                Assert.That(batchSearchResponse1.Equals(batchSearchResponse3), Is.False);
            });
        }

        [Test]
        [SuppressMessage("Assertion", "NUnit2009:The same value has been provided as both the actual and the expected argument", Justification = "Test overridden GetHashCode method")]
        public void TestGetHashcode()
        {
            var emptyBatchSearchResponse = new BatchSearchResponse();
            var batchSearchResponse1 = new BatchSearchResponse
            {
                Count = 2,
                Entries = [new BatchDetails("batch1"), new BatchDetails("batch2")],
                Links = new Links(new Link("selfLink1")),
                Total = 2
            };

            Assert.Multiple(() =>
            {
                Assert.That(emptyBatchSearchResponse.GetHashCode(), Is.Not.Zero);
                Assert.That(batchSearchResponse1.GetHashCode(), Is.Not.Zero);
            });
            Assert.Multiple(() =>
            {
                Assert.That(emptyBatchSearchResponse.GetHashCode(), Is.EqualTo(emptyBatchSearchResponse.GetHashCode()));
                Assert.That(batchSearchResponse1.GetHashCode(), Is.EqualTo(batchSearchResponse1.GetHashCode()));
            });
        }

        [Test]
        public void TestToJson()
        {
            var batchDetailsList = new List<BatchDetails>(2) { new("batch1"), new("batch2") };
            var links = new Links(new Link("selfLink1"), new Link("first"), new Link("previous"), new Link("next"));
            var batchSearchResponse = new BatchSearchResponse
            {
                Count = 2,
                Entries = batchDetailsList,
                Links = links,
                Total = 9
            };
            Assert.That(batchSearchResponse.ToJson(), Is.EqualTo($"{{\"count\":2,\"total\":9,\"entries\":[{string.Join(',', batchDetailsList.Select(e => e.ToJson()))}],\"_links\":{links.ToJson()}}}"));
        }
    }
}
