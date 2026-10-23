using System.IO.Compression;
using System.Text;
using MimeKit;
using NUnit.Framework;
using Transformations.Text;
using Transformations.Text.Ocr;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Core;
using UglyToad.PdfPig.Writer;

namespace Transformations.Tests;

/// <summary>
/// OCR hook behaviour with a fake engine (deterministic, no native code), plus one real Tesseract run that is
/// skipped unless TRANSFORMATIONS_TESSDATA points at a folder containing eng.traineddata.
/// </summary>
[TestFixture]
public class TextExtractorOcrTests
{
    private static readonly byte[] Png1x1 = Convert.FromBase64String(
        "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNkYPhfDwAChwGA60e6kgAAAABJRU5ErkJggg==");

    private static string SamplePath => Path.Combine(TestContext.CurrentContext.TestDirectory, "TestData", "ocr-sample.png");

    private TextExtractor _extractor = null!;

    [SetUp]
    public void Setup() => _extractor = new TextExtractor();

    [Test]
    public void TopLevelImage_WithEngine_ReturnsOcrTextAndUntrustedWarning()
    {
        var engine = new FakeOcr("Pay to account 123");

        var result = _extractor.GetText("photo.png", Png1x1, new ExtractionOptions { OcrEngine = engine });

        Assert.That(result.IsSuccess, Is.True, result.ErrorMessage);
        Assert.That(result.Text, Is.EqualTo("Pay to account 123"));
        Assert.That(result.ExtractorName, Is.EqualTo(nameof(ImageExtractor)));
        Assert.That(result.Warnings.Single().Code, Is.EqualTo(ExtractionWarningCodes.OcrText));
    }

    [Test]
    public void UnavailableEngine_IsNeverCalled()
    {
        var engine = new FakeOcr("x") { Available = false };

        var result = _extractor.GetText("photo.png", Png1x1, new ExtractionOptions { OcrEngine = engine });

        Assert.That(result.ErrorKind, Is.EqualTo(ExtractionErrorKind.NeedsOcr));
        Assert.That(result.ErrorMessage, Does.Contain("not available on this platform"));
        Assert.That(engine.Calls, Is.Zero);
    }

    [Test]
    public void ImageOverPixelLimit_IsRefusedBeforeDecoding()
    {
        var engine = new FakeOcr("x");
        var png = (byte[])Png1x1.Clone();
        png[16] = 0x00; png[17] = 0x01; png[18] = 0x00; png[19] = 0x00; // width 65536
        png[20] = 0x00; png[21] = 0x01; png[22] = 0x00; png[23] = 0x00; // height 65536

        var result = _extractor.GetText("bomb.png", png, new ExtractionOptions { OcrEngine = engine });

        Assert.That(result.ErrorKind, Is.EqualTo(ExtractionErrorKind.LimitExceeded));
        Assert.That(engine.Calls, Is.Zero);
    }

    [Test]
    public void NonImageWithImageExtension_IsNotSentToOcr()
    {
        var engine = new FakeOcr("x");

        var result = _extractor.GetText("photo.jpg", Encoding.UTF8.GetBytes("plain text, not a JPEG"),
            new ExtractionOptions { OcrEngine = engine });

        Assert.That(result.ErrorKind, Is.EqualTo(ExtractionErrorKind.Corrupted));
        Assert.That(engine.Calls, Is.Zero);
    }

    [Test]
    public void EmlImageAttachment_WithEngine_IsOcrdAndMarked()
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("A", "a@example.com"));
        var builder = new BodyBuilder { TextBody = "See scan." };
        builder.Attachments.Add("scan.png", Png1x1);
        message.Body = builder.ToMessageBody();
        using var ms = new MemoryStream();
        message.WriteTo(ms);

        var result = _extractor.GetText("m.eml", ms.ToArray(), new ExtractionOptions { OcrEngine = new FakeOcr("scanned words") });

        Assert.That(result.Text, Does.Contain($"[Attachment: scan.png] (OCR){Environment.NewLine}scanned words"));
        Assert.That(result.Warnings.Select(w => w.Code), Does.Contain(ExtractionWarningCodes.OcrText));
    }

    [Test]
    public void ZipImageEntry_WithoutEngine_StaysAStub()
    {
        using var zipMs = new MemoryStream();
        using (var zip = new ZipArchive(zipMs, ZipArchiveMode.Create, leaveOpen: true))
        {
            using var s = zip.CreateEntry("scan.png").Open();
            s.Write(Png1x1);
        }

        var result = _extractor.GetText("a.zip", zipMs.ToArray());

        Assert.That(result.Text, Does.Contain("[scan.png] not extracted: image (.png)"));
        Assert.That(result.Warnings.Single().Code, Is.EqualTo(ExtractionWarningCodes.NeedsOcr));
    }

    [Test]
    public void ScannedPdfPage_WithEngine_IsOcrd()
    {
        var builder = new PdfDocumentBuilder();
        builder.AddPage(PageSize.A4).AddPng(Png1x1, new PdfRectangle(0, 0, 500, 700));

        var result = _extractor.GetText("scan.pdf", builder.Build(), new ExtractionOptions { OcrEngine = new FakeOcr("page words") });

        Assert.That(result.Text, Does.Contain($"[Page 1] (OCR){Environment.NewLine}page words"));
        Assert.That(result.Warnings.Single().Code, Is.EqualTo(ExtractionWarningCodes.OcrText));
    }

    [TestCase(".png", 900, 160)]
    public void ImageHeader_ReadsDimensions(string _, int width, int height)
    {
        var engine = new FakeOcr("x");
        var bytes = File.ReadAllBytes(SamplePath);

        // A pixel limit just below the real size proves the header was read correctly.
        var result = _extractor.GetText("s.png", bytes, new ExtractionOptions { OcrEngine = engine, MaxImagePixels = width * height - 1 });

        Assert.That(result.ErrorMessage, Does.Contain($"{width}x{height}"));
    }

    [Test]
    public void TesseractEngine_MissingLanguageData_IsUnavailable()
    {
        using var engine = new TesseractOcrEngine(Path.GetTempPath(), "zzz");

        Assert.That(engine.IsAvailable, Is.False);
        Assert.That(engine.UnavailableReason, Is.Not.Null);
    }

    [Test]
    public void TesseractEngine_ReadsSampleImage()
    {
        var tessdata = Environment.GetEnvironmentVariable("TRANSFORMATIONS_TESSDATA");
        if (string.IsNullOrEmpty(tessdata) || !TesseractOcrEngine.IsPlatformSupported)
            Assert.Ignore("Set TRANSFORMATIONS_TESSDATA to a folder with eng.traineddata (Windows x64/x86) to run.");

        using var engine = new TesseractOcrEngine(tessdata, "eng");
        Assert.That(engine.IsAvailable, Is.True, engine.UnavailableReason);

        var result = _extractor.GetText("invoice.png", File.ReadAllBytes(SamplePath), new ExtractionOptions { OcrEngine = engine });

        Assert.That(result.IsSuccess, Is.True, result.ErrorMessage);
        Assert.That(result.Text, Does.Contain("Invoice 4711").And.Contain("99 EUR"));
    }

    private sealed class FakeOcr(string text) : IOcrEngine
    {
        public bool Available { get; init; } = true;
        public int Calls;
        public bool IsAvailable => Available;

        public OcrResult Recognize(byte[] image, CancellationToken cancellationToken)
        {
            Interlocked.Increment(ref Calls);
            return new OcrResult(text, 0.9f);
        }
    }
}
