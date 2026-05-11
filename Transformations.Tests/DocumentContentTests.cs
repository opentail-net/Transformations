using NUnit.Framework;
using Transformations.Text;
using DocFormat = Transformations.Text.DocumentFormat;

namespace Transformations.Tests;

[TestFixture]
public class DocumentContentTests
{
    [Test]
    public void Normalize_UsesMarkdownRules_WhenFormatIsMarkdown()
    {
        string markdown = "# Title\n\n\n\nBody";

        string normalized = DocumentContent.Normalize(markdown, DocFormat.Markdown);

        Assert.That(normalized, Is.EqualTo($"# Title{Environment.NewLine}{Environment.NewLine}Body"));
    }

    [Test]
    public void Normalize_UsesJsonRules_WhenFormatIsJson()
    {
        string json = "```json\n{\"a\":1}\n```";

        string normalized = DocumentContent.Normalize(json, DocFormat.Json);

        Assert.That(normalized, Is.EqualTo("{\"a\":1}"));
    }

    [Test]
    public void Normalize_UsesTextRules_WhenFormatIsPlainText()
    {
        string text = "Line 1\n\n\n\nLine 2";

        string normalized = DocumentContent.Normalize(text, DocFormat.PlainText);

        Assert.That(normalized, Is.EqualTo($"Line 1{Environment.NewLine}{Environment.NewLine}Line 2"));
    }

    [Test]
    public void Compare_UsesMarkdownRules_WhenFormatIsMarkdown()
    {
        string left = "# Title\n\nBody   ";
        string right = "# Title\r\n\r\nBody";

        bool changed = DocumentContent.Compare(left, right, DocFormat.Markdown);

        Assert.That(changed, Is.False);
    }

    [Test]
    public void Compare_UsesJsonRules_WhenFormatIsJson()
    {
        string left = "{\"a\":1}";
        string right = "{\"a\":2}";

        bool changed = DocumentContent.Compare(left, right, DocFormat.Json);

        Assert.That(changed, Is.True);
    }

    [Test]
    public void Compare_UsesTextRules_WhenFormatIsPlainText()
    {
        string left = "A\n\n\n\nB";
        string right = "A\r\n\r\nB";

        bool changed = DocumentContent.Compare(left, right, DocFormat.PlainText);

        Assert.That(changed, Is.False);
    }
}
