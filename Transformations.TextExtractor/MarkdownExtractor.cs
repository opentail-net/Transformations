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
    public string ExtractText(byte[] fileData)
    {
        using var stream = new MemoryStream(fileData);
        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
        string markdown = reader.ReadToEnd();
        return Markdown.ToPlainText(markdown) ?? string.Empty;
    }
}
