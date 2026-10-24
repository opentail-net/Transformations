using System.IO.Compression;
using System.Text;
using DocumentFormat.OpenXml.Packaging;
using MimeKit;
using NUnit.Framework;
using Transformations.Text;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Core;
using UglyToad.PdfPig.Fonts.Standard14Fonts;
using UglyToad.PdfPig.Writer;
using DA = DocumentFormat.OpenXml.Drawing;
using P = DocumentFormat.OpenXml.Presentation;
using W = DocumentFormat.OpenXml.Wordprocessing;

namespace Transformations.Tests;

/// <summary>
/// Regression tests for the issues raised in the external code review of Transformations.Text,
/// plus extension/content mismatch reporting.
/// </summary>
[TestFixture]
public class TextExtractorReviewFixTests
{
    private TextExtractor _extractor = null!;

    [SetUp]
    public void Setup() => _extractor = new TextExtractor();

    // ── TextChunker ──────────────────────────────────────────────────────────

    [TestCase(50, 0)]
    [TestCase(50, 10)]
    [TestCase(20, 5)]
    [TestCase(7, 6)]
    public void ChunkByCharacters_LongWordAfterShortWord_LosesNoCharacters(int maxLength, int overlap)
    {
        var text = "Hi " + new string('x', 200) + " tail words here";

        var chunks = TextChunker.ChunkByCharacters(text, maxLength, overlap);

        AssertCoversEveryCharacter(text, chunks);
    }

    [TestCase(10, 0)]
    [TestCase(10, 3)]
    [TestCase(33, 8)]
    public void ChunkByCharacters_MixedText_CoversEveryCharacterAndRespectsMaxLength(int maxLength, int overlap)
    {
        var text = "The quick brown fox jumps over the lazy dog. Pneumonoultramicroscopicsilicovolcanoconiosis is long. a b c";

        var chunks = TextChunker.ChunkByCharacters(text, maxLength, overlap);

        AssertCoversEveryCharacter(text, chunks);
        Assert.That(chunks.All(c => c.Text.Length <= maxLength), Is.True);
        Assert.That(chunks.All(c => c.Text == text[c.StartOffset..c.EndOffset]), Is.True);
    }

    [Test]
    public void ChunkByCharacters_TextShorterThanMax_ProducesOneChunk()
    {
        var chunks = TextChunker.ChunkByCharacters(new string('a', 45), maxLength: 50, overlap: 10);

        Assert.That(chunks, Has.Count.EqualTo(1));
    }

    [TestCase("Mr. Smith arrived. He sat down.", "Mr. Smith arrived.")]
    [TestCase("Dr. Jones agreed. Then left.", "Dr. Jones agreed.")]
    [TestCase("Pi is 3.14 roughly. Yes.", "Pi is 3.14 roughly.")]
    [TestCase("It was vs. them. Done.", "It was vs. them.")]
    public void ChunkBySentences_DoesNotSplitAfterAbbreviationsOrDecimals(string text, string firstSentence)
    {
        var chunks = TextChunker.ChunkBySentences(text, maxLength: firstSentence.Length);

        Assert.That(chunks[0].Text, Is.EqualTo(firstSentence));
    }

    private static void AssertCoversEveryCharacter(string text, IReadOnlyList<TextChunk> chunks)
    {
        var covered = new bool[text.Length];
        foreach (var c in chunks)
            for (int i = c.StartOffset; i < c.EndOffset; i++)
                covered[i] = true;
        var missing = Enumerable.Range(0, text.Length).Where(i => !covered[i]).ToList();
        Assert.That(missing, Is.Empty, $"characters not in any chunk: {string.Join(",", missing.Take(10))}");
    }

    // ── CodeExtractor ────────────────────────────────────────────────────────

