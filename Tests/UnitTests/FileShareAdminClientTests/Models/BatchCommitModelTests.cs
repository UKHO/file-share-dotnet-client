using System.Linq;
using Newtonsoft.Json;
using NUnit.Framework;
using UKHO.FileShareAdminClient.Models;

namespace UKHO.FileShareAdminClientTests.Models
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
            Assert.AreEqual(model.FileDetails.Count, deserialisedModel.FileDetails.Count);
            Assert.AreEqual(model.FileDetails.Single().FileName, deserialisedModel.FileDetails.Single().FileName);
            Assert.AreEqual(model.FileDetails.Single().Hash, deserialisedModel.FileDetails.Single().Hash);
        }
    }
}