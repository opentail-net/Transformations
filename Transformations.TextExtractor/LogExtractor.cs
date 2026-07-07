using System.Text;

namespace Transformations.Text;

/// <summary>
/// Extracts text from log files (.log, .out) using UTF-8 or BOM-detected encoding.
/// </summary>
public class LogExtractor : ITextExtractor
{
    /// <inheritdoc />
    public bool CanHandle(string extension) =>
        extension.Equals(".log", StringComparison.OrdinalIgnoreCase) ||
        extension.Equals(".out", StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc />
    public string ExtractText(byte[] fileData)
    {
        using var stream = new MemoryStream(fileData);
        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
        return TextNormalizer.Normalize(reader.ReadToEnd());
    }
}
