using System.Text;

namespace Transformations.Text;

/// <summary>
/// Extracts text from plain-text files using UTF-8 or BOM-detected encoding.
/// Acts as the final fallback for unknown file types.
/// </summary>
public class TxtExtractor : ITextExtractor
{
    /// <summary>
    /// Accepts any extension — acts as the final fallback for unknown text-like files.
    /// </summary>
    /// <inheritdoc />
    public bool CanHandle(string extension) => true;

    /// <inheritdoc />
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
        if (HasKnownTextBom(data))
            return false;

        int limit = Math.Min(data.Length, 8192);
        for (int i = 0; i < limit; i++)
        {
            if (data[i] == 0) return true;
        }
        return false;
    }

    private static bool HasKnownTextBom(byte[] data)
        => HasPrefix(data, Encoding.UTF8.GetPreamble())
            || HasPrefix(data, Encoding.Unicode.GetPreamble())
            || HasPrefix(data, Encoding.BigEndianUnicode.GetPreamble())
            || HasPrefix(data, Encoding.UTF32.GetPreamble());

    private static bool HasPrefix(byte[] data, byte[] prefix)
    {
        if (prefix.Length == 0 || data.Length < prefix.Length)
            return false;

        for (int i = 0; i < prefix.Length; i++)
        {
            if (data[i] != prefix[i])
                return false;
        }

        return true;
    }
}
