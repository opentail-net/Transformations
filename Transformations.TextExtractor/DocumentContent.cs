namespace Transformations.Text;

/// <summary>
/// Defines supported document formats for common normalization and comparison workflows.
/// </summary>
public enum DocumentFormat
{
    /// <summary>
    /// Plain text content.
    /// </summary>
    PlainText = 0,

    /// <summary>
    /// Markdown content.
    /// </summary>
    Markdown = 1,

    /// <summary>
    /// JSON content.
    /// </summary>
    Json = 2
}

/// <summary>
/// Provides a common façade for document normalization and comparison across supported formats.
/// </summary>
public static class DocumentContent
{
    /// <summary>
    /// Normalizes content according to the supplied format.
    /// </summary>
    /// <param name="content">The source content.</param>
    /// <param name="format">The document format.</param>
    /// <returns>Normalized content representation.</returns>
    public static string Normalize(string content, DocumentFormat format)
    {
        return format switch
        {
            DocumentFormat.Markdown => MarkdownStructureExtractor.NormalizeMarkdown(content),
            DocumentFormat.Json => JsonSchemaValidator.NormalizeJson(content),
            _ => TextNormalizer.Normalize(content)
        };
    }

    /// <summary>
    /// Compares two content payloads after format-aware normalization.
    /// </summary>
    /// <param name="left">Original content.</param>
    /// <param name="right">Updated content.</param>
    /// <param name="format">The document format.</param>
    /// <returns><c>true</c> when payloads differ; otherwise <c>false</c>.</returns>
    public static bool Compare(string left, string right, DocumentFormat format)
    {
        return format switch
        {
            DocumentFormat.Markdown => MarkdownStructureExtractor.CompareMarkdown(left, right),
            DocumentFormat.Json => JsonSchemaValidator.CompareJson(left, right),
            _ => !string.Equals(TextNormalizer.Normalize(left), TextNormalizer.Normalize(right), StringComparison.Ordinal)
        };
    }
}
