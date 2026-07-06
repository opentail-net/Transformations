using Markdig;
using System.Text;

namespace Transformations.Text;

public class MarkdownExtractor : ITextExtractor
{
    public bool CanHandle(string extension) =>
        extension.Equals(".md", StringComparison.OrdinalIgnoreCase) ||
        extension.Equals(".markdown", StringComparison.OrdinalIgnoreCase);

    public string ExtractText(byte[] fileData)
    {
        using var stream = new MemoryStream(fileData);
        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
        string markdown = reader.ReadToEnd();
        return Markdown.ToPlainText(markdown) ?? string.Empty;
    }
}
