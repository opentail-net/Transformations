using System.Text;
using System.Text.RegularExpressions;

namespace Transformations.Text;

/// <summary>
/// Specialized extractor for source code files. 
/// Focuses on preserving structural indentation (logical scope) while stripping 
/// non-semantic boilerplate to optimize for LLM context windows.
/// </summary>
internal class CodeExtractor : ITextExtractor
{
    /// <summary>
    /// Supported source code extensions spanning modern systems, scripting, and web.
    /// </summary>
    private static readonly HashSet<string> CodeExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".cs", ".py", ".js", ".ts", ".cpp", ".h", ".java", ".ps1", ".sh", ".go", ".rs", ".sql"
    };

    /// <summary>
    /// Validates if the file extension belongs to the known source code set.
    /// </summary>
    public bool CanHandle(string extension) => CodeExtensions.Contains(extension);

    /// <summary>
    /// Extracts and cleans code text from raw bytes.
    /// </summary>
    /// <param name="fileData">The raw binary content of the source file.</param>
    /// <returns>A cleaned string suitable for indexing or RAG ingestion.</returns>
    public string ExtractText(byte[] fileData)
    {
        // Convert binary to UTF8 string (standard for source code)
        var rawCode = Encoding.UTF8.GetString(fileData);

        // 1. BOILERPLATE REMOVAL: Remove large common license/copyright blocks.
        // These blocks repeat across many files and "poison" semantic search with legal noise.
        // We target /* ... Copyright ... */ style comments specifically.
        var cleanedCode = Regex.Replace(rawCode, @"/\*[\s\S]*?Copyright[\s\S]*?\*/", "", RegexOptions.IgnoreCase);

        // 2. LINE NORMALIZATION: Split by any newline variant to process line-by-line.
        var lines = cleanedCode.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        var sb = new StringBuilder();

        foreach (var line in lines)
        {
            // 3. DENSITY OPTIMIZATION: Skip purely empty lines to save tokens in the vector store.
            if (string.IsNullOrWhiteSpace(line)) continue;

            // 4. SCOPE PRESERVATION: 
            // We use TrimEnd() but NOT a full Trim(). 
            // This preserves leading whitespace (tabs/spaces), which is essential for 
            // the LLM to understand nesting, code blocks, and logical scope.
            sb.AppendLine(line.TrimEnd());
        }

        // Return the final high-density, scope-aware code block.
        return sb.ToString().Trim();
    }
}