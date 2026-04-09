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
        public void ConvertTofloat_InvalidStringInput_ReturnsDefaultValue()
        {
            //// Setup
            string valueInput = "invalid input";
            float expected = 1;

            //// Act
            float actual = valueInput.ConvertTo<float>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertTofloat_InvalidStringNullInput_ReturnsDefaultValue()
        {
            //// Setup
            string? valueInput = null;
            float expected = 1;

            //// Act
            float actual = valueInput.ConvertTo<float>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertTofloat_ValidBoolInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            bool valueInput = false;
            float expected = 0;

            //// Act
            float actual = valueInput.ConvertTo<float>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertTofloat_ValidBoolNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            bool? valueInput = false;
            float expected = 0;

            //// Act
            float actual = valueInput.ConvertTo<float>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertTofloat_ValidByteInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            byte valueInput = 2;
            float expected = 2;

            //// Act
            float actual = valueInput.ConvertTo<float>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertTofloat_ValidByteNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            byte? valueInput = 2;
            float expected = 2;

            //// Act
            float actual = valueInput.ConvertTo<float>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertTofloat_ValidCharInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            char valueInput = '2';
            //// Byte representaion of the char
            float expected = 2.0f;

            //// Act
            float actual = valueInput.ConvertTo<float>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertTofloat_ValidCharNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            char? valueInput = '2';
            //// Byte representaion of the char
            float expected = 2.0f;

            //// Act
            float actual = valueInput.ConvertTo<float>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertTofloat_ValidDecimalInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            decimal valueInput = 2;
            float expected = 2;

            //// Act
            float actual = valueInput.ConvertTo<float>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertTofloat_ValidDecimalNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            decimal? valueInput = 2;
            float expected = 2;

            //// Act
            float actual = valueInput.ConvertTo<float>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertTofloat_ValidDoubleInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            double valueInput = 2;
            float expected = 2;

            //// Act
            float actual = valueInput.ConvertTo<float>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertTofloat_ValidDoubleNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            double? valueInput = 2;
            float expected = 2;

            //// Act
            float actual = valueInput.ConvertTo<float>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertTofloat_ValidFloatInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            float valueInput = 2;
            float expected = 2;

            //// Act
            float actual = valueInput.ConvertTo<float>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertTofloat_ValidFloatNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            float? valueInput = 2;
            float expected = 2;

            //// Act
            float actual = valueInput.ConvertTo<float>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertTofloat_ValidIntInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            int valueInput = 2;
            float expected = 2;

            //// Act
            float actual = valueInput.ConvertTo<float>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertTofloat_ValidIntNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            int? valueInput = 2;
            float expected = 2;

            //// Act
            float actual = valueInput.ConvertTo<float>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertTofloat_ValidLongInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            long valueInput = 2;
            float expected = 2;

            //// Act
            float actual = valueInput.ConvertTo<float>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertTofloat_ValidLongNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            long? valueInput = 2;
            float expected = 2;

            //// Act
            float actual = valueInput.ConvertTo<float>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertTofloat_ValidSbyteInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            sbyte valueInput = 2;
            float expected = 2;

            //// Act
            float actual = valueInput.ConvertTo<float>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertTofloat_ValidSbyteNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            sbyte? valueInput = 2;
            float expected = 2;

            //// Act
            float actual = valueInput.ConvertTo<float>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertTofloat_ValidShortInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            short valueInput = 2;
            float expected = 2;

            //// Act
            float actual = valueInput.ConvertTo<float>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertTofloat_ValidShortNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            short? valueInput = 2;
            float expected = 2;

            //// Act
            float actual = valueInput.ConvertTo<float>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertTofloat_ValidStringInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            string valueInput = "123.0";
            float expected = 123.0f;

            //// Act
            float actual = valueInput.ConvertTo<float>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertTofloat_ValidUintInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            uint valueInput = 2;
            float expected = 2;

            //// Act
            float actual = valueInput.ConvertTo<float>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertTofloat_ValidUintNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            uint? valueInput = 2;
            float expected = 2;

            //// Act
            float actual = valueInput.ConvertTo<float>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertTofloat_ValidUlongInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            ulong valueInput = 2;
            float expected = 2;

            //// Act
            float actual = valueInput.ConvertTo<float>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertTofloat_ValidUlongNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            ulong? valueInput = 2;
            float expected = 2;

            //// Act
            float actual = valueInput.ConvertTo<float>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertTofloat_ValidUshortInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            ushort valueInput = 2;
            float expected = 2;

            //// Act
            float actual = valueInput.ConvertTo<float>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertTofloat_ValidUshortNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            ushort? valueInput = 2;
            float expected = 2;

            //// Act
            float actual = valueInput.ConvertTo<float>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        #endregion Methods
    }
}