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
        public void ConvertToNullablefloat_InvalidStringInput_ReturnsDefaultValue()
        {
            //// Setup
            string valueInput = "invalid input";
            float? expected = null;

            //// Act
            float? actual = valueInput.ConvertTo<float>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullablefloat_InvalidStringNullInput_ReturnsNullValue()
        {
            //// Setup
            string? valueInput = null;
            float? expected = null;

            //// Act
            float? actual = valueInput.ConvertTo<float>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullablefloat_ValidBoolInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            bool valueInput = false;
            float? expected = 0;

            //// Act
            float? actual = valueInput.ConvertTo<float>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullablefloat_ValidBoolNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            bool? valueInput = false;
            float? expected = 0;

            //// Act
            float? actual = valueInput.ConvertTo<float>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullablefloat_ValidByteInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            byte valueInput = 0;
            float? expected = 0;

            //// Act
            float? actual = valueInput.ConvertTo<float>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullablefloat_ValidByteNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            byte? valueInput = 0;
            float? expected = 0;

            //// Act
            float? actual = valueInput.ConvertTo<float>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullablefloat_ValidCharInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            char valueInput = '2';
            //// Byte representaion of the char
            float? expected = 2.0f;

            //// Act
            float? actual = valueInput.ConvertTo<float>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullablefloat_ValidCharNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            char? valueInput = '2';
            //// Byte representaion of the char
            float? expected = 2.0f;

            //// Act
            float? actual = valueInput.ConvertTo<float>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullablefloat_ValidDecimalInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            decimal valueInput = 0;
            float? expected = 0;

            //// Act
            float? actual = valueInput.ConvertTo<float>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullablefloat_ValidDecimalNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            decimal? valueInput = 0;
            float? expected = 0;

            //// Act
            float? actual = valueInput.ConvertTo<float>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullablefloat_ValidDoubleInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            double valueInput = 0;
            float? expected = 0;

            //// Act
            float? actual = valueInput.ConvertTo<float>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullablefloat_ValidDoubleNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            double? valueInput = 0;
            float? expected = 0;

            //// Act
            float? actual = valueInput.ConvertTo<float>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullablefloat_ValidFloatInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            float valueInput = 0;
            float? expected = 0;

            //// Act
            float? actual = valueInput.ConvertTo<float>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullablefloat_ValidFloatNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            float? valueInput = 0;
            float? expected = 0;

            //// Act
            float? actual = valueInput.ConvertTo<float>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullablefloat_ValidIntInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            int valueInput = 0;
            float? expected = 0;

            //// Act
            float? actual = valueInput.ConvertTo<float>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullablefloat_ValidIntNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            int? valueInput = 0;
            float? expected = 0;

            //// Act
            float? actual = valueInput.ConvertTo<float>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullablefloat_ValidLongInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            long valueInput = 0;
            float? expected = 0;

            //// Act
            float? actual = valueInput.ConvertTo<float>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullablefloat_ValidLongNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            long? valueInput = 0;
            float? expected = 0;

            //// Act
            float? actual = valueInput.ConvertTo<float>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullablefloat_ValidSbyteInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            sbyte valueInput = 0;
            float? expected = 0;

            //// Act
            float? actual = valueInput.ConvertTo<float>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullablefloat_ValidSbyteNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            sbyte? valueInput = 0;
            float? expected = 0;

            //// Act
            float? actual = valueInput.ConvertTo<float>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullablefloat_ValidShortInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            short valueInput = 0;
            float? expected = 0;

            //// Act
            float? actual = valueInput.ConvertTo<float>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullablefloat_ValidShortNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            short? valueInput = 0;
            float? expected = 0;

            //// Act
            float? actual = valueInput.ConvertTo<float>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullablefloat_ValidStringInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            string valueInput = "false";
            float? expected = 0;

            //// Act
            float? actual = valueInput.ConvertTo<float>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullablefloat_ValidUintInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            uint valueInput = 0;
            float? expected = 0;

            //// Act
            float? actual = valueInput.ConvertTo<float>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullablefloat_ValidUintNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            uint? valueInput = 0;
            float? expected = 0;

            //// Act
            float? actual = valueInput.ConvertTo<float>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullablefloat_ValidUlongInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            ulong valueInput = 0;
            float? expected = 0;

            //// Act
            float? actual = valueInput.ConvertTo<float>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullablefloat_ValidUlongNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            ulong? valueInput = 0;
            float? expected = 0;

            //// Act
            float? actual = valueInput.ConvertTo<float>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullablefloat_ValidUshortInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            ushort valueInput = 0;
            float? expected = 0;

            //// Act
            float? actual = valueInput.ConvertTo<float>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullablefloat_ValidUshortNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            ushort? valueInput = 0;
            float? expected = 0;

            //// Act
            float? actual = valueInput.ConvertTo<float>(null);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        #endregion Methods
    }
}