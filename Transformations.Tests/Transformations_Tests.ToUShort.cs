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
        public void ConvertToushort_InvalidStringInput_ReturnsDefaultValue()
        {
            //// Setup
            string valueInput = "invalid input";
            ushort expected = 1;

            //// Act
            ushort actual = valueInput.ConvertTo<ushort>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToushort_InvalidStringNullInput_ReturnsDefaultValue()
        {
            //// Setup
            string? valueInput = null;
            ushort expected = 1;

            //// Act
            ushort actual = valueInput.ConvertTo<ushort>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToushort_ValidBoolInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            bool valueInput = false;
            ushort expected = 0;

            //// Act
            ushort actual = valueInput.ConvertTo<ushort>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToushort_ValidBoolNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            bool? valueInput = false;
            ushort expected = 0;

            //// Act
            ushort actual = valueInput.ConvertTo<ushort>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToushort_ValidByteInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            byte valueInput = 2;
            ushort expected = 2;

            //// Act
            ushort actual = valueInput.ConvertTo<ushort>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToushort_ValidByteNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            byte? valueInput = 2;
            ushort expected = 2;

            //// Act
            ushort actual = valueInput.ConvertTo<ushort>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToushort_ValidCharInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            char valueInput = 'f';
            //// Byte representaion of the char
            ushort expected = 102;

            //// Act
            ushort actual = valueInput.ConvertTo<ushort>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToushort_ValidCharNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            char? valueInput = 'f';
            //// Byte representaion of the char
            ushort expected = 102;

            //// Act
            ushort actual = valueInput.ConvertTo<ushort>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToushort_ValidDecimalInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            decimal valueInput = 2;
            ushort expected = 2;

            //// Act
            ushort actual = valueInput.ConvertTo<ushort>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToushort_ValidDecimalNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            decimal? valueInput = 2;
            ushort expected = 2;

            //// Act
            ushort actual = valueInput.ConvertTo<ushort>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToushort_ValidDoubleInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            double valueInput = 2;
            ushort expected = 2;

            //// Act
            ushort actual = valueInput.ConvertTo<ushort>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToushort_ValidDoubleNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            double? valueInput = 2;
            ushort expected = 2;

            //// Act
            ushort actual = valueInput.ConvertTo<ushort>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToushort_ValidFloatInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            float valueInput = 2;
            ushort expected = 2;

            //// Act
            ushort actual = valueInput.ConvertTo<ushort>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToushort_ValidFloatNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            float? valueInput = 2;
            ushort expected = 2;

            //// Act
            ushort actual = valueInput.ConvertTo<ushort>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToushort_ValidIntInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            int valueInput = 2;
            ushort expected = 2;

            //// Act
            ushort actual = valueInput.ConvertTo<ushort>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToushort_ValidIntNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            int? valueInput = 2;
            ushort expected = 2;

            //// Act
            ushort actual = valueInput.ConvertTo<ushort>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToushort_ValidLongInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            long valueInput = 2;
            ushort expected = 2;

            //// Act
            ushort actual = valueInput.ConvertTo<ushort>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToushort_ValidLongNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            long? valueInput = 2;
            ushort expected = 2;

            //// Act
            ushort actual = valueInput.ConvertTo<ushort>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToushort_ValidSbyteInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            sbyte valueInput = 2;
            ushort expected = 2;

            //// Act
            ushort actual = valueInput.ConvertTo<ushort>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToushort_ValidSbyteNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            sbyte? valueInput = 2;
            ushort expected = 2;

            //// Act
            ushort actual = valueInput.ConvertTo<ushort>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToushort_ValidShortInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            short valueInput = 2;
            ushort expected = 2;

            //// Act
            ushort actual = valueInput.ConvertTo<ushort>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToushort_ValidShortNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            short? valueInput = 2;
            ushort expected = 2;

            //// Act
            ushort actual = valueInput.ConvertTo<ushort>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToushort_ValidStringInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            string valueInput = "123";
            ushort expected = 123;

            //// Act
            ushort actual = valueInput.ConvertTo<ushort>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToushort_ValidUintInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            uint valueInput = 2;
            ushort expected = 2;

            //// Act
            ushort actual = valueInput.ConvertTo<ushort>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToushort_ValidUintNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            uint? valueInput = 2;
            ushort expected = 2;

            //// Act
            ushort actual = valueInput.ConvertTo<ushort>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToushort_ValidUlongInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            ulong valueInput = 2;
            ushort expected = 2;

            //// Act
            ushort actual = valueInput.ConvertTo<ushort>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToushort_ValidUlongNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            ulong? valueInput = 2;
            ushort expected = 2;

            //// Act
            ushort actual = valueInput.ConvertTo<ushort>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToushort_ValidUshortInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            ushort valueInput = 2;
            ushort expected = 2;

            //// Act
            ushort actual = valueInput.ConvertTo<ushort>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToushort_ValidUshortNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            ushort? valueInput = 2;
            ushort expected = 2;

            //// Act
            ushort actual = valueInput.ConvertTo<ushort>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        #endregion Methods
    }
}