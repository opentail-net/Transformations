namespace Transformations.Tests
{
    using System;
    using System.Collections.Generic;

    using NUnit.Framework;

    [TestFixture]
    public class ComparableHelperTests
    {
        #region IsBetween (int)

        [Test]
        public void IsBetween_Int_WithinRange_ReturnsTrue()
        {
            //// Setup
            int actual = 5;
            const bool expected = true;

            //// Act
            bool result = actual.IsBetween(1, 10);

            //// Assert
            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void IsBetween_Int_AtLowerBound_ReturnsTrue()
        {
            //// Setup
            int actual = 1;
            const bool expected = true;

            //// Act
            bool result = actual.IsBetween(1, 10);

            //// Assert
            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void IsBetween_Int_AtUpperBound_ReturnsTrue()
        {
            //// Setup
            int actual = 10;
            const bool expected = true;

            //// Act
            bool result = actual.IsBetween(1, 10);

            //// Assert
            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void IsBetween_Int_OutOfRange_ReturnsFalse()
        {
            //// Setup
            int actual = 99;
            const bool expected = false;

            //// Act
            bool result = actual.IsBetween(1, 10);

            //// Assert
            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void IsBetween_Int_WithDefault_OutOfRange_ReturnsDefault()
        {
            //// Setup
            int actual = 99;
            int expected = -1;

            //// Act
            int result = actual.IsBetween(1, 10, -1);

            //// Assert
            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void IsBetween_Int_WithDefault_InRange_ReturnsActual()
        {
            //// Setup
            int actual = 5;
            int expected = 5;

            //// Act
            int result = actual.IsBetween(1, 10, -1);

            //// Assert
            Assert.That(result, Is.EqualTo(expected));
        }

        #endregion IsBetween (int)

        #region IsBetween (double)

        [Test]
        public void IsBetween_Double_WithinRange_ReturnsTrue()
        {
            //// Setup
            double actual = 5.5;
            const bool expected = true;

            //// Act
            bool result = actual.IsBetween(1.0, 10.0);

            //// Assert
            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void IsBetween_Double_OutOfRange_ReturnsFalse()
        {
            //// Setup
            double actual = 99.9;
            const bool expected = false;

            //// Act
            bool result = actual.IsBetween(1.0, 10.0);

            //// Assert
            Assert.That(result, Is.EqualTo(expected));
        }

        #endregion IsBetween (double)

        #region IsBetween (DateTime)

        [Test]
        public void IsBetween_DateTime_WithinRange_ReturnsTrue()
        {
            //// Setup
            DateTime actual = new DateTime(2024, 06, 15);
            DateTime min = new DateTime(2024, 01, 01);
            DateTime max = new DateTime(2024, 12, 31);
            const bool expected = true;

            //// Act
            bool result = actual.IsBetween(min, max);

            //// Assert
            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void IsBetween_DateTime_OutOfRange_ReturnsFalse()
        {
            //// Setup
            DateTime actual = new DateTime(2025, 01, 01);
            DateTime min = new DateTime(2024, 01, 01);
            DateTime max = new DateTime(2024, 12, 31);
            const bool expected = false;

            //// Act
            bool result = actual.IsBetween(min, max);

            //// Assert
            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void IsBetween_DateTime_WithDefault_OutOfRange_ReturnsDefault()
        {
            //// Setup
            DateTime actual = new DateTime(2025, 01, 01);
            DateTime min = new DateTime(2024, 01, 01);
            DateTime max = new DateTime(2024, 12, 31);
            DateTime defaultValue = new DateTime(2024, 06, 01);

            //// Act
            DateTime result = actual.IsBetween(min, max, defaultValue);

            //// Assert
            Assert.That(result, Is.EqualTo(defaultValue));
        }

        #endregion IsBetween (DateTime)

        #region IsBetween (List index)

        [Test]
        public void IsBetween_ListIndex_ValidIndex_ReturnsTrue()
        {
            //// Setup
            var list = new List<string> { "a", "b", "c" };
            int index = 1;
            const bool expected = true;

            //// Act
            bool result = index.IsBetween(list);

            //// Assert
            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void IsBetween_ListIndex_NegativeIndex_ReturnsFalse()
        {
            //// Setup
            var list = new List<string> { "a", "b", "c" };
            int index = -1;
            const bool expected = false;

            //// Act
            bool result = index.IsBetween(list);

            //// Assert
            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void IsBetween_ListIndex_OutOfBounds_ReturnsFalse()
        {
            //// Setup
            var list = new List<string> { "a", "b", "c" };
            int index = 5;
            const bool expected = false;

            //// Act
            bool result = index.IsBetween(list);

            //// Assert
            Assert.That(result, Is.EqualTo(expected));
        }

        #endregion IsBetween (List index)

        #region BetweenOrFirst / BetweenOrLast / BetweenOrNext

        [Test]
        public void BetweenOrFirst_OutOfRange_ReturnsZero()
        {
            //// Setup
            var list = new List<string> { "a", "b", "c" };
            int index = 99;
            int expected = 0;

            //// Act
            int result = index.BetweenOrFirst(list);

            //// Assert
            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void BetweenOrFirst_InRange_ReturnsActual()
        {
            //// Setup
            var list = new List<string> { "a", "b", "c" };
            int index = 1;
            int expected = 1;

            //// Act
            int result = index.BetweenOrFirst(list);

            //// Assert
            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void BetweenOrLast_OutOfRange_ReturnsLastIndex()
        {
            //// Setup
            var list = new List<string> { "a", "b", "c" };
            int index = 99;
            int expected = 2;

            //// Act
            int result = index.BetweenOrLast(list);

            //// Assert
            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void BetweenOrNext_OutOfRange_ReturnsCount()
        {
            //// Setup
            var list = new List<string> { "a", "b", "c" };
            int index = 99;
            int expected = 3;

            //// Act
            int result = index.BetweenOrNext(list);

            //// Assert
            Assert.That(result, Is.EqualTo(expected));
        }

        #endregion BetweenOrFirst / BetweenOrLast / BetweenOrNext

        #region IsBetween (other types)

        [Test]
        public void IsBetween_Long_WithinRange_ReturnsTrue()
        {
            //// Setup
            long actual = 500L;
            const bool expected = true;

            //// Act
            bool result = actual.IsBetween(100L, 1000L);

            //// Assert
            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void IsBetween_Short_WithinRange_ReturnsTrue()
        {
            //// Setup
            short actual = 5;
            const bool expected = true;

            //// Act
            bool result = actual.IsBetween((short)1, (short)10);

            //// Assert
            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void IsBetween_Byte_WithinRange_ReturnsTrue()
        {
            //// Setup
            byte actual = 128;
            const bool expected = true;

            //// Act
            bool result = actual.IsBetween((byte)0, (byte)255);

            //// Assert
            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void IsBetween_Float_WithDefault_OutOfRange_ReturnsDefault()
        {
            //// Setup
            float actual = 99.9f;
            float expected = 0.0f;

            //// Act
            float result = actual.IsBetween(1.0f, 10.0f, 0.0f);

            //// Assert
            Assert.That(result, Is.EqualTo(expected));
        }

        #endregion IsBetween (other types)
    }
}
