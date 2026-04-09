namespace Transformations.Tests
{
    using System;

    using NUnit.Framework;

    using Transformations;

    [TestFixture]
    public class InspectTests
    {
        #region In (string)

        [Test]
        public void In_MatchingValue_ReturnsTrue()
        {
            //// Setup
            string valueInput = "hello";
            const bool expected = true;

            //// Act
            bool actual = valueInput.In("world", "hello", "foo");

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void In_DifferentCaseMatchingValue_ReturnsTrue()
        {
            //// Setup
            string valueInput = "hello";
            const bool expected = true;

            //// Act
            bool actual = valueInput.In("HELLO");

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void In_NoMatchingValue_ReturnsFalse()
        {
            //// Setup
            string valueInput = "hello";
            const bool expected = false;

            //// Act
            bool actual = valueInput.In("world", "foo", "bar");

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void In_NullInput_ReturnsFalse()
        {
            //// Setup
            string valueInput = null!;
            const bool expected = false;

            //// Act
            bool actual = valueInput.In("hello");

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void In_EmptyInput_ReturnsFalse()
        {
            //// Setup
            string valueInput = string.Empty;
            const bool expected = false;

            //// Act
            bool actual = valueInput.In("hello");

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        #endregion In (string)

        #region In (struct)

        [Test]
        public void In_IntMatchingValue_ReturnsTrue()
        {
            //// Setup
            int valueInput = 3;
            const bool expected = true;

            //// Act
            bool actual = Inspect.In(valueInput, 1, 2, 3, 4, 5);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void In_IntNoMatchingValue_ReturnsFalse()
        {
            //// Setup
            int valueInput = 99;
            const bool expected = false;

            //// Act
            bool actual = Inspect.In(valueInput, 1, 2, 3);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        #endregion In (struct)

        #region In (separated string)

        [Test]
        public void InCsv_MatchingValue_ReturnsTrue()
        {
            //// Setup
            string valueInput = "hello";
            const bool expected = true;

            //// Act
            bool actual = valueInput.In("world,hello,foo", ',');

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void InCsv_NoMatchingValue_ReturnsFalse()
        {
            //// Setup
            string valueInput = "hello";
            const bool expected = false;

            //// Act
            bool actual = valueInput.In("world,foo,bar", ',');

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void InCsv_NullInput_ReturnsFalse()
        {
            //// Setup
            string valueInput = null!;
            const bool expected = false;

            //// Act
            bool actual = valueInput.In("hello,world", ',');

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void InCsv_SingleValue_MatchesDirect()
        {
            //// Setup
            string valueInput = "hello";
            const bool expected = true;

            //// Act
            bool actual = valueInput.In("hello", ',');

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        #endregion In (separated string)

        #region Is (InspectedString)

        [Test]
        public void Is_IsDigits_ValidDigits_ReturnsTrue()
        {
            //// Setup
            string valueInput = "12345";
            const bool expected = true;

            //// Act
            bool actual = valueInput.Is(Inspect.InspectedString.IsDigits);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void Is_IsDigits_MixedInput_ReturnsFalse()
        {
            //// Setup
            string valueInput = "123abc";
            const bool expected = false;

            //// Act
            bool actual = valueInput.Is(Inspect.InspectedString.IsDigits);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void Is_IsNumeric_ValidDecimal_ReturnsTrue()
        {
            //// Setup
            string valueInput = "123.45";
            const bool expected = true;

            //// Act
            bool actual = valueInput.Is(Inspect.InspectedString.IsNumeric);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void Is_IsNumeric_InvalidInput_ReturnsFalse()
        {
            //// Setup
            string valueInput = "abc";
            const bool expected = false;

            //// Act
            bool actual = valueInput.Is(Inspect.InspectedString.IsNumeric);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void Is_IsDate_ValidDate_ReturnsTrue()
        {
            //// Setup
            string valueInput = "15/02/2014";
            const bool expected = true;

            //// Act
            bool actual = valueInput.Is(Inspect.InspectedString.IsDate);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void Is_IsDate_InvalidInput_ReturnsFalse()
        {
            //// Setup
            string valueInput = "not a date";
            const bool expected = false;

            //// Act
            bool actual = valueInput.Is(Inspect.InspectedString.IsDate);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void Is_IsDate_NullInput_ReturnsFalse()
        {
            //// Setup
            string valueInput = null!;
            const bool expected = false;

            //// Act
            bool actual = valueInput.Is(Inspect.InspectedString.IsDate);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void Is_IsGuid_ValidGuid_ReturnsTrue()
        {
            //// Setup
            string valueInput = "7F8C14B6-B3A8-4F71-8EFC-E5A7B35923B6";
            const bool expected = true;

            //// Act
            bool actual = valueInput.Is(Inspect.InspectedString.IsGuid);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void Is_IsGuid_InvalidInput_ReturnsFalse()
        {
            //// Setup
            string valueInput = "not a guid";
            const bool expected = false;

            //// Act
            bool actual = valueInput.Is(Inspect.InspectedString.IsGuid);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void Is_IsNullOrEmpty_NullInput_ReturnsTrue()
        {
            //// Setup
            string valueInput = null!;
            const bool expected = true;

            //// Act
            bool actual = valueInput.Is(Inspect.InspectedString.IsNullOrEmpty);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void Is_IsNullOrEmpty_EmptyInput_ReturnsTrue()
        {
            //// Setup
            string valueInput = string.Empty;
            const bool expected = true;

            //// Act
            bool actual = valueInput.Is(Inspect.InspectedString.IsNullOrEmpty);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void Is_IsNullOrEmpty_ValidInput_ReturnsFalse()
        {
            //// Setup
            string valueInput = "hello";
            const bool expected = false;

            //// Act
            bool actual = valueInput.Is(Inspect.InspectedString.IsNullOrEmpty);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void Is_IsUpperCase_AllUpper_ReturnsTrue()
        {
            //// Setup
            string valueInput = "HELLO";
            const bool expected = true;

            //// Act
            bool actual = valueInput.Is(Inspect.InspectedString.IsUpperCase);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void Is_IsUpperCase_MixedCase_ReturnsFalse()
        {
            //// Setup
            string valueInput = "Hello";
            const bool expected = false;

            //// Act
            bool actual = valueInput.Is(Inspect.InspectedString.IsUpperCase);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void Is_IsLowerCase_AllLower_ReturnsTrue()
        {
            //// Setup
            string valueInput = "hello";
            const bool expected = true;

            //// Act
            bool actual = valueInput.Is(Inspect.InspectedString.IsLowerCase);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void Is_IsLetters_AllLetters_ReturnsTrue()
        {
            //// Setup
            string valueInput = "hello";
            const bool expected = true;

            //// Act
            bool actual = valueInput.Is(Inspect.InspectedString.IsLetters);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void Is_IsLetters_MixedContent_ReturnsFalse()
        {
            //// Setup
            string valueInput = "hello123";
            const bool expected = false;

            //// Act
            bool actual = valueInput.Is(Inspect.InspectedString.IsLetters);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void Is_IsLettersOrNumbers_ValidInput_ReturnsTrue()
        {
            //// Setup
            string valueInput = "hello123";
            const bool expected = true;

            //// Act
            bool actual = valueInput.Is(Inspect.InspectedString.IsLettersOrNumbers);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        #endregion Is (InspectedString)

        #region Is (InspectedDate)

        [Test]
        public void Is_IsWeekend_Saturday_ReturnsTrue()
        {
            //// Setup
            DateTime valueInput = new DateTime(2024, 01, 06); // Saturday
            const bool expected = true;

            //// Act
            bool actual = valueInput.Is(Inspect.InspectedDate.IsWeekend);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void Is_IsWeekend_Monday_ReturnsFalse()
        {
            //// Setup
            DateTime valueInput = new DateTime(2024, 01, 08); // Monday
            const bool expected = false;

            //// Act
            bool actual = valueInput.Is(Inspect.InspectedDate.IsWeekend);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void Is_IsLeapYear_2024_ReturnsTrue()
        {
            //// Setup
            DateTime valueInput = new DateTime(2024, 01, 01);
            const bool expected = true;

            //// Act
            bool actual = valueInput.Is(Inspect.InspectedDate.IsLeapYear);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void Is_IsLeapYear_2023_ReturnsFalse()
        {
            //// Setup
            DateTime valueInput = new DateTime(2023, 01, 01);
            const bool expected = false;

            //// Act
            bool actual = valueInput.Is(Inspect.InspectedDate.IsLeapYear);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void Is_IsLastDayOfTheMonth_Jan31_ReturnsTrue()
        {
            //// Setup
            DateTime valueInput = new DateTime(2024, 01, 31);
            const bool expected = true;

            //// Act
            bool actual = valueInput.Is(Inspect.InspectedDate.IsLastDayOfTheMonth);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void Is_IsLastDayOfTheMonth_Jan15_ReturnsFalse()
        {
            //// Setup
            DateTime valueInput = new DateTime(2024, 01, 15);
            const bool expected = false;

            //// Act
            bool actual = valueInput.Is(Inspect.InspectedDate.IsLastDayOfTheMonth);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void Is_IsFirstDayOfTheMonth_Jan1_ReturnsTrue()
        {
            //// Setup
            DateTime valueInput = new DateTime(2024, 01, 01);
            const bool expected = true;

            //// Act
            bool actual = valueInput.Is(Inspect.InspectedDate.IsFirstDayOfTheMonth);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void Is_IsFirstDayOfTheMonth_Jan15_ReturnsFalse()
        {
            //// Setup
            DateTime valueInput = new DateTime(2024, 01, 15);
            const bool expected = false;

            //// Act
            bool actual = valueInput.Is(Inspect.InspectedDate.IsFirstDayOfTheMonth);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        #endregion Is (InspectedDate)

        #region WithDefault

        [Test]
        public void WithDefault_NoArgs_NullInput_ReturnsEmpty()
        {
            //// Setup
            string valueInput = null!;
            string expected = string.Empty;

            //// Act
            string actual = valueInput.WithDefault();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void WithDefault_NoArgs_ValidInput_ReturnsInput()
        {
            //// Setup
            string valueInput = "hello";
            string expected = "hello";

            //// Act
            string actual = valueInput.WithDefault();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void WithDefault_WithValue_NullInput_ReturnsDefault()
        {
            //// Setup
            string valueInput = null!;
            string expected = "fallback";

            //// Act
            string actual = valueInput!.WithDefault("fallback");

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void WithDefault_WithValue_EmptyInput_ReturnsDefault()
        {
            //// Setup
            string valueInput = string.Empty;
            string expected = "fallback";

            //// Act
            string actual = valueInput.WithDefault("fallback");

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void WithDefault_WithValue_ValidInput_ReturnsInput()
        {
            //// Setup
            string valueInput = "hello";
            string expected = "hello";

            //// Act
            string actual = valueInput.WithDefault("fallback");

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void WithDefault_IsNullOrEmptyOrWhitespace_WhitespaceInput_ReturnsDefault()
        {
            //// Setup
            string valueInput = "   ";
            string expected = "fallback";

            //// Act
            string actual = valueInput.WithDefault(Inspect.ApplyDefaultIf.IsNullOrEmptyOrWhitespace, "fallback");

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void WithDefault_IsNull_EmptyInput_ReturnsEmpty()
        {
            //// Setup
            string valueInput = string.Empty;
            string expected = string.Empty;

            //// Act
            string actual = valueInput.WithDefault(Inspect.ApplyDefaultIf.IsNull, "fallback");

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        #endregion WithDefault
    }
}
