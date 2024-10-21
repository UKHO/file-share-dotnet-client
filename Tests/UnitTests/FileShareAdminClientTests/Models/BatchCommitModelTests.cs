using System.Linq;
using Newtonsoft.Json;
using NUnit.Framework;
using UKHO.FileShareAdminClient.Models;

namespace FileShareAdminClientTests.Models
{
    internal class BatchCommitModelTests
    {
        [Test]
        public void TestSerialiseAndDeserialiseBatchCommitModel()
        {
            var model = new BatchCommitModel();
            model.FileDetails.Add(new FileDetail
            {
                FileName = "File1.bin",
                Hash = "cLyPS3KoaSFGi/joRB3OUQ=="
            });

            var json = JsonConvert.SerializeObject(model);
            var deserialisedModel = JsonConvert.DeserializeObject<BatchCommitModel>(json);
            Assert.That(deserialisedModel, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(deserialisedModel.FileDetails, Has.Count.EqualTo(model.FileDetails.Count));
                Assert.That(deserialisedModel.FileDetails.Single().FileName, Is.EqualTo(model.FileDetails.Single().FileName));
                Assert.That(deserialisedModel.FileDetails.Single().Hash, Is.EqualTo(model.FileDetails.Single().Hash));
            });
        }
    }
}
