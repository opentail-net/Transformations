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
        public void ConvertToulong_InvalidStringInput_ReturnsDefaultValue()
        {
            //// Setup
            string valueInput = "invalid input";
            ulong expected = 1;

            //// Act
            ulong actual = valueInput.ConvertTo<ulong>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToulong_InvalidStringNullInput_ReturnsDefaultValue()
        {
            //// Setup
            string? valueInput = null;
            ulong expected = 1;

            //// Act
            ulong actual = valueInput.ConvertTo<ulong>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToulong_ValidBoolInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            bool valueInput = false;
            ulong expected = 0;

            //// Act
            ulong actual = valueInput.ConvertTo<ulong>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToulong_ValidBoolNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            bool? valueInput = false;
            ulong expected = 0;

            //// Act
            ulong actual = valueInput.ConvertTo<ulong>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToulong_ValidByteInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            byte valueInput = 2;
            ulong expected = 2;

            //// Act
            ulong actual = valueInput.ConvertTo<ulong>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToulong_ValidByteNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            byte? valueInput = 2;
            ulong expected = 2;

            //// Act
            ulong actual = valueInput.ConvertTo<ulong>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToulong_ValidCharInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            char valueInput = 'f';
            //// Byte representaion of the char
            ulong expected = 102;

            //// Act
            ulong actual = valueInput.ConvertTo<ulong>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToulong_ValidCharNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            char? valueInput = 'f';
            //// Byte representaion of the char
            ulong expected = 102;

            //// Act
            ulong actual = valueInput.ConvertTo<ulong>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToulong_ValidDecimalInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            decimal valueInput = 2;
            ulong expected = 2;

            //// Act
            ulong actual = valueInput.ConvertTo<ulong>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToulong_ValidDecimalNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            decimal? valueInput = 2;
            ulong expected = 2;

            //// Act
            ulong actual = valueInput.ConvertTo<ulong>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToulong_ValidDoubleInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            double valueInput = 2;
            ulong expected = 2;

            //// Act
            ulong actual = valueInput.ConvertTo<ulong>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToulong_ValidDoubleNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            double? valueInput = 2;
            ulong expected = 2;

            //// Act
            ulong actual = valueInput.ConvertTo<ulong>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToulong_ValidFloatInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            float valueInput = 2;
            ulong expected = 2;

            //// Act
            ulong actual = valueInput.ConvertTo<ulong>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToulong_ValidFloatNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            float? valueInput = 2;
            ulong expected = 2;

            //// Act
            ulong actual = valueInput.ConvertTo<ulong>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToulong_ValidIntInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            int valueInput = 2;
            ulong expected = 2;

            //// Act
            ulong actual = valueInput.ConvertTo<ulong>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToulong_ValidIntNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            int? valueInput = 2;
            ulong expected = 2;

            //// Act
            ulong actual = valueInput.ConvertTo<ulong>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToulong_ValidLongInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            long valueInput = 2;
            ulong expected = 2;

            //// Act
            ulong actual = valueInput.ConvertTo<ulong>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToulong_ValidLongNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            long? valueInput = 2;
            ulong expected = 2;

            //// Act
            ulong actual = valueInput.ConvertTo<ulong>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToulong_ValidSbyteInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            sbyte valueInput = 2;
            ulong expected = 2;

            //// Act
            ulong actual = valueInput.ConvertTo<ulong>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToulong_ValidSbyteNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            sbyte? valueInput = 2;
            ulong expected = 2;

            //// Act
            ulong actual = valueInput.ConvertTo<ulong>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToulong_ValidShortInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            short valueInput = 2;
            ulong expected = 2;

            //// Act
            ulong actual = valueInput.ConvertTo<ulong>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToulong_ValidShortNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            short? valueInput = 2;
            ulong expected = 2;

            //// Act
            ulong actual = valueInput.ConvertTo<ulong>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToulong_ValidStringInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            string valueInput = "123";
            ulong expected = 123;

            //// Act
            ulong actual = valueInput.ConvertTo<ulong>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToulong_ValidUintInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            uint valueInput = 2;
            ulong expected = 2;

            //// Act
            ulong actual = valueInput.ConvertTo<ulong>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToulong_ValidUintNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            uint? valueInput = 2;
            ulong expected = 2;

            //// Act
            ulong actual = valueInput.ConvertTo<ulong>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToulong_ValidUlongInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            ulong valueInput = 2;
            ulong expected = 2;

            //// Act
            ulong actual = valueInput.ConvertTo<ulong>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToulong_ValidUlongNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            ulong? valueInput = 2;
            ulong expected = 2;

            //// Act
            ulong actual = valueInput.ConvertTo<ulong>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToulong_ValidUshortInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            ushort valueInput = 2;
            ulong expected = 2;

            //// Act
            ulong actual = valueInput.ConvertTo<ulong>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToulong_ValidUshortNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            ushort? valueInput = 2;
            ulong expected = 2;

            //// Act
            ulong actual = valueInput.ConvertTo<ulong>(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        #endregion Methods
    }
}