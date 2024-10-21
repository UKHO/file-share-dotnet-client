using System.Diagnostics.CodeAnalysis;
using NUnit.Framework;
using UKHO.FileShareClient.Models;

namespace FileShareClientTests.Models
{
    public class LinksTests
    {
        [Test]
        [SuppressMessage("Assertion", "NUnit2010:Use EqualConstraint for better assertion messages in case of failure", Justification = "Test overridden Equals method")]
        public void TestEquals()
        {
            var emptyLinks = new Links();
            var link1 = new Links(new Link("selfLink"), new Link("firstLink"), new Link("previousLink"), new Link("nextLink"), new Link("lastLink"));
            var link1B = new Links(new Link("selfLink"), new Link("firstLink"), new Link("previousLink"), new Link("nextLink"), new Link("lastLink"));
            var link1C = new Links(link1.Self, link1.First, link1.Previous, link1.Next, link1.Last);
            var link2 = new Links(new Link("differentSelfLink"), new Link("firstLink"), new Link("previousLink"), new Link("nextLink"), new Link("lastLink"));
            var link3 = new Links(new Link("selfLink"), new Link("differentFirstLink"), new Link("previousLink"), new Link("nextLink"), new Link("lastLink"));
            var link4 = new Links(new Link("selfLink"), new Link("firstLink"), new Link("differentPreviousLink"), new Link("nextLink"), new Link("lastLink"));
            var link5 = new Links(new Link("selfLink"), new Link("firstLink"), new Link("previousLink"), new Link("differentNextLink"), new Link("lastLink"));
            var link6 = new Links(new Link("selfLink"), new Link("firstLink"), new Link("previousLink"), new Link("nextLink"), new Link("differentLastLink"));

            Assert.Multiple(() =>
            {
                Assert.That(emptyLinks.Equals(emptyLinks), Is.True);
                Assert.That(emptyLinks.Equals(link1), Is.False);
                Assert.That(emptyLinks.Equals(link2), Is.False);

                Assert.That(link1.Equals(link1B), Is.True);
                Assert.That(link1.Equals(link1C), Is.True);
                Assert.That(link1.Equals(link2), Is.False);
                Assert.That(link1.Equals(link3), Is.False);
                Assert.That(link1.Equals(link4), Is.False);
                Assert.That(link1.Equals(link5), Is.False);
                Assert.That(link1.Equals(link6), Is.False);
            });
        }

        [Test]
        [SuppressMessage("Assertion", "NUnit2009:The same value has been provided as both the actual and the expected argument", Justification = "Test overridden GetHashCode method")]
        public void TestGetHashCode()
        {
            var emptyLinks = new Links();
            var links1 = new Links(new Link("selfLink"), new Link("firstLink"), new Link("previousLink"), new Link("nextLink"), new Link("lastLink"));
            var links1B = new Links(new Link("selfLink"), new Link("firstLink"), new Link("previousLink"), new Link("nextLink"), new Link("lastLink"));

            Assert.Multiple(() =>
            {
                Assert.That(emptyLinks.GetHashCode(), Is.Not.Zero);
                Assert.That(links1.GetHashCode(), Is.Not.Zero);
                Assert.That(links1B.GetHashCode(), Is.Not.Zero);
            });
            Assert.Multiple(() =>
            {
                Assert.That(emptyLinks.GetHashCode(), Is.EqualTo(emptyLinks.GetHashCode()));
                Assert.That(links1.GetHashCode(), Is.EqualTo(links1.GetHashCode()));
            });
            Assert.That(links1.GetHashCode(), Is.EqualTo(links1B.GetHashCode()));
        }

        [Test]
        public void TestToJson()
        {
            var links1 = new Links(new Link("selfLink"), new Link("firstLink"), new Link("previousLink"), new Link("nextLink"), new Link("lastLink"));
            Assert.That(links1.ToJson(), Does.Contain($"\"self\":{links1.Self.ToJson()}"));
            Assert.That(links1.ToJson(), Does.Contain($"\"first\":{links1.First.ToJson()}"));
            Assert.That(links1.ToJson(), Does.Contain($"\"previous\":{links1.Previous.ToJson()}"));
            Assert.That(links1.ToJson(), Does.Contain($"\"next\":{links1.Next.ToJson()}"));
            Assert.That(links1.ToJson(), Does.Contain($"\"last\":{links1.Last.ToJson()}"));
        }
    }
}
