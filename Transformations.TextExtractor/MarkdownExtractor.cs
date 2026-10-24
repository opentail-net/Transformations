using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using System.Text;

namespace Transformations.Text;

/// <summary>
/// Extracts plain text from Markdown files by parsing and removing formatting markup.
/// Markdown may embed raw HTML, which a renderer shows as HTML: a document that contains any is rendered to HTML and
/// read by the HTML extractor, so hidden elements (<c>display:none</c>, <c>hidden</c>, ...) are hidden content and
/// comments are dropped, exactly as in an HTML file. Plain Markdown is converted directly.
/// </summary>
public class MarkdownExtractor : ITextExtractor
{
    /// <inheritdoc />
    public bool CanHandle(string extension) =>
        extension.Equals(".md", StringComparison.OrdinalIgnoreCase) ||
        extension.Equals(".markdown", StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc />
    public string ExtractText(byte[] fileData) => ExtractText(fileData, null);

    /// <inheritdoc />
    /// <remarks>Markdig converts in one call that cannot be interrupted; cancellation is checked before and after it.</remarks>
    public string ExtractText(byte[] fileData, ExtractionOptions? options)
    {
        ExtractionGuard.ThrowIfCancellationRequested(options);
        using var stream = new MemoryStream(fileData);
        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
        string markdown = reader.ReadToEnd();

        var document = Markdown.Parse(markdown);
        bool hasHtml = document.Descendants<HtmlBlock>().Any() || document.Descendants<HtmlInline>().Any();
        var text = hasHtml
            ? new HtmlExtractor().ExtractFromHtml(Markdown.ToHtml(markdown), options)
            : Markdown.ToPlainText(markdown) ?? string.Empty;
        ExtractionGuard.ThrowIfCancellationRequested(options);
        return text;
    }
}
