namespace Transformations.Tests
{
    using System;

    using NUnit.Framework;

    [TestFixture]
    public class BitConvertorTests
    {
        #region ConvertBitsToBool

        [Test]
        public void ConvertBitsToBool_ValidBytes_ReturnsTrue()
        {
            //// Setup
            byte[] bytes = BitConverter.GetBytes(true);

            //// Act
            bool actual = bytes.ConvertBitsToBool(reverseLittleEndianIfApplicable: false);

            //// Assert
            Assert.That(actual, Is.True);
        }

        [Test]
        public void ConvertBitsToBool_NullBytes_ReturnsDefault()
        {
            //// Setup
            byte[] bytes = null!;
            const bool expected = false;

            //// Act
            bool actual = bytes.ConvertBitsToBool();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertBitsToBool_EmptyBytes_ReturnsDefault()
        {
            //// Setup
            byte[] bytes = Array.Empty<byte>();
            const bool expected = true;

            //// Act
            bool actual = bytes.ConvertBitsToBool(useDefault: true);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        #endregion ConvertBitsToBool

        #region ConvertBitsToInt

        [Test]
        public void ConvertBitsToInt_ValidBytes_ReturnsValue()
        {
            //// Setup
            byte[] bytes = BitConverter.GetBytes(42);

            //// Act
            int actual = bytes.ConvertBitsToInt(reverseLittleEndianIfApplicable: false);

            //// Assert
            Assert.That(actual, Is.EqualTo(42));
        }

        [Test]
        public void ConvertBitsToInt_NullBytes_ReturnsDefault()
        {
            //// Setup
            byte[] bytes = null!;
            const int expected = -1;

            //// Act
            int actual = bytes.ConvertBitsToInt(useDefault: -1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        #endregion ConvertBitsToInt

        #region ConvertBitsToLong

        [Test]
        public void ConvertBitsToLong_ValidBytes_ReturnsValue()
        {
            //// Setup
            byte[] bytes = BitConverter.GetBytes(123456789L);

            //// Act
            long actual = bytes.ConvertBitsToLong(reverseLittleEndianIfApplicable: false);

            //// Assert
            Assert.That(actual, Is.EqualTo(123456789L));
        }

        [Test]
        public void ConvertBitsToLong_NullBytes_ReturnsDefault()
        {
            //// Setup
            byte[] bytes = null!;
            const long expected = 0;

            //// Act
            long actual = bytes.ConvertBitsToLong();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        #endregion ConvertBitsToLong

        #region ConvertBitsToShort

        [Test]
        public void ConvertBitsToShort_ValidBytes_ReturnsValue()
        {
            //// Setup
            byte[] bytes = BitConverter.GetBytes((short)42);

            //// Act
            short actual = bytes.ConvertBitsToShort(reverseLittleEndianIfApplicable: false);

            //// Assert
            Assert.That(actual, Is.EqualTo((short)42));
        }

        [Test]
        public void ConvertBitsToShort_NullBytes_ReturnsDefault()
        {
            //// Setup
            byte[] bytes = null!;

            //// Act
            short actual = bytes.ConvertBitsToShort();

            //// Assert
            Assert.That(actual, Is.EqualTo((short)0));
        }

        #endregion ConvertBitsToShort

        #region ConvertBitsToDouble

        [Test]
        public void ConvertBitsToDouble_ValidBytes_ReturnsValue()
        {
            //// Setup
            byte[] bytes = BitConverter.GetBytes(3.14);

            //// Act
            double actual = bytes.ConvertBitsToDouble(reverseLittleEndianIfApplicable: false);

            //// Assert
            Assert.That(actual, Is.EqualTo(3.14).Within(0.001));
        }

        [Test]
        public void ConvertBitsToDouble_NullBytes_ReturnsDefault()
        {
            //// Setup
            byte[] bytes = null!;

            //// Act
            double actual = bytes.ConvertBitsToDouble();

            //// Assert
            Assert.That(actual, Is.EqualTo(0.0));
        }

        #endregion ConvertBitsToDouble

        #region ConvertBitsToFloat

        [Test]
        public void ConvertBitsToFloat_ValidBytes_ReturnsValue()
        {
            //// Setup
            byte[] bytes = BitConverter.GetBytes(1.5f);

            //// Act
            float actual = bytes.ConvertBitsToFloat(reverseLittleEndianIfApplicable: false);

            //// Assert
            Assert.That(actual, Is.EqualTo(1.5f).Within(0.01));
        }

        #endregion ConvertBitsToFloat

        #region ConvertBitsToString

        [Test]
        public void ConvertBitsToString_ValidBytes_ReturnsHexString()
        {
            //// Setup
            byte[] bytes = { 0xAB, 0xCD };

            //// Act
            string actual = bytes.ConvertBitsToString(reverseLittleEndianIfApplicable: false);

            //// Assert
            Assert.That(actual, Is.Not.Empty);
        }

        [Test]
        public void ConvertBitsToString_NullBytes_ReturnsDefault()
        {
            //// Setup
            byte[] bytes = null!;
            string expected = "N/A";

            //// Act
            string actual = bytes.ConvertBitsToString(useDefault: "N/A");

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        #endregion ConvertBitsToString

        #region GetBytes round-trip

        [Test]
        public void GetBytes_Bool_RoundTrips()
        {
            //// Setup
            bool value = true;

            //// Act
            byte[] bytes = value.GetBytes();
            bool actual = bytes.ConvertBitsToBool(reverseLittleEndianIfApplicable: false);

            //// Assert
            Assert.That(actual, Is.EqualTo(value));
        }

        #endregion GetBytes round-trip
    }
}
