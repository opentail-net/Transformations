namespace Transformations.Tests
{
    using System.Runtime.InteropServices;

    using NUnit.Framework;

    [TestFixture]
    public class WebAndFileExtensionsTests
    {
        [TestCase("https://api.example.com", "v1", "users", "https://api.example.com/v1/users")]
        [TestCase("https://api.example.com/", "/v1/", "/users/", "https://api.example.com/v1/users")]
        [TestCase("https://api.example.com", "", "users", "https://api.example.com/users")]
        public void AppendUrlSegment_NormalizesSlashes(string baseUri, string first, string second, string expected)
        {
            string actual = baseUri.AppendUrlSegment(first, second);

            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void AppendUrlSegment_NullOrEmptyBase_ReturnsBaseSafely()
        {
            string nullBase = null!;
            string emptyBase = string.Empty;

            Assert.That(nullBase.AppendUrlSegment("a"), Is.Null);
            Assert.That(emptyBase.AppendUrlSegment("a"), Is.EqualTo(string.Empty));
        }

        [Test]
        public void ToLocalPath_ConvertsSeparatorsForCurrentOs()
        {
            string mixed = "folder/sub\\file.txt";
            string actual = mixed.ToLocalPath();

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Assert.That(actual, Does.Not.Contain('/'));
                Assert.That(actual, Does.Contain("\\"));
            }
            else
            {
                Assert.That(actual, Does.Not.Contain('\\'));
                Assert.That(actual, Does.Contain('/'));
            }
        }

        [Test]
        public void ToLocalPath_NullOrEmpty_ReturnsInputSafely()
        {
            string nullPath = null!;
            string emptyPath = string.Empty;

            Assert.That(nullPath.ToLocalPath(), Is.Null);
            Assert.That(emptyPath.ToLocalPath(), Is.EqualTo(string.Empty));
        }
    }
}
