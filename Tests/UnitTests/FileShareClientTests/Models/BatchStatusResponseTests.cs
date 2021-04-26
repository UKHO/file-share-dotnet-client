using NUnit.Framework;
using UKHO.FileShareClient.Models;

namespace UKHO.FileShareClientTests.Models
{
    public class BatchStatusResponseTests
    {
        [Test]
        public void TestEquals()
        {
            var emptyBatchStatusResponse = new BatchStatusResponse();
            var batchStatusResponse1 = new BatchStatusResponse
                {BatchId = "batch1", Status = BatchStatusResponse.StatusEnum.Committed};
            var batchStatusResponse1A = new BatchStatusResponse
                {BatchId = "batch1", Status = BatchStatusResponse.StatusEnum.Committed};
            var batchStatusResponse2 = new BatchStatusResponse
                {BatchId = "batch2", Status = BatchStatusResponse.StatusEnum.Committed};
            var batchStatusResponse3 = new BatchStatusResponse
                {BatchId = "batch1", Status = BatchStatusResponse.StatusEnum.Rolledback};

            Assert.IsTrue(emptyBatchStatusResponse.Equals(emptyBatchStatusResponse));
            Assert.IsFalse(emptyBatchStatusResponse.Equals(batchStatusResponse1));

            Assert.IsTrue(batchStatusResponse1.Equals(batchStatusResponse1));
            Assert.IsTrue(batchStatusResponse1.Equals(batchStatusResponse1A));
            Assert.IsFalse(batchStatusResponse1.Equals(emptyBatchStatusResponse));
            Assert.IsFalse(batchStatusResponse1.Equals(batchStatusResponse2));
            Assert.IsFalse(batchStatusResponse1.Equals(batchStatusResponse3));
        }

        [Test]
        public void TestGetHashcode()
        {
            var emptyBatchStatusResponse = new BatchStatusResponse();
            var batchStatusResponse1 = new BatchStatusResponse
                {BatchId = "batch1", Status = BatchStatusResponse.StatusEnum.Committed};
            var batchStatusResponse1A = new BatchStatusResponse
                {BatchId = "batch1", Status = BatchStatusResponse.StatusEnum.Committed};


            Assert.NotZero(emptyBatchStatusResponse.GetHashCode());
            Assert.AreEqual(emptyBatchStatusResponse.GetHashCode(), emptyBatchStatusResponse.GetHashCode());
            Assert.NotZero(batchStatusResponse1.GetHashCode());
            Assert.AreEqual(batchStatusResponse1.GetHashCode(), batchStatusResponse1.GetHashCode());
            Assert.AreEqual(batchStatusResponse1.GetHashCode(), batchStatusResponse1A.GetHashCode());
        }

        [Test]
        public void TestToJson()
        {
            var json = new BatchStatusResponse {BatchId = "batch1", Status = BatchStatusResponse.StatusEnum.Committed}
                .ToJson();
            Assert.AreEqual("{\"batchId\":\"batch1\",\"status\":\"Committed\"}", json);
        }
    }
}