    [Test]
    public void CodeExtractor_KeepsCodeBetweenOrdinaryCommentAndCopyrightComment()
    {
        var code = "/* Ordinary explanatory comment */\nint importantValue = 42;\n\n/* Copyright 2026 Example Corp. */\nConsole.WriteLine(importantValue);";

        var result = _extractor.GetText("Program.cs", Encoding.UTF8.GetBytes(code));

        Assert.That(result.Text, Does.Contain("int importantValue = 42;"));
        Assert.That(result.Text, Does.Contain("Ordinary explanatory comment"));
        Assert.That(result.Text, Does.Contain("Console.WriteLine(importantValue);"));
        // Only the leading header is stripped; a copyright comment after code is part of the source.
        Assert.That(result.Text, Does.Contain("/* Copyright 2026 Example Corp. */"));
    }

    [Test]
    public void CodeExtractor_StillRemovesLeadingLicenceHeader()
    {
        var code = "/*\n * Copyright (c) 2026 Example\n * All rights reserved.\n */\nclass A {}";

        var result = _extractor.GetText("A.cs", Encoding.UTF8.GetBytes(code));

        Assert.That(result.Text, Is.EqualTo("class A {}"));
    }

    // ── XmlExtractor ─────────────────────────────────────────────────────────

    [Test]
    public void XmlExtractor_KeepsMixedContentText()
    {
        var result = _extractor.GetText("doc.xml", Encoding.UTF8.GetBytes("<p>Hello <b>world</b> again</p>"));

        Assert.That(result.Text, Does.Contain("p: Hello again"));
        Assert.That(result.Text, Does.Contain("p.b: world"));
    }

    [Test]
    public void XmlExtractor_LeafElementsUnchanged()
    {
        var result = _extractor.GetText("doc.xml", Encoding.UTF8.GetBytes("<a><b>one</b><c>two</c></a>"));

        Assert.That(result.Text, Is.EqualTo($"a.b: one{Environment.NewLine}a.c: two"));
    }

    // ── RtfExtractor ─────────────────────────────────────────────────────────

    [Test]
    public void RtfExtractor_SurrogatePair_ProducesCharacterInsteadOfFailing()
    {
        // U+1F600 = UTF-16 D83D DE00 = signed 16-bit -10179, -8704
        var rtf = @"{\rtf1\ansi smile \u-10179?\u-8704? done}";

        var result = _extractor.GetText("note.rtf", Encoding.ASCII.GetBytes(rtf));

        Assert.That(result.IsSuccess, Is.True, result.ErrorMessage);
        Assert.That(result.Text, Does.Contain("smile \U0001F600 done"));
    }

    // ── HtmlExtractor ────────────────────────────────────────────────────────

    [Test]
    public void HtmlExtractor_BrBecomesLineBreak()
    {
        var result = _extractor.GetText("p.html", Encoding.UTF8.GetBytes("<html><body><p>first<br>second<br/>third</p></body></html>"));

        Assert.That(result.Text, Is.EqualTo($"first{Environment.NewLine}second{Environment.NewLine}third"));
    }

    [TestCase("<meta charset=\"windows-1252\">")]
    [TestCase("<meta http-equiv=\"Content-Type\" content=\"text/html; charset=windows-1252\">")]
    public void HtmlExtractor_HonoursDeclaredLegacyEncoding(string meta)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        var bytes = Encoding.GetEncoding(1252).GetBytes($"<html><head>{meta}</head><body><p>café</p></body></html>");

        var result = _extractor.GetText("legacy.html", bytes);

