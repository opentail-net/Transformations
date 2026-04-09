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
        public void ConvertToNullablelong_InvalidStringInput_ReturnsDefaultValue()
        {
            //// Setup
            string valueInput = "invalid input";
            long? expected = null;

            //// Act
            long? actual = valueInput.ConvertTo<long>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullablelong_InvalidStringNullInput_ReturnsNullValue()
        {
            //// Setup
            string? valueInput = null;
            long? expected = null;

            //// Act
            long? actual = valueInput.ConvertTo<long>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullablelong_ValidBoolInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            bool valueInput = false;
            long? expected = 0;

            //// Act
            long? actual = valueInput.ConvertTo<long>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullablelong_ValidBoolNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            bool? valueInput = false;
            long? expected = 0;

            //// Act
            long? actual = valueInput.ConvertTo<long>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullablelong_ValidByteInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            byte valueInput = 2;
            long? expected = 2;

            //// Act
            long? actual = valueInput.ConvertTo<long>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullablelong_ValidByteNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            byte? valueInput = 2;
            long? expected = 2;

            //// Act
            long? actual = valueInput.ConvertTo<long>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertCharCodeToNullablelong_ValidCharInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            char valueInput = 'f';
            //// Byte representaion of the char
            long? expected = 102;

            //// Act
            long? actual = valueInput.ConvertAsCharCodeTo<long>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullablelong_ValidCharNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            char? valueInput = 'f';
            //// Byte representaion of the char
            long? expected = 102;

            //// Act
            long? actual = valueInput.ConvertAsCharCodeTo<long>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullablelong_ValidDecimalInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            decimal valueInput = 2;
            long? expected = 2;

            //// Act
            long? actual = valueInput.ConvertTo<long>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullablelong_ValidDecimalNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            decimal? valueInput = 2;
            long? expected = 2;

            //// Act
            long? actual = valueInput.ConvertTo<long>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullablelong_ValidDoubleInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            double valueInput = 2;
            long? expected = 2;

            //// Act
            long? actual = valueInput.ConvertTo<long>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullablelong_ValidDoubleNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            double? valueInput = 2;
            long? expected = 2;

            //// Act
            long? actual = valueInput.ConvertTo<long>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullablelong_ValidFloatInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            float valueInput = 2;
            long? expected = 2;

            //// Act
            long? actual = valueInput.ConvertTo<long>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullablelong_ValidFloatNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            float? valueInput = 2;
            long? expected = 2;

            //// Act
            long? actual = valueInput.ConvertTo<long>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullablelong_ValidIntInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            int valueInput = 2;
            long? expected = 2;

            //// Act
            long? actual = valueInput.ConvertTo<long>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullablelong_ValidIntNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            int? valueInput = 2;
            long? expected = 2;

            //// Act
            long? actual = valueInput.ConvertTo<long>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullablelong_ValidLongInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            long valueInput = 2;
            long? expected = 2;

            //// Act
            long? actual = valueInput.ConvertTo<long>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullablelong_ValidLongNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            long? valueInput = 2;
            long? expected = 2;

            //// Act
            long? actual = valueInput.ConvertTo<long>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullablelong_ValidSbyteInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            sbyte valueInput = 2;
            long? expected = 2;

            //// Act
            long? actual = valueInput.ConvertTo<long>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullablelong_ValidSbyteNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            sbyte? valueInput = 2;
            long? expected = 2;

            //// Act
            long? actual = valueInput.ConvertTo<long>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullablelong_ValidShortInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            short valueInput = 2;
            long? expected = 2;

            //// Act
            long? actual = valueInput.ConvertTo<long>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullablelong_ValidShortNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            short? valueInput = 2;
            long? expected = 2;

            //// Act
            long? actual = valueInput.ConvertTo<long>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullablelong_ValidStringInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            string valueInput = "false";
            long? expected = 0;

            //// Act
            long? actual = valueInput.ConvertTo<long>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullablelong_ValidUintInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            uint valueInput = 2;
            long? expected = 2;

            //// Act
            long? actual = valueInput.ConvertTo<long>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullablelong_ValidUintNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            uint? valueInput = 2;
            long? expected = 2;

            //// Act
            long? actual = valueInput.ConvertTo<long>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullablelong_ValidUlongInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            ulong valueInput = 2;
            long? expected = 2;

            //// Act
            long? actual = valueInput.ConvertTo<long>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullablelong_ValidUlongNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            ulong? valueInput = 2;
            long? expected = 2;

            //// Act
            long? actual = valueInput.ConvertTo<long>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullablelong_ValidUshortInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            ushort valueInput = 2;
            long? expected = 2;

            //// Act
            long? actual = valueInput.ConvertTo<long>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullablelong_ValidUshortNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            ushort? valueInput = 2;
            long? expected = 2;

            //// Act
            long? actual = valueInput.ConvertTo<long>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        #endregion Methods
    }
}