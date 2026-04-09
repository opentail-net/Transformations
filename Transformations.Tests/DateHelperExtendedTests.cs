namespace Transformations.Tests
{
    using System;

    using NUnit.Framework;

    [TestFixture]
    public class DateHelperExtendedTests
    {
        #region AddSafely

        [Test]
        public void AddSafely_NormalAddition_ReturnsCorrectDate()
        {
            //// Setup
            DateTime date = new DateTime(2024, 06, 15);
            TimeSpan time = TimeSpan.FromDays(10);

            //// Act
            DateTime actual = date.AddSafely(time);

            //// Assert
            Assert.That(actual, Is.EqualTo(new DateTime(2024, 06, 25)));
        }

        [Test]
        public void AddSafely_OverflowMax_ReturnsMaxValue()
        {
            //// Setup
            DateTime date = DateTime.MaxValue.AddDays(-1);
            TimeSpan time = TimeSpan.FromDays(10);

            //// Act
            DateTime actual = date.AddSafely(time);

            //// Assert
            Assert.That(actual, Is.EqualTo(DateTime.MaxValue));
        }

        [Test]
        public void AddSafely_UnderflowMin_ReturnsMinValue()
        {
            //// Setup
            DateTime date = DateTime.MinValue.AddDays(1);
            TimeSpan time = TimeSpan.FromDays(-10);

            //// Act
            DateTime actual = date.AddSafely(time);

            //// Assert
            Assert.That(actual, Is.EqualTo(DateTime.MinValue));
        }

        #endregion AddSafely

        #region AddDaysSafely

        [Test]
        public void AddDaysSafely_NormalAddition_ReturnsCorrectDate()
        {
            //// Setup
            DateTime date = new DateTime(2024, 01, 01);

            //// Act
            DateTime actual = date.AddDaysSafely(31);

            //// Assert
            Assert.That(actual, Is.EqualTo(new DateTime(2024, 02, 01)));
        }

        [Test]
        public void AddDaysSafely_OverflowMax_ReturnsMaxValue()
        {
            //// Setup
            DateTime date = DateTime.MaxValue.AddDays(-1);

            //// Act
            DateTime actual = date.AddDaysSafely(10);

            //// Assert
            Assert.That(actual, Is.EqualTo(DateTime.MaxValue));
        }

        #endregion AddDaysSafely

        #region DateDiff

        [Test]
        public void DateDiff_Days_ReturnsCorrectDifference()
        {
            //// Setup
            DateTime start = new DateTime(2024, 01, 01);
            DateTime end = new DateTime(2024, 01, 31);

            //// Act
            long actual = start.DateDiff(DateHelper.DateInterval.Day, end);

            //// Assert
            Assert.That(actual, Is.EqualTo(30));
        }

        [Test]
        public void DateDiff_Hours_ReturnsCorrectDifference()
        {
            //// Setup
            DateTime start = new DateTime(2024, 01, 01, 0, 0, 0);
            DateTime end = new DateTime(2024, 01, 01, 12, 0, 0);

            //// Act
            long actual = start.DateDiff(DateHelper.DateInterval.Hour, end);

            //// Assert
            Assert.That(actual, Is.EqualTo(12));
        }

        [Test]
        public void DateDiff_Weeks_ReturnsCorrectDifference()
        {
            //// Setup
            DateTime start = new DateTime(2024, 01, 01);
            DateTime end = new DateTime(2024, 01, 29);

            //// Act
            long actual = start.DateDiff(DateHelper.DateInterval.Week, end);

            //// Assert
            Assert.That(actual, Is.EqualTo(4));
        }

        [Test]
        public void DateDiff_Months_SameDay_ReturnsCorrectDifference()
        {
            //// Setup
            DateTime start = new DateTime(2024, 01, 15);
            DateTime end = new DateTime(2024, 04, 15);

            //// Act
            long actual = start.DateDiff(DateHelper.DateInterval.Month, end);

            //// Assert
            Assert.That(actual, Is.EqualTo(3));
        }

        [Test]
        public void DateDiff_Year_FullYear_ReturnsOne()
        {
            //// Setup
            DateTime start = new DateTime(2023, 06, 15);
            DateTime end = new DateTime(2024, 06, 15);

            //// Act
            long actual = start.DateDiff(DateHelper.DateInterval.Year, end);

            //// Assert
            Assert.That(actual, Is.EqualTo(1));
        }

        [Test]
        public void DateDiff_Quarter_ReturnsCorrectDifference()
        {
            //// Setup
            DateTime start = new DateTime(2024, 01, 01);
            DateTime end = new DateTime(2024, 10, 01);

            //// Act
            long actual = start.DateDiff(DateHelper.DateInterval.Quarter, end);

            //// Assert
            Assert.That(actual, Is.EqualTo(3));
        }

        #endregion DateDiff

        #region FirstDateOfTheWeek / LastDateOfTheWeek

        [Test]
        public void FirstDateOfTheWeek_Wednesday_ReturnsPreviousMonday()
        {
            //// Setup - 2024-06-19 is a Wednesday
            DateTime date = new DateTime(2024, 06, 19);

            //// Act
            DateTime actual = date.FirstDateOfTheWeek();

            //// Assert
            Assert.That(actual.DayOfWeek, Is.EqualTo(DayOfWeek.Monday));
            Assert.That(actual, Is.EqualTo(new DateTime(2024, 06, 17)));
        }

        [Test]
        public void FirstDateOfTheWeek_Monday_ReturnsSameDay()
        {
            //// Setup - 2024-06-17 is a Monday
            DateTime date = new DateTime(2024, 06, 17);

            //// Act
            DateTime actual = date.FirstDateOfTheWeek();

            //// Assert
            Assert.That(actual, Is.EqualTo(date));
        }

        [Test]
        public void LastDateOfTheWeek_Wednesday_ReturnsFollowingSunday()
        {
            //// Setup - 2024-06-19 is a Wednesday
            DateTime date = new DateTime(2024, 06, 19);

            //// Act
            DateTime actual = date.LastDateOfTheWeek();

            //// Assert
            Assert.That(actual.DayOfWeek, Is.EqualTo(DayOfWeek.Sunday));
            Assert.That(actual, Is.EqualTo(new DateTime(2024, 06, 23)));
        }

        #endregion FirstDateOfTheWeek / LastDateOfTheWeek

        #region GetCurrentMonthName

        [Test]
        public void GetCurrentMonthName_ReturnsNonEmptyString()
        {
            //// Act
            string actual = DateHelper.GetCurrentMonthName();

            //// Assert
            Assert.That(actual, Is.Not.Null.And.Not.Empty);
        }

        #endregion GetCurrentMonthName

        #region AddMonthsSafely

        [Test]
        public void AddMonthsSafely_NormalAddition_ReturnsCorrectDate()
        {
            //// Setup
            DateTime date = new DateTime(2024, 01, 15);

            //// Act
            DateTime actual = date.AddMonthsSafely(3);

            //// Assert
            Assert.That(actual, Is.EqualTo(new DateTime(2024, 04, 15)));
        }

        [Test]
        public void AddMonthsSafely_OverflowMax_ReturnsMaxValue()
        {
            //// Setup
            DateTime date = new DateTime(9999, 10, 01);

            //// Act
            DateTime actual = date.AddMonthsSafely(12);

            //// Assert
            Assert.That(actual, Is.EqualTo(DateTime.MaxValue));
        }

        #endregion AddMonthsSafely

        #region AddSecondsSafely

        [Test]
        public void AddSecondsSafely_NormalAddition_ReturnsCorrectDate()
        {
            //// Setup
            DateTime date = new DateTime(2024, 01, 01, 12, 0, 0);

            //// Act
            DateTime actual = date.AddSecondsSafely(3600.0);

            //// Assert
            Assert.That(actual, Is.EqualTo(new DateTime(2024, 01, 01, 13, 0, 0)));
        }

        #endregion AddSecondsSafely
    }
}
