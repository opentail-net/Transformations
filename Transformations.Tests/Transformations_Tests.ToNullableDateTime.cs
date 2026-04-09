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
        public void ConvertToNullabledatetime_InvalidStringInput_ReturnsDefaultValue()
        {
            //// Setup
            string valueInput = "invalid input";
            DateTime? expected = new DateTime(2000, 1, 1);

            //// Act
            DateTime? actual = valueInput.ConvertTo<DateTime>(new DateTime(2000, 1, 1));

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullabledatetime_InvalidStringNullInput_ReturnsDefaultValue()
        {
            //// Setup
            string? valueInput = null;
            DateTime? expected = new DateTime(2000, 1, 1);

            //// Act
            DateTime? actual = valueInput.ConvertTo<DateTime>(new DateTime(2000, 1, 1));

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullabledatetime_ValidBoolInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            bool valueInput = false;
            DateTime? expected = new DateTime(1899, 12, 30);

            //// Act
            DateTime? actual = valueInput.ConvertTo<DateTime>(new DateTime(2000, 1, 1));

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullabledatetime_ValidBoolNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            bool? valueInput = false;
            DateTime? expected = new DateTime(1899, 12, 30);

            //// Act
            DateTime? actual = valueInput.ConvertTo<DateTime>(new DateTime(2000, 1, 1));

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullabledatetime_ValidByteInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            byte valueInput = 0;
            DateTime? expected = new DateTime(1899, 12, 30);

            //// Act
            DateTime? actual = valueInput.ConvertTo<DateTime>(new DateTime(2000, 1, 1));

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullabledatetime_ValidByteNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            byte? valueInput = 0;
            DateTime? expected = new DateTime(1899, 12, 30);

            //// Act
            DateTime? actual = valueInput.ConvertTo<DateTime>(new DateTime(2000, 1, 1));

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullabledatetime_ValidCharInput_ReturnsDefaultValue()
        {
            //// Setup
            char valueInput = 'f';
            DateTime? expected = new DateTime(2000, 1, 1);

            //// Act
            DateTime? actual = valueInput.ConvertTo<DateTime>(new DateTime(2000, 1, 1));

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullabledatetime_ValidCharNullableInput_ReturnsDefaultValue()
        {
            //// Setup
            char? valueInput = 'f';
            DateTime? expected = new DateTime(2000, 1, 1);

            //// Act
            DateTime? actual = valueInput.ConvertTo<DateTime>(new DateTime(2000, 1, 1));

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullabledatetime_ValidDecimalInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            decimal valueInput = 0;
            DateTime? expected = new DateTime(1899, 12, 30);

            //// Act
            DateTime? actual = valueInput.ConvertTo<DateTime>(new DateTime(2000, 1, 1));

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullabledatetime_ValidDecimalNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            decimal? valueInput = 0;
            DateTime? expected = new DateTime(1899, 12, 30);

            //// Act
            DateTime? actual = valueInput.ConvertTo<DateTime>(new DateTime(2000, 1, 1));

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullabledatetime_ValidDoubleInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            double valueInput = 0;
            DateTime? expected = new DateTime(1899, 12, 30);

            //// Act
            DateTime? actual = valueInput.ConvertTo<DateTime>(new DateTime(2000, 1, 1));

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullabledatetime_ValidDoubleNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            double? valueInput = 0;
            DateTime? expected = new DateTime(1899, 12, 30);

            //// Act
            DateTime? actual = valueInput.ConvertTo<DateTime>(new DateTime(2000, 1, 1));

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullabledatetime_ValidFloatInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            float valueInput = 0;
            DateTime? expected = new DateTime(1899, 12, 30);

            //// Act
            DateTime? actual = valueInput.ConvertTo<DateTime>(new DateTime(2000, 1, 1));

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullabledatetime_ValidFloatNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            float? valueInput = 0;
            DateTime? expected = new DateTime(1899, 12, 30);

            //// Act
            DateTime? actual = valueInput.ConvertTo<DateTime>(new DateTime(2000, 1, 1));

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullabledatetime_ValidIntInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            int valueInput = 0;
            DateTime? expected = new DateTime(1899, 12, 30);

            //// Act
            DateTime? actual = valueInput.ConvertTo<DateTime>(new DateTime(2000, 1, 1));

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullabledatetime_ValidIntNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            int? valueInput = 0;
            DateTime? expected = new DateTime(1899, 12, 30);

            //// Act
            DateTime? actual = valueInput.ConvertTo<DateTime>(new DateTime(2000, 1, 1));

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullabledatetime_ValidLongInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            long valueInput = 0;
            DateTime? expected = new DateTime(1899, 12, 30);

            //// Act
            DateTime? actual = valueInput.ConvertTo<DateTime>(new DateTime(2000, 1, 1));

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullabledatetime_ValidLongNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            long? valueInput = 0;
            DateTime? expected = new DateTime(1899, 12, 30);

            //// Act
            DateTime? actual = valueInput.ConvertTo<DateTime>(new DateTime(2000, 1, 1));

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullabledatetime_ValidSbyteInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            sbyte valueInput = 0;
            DateTime? expected = new DateTime(1899, 12, 30);

            //// Act
            DateTime? actual = valueInput.ConvertTo<DateTime>(new DateTime(2000, 1, 1));

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullabledatetime_ValidSbyteNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            sbyte? valueInput = 0;
            DateTime? expected = new DateTime(1899, 12, 30);

            //// Act
            DateTime? actual = valueInput.ConvertTo<DateTime>(new DateTime(2000, 1, 1));

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullabledatetime_ValidShortInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            short valueInput = 0;
            DateTime? expected = new DateTime(1899, 12, 30);

            //// Act
            DateTime? actual = valueInput.ConvertTo<DateTime>(new DateTime(2000, 1, 1));

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullabledatetime_ValidShortNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            short? valueInput = 0;
            DateTime? expected = new DateTime(1899, 12, 30);

            //// Act
            DateTime? actual = valueInput.ConvertTo<DateTime>(new DateTime(2000, 1, 1));

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullabledatetime_ValidStringInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            string valueInput = "10/10/2014";
            DateTime? expected = new DateTime(2014, 10, 10);

            //// Act
            DateTime? actual = valueInput.ConvertTo<DateTime>(new DateTime(2000, 1, 1));

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullabledatetime_ValidUintInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            uint valueInput = 0;
            DateTime? expected = new DateTime(1899, 12, 30);

            //// Act
            DateTime? actual = valueInput.ConvertTo<DateTime>(new DateTime(2000, 1, 1));

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullabledatetime_ValidUintNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            uint? valueInput = 0;
            DateTime? expected = new DateTime(1899, 12, 30);

            //// Act
            DateTime? actual = valueInput.ConvertTo<DateTime>(new DateTime(2000, 1, 1));

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullabledatetime_ValidUlongInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            ulong valueInput = 0;
            DateTime? expected = new DateTime(1899, 12, 30);

            //// Act
            DateTime? actual = valueInput.ConvertTo<DateTime>(new DateTime(2000, 1, 1));

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullabledatetime_ValidUlongNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            ulong? valueInput = 0;
            DateTime? expected = new DateTime(1899, 12, 30);

            //// Act
            DateTime? actual = valueInput.ConvertTo<DateTime>(new DateTime(2000, 1, 1));

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullabledatetime_ValidUshortInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            ushort valueInput = 0;
            DateTime? expected = new DateTime(1899, 12, 30);

            //// Act
            DateTime? actual = valueInput.ConvertTo<DateTime>(new DateTime(2000, 1, 1));

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToNullabledatetime_ValidUshortNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            ushort? valueInput = 0;
            DateTime? expected = new DateTime(1899, 12, 30);

            //// Act
            DateTime? actual = valueInput.ConvertTo<DateTime>(new DateTime(2000, 1, 1));

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        #endregion Methods
    }
}