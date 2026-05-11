using System.Text.RegularExpressions;

namespace Transformations.Text;

/// <summary>
/// Provides static utility methods to clean and standardize extracted text.
/// The goal is "High-Density" output: removing formatting noise while 
/// preserving the semantic structure required for LLMs and Vector Search.
/// </summary>
public static class TextNormalizer
{
    /// <summary>
    /// Standardizes whitespace, collapses bloat, and normalizes line endings.
    /// </summary>
    /// <param name="input">The raw text string extracted from a document.</param>
    /// <returns>A cleaned, token-efficient string.</returns>
    public static string Normalize(string input)
    {
        // Guard against null or empty input to prevent Regex exceptions.
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        // 1. LINE ENDING NORMALIZATION: 
        // Convert all variants (\r\n, \n, \r) to the current environment's standard.
        // This ensures consistent behavior across Windows, Linux, and Mac targets.
        string result = Regex.Replace(input, @"\r\n|\n|\r", Environment.NewLine);

        // 2. HORIZONTAL BLOAT REDUCTION:
        // Replaces 3 or more consecutive spaces or tabs with a single space.
        // This preserves basic indentation but removes "Alignment Bloat" common in 
        // fixed-width reports or poorly parsed PDFs, significantly saving LLM tokens.
        result = Regex.Replace(result, @"[ \t]{3,}", " ");

        // 3. TRAILING WHITESPACE PRUNING:
        // Iterates through every individual line to remove invisible trailing characters.
        // This prevents "dirty" lines from bloating the vector store with useless space tokens.
        var lines = result.Split(new[] { Environment.NewLine }, StringSplitOptions.None)
                          .Select(line => line.TrimEnd());

        result = string.Join(Environment.NewLine, lines);

        // 4. VERTICAL NOISE REDUCTION:
        // Collapses 3 or more consecutive blank lines into exactly two (one empty line).
        // This preserves paragraph and section intent while removing excessive vertical 
        // gaps often left behind by removed images, tables, or page breaks.
        string pattern = $"({Regex.Escape(Environment.NewLine)}){{3,}}";
        result = Regex.Replace(result, pattern, Environment.NewLine + Environment.NewLine);

        // Final trim to ensure no leading/trailing vertical noise remains.
        return result.Trim();
    }
}