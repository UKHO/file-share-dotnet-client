using System.Diagnostics.CodeAnalysis;
using UKHO.FileShareClient.Models;

namespace FileShareClientTests.Models
{
    public class BatchDetailsTests
    {
        [Test]
        [SuppressMessage("Assertion", "NUnit2010:Use EqualConstraint for better assertion messages in case of failure", Justification = "Test overridden Equals method")]
        public void TestEquals()
        {
            var emptyBatchDetails = new BatchDetails();
            var batchDetails1 = new BatchDetails
            {
                BatchId = Guid.NewGuid().ToString(),
                BusinessUnit = "BU1",
                Status = BatchDetails.StatusEnum.Incomplete
            };
            var batchDetails1B = new BatchDetails
            {
                BatchId = batchDetails1.BatchId,
                BusinessUnit = "BU1",
                Status = BatchDetails.StatusEnum.Incomplete
            };
            var link2 = new BatchDetails
            {
                BatchId = batchDetails1.BatchId,
                BusinessUnit = "BU2",
                Status = BatchDetails.StatusEnum.Rolledback
            };

            Assert.Multiple(() =>
            {
                Assert.That(emptyBatchDetails.Equals(emptyBatchDetails), Is.True);
                Assert.That(emptyBatchDetails.Equals(batchDetails1), Is.False);
                Assert.That(emptyBatchDetails.Equals(link2), Is.False);

                Assert.That(batchDetails1.Equals(batchDetails1B), Is.True);
                Assert.That(batchDetails1.Equals(link2), Is.False);
            });
        }

        [Test]
        [SuppressMessage("Assertion", "NUnit2009:The same value has been provided as both the actual and the expected argument", Justification = "Test overridden GetHashCode method")]
        public void TestGetHashCode()
        {
            var emptyBatchDetails = new BatchDetails();
            var batchDetails1 = new BatchDetails
            {
                BatchId = Guid.NewGuid().ToString(),
                BusinessUnit = "BU1",
                Status = BatchDetails.StatusEnum.Incomplete,
                Attributes = []
            };
            var batchDetails1B = new BatchDetails
            {
                BatchId = batchDetails1.BatchId,
                BusinessUnit = "BU1",
                Status = BatchDetails.StatusEnum.Incomplete,
                Attributes = []
            };

            Assert.Multiple(() =>
            {
                Assert.That(emptyBatchDetails.GetHashCode(), Is.Not.Zero);
                Assert.That(batchDetails1.GetHashCode(), Is.Not.Zero);
            });
            Assert.Multiple(() =>
            {
                Assert.That(emptyBatchDetails.GetHashCode(), Is.EqualTo(emptyBatchDetails.GetHashCode()));
                Assert.That(batchDetails1.GetHashCode(), Is.EqualTo(batchDetails1.GetHashCode()));
                Assert.That(batchDetails1.GetHashCode(), Is.EqualTo(batchDetails1B.GetHashCode()));
            });
        }

        [Test]
        public void TestToJson()
        {
            var batchDetails = new BatchDetails
            {
                BatchId = Guid.NewGuid().ToString(),
                BusinessUnit = "BU1",
                Status = BatchDetails.StatusEnum.Incomplete
            };
            Assert.That(batchDetails.ToJson(), Is.EqualTo($"{{\"batchId\":\"{batchDetails.BatchId}\",\"status\":\"Incomplete\",\"businessUnit\":\"BU1\"}}"));
        }
    }
}
