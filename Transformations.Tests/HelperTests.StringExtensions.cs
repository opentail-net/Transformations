namespace Transformations.Tests
{
    using System;

    using NUnit.Framework;

    using Transformations;

    [TestFixture]
    public class StringExtensionTests
    {
        #region ContainsAllOf

        [Test]
        public void ContainsAllOf_AllValuesPresent_ReturnsTrue()
        {
            //// Setup
            string valueInput = "abcdefg";
            const bool expected = true;

            //// Act
            bool actual = valueInput.ContainsAllOf("abc", "def");

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ContainsAllOf_OneValueMissing_ReturnsFalse()
        {
            //// Setup
            string valueInput = "abcdefg";
            const bool expected = false;

            //// Act
            bool actual = valueInput.ContainsAllOf("abc", "xyz");

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ContainsAllOf_NullInput_ReturnsFalse()
        {
            //// Setup
            string valueInput = null!;
            const bool expected = false;

            //// Act
            bool actual = valueInput.ContainsAllOf("abc");

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ContainsAllOf_EmptyInput_ReturnsFalse()
        {
            //// Setup
            string valueInput = string.Empty;
            const bool expected = false;

            //// Act
            bool actual = valueInput.ContainsAllOf("abc");

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ContainsAllOf_DifferentCase_ReturnsTrue()
        {
            //// Setup
            string valueInput = "abcdefg";
            const bool expected = true;

            //// Act
            bool actual = valueInput.ContainsAllOf("ABC", "DEF");

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        #endregion ContainsAllOf

        #region ContainsAnyOf

        [Test]
        public void ContainsAnyOf_OneValueMatches_ReturnsTrue()
        {
            //// Setup
            string valueInput = "abcdefg";
            const bool expected = true;

            //// Act
            bool actual = valueInput.ContainsAnyOf("xyz", "abc");

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ContainsAnyOf_NoValuesMatch_ReturnsFalse()
        {
            //// Setup
            string valueInput = "abcdefg";
            const bool expected = false;

            //// Act
            bool actual = valueInput.ContainsAnyOf("xyz", "123");

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ContainsAnyOf_NullInput_ReturnsFalse()
        {
            //// Setup
            string valueInput = null!;
            const bool expected = false;

            //// Act
            bool actual = valueInput.ContainsAnyOf("abc");

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ContainsAnyOf_DifferentCase_ReturnsTrue()
        {
            //// Setup
            string valueInput = "abcdefg";
            const bool expected = true;

            //// Act
            bool actual = valueInput.ContainsAnyOf("XYZ", "ABC");

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        #endregion ContainsAnyOf

        #region ContainsNullSafe

        [Test]
        public void ContainsNullSafe_ValuePresent_ReturnsTrue()
        {
            //// Setup
            string valueInput = "hello world";
            const bool expected = true;

            //// Act
            bool actual = valueInput.ContainsNullSafe("world");

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ContainsNullSafe_ValueNotPresent_ReturnsFalse()
        {
            //// Setup
            string valueInput = "hello world";
            const bool expected = false;

            //// Act
            bool actual = valueInput.ContainsNullSafe("xyz");

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ContainsNullSafe_NullInput_ReturnsFalse()
        {
            //// Setup
            string valueInput = null!;
            const bool expected = false;

            //// Act
            bool actual = valueInput.ContainsNullSafe("abc");

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ContainsNullSafe_EmptyPattern_ReturnsFalse()
        {
            //// Setup
            string valueInput = "hello world";
            const bool expected = false;

            //// Act
            bool actual = valueInput.ContainsNullSafe(string.Empty);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        #endregion ContainsNullSafe

        #region ContainsLetters

        [Test]
        public void ContainsLetters_OnlyLetters_ReturnsTrue()
        {
            //// Setup
            string valueInput = "abcdef";
            const bool expected = true;

            //// Act
            bool actual = valueInput.ContainsLetters();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ContainsLetters_MixedInput_ReturnsFalse()
        {
            //// Setup
            string valueInput = "abc123";
            const bool expected = false;

            //// Act
            bool actual = valueInput.ContainsLetters();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ContainsLetters_ContainsSome_ReturnsTrue()
        {
            //// Setup
            string valueInput = "abc123";
            const bool expected = true;

            //// Act
            bool actual = valueInput.ContainsLetters(StringHelper.ContainsCheckType.ContainsSome);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ContainsLetters_NullInput_ReturnsFalse()
        {
            //// Setup
            string? valueInput = null;
            const bool expected = false;

            //// Act
            bool actual = valueInput.ContainsLetters();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        #endregion ContainsLetters

        #region EqualsIgnoreCase

        [Test]
        public void EqualsIgnoreCase_SameCase_ReturnsTrue()
        {
            //// Setup
            string valueInput = "hello";
            const bool expected = true;

            //// Act
            bool actual = valueInput.EqualsIgnoreCase("hello");

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void EqualsIgnoreCase_DifferentCase_ReturnsTrue()
        {
            //// Setup
            string valueInput = "hello";
            const bool expected = true;

            //// Act
            bool actual = valueInput.EqualsIgnoreCase("HELLO");

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void EqualsIgnoreCase_NoMatch_ReturnsFalse()
        {
            //// Setup
            string valueInput = "hello";
            const bool expected = false;

            //// Act
            bool actual = valueInput.EqualsIgnoreCase("world");

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void EqualsIgnoreCase_WithStringComparison_DifferentCase_ReturnsTrue()
        {
            //// Setup
            string valueInput = "hello";
            const bool expected = true;

            //// Act
            bool actual = valueInput.EqualsIgnoreCase("HELLO", StringComparison.OrdinalIgnoreCase);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void EqualsIgnoreCase_WithStringComparison_CaseSensitive_ReturnsFalse()
        {
            //// Setup
            string valueInput = "hello";
            const bool expected = false;

            //// Act
            bool actual = valueInput.EqualsIgnoreCase("HELLO", StringComparison.Ordinal);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void EqualsIgnoreCase_NullInput_ReturnsFalse()
        {
            //// Setup
            string valueInput = null!;
            const bool expected = false;

            //// Act
            bool actual = valueInput.EqualsIgnoreCase("hello");

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        #endregion EqualsIgnoreCase

        #region StartsWithNullSafe

        [Test]
        public void StartsWithNullSafe_MatchingValue_ReturnsTrue()
        {
            //// Setup
            string valueInput = "hello world";
            const bool expected = true;

            //// Act
            bool actual = valueInput.StartsWithNullSafe("hello");

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void StartsWithNullSafe_NoMatch_ReturnsFalse()
        {
            //// Setup
            string valueInput = "hello world";
            const bool expected = false;

            //// Act
            bool actual = valueInput.StartsWithNullSafe("world");

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void StartsWithNullSafe_DifferentCase_ReturnsTrue()
        {
            //// Setup
            string valueInput = "hello world";
            const bool expected = true;

            //// Act
            bool actual = valueInput.StartsWithNullSafe("HELLO");

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void StartsWithNullSafe_NullInput_ReturnsFalse()
        {
            //// Setup
            string valueInput = null!;
            const bool expected = false;

            //// Act
            bool actual = valueInput.StartsWithNullSafe("hello");

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void StartsWithNullSafe_EmptyPattern_ReturnsFalse()
        {
            //// Setup
            string valueInput = "hello world";
            const bool expected = false;

            //// Act
            bool actual = valueInput.StartsWithNullSafe(string.Empty);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void StartsWithNullSafe_MultipleValues_MatchesOne_ReturnsTrue()
        {
            //// Setup
            string valueInput = "hello world";
            const bool expected = true;

            //// Act
            bool actual = valueInput.StartsWithNullSafe("xyz", "HELLO");

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        #endregion StartsWithNullSafe

        #region SubstringNullSafe

        [Test]
        public void SubstringNullSafe_ValidStartIndex_ReturnsSubstring()
        {
            //// Setup
            string valueInput = "hello world";
            string expected = "world";

            //// Act
            string actual = valueInput.SubstringNullSafe(6);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void SubstringNullSafe_StartIndexBeyondEnd_ReturnsEmpty()
        {
            //// Setup
            string valueInput = "hello";
            string expected = string.Empty;

            //// Act
            string actual = valueInput.SubstringNullSafe(100);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void SubstringNullSafe_WithLength_ReturnsCorrectSubstring()
        {
            //// Setup
            string valueInput = "hello world";
            string expected = "hello";

            //// Act
            string actual = valueInput.SubstringNullSafe(0, 5);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void SubstringNullSafe_LengthBeyondEnd_ReturnsSafeResult()
        {
            //// Setup
            string valueInput = "hello";
            string expected = "llo";

            //// Act
            string actual = valueInput.SubstringNullSafe(2, 100);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void SubstringNullSafe_NullInput_ReturnsNull()
        {
            //// Setup
            string valueInput = null!;
            string? expected = null;

            //// Act
            string actual = valueInput.SubstringNullSafe(0);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        #endregion SubstringNullSafe

        #region TrimNullSafe

        [Test]
        public void TrimNullSafe_WhitespacePadded_ReturnsTrimmed()
        {
            //// Setup
            string valueInput = "  hello  ";
            string expected = "hello";

            //// Act
            string actual = valueInput.TrimNullSafe();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void TrimNullSafe_NullInput_ReturnsNull()
        {
            //// Setup
            string valueInput = null!;
            string? expected = null;

            //// Act
            string actual = valueInput.TrimNullSafe();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void TrimNullSafe_MaxLength_TruncatesToLength()
        {
            //// Setup
            string valueInput = "hello world";
            string? expected = "hello";

            //// Act
            string? actual = valueInput.TrimNullSafe(5);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void TrimNullSafe_MaxLengthExceedsInput_ReturnsOriginal()
        {
            //// Setup
            string valueInput = "hi";
            string? expected = "hi";

            //// Act
            string? actual = valueInput.TrimNullSafe(100);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void TrimStartNullSafe_WhitespacePadded_ReturnsTrimmedStart()
        {
            //// Setup
            string valueInput = "  hello  ";
            string expected = "hello  ";

            //// Act
            string actual = valueInput.TrimStartNullSafe();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void TrimEndNullSafe_WhitespacePadded_ReturnsTrimmedEnd()
        {
            //// Setup
            string valueInput = "  hello  ";
            string expected = "  hello";

            //// Act
            string actual = valueInput.TrimEndNullSafe();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        #endregion TrimNullSafe

        #region ToLowerNullSafe / ToUpperNullSafe

        [Test]
        public void ToLowerNullSafe_MixedCase_ReturnsLower()
        {
            //// Setup
            string valueInput = "Hello World";
            string expected = "hello world";

            //// Act
            string actual = valueInput.ToLowerNullSafe();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ToLowerNullSafe_NullInput_ReturnsNull()
        {
            //// Setup
            string valueInput = null!;
            string? expected = null;

            //// Act
            string actual = valueInput.ToLowerNullSafe();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ToUpperNullSafe_MixedCase_ReturnsUpper()
        {
            //// Setup
            string valueInput = "Hello World";
            string expected = "HELLO WORLD";

            //// Act
            string actual = valueInput.ToUpperNullSafe();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ToUpperNullSafe_NullInput_ReturnsNull()
        {
            //// Setup
            string valueInput = null!;
            string? expected = null;

            //// Act
            string actual = valueInput.ToUpperNullSafe();

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        #endregion ToLowerNullSafe / ToUpperNullSafe

        #region RemoveEx

        [Test]
        public void RemoveEx_MatchingPattern_RemovesAllInstances()
        {
            //// Setup
            string valueInput = "hello world hello";
            string expected = " world ";

            //// Act
            string? actual = valueInput.RemoveEx("hello");

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void RemoveEx_MultiplePatterns_RemovesAll()
        {
            //// Setup
            string valueInput = "hello world foo";
            string expected = "  ";

            //// Act
            string? actual = valueInput.RemoveEx("hello", "world", "foo");

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void RemoveEx_NoMatchingPattern_ReturnsOriginal()
        {
            //// Setup
            string valueInput = "hello world";
            string expected = "hello world";

            //// Act
            string? actual = valueInput.RemoveEx("xyz");

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void RemoveEx_NullInput_ReturnsNull()
        {
            //// Setup
            string? valueInput = null;
            string? expected = null;

            //// Act
            string? actual = valueInput.RemoveEx(new System.Collections.Generic.List<string> { "hello" });

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        #endregion RemoveEx

        #region InsertNullSafe

        [Test]
        public void InsertNullSafe_ValidIndex_InsertsValue()
        {
            //// Setup
            string valueInput = "helloworld";
            string expected = "hello world";

            //// Act
            string actual = valueInput.InsertNullSafe(5, " ");

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void InsertNullSafe_IndexZeroNullInput_ReturnsInsertedValue()
        {
            //// Setup
            string valueInput = null!;
            string expected = "hello";

            //// Act
            string actual = valueInput.InsertNullSafe(0, "hello");

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void InsertNullSafe_IndexBeyondEnd_AppendsValue()
        {
            //// Setup
            string valueInput = "hello";
            string expected = "hello world";

            //// Act
            string actual = valueInput.InsertNullSafe(100, " world");

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        #endregion InsertNullSafe

        #region SplitNullSafe

        [Test]
        public void SplitNullSafe_ValidInput_SplitsCorrectly()
        {
            //// Setup
            string valueInput = "a,b,c";
            string[] expected = new[] { "a", "b", "c" };

            //// Act
            string[] actual = valueInput.SplitNullSafe(",");

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void SplitNullSafe_NullInput_ReturnsEmptyArray()
        {
            //// Setup
            string valueInput = null!;

            //// Act
            string[] actual = valueInput.SplitNullSafe(",");

            //// Assert
            Assert.That(actual, Is.Empty);
        }

        [Test]
        public void SplitNullSafe_EmptyInput_ReturnsEmptyArray()
        {
            //// Setup
            string valueInput = string.Empty;

            //// Act
            string[] actual = valueInput.SplitNullSafe(",");

            //// Assert
            Assert.That(actual, Is.Empty);
        }

        [Test]
        public void SplitNullSafe_NoSeparator_ReturnsOriginalAsSingleElement()
        {
            //// Setup
            string valueInput = "hello";
            string[] expected = new[] { "hello" };

            //// Act
            string[] actual = valueInput.SplitNullSafe(new string[0]);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        #endregion SplitNullSafe

        #region WithDefault

        [Test]
        public void WithDefault_NullInput_ReturnsDefault()
        {
            //// Setup
            string? valueInput = null;
            string expected = "default";

            //// Act
            string actual = valueInput!.WithDefault("default");

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void WithDefault_EmptyInput_ReturnsDefault()
        {
            //// Setup
            string valueInput = string.Empty;
            string expected = "default";

            //// Act
            string actual = valueInput.WithDefault("default");

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void WithDefault_ValidInput_ReturnsInput()
        {
            //// Setup
            string valueInput = "hello";
            string expected = "hello";

            //// Act
            string actual = valueInput.WithDefault("default");

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        #endregion WithDefault

        #region ConcatWith

        [Test]
        public void ConcatWith_ValidInputs_ReturnsConcatenatedString()
        {
            //// Setup
            string valueInput = "hello";
            string expected = "hello world";

            //// Act
            string actual = valueInput.ConcatWith(" world");

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ConcatWith_MultipleValues_ReturnsConcatenatedString()
        {
            //// Setup
            string valueInput = "a";
            string expected = "abcd";

            //// Act
            string actual = valueInput.ConcatWith("b", "c", "d");

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        #endregion ConcatWith

        #region CountSubstring

        [Test]
        public void CountSubstring_MultipleOccurrences_ReturnsCorrectCount()
        {
            //// Setup
            string valueInput = "abababab";
            int expected = 4;

            //// Act
            int actual = valueInput.CountSubstring("ab", 0, valueInput.Length, StringComparison.Ordinal);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void CountSubstring_NoOccurrences_ReturnsZero()
        {
            //// Setup
            string valueInput = "hello world";
            int expected = 0;

            //// Act
            int actual = valueInput.CountSubstring("xyz", 0, valueInput.Length, StringComparison.Ordinal);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void CountSubstring_NullInput_ReturnsZero()
        {
            //// Setup
            string valueInput = null!;
            int expected = 0;

            //// Act
            int actual = valueInput.CountSubstring("ab", 0, 0, StringComparison.Ordinal);

            //// Assert
            Assert.That(actual, Is.EqualTo(expected));
        }

        #endregion CountSubstring
    }
}
