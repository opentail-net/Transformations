namespace Transformations.Text;

public enum DocumentFormat
{
    PlainText = 0,
    Markdown = 1,
    Json = 2
}

public static class DocumentContent
{
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
