namespace Transformations.Tests
{
    using System;
    using System.Collections.Generic;

    using NUnit.Framework;

    [TestFixture]
    public class CollectionHelperExtendedTests
    {
        #region AddUnique

        [Test]
        public void AddUnique_NewItem_AddsAndReturnsFalse()
        {
            //// Setup
            var list = new List<int> { 1, 2, 3 };

            //// Act
            bool alreadyHad = list.AddUnique(4);

            //// Assert
            Assert.That(alreadyHad, Is.False);
            Assert.That(list, Does.Contain(4));
        }

        [Test]
        public void AddUnique_DuplicateItem_DoesNotAddAndReturnsTrue()
        {
            //// Setup
            var list = new List<int> { 1, 2, 3 };

            //// Act
            bool alreadyHad = list.AddUnique(2);

            //// Assert
            Assert.That(alreadyHad, Is.True);
            Assert.That(list.Count, Is.EqualTo(3));
        }

        #endregion AddUnique

        #region AddRangeUnique

        [Test]
        public void AddRangeUnique_MixedValues_ReturnsCountOfDuplicatesFound()
        {
            //// Setup
            var list = new List<int> { 1, 2, 3 };

            //// Act
            int duplicateCount = list.AddRangeUnique(new[] { 2, 3, 4, 5 });

            //// Assert
            Assert.That(duplicateCount, Is.EqualTo(2));
            Assert.That(list, Does.Contain(4));
            Assert.That(list, Does.Contain(5));
        }

        #endregion AddRangeUnique

        #region HasItems

        [Test]
        public void HasItems_NonEmptyList_ReturnsTrue()
        {
            //// Setup
            var list = new List<int> { 1 };

            //// Act
            bool actual = list.HasItems();

            //// Assert
            Assert.That(actual, Is.True);
        }

        [Test]
        public void HasItems_EmptyList_ReturnsFalse()
        {
            //// Setup
            var list = new List<int>();

            //// Act
            bool actual = list.HasItems();

            //// Assert
            Assert.That(actual, Is.False);
        }

        [Test]
        public void HasItems_NullList_ReturnsFalse()
        {
            //// Act
            bool actual = ((List<int>?)null).HasItems();

            //// Assert
            Assert.That(actual, Is.False);
        }

        #endregion HasItems

        #region ForEach

        [Test]
        public void ForEach_ExecutesActionOnEachItem()
        {
            //// Setup
            var list = new List<int> { 1, 2, 3 };
            var results = new List<int>();

            //// Act
            list.ForEach(x => results.Add(x * 2));

            //// Assert
            Assert.That(results, Is.EqualTo(new List<int> { 2, 4, 6 }));
        }

        #endregion ForEach

        #region SlowPrepend

        [Test]
        public void SlowPrepend_AddsItemToFront()
        {
            //// Setup
            var list = new List<string> { "b", "c" };

            //// Act
            var actual = list.SlowPrepend("a");

            //// Assert
            Assert.That(actual[0], Is.EqualTo("a"));
            Assert.That(actual.Count, Is.EqualTo(3));
        }

        #endregion SlowPrepend

        #region AddRange

        [Test]
        public void AddRange_MultipleParams_AddsAll()
        {
            //// Setup
            var list = new List<int> { 1 };

            //// Act
            list.AddRange<int, int>(2, 3, 4);

            //// Assert
            Assert.That(list.Count, Is.EqualTo(4));
            Assert.That(list, Is.EqualTo(new List<int> { 1, 2, 3, 4 }));
        }

        #endregion AddRange

        #region CloneList2

        [Test]
        public void CloneList2_ReturnsShallowCopy()
        {
            //// Setup
            var list = new List<int> { 10, 20, 30 };

            //// Act
            var clone = list.CloneList2();

            //// Assert
            Assert.That(clone, Is.EqualTo(list));
            Assert.That(clone, Is.Not.SameAs(list));
        }

        #endregion CloneList2
    }
}
