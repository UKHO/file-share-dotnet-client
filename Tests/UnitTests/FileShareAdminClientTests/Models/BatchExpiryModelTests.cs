using Newtonsoft.Json;
using UKHO.FileShareAdminClient.Models;

namespace FileShareAdminClientTests.Models
{
    internal class BatchExpiryModelTests
    {
        [Test]
        public void TestSerialiseAndDeserialiseBatchExpiryModel()
        {
            var model = new BatchExpiryModel { ExpiryDate = DateTime.UtcNow.AddDays(10) };

            var json = JsonConvert.SerializeObject(model);
            var deserialisedModel = JsonConvert.DeserializeObject<BatchExpiryModel>(json);

            Assert.That(deserialisedModel?.ExpiryDate, Is.EqualTo(model.ExpiryDate));
        }
    }
}
