namespace Transformations.Tests
{
    using System;
    using System.Diagnostics;

    using NUnit.Framework;

    [TestFixture]
    public class StopwatchHelperTests
    {
        #region ElapsedSeconds / ElapsedMinutes / ElapsedHours / ElapsedDays

        [Test]
        public void ElapsedSeconds_RunningStopwatch_ReturnsSeconds()
        {
            //// Setup
            var sw = Stopwatch.StartNew();
            System.Threading.Thread.Sleep(10);
            sw.Stop();

            //// Act
            long actual = sw.ElapsedSeconds();

            //// Assert
            Assert.That(actual, Is.GreaterThanOrEqualTo(0));
        }

        [Test]
        public void ElapsedMinutes_StoppedStopwatch_ReturnsMinutes()
        {
            //// Setup
            var sw = new Stopwatch();

            //// Act
            long actual = sw.ElapsedMinutes();

            //// Assert
            Assert.That(actual, Is.EqualTo(0));
        }

        [Test]
        public void ElapsedHours_NullStopwatch_ReturnsZero()
        {
            //// Setup
            Stopwatch sw = null!;
            long expected = 0;

            //// Act
            long actual = sw.ElapsedHours();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ElapsedDays_NullStopwatch_ReturnsZero()
        {
            //// Setup
            Stopwatch sw = null!;
            long expected = 0;

            //// Act
            long actual = sw.ElapsedDays();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ElapsedSeconds_NullStopwatch_ReturnsZero()
        {
            //// Setup
            Stopwatch sw = null!;
            long expected = 0;

            //// Act
            long actual = sw.ElapsedSeconds();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ElapsedMinutes_NullStopwatch_ReturnsZero()
        {
            //// Setup
            Stopwatch sw = null!;
            long expected = 0;

            //// Act
            long actual = sw.ElapsedMinutes();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        #endregion ElapsedSeconds / ElapsedMinutes / ElapsedHours / ElapsedDays

        #region ToElapsedTimeString

        [Test]
        public void ToElapsedTimeString_RunningStopwatch_ReturnsNonEmptyString()
        {
            //// Setup
            var sw = Stopwatch.StartNew();
            System.Threading.Thread.Sleep(10);
            sw.Stop();

            //// Act
            string actual = sw.ToElapsedTimeString();

            //// Assert
            Assert.That(actual, Is.Not.Empty);
        }

        [Test]
        public void ToElapsedTimeString_NullStopwatch_ReturnsEmptyString()
        {
            //// Setup
            Stopwatch sw = null!;

            //// Act
            string actual = sw.ToElapsedTimeString();

            //// Assert
            Assert.That(actual, Is.EqualTo(string.Empty));
        }

        #endregion ToElapsedTimeString
    }
}
