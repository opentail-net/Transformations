namespace Transformations.Tests
{
    using System;
    using System.Linq;

    using NUnit.Framework;

    [TestFixture]
    public class ObjectDeltaTests
    {
        private sealed class Profile
        {
            public string Name { get; set; } = string.Empty;
            public int Age { get; set; }

            [SkipDelta]
            public string Password { get; set; } = string.Empty;

            [SkipDelta]
            public DateTime Timestamp { get; set; }

            public Address? Address { get; set; }
        }

        private sealed class Address
        {
            public string Line1 { get; set; } = string.Empty;
        }

        [Test]
        public void Compare_ChangedTopLevelProperties_ReturnsDeltas()
        {
            var oldObj = new Profile { Name = "Alice", Age = 30, Password = "old", Timestamp = new DateTime(2024, 1, 1) };
            var newObj = new Profile { Name = "Bob", Age = 31, Password = "new", Timestamp = new DateTime(2025, 1, 1) };

            var deltas = ObjectDelta.Compare(oldObj, newObj);

            Assert.That(deltas.Count, Is.EqualTo(2));
            Assert.That(deltas.Any(d => d.PropertyName == "Name" && (string?)d.OldValue == "Alice" && (string?)d.NewValue == "Bob"), Is.True);
            Assert.That(deltas.Any(d => d.PropertyName == "Age" && (int)d.OldValue! == 30 && (int)d.NewValue! == 31), Is.True);
        }

        [Test]
        public void Compare_SkipDeltaProperties_AreIgnored()
        {
            var oldObj = new Profile { Name = "Same", Age = 1, Password = "p1", Timestamp = new DateTime(2024, 1, 1) };
            var newObj = new Profile { Name = "Same", Age = 1, Password = "p2", Timestamp = new DateTime(2025, 1, 1) };

            var deltas = ObjectDelta.Compare(oldObj, newObj);

            Assert.That(deltas, Is.Empty);
        }

        [Test]
        public void Compare_ShallowComparison_DoesNotWalkObjectGraph()
        {
            var oldObj = new Profile { Name = "A", Age = 1, Address = new Address { Line1 = "One" } };
            var newObj = new Profile { Name = "A", Age = 1, Address = new Address { Line1 = "One" } };

            var deltas = ObjectDelta.Compare(oldObj, newObj);

            // Important: shallow comparer treats different reference instances as a change even when nested values are equal.
            Assert.That(deltas.Count, Is.EqualTo(1));
            Assert.That(deltas[0].PropertyName, Is.EqualTo("Address"));
        }

        [Test]
        public void Compare_NullInput_Throws()
        {
            var valid = new Profile();

            Assert.Throws<ArgumentNullException>(() => ObjectDelta.Compare<Profile>(null!, valid));
            Assert.Throws<ArgumentNullException>(() => ObjectDelta.Compare(valid, null!));
        }
    }
}
