using NUnit.Framework;

namespace Transformations.Tests
{
    [TestFixture]
    public class EnglishBankHolidayRegressionTests
    {
        [TestCase(2019, "2019-01-01,2019-04-19,2019-04-22,2019-05-06,2019-05-27,2019-08-26,2019-12-25,2019-12-26")]
        [TestCase(2020, "2020-01-01,2020-04-10,2020-04-13,2020-05-08,2020-05-25,2020-08-31,2020-12-25,2020-12-28")]
        [TestCase(2021, "2021-01-01,2021-04-02,2021-04-05,2021-05-03,2021-05-31,2021-08-30,2021-12-27,2021-12-28")]
        [TestCase(2022, "2022-01-03,2022-04-15,2022-04-18,2022-05-02,2022-06-02,2022-06-03,2022-08-29,2022-09-19,2022-12-26,2022-12-27")]
        [TestCase(2023, "2023-01-02,2023-04-07,2023-04-10,2023-05-01,2023-05-08,2023-05-29,2023-08-28,2023-12-25,2023-12-26")]
        [TestCase(2024, "2024-01-01,2024-03-29,2024-04-01,2024-05-06,2024-05-27,2024-08-26,2024-12-25,2024-12-26")]
        [TestCase(2025, "2025-01-01,2025-04-18,2025-04-21,2025-05-05,2025-05-26,2025-08-25,2025-12-25,2025-12-26")]
        [TestCase(2026, "2026-01-01,2026-04-03,2026-04-06,2026-05-04,2026-05-25,2026-08-31,2026-12-25,2026-12-28")]
        [TestCase(2027, "2027-01-01,2027-03-26,2027-03-29,2027-05-03,2027-05-31,2027-08-30,2027-12-27,2027-12-28")]
        public void GetEnglishBankHolidays_MatchesOfficialEnglandAndWalesList(int year, string expected)
        {
            var actual = HolidayHelper.GetEnglishBankHolidays(year)
                .Select(d => d.ToString("yyyy-MM-dd"))
                .OrderBy(x => x)
                .ToArray();

            Assert.That(actual, Is.EqualTo(expected.Split(',')));
        }
    }
}

namespace Transformations.Tests
{
    [TestFixture]
    public class ReadableTimeStringRegressionTests
    {
        [Test]
        public void ShorthandTime_SpanOfADayOrMore_DoesNotThrow()
        {
            var span = new TimeSpan(1, 3, 5, 9, 250);
            Assert.That(span.ToReadableTimeString(TimeSpanHelper.OutputFormat.ShorthandTime), Is.EqualTo("1days 03h:05m:09s:250ms"));
        }

        [TestCase(250, "250ms")]
        [TestCase(5, "5ms")]
        [TestCase(999, "999ms")]
        public void VerboseFormats_ReportRealMilliseconds(int ms, string expected)
        {
            var span = TimeSpan.FromMilliseconds(ms);
            Assert.That(span.ToReadableTimeString(TimeSpanHelper.OutputFormat.VerboseTotalTime), Is.EqualTo(expected));
            Assert.That(span.ToReadableTimeString(TimeSpanHelper.OutputFormat.VerboseTime), Is.EqualTo(expected));
        }

        [Test]
        public void VerboseTime_IncludesMillisecondsAfterSeconds()
        {
            var span = new TimeSpan(0, 2, 30, 15, 250);
            Assert.That(span.ToReadableTimeString(TimeSpanHelper.OutputFormat.VerboseTime), Is.EqualTo("2 hours 30 minutes 15 seconds 250ms"));
        }

        [Test]
        public void HoursMinutesFormats_DoNotDropWholeDays()
        {
            var span = new TimeSpan(1, 3, 5, 9);
            Assert.That(span.ToReadableTimeString(TimeSpanHelper.OutputFormat.hhmm), Is.EqualTo("27:05"));
            Assert.That(span.ToReadableTimeString(TimeSpanHelper.OutputFormat.hhmmss), Is.EqualTo("27:05:09"));
            Assert.That(new TimeSpan(2, 30, 15).ToReadableTimeString(TimeSpanHelper.OutputFormat.hhmm), Is.EqualTo("02:30"));
        }
    }
}
