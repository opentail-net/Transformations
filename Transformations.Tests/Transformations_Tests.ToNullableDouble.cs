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
        public void ConvertToNullabledouble_InvalidStringInput_ReturnsDefaultValue()
        {
            //// Setup
            string valueInput = "invalid input";
            double? expected = null;

            //// Act
            double? actual = valueInput.ConvertTo<double>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullabledouble_InvalidStringNullInput_ReturnsNullValue()
        {
            //// Setup
            string? valueInput = null;
            double? expected = null;

            //// Act
            double? actual = valueInput.ConvertTo<double>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullabledouble_ValidBoolInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            bool valueInput = false;
            double? expected = 0;

            //// Act
            double? actual = valueInput.ConvertTo<double>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullabledouble_ValidBoolNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            bool? valueInput = false;
            double? expected = 0;

            //// Act
            double? actual = valueInput.ConvertTo<double>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullabledouble_ValidByteInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            byte valueInput = 0;
            double? expected = 0;

            //// Act
            double? actual = valueInput.ConvertTo<double>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullabledouble_ValidByteNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            byte? valueInput = 0;
            double? expected = 0;

            //// Act
            double? actual = valueInput.ConvertTo<double>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullabledouble_ValidCharInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            char valueInput = '2';
            //// Byte representaion of the char
            double? expected = 2.0;

            //// Act
            double? actual = valueInput.ConvertTo<double>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullabledouble_ValidCharNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            char? valueInput = '2';
            //// Byte representaion of the char
            double? expected = 2.0;

            //// Act
            double? actual = valueInput.ConvertTo<double>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullabledouble_ValidDecimalInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            decimal valueInput = 0;
            double? expected = 0;

            //// Act
            double? actual = valueInput.ConvertTo<double>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullabledouble_ValidDecimalNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            decimal? valueInput = 0;
            double? expected = 0;

            //// Act
            double? actual = valueInput.ConvertTo<double>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullabledouble_ValidDoubleInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            double valueInput = 0;
            double? expected = 0;

            //// Act
            double? actual = valueInput.ConvertTo<double>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullabledouble_ValidDoubleNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            double? valueInput = 0;
            double? expected = 0;

            //// Act
            double? actual = valueInput.ConvertTo<double>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullabledouble_ValidFloatInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            float valueInput = 0;
            double? expected = 0;

            //// Act
            double? actual = valueInput.ConvertTo<double>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullabledouble_ValidFloatNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            float? valueInput = 0;
            double? expected = 0;

            //// Act
            double? actual = valueInput.ConvertTo<double>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullabledouble_ValidIntInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            int valueInput = 0;
            double? expected = 0;

            //// Act
            double? actual = valueInput.ConvertTo<double>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullabledouble_ValidIntNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            int? valueInput = 0;
            double? expected = 0;

            //// Act
            double? actual = valueInput.ConvertTo<double>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullabledouble_ValidLongInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            long valueInput = 0;
            double? expected = 0;

            //// Act
            double? actual = valueInput.ConvertTo<double>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullabledouble_ValidLongNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            long? valueInput = 0;
            double? expected = 0;

            //// Act
            double? actual = valueInput.ConvertTo<double>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullabledouble_ValidSbyteInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            sbyte valueInput = 0;
            double? expected = 0;

            //// Act
            double? actual = valueInput.ConvertTo<double>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullabledouble_ValidSbyteNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            sbyte? valueInput = 0;
            double? expected = 0;

            //// Act
            double? actual = valueInput.ConvertTo<double>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullabledouble_ValidShortInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            short valueInput = 0;
            double? expected = 0;

            //// Act
            double? actual = valueInput.ConvertTo<double>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullabledouble_ValidShortNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            short? valueInput = 0;
            double? expected = 0;

            //// Act
            double? actual = valueInput.ConvertTo<double>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullabledouble_ValidStringInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            string valueInput = "false";
            double? expected = 0.0d;

            //// Act
            double? actual = valueInput.ConvertTo<double>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullabledouble_ValidUintInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            uint valueInput = 0;
            double? expected = 0;

            //// Act
            double? actual = valueInput.ConvertTo<double>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullabledouble_ValidUintNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            uint? valueInput = 0;
            double? expected = 0;

            //// Act
            double? actual = valueInput.ConvertTo<double>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullabledouble_ValidUlongInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            ulong valueInput = 0;
            double? expected = 0;

            //// Act
            double? actual = valueInput.ConvertTo<double>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullabledouble_ValidUlongNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            ulong? valueInput = 0;
            double? expected = 0;

            //// Act
            double? actual = valueInput.ConvertTo<double>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullabledouble_ValidUshortInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            ushort valueInput = 0;
            double? expected = 0;

            //// Act
            double? actual = valueInput.ConvertTo<double>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullabledouble_ValidUshortNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            ushort? valueInput = 0;
            double? expected = 0;

            //// Act
            double? actual = valueInput.ConvertTo<double>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        #endregion Methods
    }
}