using System.Text;

namespace Transformations.Text;

/// <summary>
/// OCR for images embedded in documents (DOCX, PPTX, XLSX, ODT/ODP, PDF pages that have text, HTML <c>data:</c> URIs).
/// Runs only when <see cref="ExtractionOptions.OcrEngine"/> is available and <see cref="ExtractionOptions.OcrEmbeddedImages"/>
/// is on. Icons and decorations smaller than <see cref="MinWidth"/>×<see cref="MinHeight"/> pixels are skipped, and at most
/// <see cref="ExtractionOptions.MaxOcrImages"/> images are read per document, because OCR takes around a second per
/// page-sized image; skipped images are reported. Each image's text is appended as an <c>[Image N] (OCR)</c> block and
/// reported with <see cref="ExtractionWarningCodes.OcrText"/>, so the safety gate scores the document as OCR text.
/// </summary>
internal static class EmbeddedImageOcr
{
    internal const int MinWidth = 100;
    internal const int MinHeight = 40;

    internal static bool Enabled(ExtractionOptions? options)
        => options?.OcrEmbeddedImages != false && ImageExtractor.OcrAvailable(options);

    /// <summary>Per-document count of images read and images skipped over the limit.</summary>
    internal sealed class Counter
    {
        public int Read;
        public int OverLimit;
    }

    /// <summary>True when an image of this size is worth reading and the document's limit allows it.</summary>
    internal static bool ShouldRead(int width, int height, ExtractionOptions options, Counter counter)
    {
        if (width < MinWidth || height < MinHeight) return false;
        if (counter.Read >= options.MaxOcrImages)
        {
            counter.OverLimit++;
            return false;
        }
        return true;
    }

    /// <summary>OCRs one encoded image (PNG, JPEG, ...) and appends its text block. Failures are warnings, not errors.</summary>
    internal static void Append(StringBuilder sb, byte[] image, string where, ExtractionOptions options, Counter counter)
    {
        ExtractionGuard.ThrowIfCancellationRequested(options);
        var format = ContentTypeDetector.SniffExtension(image);
        if (!ContentTypeDetector.IsImageExtension(format)) return; // EMF/WMF/SVG and other non-raster parts
        if (ImageHeader.ReadSize(image) is not var (width, height) || !ShouldRead(width, height, options, counter)) return;

        counter.Read++;
        try
        {
            var text = ImageExtractor.Recognize(image, format!, options.OcrEngine!, options, where);
            if (string.IsNullOrWhiteSpace(text)) return;
            sb.AppendLine($"[Image {counter.Read}] (OCR)");
            sb.AppendLine(text.Trim());
            sb.AppendLine();
        }
        catch (ExtractionLimitExceededException ex)
        {
            options.WarningSink?.Invoke(new ExtractionWarning(ExtractionWarningCodes.NeedsOcr, where, $"Embedded image not read: {ex.Message}"));
        }
    }

    /// <summary>Reports images left unread because of <see cref="ExtractionOptions.MaxOcrImages"/>.</summary>
    internal static void ReportSkipped(ExtractionOptions options, Counter counter, string source)
    {
        if (counter.OverLimit == 0) return;
        options.WarningSink?.Invoke(new ExtractionWarning(ExtractionWarningCodes.NeedsOcr, source,
            $"{counter.OverLimit} embedded images were not OCR'd: the limit is {options.MaxOcrImages} per document (ExtractionOptions.MaxOcrImages)."));
    }

    /// <summary>Reads a package part into memory (the package was checked against the decompression limits before parsing).</summary>
    internal static byte[] ReadPart(Stream stream)
    {
        using var ms = new MemoryStream();
        stream.CopyTo(ms);
        return ms.ToArray();
    }
}
