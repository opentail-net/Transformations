namespace Transformations.Tests
{
    using System;
    using System.Collections.Generic;

    using NUnit.Framework;

    [TestFixture]
    public class MiscHelperTests
    {
        #region Test Enum

        private enum TestEnum
        {
            [System.ComponentModel.Description("First value")]
            ValueOne = 1,

            [System.ComponentModel.Description("Second value")]
            ValueTwo = 2,

            [System.ComponentModel.Description("Third value")]
            ValueThree = 3
        }

        #endregion Test Enum

        #region ToByte

        [Test]
        public void ToByte_ValidEnum_ReturnsByteValue()
        {
            //// Setup
            Enum input = TestEnum.ValueTwo;
            byte expected = 2;

            //// Act
            byte actual = input.ToByte();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        #endregion ToByte

        #region ToInt

        [Test]
        public void ToInt_ValidEnum_ReturnsIntValue()
        {
            //// Setup
            Enum input = TestEnum.ValueThree;
            int expected = 3;

            //// Act
            int actual = input.ToInt();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        #endregion ToInt

        #region ToLong

        [Test]
        public void ToLong_ValidEnum_ReturnsLongValue()
        {
            //// Setup
            Enum input = TestEnum.ValueOne;
            long expected = 1;

            //// Act
            long actual = input.ToLong();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        #endregion ToLong

        #region ToShort

        [Test]
        public void ToShort_ValidEnum_ReturnsShortValue()
        {
            //// Setup
            Enum input = TestEnum.ValueTwo;
            short expected = 2;

            //// Act
            short actual = input.ToShort();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        #endregion ToShort

        #region ToDecimal

        [Test]
        public void ToDecimal_ValidEnum_ReturnsDecimalValue()
        {
            //// Setup
            Enum input = TestEnum.ValueThree;
            decimal expected = 3m;

            //// Act
            decimal actual = input.ToDecimal();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        #endregion ToDecimal

        #region ToDouble

        [Test]
        public void ToDouble_ValidEnum_ReturnsDoubleValue()
        {
            //// Setup
            Enum input = TestEnum.ValueTwo;
            double expected = 2.0;

            //// Act
            double actual = input.ToDouble();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        #endregion ToDouble

        #region InAnyOf

        [Test]
        public void InAnyOf_MatchingValue_ReturnsTrue()
        {
            //// Setup
            TestEnum source = TestEnum.ValueTwo;
            const bool expected = true;

            //// Act
            bool actual = source.InAnyOf(TestEnum.ValueOne, TestEnum.ValueTwo, TestEnum.ValueThree);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void InAnyOf_NoMatch_ReturnsFalse()
        {
            //// Setup
            TestEnum source = TestEnum.ValueThree;
            const bool expected = false;

            //// Act
            bool actual = source.InAnyOf(TestEnum.ValueOne, TestEnum.ValueTwo);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void InAnyOf_EmptyValues_ReturnsFalse()
        {
            //// Setup
            TestEnum source = TestEnum.ValueOne;
            const bool expected = false;

            //// Act
            bool actual = source.InAnyOf();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        #endregion InAnyOf

        #region ToYesNoString

        [Test]
        public void ToYesNoString_True_ReturnsYes()
        {
            //// Setup
            bool valueInput = true;
            string expected = "Yes";

            //// Act
            string actual = valueInput.ToYesNoString();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ToYesNoString_False_ReturnsNo()
        {
            //// Setup
            bool valueInput = false;
            string expected = "No";

            //// Act
            string actual = valueInput.ToYesNoString();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        #endregion ToYesNoString

        #region ConfirmDefault

        [Test]
        public void ConfirmDefault_NullValue_ReturnsDefault()
        {
            //// Setup
            int? value = null;
            int expected = 0;

            //// Act
            int actual = value.ConfirmDefault();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConfirmDefault_WithValue_ReturnsValue()
        {
            //// Setup
            int? value = 42;
            int expected = 42;

            //// Act
            int actual = value.ConfirmDefault();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        #endregion ConfirmDefault

        #region EnumToDictionary

        [Test]
        public void EnumToDictionary_ValidEnumType_ReturnsDictionary()
        {
            //// Act
            var actual = typeof(TestEnum).EnumToDictionary();

            //// Assert
            Assert.That(actual, Is.Not.Null);
            Assert.That(actual!.Count, Is.EqualTo(3));
            Assert.That(actual[1], Is.EqualTo("ValueOne"));
        }

        [Test]
        public void EnumToDictionary_NonEnumType_ReturnsNull()
        {
            //// Act
            var actual = typeof(string).EnumToDictionary();

            //// Assert
            Assert.That(actual, Is.Null);
        }

        #endregion EnumToDictionary

        #region EnumToNamesList

        [Test]
        public void EnumToNamesList_ValidEnumType_ReturnsNames()
        {
            //// Act
            var actual = typeof(TestEnum).EnumToNamesList();

            //// Assert
            Assert.That(actual, Is.Not.Null);
            Assert.That(actual!.Count, Is.EqualTo(3));
            Assert.That(actual, Contains.Item("ValueOne"));
        }

        [Test]
        public void EnumToNamesList_NonEnumType_ReturnsNull()
        {
            //// Act
            var actual = typeof(string).EnumToNamesList();

            //// Assert
            Assert.That(actual, Is.Null);
        }

        #endregion EnumToNamesList
    }
}
