namespace Transformations.Tests
{
    //using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Globalization;
    using FluentAssertions;
    using Transformations;
    using NUnit.Framework;

    /// <summary>
    /// The basic type converter tests.
    /// </summary>
    [TestFixture]
    public partial class BasicTypeConverterTests
    {
        #region Methods

        [Test]
        public void ConvertToByte_InvalidInput_ReturnsDefaultValue()
        {
            //// Setup
            string valueInput = "invalid input";
            byte expected = 0;

            //// Act
            byte actual = valueInput.ConvertTo<byte>(0);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToByte_NullInput_ReturnsDefaultValue()
        {
            //// Setup
            string? valueInput = null;
            byte expected = 0;

            //// Act
            byte actual = valueInput.ConvertTo<byte>(0);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToByte_ValidInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            string valueInput = "1";
            byte expected = 1;

            //// Act
            byte actual = valueInput.ConvertTo<byte>(0);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToChar_InvalidInputEmptyString_ReturnsDefaultValue()
        {
            //// Setup
            string valueInput = string.Empty;
            char expected = 'z';

            //// Act
            char actual = valueInput.ConvertTo<char>('z');

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToChar_NullInput_ReturnsDefaultValue()
        {
            //// Setup
            string? valueInput = null;
            char expected = 'z';

            //// Act
            char actual = valueInput.ConvertTo<char>('z');

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToChar_ValidInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            string valueInput = "x123";
            char expected = 'x';

            //// Act
            char actual = valueInput.ConvertTo<char>('z');

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToDateTime_InvalidInputEmptyString_ReturnsDefaultValue()
        {
            //// Setup
            string valueInput = string.Empty;
            DateTime expected = new DateTime(2000, 01, 01);

            //// Act
            DateTime actual = valueInput.ConvertTo<DateTime>(new DateTime(2000, 01, 01));

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToDateTime_InvalidInputIncorrctFormat_ReturnsDefaultValue()
        {
            //// Setup
            string valueInput = "02/15/2014";
            DateTime expected = new DateTime(2000, 01, 01);

            //// Act
            DateTime actual = valueInput.ConvertTo<DateTime>(new DateTime(2000, 01, 01));

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToDateTime_NullInput_ReturnsDefaultValue()
        {
            //// Setup
            string? valueInput = null;
            DateTime expected = new DateTime(2000, 01, 01);

            //// Act
            DateTime actual = valueInput.ConvertTo<DateTime>(new DateTime(2000, 01, 01));

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToDateTime_ValidInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            string valueInput = "15/02/2014";
            DateTime expected = new DateTime(2014, 02, 15);

            //// Act
            DateTime actual = valueInput.ConvertTo<DateTime>(new DateTime(2000, 01, 01));

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToDecimal_InvalidInput_ReturnsDefaultValue()
        {
            //// Setup
            string valueInput = "invalid input";
            decimal expected = 0.0m;

            //// Act
            decimal actual = valueInput.ConvertTo<decimal>(0);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToDecimal_NullInput_ReturnsDefaultValue()
        {
            //// Setup
            string? valueInput = null;
            decimal expected = 0.0m;

            //// Act
            decimal actual = valueInput.ConvertTo<decimal>(0);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToDecimal_ValidInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            string valueInput = "1.0";
            decimal expected = 1.0m;

            //// Act
            decimal actual = valueInput.ConvertTo<decimal>(0);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToDoubleFromDateTime_MinValueInput_ReturnsDoubleValue()
        {
            //// Setup
            DateTime valueInput = DateTime.MinValue;
            double expected = DateTime.MinValue.ToOADate();

            //// Act
            double actual = valueInput.ConvertTo<double>(0);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToDoubleFromString_InvalidInput_ReturnsDefaultValue()
        {
            //// Setup
            string valueInput = "invalid input";
            double expected = 0;

            //// Act
            double actual = valueInput.ConvertTo<double>(0);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToDoubleFromString_NullInput_ReturnsDefaultValue()
        {
            //// Setup
            string? valueInput = null;
            double expected = 0;

            //// Act
            double actual = valueInput.ConvertTo<double>(0);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToDoubleFromString_ValidInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            string valueInput = "1";
            double expected = 1;

            //// Act
            double actual = valueInput.ConvertTo<double>(0);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToFloat_InvalidInputEmptyString_ReturnsDefaultValue()
        {
            //// Setup
            string valueInput = string.Empty;
            float expected = 0.0f;

            //// Act
            float actual = valueInput.ConvertTo<float>(0.0f);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToFloat_NullInput_ReturnsDefaultValue()
        {
            //// Setup
            string? valueInput = null;
            float expected = 0.0f;

            //// Act
            float actual = valueInput.ConvertTo<float>(0.0f);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToFloat_ValidInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            string valueInput = "0.1";
            float expected = 0.1f;

            //// Act
            float actual = valueInput.ConvertTo<float>(0.0f);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToInt_InvalidInput_ReturnsDefaultValue()
        {
            //// Setup
            string valueInput = "invalid input";
            int expected = 0;

            //// Act
            int actual = valueInput.ConvertTo<int>(0);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToInt_NullInput_ReturnsDefaultValue()
        {
            //// Setup
            string? valueInput = null;
            int expected = 0;

            //// Act
            int actual = valueInput.ConvertTo<int>(0);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToInt_ValidInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            string valueInput = "1";
            int expected = 1;

            //// Act
            int actual = valueInput.ConvertTo<int>(0);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToLong_InvalidInput_ReturnsDefaultValue()
        {
            //// Setup
            string valueInput = "invalid input";
            long expected = 0;

            //// Act
            long actual = valueInput.ConvertTo<long>(0);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToLong_NullInput_ReturnsDefaultValue()
        {
            //// Setup
            string? valueInput = null;
            long expected = 0;

            //// Act
            long actual = valueInput.ConvertTo<long>(0);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToLong_ValidInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            string valueInput = "1";
            long expected = 1;

            //// Act
            long actual = valueInput.ConvertTo<long>(0);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToSbyte_InvalidInput_ReturnsDefaultValue()
        {
            //// Setup
            string valueInput = "invalid input";
            sbyte expected = 0;

            //// Act
            sbyte actual = valueInput.ConvertTo<sbyte>(0);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToSbyte_NullInput_ReturnsDefaultValue()
        {
            //// Setup
            string? valueInput = null;
            sbyte expected = 0;

            //// Act
            sbyte actual = valueInput.ConvertTo<sbyte>(0);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        /*
        [Test]
        public void ConvertToDoubleFromDateTime_MaxValueInput_ReturnsDoubleValue()
        {
            //// Setup
            DateTime valueInput = DateTime.MaxValue;
            double expected = DateTime.MaxValue.ToOADate();

            //// Act
            double actual = valueInput.ConvertTo<double>(0);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }
        */
        [Test]
        public void ConvertToSbyte_ValidInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            string valueInput = "1";
            sbyte expected = 1;

            //// Act
            sbyte actual = valueInput.ConvertTo<sbyte>(0);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToShort_InvalidInput_ReturnsDefaultValue()
        {
            //// Setup
            string valueInput = "invalid input";
            short expected = 0;

            //// Act
            short actual = valueInput.ConvertTo<short>(0);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToShort_NullInput_ReturnsDefaultValue()
        {
            //// Setup
            string? valueInput = null;
            short expected = 0;

            //// Act
            short actual = valueInput.ConvertTo<short>(0);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToShort_ValidInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            string valueInput = "1";
            short expected = 1;

            //// Act
            short actual = valueInput.ConvertTo<short>(0);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToUint_InvalidInput_ReturnsDefaultValue()
        {
            //// Setup
            string valueInput = "invalid input";
            uint expected = 0;

            //// Act
            uint actual = valueInput.ConvertTo<uint>(0);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToUint_NullInput_ReturnsDefaultValue()
        {
            //// Setup
            string? valueInput = null;
            uint expected = 0;

            //// Act
            uint actual = valueInput.ConvertTo<uint>(0);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToUint_ValidInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            string valueInput = "1";
            uint expected = 1;

            //// Act
            uint actual = valueInput.ConvertTo<uint>(0);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToUlong_InvalidInput_ReturnsDefaultValue()
        {
            //// Setup
            string valueInput = "invalid input";
            ulong expected = 0;

            //// Act
            ulong actual = valueInput.ConvertTo<ulong>(0);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToUlong_NullInput_ReturnsDefaultValue()
        {
            //// Setup
            string? valueInput = null;
            ulong expected = 0;

            //// Act
            ulong actual = valueInput.ConvertTo<ulong>(0);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToUlong_ValidInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            string valueInput = "1";
            ulong expected = 1;

            //// Act
            ulong actual = valueInput.ConvertTo<ulong>(0);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToUshort_InvalidInput_ReturnsDefaultValue()
        {
            //// Setup
            string valueInput = "invalid input";
            ushort expected = 0;

            //// Act
            ushort actual = valueInput.ConvertTo<ushort>(0);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToUshort_NullInput_ReturnsDefaultValue()
        {
            //// Setup
            string? valueInput = null;
            ushort expected = 0;

            //// Act
            ushort actual = valueInput.ConvertTo<ushort>(0);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertToUshort_ValidInput_ReturnsValueAsCorrectType()
        {
            //// Setup
            string valueInput = "1";
            ushort expected = 1;

            //// Act
            ushort actual = valueInput.ConvertTo<ushort>(0);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void LoopingConvertToByte_ValidStringInput_PerformsSufficientlyFast()
        {
            //// Setup
            string valueInput = "123";

            DateTime dateTimeStart = DateTime.Now;
            DateTime dateTimeEnd = dateTimeStart.AddSeconds(5);
            byte foo = 0;
            long baselineCount = 0;
            while (DateTime.Now < dateTimeEnd)
            {
                foo = byte.Parse(valueInput);
                baselineCount++;
            }

            // expected minimum number of conversions.
            int expected = 100000;

            //// Act
            dateTimeStart = DateTime.Now;
            dateTimeEnd = dateTimeStart.AddSeconds(5);

            long count = 0;

            while (DateTime.Now < dateTimeEnd)
            {
                foo = valueInput.ConvertTo<byte>();
                count++;
            }

            //// Assert
            Assert.That(count, Is.GreaterThanOrEqualTo(expected));
            Assert.Pass(count.ToString(CultureInfo.InvariantCulture) + " baseline: " + baselineCount.ToString(CultureInfo.InvariantCulture));
        }

        [Test]
        public void LoopingConvertToGuid_ValidStringInput_PerformsSufficientlyFast()
        {
            //// Setup
            string valueInput = Guid.NewGuid().ToString();

            DateTime dateTimeStart = DateTime.Now;
            DateTime dateTimeEnd = dateTimeStart.AddSeconds(5);
            Guid foo = Guid.NewGuid();
            long baselineCount = 0;
            while (DateTime.Now < dateTimeEnd)
            {
                foo = new Guid(valueInput);
                baselineCount++;
            }

            // expected minimum number of conversions.
            int expected = 100000;

            //// Act
            dateTimeStart = DateTime.Now;
            dateTimeEnd = dateTimeStart.AddSeconds(5);

            long count = 0;

            while (DateTime.Now < dateTimeEnd)
            {
                foo = valueInput.ConvertTo<Guid>();
                count++;
            }

            //// Assert
            Assert.That(count, Is.GreaterThanOrEqualTo(expected));
            Assert.Pass(count.ToString(CultureInfo.InvariantCulture) + " baseline: " + baselineCount.ToString(CultureInfo.InvariantCulture));
        }

        [Test]
        public void LoopingConvertToInt_ValidStringInput_PerformsSufficientlyFast()
        {
            //// Setup
            string valueInput = "123";

            DateTime dateTimeStart = DateTime.Now;
            DateTime dateTimeEnd = dateTimeStart.AddSeconds(5);
            int foo = 0;
            long baselineCount = 0;
            while (DateTime.Now < dateTimeEnd)
            {
                foo = int.Parse(valueInput);
                baselineCount++;
            }

            // expected minimum number of conversions.
            int expected = 100000;

            //// Act
            dateTimeStart = DateTime.Now;
            dateTimeEnd = dateTimeStart.AddSeconds(5);

            long count = 0;

            while (DateTime.Now < dateTimeEnd)
            {
                foo = valueInput.ConvertTo<int>();
                count++;
            }

            //// Assert
            Assert.That(count, Is.GreaterThanOrEqualTo(expected));
            Assert.Pass(count.ToString(CultureInfo.InvariantCulture) + " baseline: " + baselineCount.ToString(CultureInfo.InvariantCulture));
        }

        [Test]
        public void LoopingConvertToNullableFloat_ValidIntInputWithNullDefault_PerformsSufficientlyFast()
        {
            //// Setup
            int valueInput = 123;

            DateTime dateTimeStart = DateTime.Now;
            DateTime dateTimeEnd = dateTimeStart.AddSeconds(5);
            float? foo = 0;
            long baselineCount = 0;
            while (DateTime.Now < dateTimeEnd)
            {
                foo = (float)valueInput;
                baselineCount++;
            }

            // expected minimum number of conversions.
            int expected = 100000;

            //// Act
            dateTimeStart = DateTime.Now;
            dateTimeEnd = dateTimeStart.AddSeconds(5);

            long count = 0;

            while (DateTime.Now < dateTimeEnd)
            {
                foo = valueInput.ConvertTo<float>(null);
                count++;
            }

            //// Assert
            Assert.That(count, Is.GreaterThanOrEqualTo(expected));
            Assert.Pass(count.ToString(CultureInfo.InvariantCulture) + " baseline: " + baselineCount.ToString(CultureInfo.InvariantCulture));
        }

        [Test]
        public void LoopingConvertToNullableGuid_ValidStringInputWithDefault_PerformsSufficientlyFast()
        {
            //// Setup
            string valueInput = Guid.NewGuid().ToString();

            DateTime dateTimeStart = DateTime.Now;
            DateTime dateTimeEnd = dateTimeStart.AddSeconds(5);
            Guid? foo = Guid.NewGuid();
            long baselineCount = 0;
            while (DateTime.Now < dateTimeEnd)
            {
                foo = new Guid(valueInput);
                baselineCount++;
            }

            // expected minimum number of conversions.
            int expected = 100000;

            //// Act
            dateTimeStart = DateTime.Now;
            dateTimeEnd = dateTimeStart.AddSeconds(5);

            long count = 0;

            while (DateTime.Now < dateTimeEnd)
            {
                foo = valueInput.ConvertTo<Guid>(Guid.Empty);
                count++;
            }

            //// Assert
            Assert.That(count, Is.GreaterThanOrEqualTo(expected));
            Assert.Pass(count.ToString(CultureInfo.InvariantCulture) + " baseline: " + baselineCount.ToString(CultureInfo.InvariantCulture));
        }

        [Test]
        public void LoopingConvertToNullableGuid_ValidStringInput_PerformsSufficientlyFast()
        {
            //// Setup
            string valueInput = Guid.NewGuid().ToString();

            DateTime dateTimeStart = DateTime.Now;
            DateTime dateTimeEnd = dateTimeStart.AddSeconds(5);
            Guid? foo = Guid.NewGuid();
            long baselineCount = 0;
            while (DateTime.Now < dateTimeEnd)
            {
                foo = new Guid(valueInput);
                baselineCount++;
            }

            // expected minimum number of conversions.
            int expected = 100000;

            //// Act
            dateTimeStart = DateTime.Now;
            dateTimeEnd = dateTimeStart.AddSeconds(5);

            long count = 0;

            while (DateTime.Now < dateTimeEnd)
            {
                foo = valueInput.ConvertTo<Guid>();
                count++;
            }

            //// Assert
            Assert.That(count, Is.GreaterThanOrEqualTo(expected));
            Assert.Pass(count.ToString(CultureInfo.InvariantCulture) + " baseline: " + baselineCount.ToString(CultureInfo.InvariantCulture));
        }

        [Test]
        public void LoopingConvertToNullableULong_ValidIntInput_PerformsSufficientlyFast()
        {
            //// Setup
            int valueInput = 123;

            DateTime dateTimeStart = DateTime.Now;
            DateTime dateTimeEnd = dateTimeStart.AddSeconds(5);
            ulong? foo = 0;
            long baselineCount = 0;
            while (DateTime.Now < dateTimeEnd)
            {
                foo = (ulong?)valueInput;
                baselineCount++;
            }

            // expected minimum number of conversions.
            int expected = 100000;

            //// Act
            dateTimeStart = DateTime.Now;
            dateTimeEnd = dateTimeStart.AddSeconds(5);

            long count = 0;

            while (DateTime.Now < dateTimeEnd)
            {
                foo = valueInput.ConvertTo<ulong>();
                count++;
            }

            //// Assert
            Assert.That(count, Is.GreaterThanOrEqualTo(expected));
            Assert.Pass(count.ToString(CultureInfo.InvariantCulture) + " baseline: " + baselineCount.ToString(CultureInfo.InvariantCulture));
        }

        [Test]
        public void LoopingConvertToShort_ValidStringInput_PerformsSufficientlyFast()
        {
            //// Setup
            string valueInput = "123";

            DateTime dateTimeStart = DateTime.Now;
            DateTime dateTimeEnd = dateTimeStart.AddSeconds(5);
            short foo = 0;
            long baselineCount = 0;
            while (DateTime.Now < dateTimeEnd)
            {
                foo = short.Parse(valueInput);
                baselineCount++;
            }

            // expected minimum number of conversions.
            int expected = 100000;

            //// Act
            dateTimeStart = DateTime.Now;
            dateTimeEnd = dateTimeStart.AddSeconds(5);

            long count = 0;

            while (DateTime.Now < dateTimeEnd)
            {
                foo = valueInput.ConvertTo<short>();
                count++;
            }

            //// Assert
            Assert.That(count, Is.GreaterThanOrEqualTo(expected));
            Assert.Pass(count.ToString(CultureInfo.InvariantCulture) + " baseline: " + baselineCount.ToString(CultureInfo.InvariantCulture));
        }

        [Test]
        public void LoopingConvertToULong_ValidIntInput_PerformsSufficientlyFast()
        {
            //// Setup
            int valueInput = 123;

            DateTime dateTimeStart = DateTime.Now;
            DateTime dateTimeEnd = dateTimeStart.AddSeconds(5);
            ulong foo = 0;
            long baselineCount = 0;
            while (DateTime.Now < dateTimeEnd)
            {
                foo = (ulong)valueInput;
                baselineCount++;
            }

            // expected minimum number of conversions.
            int expected = 100000;

            //// Act
            dateTimeStart = DateTime.Now;
            dateTimeEnd = dateTimeStart.AddSeconds(5);

            long count = 0;

            while (DateTime.Now < dateTimeEnd)
            {
                foo = valueInput.ConvertTo<ulong>();
                count++;
            }

            //// Assert
            Assert.That(count, Is.GreaterThanOrEqualTo(expected));
            Assert.Pass(count.ToString(CultureInfo.InvariantCulture) + " baseline: " + baselineCount.ToString(CultureInfo.InvariantCulture));
        }

        [Test]
        public void LoopingConvertToUShort_ValidStringInput_PerformsSufficientlyFast()
        {
            //// Setup
            string valueInput = "123";

            DateTime dateTimeStart = DateTime.Now;
            DateTime dateTimeEnd = dateTimeStart.AddSeconds(5);
            ushort foo = 0;
            long baselineCount = 0;
            while (DateTime.Now < dateTimeEnd)
            {
                foo = ushort.Parse(valueInput);
                baselineCount++;
            }

            // expected minimum number of conversions.
            int expected = 100000;

            //// Act
            dateTimeStart = DateTime.Now;
            dateTimeEnd = dateTimeStart.AddSeconds(5);

            long count = 0;

            while (DateTime.Now < dateTimeEnd)
            {
                foo = valueInput.ConvertTo<ushort>();
                count++;
            }

            //// Assert
            Assert.That(count, Is.GreaterThanOrEqualTo(expected));
            Assert.Pass(count.ToString(CultureInfo.InvariantCulture) + " baseline: " + baselineCount.ToString(CultureInfo.InvariantCulture));
        }

        #endregion Methods
    }
}