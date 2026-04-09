namespace Transformations.Tests
{
    using System;

    using NUnit.Framework;

    [TestFixture]
    public class BasicTypeConverterCoverageTests
    {
        [TestCase(null, false, 0)]
        [TestCase("", false, 0)]
        [TestCase("   ", false, 0)]
        [TestCase("42", true, 42)]
        [TestCase("-17", true, -17)]
        [TestCase("abc", false, 0)]
        public void TryConvertTo_Int_FromString_CoversNullAndInvalidBranches(string? input, bool expectedSuccess, int expectedValue)
        {
            bool success = input.TryConvertTo<int>(out int result);

            Assert.That(success, Is.EqualTo(expectedSuccess));
            Assert.That(result, Is.EqualTo(expectedValue));
        }

        [TestCase("yes", true, 1)]
        [TestCase("y", true, 1)]
        [TestCase("true", true, 1)]
        [TestCase("no", true, 0)]
        [TestCase("n", true, 0)]
        [TestCase("false", true, 0)]
        [TestCase("garbage", false, -9)]
        [TestCase(null, false, -9)]
        public void TryConvertTo_Int_WithDefault_CoversBooleanAliasFallback(string? input, bool expectedSuccess, int expectedValue)
        {
            bool success = input.TryConvertTo<int>(-9, out int? result);

            Assert.That(success, Is.EqualTo(expectedSuccess));
            Assert.That(result, Is.EqualTo(expectedValue));
        }

        [Test]
        public void TryConvertTo_Guid_InvalidValue_ReturnsFalseAndDefault()
        {
            bool success = "not-a-guid".TryConvertTo<Guid>(out Guid result);

            Assert.That(success, Is.False);
            Assert.That(result, Is.EqualTo(Guid.Empty));
        }

        [Test]
        public void TryConvertTo_Guid_ValidValue_ReturnsTrue()
        {
            string input = "7f5f9f8a-9ebf-4a0d-9bbd-0a6be6f8fd77";

            bool success = input.TryConvertTo<Guid>(out Guid result);

            Assert.That(success, Is.True);
            Assert.That(result, Is.EqualTo(Guid.Parse(input)));
        }

        [TestCase(null, 0)]
        [TestCase("", 0)]
        [TestCase("abc", 0)]
        [TestCase("123", 123)]
        public void ConvertTo_Int_FromString_UsesDefaultOnInvalid(string? input, int expected)
        {
            int actual = input.ConvertTo<int>();

            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertTo_GuidToNonString_ThrowsArgumentException()
        {
            Guid value = Guid.NewGuid();

            Assert.Throws<ArgumentException>(() => value.ConvertTo<int>());
        }

        [Test]
        public void ConvertTo_NullableGuidToNonString_ThrowsArgumentException()
        {
            Guid? value = Guid.NewGuid();

            Assert.Throws<ArgumentException>(() => value.ConvertTo<int>());
        }

        [Test]
        public void ConvertTo_DirtyDBNullString_UsesDefaultBranch()
        {
            string input = DBNull.Value.ToString();

            int actual = input.ConvertTo<int>();

            Assert.That(actual, Is.EqualTo(0));
        }
    }
}
