namespace Transformations.Tests
{
    using System;

    using NUnit.Framework;

    using Transformations;

    [TestFixture]
    public class DateHelperTests
    {
        #region CalculateAge

        [Test]
        public void CalculateAge_KnownAge_ReturnsCorrectAge()
        {
            //// Setup
            DateTime dateOfBirth = new DateTime(1990, 06, 15);
            DateTime referenceDate = new DateTime(2024, 06, 14);
            int expected = 33;

            //// Act
            int actual = dateOfBirth.CalculateAge(referenceDate);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void CalculateAge_BirthdayToday_ReturnsNewAge()
        {
            //// Setup
            DateTime dateOfBirth = new DateTime(1990, 06, 15);
            DateTime referenceDate = new DateTime(2024, 06, 15);
            int expected = 34;

            //// Act
            int actual = dateOfBirth.CalculateAge(referenceDate);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        #endregion CalculateAge

        #region CountDaysOfMonth

        [Test]
        public void CountDaysOfMonth_January_Returns31()
        {
            //// Setup
            DateTime date = new DateTime(2024, 01, 15);
            int expected = 31;

            //// Act
            int actual = date.CountDaysOfMonth();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void CountDaysOfMonth_FebruaryLeapYear_Returns29()
        {
            //// Setup
            DateTime date = new DateTime(2024, 02, 15);
            int expected = 29;

            //// Act
            int actual = date.CountDaysOfMonth();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void CountDaysOfMonth_FebruaryNonLeapYear_Returns28()
        {
            //// Setup
            DateTime date = new DateTime(2023, 02, 15);
            int expected = 28;

            //// Act
            int actual = date.CountDaysOfMonth();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        #endregion CountDaysOfMonth

        #region FirstOfMonth

        [Test]
        public void FirstOfMonth_MidMonth_ReturnsFirstDay()
        {
            //// Setup
            DateTime date = new DateTime(2024, 03, 15);
            DateTime expected = new DateTime(2024, 03, 01);

            //// Act
            DateTime actual = date.FirstOfMonth();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void FirstOfMonth_AlreadyFirstDay_ReturnsSame()
        {
            //// Setup
            DateTime date = new DateTime(2024, 03, 01);
            DateTime expected = new DateTime(2024, 03, 01);

            //// Act
            DateTime actual = date.FirstOfMonth();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        #endregion FirstOfMonth

        #region LastOfMonth

        [Test]
        public void LastOfMonth_January_Returns31st()
        {
            //// Setup
            DateTime date = new DateTime(2024, 01, 15);
            DateTime expected = new DateTime(2024, 01, 31);

            //// Act
            DateTime actual = date.LastOfMonth();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void LastOfMonth_FebruaryLeapYear_Returns29th()
        {
            //// Setup
            DateTime date = new DateTime(2024, 02, 10);
            DateTime expected = new DateTime(2024, 02, 29);

            //// Act
            DateTime actual = date.LastOfMonth();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        #endregion LastOfMonth

        #region IsLeapYear

        [Test]
        public void IsLeapYear_2024_ReturnsTrue()
        {
            //// Setup
            DateTime date = new DateTime(2024, 01, 01);
            const bool expected = true;

            //// Act
            bool actual = date.IsLeapYear();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void IsLeapYear_2023_ReturnsFalse()
        {
            //// Setup
            DateTime date = new DateTime(2023, 01, 01);
            const bool expected = false;

            //// Act
            bool actual = date.IsLeapYear();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        #endregion IsLeapYear

        #region IsToday / IsTomorrow

        [Test]
        public void IsToday_TodaysDate_ReturnsTrue()
        {
            //// Setup
            DateTime date = DateTime.Today;
            const bool expected = true;

            //// Act
            bool actual = date.IsToday();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void IsToday_YesterdaysDate_ReturnsFalse()
        {
            //// Setup
            DateTime date = DateTime.Today.AddDays(-1);
            const bool expected = false;

            //// Act
            bool actual = date.IsToday();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void IsTomorrow_TomorrowsDate_ReturnsTrue()
        {
            //// Setup
            DateTime date = DateTime.Today.AddDays(1);
            const bool expected = true;

            //// Act
            bool actual = date.IsTomorrow();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void IsTomorrow_TodaysDate_ReturnsFalse()
        {
            //// Setup
            DateTime date = DateTime.Today;
            const bool expected = false;

            //// Act
            bool actual = date.IsTomorrow();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        #endregion IsToday / IsTomorrow

        #region Tomorrow / Yesterday

        [Test]
        public void Tomorrow_ReturnsDateOneDayFromToday()
        {
            //// Setup
            DateTime date = DateTime.Today;
            DateTime expected = DateTime.Today.AddDays(1);

            //// Act
            DateTime actual = date.Tomorrow();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void Yesterday_ReturnsDateOneDayBeforeToday()
        {
            //// Setup
            DateTime date = DateTime.Today;
            DateTime expected = DateTime.Today.AddDays(-1);

            //// Act
            DateTime actual = date.Yesterday();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        #endregion Tomorrow / Yesterday

        #region SetTime

        [Test]
        public void SetTime_ValidTime_SetsCorrectly()
        {
            //// Setup
            DateTime date = new DateTime(2024, 01, 15, 0, 0, 0);
            DateTime expected = new DateTime(2024, 01, 15, 14, 30, 45);

            //// Act
            DateTime actual = date.SetTime(14, 30, 45);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void SetTime_WithMilliseconds_SetsCorrectly()
        {
            //// Setup
            DateTime date = new DateTime(2024, 01, 15, 0, 0, 0);
            DateTime expected = new DateTime(2024, 01, 15, 14, 30, 45, 500);

            //// Act
            DateTime actual = date.SetTime(14, 30, 45, 500);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void SetTime_OverwritesExistingTime()
        {
            //// Setup
            DateTime date = new DateTime(2024, 01, 15, 10, 20, 30);
            DateTime expected = new DateTime(2024, 01, 15, 1, 2, 3);

            //// Act
            DateTime actual = date.SetTime(1, 2, 3);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        #endregion SetTime

        #region FindLastMonday

        [Test]
        public void FindLastMonday_Tuesday_ReturnsPreviousMonday()
        {
            //// Setup
            DateTime date = new DateTime(2024, 01, 09); // Tuesday
            DateTime expected = new DateTime(2024, 01, 08); // Monday

            //// Act
            DateTime actual = date.FindLastMonday();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void FindLastMonday_Monday_ReturnsSameDay()
        {
            //// Setup
            DateTime date = new DateTime(2024, 01, 08); // Monday
            DateTime expected = new DateTime(2024, 01, 08);

            //// Act
            DateTime actual = date.FindLastMonday();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void FindLastMonday_Sunday_ReturnsPreviousMonday()
        {
            //// Setup
            DateTime date = new DateTime(2024, 01, 14); // Sunday
            DateTime expected = new DateTime(2024, 01, 08); // Monday

            //// Act
            DateTime actual = date.FindLastMonday();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        #endregion FindLastMonday

        #region TimeElapsed

        [Test]
        public void TimeElapsed_PastDate_ReturnsPositiveTimeSpan()
        {
            //// Setup
            DateTime date = DateTime.Now.AddHours(-2);

            //// Act
            TimeSpan actual = date.TimeElapsed();

            //// Assert
            Assert.That(actual.TotalHours, Is.GreaterThanOrEqualTo(1.9));
            Assert.That(actual.TotalHours, Is.LessThanOrEqualTo(2.1));
        }

        #endregion TimeElapsed
    }
}
