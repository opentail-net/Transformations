namespace Transformations.Text;

/// <summary>
/// Specifies the underlying format of a document.
/// </summary>
public enum DocumentFormat
{
    /// <summary>Unformatted plain text.</summary>
    PlainText = 0,
    /// <summary>Markdown formatted text.</summary>
    Markdown = 1,
    /// <summary>JSON structured data.</summary>
    Json = 2
}

/// <summary>
/// Provides utility methods for format-aware text normalization and comparison.
/// </summary>
public static class DocumentContent
{
    /// <summary>
    /// Normalizes the provided content based on its specific format.
    /// </summary>
    public static string Normalize(string content, DocumentFormat format) =>
        format switch
        {
            DocumentFormat.Markdown => MarkdownStructureExtractor.NormalizeMarkdown(content),
            DocumentFormat.Json => JsonSchemaValidator.NormalizeJson(content),
            _ => TextNormalizer.Normalize(content)
        };

    /// <summary>
    /// Returns <c>true</c> when the two payloads differ after format-aware normalization.
    /// </summary>
    public static bool HasChanged(string left, string right, DocumentFormat format) =>
        format switch
        {
            DocumentFormat.Markdown => MarkdownStructureExtractor.HasChanged(left, right),
            DocumentFormat.Json => JsonSchemaValidator.HasChanged(left, right),
            _ => !string.Equals(TextNormalizer.Normalize(left), TextNormalizer.Normalize(right), StringComparison.Ordinal)
        };

    /// <inheritdoc cref="HasChanged"/>
    [Obsolete("Use HasChanged instead — Compare returns true when content differs, which is unintuitive.")]
    public static bool Compare(string left, string right, DocumentFormat format) =>
        HasChanged(left, right, format);
}
