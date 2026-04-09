namespace Transformations.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using NUnit.Framework;

    using Transformations;

    [TestFixture]
    public class HolidayHelperCoverageTests
    {
        [TestCase(1977, 6, 4)]
        [TestCase(2002, 6, 4)]
        [TestCase(2012, 6, 4)]
        [TestCase(2024, 5, 31)]
        public void GetSpringBankHoliday_CoversSpecialAndDefaultBranches(int year, int expectedMonth, int expectedDay)
        {
            DateTime result = HolidayHelper.GetSpringBankHoliday(year);

            Assert.That(result.Year, Is.EqualTo(year));
            Assert.That(result.Month, Is.EqualTo(expectedMonth));
            Assert.That(result.Day, Is.EqualTo(expectedDay));
        }

        [TestCase(1963, 8, 5, DayOfWeek.Monday)]
        [TestCase(2024, 8, 30, DayOfWeek.Friday)]
        public void GetLateSummerBankHoliday_CoversYearBoundaryBranches(int year, int expectedMonth, int expectedDay, DayOfWeek expectedDayOfWeek)
        {
            DateTime result = HolidayHelper.GetLateSummerBankHoliday(year);

            Assert.That(result.Year, Is.EqualTo(year));
            Assert.That(result.Month, Is.EqualTo(expectedMonth));
            Assert.That(result.Day, Is.EqualTo(expectedDay));
            Assert.That(result.DayOfWeek, Is.EqualTo(expectedDayOfWeek));
        }

        [TestCase(2024, 9, 2)]
        [TestCase(2025, 9, 1)]
        public void GetLabourDay_ReturnsFirstMondayInSeptember(int year, int month, int day)
        {
            DateTime result = HolidayHelper.GetLabourDay(year);

            Assert.That(result, Is.EqualTo(new DateTime(year, month, day)));
            Assert.That(result.DayOfWeek, Is.EqualTo(DayOfWeek.Monday));
        }

        [TestCase(2024, 10, 14)]
        [TestCase(2025, 10, 13)]
        public void GetColumbusDay_ReturnsSecondMondayInOctober(int year, int month, int day)
        {
            DateTime result = HolidayHelper.GetColumbusDay(year);

            Assert.That(result, Is.EqualTo(new DateTime(year, month, day)));
            Assert.That(result.DayOfWeek, Is.EqualTo(DayOfWeek.Monday));
        }

        [TestCase(2024, 11, 11)]
        [TestCase(2025, 11, 11)]
        public void GetVeteransDay_ReturnsFixedDate(int year, int month, int day)
        {
            DateTime result = HolidayHelper.GetVeteransDay(year);

            Assert.That(result, Is.EqualTo(new DateTime(year, month, day)));
        }

        [TestCase(2024, 11, 28)]
        [TestCase(2025, 11, 27)]
        public void GetThanksgivingDay_ReturnsFourthThursday(int year, int month, int day)
        {
            DateTime result = HolidayHelper.GetThanksgivingDay(year);

            Assert.That(result, Is.EqualTo(new DateTime(year, month, day)));
            Assert.That(result.DayOfWeek, Is.EqualTo(DayOfWeek.Thursday));
        }

        [Test]
        public void GetBoxingAndXmasBankHolidays_AreWeekdaysAndNotSameDay()
        {
            DateTime boxing = HolidayHelper.GetBoxingDayBankHoliday();
            DateTime xmas = HolidayHelper.GetXmasDayBankHoliday();

            Assert.That(boxing.DayOfWeek, Is.Not.EqualTo(DayOfWeek.Saturday));
            Assert.That(boxing.DayOfWeek, Is.Not.EqualTo(DayOfWeek.Sunday));
            Assert.That(xmas.DayOfWeek, Is.Not.EqualTo(DayOfWeek.Saturday));
            Assert.That(xmas.DayOfWeek, Is.Not.EqualTo(DayOfWeek.Sunday));
            Assert.That(xmas.Date, Is.Not.EqualTo(boxing.Date));
        }

        [Test]
        public void GetEnglishBankHolidays_Year1977_DoesNotIncludeMayDayButIncludesJubilee()
        {
            List<DateTime> holidays = HolidayHelper.GetEnglishBankHolidays(1977);

            Assert.That(holidays.Any(d => d.Month == 5 && d.Day == 1), Is.False);
            Assert.That(holidays.Any(d => d.Date == new DateTime(1977, 6, 7)), Is.True);
        }

        [Test]
        public void GetEnglishBankHolidays_Year2024_IncludesMayDayAndMainBankHolidays()
        {
            List<DateTime> holidays = HolidayHelper.GetEnglishBankHolidays(2024);

            Assert.That(holidays.Any(d => d.Date == new DateTime(2024, 5, 6)), Is.True);
            Assert.That(holidays.Any(d => d.Date == new DateTime(2024, 12, 25)), Is.True);
            Assert.That(holidays.Any(d => d.Date == new DateTime(2024, 12, 26)), Is.True);
        }

        [Test]
        public void GetInaugurationDayList_StartsAt1940AndContainsCurrentYear()
        {
            List<DateTime> dates = HolidayHelper.GetInaugurationDayList();

            Assert.That(dates.First(), Is.EqualTo(new DateTime(1940, 1, 20)));
            Assert.That(dates.Last().Year, Is.EqualTo(DateTime.Now.Year));
            Assert.That(dates.All(d => d.Month == 1 && d.Day == 20), Is.True);
        }

        [TestCase("2024-12-23", "2024-12-31", 4)]
        [TestCase("2024-12-30", "2025-01-03", 3)]
        [TestCase("2024-01-01", "2024-01-01", 0)]
        [TestCase("1899-12-31", "1900-01-02", 0)]
        public void GetEnglishWorkingDaysCount_CoversWeekendHolidayAndBoundaryBranches(string startValue, string endValue, int expected)
        {
            DateTime start = DateTime.Parse(startValue);
            DateTime end = DateTime.Parse(endValue);

            int actual = HolidayHelper.GetEnglishWorkingDaysCount(start, end);

            Assert.That(actual, Is.EqualTo(expected));
        }
    }
}
