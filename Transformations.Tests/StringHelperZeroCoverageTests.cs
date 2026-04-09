namespace Transformations.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Xml;
    using System.Xml.Linq;

    using NUnit.Framework;

    [TestFixture]
    public class StringHelperZeroCoverageTests
    {
        #region GetAnyCaps

        [Test]
        public void GetAnyCaps_MixedCase_ReturnsUpperOnly()
        {
            //// Act
            string actual = "Hello World".GetAnyCaps();

            //// Assert
            Assert.That(actual, Is.EqualTo("HW"));
        }

        [Test]
        public void GetAnyCaps_AllLower_ReturnsEmpty()
        {
            Assert.That("hello".GetAnyCaps(), Is.EqualTo(string.Empty));
        }

        [Test]
        public void GetAnyCaps_Empty_ReturnsEmpty()
        {
            Assert.That(string.Empty.GetAnyCaps(), Is.EqualTo(string.Empty));
        }

        #endregion GetAnyCaps

        #region GetDomainFromUrlString

        [Test]
        public void GetDomainFromUrlString_FullUrl_ReturnsDomain()
        {
            //// Act
            string actual = "https://www.example.com/page?q=1".GetDomainFromUrlString();

            //// Assert
            Assert.That(actual, Is.EqualTo("www.example.com"));
        }

        [Test]
        public void GetDomainFromUrlString_NoDomain_ReturnsEmpty()
        {
            Assert.That(string.Empty.GetDomainFromUrlString(), Is.EqualTo(string.Empty));
        }

        [Test]
        public void GetDomainFromUrlString2_WithProtocol_ReturnsDomain()
        {
            //// Act
            string actual = "http://example.com/path".GetDomainFromUrlString2();

            //// Assert
            Assert.That(actual, Is.EqualTo("example.com"));
        }

        [Test]
        public void GetDomainFromUrlString2_NoProtocol_ReturnsDomain()
        {
            //// Act
            string actual = "example.com/path".GetDomainFromUrlString2();

            //// Assert
            Assert.That(actual, Is.EqualTo("example.com"));
        }

        #endregion GetDomainFromUrlString

        #region PadLeftNullSafe / PadRightNullSafe

        [Test]
        public void PadLeftNullSafe_ShortString_PadsCorrectly()
        {
            Assert.That("Hi".PadLeftNullSafe(5), Is.EqualTo("   Hi"));
        }

        [Test]
        public void PadLeftNullSafe_NullInput_ReturnsPaddedEmpty()
        {
            Assert.That(((string)null!).PadLeftNullSafe(3), Has.Length.EqualTo(3));
        }

        [Test]
        public void PadRightNullSafe_ShortString_PadsCorrectly()
        {
            Assert.That("Hi".PadRightNullSafe(5), Is.EqualTo("Hi   "));
        }

        [Test]
        public void PadRightNullSafe_NullInput_ReturnsPaddedEmpty()
        {
            Assert.That(((string)null!).PadRightNullSafe(3), Has.Length.EqualTo(3));
        }

        #endregion PadLeftNullSafe / PadRightNullSafe

        #region ReplaceAll

        [Test]
        public void ReplaceAll_WithNewValue_ReplacesAllOldValues()
        {
            //// Setup
            string input = "White Red Blue White";
            var oldValues = new[] { "White" };

            //// Act
            string actual = input.ReplaceAll(oldValues, "[Color]");

            //// Assert
            Assert.That(actual, Is.EqualTo("[Color] Red Blue [Color]"));
        }

        [Test]
        public void ReplaceAll_WithPredicate_TransformsMatches()
        {
            //// Setup
            string input = "Red Blue Green";
            var oldValues = new[] { "Red", "Green" };

            //// Act
            string actual = input.ReplaceAll(oldValues, v => "[" + v + "]");

            //// Assert
            Assert.That(actual, Is.EqualTo("[Red] Blue [Green]"));
        }

        [Test]
        public void ReplaceAll_WithMatchingLists_ReplacesCorrectly()
        {
            //// Setup
            string input = "Red Blue Green";
            var oldValues = new[] { "Red", "Green" };
            var newValues = new[] { "Crimson", "Lime" };

            //// Act
            string actual = input.ReplaceAll(oldValues, newValues);

            //// Assert
            Assert.That(actual, Is.EqualTo("Crimson Blue Lime"));
        }

        #endregion ReplaceAll

        #region ToTitleCase

        [Test]
        public void ToTitleCase_AllLower_CapitalisesFirstLetters()
        {
            //// Act
            string actual = "hello world".ToTitleCase();

            //// Assert
            Assert.That(actual, Is.EqualTo("Hello World"));
        }

        [Test]
        public void ToTitleCase_EmptyString_ReturnsEmpty()
        {
            Assert.That(string.Empty.ToTitleCase(), Is.EqualTo(string.Empty));
        }

        #endregion ToTitleCase

        #region JoinWithStrings / JoinWithObjects

        [Test]
        public void JoinWithStrings_ValidInputs_JoinsCorrectly()
        {
            //// Act
            string actual = "first".JoinWithStrings(", ", "second", "third");

            //// Assert
            Assert.That(actual, Does.Contain("first"));
            Assert.That(actual, Is.Not.Empty);
        }

        [Test]
        public void JoinWithStrings_NoValues_ReturnsOriginal()
        {
            //// Act
            string actual = "original".JoinWithStrings(", ");

            //// Assert
            Assert.That(actual, Is.EqualTo("original"));
        }

        [Test]
        public void JoinWithObjects_EmptyInput_JoinsValues()
        {
            //// Act - when inputText is empty, values are joined directly
            string actual = string.Empty.JoinWithObjects("-", 42, "end");

            //// Assert
            Assert.That(actual, Does.Contain("42"));
            Assert.That(actual, Does.Contain("end"));
        }

        #endregion JoinWithStrings / JoinWithObjects

        #region ContainsInString

        [Test]
        public void ContainsInString_MatchingSuspect_ReturnsTrue()
        {
            //// Act
            bool actual = "Hello World".ContainsInString(new[] { "Hello" });

            //// Assert
            Assert.That(actual, Is.True);
        }

        [Test]
        public void ContainsInString_NoMatch_ReturnsFalse()
        {
            //// Act
            bool actual = "Hello World".ContainsInString(new[] { "xyz" });

            //// Assert
            Assert.That(actual, Is.False);
        }

        #endregion ContainsInString

        #region ToXDocument / ToXmlDocument

        [Test]
        public void ToXDocument_ValidXml_ReturnsXDocument()
        {
            //// Setup
            string xml = "<root><item>test</item></root>";

            //// Act
            XDocument actual = xml.ToXDocument();

            //// Assert
            Assert.That(actual, Is.Not.Null);
            Assert.That(actual.Root!.Name.LocalName, Is.EqualTo("root"));
        }

        [Test]
        public void ToXmlDocument_ValidXml_ReturnsXmlDocument()
        {
            //// Setup
            string xml = "<root><item>test</item></root>";

            //// Act
            XmlDocument actual = xml.ToXmlDocument();

            //// Assert
            Assert.That(actual, Is.Not.Null);
            Assert.That(actual.DocumentElement!.Name, Is.EqualTo("root"));
        }

        #endregion ToXDocument / ToXmlDocument

        #region TryFormat

        [Test]
        public void TryFormat_ValidFormat_ReturnsTrue()
        {
            //// Act
            bool success = "Hello {0}".TryFormat(out string result, "World");

            //// Assert
            Assert.That(success, Is.True);
            Assert.That(result, Is.EqualTo("Hello World"));
        }

        [Test]
        public void TryFormat_InvalidFormat_ReturnsFalse()
        {
            //// Act
            bool success = "Hello {999}".TryFormat(out string result, "World");

            //// Assert
            Assert.That(success, Is.False);
        }

        #endregion TryFormat

        #region IndexOfAnyNullSafe / LastIndexOfAnyNullSafe / LastIndexOfNullSafe

        [Test]
        public void IndexOfAnyNullSafe_Found_ReturnsIndex()
        {
            Assert.That("hello".IndexOfAnyNullSafe(new[] { 'l', 'o' }), Is.EqualTo(2));
        }

        [Test]
        public void IndexOfAnyNullSafe_NullInput_ReturnsMinusOne()
        {
            Assert.That(((string)null!).IndexOfAnyNullSafe(new[] { 'a' }), Is.EqualTo(-1));
        }

        [Test]
        public void LastIndexOfNullSafe_Found_ReturnsLastIndex()
        {
            Assert.That("hello world".LastIndexOfNullSafe("o"), Is.EqualTo(7));
        }

        [Test]
        public void LastIndexOfNullSafe_NullInput_ReturnsMinusOne()
        {
            Assert.That(((string)null!).LastIndexOfNullSafe("a"), Is.EqualTo(-1));
        }

        [Test]
        public void LastIndexOfAnyNullSafe_Found_ReturnsLastIndex()
        {
            Assert.That("hello".LastIndexOfAnyNullSafe(new[] { 'l' }), Is.EqualTo(3));
        }

        [Test]
        public void LastIndexOfAnyNullSafe_NullInput_ReturnsMinusOne()
        {
            Assert.That(((string)null!).LastIndexOfAnyNullSafe(new[] { 'a' }), Is.EqualTo(-1));
        }

        #endregion IndexOfAnyNullSafe / LastIndexOfAnyNullSafe / LastIndexOfNullSafe

        #region EndsWithNullSafe

        [Test]
        public void EndsWithNullSafe_MatchingValue_ReturnsTrue()
        {
            Assert.That("hello.txt".EndsWithNullSafe(".txt"), Is.True);
        }

        [Test]
        public void EndsWithNullSafe_NoMatch_ReturnsFalse()
        {
            Assert.That("hello.txt".EndsWithNullSafe(".csv"), Is.False);
        }

        [Test]
        public void EndsWithNullSafe_NullInput_ReturnsFalse()
        {
            Assert.That(((string)null!).EndsWithNullSafe(".txt"), Is.False);
        }

        #endregion EndsWithNullSafe

        #region ContainsDigits

        [Test]
        public void ContainsDigits_OnlyDigits_ReturnsTrue()
        {
            Assert.That("12345".ContainsDigits(StringHelper.ContainsCheckType.ContainsOnly), Is.True);
        }

        [Test]
        public void ContainsDigits_MixedContent_ContainsSome_ReturnsTrue()
        {
            Assert.That("abc123".ContainsDigits(StringHelper.ContainsCheckType.ContainsSome), Is.True);
        }

        [Test]
        public void ContainsDigits_NoDigits_ContainsSome_ReturnsFalse()
        {
            Assert.That("abcdef".ContainsDigits(StringHelper.ContainsCheckType.ContainsSome), Is.False);
        }

        #endregion ContainsDigits

        #region ContainsIgnoreCase

        [Test]
        public void ContainsIgnoreCase_DifferentCase_ReturnsTrue()
        {
            Assert.That("Hello World".ContainsIgnoreCase("hello world", StringComparison.OrdinalIgnoreCase), Is.True);
        }

        [Test]
        public void ContainsIgnoreCase_NoMatch_ReturnsFalse()
        {
            Assert.That("Hello World".ContainsIgnoreCase("xyz", StringComparison.OrdinalIgnoreCase), Is.False);
        }

        #endregion ContainsIgnoreCase
    }
}
