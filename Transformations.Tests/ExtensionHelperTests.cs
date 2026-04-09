namespace Transformations.Tests
{
    using System;
    using System.Collections.Generic;

    using NUnit.Framework;

    [TestFixture]
    public class ExtensionHelperTests
    {
        #region Between (int)

        [Test]
        public void Between_Int_WithinRange_ReturnsTrue()
        {
            //// Setup
            int actual = 5;
            const bool expected = true;

            //// Act
            bool result = actual.Between(1, 10);

            //// Assert
            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void Between_Int_OutOfRange_ReturnsFalse()
        {
            //// Setup
            int actual = 99;
            const bool expected = false;

            //// Act
            bool result = actual.Between(1, 10);

            //// Assert
            Assert.That(result, Is.EqualTo(expected));
        }

        #endregion Between (int)

        #region Between (double)

        [Test]
        public void Between_Double_WithinRange_ReturnsTrue()
        {
            //// Setup
            double actual = 5.5;
            const bool expected = true;

            //// Act
            bool result = actual.Between(1.0, 10.0);

            //// Assert
            Assert.That(result, Is.EqualTo(expected));
        }

        #endregion Between (double)

        #region Between (DateTime)

        [Test]
        public void Between_DateTime_WithinRange_ReturnsTrue()
        {
            //// Setup
            DateTime actual = new DateTime(2024, 06, 15);
            const bool expected = true;

            //// Act
            bool result = actual.Between(new DateTime(2024, 01, 01), new DateTime(2024, 12, 31));

            //// Assert
            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void Between_DateTime_OutOfRange_ReturnsFalse()
        {
            //// Setup
            DateTime actual = new DateTime(2025, 01, 01);
            const bool expected = false;

            //// Act
            bool result = actual.Between(new DateTime(2024, 01, 01), new DateTime(2024, 12, 31));

            //// Assert
            Assert.That(result, Is.EqualTo(expected));
        }

        #endregion Between (DateTime)

        #region Between (generic IComparable)

        [Test]
        public void Between_Generic_WithinRange_ReturnsTrue()
        {
            //// Setup
            int actual = 5;
            const bool expected = true;

            //// Act
            bool result = ExtensionHelper.Between<int>(actual, 1, 10);

            //// Assert
            Assert.That(result, Is.EqualTo(expected));
        }

        #endregion Between (generic IComparable)

        #region Between (List index)

        [Test]
        public void Between_ListIndex_ValidIndex_ReturnsTrue()
        {
            //// Setup
            var list = new List<string> { "a", "b", "c" };
            int index = 1;
            const bool expected = true;

            //// Act
            bool result = index.Between(list);

            //// Assert
            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void Between_ListIndex_OutOfBounds_ReturnsFalse()
        {
            //// Setup
            var list = new List<string> { "a", "b", "c" };
            int index = 5;
            const bool expected = false;

            //// Act
            bool result = index.Between(list);

            //// Assert
            Assert.That(result, Is.EqualTo(expected));
        }

        #endregion Between (List index)

        #region In

        [Test]
        public void In_MatchingValue_ReturnsTrue()
        {
            //// Setup
            int valueInput = 3;
            const bool expected = true;

            //// Act
            bool actual = ExtensionHelper.In(valueInput, 1, 2, 3, 4, 5);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void In_NoMatchingValue_ReturnsFalse()
        {
            //// Setup
            int valueInput = 99;
            const bool expected = false;

            //// Act
            bool actual = ExtensionHelper.In(valueInput, 1, 2, 3);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        #endregion In
    }
}
