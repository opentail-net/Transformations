namespace Transformations.Tests
{
    using NUnit.Framework;

    [TestFixture]
    public class SemanticStringComparerTests
    {
        [TestCase("+1 (555) 111-2222", "15551112222", true)]
        [TestCase("555-111-2222", "5551113333", false)]
        [TestCase("abc", "def", false)]
        public void IsSemanticMatch_PhoneNumber_NormalizesDigits(string left, string right, bool expected)
        {
            bool actual = left.IsSemanticMatch(right, SemanticType.PhoneNumber);

            Assert.That(actual, Is.EqualTo(expected));
        }

        [TestCase(" User@Example.com ", "user@example.com", true)]
        [TestCase("a@b.com", "c@b.com", false)]
        public void IsSemanticMatch_Email_TrimAndIgnoreCase(string left, string right, bool expected)
        {
            bool actual = left.IsSemanticMatch(right, SemanticType.Email);

            Assert.That(actual, Is.EqualTo(expected));
        }

        [TestCase("AB-123", "ab 123", true)]
        [TestCase("AB-123", "ab 124", false)]
        public void IsSemanticMatch_AlphaNumericOnly_StripsNonAlphaNumeric(string left, string right, bool expected)
        {
            bool actual = left.IsSemanticMatch(right, SemanticType.AlphaNumericOnly);

            Assert.That(actual, Is.EqualTo(expected));
        }

        [TestCase("C:/temp/data/", "C:\\temp\\data", true)]
        [TestCase("/var/log/app/", "\\var\\log\\app", true)]
        [TestCase("/var/log/a", "/var/log/b", false)]
        public void IsSemanticMatch_NormalizedPath_NormalizesSeparatorsAndTrailingSlashes(string left, string right, bool expected)
        {
            bool actual = left.IsSemanticMatch(right, SemanticType.NormalizedPath);

            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void IsSemanticMatch_NullSafety_ReturnsFalseWhenEitherSideNull()
        {
            string left = null!;
            string right = "value";

            Assert.That(left.IsSemanticMatch(right, SemanticType.Email), Is.False);
            Assert.That(right.IsSemanticMatch(null!, SemanticType.Email), Is.False);
        }
    }
}
