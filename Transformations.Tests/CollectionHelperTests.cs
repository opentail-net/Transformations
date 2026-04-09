namespace Transformations.Tests
{
    using System.Collections.Generic;

    using NUnit.Framework;

    [TestFixture]
    public class CollectionHelperTests
    {
        #region Contains InspectedComparison

        [Test]
        public void Contains_ContainsAllOf_AllPresent_ReturnsTrue()
        {
            //// Setup
            var collection = new List<int> { 1, 2, 3, 4, 5 };
            const bool expected = true;

            //// Act
            bool actual = collection.Contains(Inspect.InspectedComparison.ContainsAllOf, 1, 3, 5);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void Contains_ContainsAllOf_OneNotPresent_ReturnsFalse()
        {
            //// Setup
            var collection = new List<int> { 1, 2, 3 };
            const bool expected = false;

            //// Act
            bool actual = collection.Contains(Inspect.InspectedComparison.ContainsAllOf, 1, 2, 99);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void Contains_ContainsAnyOf_OnePresent_ReturnsTrue()
        {
            //// Setup
            var collection = new List<int> { 1, 2, 3 };
            const bool expected = true;

            //// Act
            bool actual = collection.Contains(Inspect.InspectedComparison.ContainsAnyOf, 99, 2, 100);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void Contains_ContainsAnyOf_NonePresent_ReturnsFalse()
        {
            //// Setup
            var collection = new List<int> { 1, 2, 3 };
            const bool expected = false;

            //// Act
            bool actual = collection.Contains(Inspect.InspectedComparison.ContainsAnyOf, 97, 98, 99);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void Contains_ContainsNoneOf_NonePresent_ReturnsTrue()
        {
            //// Setup
            var collection = new List<int> { 1, 2, 3 };
            const bool expected = true;

            //// Act
            bool actual = collection.Contains(Inspect.InspectedComparison.ContainsNoneOf, 97, 98, 99);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void Contains_ContainsNoneOf_OnePresent_ReturnsFalse()
        {
            //// Setup
            var collection = new List<int> { 1, 2, 3 };
            const bool expected = false;

            //// Act
            bool actual = collection.Contains(Inspect.InspectedComparison.ContainsNoneOf, 99, 1, 100);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void Contains_NullCollection_ReturnsFalse()
        {
            //// Setup
            List<int> collection = null!;
            const bool expected = false;

            //// Act
            bool actual = collection.Contains(Inspect.InspectedComparison.ContainsAnyOf, 1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void Contains_NullCollection_ContainsNoneOf_ReturnsTrue()
        {
            //// Setup
            List<int> collection = null!;
            const bool expected = true;

            //// Act
            bool actual = collection.Contains(Inspect.InspectedComparison.ContainsNoneOf, 1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void Contains_EmptyCollection_ContainsAllOf_ReturnsFalse()
        {
            //// Setup
            var collection = new List<int>();
            const bool expected = false;

            //// Act
            bool actual = collection.Contains(Inspect.InspectedComparison.ContainsAllOf, 1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        #endregion Contains InspectedComparison

        #region ContainsIgnoreCase

        [Test]
        public void ContainsIgnoreCase_MatchingValue_ReturnsTrue()
        {
            //// Setup
            var collection = new List<string> { "hello", "world", "foo" };
            const bool expected = true;

            //// Act
            bool actual = collection.ContainsIgnoreCase("WORLD");

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ContainsIgnoreCase_NoMatchingValue_ReturnsFalse()
        {
            //// Setup
            var collection = new List<string> { "hello", "world", "foo" };
            const bool expected = false;

            //// Act
            bool actual = collection.ContainsIgnoreCase("xyz");

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ContainsIgnoreCase_NullCollection_ReturnsFalse()
        {
            //// Setup
            List<string> collection = null!;
            const bool expected = false;

            //// Act
            bool actual = collection.ContainsIgnoreCase("hello");

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ContainsIgnoreCase_NullSearchValue_ReturnsFalse()
        {
            //// Setup
            var collection = new List<string> { "hello", "world" };
            const bool expected = false;

            //// Act
            bool actual = collection.ContainsIgnoreCase(null!);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ContainsIgnoreCase_EmptyCollection_ReturnsFalse()
        {
            //// Setup
            var collection = new List<string>();
            const bool expected = false;

            //// Act
            bool actual = collection.ContainsIgnoreCase("hello");

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        #endregion ContainsIgnoreCase
    }
}
