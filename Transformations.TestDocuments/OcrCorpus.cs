using System.Collections.Generic;
using System.IO;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Core;
using UglyToad.PdfPig.Writer;

namespace Transformations.TestDocuments;

/// <summary>
/// Image and scanned-page samples for the OCR path. The image is the repository's OCR fixture (a 900x160 invoice line
/// that Tesseract reads as "Invoice 4711" and "99 EUR"). Rows with <see cref="CorpusSample.RequiresTesseract"/> run only
/// when trained data is configured; the others run everywhere.
/// </summary>
public static class OcrCorpus
{
    private const string Expected1 = "Invoice 4711";
    private const string Expected2 = "99 EUR";

    public static IEnumerable<CorpusSample> All()
    {
        // ── No OCR engine: must not claim the text was read ──
        yield return new("png image without OCR engine", "ocr-sample.png",
            () => InvoiceImage(),
            CorpusLabel.Ocr, "A top-level image with no engine is NeedsOcr.",
            MustNotContain: [Expected1]);

        yield return new("scanned pdf page without OCR engine", "scanned-page.pdf",
            () => ScannedPdf(),
            CorpusLabel.Ocr, "An image-only page with no engine is partial content.",
            MustNotContain: [Expected1]);

        // ── Fixed-text engine: always runs ──
        yield return new("png image with fixed OCR text", "ocr-fixed.png",
            () => InvoiceImage(),
            CorpusLabel.Ocr, "The OCR path with a fixed-text engine.",
            OcrText: $"{Expected1} {Expected2}",
            MustContain: [Expected1, Expected2]);

        // ── Real Tesseract: runs when TRANSFORMATIONS_TESSDATA is configured ──
        yield return new("png image with Tesseract", "ocr-tesseract.png",
            () => InvoiceImage(),
            CorpusLabel.Ocr, "The real engine reads the invoice line.",
            MustContain: [Expected1, Expected2],
            RequiresTesseract: true);

        yield return new("scanned pdf page with Tesseract", "scanned-tesseract.pdf",
            () => ScannedPdf(),
            CorpusLabel.Ocr, "The real engine reads an image-only PDF page.",
            MustContain: [Expected1, Expected2],
            RequiresTesseract: true);
    }

    private static byte[] InvoiceImage() => ReadEmbedded("ocr-sample.png");

    private static byte[] ScannedPdf()
    {
        var builder = new PdfDocumentBuilder();
        builder.AddPage(PageSize.A4).AddPng(InvoiceImage(), new PdfRectangle(0, 0, 500, 89));
        return builder.Build();
    }

    private static byte[] ReadEmbedded(string name)
    {
        using Stream stream = typeof(OcrCorpus).Assembly.GetManifestResourceStream($"Transformations.TestDocuments.Assets.{name}")
            ?? throw new FileNotFoundException($"Embedded asset '{name}' is missing from Transformations.TestDocuments.");
        using var memory = new MemoryStream();
        stream.CopyTo(memory);
        return memory.ToArray();
    }
}
