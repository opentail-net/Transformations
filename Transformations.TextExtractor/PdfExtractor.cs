using System.Text;
using UglyToad.PdfPig;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;

namespace Transformations.Text;

/// <summary>
/// Extracts text from Portable Document Format (.pdf) files using PdfPig.
/// </summary>
public class PdfExtractor : ITextExtractor
{
    /// <inheritdoc />
    public bool CanHandle(string extension) =>
        extension.Equals(".pdf", StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc />
    public string ExtractText(byte[] fileData) => ExtractText(fileData, null);

    /// <inheritdoc />
    public string ExtractText(byte[] fileData, ExtractionOptions? options)
    {
        using var stream = new MemoryStream(fileData);
        return ExtractFromStream(stream, options);
    }

    /// <summary>
    /// Extracts text directly from a stream, optionally tracking page numbers.
    /// </summary>
    public string ExtractFromStream(Stream stream, ExtractionOptions? options = null)
    {
        var sb = new StringBuilder();
        int start = options?.StartPage ?? 1;
        int end = options?.EndPage ?? int.MaxValue;
        int max = options?.MaxPages.HasValue == true ? (start - 1) + options.MaxPages.Value : int.MaxValue;
        end = Math.Min(end, max);
        bool pageMarkers = options?.IncludePageMarkers == true;

        try
        {
            using var document = PdfDocument.Open(stream);

            foreach (var page in document.GetPages())
            {
                int pageNum = page.Number;
                if (pageNum < start) continue;
                if (pageNum > end) break;

                if (pageMarkers)
                    sb.AppendLine($"[Page {pageNum}]");

                var pageText = ContentOrderTextExtractor.GetText(page);
                if (!string.IsNullOrWhiteSpace(pageText))
                {
                    sb.AppendLine(pageText);
                    sb.AppendLine();
                }
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"PDF extraction failed: {ex.Message}", ex);
        }

        return sb.ToString().Trim();
    }
}
