namespace Transformations.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using NUnit.Framework;

    using Transformations;

    [TestFixture]
    public class EnumHelperTests
    {
        #region Test Enum

        private enum TestGrades
        {
            [System.ComponentModel.Description("Passed the exam")]
            Pass = 0,

            [System.ComponentModel.Description("Failed the exam")]
            Fail = 1,

            Promoted = 2
        }

        #endregion Test Enum

        #region GetEnumDescription

        [Test]
        public void GetEnumDescription_WithDescription_ReturnsDescription()
        {
            //// Setup
            TestGrades valueInput = TestGrades.Pass;
            string expected = "Passed the exam";

            //// Act
            string? actual = valueInput.GetEnumDescription();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void GetEnumDescription_WithoutDescription_ReturnsName()
        {
            //// Setup
            TestGrades valueInput = TestGrades.Promoted;
            string expected = "Promoted";

            //// Act
            string? actual = valueInput.GetEnumDescription();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        #endregion GetEnumDescription

        #region GetEnumDescription2

        [Test]
        public void GetEnumDescription2_WithDescription_ReturnsDescription()
        {
            //// Setup
            TestGrades valueInput = TestGrades.Fail;
            string expected = "Failed the exam";

            //// Act
            string? actual = valueInput.GetEnumDescription2();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void GetEnumDescription2_WithoutDescription_ReturnsName()
        {
            //// Setup
            TestGrades valueInput = TestGrades.Promoted;
            string expected = "Promoted";

            //// Act
            string? actual = valueInput.GetEnumDescription2();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        #endregion GetEnumDescription2

        #region GetEnumDescription3

        [Test]
        public void GetEnumDescription3_WithDescription_ReturnsDescription()
        {
            //// Setup
            Enum valueInput = TestGrades.Pass;
            string expected = "Passed the exam";

            //// Act
            string actual = valueInput.GetEnumDescription3();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        #endregion GetEnumDescription3

        #region GetEnumDescriptionsList

        [Test]
        public void GetEnumDescriptionsList_ValidCollection_ReturnsDescriptions()
        {
            //// Setup
            var collection = new List<TestGrades> { TestGrades.Pass, TestGrades.Fail, TestGrades.Promoted };
            int expected = 3;

            //// Act
            IEnumerable<string>? actual = collection.GetEnumDescriptionsList();

            //// Assert
            Assert.That(actual, Is.Not.Null);
            Assert.That(actual!.Count(), Is.EqualTo(expected));
            Assert.That(actual!.First(), Is.EqualTo("Passed the exam"));
        }

        [Test]
        public void GetEnumDescriptionsList_NullCollection_ReturnsNull()
        {
            //// Setup
            IEnumerable<TestGrades>? collection = null;

            //// Act
            IEnumerable<string>? actual = collection!.GetEnumDescriptionsList();

            //// Assert
            Assert.That(actual, Is.Null);
        }

        #endregion GetEnumDescriptionsList

        #region ToEnum (string)

        [Test]
        public void ToEnum_ValidString_ReturnsEnum()
        {
            //// Setup
            string valueInput = "Pass";
            TestGrades expected = TestGrades.Pass;

            //// Act
            TestGrades actual = valueInput.ToEnum<TestGrades>();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ToEnum_CaseInsensitive_ReturnsEnum()
        {
            //// Setup
            string valueInput = "fail";
            TestGrades expected = TestGrades.Fail;

            //// Act
            TestGrades actual = valueInput.ToEnum<TestGrades>();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ToEnum_InvalidString_ReturnsDefault()
        {
            //// Setup
            string valueInput = "invalid";
            TestGrades expected = TestGrades.Pass;

            //// Act
            TestGrades actual = valueInput.ToEnum<TestGrades>();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ToEnum_InvalidStringWithDefault_ReturnsDefaultValue()
        {
            //// Setup
            string valueInput = "invalid";
            TestGrades expected = TestGrades.Promoted;

            //// Act
            TestGrades actual = valueInput.ToEnum<TestGrades>(TestGrades.Promoted);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        #endregion ToEnum (string)

        #region ToEnum (int)

        [Test]
        public void ToEnum_ValidInt_ReturnsEnum()
        {
            //// Setup
            int valueInput = 1;
            TestGrades expected = TestGrades.Fail;

            //// Act
            TestGrades actual = valueInput.ToEnum<TestGrades>();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ToEnum_InvalidInt_ReturnsDefault()
        {
            //// Setup
            int valueInput = 999;
            TestGrades expected = TestGrades.Pass;

            //// Act
            TestGrades actual = valueInput.ToEnum<TestGrades>();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        #endregion ToEnum (int)

        #region TryToEnum

        [Test]
        public void TryToEnum_ValidString_ReturnsTrueAndValue()
        {
            //// Setup
            string valueInput = "Fail";
            const bool expectedResult = true;
            TestGrades expectedValue = TestGrades.Fail;

            //// Act
            bool actual = valueInput.TryToEnum<TestGrades>(out TestGrades result);

            //// Assert
            Assert.That(actual, Is.EqualTo(expectedResult));
            Assert.That(result, Is.EqualTo(expectedValue));
        }

        [Test]
        public void TryToEnum_InvalidString_ReturnsFalseAndDefault()
        {
            //// Setup
            string valueInput = "invalid";
            const bool expectedResult = false;

            //// Act
            bool actual = valueInput.TryToEnum<TestGrades>(out TestGrades result);

            //// Assert
            Assert.That(actual, Is.EqualTo(expectedResult));
            Assert.That(result, Is.EqualTo(default(TestGrades)));
        }

        [Test]
        public void TryToEnum_ValidInt_ReturnsTrueAndValue()
        {
            //// Setup
            int valueInput = 2;
            const bool expectedResult = true;
            TestGrades expectedValue = TestGrades.Promoted;

            //// Act
            bool actual = valueInput.TryToEnum<TestGrades>(out TestGrades result);

            //// Assert
            Assert.That(actual, Is.EqualTo(expectedResult));
            Assert.That(result, Is.EqualTo(expectedValue));
        }

        [Test]
        public void TryToEnum_InvalidInt_ReturnsFalseAndDefault()
        {
            //// Setup
            int valueInput = 999;
            const bool expectedResult = false;

            //// Act
            bool actual = valueInput.TryToEnum<TestGrades>(out TestGrades result);

            //// Assert
            Assert.That(actual, Is.EqualTo(expectedResult));
            Assert.That(result, Is.EqualTo(default(TestGrades)));
        }

        #endregion TryToEnum

        #region GetEnumDescriptionKeyValuePairs

        [Test]
        public void GetEnumDescriptionKeyValuePairs_ValidCollection_ReturnsKeyValuePairs()
        {
            //// Setup
            var collection = new List<TestGrades> { TestGrades.Pass, TestGrades.Fail };

            //// Act
            var actual = collection.GetEnumDescriptionKeyValuePairs();

            //// Assert
            Assert.That(actual, Is.Not.Null);
            Assert.That(actual!.Count, Is.EqualTo(2));
            Assert.That(actual[0].Value, Is.EqualTo("Passed the exam"));
        }

        [Test]
        public void GetEnumDescriptionKeyValuePairs3_FromType_ReturnsAll()
        {
            //// Act
            var actual = typeof(TestGrades).GetEnumDescriptionKeyValuePairs3();

            //// Assert
            Assert.That(actual, Is.Not.Null);
            Assert.That(actual!.Count, Is.EqualTo(3));
        }

        [Test]
        public void GetEnumDescriptionKeyValuePairs3_NonEnumType_ReturnsNull()
        {
            //// Act
            var actual = typeof(string).GetEnumDescriptionKeyValuePairs3();

            //// Assert
            Assert.That(actual, Is.Null);
        }

        #endregion GetEnumDescriptionKeyValuePairs
    }
}
