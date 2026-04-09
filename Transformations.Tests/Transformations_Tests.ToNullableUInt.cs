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
        public void ConvertToNullableuint_InvalidStringInput_ReturnsDefaultValue()
        {
            //// Setup
            string valueInput = "invalid input";
            uint? expected = null;

            //// Act
            uint? actual = valueInput.ConvertTo<uint>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullableuint_InvalidStringNullInput_ReturnsNullValue()
        {
            //// Setup
            string? valueInput = null;
            uint? expected = null;

            //// Act
            uint? actual = valueInput.ConvertTo<uint>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullableuint_ValidBoolInput_ReturnsValueAsCorrectType()
        {
            //// Setupsbyte
            bool valueInput = false;
            uint? expected = 0;

            //// Act
            uint? actual = valueInput.ConvertTo<uint>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullableuint_ValidBoolNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            bool? valueInput = false;
            uint? expected = 0;

            //// Act
            uint? actual = valueInput.ConvertTo<uint>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullableuint_ValidByteInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            byte valueInput = 0;
            uint? expected = 0;

            //// Act
            uint? actual = valueInput.ConvertTo<uint>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullableuint_ValidByteNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            byte? valueInput = 0;
            uint? expected = 0;

            //// Act
            uint? actual = valueInput.ConvertTo<uint>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullableuint_ValidCharInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            char valueInput = 'f';
            //// Byte representaion of the char
            uint? expected = 102;

            //// Act
            uint? actual = valueInput.ConvertAsCharCodeTo<uint>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullableuint_ValidCharNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            char? valueInput = 'f';
            //// Byte representaion of the char
            uint? expected = 102;

            //// Act
            uint? actual = valueInput.ConvertAsCharCodeTo<uint>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullableuint_ValidDecimalInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            decimal valueInput = 0;
            uint? expected = 0;

            //// Act
            uint? actual = valueInput.ConvertTo<uint>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullableuint_ValidDecimalNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            decimal? valueInput = 0;
            uint? expected = 0;

            //// Act
            uint? actual = valueInput.ConvertTo<uint>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullableuint_ValidDoubleInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            double valueInput = 0;
            uint? expected = 0;

            //// Act
            uint? actual = valueInput.ConvertTo<uint>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullableuint_ValidDoubleNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            double? valueInput = 0;
            uint? expected = 0;

            //// Act
            uint? actual = valueInput.ConvertTo<uint>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullableuint_ValidFloatInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            float valueInput = 0;
            uint? expected = 0;

            //// Act
            uint? actual = valueInput.ConvertTo<uint>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullableuint_ValidFloatNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            float? valueInput = 0;
            uint? expected = 0;

            //// Act
            uint? actual = valueInput.ConvertTo<uint>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullableuint_ValidIntInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            int valueInput = 0;
            uint? expected = 0;

            //// Act
            uint? actual = valueInput.ConvertTo<uint>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullableuint_ValidIntNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            int? valueInput = 0;
            uint? expected = 0;

            //// Act
            uint? actual = valueInput.ConvertTo<uint>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullableuint_ValidLongInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            long valueInput = 0;
            uint? expected = 0;

            //// Act
            uint? actual = valueInput.ConvertTo<uint>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullableuint_ValidLongNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            long? valueInput = 0;
            uint? expected = 0;

            //// Act
            uint? actual = valueInput.ConvertTo<uint>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullableuint_ValidSbyteInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            sbyte valueInput = 0;
            uint? expected = 0;

            //// Act
            uint? actual = valueInput.ConvertTo<uint>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullableuint_ValidSbyteNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            sbyte? valueInput = 0;
            uint? expected = 0;

            //// Act
            uint? actual = valueInput.ConvertTo<uint>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullableuint_ValidShortInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            short valueInput = 0;
            uint? expected = 0;

            //// Act
            uint? actual = valueInput.ConvertTo<uint>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullableuint_ValidShortNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            short? valueInput = 0;
            uint? expected = 0;

            //// Act
            uint? actual = valueInput.ConvertTo<uint>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullableuint_ValidStringInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            string valueInput = "false";
            uint? expected = 0;

            //// Act
            uint? actual = valueInput.ConvertTo<uint>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullableuint_ValidUintInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            uint valueInput = 0;
            uint? expected = 0;

            //// Act
            uint? actual = valueInput.ConvertTo<uint>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullableuint_ValidUintNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            uint? valueInput = 0;
            uint? expected = 0;

            //// Act
            uint? actual = valueInput.ConvertTo<uint>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullableuint_ValidUlongInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            ulong valueInput = 0;
            uint? expected = 0;

            //// Act
            uint? actual = valueInput.ConvertTo<uint>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullableuint_ValidUlongNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            ulong? valueInput = 0;
            uint? expected = 0;

            //// Act
            uint? actual = valueInput.ConvertTo<uint>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullableuint_ValidUshortInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            ushort valueInput = 0;
            uint? expected = 0;

            //// Act
            uint? actual = valueInput.ConvertTo<uint>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullableuint_ValidUshortNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            ushort? valueInput = 0;
            uint? expected = 0;

            //// Act
            uint? actual = valueInput.ConvertTo<uint>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        #endregion Methods
    }
}