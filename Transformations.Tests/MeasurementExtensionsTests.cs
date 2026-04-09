namespace Transformations.Tests
{
    using System;

    using NUnit.Framework;

    [TestFixture]
    public class MeasurementExtensionsTests
    {
        [TestCase(0L, "0.00 B")]
        [TestCase(1023L, "1023.00 B")]
        [TestCase(1024L, "1.00 KB")]
        [TestCase(1536L, "1.50 KB")]
        [TestCase(1048576L, "1.00 MB")]
        [TestCase(1073741824L, "1.00 GB")]
        [TestCase(1099511627776L, "1.00 TB")]
        [TestCase(-1536L, "-1.50 KB")]
        public void ToSizeString_FormatsExpectedUnits(long input, string expected)
        {
            string actual = input.ToSizeString();

            Assert.That(actual, Is.EqualTo(expected));
        }

        [TestCase(2, 4, 0, 0, 0, "2d 4h")]
        [TestCase(0, 1, 20, 0, 0, "1h 20m")]
        [TestCase(0, 0, 0, 0, 450, "450ms")]
        [TestCase(0, 0, 3, 15, 0, "3m 15s")]
        [TestCase(0, 0, 0, 8, 120, "8s 120ms")]
        public void ToShortElapsedString_FormatsCompactOutput(int days, int hours, int minutes, int seconds, int milliseconds, string expected)
        {
            TimeSpan input = new TimeSpan(days, hours, minutes, seconds, milliseconds);

            string actual = input.ToShortElapsedString();

            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ToShortElapsedString_NegativeDuration_PreservesSign()
        {
            TimeSpan input = TimeSpan.FromMinutes(-2);

            string actual = input.ToShortElapsedString();

            Assert.That(actual, Is.EqualTo("-2m"));
        }
    }
}
