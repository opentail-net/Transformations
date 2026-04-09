namespace Transformations.Tests
{
    using System;
    using System.Collections.Generic;

    using NUnit.Framework;

    [TestFixture]
    public class StringHelperExtendedTests
    {
        #region Left

        [Test]
        public void Left_ValidCount_ReturnsLeftPart()
        {
            //// Setup
            string input = "Hello World";

            //// Act
            string actual = input.Left(5);

            //// Assert
            Assert.That(actual, Is.EqualTo("Hello"));
        }

        [Test]
        public void Left_CountExceedsLength_ReturnsFullString()
        {
            //// Setup
            string input = "Hi";

            //// Act
            string actual = input.Left(10);

            //// Assert
            Assert.That(actual, Is.EqualTo("Hi"));
        }

        [Test]
        public void Left_ZeroCount_ReturnsEmpty()
        {
            //// Setup
            string input = "Hello";

            //// Act
            string actual = input.Left(0);

            //// Assert
            Assert.That(actual, Is.EqualTo(string.Empty));
        }

        [Test]
        public void Left_ByPattern_ReturnsPartBeforePattern()
        {
            //// Setup
            string input = "user@example.com";

            //// Act
            string actual = input.Left("@");

            //// Assert
            Assert.That(actual, Is.EqualTo("user"));
        }

        [Test]
        public void Left_ByPattern_NotFound_ThrowsArgumentOutOfRange()
        {
            //// Setup
            string input = "noatsign";

            //// Act / Assert - IndexOfNullSafe returns -1, passed as length to SubstringNullSafe
            Assert.Throws<ArgumentOutOfRangeException>(() => input.Left("@"));
        }

        #endregion Left

        #region Right

        [Test]
        public void Right_ValidCount_ReturnsRightPart()
        {
            //// Setup
            string input = "Hello World";

            //// Act
            string actual = input.Right(5);

            //// Assert
            Assert.That(actual, Is.EqualTo("World"));
        }

        [Test]
        public void Right_NullInput_ReturnsNull()
        {
            //// Setup
            string input = null!;

            //// Act
            string actual = input.Right(5);

            //// Assert
            Assert.That(actual, Is.Null);
        }

        [Test]
        public void Right_ByPattern_ReturnsPartAfterPattern()
        {
            //// Setup
            string input = "user@example.com";

            //// Act
            string actual = input.Right("@");

            //// Assert
            Assert.That(actual, Is.EqualTo("example.com"));
        }

        [Test]
        public void Right_ByPattern_NotFound_ReturnsEmpty()
        {
            //// Setup
            string input = "noatsign";

            //// Act
            string actual = input.Right("@");

            //// Assert
            Assert.That(actual, Is.EqualTo(string.Empty));
        }

        #endregion Right

        #region GetInitials

        [Test]
        public void GetInitials_TwoWords_ReturnsTwoLetters()
        {
            //// Setup
            string input = "Hello World";

            //// Act
            string actual = input.GetInitials();

            //// Assert
            Assert.That(actual, Is.EqualTo("HW"));
        }

        [Test]
        public void GetInitials_ThreeWords_ReturnsThreeLetters()
        {
            //// Setup
            string input = "John James Smith";

            //// Act
            string actual = input.GetInitials();

            //// Assert
            Assert.That(actual, Is.EqualTo("JJS"));
        }

        [Test]
        public void GetInitials_EmptyString_ReturnsEmpty()
        {
            //// Setup
            string input = string.Empty;

            //// Act
            string actual = input.GetInitials();

            //// Assert
            Assert.That(actual, Is.EqualTo(string.Empty));
        }

        [Test]
        public void GetInitials_SingleWord_ReturnsSingleLetter()
        {
            //// Setup
            string input = "Hello";

            //// Act
            string actual = input.GetInitials();

            //// Assert
            Assert.That(actual, Is.EqualTo("H"));
        }

        #endregion GetInitials

        #region FormatWithMask

        [Test]
        public void FormatWithMask_PhoneNumber_FormatsCorrectly()
        {
            //// Setup
            string input = "1234567890";
            string mask = "(###) ###-####";

            //// Act
            string actual = input.FormatWithMask(mask);

            //// Assert
            Assert.That(actual, Is.EqualTo("(123) 456-7890"));
        }

        [Test]
        public void FormatWithMask_ShortInput_FillsPartially()
        {
            //// Setup
            string input = "abc";
            string mask = "###-#";

            //// Act
            string actual = input.FormatWithMask(mask);

            //// Assert
            Assert.That(actual, Is.EqualTo("abc-"));
        }

        [Test]
        public void FormatWithMask_EmptyInput_ReturnsEmpty()
        {
            //// Setup
            string input = string.Empty;

            //// Act
            string actual = input.FormatWithMask("###-####");

            //// Assert
            Assert.That(actual, Is.EqualTo(string.Empty));
        }

        #endregion FormatWithMask

        #region ProperCase

        [Test]
        public void ProperCase_AllLower_CapitalisesEachWord()
        {
            //// Setup
            string input = "hello world";

            //// Act
            string? actual = input.ProperCase();

            //// Assert
            Assert.That(actual, Is.EqualTo("Hello World"));
        }

        [Test]
        public void ProperCase_AllUpper_ConvertsToProperCase()
        {
            //// Setup
            string input = "HELLO WORLD";

            //// Act
            string? actual = input.ProperCase();

            //// Assert
            Assert.That(actual, Is.EqualTo("Hello World"));
        }

        [Test]
        public void ProperCase_NullInput_ReturnsEmpty()
        {
            //// Act
            string? actual = ((string?)null).ProperCase();

            //// Assert
            Assert.That(actual, Is.EqualTo(string.Empty));
        }

        #endregion ProperCase

        #region RemoveChars

        [Test]
        public void RemoveChars_RemovesMatchingCharacters()
        {
            //// Setup
            string input = "H3ll0 W0rld";

            //// Act
            string? actual = input.RemoveChars("0123456789");

            //// Assert
            Assert.That(actual, Is.EqualTo("Hll Wrld"));
        }

        [Test]
        public void RemoveChars_NoMatch_ReturnsOriginal()
        {
            //// Setup
            string input = "Hello";

            //// Act
            string? actual = input.RemoveChars("xyz");

            //// Assert
            Assert.That(actual, Is.EqualTo("Hello"));
        }

        [Test]
        public void RemoveChars_NullInput_ReturnsNull()
        {
            //// Act
            string? actual = ((string?)null).RemoveChars("abc");

            //// Assert
            Assert.That(actual, Is.Null);
        }

        #endregion RemoveChars

        #region RemoveFirst / RemoveLast

        [Test]
        public void RemoveFirst_ValidString_RemovesFirstCharacter()
        {
            //// Setup
            string input = "Hello";

            //// Act
            string? actual = input.RemoveFirst();

            //// Assert
            Assert.That(actual, Is.EqualTo("ello"));
        }

        [Test]
        public void RemoveFirst_NullInput_ReturnsNull()
        {
            //// Act
            string? actual = ((string?)null).RemoveFirst();

            //// Assert
            Assert.That(actual, Is.Null);
        }

        [Test]
        public void RemoveLast_ValidString_RemovesLastCharacter()
        {
            //// Setup
            string input = "Hello";

            //// Act
            string? actual = input.RemoveLast();

            //// Assert
            Assert.That(actual, Is.EqualTo("Hell"));
        }

        [Test]
        public void RemoveLast_NullInput_ReturnsNull()
        {
            //// Act
            string? actual = ((string?)null).RemoveLast();

            //// Assert
            Assert.That(actual, Is.Null);
        }

        #endregion RemoveFirst / RemoveLast

        #region ParseCommandLine

        [Test]
        public void ParseCommandLine_SimpleArgs_SplitsCorrectly()
        {
            //// Setup
            string input = "app --flag value";

            //// Act
            string[] actual = input.ParseCommandLine();

            //// Assert
            Assert.That(actual.Length, Is.EqualTo(3));
            Assert.That(actual[0], Is.EqualTo("app"));
            Assert.That(actual[1], Is.EqualTo("--flag"));
            Assert.That(actual[2], Is.EqualTo("value"));
        }

        [Test]
        public void ParseCommandLine_QuotedArgs_PreservesSpacesInQuotes()
        {
            //// Setup
            string input = "app \"hello world\" other";

            //// Act
            string[] actual = input.ParseCommandLine();

            //// Assert
            Assert.That(actual.Length, Is.EqualTo(3));
            Assert.That(actual[1], Is.EqualTo("\"hello world\""));
        }

        [Test]
        public void ParseCommandLine_EmptyInput_ReturnsEmptyArray()
        {
            //// Act
            string[] actual = string.Empty.ParseCommandLine();

            //// Assert
            Assert.That(actual, Is.Empty);
        }

        #endregion ParseCommandLine

        #region ToDictionary

        [Test]
        public void ToDictionary_QueryString_ParsesCorrectly()
        {
            //// Setup
            string input = "key1=value1&key2=value2";

            //// Act
            Dictionary<string, string> actual = input.ToDictionary("&");

            //// Assert
            Assert.That(actual.Count, Is.EqualTo(2));
            Assert.That(actual["key1"], Is.EqualTo("value1"));
            Assert.That(actual["key2"], Is.EqualTo("value2"));
        }

        [Test]
        public void ToDictionary_CustomSeparatorAndEquals_ParsesCorrectly()
        {
            //// Setup
            string input = "name:John;age:30";

            //// Act
            Dictionary<string, string> actual = input.ToDictionary(";", ":");

            //// Assert
            Assert.That(actual.Count, Is.EqualTo(2));
            Assert.That(actual["name"], Is.EqualTo("John"));
            Assert.That(actual["age"], Is.EqualTo("30"));
        }

        [Test]
        public void ToDictionary_DuplicateKeys_KeepsFirst()
        {
            //// Setup
            string input = "key=first&key=second";

            //// Act
            Dictionary<string, string> actual = input.ToDictionary("&");

            //// Assert
            Assert.That(actual.Count, Is.EqualTo(1));
            Assert.That(actual["key"], Is.EqualTo("first"));
        }

        #endregion ToDictionary

        #region ReplaceEx (case-insensitive replace)

        [Test]
        public void ReplaceEx_CaseInsensitive_ReplacesAllOccurrences()
        {
            //// Setup
            string input = "Hello hello HELLO";

            //// Act
            string actual = input.ReplaceEx("hello", "hi");

            //// Assert
            Assert.That(actual, Is.EqualTo("hi hi hi"));
        }

        [Test]
        public void ReplaceEx_NoMatch_ReturnsOriginal()
        {
            //// Setup
            string input = "Hello World";

            //// Act
            string actual = input.ReplaceEx("xyz", "abc");

            //// Assert
            Assert.That(actual, Is.EqualTo("Hello World"));
        }

        [Test]
        public void ReplaceEx_NullInput_ReturnsNull()
        {
            //// Act
            string actual = ((string)null!).ReplaceEx("a", "b");

            //// Assert
            Assert.That(actual, Is.Null);
        }

        #endregion ReplaceEx (case-insensitive replace)

        #region GetFirstCharacter

        [Test]
        public void GetFirstCharacter_MultipleChars_ReturnsFirst()
        {
            //// Setup
            string input = "Hello";

            //// Act
            string actual = input.GetFirstCharacter();

            //// Assert
            Assert.That(actual, Is.EqualTo("H"));
        }

        [Test]
        public void GetFirstCharacter_SingleChar_ReturnsSame()
        {
            //// Setup
            string input = "X";

            //// Act
            string actual = input.GetFirstCharacter();

            //// Assert
            Assert.That(actual, Is.EqualTo("X"));
        }

        #endregion GetFirstCharacter
    }
}
