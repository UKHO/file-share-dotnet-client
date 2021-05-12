using NUnit.Framework;
using UKHO.FileShareClient.Models;

namespace UKHO.FileShareClientTests.Models
{
    public class LinksTests
    {
        [Test]
        public void TestEquals()
        {
            var emptyLinks = new Links();
            var link1 = new Links(new Link("selfLink"), new Link("firstLink"), new Link("previousLink"),
                new Link("nextLink"), new Link("lastLink"));
            var link1B = new Links(new Link("selfLink"), new Link("firstLink"), new Link("previousLink"),
                new Link("nextLink"), new Link("lastLink"));
            var link1C = new Links(link1.Self, link1.First, link1.Previous, link1.Next, link1.Last);
            var link2 = new Links(new Link("differentSelfLink"), new Link("firstLink"), new Link("previousLink"),
                new Link("nextLink"), new Link("lastLink"));
            var link3 = new Links(new Link("selfLink"), new Link("differentFirstLink"), new Link("previousLink"),
                new Link("nextLink"), new Link("lastLink"));
            var link4 = new Links(new Link("selfLink"), new Link("firstLink"), new Link("differentPreviousLink"),
                new Link("nextLink"), new Link("lastLink"));
            var link5 = new Links(new Link("selfLink"), new Link("firstLink"), new Link("previousLink"),
                new Link("differentNextLink"), new Link("lastLink"));
            var link6 = new Links(new Link("selfLink"), new Link("firstLink"), new Link("previousLink"),
                new Link("nextLink"), new Link("differentLastLink"));

            Assert.IsTrue(emptyLinks.Equals(emptyLinks));
            Assert.IsFalse(emptyLinks.Equals(link1));
            Assert.IsFalse(emptyLinks.Equals(link2));

            Assert.IsTrue(link1.Equals(link1B));
            Assert.IsTrue(link1.Equals(link1C));
            Assert.IsFalse(link1.Equals(link2));
            Assert.IsFalse(link1.Equals(link3));
            Assert.IsFalse(link1.Equals(link4));
            Assert.IsFalse(link1.Equals(link5));
            Assert.IsFalse(link1.Equals(link6));
        }

        [Test]
        public void TestGetHashcode()
        {
            var emptyLinks = new Links();
            var links1 = new Links(new Link("selfLink"), new Link("firstLink"), new Link("previousLink"),
                new Link("nextLink"), new Link("lastLink"));
            var links1B = new Links(new Link("selfLink"), new Link("firstLink"), new Link("previousLink"),
                new Link("nextLink"), new Link("lastLink"));

            Assert.AreEqual(emptyLinks.GetHashCode(), emptyLinks.GetHashCode());
            Assert.AreEqual(links1.GetHashCode(), links1.GetHashCode());
            Assert.AreEqual(links1.GetHashCode(), links1B.GetHashCode());
        }

        [Test]
        public void TestToJson()
        {
            var links1 = new Links(new Link("selfLink"), new Link("firstLink"), new Link("previousLink"),
                new Link("nextLink"), new Link("lastLink"));
            StringAssert.Contains($"\"self\":{links1.Self.ToJson()}", links1.ToJson());
            StringAssert.Contains($"\"first\":{links1.First.ToJson()}", links1.ToJson());
            StringAssert.Contains($"\"previous\":{links1.Previous.ToJson()}", links1.ToJson());
            StringAssert.Contains($"\"next\":{links1.Next.ToJson()}", links1.ToJson());
            StringAssert.Contains($"\"last\":{links1.Last.ToJson()}", links1.ToJson());
        }
    }
}