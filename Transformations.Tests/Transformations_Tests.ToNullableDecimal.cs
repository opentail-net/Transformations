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
        public void ConvertToNullabledecimal_InvalidStringInput_ReturnsDefaultValue()
        {
            //// Setup
            string valueInput = "invalid input";
            decimal? expected = null;

            //// Act
            decimal? actual = valueInput.ConvertTo<decimal>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullabledecimal_InvalidStringNullInput_ReturnsNullValue()
        {
            //// Setup
            string? valueInput = null;
            decimal? expected = null;

            //// Act
            decimal? actual = valueInput.ConvertTo<decimal>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullabledecimal_ValidBoolInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            bool valueInput = false;
            decimal? expected = 0;

            //// Act
            decimal? actual = valueInput.ConvertTo<decimal>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullabledecimal_ValidBoolNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            bool? valueInput = false;
            decimal? expected = 0;

            //// Act
            decimal? actual = valueInput.ConvertTo<decimal>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullabledecimal_ValidByteInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            byte valueInput = 0;
            decimal? expected = 0;

            //// Act
            decimal? actual = valueInput.ConvertTo<decimal>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullabledecimal_ValidByteNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            byte? valueInput = 0;
            decimal? expected = 0;

            //// Act
            decimal? actual = valueInput.ConvertTo<decimal>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertAsCharCodeToNullabledecimal_ValidCharInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            char valueInput = '2';
            //// Byte representaion of the char
            decimal? expected = 50.0m;

            //// Act
            decimal? actual = valueInput.ConvertAsCharCodeTo<decimal>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertAsCharCodeToNullabledecimal_ValidCharNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            char? valueInput = '2';
            //// Byte representaion of the char code
            decimal? expected = 50.0m;

            //// Act
            decimal? actual = valueInput.ConvertAsCharCodeTo<decimal>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullabledecimal_ValidDecimalInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            decimal valueInput = 0;
            decimal? expected = 0;

            //// Act
            decimal? actual = valueInput.ConvertTo<decimal>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullabledecimal_ValidDecimalNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            decimal? valueInput = 0;
            decimal? expected = 0;

            //// Act
            decimal? actual = valueInput.ConvertTo<decimal>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullabledecimal_ValidDoubleInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            double valueInput = 0;
            decimal? expected = 0;

            //// Act
            decimal? actual = valueInput.ConvertTo<decimal>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullabledecimal_ValidDoubleNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            double? valueInput = 0;
            decimal? expected = 0;

            //// Act
            decimal? actual = valueInput.ConvertTo<decimal>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullabledecimal_ValidFloatInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            float valueInput = 0;
            decimal? expected = 0;

            //// Act
            decimal? actual = valueInput.ConvertTo<decimal>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullabledecimal_ValidFloatNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            float? valueInput = 0;
            decimal? expected = 0;

            //// Act
            decimal? actual = valueInput.ConvertTo<decimal>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullabledecimal_ValidIntInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            int valueInput = 0;
            decimal? expected = 0;

            //// Act
            decimal? actual = valueInput.ConvertTo<decimal>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullabledecimal_ValidIntNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            int? valueInput = 0;
            decimal? expected = 0;

            //// Act
            decimal? actual = valueInput.ConvertTo<decimal>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullabledecimal_ValidLongInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            long valueInput = 0;
            decimal? expected = 0;

            //// Act
            decimal? actual = valueInput.ConvertTo<decimal>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullabledecimal_ValidLongNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            long? valueInput = 0;
            decimal? expected = 0;

            //// Act
            decimal? actual = valueInput.ConvertTo<decimal>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullabledecimal_ValidSbyteInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            sbyte valueInput = 0;
            decimal? expected = 0;

            //// Act
            decimal? actual = valueInput.ConvertTo<decimal>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullabledecimal_ValidSbyteNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            sbyte? valueInput = 0;
            decimal? expected = 0;

            //// Act
            decimal? actual = valueInput.ConvertTo<decimal>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullabledecimal_ValidShortInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            short valueInput = 0;
            decimal? expected = 0;

            //// Act
            decimal? actual = valueInput.ConvertTo<decimal>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullabledecimal_ValidShortNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            short? valueInput = 0;
            decimal? expected = 0;

            //// Act
            decimal? actual = valueInput.ConvertTo<decimal>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullabledecimal_ValidStringInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            string valueInput = "false";
            decimal? expected = 0;

            //// Act
            decimal? actual = valueInput.ConvertTo<decimal>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullabledecimal_ValidUintInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            uint valueInput = 0;
            decimal? expected = 0;

            //// Act
            decimal? actual = valueInput.ConvertTo<decimal>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullabledecimal_ValidUintNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            uint? valueInput = 0;
            decimal? expected = 0;

            //// Act
            decimal? actual = valueInput.ConvertTo<decimal>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullabledecimal_ValidUlongInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            ulong valueInput = 0;
            decimal? expected = 0;

            //// Act
            decimal? actual = valueInput.ConvertTo<decimal>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullabledecimal_ValidUlongNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            ulong? valueInput = 0;
            decimal? expected = 0;

            //// Act
            decimal? actual = valueInput.ConvertTo<decimal>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullabledecimal_ValidUshortInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            ushort valueInput = 0;
            decimal? expected = 0;

            //// Act
            decimal? actual = valueInput.ConvertTo<decimal>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullabledecimal_ValidUshortNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            ushort? valueInput = 0;
            decimal? expected = 0;

            //// Act
            decimal? actual = valueInput.ConvertTo<decimal>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        #endregion Methods
    }
}