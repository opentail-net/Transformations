using NUnit.Framework;

namespace Transformations.Tests
{
    [TestFixture]
    public class UntrustedInputRegressionTests
    {
        [TestCase("yes", true)]
        [TestCase("Y", true)]
        [TestCase("1", true)]
        [TestCase("TRUE", true)]
        [TestCase(" t ", true)]
        [TestCase("no", false)]
        [TestCase("0", false)]
        [TestCase("false", false)]
        public void ConvertTo_Bool_RecognisesWords_OnEveryStringPath(string input, bool expected)
        {
            Assert.That(input.ConvertTo<bool>(), Is.EqualTo(expected));
            Assert.That(input.ConvertTo<bool>(!expected), Is.EqualTo(expected));
            Assert.That(input.TryConvertTo<bool>(out bool tried), Is.True);
            Assert.That(tried, Is.EqualTo(expected));
            Assert.That(input.TryConvertTo(!expected, out bool triedWithDefault), Is.True);
            Assert.That(triedWithDefault, Is.EqualTo(expected));
        }

        [Test]
        public void ConvertTo_Bool_UnrecognisedText_FallsBackToDefault_NotTrue()
        {
            Assert.That("maybe".ConvertTo<bool>(), Is.False);
            Assert.That("maybe".ConvertTo<bool>(true), Is.True);
            Assert.That("maybe".ConvertTo<bool>(false), Is.False);
            Assert.That("maybe".TryConvertTo<bool>(out bool r), Is.False);
            Assert.That(r, Is.False);
            Assert.That("maybe".TryConvertTo(true, out bool withDefault), Is.False);
            Assert.That(withDefault, Is.True);
        }

        [TestCase("<p>Fish &amp; <b>chips</b> are <em>great</em> with vinegar</p>", 20, false, "<p>Fish &amp; <b>chips</b> are...</p>")]
        [TestCase("<p>Fish &amp; <b>chips</b> are <em>great</em> with vinegar</p>", 20, true, "<p>Fish &amp;...</p>")]
        [TestCase("<div><b>bold text here</b> and more words</div>", 9, false, "<div><b>bold text...</b></div>")]
        public void TruncateSemantic_ClosesOnlyTagsThatSurvivedTheCut(string html, int max, bool countTags, string expected)
        {
            Assert.That(html.TruncateSemantic(max, countTags), Is.EqualTo(expected));
        }

        [Test]
        public void TruncateSemantic_NeverLeavesDanglingTagFragment()
        {
            var result = "<p>some <a href=\"https://example.com\">linked</a> text</p>".TruncateSemantic(14, true);
            Assert.That(result, Does.Not.Contain("<a").Or.Contain(">"));
            Assert.That(result.LastIndexOf('<'), Is.LessThan(result.Length));
            Assert.That(result.Split('<').Length - 1, Is.EqualTo(result.Split('>').Length - 1));
        }
    }
}
