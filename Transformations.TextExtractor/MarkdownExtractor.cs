using Markdig;
using System.Text;
using System.IO;

namespace Transformations.Text;

/// <summary>
/// Specialized extractor for Markdown files (.md, .markdown).
/// Utilizes a full Abstract Syntax Tree (AST) parser to ensure that formatting 
/// characters are stripped while the semantic hierarchy of the document is preserved.
/// </summary>
internal class MarkdownExtractor : ITextExtractor
{
    /// <summary>
    /// Validates if the extractor can handle standard Markdown extensions.
    /// </summary>
    public bool CanHandle(string extension) =>
        extension.Equals(".md", StringComparison.OrdinalIgnoreCase) ||
        extension.Equals(".markdown", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Transforms Markdown bytes into clean, human-readable plain text.
    /// </summary>
    /// <param name="fileData">The raw binary content of the Markdown file.</param>
    /// <returns>A string of extracted plain text or an error message.</returns>
    public string ExtractText(byte[] fileData)
    {
        try
        {
            // 1. CONTENT CONVERSION: Convert raw bytes to a UTF-8 string.
            // Markdown is inherently text-based and typically encoded in UTF-8.
            string markdown = Encoding.UTF8.GetString(fileData);

            // 2. AST RENDERING: Use the Markdig built-in plain-text renderer.
            // This handles the complex AST parsing (Abstract Syntax Tree) and internal 
            // TextRendererBase implementation. It effectively strips Markdown-specific 
            // syntax (like links, bolding, and headers) while keeping the content legible.
            string plainText = Markdown.ToPlainText(markdown);

            // Ensure we return an empty string rather than null if the file is blank.
            return plainText ?? string.Empty;
        }
        catch (Exception ex)
        {
            // DIAGNOSTICS: Provide a minimalist error string to be intercepted 
            // by the TextExtractor orchestrator for professional logging.
            return $"Markdown Extraction Error: {ex.Message}";
        }
    }
}