namespace Transformations.Tests
{
    using System;

    using NUnit.Framework;

    [TestFixture]
    public class TimeSpanHelperAdditionalTests
    {
        [Test]
        public void TimeStringSinceDate_ReturnsNonEmptyReadableValue()
        {
            string actual = DateTime.Now.AddMinutes(-5).TimeStringSinceDate();

            Assert.That(actual, Is.Not.Null.And.Not.Empty);
        }
    }
}
