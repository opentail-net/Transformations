using System.Text;

namespace Transformations.Text;

public class LogExtractor : ITextExtractor
{
    public bool CanHandle(string extension) =>
        extension.Equals(".log", StringComparison.OrdinalIgnoreCase) ||
        extension.Equals(".out", StringComparison.OrdinalIgnoreCase);

    public string ExtractText(byte[] fileData)
    {
        using var stream = new MemoryStream(fileData);
        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
        return TextNormalizer.Normalize(reader.ReadToEnd());
    }
}
