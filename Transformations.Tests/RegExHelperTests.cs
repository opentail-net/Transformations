namespace Transformations.Tests
{
    using System;
    using System.Collections.Generic;

    using NUnit.Framework;

    [TestFixture]
    public class RegExHelperTests
    {
        #region RegExMatch

        [Test]
        public void RegExMatch_MatchingPattern_ReturnsTrue()
        {
            //// Setup
            string valueInput = "abc123";
            const bool expected = true;

            //// Act
            bool actual = valueInput.RegExMatch("[0-9]");

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void RegExMatch_NoMatch_ReturnsFalse()
        {
            //// Setup
            string valueInput = "abcdef";
            const bool expected = false;

            //// Act
            bool actual = valueInput.RegExMatch("^[0-9]+$");

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void RegExMatch_NullInput_ReturnsFalse()
        {
            //// Setup
            string valueInput = null!;
            const bool expected = false;

            //// Act
            bool actual = valueInput.RegExMatch("[0-9]");

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void RegExMatch_NullPattern_ReturnsFalse()
        {
            //// Setup
            string valueInput = "abc123";
            const bool expected = false;

            //// Act
            bool actual = valueInput.RegExMatch(null!);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void RegExMatch_WithResultList_PopulatesGroups()
        {
            //// Setup
            string valueInput = "abc123";
            const bool expected = true;

            //// Act
            bool actual = valueInput.RegExMatch("([a-z]+)([0-9]+)", out List<string> resultList);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
            Assert.That(resultList.Count, Is.GreaterThan(0));
        }

        [Test]
        public void RegExMatch_WithResultList_NullInput_ReturnsFalseAndEmptyList()
        {
            //// Setup
            string valueInput = null!;

            //// Act
            bool actual = valueInput.RegExMatch("[0-9]", out List<string> resultList);

            //// Assert
            Assert.That(actual, Is.False);
            Assert.That(resultList, Is.Empty);
        }

        #endregion RegExMatch

        #region RegExSplit

        [Test]
        public void RegExSplit_ValidPattern_SplitsCorrectly()
        {
            //// Setup
            string valueInput = "abc123def456";

            //// Act
            IList<string> actual = valueInput.RegExSplit("[0-9]+");

            //// Assert
            Assert.That(actual, Contains.Item("abc"));
            Assert.That(actual, Contains.Item("def"));
        }

        [Test]
        public void RegExSplit_NullInput_ReturnsEmptyList()
        {
            //// Setup
            string valueInput = null!;

            //// Act
            IList<string> actual = valueInput.RegExSplit("[0-9]+");

            //// Assert
            Assert.That(actual, Is.Empty);
        }

        [Test]
        public void RegExSplit_EmptyPattern_ReturnsEmptyList()
        {
            //// Setup
            string valueInput = "abc123";

            //// Act
            IList<string> actual = valueInput.RegExSplit(string.Empty);

            //// Assert
            Assert.That(actual, Is.Empty);
        }

        #endregion RegExSplit
    }
}
