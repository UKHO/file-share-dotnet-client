using System.Diagnostics.CodeAnalysis;
using NUnit.Framework;
using UKHO.FileShareClient.Models;

namespace FileShareClientTests.Models
{
    public class BatchDetailsLinksTests
    {
        [Test]
        [SuppressMessage("Assertion", "NUnit2010:Use EqualConstraint for better assertion messages in case of failure", Justification = "Test overridden Equals method")]
        public void TestEquals()
        {
            var emptyBatchDetailsLinks = new BatchDetailsLinks();
            var batchDetailsLinks1 = new BatchDetailsLinks(new Link("Link1"));
            var batchDetailsLinks1B = new BatchDetailsLinks(new Link("Link1"));
            var batchDetailsLinks1C = new BatchDetailsLinks(batchDetailsLinks1.Get);
            var batchDetailsLinks2 = new BatchDetailsLinks(new Link("Link2"));

            Assert.Multiple(() =>
            {
                Assert.That(emptyBatchDetailsLinks.Equals(emptyBatchDetailsLinks), Is.True);
                Assert.That(emptyBatchDetailsLinks.Equals(batchDetailsLinks1), Is.False);
                Assert.That(emptyBatchDetailsLinks.Equals(batchDetailsLinks2), Is.False);

                Assert.That(batchDetailsLinks1.Equals(batchDetailsLinks1B), Is.True);
                Assert.That(batchDetailsLinks1.Equals(batchDetailsLinks1C), Is.True);
                Assert.That(batchDetailsLinks1.Equals(batchDetailsLinks2), Is.False);
            });
        }

        [Test]
        [SuppressMessage("Assertion", "NUnit2009:The same value has been provided as both the actual and the expected argument", Justification = "Test overridden GetHashCode method")]
        public void TestGetHashCode()
        {
            var emptyBatchDetailsLinks = new BatchDetailsLinks();
            var batchDetailsLinks1 = new BatchDetailsLinks(new Link("Link1"));
            var batchDetailsLinks1B = new BatchDetailsLinks(new Link("Link1"));

            Assert.Multiple(() =>
            {
                Assert.That(emptyBatchDetailsLinks.GetHashCode(), Is.Not.Zero);
                Assert.That(batchDetailsLinks1.GetHashCode(), Is.Not.Zero);
            });
            Assert.Multiple(() =>
            {
                Assert.That(emptyBatchDetailsLinks.GetHashCode(), Is.EqualTo(emptyBatchDetailsLinks.GetHashCode()));
                Assert.That(batchDetailsLinks1.GetHashCode(), Is.EqualTo(batchDetailsLinks1.GetHashCode()));
            });
            Assert.That(batchDetailsLinks1.GetHashCode(), Is.EqualTo(batchDetailsLinks1B.GetHashCode()));
        }

        [Test]
        public void TestToJson()
        {
            var json = new BatchDetailsLinks(new Link("Link1")).ToJson();
            Assert.That(json, Is.EqualTo("{\"get\":{\"href\":\"Link1\"}}"));
        }
    }
}
