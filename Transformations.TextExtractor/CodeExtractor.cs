using System.Text;
using System.Text.RegularExpressions;

namespace Transformations.Text;

/// <summary>
/// Extracts text from source code files, stripping empty lines and copyright headers.
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

        var cleanedCode = Regex.Replace(rawCode, @"/\*[\s\S]*?Copyright[\s\S]*?\*/", "", RegexOptions.IgnoreCase);

        var lines = cleanedCode.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        var sb = new StringBuilder();

        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            sb.AppendLine(line.TrimEnd());
        }

        return sb.ToString().Trim();
    }
}
