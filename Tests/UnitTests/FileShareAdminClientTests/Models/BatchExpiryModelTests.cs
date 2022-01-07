using System;
using System.Globalization;
using Newtonsoft.Json;
using NUnit.Framework;
using UKHO.FileShareAdminClient.Models;

namespace UKHO.FileShareAdminClientTests.Models
{
    internal class BatchExpiryModelTests
    {
        [Test]
        public void TestSerialiseAndDeserialiseBatchExpiryModel()
        {
            var model = new BatchExpiryModel { ExpiryDate = DateTime.UtcNow.AddDays(10) };

            var json = JsonConvert.SerializeObject(model);
            var deserialisedModel = JsonConvert.DeserializeObject<BatchExpiryModel>(json);
          
            Assert.AreEqual(model.ExpiryDate, deserialisedModel.ExpiryDate);
        }
    }
}
