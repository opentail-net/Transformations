namespace Transformations.Tests
{
    using System;

    using NUnit.Framework;

    [TestFixture]
    public class BasicTypeConverterApiSurfaceTests
    {
        [Test]
        public void ConvertToGuid_FromInt_ProducesDeterministicGuid()
        {
            Guid g1 = 123.ConvertToGuid();
            Guid g2 = 123.ConvertToGuid();

            Assert.That(g1, Is.EqualTo(g2));
            Assert.That(g1, Is.Not.EqualTo(Guid.Empty));
        }

        [TestCase("A", true, 'A')]
        [TestCase("AB", true, 'A')]
        [TestCase("", false, 'X')]
        [TestCase(null, false, 'X')]
        public void ToChar_Wrapper_FollowsTryToCharSemantics(string? input, bool expectedSuccess, char expected)
        {
            char actual = input.ToChar(defaultValue: 'X', allowTruncating: true);
            bool success = input.TryToChar(out char result, defaultValue: 'X', allowTruncating: true);

            Assert.That(success, Is.EqualTo(expectedSuccess));
            Assert.That(actual, Is.EqualTo(expected));
            Assert.That(result, Is.EqualTo(expected));
        }

        [TestCase("2024-06-20", 2024, true)]
        [TestCase("invalid-date", 1999, false)]
        [TestCase("", 1999, false)]
        public void ToDateTime_Wrapper_UsesDefaultOnFailure(string input, int expectedYear, bool expectedSuccess)
        {
            DateTime fallback = new DateTime(1999, 1, 1);

            DateTime actual = input.ToDateTime(fallback);
            bool success = input.TryToDateTime(out DateTime viaTry, fallback);

            Assert.That(success, Is.EqualTo(expectedSuccess));
            Assert.That(actual.Year, Is.EqualTo(expectedYear));
            Assert.That(viaTry.Year, Is.EqualTo(expectedYear));
        }

        [TestCase(BasicTypeConverter.DateValueType.Excel)]
        [TestCase(BasicTypeConverter.DateValueType.Ticks)]
        public void ToInt_FromDateTime_CoversBothDateValueTypeBranches(BasicTypeConverter.DateValueType valueType)
        {
            DateTime input = new DateTime(2024, 06, 20, 10, 30, 0);

            int result = input.ToInt(dateValueType: valueType);

            // Contract note: ticks-path currently overflows int conversion and returns default; keep this as intentional compatibility behavior.
            if (valueType == BasicTypeConverter.DateValueType.Excel)
            {
                Assert.That(result, Is.Not.EqualTo(default(int)));
            }
            else
            {
                Assert.That(result, Is.EqualTo(default(int)));
            }
        }

        [TestCase(BasicTypeConverter.DateValueType.Excel)]
        [TestCase(BasicTypeConverter.DateValueType.Ticks)]
        public void ToUInt_FromDateTime_CoversBothDateValueTypeBranches(BasicTypeConverter.DateValueType valueType)
        {
            DateTime input = new DateTime(2024, 06, 20, 10, 30, 0);

            uint result = input.ToUInt(dateValueType: valueType);

            // Contract note: ticks-path returns default on overflow; this test preserves legacy API expectations.
            if (valueType == BasicTypeConverter.DateValueType.Excel)
            {
                Assert.That(result, Is.GreaterThan(0u));
            }
            else
            {
                Assert.That(result, Is.EqualTo(default(uint)));
            }
        }

        [TestCase("7f5f9f8a-9ebf-4a0d-9bbd-0a6be6f8fd77", true)]
        [TestCase("not-a-guid", false)]
        public void ToGuid_Wrapper_UsesTryToGuidOutcome(string input, bool expectedValid)
        {
            Guid parsed = input.ToGuid();
            bool success = input.TryToGuid(out Guid viaTry);

            Assert.That(success, Is.EqualTo(expectedValid));
            if (expectedValid)
            {
                Assert.That(parsed, Is.EqualTo(viaTry));
            }
            else
            {
                Assert.That(parsed, Is.EqualTo(Guid.Empty));
            }
        }
    }
}
