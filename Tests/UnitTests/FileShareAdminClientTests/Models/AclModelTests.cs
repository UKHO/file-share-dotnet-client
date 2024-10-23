using System.Collections.Generic;
using Newtonsoft.Json;
using NUnit.Framework;
using UKHO.FileShareAdminClient.Models;

namespace FileShareAdminClientTests.Models
{
    internal class AclModelTests
    {
        [Test]
        public void TestSerialiseAndDeserialiseAclModel()
        {
            var model = new Acl
            {
                ReadGroups = new List<string> { "ReplaceAclTest" },
                ReadUsers = new List<string> { "public" }
            };

            var json = JsonConvert.SerializeObject(model);
            var deserialisedModel = JsonConvert.DeserializeObject<Acl>(json);
            Assert.That(deserialisedModel, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(deserialisedModel.ReadGroups, Is.EqualTo(model.ReadGroups));
                Assert.That(deserialisedModel.ReadUsers, Is.EqualTo(model.ReadUsers));
            });
        }
    }
}
