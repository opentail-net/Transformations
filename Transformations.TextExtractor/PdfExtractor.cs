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
                ExtractionGuard.ThrowIfCancellationRequested(options);
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
                else if (page.GetImages().Any() && ImageExtractor.OcrAvailable(options))
                {
                    AppendOcrPage(page, pageNum, sb, options!);
                }
                else if (page.GetImages().Any())
                {
                    // A scanned page: the text is in the image. Say so instead of returning silence.
                    sb.AppendLine($"[Page {pageNum}] not extracted: no text layer (scanned image); text, if any, needs OCR");
                    sb.AppendLine();
                    options?.WarningSink?.Invoke(new ExtractionWarning(ExtractionWarningCodes.NeedsOcr, $"page {pageNum}",
                        "Page has images but no text layer; it is probably scanned."));
                }
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            throw new InvalidOperationException($"PDF extraction failed: {ex.Message}", ex);
        }

        return sb.ToString().Trim();
    }

    private static void AppendOcrPage(UglyToad.PdfPig.Content.Page page, int pageNum, StringBuilder sb, ExtractionOptions options)
    {
        var texts = new List<string>();
        foreach (var image in page.GetImages())
        {
            // PNG conversion covers most scans; DCT (JPEG) streams can be passed through as they are.
            byte[]? encoded = image.TryGetPng(out var png) ? png : image.RawBytes.ToArray();
            if (!ContentTypeDetector.IsImageExtension(ContentTypeDetector.SniffExtension(encoded)))
                continue;
            var text = ImageExtractor.Recognize(encoded, ContentTypeDetector.SniffExtension(encoded)!,
                options.OcrEngine!, options, $"page {pageNum}");
            if (!string.IsNullOrWhiteSpace(text))
                texts.Add(text.Trim());
        }

        if (texts.Count == 0) return;
        sb.AppendLine($"[Page {pageNum}] (OCR)");
        sb.AppendLine(string.Join(Environment.NewLine, texts));
        sb.AppendLine();
    }
}
