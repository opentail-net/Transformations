namespace Transformations.Tests
{
    using System;

    using NUnit.Framework;

    [TestFixture]
    public class BasicTypeConverterLegacyCoverageTests
    {
        [TestCase("A", true, 'A')]
        [TestCase("AB", true, 'A')]
        [TestCase("", false, '\0')]
        [TestCase(null, false, '\0')]
        public void TryToChar_CoversNullAndTruncationBranches(string? input, bool expectedSuccess, char expectedValue)
        {
            bool success = input.TryToChar(out char result);

            Assert.That(success, Is.EqualTo(expectedSuccess));
            Assert.That(result, Is.EqualTo(expectedValue));
        }

        [Test]
        public void TryToChar_DisallowTruncating_LongInputReturnsFalse()
        {
            bool success = "AB".TryToChar(out char result, defaultValue: 'Z', allowTruncating: false);

            Assert.That(success, Is.False);
            Assert.That(result, Is.EqualTo('Z'));
        }

        [TestCase("2024-06-20", true)]
        [TestCase("not-a-date", false)]
        [TestCase("", false)]
        public void TryToDateTime_FromString_CoversSuccessAndFailure(string input, bool expectedSuccess)
        {
            bool success = input.TryToDateTime(out DateTime result);

            Assert.That(success, Is.EqualTo(expectedSuccess));
            if (expectedSuccess)
            {
                Assert.That(result.Year, Is.EqualTo(2024));
            }
            else
            {
                Assert.That(result, Is.EqualTo(default(DateTime)));
            }
        }

        [TestCase(45200, true)]
        [TestCase(0, true)]
        public void TryToDateTime_FromExcelInt_CoversExcelBranch(int input, bool expectedSuccess)
        {
            bool success = input.TryToDateTime(out DateTime result, dateValueType: BasicTypeConverter.DateValueType.Excel);

            Assert.That(success, Is.EqualTo(expectedSuccess));
            Assert.That(result, Is.Not.EqualTo(default(DateTime)));
        }

        [TestCase(638500000000000000L, true)]
        [TestCase(long.MaxValue, false)]
        public void TryToDateTime_FromTicksLong_CoversTicksBranch(long input, bool expectedSuccess)
        {
            DateTime defaultDate = new DateTime(2000, 1, 1);
            bool success = input.TryToDateTime(out DateTime result, defaultDate, BasicTypeConverter.DateValueType.Ticks);

            Assert.That(success, Is.EqualTo(expectedSuccess));
            if (!expectedSuccess)
            {
                Assert.That(result, Is.EqualTo(defaultDate));
            }
        }

        [TestCase(BasicTypeConverter.DateValueType.Excel)]
        [TestCase(BasicTypeConverter.DateValueType.Ticks)]
        public void TryToDouble_FromDateTime_CoversBothDateValueTypeBranches(BasicTypeConverter.DateValueType valueType)
        {
            DateTime input = new DateTime(2024, 6, 20, 10, 30, 0);

            bool success = input.TryToDouble(out double result, dateValueType: valueType);

            Assert.That(success, Is.True);
            Assert.That(result, Is.Not.EqualTo(default(double)));
        }

        [TestCase("7f5f9f8a-9ebf-4a0d-9bbd-0a6be6f8fd77", true)]
        [TestCase("7f5f9f8a-9ebf-4a0d-9bbd-0a6be6f8fd77xxxxx", true)]
        [TestCase("short", false)]
        [TestCase("", false)]
        public void TryToGuid_CoversLengthAndParseBranches(string input, bool expectedSuccess)
        {
            Guid defaultGuid = Guid.Parse("11111111-1111-1111-1111-111111111111");
            bool success = input.TryToGuid(out Guid result, defaultGuid);

            Assert.That(success, Is.EqualTo(expectedSuccess));
            if (!expectedSuccess)
            {
                Assert.That(result, Is.EqualTo(defaultGuid));
            }
        }

        [Test]
        public void TrySetValue_Int_CoversValidInvalidAndDBNull()
        {
            bool successValid = BasicTypeConverter.TrySetValue("123", out int validResult);
            bool successInvalid = BasicTypeConverter.TrySetValue("abc", out int invalidResult);
            bool successDbNull = BasicTypeConverter.TrySetValue(DBNull.Value, out int dbNullResult);

            Assert.That(successValid, Is.True);
            Assert.That(validResult, Is.EqualTo(123));
            Assert.That(successInvalid, Is.False);
            Assert.That(invalidResult, Is.EqualTo(0));
            Assert.That(successDbNull, Is.False);
            Assert.That(dbNullResult, Is.EqualTo(0));
        }

        [Test]
        public void TrySetValue_IntWithDefault_UsesProvidedDefaultOnFailure()
        {
            bool success = BasicTypeConverter.TrySetValue("bad", 99, out int result);

            Assert.That(success, Is.False);
            Assert.That(result, Is.EqualTo(99));
        }

        [Test]
        public void TrySetValue_DateTimeWithDefault_UsesDefaultOnFailure()
        {
            DateTime fallback = new DateTime(2001, 1, 1);
            bool success = BasicTypeConverter.TrySetValue("bad-date", fallback, out DateTime result);

            Assert.That(success, Is.False);
            Assert.That(result, Is.EqualTo(fallback));
        }

        [Test]
        public void ConvertToGuid_ValidAndInvalidInputs()
        {
            Guid expected = Guid.Parse("7f5f9f8a-9ebf-4a0d-9bbd-0a6be6f8fd77");
            Guid valid = "7f5f9f8a-9ebf-4a0d-9bbd-0a6be6f8fd77".ToGuid();
            Guid invalid = "not-guid".ToGuid();

            Assert.That(valid, Is.EqualTo(expected));
            Assert.That(invalid, Is.EqualTo(Guid.Empty));
        }
    }
}
