namespace Transformations.Tests
{
    using System;
    using System.Globalization;

    using NUnit.Framework;

    [TestFixture]
    public class InspectExtendedTests
    {
        #region IsLetterOrDigit

        [Test]
        public void IsLetterOrDigit_Letter_ReturnsTrue()
        {
            Assert.That('A'.IsLetterOrDigit(), Is.True);
        }

        [Test]
        public void IsLetterOrDigit_Digit_ReturnsTrue()
        {
            Assert.That('5'.IsLetterOrDigit(), Is.True);
        }

        [Test]
        public void IsLetterOrDigit_Symbol_ReturnsFalse()
        {
            Assert.That('#'.IsLetterOrDigit(), Is.False);
        }

        #endregion IsLetterOrDigit

        #region IsNumber

        [Test]
        public void IsNumber_ValidDigitChar_ReturnsTrue()
        {
            Assert.That('7'.IsNumber(), Is.True);
        }

        [Test]
        public void IsNumber_LetterChar_ReturnsFalse()
        {
            Assert.That('A'.IsNumber(), Is.False);
        }

        #endregion IsNumber

        #region IsSymbol

        [Test]
        public void IsSymbol_DollarSign_ReturnsTrue()
        {
            Assert.That('$'.IsSymbol(), Is.True);
        }

        [Test]
        public void IsSymbol_Letter_ReturnsFalse()
        {
            Assert.That('A'.IsSymbol(), Is.False);
        }

        #endregion IsSymbol

        #region InObjects

        [Test]
        public void InObjects_MatchingValue_ReturnsTrue()
        {
            //// Setup
            string input = "hello";

            //// Act
            bool actual = input.InObjects("world", "hello", 42);

            //// Assert
            Assert.That(actual, Is.True);
        }

        [Test]
        public void InObjects_NoMatch_ReturnsFalse()
        {
            //// Setup
            string input = "hello";

            //// Act
            bool actual = input.InObjects("world", "foo", 42);

            //// Assert
            Assert.That(actual, Is.False);
        }

        [Test]
        public void InObjects_NullInput_ReturnsFalse()
        {
            //// Act
            bool actual = ((string)null!).InObjects("a", "b");

            //// Assert
            Assert.That(actual, Is.False);
        }

        [Test]
        public void InObjects_WithStringComparison_CaseSensitive_ReturnsFalse()
        {
            //// Setup
            string input = "Hello";

            //// Act
            bool actual = input.InObjects(StringComparison.Ordinal, "hello", "world");

            //// Assert
            Assert.That(actual, Is.False);
        }

        [Test]
        public void InObjects_WithStringComparison_IgnoreCase_ReturnsTrue()
        {
            //// Setup
            string input = "Hello";

            //// Act
            bool actual = input.InObjects(StringComparison.OrdinalIgnoreCase, "hello", "world");

            //// Assert
            Assert.That(actual, Is.True);
        }

        #endregion InObjects
    }
}