        Assert.That(result.Text, Is.EqualTo("café"));
    }

    [Test]
    public void HtmlExtractor_DefaultsToUtf8()
    {
        var result = _extractor.GetText("p.html", Encoding.UTF8.GetBytes("<p>naïve — ✓</p>"));

        Assert.That(result.Text, Is.EqualTo("naïve — ✓"));
    }

    [Test]
    public void EmlHtmlBody_WithCharsetMeta_IsNotDecodedTwice()
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("A", "a@example.com"));
        message.Subject = "s";
        message.Body = new TextPart("html")
        {
            Text = "<html><head><meta charset=\"iso-8859-1\"></head><body><p>Grüße</p></body></html>"
        };
        using var ms = new MemoryStream();
        message.WriteTo(ms);

        var result = _extractor.GetText("m.eml", ms.ToArray());

        Assert.That(result.Text, Does.Contain("Grüße"));
    }

    // ── Single-row and header-only tables ────────────────────────────────────

    [Test]
    public void HtmlSingleRowTable_IsNotDropped()
    {
        var result = _extractor.GetText("t.html",
            Encoding.UTF8.GetBytes("<table><tr><td>Only</td><td>Row</td></tr></table>"));

        Assert.That(result.Text, Does.Contain("Only | Row"));
    }

    [Test]
    public void DocxSingleRowTable_IsNotDropped()
    {
        using var ms = new MemoryStream();
        using (var doc = WordprocessingDocument.Create(ms, DocumentFormat.OpenXml.WordprocessingDocumentType.Document))
        {
            var main = doc.AddMainDocumentPart();
            main.Document = new W.Document(new W.Body(new W.Table(new W.TableRow(
                new W.TableCell(new W.Paragraph(new W.Run(new W.Text("Lonely")))),
                new W.TableCell(new W.Paragraph(new W.Run(new W.Text("Cells"))))))));
        }

        var result = _extractor.GetText("t.docx", ms.ToArray());

        Assert.That(result.Text, Does.Contain("Lonely | Cells"));
    }

    [Test]
    public void CsvHeaderOnly_IsNotDropped()
    {
        var result = _extractor.GetText("t.csv", Encoding.UTF8.GetBytes("Name,Price\n"));

        Assert.That(result.Text, Is.EqualTo("Name | Price"));
    }

    [Test]
    public void HtmlMultiRowTable_StillUsesFirstRowAsKeys()
    {
        var result = _extractor.GetText("t.html", Encoding.UTF8.GetBytes(
            "<table><tr><th>Name</th><th>Price</th></tr><tr><td>Widget</td><td>9</td></tr></table>"));

        Assert.That(result.Text, Does.Contain("Name: Widget | Price: 9"));
    }

    // ── PPTX ─────────────────────────────────────────────────────────────────

    [Test]
    public void Pptx_SlideMarkers_FollowIncludePageMarkers()
    {
        var pptx = CreatePptx();

        var without = _extractor.GetText("d.pptx", pptx);
        var with = _extractor.GetText("d.pptx", pptx, new ExtractionOptions { IncludePageMarkers = true });

        Assert.That(without.Text, Does.Not.Contain("[Slide 1]"));
        Assert.That(with.Text, Does.StartWith("[Slide 1]"));
    }

    [Test]
    public void Pptx_ExtractsGroupedShapesAndTables()
    {
        var result = _extractor.GetText("d.pptx", CreatePptx());

        Assert.That(result.IsSuccess, Is.True, result.ErrorMessage);
        Assert.That(result.Text, Does.Contain("Top level"));
        Assert.That(result.Text, Does.Contain("Inside group"));
        Assert.That(result.Text, Does.Contain("Cell A | Cell B"));
    }

    // ── Routing, retry and mismatch ──────────────────────────────────────────

    [Test]
    public void DocxNamedPdf_IsExtractedAsDocx_WithMismatchWarning()
    {
        var result = _extractor.GetText("invoice.pdf", CreateDocx("Pay now"));

        Assert.That(result.IsSuccess, Is.True, result.ErrorMessage);
        Assert.That(result.Text, Is.EqualTo("Pay now"));
        Assert.That(result.ExtractorName, Is.EqualTo(nameof(DocxExtractor)));
        Assert.That(result.Warnings.Select(w => w.Code), Is.EqualTo(new[] { ExtractionWarningCodes.ContentTypeMismatch }));
        Assert.That(result.Warnings[0].Message, Does.Contain(".docx"));
    }

    [Test]
    public void DocxNamedPdf_RetryDisabled_FailsButStillReportsMismatch()
    {
        var result = _extractor.GetText("invoice.pdf", CreateDocx("Pay now"),
            new ExtractionOptions { RetryWithDetectedFormat = false });

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.ExtractorName, Is.EqualTo(nameof(PdfExtractor)));
        Assert.That(result.Warnings.Select(w => w.Code), Does.Contain(ExtractionWarningCodes.ContentTypeMismatch));
    }

    [Test]
    public void ZipDisguisedAsPdf_IsNotUnpacked()
    {
        var zip = CreateZip(("payload.txt", Encoding.UTF8.GetBytes("hidden")));

        var result = _extractor.GetText("invoice.pdf", zip);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Text, Is.Empty);
        Assert.That(result.ExtractorName, Is.EqualTo(nameof(PdfExtractor)));
        Assert.That(result.Warnings.Select(w => w.Code), Does.Contain(ExtractionWarningCodes.ContentTypeMismatch));
    }

    [Test]
    public void LimitExceeded_IsNotRetriedWithAnotherParser()
    {
        var bomb = AddPart(CreateDocx("x"), "customXml/pad.bin", new byte[2 * 1024 * 1024]);

        var result = _extractor.GetText("deck.pptx", bomb, new ExtractionOptions { MaxDecompressedBytes = 1024 * 1024 });

        Assert.That(result.ErrorKind, Is.EqualTo(ExtractionErrorKind.LimitExceeded));
        Assert.That(result.ExtractorName, Is.EqualTo(nameof(PptxExtractor)));
    }

    [Test]
    public void PdfNamedTxt_GetsPdfMetadataAndMismatch()
    {
        var result = _extractor.GetTextWithMetadata("upload.txt", CreatePdf("Hello PDF"));

        Assert.That(result.IsSuccess, Is.True, result.ErrorMessage);
        Assert.That(result.ExtractorName, Is.EqualTo(nameof(PdfExtractor)));
        Assert.That(result.Metadata["pdf.pageCount"], Is.EqualTo("1"));
        Assert.That(result.Metadata["file.extension"], Is.EqualTo(".txt"));
        Assert.That(result.Metadata["file.detectedFormat"], Is.EqualTo(".pdf"));
        Assert.That(result.Warnings.Select(w => w.Code), Does.Contain(ExtractionWarningCodes.ContentTypeMismatch));
    }

    [TestCase("report.docx")]
    [TestCase("report.docm")]
    [TestCase("report.zip")]
    [TestCase("report")]
    public void MatchingOrSiblingExtension_HasNoMismatch(string fileName)
    {
        var result = _extractor.GetText(fileName, CreateDocx("ok"));

        Assert.That(result.Warnings.Select(w => w.Code), fileName.EndsWith(".zip")
            ? Does.Contain(ExtractionWarningCodes.ContentTypeMismatch) // a DOCX named .zip is still a disguise
            : Does.Not.Contain(ExtractionWarningCodes.ContentTypeMismatch));
    }

    [Test]
    public void OdtWithOdtExtension_HasNoMismatch()
    {
        var odt = CreateZip(("mimetype", Encoding.ASCII.GetBytes("application/vnd.oasis.opendocument.text")),
            ("content.xml", Encoding.UTF8.GetBytes(
                "<office:document-content xmlns:office=\"urn:oasis:names:tc:opendocument:xmlns:office:1.0\" xmlns:text=\"urn:oasis:names:tc:opendocument:xmlns:text:1.0\"><office:body><office:text><text:p>Hi</text:p></office:text></office:body></office:document-content>")));

        var result = _extractor.GetText("a.odt", odt);

        Assert.That(result.Text, Is.EqualTo("Hi"));
        Assert.That(result.Warnings, Is.Empty);
    }

    [Test]
    public void TextLookingLikeEmailHeaders_IsNotAMismatch()
    {
        var result = _extractor.GetText("notes.txt", Encoding.UTF8.GetBytes("Date: tomorrow\nBring snacks."));

        Assert.That(result.Warnings, Is.Empty);
    }

    [Test]
    public void EmlAttachment_DisguisedOfficeFile_IsReported()
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("A", "a@example.com"));
        message.Subject = "Invoice";
        var builder = new BodyBuilder { TextBody = "see attached" };
        builder.Attachments.Add("invoice.pdf", CreateDocx("macro bait"));
        message.Body = builder.ToMessageBody();
        using var ms = new MemoryStream();
        message.WriteTo(ms);

        var result = _extractor.GetText("mail.eml", ms.ToArray());

        Assert.That(result.IsSuccess, Is.True);
        var mismatch = result.Warnings.Single(w => w.Code == ExtractionWarningCodes.ContentTypeMismatch);
        Assert.That(mismatch.Source, Is.EqualTo("invoice.pdf"));
    }

    [Test]
    public void ZipEntry_DisguisedFile_IsReported()
    {
        var zip = CreateZip(("readme.txt", CreatePdf("not text")));

        var result = _extractor.GetText("a.zip", zip);

        var mismatch = result.Warnings.Single(w => w.Code == ExtractionWarningCodes.ContentTypeMismatch);
        Assert.That(mismatch.Source, Is.EqualTo("readme.txt"));
    }

    // ── OLE2 detection ───────────────────────────────────────────────────────

    [TestCase("__substg1.0_0037001F", ".msg")]
    [TestCase("WordDocument", ".doc")]
    [TestCase("Workbook", ".xls")]
    [TestCase("PowerPoint Document", ".ppt")]
    [TestCase("SomethingElse", null)]
    public void DetectFormat_CompoundFile_UsesStreamNames(string streamName, string? expected)
    {
        var bytes = new byte[] { 0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1 }
            .Concat(new byte[504])
            .Concat(Encoding.Unicode.GetBytes(streamName))
            .ToArray();

        Assert.That(TextExtractor.DetectFormat(bytes), Is.EqualTo(expected));
    }

    // ── Options validation and classification ────────────────────────────────

    private static IEnumerable<TestCaseData> InvalidOptions()
    {
        yield return new TestCaseData(new ExtractionOptions { MaxCharacters = -1 }).SetName("MaxCharacters negative");
        yield return new TestCaseData(new ExtractionOptions { MaxPages = 0 }).SetName("MaxPages zero");
        yield return new TestCaseData(new ExtractionOptions { StartPage = 0 }).SetName("StartPage zero");
        yield return new TestCaseData(new ExtractionOptions { EndPage = 0 }).SetName("EndPage zero");
        yield return new TestCaseData(new ExtractionOptions { StartPage = 5, EndPage = 2 }).SetName("EndPage before StartPage");
        yield return new TestCaseData(new ExtractionOptions { MaxDecompressedBytes = -1 }).SetName("MaxDecompressedBytes negative");
        yield return new TestCaseData(new ExtractionOptions { MaxContainerEntries = -1 }).SetName("MaxContainerEntries negative");
        yield return new TestCaseData(new ExtractionOptions { MaxInputBytes = -1 }).SetName("MaxInputBytes negative");
        yield return new TestCaseData(new ExtractionOptions { Timeout = TimeSpan.Zero }).SetName("Timeout zero");
    }

    [TestCaseSource(nameof(InvalidOptions))]
    public void InvalidOptions_ThrowBeforeAnyWork(ExtractionOptions options)
    {
        var bytes = Encoding.UTF8.GetBytes("text");

        Assert.Throws<ArgumentOutOfRangeException>(() => _extractor.GetText("a.txt", bytes, options));
        Assert.Throws<ArgumentOutOfRangeException>(() => _extractor.GetText("a.txt", new MemoryStream(bytes), options));
    }

    [Test]
    public void InfiniteTimeout_IsAllowed()
    {
        var result = _extractor.GetText("a.txt", Encoding.UTF8.GetBytes("text"),
            new ExtractionOptions { Timeout = Timeout.InfiniteTimeSpan });

        Assert.That(result.IsSuccess, Is.True);
    }

    [Test]
    public void WrappedIoException_IsClassifiedAsCorrupted()
    {
        var extractor = new TextExtractor([new ThrowingExtractor(new InvalidOperationException("wrapped", new IOException("bad stream"))), new TxtExtractor()]);

        var result = extractor.GetText("x.throw", [1]);

        Assert.That(result.ErrorKind, Is.EqualTo(ExtractionErrorKind.Corrupted));
    }

    // ── Batch ────────────────────────────────────────────────────────────────

    [Test]
    public void Batch_YieldsFirstResultWhileLaterDocumentsAreStillRunning_AndStopsThemOnBreak()
    {
        var slow = new SpinUntilCancelledExtractor();
        var extractor = new TextExtractor([slow, new TxtExtractor()]);
        var docs = new List<(string, byte[])> { ("fast.txt", Encoding.UTF8.GetBytes("fast")) };
        docs.AddRange(Enumerable.Range(0, 6).Select(i => ($"s{i}.slow", new byte[] { 1 })));

        var first = Task.Run(async () =>
        {
            await foreach (var r in extractor.GetTextBatchAsync(docs, maxDegreeOfParallelism: 2))
                return r; // stop enumerating after the first result
            return null;
        });

        // The slow documents never finish on their own, so any bound proves the point; 30 s leaves room for loaded CI runners.
        Assert.That(first.Wait(TimeSpan.FromSeconds(30)), Is.True, "first result should not wait for the slow documents");
        Assert.That(first.Result!.Text, Is.EqualTo("fast"));
        Assert.That(slow.StoppedByToken, Is.True, "outstanding work should be cancelled when enumeration stops");
    }

    [Test]
    public void Batch_ResultsKeepInputOrder()
    {
        var docs = Enumerable.Range(0, 20).Select(i => ($"{i}.txt", Encoding.UTF8.GetBytes($"doc {i}"))).ToList();

        var texts = Task.Run(async () =>
        {
            var list = new List<string>();
            await foreach (var r in _extractor.GetTextBatchAsync(docs, maxDegreeOfParallelism: 3))
                list.Add(r.Text);
            return list;
        }).GetAwaiter().GetResult();

        Assert.That(texts, Is.EqualTo(Enumerable.Range(0, 20).Select(i => $"doc {i}")));
    }

    // ── Stub records ─────────────────────────────────────────────────────────

    private static readonly byte[] Png1x1 = Convert.FromBase64String(
        "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNkYPhfDwAChwGA60e6kgAAAABJRU5ErkJggg==");

    [Test]
    public void EmlImageAttachment_GetsStubLineAndNeedsOcrWarning()
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("A", "a@example.com"));
        var builder = new BodyBuilder { TextBody = "Invoice attached." };
        builder.Attachments.Add("invoice.jpg", Png1x1);
        message.Body = builder.ToMessageBody();
        using var ms = new MemoryStream();
        message.WriteTo(ms);

        var result = _extractor.GetText("mail.eml", ms.ToArray());

        Assert.That(result.Text, Does.Contain("[Attachment: invoice.jpg] not extracted: image (.png); text, if any, needs OCR"));
        Assert.That(result.Warnings.Single().Code, Is.EqualTo(ExtractionWarningCodes.NeedsOcr));
    }

    [Test]
    public void ZipUnsupportedEntry_GetsStubLine_WithDetectedType()
    {
        var zip = CreateZip(("tool.bin", CreatePdf("x")), ("a.txt", Encoding.UTF8.GetBytes("text")));

        var result = _extractor.GetText("a.zip", zip);

        Assert.That(result.Text, Does.Contain("[tool.bin] not extracted: no extractor for '.bin'; content is '.pdf'"));
        Assert.That(result.Warnings.Select(w => w.Code), Is.EquivalentTo(new[]
            { ExtractionWarningCodes.ContentTypeMismatch, ExtractionWarningCodes.ContainerUnsupportedEntry }));
    }

    [Test]
    public void PngNamedJpg_IsNotAMismatch()
    {
        var zip = CreateZip(("photo.jpg", Png1x1));

        var result = _extractor.GetText("a.zip", zip);

        Assert.That(result.Warnings.Select(w => w.Code), Is.EqualTo(new[] { ExtractionWarningCodes.NeedsOcr }));
    }

    [TestCase("photo.jpg")]
    [TestCase("photo.dat")]
    public void TopLevelImage_ReturnsNeedsOcr(string fileName)
    {
        var result = _extractor.GetText(fileName, Png1x1);

        Assert.That(result.ErrorKind, Is.EqualTo(ExtractionErrorKind.NeedsOcr));
    }

    [Test]
    public void TextStartingWithBM_IsNotTakenForBitmap()
        => Assert.That(TextExtractor.DetectFormat(Encoding.ASCII.GetBytes("BMW quarterly report, all figures")), Is.Null);

    [Test]
    public void ScannedPdfPage_GetsStubAndNeedsOcrWarning()
    {
        var builder = new PdfDocumentBuilder();
        var page = builder.AddPage(PageSize.A4);
        page.AddPng(Png1x1, new PdfRectangle(0, 0, 500, 700));
        var pdf = builder.Build();

        var result = _extractor.GetText("scan.pdf", pdf);

        Assert.That(result.IsSuccess, Is.True, result.ErrorMessage);
        Assert.That(result.Text, Does.Contain("[Page 1] not extracted: no text layer"));
        Assert.That(result.Warnings.Single().Code, Is.EqualTo(ExtractionWarningCodes.NeedsOcr));
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static byte[] CreateZip(params (string Name, byte[] Content)[] entries)
    {
        using var ms = new MemoryStream();
        using (var zip = new ZipArchive(ms, ZipArchiveMode.Create, leaveOpen: true))
        {
            foreach (var (name, content) in entries)
            {
                using var s = zip.CreateEntry(name).Open();
                s.Write(content);
            }
        }
        return ms.ToArray();
    }

    private static byte[] AddPart(byte[] package, string name, byte[] content)
    {
        using var ms = new MemoryStream();
        ms.Write(package);
        using (var zip = new ZipArchive(ms, ZipArchiveMode.Update, leaveOpen: true))
        {
            using var s = zip.CreateEntry(name).Open();
            s.Write(content);
        }
        return ms.ToArray();
    }

    private static byte[] CreateDocx(string text)
    {
        using var ms = new MemoryStream();
        using (var doc = WordprocessingDocument.Create(ms, DocumentFormat.OpenXml.WordprocessingDocumentType.Document))
        {
            var main = doc.AddMainDocumentPart();
            main.Document = new W.Document(new W.Body(new W.Paragraph(new W.Run(new W.Text(text)))));
        }
        return ms.ToArray();
    }

    private static byte[] CreatePdf(string text)
    {
        var builder = new PdfDocumentBuilder();
        var font = builder.AddStandard14Font(Standard14Font.Helvetica);
        var page = builder.AddPage(PageSize.A4);
        page.AddText(text, 12, new PdfPoint(25, 700), font);
        return builder.Build();
    }

    /// <summary>One slide: a top-level shape, a group containing a shape, and a 1x2 table.</summary>
    private static byte[] CreatePptx()
    {
        using var ms = new MemoryStream();
        using (var pres = PresentationDocument.Create(ms, DocumentFormat.OpenXml.PresentationDocumentType.Presentation))
        {
            var presPart = pres.AddPresentationPart();
            var slidePart = presPart.AddNewPart<SlidePart>();

            var tree = new P.ShapeTree();
            tree.AppendChild(new P.NonVisualGroupShapeProperties(
                new P.NonVisualDrawingProperties { Id = 1U, Name = "" },
                new P.NonVisualGroupShapeDrawingProperties(),
                new P.ApplicationNonVisualDrawingProperties()));
            tree.AppendChild(new P.GroupShapeProperties(new DA.TransformGroup()));
            tree.AppendChild(TextShape(2U, "Top level"));

            var group = new P.GroupShape(
                new P.NonVisualGroupShapeProperties(
                    new P.NonVisualDrawingProperties { Id = 3U, Name = "Group" },
                    new P.NonVisualGroupShapeDrawingProperties(),
                    new P.ApplicationNonVisualDrawingProperties()),
                new P.GroupShapeProperties(new DA.TransformGroup()));
            group.AppendChild(TextShape(4U, "Inside group"));
            tree.AppendChild(group);

            var table = new DA.Table(new DA.TableGrid(new DA.GridColumn { Width = 100 }, new DA.GridColumn { Width = 100 }),
                new DA.TableRow(TableCell("Cell A"), TableCell("Cell B")) { Height = 100 });
            tree.AppendChild(new P.GraphicFrame(
                new P.NonVisualGraphicFrameProperties(
                    new P.NonVisualDrawingProperties { Id = 5U, Name = "Table" },
                    new P.NonVisualGraphicFrameDrawingProperties(),
                    new P.ApplicationNonVisualDrawingProperties()),
                new P.Transform(),
                new DA.Graphic(new DA.GraphicData(table) { Uri = "http://schemas.openxmlformats.org/drawingml/2006/table" })));

            slidePart.Slide = new P.Slide(new P.CommonSlideData(tree));
            presPart.Presentation = new P.Presentation(
                new P.SlideIdList(new P.SlideId { Id = 256U, RelationshipId = presPart.GetIdOfPart(slidePart) }));
        }
        return ms.ToArray();
    }

    private static P.Shape TextShape(uint id, string text) => new(
        new P.NonVisualShapeProperties(
            new P.NonVisualDrawingProperties { Id = id, Name = $"Shape {id}" },
            new P.NonVisualShapeDrawingProperties(),
            new P.ApplicationNonVisualDrawingProperties()),
        new P.ShapeProperties(),
        new P.TextBody(new DA.BodyProperties(), new DA.ListStyle(),
            new DA.Paragraph(new DA.Run(new DA.Text(text)))));

    private static DA.TableCell TableCell(string text) => new(
        new DA.TextBody(new DA.BodyProperties(), new DA.ListStyle(),
            new DA.Paragraph(new DA.Run(new DA.Text(text)))),
        new DA.TableCellProperties());

    private sealed class ThrowingExtractor(Exception toThrow) : ITextExtractor
    {
        public bool CanHandle(string extension) => extension == ".throw";
        public string ExtractText(byte[] data) => throw toThrow;
    }

    private sealed class SpinUntilCancelledExtractor : ITextExtractor
    {
        public volatile bool StoppedByToken;

        public bool CanHandle(string extension) => extension == ".slow";

        public string ExtractText(byte[] data) => throw new InvalidOperationException("Options overload expected.");

        public string ExtractText(byte[] data, ExtractionOptions? options)
        {
            var deadline = DateTime.UtcNow.AddSeconds(30);
            while (DateTime.UtcNow < deadline)
            {
                if (options?.CancellationToken.IsCancellationRequested == true)
                {
                    StoppedByToken = true;
                    options.CancellationToken.ThrowIfCancellationRequested();
                }
                Thread.Sleep(1);
            }
            return "never cancelled";
        }
    }
}
