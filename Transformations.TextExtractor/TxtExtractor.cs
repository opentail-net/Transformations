using System.Text;

namespace Transformations.Text;

public class TxtExtractor : ITextExtractor
{
    /// <summary>
    /// Accepts any extension — acts as the final fallback for unknown text-like files.
    /// </summary>
    public bool CanHandle(string extension) => true;

    public string ExtractText(byte[] fileData)
    {
        if (IsBinary(fileData))
            return string.Empty;

        using var stream = new MemoryStream(fileData);
        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
        return TextNormalizer.Normalize(reader.ReadToEnd());
    }

    private static bool IsBinary(byte[] data)
    {
        int limit = Math.Min(data.Length, 8192);
        for (int i = 0; i < limit; i++)
        {
            if (data[i] == 0) return true;
        }
        return false;
    }
}
