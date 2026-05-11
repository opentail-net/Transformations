using System.Text;

namespace Transformations.Text;

/// <summary>
/// Extracts and cleans diagnostic log data. 
/// Unlike structured documents, logs are treated as a sequence of "Knowledge Events,"
/// where preserving the chronological line order is essential for troubleshooting.
/// </summary>
internal class LogExtractor : ITextExtractor
{
    /// <summary>
    /// Validates if the extractor can handle standard .log and generic .out diagnostic files.
    /// </summary>
    public bool CanHandle(string extension) =>
        extension.Equals(".log", StringComparison.OrdinalIgnoreCase) ||
        extension.Equals(".out", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Transforms raw log bytes into normalized, high-density event text.
    /// </summary>
    /// <param name="fileData">The raw binary content of the log file.</param>
    /// <returns>A cleaned string of log entries ready for indexing.</returns>
    public string ExtractText(byte[] fileData)
    {
        // Convert binary to UTF8 string (standard for modern logging systems).
        var text = Encoding.UTF8.GetString(fileData);

        // DENSITY OPTIMIZATION: 
        // We leverage the shared TextNormalizer to handle inconsistent line endings 
        // and collapse excessive vertical whitespace (noise). 
        // We do not apply aggressive flattening because logs are already 
        // inherently line-oriented "Knowledge Events" that the LLM can 
        // parse chronologically.
        return TextNormalizer.Normalize(text);
    }
}