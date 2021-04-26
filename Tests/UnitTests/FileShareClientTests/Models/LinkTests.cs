using NUnit.Framework;
using UKHO.FileShareClient.Models;

namespace UKHO.FileShareClientTests.Models
{
    public class LinkTests
    {
        [Test]
        public void TestEquals()
        {
            var emptyLink = new Link();
            var link1 = new Link("Link1");
            var link1B = new Link("Link1");
            var link2 = new Link("Link2");

            Assert.IsTrue(emptyLink.Equals(emptyLink));
            Assert.IsFalse(emptyLink.Equals(link1));
            Assert.IsFalse(emptyLink.Equals(link2));

            Assert.IsTrue(link1.Equals(link1B));
            Assert.IsFalse(link1.Equals(link2));
        }

        [Test]
        public void TestGetHashcode()
        {
            var emptyLink = new Link();
            var link1 = new Link("Link1");
            var link1B = new Link("Link1");

            Assert.AreEqual(emptyLink.GetHashCode(), emptyLink.GetHashCode());
            Assert.AreEqual(link1.GetHashCode(), link1.GetHashCode());
            Assert.AreEqual(link1.GetHashCode(), link1B.GetHashCode());
        }
    }
}