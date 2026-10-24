using System.Text;

namespace Transformations.Text;

/// <summary>
/// OCR for images embedded in documents (DOCX, PPTX, XLSX, ODT/ODP, PDF pages that have text, HTML <c>&lt;img&gt;</c>
/// <c>data:</c> URIs). Only images the document actually shows are considered: the extractors walk the document's own
/// image references, not every media part in the package. Icons and decorations smaller than
/// <see cref="MinWidth"/>×<see cref="MinHeight"/> pixels are ignored.
/// <list type="bullet">
/// <item>With an available <see cref="ExtractionOptions.OcrEngine"/> and <see cref="ExtractionOptions.OcrEmbeddedImages"/>
/// on, at most <see cref="ExtractionOptions.MaxOcrImages"/> images are read per document; each one's text is an
/// <c>[Image N] (OCR)</c> block reported with <see cref="ExtractionWarningCodes.OcrText"/>.</item>
/// <item>A hidden image (hidden element, hidden run, shape or sheet, off the page) is still read, and its text is hidden
/// content: reported as a hidden-content finding and rendered by <see cref="ExtractionOptions.HiddenText"/>.</item>
/// <item>Images left unread (no engine, OCR turned off, over the limit) are reported with
/// <see cref="ExtractionWarningCodes.NeedsOcr"/>, so the result is marked partial rather than looking fully inspected.</item>
/// </list>
/// </summary>
internal static class EmbeddedImageOcr
{
    internal const int MinWidth = 100;
    internal const int MinHeight = 40;

    internal static bool Enabled(ExtractionOptions? options)
        => options?.OcrEmbeddedImages != false && ImageExtractor.OcrAvailable(options);

    /// <summary>Per-document counts.</summary>
    internal sealed class Counter
    {
        public int Read;
        public int OverLimit;
        public int Unread;
        public int UnverifiedPages;
    }

    /// <summary>
    /// Decides for an image of this size: <c>true</c> = read it now; <c>false</c> = ignore it (too small) or count it as
    /// not read (no OCR, over the limit).
    /// </summary>
    internal static bool ShouldRead(int width, int height, ExtractionOptions? options, Counter counter)
    {
        if (width < MinWidth || height < MinHeight) return false;
        if (!Enabled(options))
        {
            counter.Unread++;
            return false;
        }
        if (counter.Read >= options!.MaxOcrImages)
        {
            counter.OverLimit++;
            return false;
        }
        return true;
    }

    /// <summary>
    /// OCRs one encoded image (PNG, JPEG, ...) and appends its text block. <paramref name="hiddenHow"/> says why the
    /// image is hidden, or is <c>null</c> for a visible image; hidden text goes to <paramref name="onHidden"/> and is
    /// rendered by <see cref="ExtractionOptions.HiddenText"/>. Failures are warnings, not errors.
    /// </summary>
    internal static void Append(StringBuilder sb, byte[] image, string where, ExtractionOptions? options, Counter counter,
        string? hiddenHow = null, Action<string, string>? onHidden = null)
    {
        ExtractionGuard.ThrowIfCancellationRequested(options);
        var format = ContentTypeDetector.SniffExtension(image);
        if (!ContentTypeDetector.IsImageExtension(format)) return; // EMF/WMF/SVG and other non-raster parts
        if (ImageHeader.ReadSize(image) is not var (width, height) || !ShouldRead(width, height, options, counter)) return;

        counter.Read++;
        try
        {
            var text = ImageExtractor.Recognize(image, format!, options!.OcrEngine!, options, where)?.Trim();
            if (string.IsNullOrWhiteSpace(text)) return;
            if (hiddenHow != null)
            {
                onHidden?.Invoke(text, hiddenHow);
                if (SafetyGate.RenderHidden(options, text) is not { } rendered) return;
                text = rendered;
            }
            sb.AppendLine(ModelInput.MarkerLine(options, "Image", counter.Read, ocr: true));
            sb.AppendLine(text);
            sb.AppendLine();
        }
        catch (ExtractionLimitExceededException ex)
        {
            options!.WarningSink?.Invoke(new ExtractionWarning(ExtractionWarningCodes.NeedsOcr, where, $"Embedded image not read: {ex.Message}"));
        }
    }

    /// <summary>Reports images left unread, and invisible text layers that could not be compared with their image.</summary>
    internal static void ReportSkipped(ExtractionOptions? options, Counter counter, string source)
    {
        if (options?.WarningSink is not { } warn) return;
        if (counter.Unread > 0)
            warn(new ExtractionWarning(ExtractionWarningCodes.NeedsOcr, source,
                $"{counter.Unread} embedded images were not inspected: " +
                (ImageExtractor.OcrAvailable(options) ? "OcrEmbeddedImages is off." : "no OCR engine is available (ExtractionOptions.OcrEngine).")));
        if (counter.OverLimit > 0)
            warn(new ExtractionWarning(ExtractionWarningCodes.NeedsOcr, source,
                $"{counter.OverLimit} embedded images were not OCR'd: the limit is {options.MaxOcrImages} per document (ExtractionOptions.MaxOcrImages)."));
        if (counter.UnverifiedPages > 0)
            warn(new ExtractionWarning(ExtractionWarningCodes.NeedsOcr, source,
                $"Invisible text over images on {counter.UnverifiedPages} pages was not compared with the images (needs OCR); it is treated as a searchable-scan text layer."));
    }

    /// <summary>Reads a package part into memory (the package was checked against the decompression limits before parsing).</summary>
    internal static byte[] ReadPart(Stream stream)
    {
        using var ms = new MemoryStream();
        stream.CopyTo(ms);
        return ms.ToArray();
    }
}
