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
        public void ConvertTosbyte_InvalidStringInput_ReturnsDefaultValue()
        {
            //// Setup
            string valueInput = "invalid input";
            sbyte expected = 1;

            //// Act
            sbyte actual = valueInput.ConvertTo<sbyte>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertTosbyte_InvalidStringNullInput_ReturnsDefaultValue()
        {
            //// Setup
            string? valueInput = null;
            sbyte expected = 1;

            //// Act
            sbyte actual = valueInput.ConvertTo<sbyte>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertTosbyte_ValidBoolInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            bool valueInput = false;
            sbyte expected = 0;

            //// Act
            sbyte actual = valueInput.ConvertTo<sbyte>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertTosbyte_ValidBoolNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            bool? valueInput = false;
            sbyte expected = 0;

            //// Act
            sbyte actual = valueInput.ConvertTo<sbyte>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertTosbyte_ValidByteInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            byte valueInput = 2;
            sbyte expected = 2;

            //// Act
            sbyte actual = valueInput.ConvertTo<sbyte>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertTosbyte_ValidByteNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            byte? valueInput = 2;
            sbyte expected = 2;

            //// Act
            sbyte actual = valueInput.ConvertTo<sbyte>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertTosbyte_ValidCharInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            char valueInput = 'f';
            //// Byte representaion of the char
            sbyte expected = 102;

            //// Act
            sbyte actual = valueInput.ConvertTo<sbyte>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertTosbyte_ValidCharNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            char? valueInput = 'f';
            //// Byte representaion of the char
            sbyte expected = 102;

            //// Act
            sbyte actual = valueInput.ConvertTo<sbyte>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertTosbyte_ValidDecimalInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            decimal valueInput = 2;
            sbyte expected = 2;

            //// Act
            sbyte actual = valueInput.ConvertTo<sbyte>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertTosbyte_ValidDecimalNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            decimal? valueInput = 2;
            sbyte expected = 2;

            //// Act
            sbyte actual = valueInput.ConvertTo<sbyte>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertTosbyte_ValidDoubleInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            double valueInput = 2;
            sbyte expected = 2;

            //// Act
            sbyte actual = valueInput.ConvertTo<sbyte>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertTosbyte_ValidDoubleNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            double? valueInput = 2;
            sbyte expected = 2;

            //// Act
            sbyte actual = valueInput.ConvertTo<sbyte>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertTosbyte_ValidFloatInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            float valueInput = 2;
            sbyte expected = 2;

            //// Act
            sbyte actual = valueInput.ConvertTo<sbyte>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertTosbyte_ValidFloatNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            float? valueInput = 2;
            sbyte expected = 2;

            //// Act
            sbyte actual = valueInput.ConvertTo<sbyte>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertTosbyte_ValidIntInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            int valueInput = 2;
            sbyte expected = 2;

            //// Act
            sbyte actual = valueInput.ConvertTo<sbyte>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertTosbyte_ValidIntNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            int? valueInput = 2;
            sbyte expected = 2;

            //// Act
            sbyte actual = valueInput.ConvertTo<sbyte>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertTosbyte_ValidLongInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            long valueInput = 2;
            sbyte expected = 2;

            //// Act
            sbyte actual = valueInput.ConvertTo<sbyte>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertTosbyte_ValidLongNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            long? valueInput = 2;
            sbyte expected = 2;

            //// Act
            sbyte actual = valueInput.ConvertTo<sbyte>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertTosbyte_ValidSbyteInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            sbyte valueInput = 2;
            sbyte expected = 2;

            //// Act
            sbyte actual = valueInput.ConvertTo<sbyte>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertTosbyte_ValidSbyteNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            sbyte? valueInput = 2;
            sbyte expected = 2;

            //// Act
            sbyte actual = valueInput.ConvertTo<sbyte>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertTosbyte_ValidShortInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            short valueInput = 2;
            sbyte expected = 2;

            //// Act
            sbyte actual = valueInput.ConvertTo<sbyte>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertTosbyte_ValidShortNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            short? valueInput = 2;
            sbyte expected = 2;

            //// Act
            sbyte actual = valueInput.ConvertTo<sbyte>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertTosbyte_ValidStringInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            string valueInput = "123";
            sbyte expected = 123;

            //// Act
            sbyte actual = valueInput.ConvertTo<sbyte>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertTosbyte_ValidUintInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            uint valueInput = 2;
            sbyte expected = 2;

            //// Act
            sbyte actual = valueInput.ConvertTo<sbyte>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertTosbyte_ValidUintNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            uint? valueInput = 2;
            sbyte expected = 2;

            //// Act
            sbyte actual = valueInput.ConvertTo<sbyte>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertTosbyte_ValidUlongInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            ulong valueInput = 2;
            sbyte expected = 2;

            //// Act
            sbyte actual = valueInput.ConvertTo<sbyte>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertTosbyte_ValidUlongNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            ulong? valueInput = 2;
            sbyte expected = 2;

            //// Act
            sbyte actual = valueInput.ConvertTo<sbyte>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertTosbyte_ValidUshortInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            ushort valueInput = 2;
            sbyte expected = 2;

            //// Act
            sbyte actual = valueInput.ConvertTo<sbyte>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertTosbyte_ValidUshortNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            ushort? valueInput = 2;
            sbyte expected = 2;

            //// Act
            sbyte actual = valueInput.ConvertTo<sbyte>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        #endregion Methods
    }
}