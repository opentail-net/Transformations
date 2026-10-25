using System.Collections.Generic;
using Transformations.Text;

namespace Transformations.TestDocuments;

/// <summary>What a corpus sample is for. Ordinary samples check extraction; the rest check the safety contract.</summary>
public enum CorpusLabel
{
    /// <summary>An ordinary document whose expected text is listed in <see cref="CorpusSample.MustContain"/>.</summary>
    Ordinary,

    /// <summary>An adversarial document that must be caught: ReviewRequired, PartsWithheld or Quarantined.</summary>
    Malicious,

    /// <summary>A documented miss. It must stay uncaught until it is moved to <see cref="Malicious"/>.</summary>
    KnownGap,

    /// <summary>A benign document that must not be caught, so it is never quarantined.</summary>
    Benign,

    /// <summary>A malformed or misnamed file. It must fail or warn, and must never come back silently empty.</summary>
    Malformed,

    /// <summary>
    /// An image or scanned page. With no OCR engine it must report <c>NeedsOcr</c> or partial content; with a fixed
    /// or Tesseract engine its text must be extracted, labelled as OCR output.
    /// </summary>
    Ocr,
}

/// <summary>
/// One corpus entry. <see cref="Build"/> produces the bytes; <see cref="FileName"/> is the name the extractor sees.
/// The optional lists are the expected text for ordinary samples: markers that must appear in extracted text, and
/// markers that must never appear in model input. <see cref="OcrText"/> runs the sample through a fixed-text engine;
/// <see cref="RequiresTesseract"/> runs it through the real Tesseract engine when trained data is configured.
/// </summary>
public sealed record CorpusSample(
    string Name,
    string FileName,
    Func<byte[]> Build,
    CorpusLabel Label,
    string Note = "",
    string? OcrText = null,
    IReadOnlyList<string>? MustContain = null,
    IReadOnlyList<string>? MustNotContain = null,
    bool RequiresTesseract = false);

/// <summary>An OCR engine that returns fixed text, so OCR-path samples run without Tesseract.</summary>
public sealed class FakeOcrEngine(string text) : IOcrEngine
{
    public bool IsAvailable => true;

    public OcrResult Recognize(byte[] image, CancellationToken cancellationToken) => new(text);
}
