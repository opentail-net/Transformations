namespace Transformations.Tests
{
    using System;

    using NUnit.Framework;

    [TestFixture]
    public class ApiRationalizationTests
    {
        private enum SampleEnum
        {
            [System.ComponentModel.Description("Friendly")]
            Value = 1,
        }

        [Test]
        public void BetweenExclusive_UsesExclusiveUpperBound()
        {
            bool inRange = 5.BetweenExclusive(1, 10);
            bool upperExcluded = 10.BetweenExclusive(1, 10);

            Assert.That(inRange, Is.True);
            Assert.That(upperExcluded, Is.False);
        }

        [Test]
        public void IsBetweenInclusive_UsesInclusiveUpperBound()
        {
            bool inRange = 10.IsBetweenInclusive(1, 10);

            Assert.That(inRange, Is.True);
        }

        [Test]
        public void EnumGetDescription_CanonicalApi_ReadsDescriptionAttribute()
        {
            string descriptionFromEnum = SampleEnum.Value.GetDescription();
            string? descriptionFromGeneric = SampleEnum.Value.GetEnumDescription();

            Assert.That(descriptionFromEnum, Is.EqualTo("Friendly"));
            Assert.That(descriptionFromGeneric, Is.EqualTo("Friendly"));
        }
    }
}
