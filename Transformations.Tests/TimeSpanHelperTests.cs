namespace Transformations.Tests
{
    using System;

    using NUnit.Framework;

    using Transformations;

    [TestFixture]
    public class TimeSpanHelperTests
    {
        #region ToReadableTimeString

        [Test]
        public void ToReadableTimeString_HhMm_ReturnsFormattedString()
        {
            //// Setup
            TimeSpan timespan = new TimeSpan(2, 30, 0);
            string expected = "02:30";

            //// Act
            string actual = timespan.ToReadableTimeString(TimeSpanHelper.OutputFormat.hhmm);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ToReadableTimeString_HhMmSs_ReturnsFormattedString()
        {
            //// Setup
            TimeSpan timespan = new TimeSpan(2, 30, 45);
            string expected = "02:30:45";

            //// Act
            string actual = timespan.ToReadableTimeString(TimeSpanHelper.OutputFormat.hhmmss);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ToReadableTimeString_ShorthandTotalTime_Seconds_ReturnsSeconds()
        {
            //// Setup
            TimeSpan timespan = TimeSpan.FromSeconds(30);

            //// Act
            string actual = timespan.ToReadableTimeString(TimeSpanHelper.OutputFormat.ShorthandTotalTime);

            //// Assert
            Assert.That(actual, Does.EndWith("s"));
        }

        [Test]
        public void ToReadableTimeString_ShorthandTotalTime_Minutes_ReturnsMinutes()
        {
            //// Setup
            TimeSpan timespan = TimeSpan.FromMinutes(5);

            //// Act
            string actual = timespan.ToReadableTimeString(TimeSpanHelper.OutputFormat.ShorthandTotalTime);

            //// Assert
            Assert.That(actual, Does.EndWith("m"));
        }

        [Test]
        public void ToReadableTimeString_VerboseTotalTime_Hours_ReturnsHours()
        {
            //// Setup
            TimeSpan timespan = TimeSpan.FromHours(3);

            //// Act
            string actual = timespan.ToReadableTimeString(TimeSpanHelper.OutputFormat.VerboseTotalTime);

            //// Assert
            Assert.That(actual, Does.EndWith("hours"));
        }

        [Test]
        public void ToReadableTimeString_VerboseTotalTime_Days_ReturnsDays()
        {
            //// Setup
            TimeSpan timespan = TimeSpan.FromDays(2);

            //// Act
            string actual = timespan.ToReadableTimeString(TimeSpanHelper.OutputFormat.VerboseTotalTime);

            //// Assert
            Assert.That(actual, Does.EndWith("days"));
        }

        #endregion ToReadableTimeString

        #region ToTimeSpanAs

        [Test]
        public void ToTimeSpanAs_Hours_ReturnsCorrectTimeSpan()
        {
            //// Setup
            int value = 2;
            TimeSpan expected = TimeSpan.FromHours(2);

            //// Act
            TimeSpan actual = value.ToTimeSpanAs(DateHelper.TimeInterval.Hour);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ToTimeSpanAs_Minutes_ReturnsCorrectTimeSpan()
        {
            //// Setup
            int value = 30;
            TimeSpan expected = TimeSpan.FromMinutes(30);

            //// Act
            TimeSpan actual = value.ToTimeSpanAs(DateHelper.TimeInterval.Minute);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ToTimeSpanAs_Seconds_ReturnsCorrectTimeSpan()
        {
            //// Setup
            int value = 45;
            TimeSpan expected = TimeSpan.FromSeconds(45);

            //// Act
            TimeSpan actual = value.ToTimeSpanAs(DateHelper.TimeInterval.Second);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ToTimeSpanAs_Milliseconds_ReturnsCorrectTimeSpan()
        {
            //// Setup
            int value = 500;
            TimeSpan expected = TimeSpan.FromMilliseconds(500);

            //// Act
            TimeSpan actual = value.ToTimeSpanAs(DateHelper.TimeInterval.Millisecond);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        #endregion ToTimeSpanAs

        #region TimeSinceDate

        [Test]
        public void TimeSinceDate_PastDate_ReturnsPositiveTimeSpan()
        {
            //// Setup
            DateTime date = DateTime.Now.AddHours(-1);

            //// Act
            TimeSpan actual = date.TimeSinceDate();

            //// Assert
            Assert.That(actual.TotalMinutes, Is.GreaterThan(59));
        }

        #endregion TimeSinceDate
    }
}
