using System.Diagnostics.CodeAnalysis;
using NUnit.Framework;
using UKHO.FileShareClient.Models;

namespace FileShareClientTests.Models
{
    public class LinkTests
    {
        [Test]
        [SuppressMessage("Assertion", "NUnit2010:Use EqualConstraint for better assertion messages in case of failure", Justification = "Test overridden Equals method")]
        public void TestEquals()
        {
            var emptyLink = new Link();
            var link1 = new Link("Link1");
            var link1B = new Link("Link1");
            var link2 = new Link("Link2");

            Assert.Multiple(() =>
            {
                Assert.That(emptyLink.Equals(emptyLink), Is.True);
                Assert.That(emptyLink.Equals(link1), Is.False);
                Assert.That(emptyLink.Equals(link2), Is.False);

                Assert.That(link1.Equals(link1B), Is.True);
                Assert.That(link1.Equals(link2), Is.False);
            });
        }

        [Test]
        [SuppressMessage("Assertion", "NUnit2009:The same value has been provided as both the actual and the expected argument", Justification = "Test overridden GetHashCode method")]
        public void TestGetHashCode()
        {
            var emptyLink = new Link();
            var link1 = new Link("Link1");
            var link1B = new Link("Link1");

            Assert.Multiple(() =>
            {
                Assert.That(emptyLink.GetHashCode(), Is.Not.Zero);
                Assert.That(link1.GetHashCode(), Is.Not.Zero);
                Assert.That(link1B.GetHashCode(), Is.Not.Zero);
            });
            Assert.Multiple(() =>
            {
                Assert.That(emptyLink.GetHashCode(), Is.EqualTo(emptyLink.GetHashCode()));
                Assert.That(link1.GetHashCode(), Is.EqualTo(link1.GetHashCode()));
            });
            Assert.That(link1.GetHashCode(), Is.EqualTo(link1B.GetHashCode()));
        }
    }
}
