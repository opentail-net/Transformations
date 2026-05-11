using System.Text;
using System.Text.RegularExpressions;

namespace Transformations.Text;

/// <summary>
/// The universal fallback extractor for plain text files. 
/// It implements a "Binary Shield" to protect the system from indexing non-textual data 
/// and uses automatic encoding detection to ensure broad compatibility.
/// </summary>
internal class TxtExtractor : ITextExtractor
{
    /// <summary>
    /// The TxtExtractor is the "Final Fallback." 
    /// It returns true for any extension to ensure that unknown text-like files 
    /// (e.g., .conf, .ini, .bat) are still processed.
    /// </summary>
    public bool CanHandle(string extension) => true;

    /// <summary>
    /// Extracts text from raw bytes while validating that the content is not binary.
    /// </summary>
    /// <param name="fileData">The raw binary content of the file.</param>
    /// <returns>Normalized text content or a skip message if binary is detected.</returns>
    public string ExtractText(byte[] fileData)
    {
        // 1. THE SHIELD: Prevent binary garbage from entering the Knowledge Graph.
        // Indexing binary blobs (like .exe or .png) creates "poison" tokens 
        // that degrade vector search quality.
        if (IsBinary(fileData))
        {
            return "[Skipped: Binary Content Detected]";
        }

        try
        {
            using var stream = new MemoryStream(fileData);

            // LOW-MAGIC ENCODING: detectEncodingFromByteOrderMarks is set to true.
            // This allows the reader to handle UTF-8, UTF-16, and other BOM-indexed 
            // formats automatically without manual configuration.
            using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);

            string content = reader.ReadToEnd();

            // 2. High-Density Normalization:
            // Route the raw text through the shared TextNormalizer to strip 
            // horizontal/vertical bloat and standardize line endings.
            return TextNormalizer.Normalize(content);
        }
        catch (Exception ex)
        {
            // DIAGNOSTICS: Capture and report read errors for the orchestrator.
            return $"Text Extraction Error: {ex.Message}";
        }
    }

    /// <summary>
    /// Heuristic check to identify binary files. 
    /// Real text files (UTF8/ASCII) almost never contain a null byte (0x00).
    /// </summary>
    /// <param name="data">The byte array to inspect.</param>
    /// <returns>True if the file appears to be binary; otherwise, false.</returns>
    private bool IsBinary(byte[] data)
    {
        // We only check the first 8KB (Standard Sector/Buffer size).
        // If a null byte appears in the header, it's statistically guaranteed 
        // to be a binary format (executable, image, etc.) rather than prose.
        int checkLimit = Math.Min(data.Length, 8192);
        for (int i = 0; i < checkLimit; i++)
        {
            if (data[i] == 0) return true;
        }
        return false;
    }
}