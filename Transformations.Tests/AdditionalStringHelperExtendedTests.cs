namespace Transformations.Tests
{
    using System;

    using NUnit.Framework;

    [TestFixture]
    public class AdditionalStringHelperExtendedTests
    {
        #region HtmlEncode / HtmlDecode

        [Test]
        public void HtmlEncode_SpecialCharacters_EncodesCorrectly()
        {
            //// Setup
            string input = "<b>Hello & \"World\"</b>";

            //// Act
            string actual = input.HtmlEncode();

            //// Assert
            Assert.That(actual, Does.Contain("&lt;"));
            Assert.That(actual, Does.Contain("&gt;"));
            Assert.That(actual, Does.Contain("&amp;"));
        }

        [Test]
        public void HtmlEncode_HtmlDecode_RoundTrip_ReturnsOriginal()
        {
            //// Setup
            string input = "<div class=\"test\">Hello & World</div>";

            //// Act
            string encoded = input.HtmlEncode();
            string decoded = encoded.HtmlDecode();

            //// Assert
            Assert.That(decoded, Is.EqualTo(input));
        }

        [Test]
        public void HtmlDecode_EncodedInput_DecodesCorrectly()
        {
            //// Setup
            string input = "&lt;b&gt;Hello&lt;/b&gt;";

            //// Act
            string actual = input.HtmlDecode();

            //// Assert
            Assert.That(actual, Is.EqualTo("<b>Hello</b>"));
        }

        #endregion HtmlEncode / HtmlDecode

        #region UrlEncode / UrlDecode

        [Test]
        public void UrlEncode_SpacesAndSpecials_EncodesCorrectly()
        {
            //// Setup
            string input = "hello world&foo=bar";

            //// Act
            string actual = input.UrlEncode();

            //// Assert
            Assert.That(actual, Does.Not.Contain(" "));
            Assert.That(actual, Does.Contain("hello"));
        }

        [Test]
        public void UrlEncode_UrlDecode_RoundTrip_ReturnsOriginal()
        {
            //// Setup
            string input = "name=John Doe&city=New York";

            //// Act
            string encoded = input.UrlEncode();
            string decoded = encoded.UrlDecode();

            //// Assert
            Assert.That(decoded, Is.EqualTo(input));
        }

        #endregion UrlEncode / UrlDecode

        #region ToPlural

        [Test]
        public void ToPlural_RegularWord_AddsS()
        {
            //// Setup
            string input = "car";

            //// Act
            string actual = input.ToPlural();

            //// Assert
            Assert.That(actual, Is.EqualTo("cars"));
        }

        [Test]
        public void ToPlural_WordEndingInS_AddsEs()
        {
            //// Setup
            string input = "bus";

            //// Act
            string actual = input.ToPlural();

            //// Assert
            Assert.That(actual, Is.EqualTo("buses"));
        }

        [Test]
        public void ToPlural_WordEndingInY_ChangesToIes()
        {
            //// Setup
            string input = "baby";

            //// Act
            string actual = input.ToPlural();

            //// Assert
            Assert.That(actual, Is.EqualTo("babies"));
        }

        [Test]
        public void ToPlural_CountOfOne_ReturnsSingular()
        {
            //// Setup
            string input = "car";

            //// Act
            string actual = input.ToPlural(1);

            //// Assert
            Assert.That(actual, Is.EqualTo("car"));
        }

        #endregion ToPlural

        #region Pluralize

        [TestCase("User", 0, "Users")]
        [TestCase("User", 2, "Users")]
        [TestCase("User", 1, "User")]
        [TestCase("Box", 5, "Boxes")]
        [TestCase("city", 3, "cities")]
        [TestCase("child", 2, "children")]
        [TestCase("person", 10, "people")]
        [TestCase("tooth", 2, "teeth")]
        [TestCase("foot", 4, "feet")]
        [TestCase("goose", 3, "geese")]
        [TestCase("ox", 2, "oxen")]
        [TestCase("bus", 2, "buses")]
        [TestCase("tomato", 3, "tomatoes")]
        [TestCase("quiz", 2, "quizzes")]
        [TestCase("datum", 5, "data")]
        [TestCase("wife", 2, "wives")]
        [TestCase("leaf", 2, "leaves")]
        [TestCase("mouse", 2, "mice")]
        [TestCase("axis", 2, "axes")]
        [TestCase("status", 2, "statuses")]
        [TestCase("city", -1, "cities")]
        [TestCase("", 5, "")]
        public void Pluralize_CoversRulesAndEdgeCases(string input, int count, string expected)
        {
            Assert.That(input.Pluralize(count), Is.EqualTo(expected));
        }

        [Test]
        public void Pluralize_NullInput_ReturnsNull()
        {
            //// Setup
            string? input = null;

            //// Act
            string actual = input!.Pluralize(2);

            //// Assert
            Assert.That(actual, Is.Null);
        }

        #endregion Pluralize

        #region StripHtmlScripts

        [Test]
        public void StripHtmlScripts_WithScriptTag_NeutralisesScript()
        {
            //// Setup
            string input = "Hello<script>alert('xss')</script>World";

            //// Act
            string actual = input.StripHtmlScripts();

            //// Assert
            Assert.That(actual, Does.Not.Contain("<script>"));
            Assert.That(actual, Does.Not.Contain("</script>"));
            Assert.That(actual, Does.Contain("Hello"));
            Assert.That(actual, Does.Contain("World"));
        }

        [Test]
        public void StripHtmlScripts_NoScript_ReturnsOriginal()
        {
            //// Setup
            string input = "Hello World";

            //// Act
            string actual = input.StripHtmlScripts();

            //// Assert
            Assert.That(actual, Is.EqualTo("Hello World"));
        }

        #endregion StripHtmlScripts

        #region SanitizeHtml

        [Test]
        public void SanitizeHtml_StripAll_RemovesAllTags()
        {
            //// Setup
            string input = "<div><b>Hello</b> <a href='x'>World</a></div>";

            //// Act
            string actual = input.SanitizeHtml(HtmlSanitizationPolicy.StripAll);

            //// Assert
            Assert.That(actual, Is.EqualTo("Hello World"));
        }

        [Test]
        public void SanitizeHtml_PermitInlineFormatting_KeepsFormattingTagsOnly()
        {
            //// Setup
            string input = "<b>Bold</b><script>alert(1)</script><u>Under</u><a href='x'>Link</a>";

            //// Act
            string actual = input.SanitizeHtml(HtmlSanitizationPolicy.PermitInlineFormatting);

            //// Assert
            Assert.That(actual, Does.Contain("<b>Bold</b>"));
            Assert.That(actual, Does.Contain("<u>Under</u>"));
            Assert.That(actual, Does.Not.Contain("<a"));
            Assert.That(actual, Does.Not.Contain("script"));
        }

        [Test]
        public void SanitizeHtml_PermitLinks_StripsOnAttributesAndJavascriptHref()
        {
            //// Setup
            string input = "<a href='javascript:alert(1)' onclick='doBad()'>Bad</a><a href='https://example.com' onmouseover='x'>Good</a>";

            //// Act
            string actual = input.SanitizeHtml(HtmlSanitizationPolicy.PermitLinks);

            //// Assert
            Assert.That(actual, Does.Not.Contain("onclick"));
            Assert.That(actual, Does.Not.Contain("onmouseover"));
            Assert.That(actual, Does.Not.Contain("javascript:"));
            Assert.That(actual, Does.Contain("<a href=\"https://example.com\">Good</a>"));
        }

        [Test]
        public void SanitizeHtml_MalformedHtml_DoesNotThrow()
        {
            //// Setup
            string input = "<div><b>Broken";

            //// Act / Assert
            Assert.DoesNotThrow(() => input.SanitizeHtml(HtmlSanitizationPolicy.PermitLinks));
        }

        #endregion SanitizeHtml

        #region StripHtml (with maxLength)

        [Test]
        public void StripHtml_WithMaxLength_TruncatesInput()
        {
            //// Setup
            string input = "<p>Hello World this is a long paragraph</p>";
            int maxLength = 11;

            //// Act
            string actual = input.StripHtml(maxLength);

            //// Assert - truncates source to maxLength chars then strips + appends "..."
            Assert.That(actual.Length, Is.LessThan(input.StripHtml().Length));
        }

        #endregion StripHtml (with maxLength)

        #region ToBase64String (with encoding)

        [Test]
        public void ToBase64String_DefaultEncoding_RoundTrips()
        {
            //// Setup
            string input = "Hello World!";

            //// Act
            string base64 = input.ToBase64String();
            string decoded = base64.FromBase64String();

            //// Assert
            Assert.That(decoded, Is.EqualTo(input));
        }

        [Test]
        public void ToBase64String_SpecificEncoding_RoundTrips()
        {
            //// Setup
            string input = "Test with UTF8";

            //// Act
            string base64 = input.ToBase64String(System.Text.Encoding.UTF8);
            string decoded = base64.FromBase64String(System.Text.Encoding.UTF8);

            //// Assert
            Assert.That(decoded, Is.EqualTo(input));
        }

        #endregion ToBase64String (with encoding)

        #region TruncateSemantic

        [Test]
        public void TruncateSemantic_WordAware_DoesNotCutMidWord()
        {
            //// Setup
            string input = "Hello wonderful world";

            //// Act
            string actual = input.TruncateSemantic(12);

            //// Assert
            Assert.That(actual, Is.EqualTo("Hello...") );
        }

        [Test]
        public void TruncateSemantic_EntitySafe_DoesNotCutInsideEntity()
        {
            //// Setup
            string input = "Tom &amp; Jerry and friends";

            //// Act
            string actual = input.TruncateSemantic(6);

            //// Assert
            Assert.That(actual, Does.Contain("&amp;"));
            Assert.That(actual, Does.EndWith("..."));
        }

        [Test]
        public void TruncateSemantic_WithHtml_ClosesOpenTagsAfterEllipsis()
        {
            //// Setup
            string input = "<b>Hello <i>world and universe</i></b>";

            //// Act
            string actual = input.TruncateSemantic(8, countHtmlTags: false);

            //// Assert
            Assert.That(actual, Does.Contain("..."));
            Assert.That(actual, Does.EndWith("</i></b>").Or.EndWith("</b>"));
        }

        [Test]
        public void TruncateSemantic_CountHtmlTags_UsesTagCharactersInLimit()
        {
            //// Setup
            string input = "<b>Hello</b>";

            //// Act
            string actual = input.TruncateSemantic(5, countHtmlTags: true);

            //// Assert
            Assert.That(actual, Does.Contain("..."));
        }

        [Test]
        public void TruncateSemantic_MalformedHtml_DoesNotThrow()
        {
            //// Setup
            string input = "<div><span>Hello world";

            //// Act / Assert
            Assert.DoesNotThrow(() => input.TruncateSemantic(8));
        }

        [Test]
        public void TruncateSemantic_BoundaryInputs_HandleSafely()
        {
            //// Setup
            string input = "hello world";

            //// Act
            string zero = input.TruncateSemantic(0);
            string noTruncate = input.TruncateSemantic(100);

            //// Assert
            Assert.That(zero, Is.EqualTo(string.Empty));
            Assert.That(noTruncate, Is.EqualTo(input));
        }

        #endregion TruncateSemantic
    }
}
