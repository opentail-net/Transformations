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

        /*
         * -- not sure what the expected value should be...
                [Test]
                public void ConvertTochar_ValidBoolInput_ReturnsValueAsCorrectType()
                {
                    //// Setup
                    bool valueInput = false;
                    char expected = 'f';

                    //// Act
                    char actual = valueInput.ConvertTo<char>();

                    //// Assert
                    Assert.That(actual, Is.EqualTo(expected));
                }

                [Test]
                public void ConvertTochar_ValidBoolNullableInput_ReturnsValueAsCorrectType()
                {
                    //// Setup
                    bool? valueInput = false;
                    char expected = 'f';

                    //// Act
                    char actual = valueInput.ConvertTo<char>();

                    //// Assert
                    Assert.That(actual, Is.EqualTo(expected));
                }

        
                -- not sure what the expected value should be...
                [Test]
                public void ConvertTochar_ValidByteInput_ReturnsValueAsCorrectType()
                {
                    //// Setup
                    byte valueInput = 0;
                    char expected = '0';

                    //// Act
                    char actual = valueInput.ConvertTo<char>();

                    //// Assert
                    Assert.That(actual, Is.EqualTo(expected));
                }
                */
        [Test]
        public void ConvertTochar_ValidByteNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            byte? valueInput = 0;
            char expected = '\0';

            //// Act
            char actual = valueInput.ConvertTo<char>();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertTochar_ValidCharInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            char valueInput = 'f';
            char expected = 'f';

            //// Act
            char actual = valueInput.ConvertTo<char>();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertTochar_ValidCharNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            char? valueInput = 'f';
            char expected = 'f';

            //// Act
            char actual = valueInput.ConvertTo<char>();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertTochar_ValidDecimalInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            decimal valueInput = 0;
            char expected = '\0';

            //// Act
            char actual = valueInput.ConvertTo<char>();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertTochar_ValidDecimalNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            decimal? valueInput = 0;
            char expected = '\0';

            //// Act
            char actual = valueInput.ConvertTo<char>();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertTochar_ValidDoubleInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            double valueInput = 0;
            char expected = '\0';

            //// Act
            char actual = valueInput.ConvertTo<char>();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertTochar_ValidDoubleNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            double? valueInput = 0;
            char expected = '\0';

            //// Act
            char actual = valueInput.ConvertTo<char>();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertTochar_ValidFloatInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            float valueInput = 0;
            char expected = '\0';

            //// Act
            char actual = valueInput.ConvertTo<char>();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertTochar_ValidFloatNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            float? valueInput = 0;
            char expected = '\0';

            //// Act
            char actual = valueInput.ConvertTo<char>();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertTochar_ValidIntInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            int valueInput = 0;
            char expected = '\0';

            //// Act
            char actual = valueInput.ConvertTo<char>();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertTochar_ValidIntNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            int? valueInput = 0;
            char expected = '\0';

            //// Act
            char actual = valueInput.ConvertTo<char>();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertTochar_ValidLongInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            long valueInput = 0;
            char expected = '\0';

            //// Act
            char actual = valueInput.ConvertTo<char>();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertTochar_ValidLongNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            long? valueInput = 0;
            char expected = '\0';

            //// Act
            char actual = valueInput.ConvertTo<char>();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertTochar_ValidSbyteInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            sbyte valueInput = 0;
            char expected = '\0';

            //// Act
            char actual = valueInput.ConvertTo<char>();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertTochar_ValidSbyteNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            sbyte? valueInput = 0;
            char expected = '\0';

            //// Act
            char actual = valueInput.ConvertTo<char>();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertTochar_ValidShortInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            short valueInput = 0;
            char expected = '\0';

            //// Act
            char actual = valueInput.ConvertTo<char>();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertTochar_ValidShortNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            short? valueInput = 0;
            char expected = '\0';

            //// Act
            char actual = valueInput.ConvertTo<char>();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertTochar_ValidStringInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            string valueInput = "false";
            char expected = 'f';

            //// Act
            char actual = valueInput.ConvertTo<char>();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertTochar_ValidUintInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            uint valueInput = 0;
            char expected = '\0';

            //// Act
            char actual = valueInput.ConvertTo<char>();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertTochar_ValidUintNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            uint? valueInput = 0;
            char expected = '\0';

            //// Act
            char actual = valueInput.ConvertTo<char>();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertTochar_ValidUlongInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            ulong valueInput = 0;
            char expected = '\0';

            //// Act
            char actual = valueInput.ConvertTo<char>();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertTochar_ValidUlongNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            ulong? valueInput = 0;
            char expected = '\0';

            //// Act
            char actual = valueInput.ConvertTo<char>();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertTochar_ValidUshortInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            ushort valueInput = 0;
            char expected = '\0';

            //// Act
            char actual = valueInput.ConvertTo<char>();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertTochar_ValidUshortNullableInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            ushort? valueInput = 0;
            char expected = '\0';

            //// Act
            char actual = valueInput.ConvertTo<char>();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        /*
        [Test]
        public void ConvertTochar_InvalidStringInput_ReturnsDefaultValue()
        {
            //// Setup
            string valueInput = "invalid input";
            char expected = true;

            //// Act
            char actual = valueInput.ConvertTo<char>();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }
        */

        [Test]
        public void ConvertTochar_InvalidStringNullInput_ReturnEmptyValue()
        {
            //// Setup
            string? valueInput = null;
            char expected = ' ';

            //// Act
            char actual = valueInput.ConvertTo<char>(' ');

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }
    }
}