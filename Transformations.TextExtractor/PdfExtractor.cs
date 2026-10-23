using System.Text;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Core;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;
using UglyToad.PdfPig.DocumentLayoutAnalysis.WordExtractor;

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
        var hidden = new List<(string Source, string Text, string How)>();

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

                var pageText = PageText(page, pageNum, options, hidden);
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
        catch (Exception ex) when (ex is not (OperationCanceledException or ExtractionLimitExceededException))
        {
            throw new InvalidOperationException($"PDF extraction failed: {ex.Message}", ex);
        }

        if (hidden.Count > 0)
            SafetyGate.ReportHidden(options, SafetyFindingCodes.HiddenPdf,
                string.Join(", ", hidden.Select(h => h.Source).Distinct()),
                string.Join(Environment.NewLine, hidden.Select(h => h.Text)),
                string.Join(", ", hidden.Select(h => h.How).Distinct()));
        return sb.ToString().Trim();
    }

    /// <summary>
    /// The page's text, with letters that are not drawn handled by <see cref="ExtractionOptions.HiddenText"/> and
    /// collected in <paramref name="hidden"/>. Not drawn: render mode 3 (invisible) on a page with no image, a point
    /// size below 1, or a glyph entirely outside the crop box. Render mode 3 over an image is the text layer of a
    /// searchable scan, which is normal, so it is not treated as hidden.
    /// </summary>
    private static string PageText(Page page, int pageNum, ExtractionOptions? options,
        List<(string Source, string Text, string How)> hidden)
    {
        var bounds = page.CropBox.Bounds;
        bool hasImages = page.NumberOfImages > 0;
        var invisible = new List<Letter>();
        var hows = new HashSet<string>();
        foreach (var letter in page.Letters)
        {
            if (string.IsNullOrWhiteSpace(letter.Value)) continue;
            string? how = null;
            if (!hasImages && letter.RenderingMode is TextRenderingMode.Neither or TextRenderingMode.NeitherClip)
                how = "render mode 3 (invisible)";
            else if (letter.PointSize is > 0 and < 1)
                how = "font size below 1 pt";
            else if (letter.GlyphRectangle is var r && (r.Right < bounds.Left || r.Left > bounds.Right
                                                        || r.Top < bounds.Bottom || r.Bottom > bounds.Top))
                how = "outside the page";
            if (how == null) continue;
            invisible.Add(letter);
            hows.Add(how);
        }

        if (invisible.Count == 0)
            return ContentOrderTextExtractor.GetText(page);

        var hiddenText = WordsToText(NearestNeighbourWordExtractor.Instance.GetWords(invisible));
        hidden.Add(($"page {pageNum}", hiddenText, string.Join(", ", hows)));

        var mode = options?.HiddenText ?? HiddenTextMode.Mark;
        if (mode == HiddenTextMode.Keep)
            return ContentOrderTextExtractor.GetText(page);

        var hiddenSet = new HashSet<Letter>(invisible);
        var visibleLetters = page.Letters.Where(l => !hiddenSet.Contains(l)).ToList();
        var visible = WordsToText(NearestNeighbourWordExtractor.Instance.GetWords(visibleLetters));
        return SafetyGate.RenderHidden(options, hiddenText) is { } marked
            ? (string.IsNullOrWhiteSpace(visible) ? marked : visible + Environment.NewLine + marked)
            : visible;
    }

    /// <summary>Joins words into lines, top to bottom and left to right, starting a line when the baseline moves.</summary>
    private static string WordsToText(IEnumerable<Word> words)
    {
        var sb = new StringBuilder();
        double? lineBottom = null;
        foreach (var word in words.OrderByDescending(w => Math.Round(w.BoundingBox.Bottom)).ThenBy(w => w.BoundingBox.Left))
        {
            if (lineBottom is double y && Math.Abs(y - word.BoundingBox.Bottom) > Math.Max(1, word.BoundingBox.Height / 2))
                sb.AppendLine();
            else if (lineBottom != null)
                sb.Append(' ');
            sb.Append(word.Text);
            lineBottom = word.BoundingBox.Bottom;
        }
        return sb.ToString();
    }

    private static void AppendOcrPage(Page page, int pageNum, StringBuilder sb, ExtractionOptions options)
    {
        var texts = new List<string>();
        long maxPixels = options.MaxImagePixels;
        foreach (var image in page.GetImages())
        {
            // Check the size the PDF declares before anything decodes the image: PNG conversion decodes it.
            if ((long)image.WidthInSamples * image.HeightInSamples > maxPixels)
                throw new ExtractionLimitExceededException(
                    $"Image on page {pageNum} is {image.WidthInSamples}x{image.HeightInSamples} pixels; the limit is {maxPixels} (ExtractionOptions.MaxImagePixels).");

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
