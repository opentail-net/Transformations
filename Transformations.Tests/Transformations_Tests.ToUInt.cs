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
        public void ConvertTouint_InvalidStringInput_ReturnsDefaultValue()
        {
            //// Setup
            string valueInput = "invalid input";
            uint expected = 1;

            //// Act
            uint actual = valueInput.ConvertTo<uint>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertTouint_InvalidStringNullInput_ReturnsDefaultValue()
        {
            //// Setup
            string? valueInput = null;
            uint expected = 1;

            //// Act
            uint actual = valueInput.ConvertTo<uint>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertTouint_ValidBoolInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            bool valueInput = false;
            uint expected = 0;

            //// Act
            uint actual = valueInput.ConvertTo<uint>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertTouint_ValidBoolNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            bool? valueInput = false;
            uint expected = 0;

            //// Act
            uint actual = valueInput.ConvertTo<uint>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertTouint_ValidByteInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            byte valueInput = 2;
            uint expected = 2;

            //// Act
            uint actual = valueInput.ConvertTo<uint>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertTouint_ValidByteNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            byte? valueInput = 2;
            uint expected = 2;

            //// Act
            uint actual = valueInput.ConvertTo<uint>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertTouint_ValidCharInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            char valueInput = 'f';
            //// Byte representaion of the char
            uint expected = 102;

            //// Act
            uint actual = valueInput.ConvertTo<uint>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertTouint_ValidCharNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            char? valueInput = 'f';
            //// Byte representaion of the char
            uint expected = 102;

            //// Act
            uint actual = valueInput.ConvertTo<uint>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertTouint_ValidDecimalInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            decimal valueInput = 2;
            uint expected = 2;

            //// Act
            uint actual = valueInput.ConvertTo<uint>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertTouint_ValidDecimalNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            decimal? valueInput = 2;
            uint expected = 2;

            //// Act
            uint actual = valueInput.ConvertTo<uint>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertTouint_ValidDoubleInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            double valueInput = 2;
            uint expected = 2;

            //// Act
            uint actual = valueInput.ConvertTo<uint>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertTouint_ValidDoubleNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            double? valueInput = 2;
            uint expected = 2;

            //// Act
            uint actual = valueInput.ConvertTo<uint>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertTouint_ValidFloatInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            float valueInput = 2;
            uint expected = 2;

            //// Act
            uint actual = valueInput.ConvertTo<uint>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertTouint_ValidFloatNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            float? valueInput = 2;
            uint expected = 2;

            //// Act
            uint actual = valueInput.ConvertTo<uint>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertTouint_ValidIntInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            int valueInput = 2;
            uint expected = 2;

            //// Act
            uint actual = valueInput.ConvertTo<uint>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertTouint_ValidIntNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            int? valueInput = 2;
            uint expected = 2;

            //// Act
            uint actual = valueInput.ConvertTo<uint>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertTouint_ValidLongInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            long valueInput = 2;
            uint expected = 2;

            //// Act
            uint actual = valueInput.ConvertTo<uint>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertTouint_ValidLongNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            long? valueInput = 2;
            uint expected = 2;

            //// Act
            uint actual = valueInput.ConvertTo<uint>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertTouint_ValidSbyteInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            sbyte valueInput = 2;
            uint expected = 2;

            //// Act
            uint actual = valueInput.ConvertTo<uint>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertTouint_ValidSbyteNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            sbyte? valueInput = 2;
            uint expected = 2;

            //// Act
            uint actual = valueInput.ConvertTo<uint>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertTouint_ValidShortInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            short valueInput = 2;
            uint expected = 2;

            //// Act
            uint actual = valueInput.ConvertTo<uint>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertTouint_ValidShortNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            short? valueInput = 2;
            uint expected = 2;

            //// Act
            uint actual = valueInput.ConvertTo<uint>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertTouint_ValidStringInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            string valueInput = "123";
            uint expected = 123;

            //// Act
            uint actual = valueInput.ConvertTo<uint>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertTouint_ValidUintInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            uint valueInput = 2;
            uint expected = 2;

            //// Act
            uint actual = valueInput.ConvertTo<uint>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertTouint_ValidUintNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            uint? valueInput = 2;
            uint expected = 2;

            //// Act
            uint actual = valueInput.ConvertTo<uint>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertTouint_ValidUlongInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            ulong valueInput = 2;
            uint expected = 2;

            //// Act
            uint actual = valueInput.ConvertTo<uint>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertTouint_ValidUlongNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            ulong? valueInput = 2;
            uint expected = 2;

            //// Act
            uint actual = valueInput.ConvertTo<uint>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertTouint_ValidUshortInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            ushort valueInput = 2;
            uint expected = 2;

            //// Act
            uint actual = valueInput.ConvertTo<uint>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertTouint_ValidUshortNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            ushort? valueInput = 2;
            uint expected = 2;

            //// Act
            uint actual = valueInput.ConvertTo<uint>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        #endregion Methods
    }
}