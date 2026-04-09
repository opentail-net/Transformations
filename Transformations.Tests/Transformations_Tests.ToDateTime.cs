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
        public void ConvertTodatetime_InvalidStringInput_ReturnsDefaultValue()
        {
            //// Setup
            string valueInput = "invalid input";
            DateTime expected = new DateTime(2000, 1, 1);

            //// Act
            DateTime actual = valueInput.ConvertTo<DateTime>(new DateTime(2000, 1, 1));

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertTodatetime_InvalidStringNullInput_ReturnsDefaultValue()
        {
            //// Setup
            string? valueInput = null;
            DateTime expected = new DateTime(2000, 1, 1);

            //// Act
            DateTime actual = valueInput.ConvertTo<DateTime>(new DateTime(2000, 1, 1));

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertTodatetime_ValidBoolInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            bool valueInput = false;
            DateTime expected = new DateTime(1899, 12, 30);

            //// Act
            DateTime actual = valueInput.ConvertTo<DateTime>(new DateTime(2000, 1, 1));

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertTodatetime_ValidBoolNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            bool? valueInput = false;
            DateTime expected = new DateTime(1899, 12, 30);

            //// Act
            DateTime actual = valueInput.ConvertTo<DateTime>(new DateTime(2000, 1, 1));

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertTodatetime_ValidByteInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            byte valueInput = 0;
            DateTime expected = new DateTime(1899, 12, 30);

            //// Act
            DateTime actual = valueInput.ConvertTo<DateTime>(new DateTime(2000, 1, 1));

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertTodatetime_ValidByteNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            byte? valueInput = 0;
            DateTime expected = new DateTime(1899, 12, 30);

            //// Act
            DateTime actual = valueInput.ConvertTo<DateTime>(new DateTime(2000, 1, 1));

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertTodatetime_ValidCharInput_ReturnsDefaultValue()
        {
            //// Setup
            char valueInput = 'f';
            DateTime expected = new DateTime(2000, 1, 1);

            //// Act
            DateTime actual = valueInput.ConvertTo<DateTime>(new DateTime(2000, 1, 1));

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertTodatetime_ValidCharNullableInput_ReturnsDefaultValue()
        {
            //// Setup
            char? valueInput = 'f';
            DateTime expected = new DateTime(2000, 1, 1);

            //// Act
            DateTime actual = valueInput.ConvertTo<DateTime>(new DateTime(2000, 1, 1));

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertTodatetime_ValidDecimalInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            decimal valueInput = 0;
            DateTime expected = new DateTime(1899, 12, 30);

            //// Act
            DateTime actual = valueInput.ConvertTo<DateTime>(new DateTime(2000, 1, 1));

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertTodatetime_ValidDecimalNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            decimal? valueInput = 0;
            DateTime expected = new DateTime(1899, 12, 30);

            //// Act
            DateTime actual = valueInput.ConvertTo<DateTime>(new DateTime(2000, 1, 1));

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertTodatetime_ValidDoubleInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            double valueInput = 0;
            DateTime expected = new DateTime(1899, 12, 30);

            //// Act
            DateTime actual = valueInput.ConvertTo<DateTime>(new DateTime(2000, 1, 1));

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertTodatetime_ValidDoubleNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            double? valueInput = 0;
            DateTime expected = new DateTime(1899, 12, 30);

            //// Act
            DateTime actual = valueInput.ConvertTo<DateTime>(new DateTime(2000, 1, 1));

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertTodatetime_ValidFloatInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            float valueInput = 0;
            DateTime expected = new DateTime(1899, 12, 30);

            //// Act
            DateTime actual = valueInput.ConvertTo<DateTime>(new DateTime(2000, 1, 1));

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertTodatetime_ValidFloatNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            float? valueInput = 0;
            DateTime expected = new DateTime(1899, 12, 30);

            //// Act
            DateTime actual = valueInput.ConvertTo<DateTime>(new DateTime(2000, 1, 1));

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertTodatetime_ValidIntInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            int valueInput = 0;
            DateTime expected = new DateTime(1899, 12, 30);

            //// Act
            DateTime actual = valueInput.ConvertTo<DateTime>(new DateTime(2000, 1, 1));

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertTodatetime_ValidIntNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            int? valueInput = 0;
            DateTime expected = new DateTime(1899, 12, 30);

            //// Act
            DateTime actual = valueInput.ConvertTo<DateTime>(new DateTime(2000, 1, 1));

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertTodatetime_ValidLongInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            long valueInput = 0;
            DateTime expected = new DateTime(1899, 12, 30);

            //// Act
            DateTime actual = valueInput.ConvertTo<DateTime>(new DateTime(2000, 1, 1));

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertTodatetime_ValidLongNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            long? valueInput = 0;
            DateTime expected = new DateTime(1899, 12, 30);

            //// Act
            DateTime actual = valueInput.ConvertTo<DateTime>(new DateTime(2000, 1, 1));

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertTodatetime_ValidSbyteInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            sbyte valueInput = 0;
            DateTime expected = new DateTime(1899, 12, 30);

            //// Act
            DateTime actual = valueInput.ConvertTo<DateTime>(new DateTime(2000, 1, 1));

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertTodatetime_ValidSbyteNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            sbyte? valueInput = 0;
            DateTime expected = new DateTime(1899, 12, 30);

            //// Act
            DateTime actual = valueInput.ConvertTo<DateTime>(new DateTime(2000, 1, 1));

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertTodatetime_ValidShortInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            short valueInput = 0;
            DateTime expected = new DateTime(1899, 12, 30);

            //// Act
            DateTime actual = valueInput.ConvertTo<DateTime>(new DateTime(2000, 1, 1));

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertTodatetime_ValidShortNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            short? valueInput = 0;
            DateTime expected = new DateTime(1899, 12, 30);

            //// Act
            DateTime actual = valueInput.ConvertTo<DateTime>(new DateTime(2000, 1, 1));

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertTodatetime_ValidStringInput_ReturnsDefaultValue()
        {
            //// Setup
            string valueInput = "false";
            DateTime expected = new DateTime(2000, 1, 1);

            //// Act
            DateTime actual = valueInput.ConvertTo<DateTime>(new DateTime(2000, 1, 1));

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertTodatetime_ValidUintInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            uint valueInput = 0;
            DateTime expected = new DateTime(1899, 12, 30);

            //// Act
            DateTime actual = valueInput.ConvertTo<DateTime>(new DateTime(2000, 1, 1));

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertTodatetime_ValidUintNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            uint? valueInput = 0;
            DateTime expected = new DateTime(1899, 12, 30);

            //// Act
            DateTime actual = valueInput.ConvertTo<DateTime>(new DateTime(2000, 1, 1));

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertTodatetime_ValidUlongInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            ulong valueInput = 0;
            DateTime expected = new DateTime(1899, 12, 30);

            //// Act
            DateTime actual = valueInput.ConvertTo<DateTime>(new DateTime(2000, 1, 1));

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertTodatetime_ValidUlongNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            ulong? valueInput = 0;
            DateTime expected = new DateTime(1899, 12, 30);

            //// Act
            DateTime actual = valueInput.ConvertTo<DateTime>(new DateTime(2000, 1, 1));

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertTodatetime_ValidUshortInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            ushort valueInput = 0;
            DateTime expected = new DateTime(1899, 12, 30);

            //// Act
            DateTime actual = valueInput.ConvertTo<DateTime>(new DateTime(2000, 1, 1));

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertTodatetime_ValidUshortNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            ushort? valueInput = 0;
            DateTime expected = new DateTime(1899, 12, 30);

            //// Act
            DateTime actual = valueInput.ConvertTo<DateTime>(new DateTime(2000, 1, 1));

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        #endregion Methods
    }
}