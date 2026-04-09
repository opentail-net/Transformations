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
        public void ConvertToNullablesbyte_InvalidStringInput_ReturnsDefaultValue()
        {
            //// Setup
            string valueInput = "invalid input";
            sbyte? expected = null;

            //// Act
            sbyte? actual = valueInput.ConvertTo<sbyte>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullablesbyte_InvalidStringNullInput_ReturnsNullValue()
        {
            //// Setup
            string? valueInput = null;
            sbyte? expected = null;

            //// Act
            sbyte? actual = valueInput.ConvertTo<sbyte>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullablesbyte_ValidBoolInput_ReturnsValueAsCorrectType()
        {
            //// Setupsbyte
            bool valueInput = false;
            sbyte? expected = 0;

            //// Act
            sbyte? actual = valueInput.ConvertTo<sbyte>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullablesbyte_ValidBoolNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            bool? valueInput = false;
            sbyte? expected = 0;

            //// Act
            sbyte? actual = valueInput.ConvertTo<sbyte>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullablesbyte_ValidByteInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            byte valueInput = 0;
            sbyte? expected = 0;

            //// Act
            sbyte? actual = valueInput.ConvertTo<sbyte>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullablesbyte_ValidByteNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            byte? valueInput = 0;
            sbyte? expected = 0;

            //// Act
            sbyte? actual = valueInput.ConvertTo<sbyte>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertCharCodeToNullablesbyte_ValidCharInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            char valueInput = 'f';
            //// Byte representaion of the char
            sbyte? expected = 102;

            //// Act
            sbyte? actual = valueInput.ConvertAsCharCodeTo<sbyte>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToCharCodeNullablesbyte_ValidCharNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            char? valueInput = 'f';
            //// Byte representaion of the char
            sbyte? expected = 102;

            //// Act
            sbyte? actual = valueInput.ConvertAsCharCodeTo<sbyte>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullablesbyte_ValidDecimalInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            decimal valueInput = 0;
            sbyte? expected = 0;

            //// Act
            sbyte? actual = valueInput.ConvertTo<sbyte>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullablesbyte_ValidDecimalNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            decimal? valueInput = 0;
            sbyte? expected = 0;

            //// Act
            sbyte? actual = valueInput.ConvertTo<sbyte>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullablesbyte_ValidDoubleInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            double valueInput = 0;
            sbyte? expected = 0;

            //// Act
            sbyte? actual = valueInput.ConvertTo<sbyte>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullablesbyte_ValidDoubleNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            double? valueInput = 0;
            sbyte? expected = 0;

            //// Act
            sbyte? actual = valueInput.ConvertTo<sbyte>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullablesbyte_ValidFloatInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            float valueInput = 0;
            sbyte? expected = 0;

            //// Act
            sbyte? actual = valueInput.ConvertTo<sbyte>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullablesbyte_ValidFloatNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            float? valueInput = 0;
            sbyte? expected = 0;

            //// Act
            sbyte? actual = valueInput.ConvertTo<sbyte>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullablesbyte_ValidIntInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            int valueInput = 0;
            sbyte? expected = 0;

            //// Act
            sbyte? actual = valueInput.ConvertTo<sbyte>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullablesbyte_ValidIntNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            int? valueInput = 0;
            sbyte? expected = 0;

            //// Act
            sbyte? actual = valueInput.ConvertTo<sbyte>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullablesbyte_ValidLongInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            long valueInput = 0;
            sbyte? expected = 0;

            //// Act
            sbyte? actual = valueInput.ConvertTo<sbyte>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullablesbyte_ValidLongNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            long? valueInput = 0;
            sbyte? expected = 0;

            //// Act
            sbyte? actual = valueInput.ConvertTo<sbyte>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullablesbyte_ValidSbyteInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            sbyte valueInput = 0;
            sbyte? expected = 0;

            //// Act
            sbyte? actual = valueInput.ConvertTo<sbyte>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullablesbyte_ValidSbyteNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            sbyte? valueInput = 0;
            sbyte? expected = 0;

            //// Act
            sbyte? actual = valueInput.ConvertTo<sbyte>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullablesbyte_ValidShortInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            short valueInput = 0;
            sbyte? expected = 0;

            //// Act
            sbyte? actual = valueInput.ConvertTo<sbyte>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullablesbyte_ValidShortNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            short? valueInput = 0;
            sbyte? expected = 0;

            //// Act
            sbyte? actual = valueInput.ConvertTo<sbyte>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullablesbyte_ValidStringInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            string valueInput = "false";
            sbyte? expected = 0;

            //// Act
            sbyte? actual = valueInput.ConvertTo<sbyte>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullablesbyte_ValidUintInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            uint valueInput = 0;
            sbyte? expected = 0;

            //// Act
            sbyte? actual = valueInput.ConvertTo<sbyte>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullablesbyte_ValidUintNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            uint? valueInput = 0;
            sbyte? expected = 0;

            //// Act
            sbyte? actual = valueInput.ConvertTo<sbyte>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullablesbyte_ValidUlongInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            ulong valueInput = 0;
            sbyte? expected = 0;

            //// Act
            sbyte? actual = valueInput.ConvertTo<sbyte>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullablesbyte_ValidUlongNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            ulong? valueInput = 0;
            sbyte? expected = 0;

            //// Act
            sbyte? actual = valueInput.ConvertTo<sbyte>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullablesbyte_ValidUshortInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            ushort valueInput = 0;
            sbyte? expected = 0;

            //// Act
            sbyte? actual = valueInput.ConvertTo<sbyte>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullablesbyte_ValidUshortNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            ushort? valueInput = 0;
            sbyte? expected = 0;

            //// Act
            sbyte? actual = valueInput.ConvertTo<sbyte>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        #endregion Methods
    }
}