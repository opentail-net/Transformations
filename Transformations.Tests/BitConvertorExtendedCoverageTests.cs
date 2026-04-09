namespace Transformations.Tests
{
    using NUnit.Framework;

    [TestFixture]
    public class BitConvertorExtendedCoverageTests
    {
        [Test]
        public void TryConvertBitsTo_Primitives_ValidBytes_ReturnTrueAndExpectedValues()
        {
            Assert.That(BitConverter.GetBytes(true).TryConvertBitsToBool(out bool b, reverseLittleEndianIfApplicable: false), Is.True);
            Assert.That(b, Is.True);

            Assert.That(BitConverter.GetBytes('Z').TryConvertBitsToChar(out char c, reverseLittleEndianIfApplicable: false), Is.True);
            Assert.That(c, Is.EqualTo('Z'));

            Assert.That(BitConverter.GetBytes(12.5d).TryConvertBitsToDouble(out double d, reverseLittleEndianIfApplicable: false), Is.True);
            Assert.That(d, Is.EqualTo(12.5d));

            Assert.That(BitConverter.GetBytes(9.25f).TryConvertBitsToFloat(out float f, reverseLittleEndianIfApplicable: false), Is.True);
            Assert.That(f, Is.EqualTo(9.25f));

            Assert.That(BitConverter.GetBytes(42).TryConvertBitsToInt(out int i, reverseLittleEndianIfApplicable: false), Is.True);
            Assert.That(i, Is.EqualTo(42));

            Assert.That(BitConverter.GetBytes(4242L).TryConvertBitsToLong(out long l, reverseLittleEndianIfApplicable: false), Is.True);
            Assert.That(l, Is.EqualTo(4242L));

            Assert.That(BitConverter.GetBytes((short)7).TryConvertBitsToShort(out short s, reverseLittleEndianIfApplicable: false), Is.True);
            Assert.That(s, Is.EqualTo((short)7));

            Assert.That(BitConverter.GetBytes(42u).TryConvertBitsToUInt(out uint ui, reverseLittleEndianIfApplicable: false), Is.True);
            Assert.That(ui, Is.EqualTo(42u));

            Assert.That(BitConverter.GetBytes(4242UL).TryConvertBitsToULong(out ulong ul, reverseLittleEndianIfApplicable: false), Is.True);
            Assert.That(ul, Is.EqualTo(4242UL));

            Assert.That(BitConverter.GetBytes((ushort)17).TryConvertBitsToUShort(out ushort us, reverseLittleEndianIfApplicable: false), Is.True);
            Assert.That(us, Is.EqualTo((ushort)17));

            byte[] bytesForString = new byte[] { 0x01, 0x02, 0x03 };
            Assert.That(bytesForString.TryConvertBitsToString(out string str, reverseLittleEndianIfApplicable: false), Is.True);
            Assert.That(str, Is.EqualTo("01-02-03"));
        }

        [Test]
        public void TryConvertBitsTo_Primitives_NullOrEmpty_ReturnFalseAndDefaults()
        {
            byte[] empty = Array.Empty<byte>();
            byte[]? nullBytes = null;

            Assert.That(empty.TryConvertBitsToBool(out bool b, useDefault: true), Is.False);
            Assert.That(b, Is.True);

            Assert.That(nullBytes.TryConvertBitsToChar(out char c, useDefault: 'X'), Is.False);
            Assert.That(c, Is.EqualTo('X'));

            Assert.That(empty.TryConvertBitsToDouble(out double d, useDefault: -1d), Is.False);
            Assert.That(d, Is.EqualTo(-1d));

            Assert.That(nullBytes.TryConvertBitsToFloat(out float f, useDefault: -1f), Is.False);
            Assert.That(f, Is.EqualTo(-1f));

            Assert.That(empty.TryConvertBitsToInt(out int i, useDefault: -1), Is.False);
            Assert.That(i, Is.EqualTo(-1));

            Assert.That(nullBytes.TryConvertBitsToLong(out long l, useDefault: -1L), Is.False);
            Assert.That(l, Is.EqualTo(-1L));

            Assert.That(empty.TryConvertBitsToShort(out short s, useDefault: -1), Is.False);
            Assert.That(s, Is.EqualTo(-1));

            Assert.That(nullBytes.TryConvertBitsToString(out string str, useDefault: "fallback"), Is.False);
            Assert.That(str, Is.EqualTo("fallback"));

            Assert.That(empty.TryConvertBitsToUInt(out uint ui, useDefault: 9), Is.False);
            Assert.That(ui, Is.EqualTo(9u));

            Assert.That(nullBytes.TryConvertBitsToULong(out ulong ul, useDefault: 9), Is.False);
            Assert.That(ul, Is.EqualTo(9UL));

            Assert.That(empty.TryConvertBitsToUShort(out ushort us, useDefault: 9), Is.False);
            Assert.That(us, Is.EqualTo((ushort)9));
        }

        [Test]
        public void ConvertBitsTo_UnsignedOverloads_AreExercised()
        {
            uint ui = BitConverter.GetBytes(123u).ConvertBitsToUInt(reverseLittleEndianIfApplicable: false);
            ulong ul = BitConverter.GetBytes(999UL).ConvertBitsToULong(reverseLittleEndianIfApplicable: false);
            ushort us = BitConverter.GetBytes((ushort)5).ConvertBitsToUShort(reverseLittleEndianIfApplicable: false);

            Assert.That(ui, Is.EqualTo(123u));
            Assert.That(ul, Is.EqualTo(999UL));
            Assert.That(us, Is.EqualTo((ushort)5));
        }

        [Test]
        public void ConvertBitsToChar_ValidAndInvalidPaths_AreCovered()
        {
            char valid = BitConverter.GetBytes('Q').ConvertBitsToChar(reverseLittleEndianIfApplicable: false);
            char fallback = ((byte[]?)null).ConvertBitsToChar(useDefault: 'X');

            Assert.That(valid, Is.EqualTo('Q'));
            Assert.That(fallback, Is.EqualTo('X'));
        }
    }
}
