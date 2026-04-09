namespace Transformations.Tests
{
    using System;

    using NUnit.Framework;

    using Transformations;

    [TestFixture]
    public class AdditionalStringHelperTests
    {
        #region EnsureEndsWith

        [Test]
        public void EnsureEndsWith_AlreadyEnds_ReturnsOriginal()
        {
            //// Setup
            string valueInput = "http://example.com/";
            string expected = "http://example.com/";

            //// Act
            string actual = valueInput.EnsureEndsWith("/");

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void EnsureEndsWith_DoesNotEnd_ReturnsSuffixed()
        {
            //// Setup
            string valueInput = "http://example.com";
            string expected = "http://example.com/";

            //// Act
            string actual = valueInput.EnsureEndsWith("/");

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        #endregion EnsureEndsWith

        #region EnsureStartsWith

        [Test]
        public void EnsureStartsWith_AlreadyStarts_ReturnsOriginal()
        {
            //// Setup
            string valueInput = ".txt";
            string expected = ".txt";

            //// Act
            string actual = valueInput.EnsureStartsWith(".");

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void EnsureStartsWith_DoesNotStart_ReturnsPrefixed()
        {
            //// Setup
            string valueInput = "txt";
            string expected = ".txt";

            //// Act
            string actual = valueInput.EnsureStartsWith(".");

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        #endregion EnsureStartsWith

        #region ExtractDigits

        [Test]
        public void ExtractDigits_MixedInput_ReturnsOnlyDigits()
        {
            //// Setup
            string valueInput = "abc123def456";
            string expected = "123456";

            //// Act
            string actual = valueInput.ExtractDigits();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ExtractDigits_NoDigits_ReturnsEmpty()
        {
            //// Setup
            string valueInput = "abcdef";
            string expected = string.Empty;

            //// Act
            string actual = valueInput.ExtractDigits();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ExtractDigits_EmptyInput_ReturnsEmpty()
        {
            //// Setup
            string valueInput = string.Empty;
            string expected = string.Empty;

            //// Act
            string actual = valueInput.ExtractDigits();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ExtractDigits_NullInput_ReturnsEmpty()
        {
            //// Setup
            string valueInput = null!;
            string expected = string.Empty;

            //// Act
            string actual = valueInput.ExtractDigits();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        #endregion ExtractDigits

        #region GetPart

        [Test]
        public void GetPart_After_ReturnsPartAfterPattern()
        {
            //// Setup
            string valueInput = "key=value";
            string expected = "value";

            //// Act
            string actual = valueInput.GetPart("=", Transform.Part.After);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void GetPart_Before_ReturnsPartBeforePattern()
        {
            //// Setup
            string valueInput = "key=value";
            string expected = "key";

            //// Act
            string actual = valueInput.GetPart("=", Transform.Part.Before);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void GetPart_PatternNotFound_ReturnsEmpty()
        {
            //// Setup
            string valueInput = "hello world";
            string expected = string.Empty;

            //// Act
            string actual = valueInput.GetPart("=", Transform.Part.After);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void GetPart_EmptyInput_ReturnsEmpty()
        {
            //// Setup
            string valueInput = string.Empty;
            string expected = string.Empty;

            //// Act
            string actual = valueInput.GetPart("=");

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void GetPart_Between_ReturnsBetweenPatterns()
        {
            //// Setup
            string valueInput = "[hello]";
            string expected = "hello";

            //// Act
            string actual = valueInput.GetPart("[", "]");

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void GetPart_Between_PatternNotFound_ReturnsEmpty()
        {
            //// Setup
            string valueInput = "hello world";
            string expected = string.Empty;

            //// Act
            string actual = valueInput.GetPart("[", "]");

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        #endregion GetPart

        #region Repeat

        [Test]
        public void Repeat_SingleChar_ReturnsRepeated()
        {
            //// Setup
            string valueInput = "x";
            string expected = "xxx";

            //// Act
            string actual = valueInput.Repeat(3);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void Repeat_MultipleChars_ReturnsRepeated()
        {
            //// Setup
            string valueInput = "ab";
            string expected = "ababab";

            //// Act
            string actual = valueInput.Repeat(3);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void Repeat_ZeroCount_ReturnsEmpty()
        {
            //// Setup
            string valueInput = "hello";
            string expected = string.Empty;

            //// Act
            string actual = valueInput.Repeat(0);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        #endregion Repeat

        #region Reverse

        [Test]
        public void Reverse_ValidInput_ReturnsReversed()
        {
            //// Setup
            string valueInput = "hello";
            string expected = "olleh";

            //// Act
            string actual = valueInput.Reverse();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void Reverse_EmptyInput_ReturnsEmpty()
        {
            //// Setup
            string valueInput = string.Empty;
            string expected = string.Empty;

            //// Act
            string actual = valueInput.Reverse();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void Reverse_NullInput_ReturnsNull()
        {
            //// Setup
            string valueInput = null!;
            string? expected = null;

            //// Act
            string actual = valueInput.Reverse();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void Reverse_SingleChar_ReturnsSame()
        {
            //// Setup
            string valueInput = "a";
            string expected = "a";

            //// Act
            string actual = valueInput.Reverse();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        #endregion Reverse

        #region StripHtml

        [Test]
        public void StripHtml_EmptyInput_ReturnsEmpty()
        {
            //// Setup
            string valueInput = string.Empty;
            string expected = string.Empty;

            //// Act
            string actual = valueInput.StripHtml();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void StripHtml_NullInput_ReturnsNull()
        {
            //// Setup
            string valueInput = null!;
            string? expected = null;

            //// Act
            string actual = valueInput.StripHtml();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void StripHtml_NoHtml_ReturnsOriginal()
        {
            //// Setup
            string valueInput = "Hello World";
            string expected = "Hello World";

            //// Act
            string actual = valueInput.StripHtml();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        #endregion StripHtml

        #region ToBase64String / FromBase64String

        [Test]
        public void ToBase64String_ValidInput_ReturnsBase64()
        {
            //// Setup
            string valueInput = "hello";

            //// Act
            string encoded = valueInput.ToBase64String();
            string decoded = encoded.FromBase64String();

            //// Assert
            Assert.That(decoded, Is.EqualTo(valueInput));
        }

        [Test]
        public void ToBase64String_EmptyInput_RoundTrips()
        {
            //// Setup
            string valueInput = string.Empty;

            //// Act
            string encoded = valueInput.ToBase64String();
            string decoded = encoded.FromBase64String();

            //// Assert
            Assert.That(decoded, Is.EqualTo(valueInput));
        }

        #endregion ToBase64String / FromBase64String
    }
}
