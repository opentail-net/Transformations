using System.Text;
using System.Text.RegularExpressions;

namespace Transformations.Text;


public class TxtExtractor : ITextExtractor
{
    public bool CanHandle(string extension) =>
        extension.Equals(".txt", StringComparison.OrdinalIgnoreCase);

    public string ExtractText(byte[] fileData)
    {
        using var stream = new MemoryStream(fileData);
        return ExtractFromStream(stream);
    }

    public string ExtractFromStream(Stream stream)
    {
        try
        {
            // Use StreamReader with leaveOpen: true to allow the caller to manage the stream
            // It automatically detects UTF-8, UTF-16, etc., via BOM.
            using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: 1024, leaveOpen: true);

            string content = reader.ReadToEnd();

            if (string.IsNullOrEmpty(content)) return string.Empty;

            // 1. Normalize line endings to Environment.NewLine
            // 2. Trim only leading and trailing whitespace
            string normalized = NormalizeLineEndings(content).Trim();

            return normalized;
        }
        catch (Exception ex)
        {
            return $"Error reading text file: {ex.Message}";
        }
    }

    private static string NormalizeLineEndings(string text)
    {
        // Replaces all variations of newlines (\r\n, \r, \n) with the current OS standard
        return Regex.Replace(text, @"\r\n|\n|\r", Environment.NewLine);
    }
}