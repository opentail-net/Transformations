using System.Text;

namespace Transformations.Text;

/// <summary>
/// Extracts text from source code files, stripping empty lines and copyright headers.
/// Only <c>/* ... */</c> comments in the file's leading header (before the first line of code) are candidates, so
/// the same characters inside a string literal or later in the file are never touched.
/// </summary>
public class CodeExtractor : ITextExtractor
{
    private static readonly HashSet<string> CodeExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".cs", ".py", ".js", ".ts", ".cpp", ".h", ".java", ".ps1", ".sh", ".go", ".rs", ".sql",
        ".rb", ".php", ".swift", ".kt", ".scala", ".fs", ".vb", ".lua", ".r", ".m", ".dart"
    };


    /// <inheritdoc />
    public bool CanHandle(string extension) => CodeExtensions.Contains(extension);

    /// <inheritdoc />
    public string ExtractText(byte[] fileData)
    {
        using var stream = new MemoryStream(fileData);
        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
        var rawCode = reader.ReadToEnd();

        var cleanedCode = StripLeadingCopyrightComments(rawCode);

        var lines = cleanedCode.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        var sb = new StringBuilder();

        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            sb.AppendLine(line.TrimEnd());
        }

        // Keep the first line's indentation; indentation is meaningful in Python, YAML-like DSLs and others.
        return sb.ToString().TrimEnd();
    }

    /// <summary>
    /// Removes <c>/* ... */</c> comments that mention "Copyright" from the leading header: the run of whitespace,
    /// a <c>#!</c> line, <c>//</c> line comments and block comments before the first code. Everything from the first
    /// code character on is returned unchanged.
    /// </summary>
    private static string StripLeadingCopyrightComments(string code)
    {
        var kept = new StringBuilder();
        int i = 0;
        while (i < code.Length)
        {
            int start = i;
            while (i < code.Length && char.IsWhiteSpace(code[i])) i++;
            kept.Append(code, start, i - start);
            if (i >= code.Length) break;

            if (code.AsSpan(i).StartsWith("/*"))
            {
                int close = code.IndexOf("*/", i + 2, StringComparison.Ordinal);
                if (close < 0) break; // unterminated: leave it as it is
                int end = close + 2;
                if (code.IndexOf("Copyright", i, end - i, StringComparison.OrdinalIgnoreCase) < 0)
                    kept.Append(code, i, end - i);
                i = end;
            }
            else if (code.AsSpan(i).StartsWith("//") || (i == 0 && code.AsSpan(i).StartsWith("#!")))
            {
                int lineEnd = code.IndexOf('\n', i);
                int end = lineEnd < 0 ? code.Length : lineEnd;
                kept.Append(code, i, end - i);
                i = end;
            }
            else
                break;
        }
        return kept.Append(code, i, code.Length - i).ToString();
    }
}
