namespace Transformations.Tests
{
    using System;
    using System.Collections.Generic;

    using NUnit.Framework;

    using Transformations;

    [TestFixture]
    public class CollectionConvertorTests
    {
        #region Methods

        [Test]
        public void ConvertIntListToFloatList_ValidInput_ConvertsSuccessfully()
        {
            //// Setup
            List<int> valueInput = new List<int>(new int[] { 2, 3, 4 });
            List<float> expected = new List<float>(new float[] { 2.0F, 3.0F, 4.0F });

            //// Act
            List<float> actual = valueInput.ConvertToList<int, float>();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertIntListToStringList_ValidInput_ConvertsSuccessfully()
        {
            //// Setup
            List<int> valueInput = new List<int>(new int[] { 2, 3, 4 });
            List<string> expected = new List<string>(new string[] { "2", "3", "4" });

            //// Act
            List<string> actual = valueInput.ConvertToStringList<int>();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        #endregion Methods
    }
}