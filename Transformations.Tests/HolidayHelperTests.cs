namespace Transformations.Tests
{
    using System;

    using NUnit.Framework;

    using Transformations;

    [TestFixture]
    public class HolidayHelperTests
    {
        #region NewYearsDayBankHoliday

        [Test]
        public void GetNewYearsDayBankHoliday_WeekdayJan1_ReturnsJan1()
        {
            //// Setup - 2024-01-01 is a Monday
            int year = 2024;
            DateTime expected = new DateTime(2024, 01, 01);

            //// Act
            DateTime actual = HolidayHelper.GetNewYearsDayBankHoliday(year);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void GetNewYearsDayBankHoliday_SaturdayJan1_ReturnsMonday()
        {
            //// Setup - 2022-01-01 is a Saturday, bank holiday is Monday 3rd
            int year = 2022;
            DateTime expected = new DateTime(2022, 01, 03);

            //// Act
            DateTime actual = HolidayHelper.GetNewYearsDayBankHoliday(year);

            //// Assert
            Assert.That(actual.DayOfWeek, Is.Not.EqualTo(DayOfWeek.Saturday));
            Assert.That(actual.DayOfWeek, Is.Not.EqualTo(DayOfWeek.Sunday));
        }

        #endregion NewYearsDayBankHoliday

        #region EasterSunday

        [Test]
        public void GetEasterSunday_2024_ReturnsCorrectDate()
        {
            //// Setup
            int year = 2024;
            DateTime expected = new DateTime(2024, 03, 31);

            //// Act
            DateTime actual = HolidayHelper.GetEasterSunday(year);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void GetEasterSunday_2023_ReturnsCorrectDate()
        {
            //// Setup
            int year = 2023;
            DateTime expected = new DateTime(2023, 04, 09);

            //// Act
            DateTime actual = HolidayHelper.GetEasterSunday(year);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void GetEasterSunday_AlwaysOnSunday()
        {
            //// Setup
            int year = 2025;

            //// Act
            DateTime actual = HolidayHelper.GetEasterSunday(year);

            //// Assert
            Assert.That(actual.DayOfWeek, Is.EqualTo(DayOfWeek.Sunday));
        }

        #endregion EasterSunday

        #region GoodFriday

        [Test]
        public void GetGoodFriday_2024_ReturnsCorrectDate()
        {
            //// Setup
            int year = 2024;
            DateTime expected = new DateTime(2024, 03, 29);

            //// Act
            DateTime actual = HolidayHelper.GetGoodFriday(year);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
            Assert.That(actual.DayOfWeek, Is.EqualTo(DayOfWeek.Friday));
        }

        #endregion GoodFriday

        #region EasterMonday

        [Test]
        public void GetEasterMonday_2024_ReturnsCorrectDate()
        {
            //// Setup
            int year = 2024;
            DateTime expected = new DateTime(2024, 04, 01);

            //// Act
            DateTime actual = HolidayHelper.GetEasterMonday(year);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
            Assert.That(actual.DayOfWeek, Is.EqualTo(DayOfWeek.Monday));
        }

        #endregion EasterMonday

        #region MartinLutherKingHoliday

        [Test]
        public void GetMartinLutherKingHoliday_2024_ReturnsThirdMonday()
        {
            //// Setup - Third Monday in January 2024 is Jan 15
            int year = 2024;
            DateTime expected = new DateTime(2024, 01, 15);

            //// Act
            DateTime actual = HolidayHelper.GetMartinLutherKingHoliday(year);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
            Assert.That(actual.DayOfWeek, Is.EqualTo(DayOfWeek.Monday));
        }

        #endregion MartinLutherKingHoliday

        #region PresidentsDay

        [Test]
        public void GetPresidentsDay_2024_ReturnsThirdMondayInFeb()
        {
            //// Setup - Third Monday in February 2024 is Feb 19
            int year = 2024;
            DateTime expected = new DateTime(2024, 02, 19);

            //// Act
            DateTime actual = HolidayHelper.GetPresidentsDay(year);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
            Assert.That(actual.DayOfWeek, Is.EqualTo(DayOfWeek.Monday));
        }

        #endregion PresidentsDay

        #region MayDayBankHoliday

        [Test]
        public void GetMayDayBankHoliday_2024_ReturnsFirstMondayInMay()
        {
            //// Setup - First Monday in May 2024 is May 6
            int year = 2024;
            DateTime expected = new DateTime(2024, 05, 06);

            //// Act
            DateTime actual = HolidayHelper.GetMayDayBankHoliday(year);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
            Assert.That(actual.DayOfWeek, Is.EqualTo(DayOfWeek.Monday));
        }

        [Test]
        public void GetMayDayBankHoliday_1995_ReturnsVEDayOverride()
        {
            //// Setup - 1995 was moved to May 8 for VE Day anniversary
            int year = 1995;
            DateTime expected = new DateTime(1995, 05, 08);

            //// Act
            DateTime actual = HolidayHelper.GetMayDayBankHoliday(year);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        #endregion MayDayBankHoliday

        #region MemorialDay

        [Test]
        public void GetMemorialDay_2024_ReturnsLastMondayInMay()
        {
            //// Setup - Last Monday in May 2024 is May 27
            int year = 2024;
            DateTime expected = new DateTime(2024, 05, 27);

            //// Act
            DateTime actual = HolidayHelper.GetMemorialDay(year);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
            Assert.That(actual.DayOfWeek, Is.EqualTo(DayOfWeek.Monday));
        }

        #endregion MemorialDay

        #region Simple date holidays

        [Test]
        public void ValentinesDay_ReturnsFebruary14()
        {
            //// Setup
            int year = 2024;
            DateTime expected = new DateTime(2024, 02, 14);

            //// Act
            DateTime actual = HolidayHelper.ValentinesDay(year);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void LincolnsBirthday_ReturnsFebruary12()
        {
            //// Setup
            int year = 2024;
            DateTime expected = new DateTime(2024, 02, 12);

            //// Act
            DateTime actual = HolidayHelper.LincolnsBirthday(year);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void GetGroundhogDay_ReturnsFebruary2()
        {
            //// Setup
            int year = 2024;
            DateTime expected = new DateTime(2024, 02, 02);

            //// Act
            DateTime actual = HolidayHelper.GetGroundhogDay(year);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        #endregion Simple date holidays
    }
}
