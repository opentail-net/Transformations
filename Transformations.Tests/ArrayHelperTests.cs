namespace Transformations.Tests
{
    using System;

    using NUnit.Framework;

    [TestFixture]
    public class ArrayHelperTests
    {
        #region BlockCopy

        [Test]
        public void BlockCopy_ValidRange_ReturnsCopiedBlock()
        {
            //// Setup
            int[] input = { 1, 2, 3, 4, 5 };
            int[] expected = { 2, 3, 4 };

            //// Act
            int[] actual = input.BlockCopy(1, 3);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void BlockCopy_LengthExceedsAvailable_ReturnsAvailable()
        {
            //// Setup
            int[] input = { 1, 2, 3 };
            int[] expected = { 2, 3 };

            //// Act
            int[] actual = input.BlockCopy(1, 100);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void BlockCopy_PadToLength_PadsWithDefault()
        {
            //// Setup
            int[] input = { 1, 2, 3 };
            int expectedLength = 5;

            //// Act
            int[] actual = input.BlockCopy(1, 5, padToLength: true);

            //// Assert
            Assert.That(actual.Length, Is.EqualTo(expectedLength));
            Assert.That(actual[0], Is.EqualTo(2));
            Assert.That(actual[1], Is.EqualTo(3));
            Assert.That(actual[2], Is.EqualTo(0));
        }

        [Test]
        public void BlockCopy_NullArray_ThrowsArgumentNullException()
        {
            //// Setup
            int[] input = null!;

            //// Act / Assert
            Assert.Throws<ArgumentNullException>(() => input.BlockCopy(0, 1));
        }

        [Test]
        public void BlockCopy_IndexBeyondEnd_ReturnsEmpty()
        {
            //// Setup
            int[] input = { 1, 2, 3 };
            int[] expected = Array.Empty<int>();

            //// Act
            int[] actual = input.BlockCopy(100, 5);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        #endregion BlockCopy

        #region ClearAll

        [Test]
        public void ClearAll_PopulatedArray_ClearsAllElements()
        {
            //// Setup
            int[] input = { 1, 2, 3 };
            int[] expected = { 0, 0, 0 };

            //// Act
            int[] actual = input.ClearAll();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
            Assert.That(actual, Is.SameAs(input));
        }

        [Test]
        public void ClearAll_NullArray_ReturnsEmptyArray()
        {
            //// Setup
            int[] input = null!;

            //// Act
            int[] actual = input.ClearAll();

            //// Assert
            Assert.That(actual, Is.Empty);
        }

        #endregion ClearAll

        #region CombineArrays

        [Test]
        public void CombineArrays_TwoArrays_ReturnsCombined()
        {
            //// Setup
            int[] first = { 1, 2, 3 };
            int[] second = { 4, 5 };
            int[] expected = { 1, 2, 3, 4, 5 };

            //// Act
            int[] actual = first.CombineArrays(second);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void CombineArrays_FirstNull_ReturnsSecond()
        {
            //// Setup
            int[]? first = null;
            int[] second = { 4, 5 };

            //// Act
            int[] actual = first.CombineArrays(second);

            //// Assert
            Assert.That(actual, Is.EqualTo(second));
        }

        [Test]
        public void CombineArrays_SecondNull_ReturnsFirst()
        {
            //// Setup
            int[] first = { 1, 2, 3 };
            int[]? second = null;

            //// Act
            int[] actual = first.CombineArrays(second);

            //// Assert
            Assert.That(actual, Is.EqualTo(first));
        }

        [Test]
        public void CombineArrays_BothNull_ReturnsEmpty()
        {
            //// Setup
            int[]? first = null;
            int[]? second = null;

            //// Act
            int[] actual = first.CombineArrays(second);

            //// Assert
            Assert.That(actual, Is.Empty);
        }

        #endregion CombineArrays

        #region PrependItem

        [Test]
        public void PrependItem_ValidArray_InsertsAtFront()
        {
            //// Setup
            int[] input = { 2, 3, 4 };
            int[] expected = { 1, 2, 3, 4 };

            //// Act
            int[] actual = input.PrependItem(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void PrependItem_NullArray_ReturnsSingleElementArray()
        {
            //// Setup
            int[]? input = null;
            int[] expected = { 1 };

            //// Act
            int[] actual = input.PrependItem(1);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        #endregion PrependItem
    }
}
