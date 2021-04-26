using System;
using System.Collections.Generic;
using NUnit.Framework;
using UKHO.FileShareClient.Models;

namespace UKHO.FileShareClientTests.Models
{
    public class BatchDetailsTests
    {
        [Test]
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

            Assert.IsTrue(emptyBatchDetails.Equals(emptyBatchDetails));
            Assert.IsFalse(emptyBatchDetails.Equals(batchDetails1));
            Assert.IsFalse(emptyBatchDetails.Equals(link2));

            Assert.IsTrue(batchDetails1.Equals(batchDetails1B));
            Assert.IsFalse(batchDetails1.Equals(link2));
        }

        [Test]
        public void TestGetHashcode()
        {
            var emptyBatchDetails = new BatchDetails();
            var batchDetails1 = new BatchDetails
            {
                BatchId = Guid.NewGuid().ToString(),
                BusinessUnit = "BU1",
                Status = BatchDetails.StatusEnum.Incomplete,
                Attributes = new List<BatchDetailsAttributes>()
            };
            var batchDetails1B = new BatchDetails
            {
                BatchId = batchDetails1.BatchId,
                BusinessUnit = "BU1",
                Status = BatchDetails.StatusEnum.Incomplete,
                Attributes = new List<BatchDetailsAttributes>()
            };
            Assert.NotZero(emptyBatchDetails.GetHashCode());
            Assert.NotZero(batchDetails1.GetHashCode());

            Assert.AreEqual(emptyBatchDetails.GetHashCode(), emptyBatchDetails.GetHashCode());
            Assert.AreEqual(batchDetails1.GetHashCode(), batchDetails1.GetHashCode());
            Assert.AreEqual(batchDetails1.GetHashCode(), batchDetails1B.GetHashCode());
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
            Assert.AreEqual(
                $"{{\"batchId\":\"{batchDetails.BatchId}\",\"status\":\"Incomplete\",\"businessUnit\":\"BU1\"}}",
                batchDetails.ToJson());
        }
    }
}