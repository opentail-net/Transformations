using System.Text;
using UglyToad.PdfPig;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;

namespace Transformations.Text;

/// <summary>
/// Extracts high-fidelity text from PDF documents using the PdfPig library.
/// This implementation uses layout-aware extraction to ensure that the 
/// resulting text follows the natural reading order intended by the document creator.
/// </summary>
public class PdfExtractor : ITextExtractor
{
    /// <summary>
    /// Validates if the extractor can handle the provided file extension.
    /// </summary>
    public bool CanHandle(string extension) =>
        extension.Equals(".pdf", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Orchestrates the extraction of text from a raw byte array.
    /// </summary>
    /// <param name="fileData">The binary content of the .pdf file.</param>
    /// <returns>The extracted text string or a descriptive error message.</returns>
    public string ExtractText(byte[] fileData)
    {
        using var stream = new MemoryStream(fileData);
        return ExtractFromStream(stream);
    }

    /// <summary>
    /// Performs the layout-aware text extraction from the PDF stream.
    /// </summary>
    /// <param name="stream">The stream containing the PDF document.</param>
    /// <returns>A normalized string containing all text from the document pages.</returns>
    public string ExtractFromStream(Stream stream)
    {
        var sb = new StringBuilder();

        try
        {
            // OPEN: Initialize the PdfDocument. PdfPig is highly efficient 
            // for text-heavy documents and handles complex font encodings internally.
            using var document = PdfDocument.Open(stream);

            foreach (var page in document.GetPages())
            {
                // LAYOUT ANALYSIS: ContentOrderTextExtractor is layout-aware.
                // It groups individual characters into words and lines based on 
                // physical proximity (coordinates) rather than the raw byte order.
                // This is essential for correctly reading multi-column layouts.
                var pageText = ContentOrderTextExtractor.GetText(page);

                if (!string.IsNullOrWhiteSpace(pageText))
                {
                    sb.AppendLine(pageText);

                    // PAGE BOUNDARY: Append an extra newline between pages to 
                    // preserve paragraph separation and assist the Hierarchical 
                    // Knowledge Graph in identifying logical breaks.
                    sb.AppendLine();
                }
            }
        }
        catch (Exception ex)
        {
            // DIAGNOSTICS: Return the exception message to be intercepted 
            // by the TextExtractor façade for professional error reporting.
            return $"Error extracting PDF text: {ex.Message}";
        }

        // Final trim to remove the extra newlines appended after the last page.
        return sb.ToString().Trim();
    }
}