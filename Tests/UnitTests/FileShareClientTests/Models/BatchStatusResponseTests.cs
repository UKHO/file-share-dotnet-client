using System.Diagnostics.CodeAnalysis;
using UKHO.FileShareClient.Models;

namespace FileShareClientTests.Models
{
    public class BatchStatusResponseTests
    {
        [Test]
        [SuppressMessage("Assertion", "NUnit2010:Use EqualConstraint for better assertion messages in case of failure", Justification = "Test overridden Equals method")]
        public void TestEquals()
        {
            var emptyBatchStatusResponse = new BatchStatusResponse();
            var batchStatusResponse1 = new BatchStatusResponse { BatchId = "batch1", Status = BatchStatusResponse.StatusEnum.Committed };
            var batchStatusResponse1A = new BatchStatusResponse { BatchId = "batch1", Status = BatchStatusResponse.StatusEnum.Committed };
            var batchStatusResponse2 = new BatchStatusResponse { BatchId = "batch2", Status = BatchStatusResponse.StatusEnum.Committed };
            var batchStatusResponse3 = new BatchStatusResponse { BatchId = "batch1", Status = BatchStatusResponse.StatusEnum.Rolledback };

            Assert.Multiple(() =>
            {
                Assert.That(emptyBatchStatusResponse.Equals(emptyBatchStatusResponse), Is.True);
                Assert.That(emptyBatchStatusResponse.Equals(batchStatusResponse1), Is.False);

                Assert.That(batchStatusResponse1.Equals(batchStatusResponse1), Is.True);
                Assert.That(batchStatusResponse1.Equals(batchStatusResponse1A), Is.True);
                Assert.That(batchStatusResponse1.Equals(emptyBatchStatusResponse), Is.False);
                Assert.That(batchStatusResponse1.Equals(batchStatusResponse2), Is.False);
                Assert.That(batchStatusResponse1.Equals(batchStatusResponse3), Is.False);
            });
        }

        [Test]
        [SuppressMessage("Assertion", "NUnit2009:The same value has been provided as both the actual and the expected argument", Justification = "Test overridden GetHashCode method")]
        public void TestGetHashCode()
        {
            var emptyBatchStatusResponse = new BatchStatusResponse();
            var batchStatusResponse1 = new BatchStatusResponse { BatchId = "batch1", Status = BatchStatusResponse.StatusEnum.Committed };
            var batchStatusResponse1A = new BatchStatusResponse { BatchId = "batch1", Status = BatchStatusResponse.StatusEnum.Committed };

            Assert.Multiple(() =>
            {
                Assert.That(emptyBatchStatusResponse.GetHashCode(), Is.Not.Zero);
                Assert.That(batchStatusResponse1.GetHashCode(), Is.Not.Zero);
            });
            Assert.Multiple(() =>
            {
                Assert.That(emptyBatchStatusResponse.GetHashCode(), Is.EqualTo(emptyBatchStatusResponse.GetHashCode()));
                Assert.That(batchStatusResponse1.GetHashCode(), Is.EqualTo(batchStatusResponse1.GetHashCode()));
            });
            Assert.That(batchStatusResponse1.GetHashCode(), Is.EqualTo(batchStatusResponse1A.GetHashCode()));
        }

        [Test]
        public void TestToJson()
        {
            var batchStatusResponse = new BatchStatusResponse { BatchId = "batch1", Status = BatchStatusResponse.StatusEnum.Committed };
            Assert.That(batchStatusResponse.ToJson(), Is.EqualTo("{\"batchId\":\"batch1\",\"status\":\"Committed\"}"));
        }
    }
}
