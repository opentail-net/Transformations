using System.Text;
using UglyToad.PdfPig;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;

namespace Transformations.Text;

public class PdfPigExtractor : ITextExtractor
{
    public bool CanHandle(string extension) =>
        extension.Equals(".pdf", StringComparison.OrdinalIgnoreCase);

    public string ExtractText(byte[] fileData)
    {
        using var stream = new MemoryStream(fileData);
        return ExtractFromStream(stream);
    }

    public string ExtractFromStream(Stream stream)
    {
        var sb = new StringBuilder();

        try
        {
            using var document = PdfDocument.Open(stream);

            foreach (var page in document.GetPages())
            {
                // ContentOrderTextExtractor is layout-aware.
                // It attempts to group letters into words and lines based on 
                // physical proximity rather than the order they appear in the file's raw bytes.
                var pageText = ContentOrderTextExtractor.GetText(page);

                if (!string.IsNullOrWhiteSpace(pageText))
                {
                    sb.AppendLine(pageText);
                    // Ensure paragraph separation between pages
                    sb.AppendLine();
                }
            }
        }
        catch (Exception ex)
        {
            // Minimalist error handling as requested
            return $"Error extracting PDF text: {ex.Message}";
        }

        return sb.ToString().Trim();
    }
}