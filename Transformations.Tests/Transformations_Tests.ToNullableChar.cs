namespace Transformations.Tests
{
    using System;
    using NUnit.Framework;
    using Transformations;

    /// <summary>
    /// The basic type converter tests.
    /// </summary>
    [TestFixture]
    public partial class BasicTypeConverterTests
    {
        [Test]
        public void ConvertToNullablechar_ValidBoolInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            bool valueInput = false;
            char? expected = '0';

            //// Act
            char? actual = valueInput.ConvertTo<char>();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullablechar_ValidBoolNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            bool? valueInput = false;
            char? expected = '0';

            //// Act
            char? actual = valueInput.ConvertTo<char>();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullablechar_ValidByteInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            byte valueInput = 0;
            char? expected = '\0';

            //// Act
            char? actual = valueInput.ConvertTo<char>();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullablechar_ValidByteNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            byte? valueInput = 0;
            char? expected = '\0';

            //// Act
            char? actual = valueInput.ConvertTo<char>();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullablechar_ValidCharInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            char valueInput = 'f';
            char? expected = 'f';

            //// Act
            char? actual = valueInput.ConvertTo<char>();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullablechar_ValidCharNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            char? valueInput = 'f';
            char? expected = 'f';

            //// Act
            char? actual = valueInput.ConvertTo<char>();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullablechar_ValidDecimalInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            decimal valueInput = 0;
            char? expected = '\0';

            //// Act
            char? actual = valueInput.ConvertTo<char>();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullablechar_ValidDecimalNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            decimal? valueInput = 0;
            char? expected = '\0';

            //// Act
            char? actual = valueInput.ConvertTo<char>();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullablechar_ValidDoubleInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            double valueInput = 0;
            char? expected = '\0';

            //// Act
            char? actual = valueInput.ConvertTo<char>();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullablechar_ValidDoubleNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            double? valueInput = 0;
            char? expected = '\0';

            //// Act
            char? actual = valueInput.ConvertTo<char>();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullablechar_ValidFloatInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            float valueInput = 0;
            char? expected = '\0';

            //// Act
            char? actual = valueInput.ConvertTo<char>();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullablechar_ValidFloatNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            float? valueInput = 0;
            char? expected = '\0';

            //// Act
            char? actual = valueInput.ConvertTo<char>();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullablechar_ValidIntInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            int valueInput = 0;
            char? expected = '\0';

            //// Act
            char? actual = valueInput.ConvertTo<char>();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullablechar_ValidIntNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            int? valueInput = 0;
            char? expected = '\0';

            //// Act
            char? actual = valueInput.ConvertTo<char>();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullablechar_ValidLongInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            long valueInput = 0;
            char? expected = '\0';

            //// Act
            char? actual = valueInput.ConvertTo<char>();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullablechar_ValidLongNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            long? valueInput = 0;
            char? expected = '\0';

            //// Act
            char? actual = valueInput.ConvertTo<char>();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullablechar_ValidSbyteInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            sbyte valueInput = 0;
            char? expected = '\0';

            //// Act
            char? actual = valueInput.ConvertTo<char>();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullablechar_ValidSbyteNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            sbyte? valueInput = 0;
            char? expected = '\0';

            //// Act
            char? actual = valueInput.ConvertTo<char>();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullablechar_ValidShortInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            short valueInput = 0;
            char? expected = '\0';

            //// Act
            char? actual = valueInput.ConvertTo<char>();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullablechar_ValidShortNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            short? valueInput = 0;
            char? expected = '\0';

            //// Act
            char? actual = valueInput.ConvertTo<char>();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullablechar_ValidStringInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            string valueInput = "false";
            char? expected = 'f';

            //// Act
            char? actual = valueInput.ConvertTo<char>();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullablechar_ValidUintInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            uint valueInput = 0;
            char? expected = '\0';

            //// Act
            char? actual = valueInput.ConvertTo<char>();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullablechar_ValidUintNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            uint? valueInput = 0;
            char? expected = '\0';

            //// Act
            char? actual = valueInput.ConvertTo<char>();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullablechar_ValidUlongInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            ulong valueInput = 0;
            char? expected = '\0';

            //// Act
            char? actual = valueInput.ConvertTo<char>();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullablechar_ValidUlongNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            ulong? valueInput = 0;
            char? expected = '\0';

            //// Act
            char? actual = valueInput.ConvertTo<char>();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullablechar_ValidUshortInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            ushort valueInput = 0;
            char? expected = '\0';

            //// Act
            char? actual = valueInput.ConvertTo<char>();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullablechar_ValidUshortNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            ushort? valueInput = 0;
            char? expected = '\0';

            //// Act
            char? actual = valueInput.ConvertTo<char>();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        /*
        [Test]
        public void ConvertToNullablechar_InvalidStringInput_ReturnsDefaultValue()
        {
            //// Setup
            string valueInput = "invalid input";
            char? expected = true;

            //// Act
            char? actual = valueInput.ConvertTo<char>();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }
        */

        [Test]
        public void ConvertToNullablechar_InvalidStringNullInput_ReturnNullValue()
        {
            //// Setup
            string? valueInput = null;
            char? expected = null;

            //// Act
            char? actual = valueInput.ConvertTo<char>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }
    }
}