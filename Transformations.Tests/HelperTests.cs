namespace Transformations.Tests
{
    using System;
    using System.Data;

    using NUnit.Framework;

    using Transformations;

    [TestFixture]
    public class HelperTests
    {
        #region Methods

        /////// *********************************************************************
        [Test]
        public void ContainsDigits_DigitsPresent_ReturnsTrue()
        {
            //// Setup
            string valueInput = "123";
            const bool expected = true;

            //// Act
            bool actual = valueInput!.ContainsDigits();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ContainsDigits_EmptyValue_ReturnsFalse()
        {
            //// Setup
            string valueInput = string.Empty;
            const bool expected = false;

            //// Act
            bool actual = valueInput!.ContainsDigits();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ContainsDigits_NoDigitsPresent_ReturnsFalse()
        {
            //// Setup
            string valueInput = "abc";
            const bool expected = false;

            //// Act
            bool actual = valueInput!.ContainsDigits();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ContainsDigits_NullValue_ReturnsFalse()
        {
            //// Setup
            string? valueInput = null;
            const bool expected = false;

            //// Act
            bool actual = valueInput.ContainsDigits();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ContainsIgnoreCase_EmptyInputValidPattern_ReturnsFalse()
        {
            //// Setup
            string valueInput = string.Empty;
            string valuePattern = "23";
            const bool expected = false;

            //// Act
            bool actual = valueInput.ContainsIgnoreCase(valuePattern);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ContainsIgnoreCase_NullInputValidPattern_ReturnsFalse()
        {
            //// Setup
            string valueInput = string.Empty;
            string valuePattern = "23";
            const bool expected = false;

            //// Act
            bool actual = valueInput.ContainsIgnoreCase(valuePattern);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ContainsIgnoreCase_ValidInputDifferentCaseMatchingPattern_ReturnsTrue()
        {
            //// Setup
            string valueInput = "abcdefg";
            string[] valuePatterns = new string[] { "12", "CD" };
            const bool expected = true;

            //// Act
            bool actual = valueInput.ContainsIgnoreCase(valuePatterns);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ContainsIgnoreCase_ValidInputDifferentCasePattern_ReturnsTrue()
        {
            //// Setup
            string valueInput = "abcdefg";
            string valuePattern = "CD";
            const bool expected = true;

            //// Act
            bool actual = valueInput.ContainsIgnoreCase(valuePattern);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ContainsIgnoreCase_ValidInputEmptyPattern_ReturnsFalse()
        {
            //// Setup
            string valueInput = "1234567890";
            string valuePattern = "";
            const bool expected = false;

            //// Act
            bool actual = valueInput.ContainsIgnoreCase(valuePattern);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ContainsIgnoreCase_ValidInputEmptyStringInPatternsOneMatchingPattern_ReturnsTrue()
        {
            //// Setup
            string valueInput = "1234567890";
            string[] valuePatterns = new string[] { string.Empty, "23" };
            const bool expected = true;

            //// Act
            bool actual = valueInput.ContainsIgnoreCase(valuePatterns);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ContainsIgnoreCase_ValidInputMatchingPattern_ReturnsTrue()
        {
            //// Setup
            string valueInput = "1234567890";
            string valuePattern = "23";
            const bool expected = true;

            //// Act
            bool actual = valueInput.ContainsIgnoreCase(valuePattern);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ContainsIgnoreCase_ValidInputNoMatchingPatterns_ReturnsFalse()
        {
            //// Setup
            string valueInput = "1234567890";
            string[] valuePatterns = new string[] { "zz", "xx" };
            const bool expected = false;

            //// Act
            bool actual = valueInput.ContainsIgnoreCase(valuePatterns);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ContainsIgnoreCase_ValidInputNonMatchingPattern_ReturnsFalse()
        {
            //// Setup
            string valueInput = "1234567890";
            string valuePattern = "zz";
            const bool expected = false;

            //// Act
            bool actual = valueInput.ContainsIgnoreCase(valuePattern);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ContainsIgnoreCase_ValidInputNullInPatternsOneMatchingPattern_ReturnsTrue()
        {
            //// Setup
            string valueInput = "1234567890";
            string[] valuePatterns = new string[]
            {
                null!,
                "23"
            };
            const bool expected = true;

            //// Act
            bool actual = valueInput.ContainsIgnoreCase(valuePatterns);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ContainsIgnoreCase_ValidInputNullPattern_ReturnsFalse()
        {
            //// Setup
            string valueInput = "1234567890";
            string valuePattern = null!;
            const bool expected = false;

            //// Act
            bool actual = valueInput.ContainsIgnoreCase(valuePattern);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void DateDiff_AddDaysExample1_ReturnsCorrectDifference()
        {
            DateTime valueInput = new DateTime(2008, 2, 29);
            DateTime valueInput2 = new DateTime(2009, 2, 28);

            long expected = 0;
            DateTime ExpectedDateTime = valueInput;
            while (ExpectedDateTime < valueInput2)
            {
                ExpectedDateTime = ExpectedDateTime.AddDays(1);
                expected++;
            }

            long actual = valueInput.DateDiff(DateHelper.DateInterval.Day, valueInput2);

            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void DateDiff_AddHoursExample1_ReturnsCorrectDifference()
        {
            DateTime valueInput = new DateTime(2008, 2, 29);
            DateTime valueInput2 = new DateTime(2009, 2, 28);

            long expected = 0;
            DateTime ExpectedDateTime = valueInput;
            while (ExpectedDateTime < valueInput2)
            {
                ExpectedDateTime = ExpectedDateTime.AddHours(1);
                expected++;
            }

            long actual = valueInput.DateDiff(DateHelper.DateInterval.Hour, valueInput2);

            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void DateDiff_AddMilliecondsExample1_ReturnsCorrectDifference()
        {
            DateTime valueInput = new DateTime(2008, 2, 29);
            DateTime valueInput2 = new DateTime(2008, 3, 20);

            long expected = 0;
            DateTime ExpectedDateTime = valueInput;
            while (ExpectedDateTime < valueInput2)
            {
                ExpectedDateTime = ExpectedDateTime.AddMilliseconds(1);
                expected++;
            }

            long actual = valueInput.DateDiff(DateHelper.DateInterval.Millisecond, valueInput2);

            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void DateDiff_AddMinutesExample1_ReturnsCorrectDifference()
        {
            DateTime valueInput = new DateTime(2008, 2, 29);
            DateTime valueInput2 = new DateTime(2009, 2, 28);

            long expected = 0;
            DateTime ExpectedDateTime = valueInput;
            while (ExpectedDateTime < valueInput2)
            {
                ExpectedDateTime = ExpectedDateTime.AddMinutes(1);
                expected++;
            }

            long actual = valueInput.DateDiff(DateHelper.DateInterval.Minute, valueInput2);

            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void DateDiff_AddSecondsExample1_ReturnsCorrectDifference()
        {
            DateTime valueInput = new DateTime(2008, 2, 29);
            DateTime valueInput2 = new DateTime(2009, 2, 28);

            long expected = 0;
            DateTime ExpectedDateTime = valueInput;
            while (ExpectedDateTime < valueInput2)
            {
                ExpectedDateTime = ExpectedDateTime.AddSeconds(1);
                expected++;
            }

            long actual = valueInput.DateDiff(DateHelper.DateInterval.Second, valueInput2);

            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void DateDiff_AddYearsExample1_ReturnsCorrectDifference()
        {
            DateTime valueInput = new DateTime(2000, 2, 29);
            DateTime valueInput2 = new DateTime(2008, 1, 1);

            long expected = 0;
            DateTime ExpectedDateTime = valueInput;
            while (ExpectedDateTime.AddYears(1) < valueInput2)
            {
                ExpectedDateTime = ExpectedDateTime.AddYears(1);
                expected++;
            }

            long actual = valueInput.DateDiff(DateHelper.DateInterval.Year, valueInput2);

            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void DateDiff_AddYearsExample2_ReturnsCorrectDifference()
        {
            DateTime valueInput = new DateTime(2008, 2, 29);
            DateTime valueInput2 = new DateTime(2000, 1, 1);

            long expected = 0;
            DateTime ExpectedDateTime = valueInput;
            while (ExpectedDateTime.AddYears(-1) > valueInput2)
            {
                ExpectedDateTime = ExpectedDateTime.AddYears(-1);
                expected--;
            }

            long actual = valueInput.DateDiff(DateHelper.DateInterval.Year, valueInput2);

            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void DateDiff_LeapYear_ReturnsCorrectDifference()
        {
            DateTime valueInput = new DateTime(2008, 2, 29);
            DateTime valueInput2 = new DateTime(2009, 2, 28);

            long expected = 0;
            long actual = valueInput.DateDiff(DateHelper.DateInterval.Year, valueInput2);

            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void DateDiff_LoopingMonths_ReturnsCorrectDifferenceInMonths()
        {
            bool anyErrors = false;

            for (int dayOffset = 0; dayOffset < 60; dayOffset++)
            {
                DateTime valueInput = new DateTime(2015, 1, 1).AddDays(dayOffset);
                DateTime valueInput2 = new DateTime(2014, 6, 1);

                int startMonthOffset = -50;
                int endMonthOffset = 50;
                long yearOffest = startMonthOffset;

                while (yearOffest <= endMonthOffset)
                {
                    DateTime evaluationDate = valueInput2.AddMonths((int)yearOffest);

                    //// Act
                    long actual = valueInput.DateDiff(DateHelper.DateInterval.Month, evaluationDate);

                    if (evaluationDate < valueInput)
                    {
                        if (!(valueInput.AddMonths((int)actual - 1) < evaluationDate && valueInput.AddMonths((int)actual) >= evaluationDate))
                        {
                            anyErrors = true;
                        }
                    }
                    else if (evaluationDate > valueInput)
                    {
                        if (!(valueInput.AddMonths((int)actual) <= evaluationDate && valueInput.AddMonths((int)actual + 1) > evaluationDate))
                        {
                            anyErrors = true;
                        }
                    }
                    else
                    {
                        if (actual != 0)
                        {
                            anyErrors = true;
                        }
                    }

                    //// Assert for debugging...
                    //if (Expected != actual)
                    //{
                    //    anyErrors = true;
                    //    //// Assert
                    //    Assert.That(actual, Is.EqualTo(expected));
                    //}

                    yearOffest++;
                }
            }

            if (anyErrors)
            {
                //// Assert
                Assert.Fail();
            }
            else
            {
                //// Assert
                //Assert.Pass();
            }
        }

        [Test]
        public void DateDiff_LoopingYears_ReturnsCorrectDifferenceInYears()
        {
            bool anyErrors = false;

            for (int monthOffset = 0; monthOffset < 12; monthOffset++)
            {
                DateTime valueInput = new DateTime(2015, 1 + monthOffset, 1);
                DateTime valueInput2 = new DateTime(2014, 6, 1);

                int startYearOffset = -50;
                int endYearOffset = 50;
                long yearOffest = startYearOffset;

                while (yearOffest <= endYearOffset)
                {
                    DateTime evaluationDate = valueInput2.AddYears((int)yearOffest);

                    //// Act
                    long actual = valueInput.DateDiff(DateHelper.DateInterval.Year, evaluationDate);

                    if (evaluationDate < valueInput)
                    {
                        if (!(valueInput.AddYears((int)actual - 1) < evaluationDate && valueInput.AddYears((int)actual) >= evaluationDate))
                        {
                            anyErrors = true;
                        }
                    }
                    else if (evaluationDate > valueInput)
                    {
                        if (!(valueInput.AddYears((int)actual) <= evaluationDate && valueInput.AddYears((int)actual + 1) > evaluationDate))
                        {
                            anyErrors = true;
                        }
                    }
                    else
                    {
                        if (actual != 0)
                        {
                            anyErrors = true;
                        }
                    }

                    //// Assert for debugging...
                    //if (Expected != actual)
                    //{
                    //    anyErrors = true;
                    //    //// Assert
                    //    Assert.That(actual, Is.EqualTo(expected));
                    //}

                    yearOffest++;
                }
            }

            if (anyErrors)
            {
                //// Assert
                Assert.Fail();
            }
            else
            {
                //// Assert
                ////Assert.Pass();
            }
        }

        [Test]
        public void EndsWithIgnoreCase_EmptyInputValidPattern_ReturnsFalse()
        {
            //// Setup
            string valueInput = string.Empty;
            string valuePattern = "23";
            const bool expected = false;

            //// Act
            bool actual = valueInput.EndsWithNullSafe(valuePattern);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void EndsWithIgnoreCase_NullInputValidPattern_ReturnsFalse()
        {
            //// Setup
            string valueInput = string.Empty;
            string valuePattern = "23";
            const bool expected = false;

            //// Act
            bool actual = valueInput.EndsWithNullSafe(valuePattern);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void EndsWithIgnoreCase_ValidInputDifferentCaseMatchingPattern_ReturnsTrue()
        {
            //// Setup
            string valueInput = "abcdefg";
            string[] valuePatterns = new string[] { "12", "FG" };
            const bool expected = true;

            //// Act
            bool actual = valueInput.EndsWithNullSafe(valuePatterns);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void EndsWithIgnoreCase_ValidInputDifferentCasePattern_ReturnsTrue()
        {
            //// Setup
            string valueInput = "abcdefg";
            string valuePattern = "FG";
            const bool expected = true;

            //// Act
            bool actual = valueInput.EndsWithNullSafe(valuePattern);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void EndsWithIgnoreCase_ValidInputEmptyPattern_ReturnsFalse()
        {
            //// Setup
            string valueInput = "1234567890";
            string valuePattern = "";
            const bool expected = false;

            //// Act
            bool actual = valueInput.EndsWithNullSafe(valuePattern);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void EndsWithIgnoreCase_ValidInputEmptyStringInPatternsOneMatchingPattern_ReturnsTrue()
        {
            //// Setup
            string valueInput = "1234567890";
            string[] valuePatterns = new string[] { string.Empty, "90" };
            const bool expected = true;

            //// Act
            bool actual = valueInput.EndsWithNullSafe(valuePatterns);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void EndsWithIgnoreCase_ValidInputNoMatchingPatterns_ReturnsFalse()
        {
            //// Setup
            string valueInput = "1234567890";
            string[] valuePatterns = new string[] { "zz", "xx" };
            const bool expected = false;

            //// Act
            bool actual = valueInput.EndsWithNullSafe(valuePatterns);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void EndsWithIgnoreCase_ValidInputNonMatchingPattern_ReturnsFalse()
        {
            //// Setup
            string valueInput = "1234567890";
            string valuePattern = "zz";
            const bool expected = false;

            //// Act
            bool actual = valueInput.EndsWithNullSafe(valuePattern);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void EndsWithIgnoreCase_ValidInputNullInPatternsOneMatchingPattern_ReturnsTrue()
        {
            //// Setup
            string valueInput = "1234567890";
            string[] valuePatterns = new string[] { null!, "90" };
            const bool expected = true;

            //// Act
            bool actual = valueInput.EndsWithNullSafe(valuePatterns);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void EndsWithIgnoreCase_ValidInputNullPattern_ReturnsFalse()
        {
            //// Setup
            string valueInput = "1234567890";
            string valuePattern = null!;
            const bool expected = false;

            //// Act
            bool actual = valueInput.EndsWithNullSafe(valuePattern);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void EndsWith_ValidInputMatchingPattern_ReturnsTrue()
        {
            //// Setup
            string valueInput = "1234567890";
            string valuePattern = "90";
            const bool expected = true;

            //// Act
            bool actual = valueInput.EndsWith(valuePattern);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        /// <summary>
        /// Although technically DateTime.MinValue may not be Monday, that's ok since we just want a valid start of a date range.
        /// </summary>
        [Test]
        public void FindLastMonday_MinValueValidInput_ReturnsMinValue()
        {
            //// Setup
            DateTime valueInput = DateTime.MinValue;
            DateTime expected = DateTime.MinValue;

            //// Act
            DateTime actual = valueInput.FindLastMonday();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void FindLastMonday_ValidInput_ReturnsExpectedValue()
        {
            //// Setup
            DateTime valueInput = new DateTime(2014, 2, 15);
            DateTime expected = new DateTime(2014, 2, 10);

            //// Act
            DateTime actual = valueInput.FindLastMonday();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void InCsv_EmptyInput_ReturnsFalse()
        {
            //// Setup
            const string ValueInput = "15/02/2014";
            const bool expected = false;

            //// Act
            bool actual = ValueInput.In(string.Empty, ',');

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void InCsv_MatchingValidInput_ReturnsTrue()
        {
            //// Setup
            const string ValueInput = "15/02/2014";
            const bool expected = true;

            //// Act
            bool actual = ValueInput.In("aaa,15/02/2014,zzz", ',');
            ValueInput.WithDefault("###-###");

            //Type testType = typeof(string);
            //if (testType.In(typeof(int), typeof(bool)))
            //{

            //}

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void InCsv_NoMatchingValidInput_ReturnsFalse()
        {
            //// Setup
            const string ValueInput = "asdfasdfasd";
            const bool expected = false;

            //// Act
            bool actual = ValueInput.In("aaa,15/02/2014,zzz", ',');

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void InCsv_NullInput_ReturnsFalse()
        {
            //// Setup
            const string ValueInput = "15/02/2014";
            const bool expected = false;

            //// Act
            bool actual = ValueInput.In(null!, ',');

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void InsertSpaces_EmptyValueInput_ReturnsEmptyValue()
        {
            //// Setup
            string valueInput = string.Empty;
            string expected = string.Empty;

            //// Act
            string? actual = valueInput.InsertSpaces();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void InsertSpaces_NullInput_ReturnsNull()
        {
            //// Setup
            string? valueInput = null;
            string? expected = null;

            //// Act
            string? actual = valueInput.InsertSpaces();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void InsertSpaces_ValidInputNoProperCase_ReturnsSameValue()
        {
            //// Setup
            string valueInput = "Mystring";
            string expected = "Mystring";

            //// Act
            string? actual = valueInput.InsertSpaces();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void InsertSpaces_ValidInput_ReturnsWhitespaceSeparatedValue()
        {
            //// Setup
            string valueInput = "MyString";
            string expected = "My String";

            //// Act
            string? actual = valueInput.InsertSpaces();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void In_NoMatchingValidInput_ReturnsTrue()
        {
            //// Setup
            const string ValueInput = "asdfasdfasd";
            const bool expected = false;

            //// Act
            bool actual = ValueInput.In("aaa","15/02/2014","zzz");

            ValueInput.ConcatWith("aaa");

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void IsDate_InvalidInputEmptyString_ReturnsFalse()
        {
            //// Setup
            const string ValueInput = "";
            const bool expected = false;

            //// Act
            bool actual = ValueInput.IsDate(string.Empty);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void IsDate_InvalidInput_ReturnsFalse()
        {
            //// Setup
            const string ValueInput = "02/15/2014";
            const bool expected = false;

            //// Act
            bool actual = ValueInput.IsDate();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void IsDate_NullInput_ReturnsFalse()
        {
            //// Setup
            string? valueInput = null;
            const bool expected = false;

            //// Act
            bool actual = valueInput!.IsDate();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void IsDate_ValidInput_ReturnsTrue()
        {
            //// Setup
            const string ValueInput = "15/02/2014";
            const bool expected = true;

            //// Act
            bool actual = ValueInput.IsDate();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ProperCase_EmptyValueInput_ReturnsEmptyValue()
        {
            //// Setup
            string? valueInput = string.Empty;
            string? expected = string.Empty;

            //// Act
            string? actual = valueInput.ProperCase();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ProperCase_NullValueInput_ReturnsEmptyValue()
        {
            //// Setup
            string? valueInput = null;
            string? expected = string.Empty;

            //// Act
            string? actual = valueInput.ProperCase();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ProperCase_ValidInput_ReturnsProperCasedResult()
        {
            //// Setup
            string? valueInput = "sam baker";
            string? expected = "Sam Baker";

            //// Act
            string? actual = valueInput.ProperCase();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void RemoveChars_ValidInputEmptyPattern_ReturnsOriginalValue()
        {
            //// Setup
            string? valueInput = "12345678901234567890";
            string? valuePattern = string.Empty;
            string? expected = "12345678901234567890";

            //// Act
            string? actual = valueInput.RemoveChars(valuePattern!);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void RemoveChars_ValidInputMatchingPattern_RemovesMatchingPattern()
        {
            //// Setup
            string? valueInput = "12345678901234567890";
            string valuePattern = "78";
            string? expected = "1234569012345690";

            //// Act
            string? actual = valueInput.RemoveChars(valuePattern);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void RemoveChars_ValidInputNoMatchingPattern_ReturnsOriginalValue()
        {
            //// Setup
            string? valueInput = "12345678901234567890";
            string valuePattern = "zz";
            string? expected = "12345678901234567890";

            //// Act
            string? actual = valueInput.RemoveChars(valuePattern!);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void RemoveChars_ValidInputNullPattern_ReturnsOriginalValue()
        {
            //// Setup
            string? valueInput = "12345678901234567890";
            string? valuePattern = null;
            string? expected = "12345678901234567890";

            //// Act
            string? actual = valueInput.RemoveChars(valuePattern);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ReplaceEx_ValidInputEmptyPattern_ReturnsOriginalValue()
        {
            //// Setup
            string? valueInput = "12345678901234567890";
            string? valuePattern = string.Empty;
            string? expected = "12345678901234567890";

            //// Act
            string? actual = valueInput.ReplaceEx(valuePattern!, "aa");

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ReplaceEx_ValidInputMatchingPattern_RemovesMatchingPattern()
        {
            //// Setup
            string? valueInput = "12345678901234567890";
            string valuePattern = "78";
            string? expected = "123456aa90123456aa90";

            //// Act
            string? actual = valueInput.ReplaceEx(valuePattern, "aa");

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ReplaceEx_ValidInputNoMatchingPattern_ReturnsOriginalValue()
        {
            //// Setup
            string? valueInput = "12345678901234567890";
            string valuePattern = "zz";
            string? expected = "12345678901234567890";

            //// Act
            string? actual = valueInput.ReplaceEx(valuePattern, "aa");

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ReplaceEx_ValidInputNullPattern_ReturnsOriginalValue()
        {
            //// Setup
            string? valueInput = "12345678901234567890";
            string valuePattern = null!;
            string? expected = "12345678901234567890";

            //// Act
            string? actual = valueInput.ReplaceEx(valuePattern!, "aa");

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        #endregion Methods

        #region Other

        //[Test]
        //public void ContainsDigits_ContainsSomeDigitsParameterdAndPartialDigitsInput_ReturnsTrue()
        //{
        //    //// Setup
        //    string valueInput = null;
        //    const bool Expected = false;
        //    //// Act
        //    bool actual = valueInput.ContainsDigits(StringHelper.ContainsCheckType.ContainsSome);
        //    //// Assert
        //    Assert.That(actual, Is.EqualTo(expected));
        //}

        #endregion Other
    }
}