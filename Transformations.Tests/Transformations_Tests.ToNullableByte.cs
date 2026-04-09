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
        #region Methods

        [Test]
        public void ConvertToNullablebyte_InvalidStringInput_ReturnsDefaultValue()
        {
            //// Setup
            string valueInput = "invalid input";
            byte? expected = null;

            //// Act
            byte? actual = valueInput.ConvertTo<byte>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullablebyte_InvalidStringNullInput_ReturnsNullValue()
        {
            //// Setup
            string? valueInput = null;
            byte? expected = null;

            //// Act
            byte? actual = valueInput.ConvertTo<byte>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullablebyte_ValidBoolInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            bool valueInput = false;
            byte? expected = 0;

            //// Act
            byte? actual = valueInput.ConvertTo<byte>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullablebyte_ValidBoolNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            bool? valueInput = false;
            byte? expected = 0;

            //// Act
            byte? actual = valueInput.ConvertTo<byte>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullablebyte_ValidByteInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            byte valueInput = 0;
            byte? expected = 0;

            //// Act
            byte? actual = valueInput.ConvertTo<byte>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullablebyte_ValidByteNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            byte? valueInput = 0;
            byte? expected = 0;

            //// Act
            byte? actual = valueInput.ConvertTo<byte>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertCharCodeToNullablebyte_ValidCharInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            char valueInput = 'f';
            //// Byte representaion of the char
            byte? expected = 102;

            //// Act
            byte? actual = valueInput.ConvertAsCharCodeTo<byte>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullablebyte_ValidCharNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            char? valueInput = 'f';
            //// Byte representaion of the char
            byte? expected = 102;

            //// Act
            byte? actual = valueInput.ConvertAsCharCodeTo<byte>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullablebyte_ValidDecimalInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            decimal valueInput = 0;
            byte? expected = 0;

            //// Act
            byte? actual = valueInput.ConvertTo<byte>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullablebyte_ValidDecimalNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            decimal? valueInput = 0;
            byte? expected = 0;

            //// Act
            byte? actual = valueInput.ConvertTo<byte>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullablebyte_ValidDoubleInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            double valueInput = 0;
            byte? expected = 0;

            //// Act
            byte? actual = valueInput.ConvertTo<byte>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullablebyte_ValidDoubleNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            double? valueInput = 0;
            byte? expected = 0;

            //// Act
            byte? actual = valueInput.ConvertTo<byte>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullablebyte_ValidFloatInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            float valueInput = 0;
            byte? expected = 0;

            //// Act
            byte? actual = valueInput.ConvertTo<byte>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullablebyte_ValidFloatNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            float? valueInput = 0;
            byte? expected = 0;

            //// Act
            byte? actual = valueInput.ConvertTo<byte>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullablebyte_ValidIntInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            int valueInput = 0;
            byte? expected = 0;

            //// Act
            byte? actual = valueInput.ConvertTo<byte>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullablebyte_ValidIntNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            int? valueInput = 0;
            byte? expected = 0;

            //// Act
            byte? actual = valueInput.ConvertTo<byte>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullablebyte_ValidLongInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            long valueInput = 0;
            byte? expected = 0;

            //// Act
            byte? actual = valueInput.ConvertTo<byte>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullablebyte_ValidLongNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            long? valueInput = 0;
            byte? expected = 0;

            //// Act
            byte? actual = valueInput.ConvertTo<byte>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullablebyte_ValidSbyteInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            sbyte valueInput = 0;
            byte? expected = 0;

            //// Act
            byte? actual = valueInput.ConvertTo<byte>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullablebyte_ValidSbyteNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            sbyte? valueInput = 0;
            byte? expected = 0;

            //// Act
            byte? actual = valueInput.ConvertTo<byte>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullablebyte_ValidShortInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            short valueInput = 0;
            byte? expected = 0;

            //// Act
            byte? actual = valueInput.ConvertTo<byte>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullablebyte_ValidShortNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            short? valueInput = 0;
            byte? expected = 0;

            //// Act
            byte? actual = valueInput.ConvertTo<byte>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullablebyte_ValidStringInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            string valueInput = "false";
            byte? expected = 0;

            //// Act
            byte? actual = valueInput.ConvertTo<byte>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullablebyte_ValidUintInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            uint valueInput = 0;
            byte? expected = 0;

            //// Act
            byte? actual = valueInput.ConvertTo<byte>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullablebyte_ValidUintNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            uint? valueInput = 0;
            byte? expected = 0;

            //// Act
            byte? actual = valueInput.ConvertTo<byte>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullablebyte_ValidUlongInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            ulong valueInput = 0;
            byte? expected = 0;

            //// Act
            byte? actual = valueInput.ConvertTo<byte>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullablebyte_ValidUlongNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            ulong? valueInput = 0;
            byte? expected = 0;

            //// Act
            byte? actual = valueInput.ConvertTo<byte>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullablebyte_ValidUshortInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            ushort valueInput = 0;
            byte? expected = 0;

            //// Act
            byte? actual = valueInput.ConvertTo<byte>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullablebyte_ValidUshortNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            ushort? valueInput = 0;
            byte? expected = 0;

            //// Act
            byte? actual = valueInput.ConvertTo<byte>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        #endregion Methods
    }
}