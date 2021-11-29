using Newtonsoft.Json;
using NUnit.Framework;
using System.Collections.Generic;
using UKHO.FileShareAdminClient.Models;

namespace UKHO.FileShareAdminClientTests.Models
{
    class AclModelTests
    {
        [Test]
        public void TestSerialiseAndDeserialiseAclModel()
        {
            var model = new Acl {
                ReadGroups = new List<string> { "ReplaceAclTest" },
                ReadUsers = new List<string> { "public" }
            };

            var json = JsonConvert.SerializeObject(model);
            var deserialisedModel = JsonConvert.DeserializeObject<Acl>(json);

            Assert.AreEqual(model.ReadGroups, deserialisedModel.ReadGroups);
            Assert.AreEqual(model.ReadUsers, deserialisedModel.ReadUsers);
        }
    }
}