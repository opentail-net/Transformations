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
        var ocrCounter = new EmbeddedImageOcr.Counter();
        int pageCount = 0;

        try
        {
            using var document = PdfDocument.Open(stream);
            pageCount = document.NumberOfPages;

            foreach (var page in document.GetPages())
            {
                ExtractionGuard.ThrowIfCancellationRequested(options);
                int pageNum = page.Number;
                if (pageNum < start) continue;
                if (pageNum > end) break;

                if (pageMarkers)
                    sb.AppendLine(ModelInput.MarkerLine(options, "Page", pageNum));

                var layerOverImages = new List<Letter>();
                var pageText = PageText(page, pageNum, options, hidden, layerOverImages);
                if (layerOverImages.Count > 0)
                    VerifyTextLayer(page, pageNum, layerOverImages, options, ocrCounter, hidden);
                if (!string.IsNullOrWhiteSpace(pageText))
                {
                    sb.AppendLine(pageText);
                    sb.AppendLine();
                    AppendEmbeddedImages(page, pageNum, sb, options, ocrCounter, hidden, layerOverImages);
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

        EmbeddedImageOcr.ReportSkipped(options, ocrCounter, "pdf");
        if (pageCount > 0)
            ExtractionGuard.ReportOmitted(options, "pdf", "page", pageCount, start, end);
        if (hidden.Count > 0)
            SafetyGate.ReportHidden(options, SafetyFindingCodes.HiddenPdf,
                string.Join(", ", hidden.Select(h => h.Source).Distinct()),
                string.Join(Environment.NewLine, hidden.Select(h => h.Text)),
                string.Join(", ", hidden.Select(h => h.How).Distinct()));
        return sb.ToString().Trim();
    }

    /// <summary>
    /// The page's text, with letters that are not drawn handled by <see cref="ExtractionOptions.HiddenText"/> and
    /// collected in <paramref name="hidden"/>. Not drawn: render mode 3 (invisible) that does not sit over an image,
    /// a point size below 1, or a glyph entirely outside the crop box. Invisible text whose glyph centre lies inside
    /// an image's placement rectangle is the text layer of a searchable scan, which is normal, so it is not hidden;
    /// invisible text elsewhere on the same page still is.
    /// </summary>
    private static string PageText(Page page, int pageNum, ExtractionOptions? options,
        List<(string Source, string Text, string How)> hidden, List<Letter> layerOverImages)
    {
        var bounds = page.CropBox.Bounds;
        var imageAreas = page.NumberOfImages > 0 ? page.GetImages().Select(i => i.BoundingBox).ToList() : [];
        var invisible = new List<Letter>();
        var hows = new HashSet<string>();
        foreach (var letter in page.Letters)
        {
            if (string.IsNullOrWhiteSpace(letter.Value)) continue;
            string? how = null;
            if (letter.RenderingMode is TextRenderingMode.Neither or TextRenderingMode.NeitherClip)
            {
                if (OverAnImage(letter.BoundingBox, imageAreas))
                {
                    layerOverImages.Add(letter); // looks like a searchable scan's text layer; verified separately
                    continue;
                }
                how = "render mode 3 (invisible)";
            }
            else if (letter.PointSize is > 0 and < 1)
                how = "font size below 1 pt";
            else if (letter.BoundingBox is var r && (r.Right < bounds.Left || r.Left > bounds.Right
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

    private static bool OverAnImage(PdfRectangle glyph, List<PdfRectangle> imageAreas)
        => imageAreas.Any(a => Contains(a, glyph.Centroid));

    private static bool Contains(PdfRectangle area, PdfPoint point)
        => point.X >= area.Left && point.X <= area.Right && point.Y >= area.Bottom && point.Y <= area.Top;

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

    /// <summary>
    /// OCRs pictures on a page that has a text layer (screenshots, scanned inserts). An image covering half the page or
    /// more with invisible text over it is the scan behind a searchable PDF's text layer: <see cref="VerifyTextLayer"/>
    /// compares the two, so it is not read again here. A large image without such a layer (a full-page screenshot, a
    /// poster) is read like any other, or reported as not inspected. Sizes are checked from the PDF's image dictionary
    /// before anything is decoded.
    /// </summary>
    private static void AppendEmbeddedImages(Page page, int pageNum, StringBuilder sb, ExtractionOptions? options,
        EmbeddedImageOcr.Counter counter, List<(string Source, string Text, string How)> hidden, List<Letter> layerOverImages)
    {
        double pageArea = Math.Max(1, page.Width * page.Height);
        var bounds = page.CropBox.Bounds;
        long maxPixels = options?.MaxImagePixels ?? ExtractionOptions.Default.MaxImagePixels;
        foreach (var image in page.GetImages())
        {
            var box = image.BoundingBox;
            if (Math.Abs(box.Width * box.Height) >= pageArea / 2 && layerOverImages.Any(l => Contains(box, l.BoundingBox.Centroid)))
                continue;
            if (image.WidthInSamples < EmbeddedImageOcr.MinWidth || image.HeightInSamples < EmbeddedImageOcr.MinHeight) continue;
            if (!EmbeddedImageOcr.Enabled(options))
            {
                counter.Unread++; // counted, not decoded
                continue;
            }
            if ((long)image.WidthInSamples * image.HeightInSamples > maxPixels)
            {
                options!.WarningSink?.Invoke(new ExtractionWarning(ExtractionWarningCodes.NeedsOcr, $"page {pageNum}",
                    $"Image of {image.WidthInSamples}x{image.HeightInSamples} pixels not read; the limit is {maxPixels} (ExtractionOptions.MaxImagePixels)."));
                continue;
            }
            bool offPage = box.Right < bounds.Left || box.Left > bounds.Right || box.Top < bounds.Bottom || box.Bottom > bounds.Top;
            byte[] encoded = image.TryGetPng(out var png) ? png : image.RawBytes.ToArray();
            EmbeddedImageOcr.Append(sb, encoded, $"page {pageNum} image", options, counter,
                offPage ? "image placed outside the page" : null, (text, why) => hidden.Add(($"page {pageNum}", text, why)));
        }
    }

    /// <summary>
    /// Invisible text over an image is normally the text layer of a searchable scan, but geometry alone does not prove
    /// it matches the picture. With OCR, the images under it are read and compared word by word: if more than half of
    /// the layer's words (3+ letters) are not in the picture, the layer is hidden text (instruction wording in it is
    /// then High). Otherwise only the words the picture does not show are checked, for instruction wording or a long
    /// unbroken run. Without OCR the page is counted as unverified and reported, so the result is marked partial.
    /// </summary>
    private static void VerifyTextLayer(Page page, int pageNum, List<Letter> layer, ExtractionOptions? options,
        EmbeddedImageOcr.Counter counter, List<(string Source, string Text, string How)> hidden)
    {
        var layerText = WordsToText(NearestNeighbourWordExtractor.Instance.GetWords(layer));
        var layerWords = Words(layerText);
        if (layerWords.Count < 3) return;
        if (!EmbeddedImageOcr.Enabled(options))
        {
            counter.UnverifiedPages++;
            return;
        }

        var centres = layer.Select(l => l.BoundingBox.Centroid).ToList();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        long maxPixels = options!.MaxImagePixels;
        foreach (var image in page.GetImages())
        {
            var a = image.BoundingBox;
            if (!centres.Any(c => c.X >= a.Left && c.X <= a.Right && c.Y >= a.Bottom && c.Y <= a.Top)) continue;
            if ((long)image.WidthInSamples * image.HeightInSamples > maxPixels || counter.Read >= options.MaxOcrImages)
            {
                counter.UnverifiedPages++;
                return;
            }
            byte[] encoded = image.TryGetPng(out var png) ? png : image.RawBytes.ToArray();
            if (ContentTypeDetector.SniffExtension(encoded) is not { } format || !ContentTypeDetector.IsImageExtension(format)) continue;
            counter.Read++;
            try
            {
                seen.UnionWith(Words(ImageExtractor.Recognize(encoded, format, options.OcrEngine!, options, $"page {pageNum}")));
            }
            catch (ExtractionLimitExceededException)
            {
                counter.UnverifiedPages++;
                return;
            }
        }

        int unmatched = layerWords.Count(w => !seen.Contains(w));
        if (unmatched * 2 > layerWords.Count)
        {
            hidden.Add(($"page {pageNum}", layerText, "invisible text over an image that does not match what the image shows"));
            return;
        }

        // A layer that mostly matches can still carry a short passage the picture does not show, such as an instruction
        // appended to a genuine OCR layer. Unmatched words with instruction wording are hidden text (High); so is a long
        // unbroken unmatched run (Low on its own). Shorter scattered misses are ordinary OCR error.
        var tokens = WordPattern.Matches(layerText);
        var missing = tokens.Where(m => !seen.Contains(m.Value.ToLowerInvariant())).ToList();
        if (missing.Count == 0) return;
        var missingText = string.Join(" ", missing.Select(m => m.Value));
        if (SafetyGate.ContainsInstruction(missingText, options.Safety ?? SafetyPolicy.Default))
        {
            hidden.Add(($"page {pageNum}", missingText, "invisible text over an image, not shown in the image"));
            return;
        }

        var runs = new List<string>();
        for (int i = 0; i < tokens.Count;)
        {
            if (seen.Contains(tokens[i].Value.ToLowerInvariant())) { i++; continue; }
            int j = i;
            while (j + 1 < tokens.Count && !seen.Contains(tokens[j + 1].Value.ToLowerInvariant())) j++;
            if (j - i + 1 >= MinUnmatchedRun)
                runs.Add(layerText[tokens[i].Index..(tokens[j].Index + tokens[j].Length)]);
            i = j + 1;
        }
        if (runs.Count > 0)
            hidden.Add(($"page {pageNum}", string.Join(Environment.NewLine, runs), "invisible text over an image, not shown in the image"));
    }

    /// <summary>Consecutive layer words missing from the OCR text that count as a hidden passage, not OCR error.</summary>
    private const int MinUnmatchedRun = 8;

    private static readonly System.Text.RegularExpressions.Regex WordPattern = new(@"[\p{L}\p{N}]{3,}",
        System.Text.RegularExpressions.RegexOptions.Compiled);

    private static HashSet<string> Words(string text)
        => WordPattern.Matches(text.ToLowerInvariant()).Select(m => m.Value).ToHashSet(StringComparer.Ordinal);

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
        sb.AppendLine(ModelInput.MarkerLine(options, "Page", pageNum, ocr: true));
        sb.AppendLine(string.Join(Environment.NewLine, texts));
        sb.AppendLine();
    }
}
