using NUnit.Framework;
using Transformations.Text;

namespace Transformations.Tests;

[TestFixture]
public class MarkdownStructureExtractorTests
{
    [Test]
    public void CompareMarkdown_ReturnsFalse_ForEquivalentMarkdownAfterNormalization()
    {
        string left = "# Title\n\nBody   ";
        string right = "# Title\r\n\r\nBody";

        bool changed = MarkdownStructureExtractor.CompareMarkdown(left, right);

        Assert.That(changed, Is.False);
    }

    [Test]
    public void CompareMarkdown_ReturnsTrue_WhenMarkdownContentDiffers()
    {
        string left = "# Title\n\nBody";
        string right = "# Title\n\nBody updated";

        bool changed = MarkdownStructureExtractor.CompareMarkdown(left, right);

        Assert.That(changed, Is.True);
    }

    [Test]
    public void NormalizeMarkdown_CollapsesExcessiveBlankLines()
    {
        string markdown = "# Title\n\n\n\nBody";

        string result = MarkdownStructureExtractor.NormalizeMarkdown(markdown);

        Assert.That(result, Is.EqualTo($"# Title{Environment.NewLine}{Environment.NewLine}Body"));
    }

    [Test]
    public void BuildHeadingMap_ExtractsAtxAndSetextHeadings()
    {
        string markdown = "# Top\n\nIntro\n\nSub heading\n---\n\n### Deep";

        var map = MarkdownStructureExtractor.BuildHeadingMap(markdown);

        Assert.That(map, Has.Count.EqualTo(3));
        Assert.That(map[0].Level, Is.EqualTo(1));
        Assert.That(map[0].Title, Is.EqualTo("Top"));
        Assert.That(map[1].Level, Is.EqualTo(2));
        Assert.That(map[1].Title, Is.EqualTo("Sub heading"));
        Assert.That(map[2].Level, Is.EqualTo(3));
        Assert.That(map[2].Title, Is.EqualTo("Deep"));
    }

    [Test]
    public void ExtractSections_SplitsContentByHeadingBoundaries()
    {
        string markdown = "# A\nLine A\n\n## B\nLine B\n\n### C\nLine C";

        var sections = MarkdownStructureExtractor.ExtractSections(markdown);

        Assert.That(sections, Has.Count.EqualTo(3));
        Assert.That(sections[0].Title, Is.EqualTo("A"));
        Assert.That(sections[0].Content, Does.Contain("Line A"));
        Assert.That(sections[1].Title, Is.EqualTo("B"));
        Assert.That(sections[1].Content, Does.Contain("Line B"));
        Assert.That(sections[2].Title, Is.EqualTo("C"));
        Assert.That(sections[2].Content, Does.Contain("Line C"));
    }

    [Test]
    public void ExtractSections_AddsIntroduction_WhenPrefaceExists()
    {
        string markdown = "Preface line\n\n# Start\nBody";

        var sections = MarkdownStructureExtractor.ExtractSections(markdown);

        Assert.That(sections, Has.Count.EqualTo(2));
        Assert.That(sections[0].Level, Is.EqualTo(0));
        Assert.That(sections[0].Title, Is.EqualTo("Introduction"));
        Assert.That(sections[0].Content, Does.Contain("Preface line"));
        Assert.That(sections[1].Title, Is.EqualTo("Start"));
    }

    [Test]
    public void ExtractSections_ReturnsDocumentSection_WhenNoHeadings()
    {
        string markdown = "Plain paragraph one.\n\nPlain paragraph two.";

        var sections = MarkdownStructureExtractor.ExtractSections(markdown);

        Assert.That(sections, Has.Count.EqualTo(1));
        Assert.That(sections[0].Level, Is.EqualTo(0));
        Assert.That(sections[0].Title, Is.EqualTo("Document"));
        Assert.That(sections[0].Content, Does.Contain("Plain paragraph one."));
    }
}
