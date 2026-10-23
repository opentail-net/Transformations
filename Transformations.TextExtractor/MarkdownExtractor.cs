using Markdig;
using System.Text;

namespace Transformations.Text;

/// <summary>
/// Extracts plain text from Markdown files by parsing and removing formatting markup.
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
        var text = Markdown.ToPlainText(markdown) ?? string.Empty;
        ExtractionGuard.ThrowIfCancellationRequested(options);
        return text;
    }
}
