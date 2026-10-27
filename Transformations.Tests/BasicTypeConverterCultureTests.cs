namespace Transformations.Tests
{
    using System;
    using System.Globalization;

    using NUnit.Framework;

    [TestFixture]
    public class BasicTypeConverterCultureTests
    {
        [TestCase("1.234,56", "de-DE", true, 1234.56)]
        [TestCase("1,234.56", "en-GB", true, 1234.56)]
        [TestCase("1,234.56", "de-DE", false, 0.0)]
        [TestCase("1.234,56", "en-GB", false, 0.0)]
        public void TryConvertTo_WithProvider_DecimalSeparatorsFollowTheProvider(string input, string culture, bool expectedSuccess, double expectedValue)
        {
            IFormatProvider provider = CultureInfo.GetCultureInfo(culture);

            bool success = input.TryConvertTo<decimal>(provider, out decimal result);

            Assert.That(success, Is.EqualTo(expectedSuccess));
            if (expectedSuccess)
            {
                Assert.That(result, Is.EqualTo((decimal)expectedValue));
            }
        }

        [TestCase("1,5", "de-DE", 1.5)]
        [TestCase("1.5", "en-GB", 1.5)]
        public void TryConvertTo_WithProvider_DoubleParsesWithTheSuppliedCulture(string input, string culture, double expected)
        {
            IFormatProvider provider = CultureInfo.GetCultureInfo(culture);

            Assert.That(input.TryConvertTo<double>(provider, out double result), Is.True);
            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void TryConvertTo_CultureFreeOverload_UsesCurrentCulture()
        {
            CultureInfo previous = CultureInfo.CurrentCulture;
            try
            {
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("de-DE");

                Assert.That("1,5".TryConvertTo<double>(out double viaCurrent), Is.True);
                Assert.That(viaCurrent, Is.EqualTo(1.5));
                Assert.That("1,5".TryConvertTo<double>(CultureInfo.GetCultureInfo("de-DE"), out double viaProvider), Is.True);
                Assert.That(viaProvider, Is.EqualTo(viaCurrent));
            }
            finally
            {
                CultureInfo.CurrentCulture = previous;
            }
        }

        [TestCase("yes", 1)]
        [TestCase("no", 0)]
        [TestCase("t", 1)]
        [TestCase("f", 0)]
        public void TryConvertTo_WithProvider_DefaultOverload_KeepsWordFallback(string input, int expected)
        {
            IFormatProvider provider = CultureInfo.GetCultureInfo("en-GB");

            Assert.That(input.TryConvertTo<int>(provider, null, out int? converted), Is.True);
            Assert.That(converted, Is.EqualTo(expected));
        }

        [Test]
        public void TryConvertTo_WithProvider_DefaultOverload_FailureReturnsTheDefault()
        {
            IFormatProvider provider = CultureInfo.GetCultureInfo("en-GB");

            bool success = "not a number".TryConvertTo<int>(provider, 99, out int? result);

            Assert.That(success, Is.False);
            Assert.That(result, Is.EqualTo(99));
        }

        [Test]
        public void TryConvertTo_WithProvider_NullInput_ReturnsFalse()
        {
            IFormatProvider provider = CultureInfo.GetCultureInfo("en-GB");

            Assert.That(((string?)null).TryConvertTo<decimal>(provider, out decimal result), Is.False);
            Assert.That(result, Is.EqualTo(0m));
        }

        [TestCase("04/05/2026", "dd/MM/yyyy", 2026, 5, 4)]
        [TestCase("04/05/2026", "MM/dd/yyyy", 2026, 4, 5)]
        public void TryToDateTimeExact_ParsesOnlyTheGivenFormat(string input, string format, int year, int month, int day)
        {
            IFormatProvider provider = CultureInfo.GetCultureInfo("en-GB");

            bool success = input.TryToDateTimeExact(provider, new[] { format }, out DateTime result);

            Assert.That(success, Is.True);
            Assert.That(result, Is.EqualTo(new DateTime(year, month, day)));
        }

        [TestCase("2026-05-04", "dd/MM/yyyy")]
        [TestCase("32/01/2026", "dd/MM/yyyy")]
        public void TryToDateTimeExact_NonMatchingText_ReturnsFalse(string input, string format)
        {
            IFormatProvider provider = CultureInfo.GetCultureInfo("en-GB");

            bool success = input.TryToDateTimeExact(provider, new[] { format }, out DateTime result);

            Assert.That(success, Is.False);
            Assert.That(result, Is.EqualTo(default(DateTime)));
        }

        private static readonly System.Collections.Generic.Dictionary<string, int> MonthTable = new()
        {
            ["Jan"] = 1, ["Feb"] = 2, ["Mar"] = 3, ["Apr"] = 4, ["May"] = 5, ["Jun"] = 6,
            ["Jul"] = 7, ["Aug"] = 8, ["Sep"] = 9, ["Sept"] = 9, ["Oct"] = 10, ["Nov"] = 11, ["Dec"] = 12,
        };

        [TestCase("03 Sep 2026", "en-GB")]
        [TestCase("03 Sept 2026", "en-GB")]
        [TestCase("03 Sep 2026", "en-US")]
        [TestCase("03 Sept 2026", "en-US")]
        [TestCase("03 Sept 2026", "")]
        [TestCase("03 SEPT 2026", "en-GB")]
        [TestCase("03 sep 2026", "de-DE")]
        public void TryToDateTimeExact_MonthTable_IgnoresTheCulturesMonthNames(string input, string culture)
        {
            IFormatProvider provider = CultureInfo.GetCultureInfo(culture);

            bool success = input.TryToDateTimeExact(provider, new[] { "dd MMM yyyy" }, MonthTable, out DateTime result);

            Assert.That(success, Is.True);
            Assert.That(result, Is.EqualTo(new DateTime(2026, 9, 3)));
        }

        [TestCase("03 Sepp 2026")]
        [TestCase("03 Sep Oct 2026")]
        [TestCase("03 September 2026")]
        [TestCase("31 Sep 2026")]
        [TestCase("03/09/2026")]
        public void TryToDateTimeExact_MonthTable_UnknownOrInvalid_ReturnsFalse(string input)
        {
            IFormatProvider provider = CultureInfo.GetCultureInfo("en-GB");

            bool success = input.TryToDateTimeExact(provider, new[] { "dd MMM yyyy" }, MonthTable, out DateTime result);

            Assert.That(success, Is.False);
            Assert.That(result, Is.EqualTo(default(DateTime)));
        }

        [TestCase("03/09/2026", 2026, 9, 3)]
        [TestCase("03 Oct 2026", 2026, 10, 3)]
        public void TryToDateTimeExact_MonthTable_MixesNumericAndNamedFormats(string input, int year, int month, int day)
        {
            IFormatProvider provider = CultureInfo.GetCultureInfo("en-GB");

            bool success = input.TryToDateTimeExact(provider, new[] { "dd/MM/yyyy", "dd MMM yyyy" }, MonthTable, out DateTime result);

            Assert.That(success, Is.True);
            Assert.That(result, Is.EqualTo(new DateTime(year, month, day)));
        }

        [Test]
        public void TryToDateTimeExact_MonthTable_InvalidTable_Throws()
        {
            IFormatProvider provider = CultureInfo.GetCultureInfo("en-GB");
            string[] formats = { "dd MMM yyyy" };

            Assert.Throws<ArgumentException>(() => "03 Sep 2026".TryToDateTimeExact(provider, formats, new System.Collections.Generic.Dictionary<string, int> { ["Sep"] = 13 }, out _));
            Assert.Throws<ArgumentException>(() => "03 Sep 2026".TryToDateTimeExact(provider, formats, new System.Collections.Generic.Dictionary<string, int> { ["Sep"] = 9, ["SEP"] = 10 }, out _));
            Assert.Throws<ArgumentException>(() => "03 Sep 2026".TryToDateTimeExact(provider, formats, new System.Collections.Generic.Dictionary<string, int> { [" "] = 9 }, out _));
            Assert.Throws<ArgumentNullException>(() => "03 Sep 2026".TryToDateTimeExact(provider, formats, null!, out _));
        }

        [Test]
        public void TryToDateTimeExact_NullOrEmptyFormats_ReturnsFalse()
        {
            IFormatProvider provider = CultureInfo.GetCultureInfo("en-GB");

            Assert.That("04/05/2026".TryToDateTimeExact(provider, Array.Empty<string>(), out DateTime none), Is.False);
            Assert.That(none, Is.EqualTo(default(DateTime)));
            Assert.That(((string?)null).TryToDateTimeExact(provider, new[] { "dd/MM/yyyy" }, out DateTime nullInput), Is.False);
            Assert.That(nullInput, Is.EqualTo(default(DateTime)));
        }
    }
}
