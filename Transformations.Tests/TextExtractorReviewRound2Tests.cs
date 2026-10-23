using System.IO.Compression;
using System.Text;
using DocumentFormat.OpenXml.Packaging;
using MimeKit;
using NUnit.Framework;
using Transformations.Text;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Core;
using UglyToad.PdfPig.Writer;
using W = DocumentFormat.OpenXml.Wordprocessing;

namespace Transformations.Tests;

/// <summary>
/// Regression tests for the external review findings in Transformations.TextExtractor/"to fix 1.md", "to fix 2.md"
/// and "to fix 3.txt". Each test names the finding it covers (file.number).
/// </summary>
[TestFixture]
public class TextExtractorReviewRound2Tests
{
    private static readonly byte[] Png1x1 = Convert.FromBase64String(
        "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNkYPhfDwAChwGA60e6kgAAAABJRU5ErkJggg==");

    private TextExtractor _extractor = null!;

    [SetUp]
    public void Setup() => _extractor = new TextExtractor();

    // ── 1.1 Metadata decodes the source only where it needs it ───────────────

    [Test]
    public void Metadata_MarkdownAndJsonStillReadSource_BinaryFormatsDoNot()
    {
        var md = _extractor.GetTextWithMetadata("a.md", Utf8("# Title\n\nBody"));
        var json = _extractor.GetTextWithMetadata("a.json", Utf8("{\"a\":1,\"b\":2}"));
        var pdf = _extractor.GetTextWithMetadata("a.pdf", CreatePdf("Hello"));

        Assert.That(md.Metadata["markdown.headingCount"], Is.EqualTo("1"));
        Assert.That(json.Metadata["json.topLevelPropertyCount"], Is.EqualTo("2"));
        Assert.That(pdf.Metadata["pdf.pageCount"], Is.EqualTo("1"));
    }

    // ── 1.2 Decompression budget counts failed entries ───────────────────────

    [Test]
    public void Zip_UnderDeclaredEntries_SpendTheBudgetOnce_ThenStop()
    {
        // Every entry really holds 4,000 bytes but its headers claim 10. The budget is 6,000 bytes: the second entry
        // passes it, and the extractor must stop there instead of spending the remaining budget again on each entry.
        var payload = new string('a', 4000);
        var zip = UnderDeclare(CreateZip(("1.txt", Utf8(payload)), ("2.txt", Utf8(payload)), ("3.txt", Utf8(payload)), ("4.txt", Utf8(payload))), 10);

        var result = _extractor.GetText("a.zip", zip, new ExtractionOptions { MaxDecompressedBytes = 6000 });

        var codes = result.Warnings.Select(w => w.Code).ToList();
        Assert.That(codes.Count(c => c == ExtractionWarningCodes.ContainerByteLimit), Is.EqualTo(1), string.Join(", ", codes));
        Assert.That(result.Warnings.Any(w => w.Source is "3.txt" or "4.txt"), Is.False);
    }

    [Test]
    public void Epub_ChapterBudget_CountsPackageDocumentsAndChapters()
    {
        var epub = CreateEpub([("chapter1.xhtml", new string('x', 3000)), ("chapter2.xhtml", new string('y', 3000))]);

        var result = _extractor.GetText("b.epub", epub, new ExtractionOptions { MaxDecompressedBytes = 4000 });

        Assert.That(result.Text, Does.Contain("xxx"));
        Assert.That(result.Text, Does.Not.Contain("yyy"));
        Assert.That(result.Warnings.Select(w => w.Code), Does.Contain(ExtractionWarningCodes.ContainerByteLimit));
    }

    // ── 1.3 / 3.3 Table headers and nested tables ────────────────────────────

    [Test]
    public void HtmlTable_TdFirstRow_IsData()
    {
        var result = _extractor.GetText("t.html", Utf8("<table><tr><td>Alice</td><td>30</td></tr><tr><td>Bob</td><td>40</td></tr></table>"));

        Assert.That(result.Text, Does.Contain("Alice | 30").And.Contain("Bob | 40"));
        Assert.That(result.Text, Does.Not.Contain("Alice: Bob"));
    }

    [TestCase("<table><tr><th>Name</th><th>Age</th></tr><tr><td>Bob</td><td>40</td></tr></table>")]
    [TestCase("<table><thead><tr><td>Name</td><td>Age</td></tr></thead><tbody><tr><td>Bob</td><td>40</td></tr></tbody></table>")]
    public void HtmlTable_MarkedHeader_LabelsCells(string html)
    {
        var result = _extractor.GetText("t.html", Utf8(html));

        Assert.That(result.Text, Does.Contain("Name: Bob | Age: 40"));
    }

    [TestCase(TableHeaderMode.FirstRow, "Alice: Bob")]
    [TestCase(TableHeaderMode.None, "Alice | 30")]
    public void TableHeaders_OptionOverridesMarkers(TableHeaderMode mode, string expected)
    {
        var result = _extractor.GetText("t.html", Utf8("<table><tr><td>Alice</td><td>30</td></tr><tr><td>Bob</td><td>40</td></tr></table>"),
            new ExtractionOptions { TableHeaders = mode });

        Assert.That(result.Text, Does.Contain(expected));
    }

    [Test]
    public void HtmlNestedTable_RowsStayInTheirOwnTable()
    {
        var html = "<table><tr><th>Item</th><th>Detail</th></tr>" +
                   "<tr><td>Box</td><td><table><tr><td>inner1</td></tr><tr><td>inner2</td></tr></table></td></tr></table>";

        var result = _extractor.GetText("t.html", Utf8(html));

        Assert.That(result.Text, Does.Contain("Item: Box"));
        Assert.That(result.Text.Split("inner1").Length - 1, Is.EqualTo(1), result.Text);
        Assert.That(result.Text, Does.Not.Contain("Item: inner1"));
    }

    [Test]
    public void DocxTable_Unmarked_IsData_TblHeaderIsHeader()
    {
        var plain = _extractor.GetText("a.docx", CreateDocxTable(markHeader: false));
        var marked = _extractor.GetText("b.docx", CreateDocxTable(markHeader: true));

        Assert.That(plain.Text, Does.Contain("Alice | 30").And.Contain("Bob | 40"));
        Assert.That(marked.Text, Does.Contain("Alice: Bob | 30: 40"));
    }

    [Test]
    public void OdtTable_HeaderRows_AreReadAndUsedAsLabels()
    {
        var odt = CreateOdt("""
            <table:table><table:table-header-rows><table:table-row>
              <table:table-cell><text:p>Name</text:p></table:table-cell><table:table-cell><text:p>Age</text:p></table:table-cell>
            </table:table-row></table:table-header-rows>
            <table:table-row><table:table-cell><text:p>Bob</text:p></table:table-cell><table:table-cell><text:p>40</text:p></table:table-cell></table:table-row>
            </table:table>
            """, presentation: false);

        var result = _extractor.GetText("t.odt", odt);

        Assert.That(result.Text, Does.Contain("Name: Bob | Age: 40"));
    }

    // ── 1.4 PDF image size is checked before conversion ──────────────────────

    [Test]
    public void ScannedPdf_ImageOverPixelLimit_IsRefusedBeforeDecoding()
    {
        var builder = new PdfDocumentBuilder();
        builder.AddPage(PageSize.A4).AddPng(Png1x1, new PdfRectangle(0, 0, 500, 700));
        var engine = new CountingOcr();

        var result = _extractor.GetText("scan.pdf", builder.Build(), new ExtractionOptions { OcrEngine = engine, MaxImagePixels = 0 });

        Assert.That(result.ErrorKind, Is.EqualTo(ExtractionErrorKind.LimitExceeded));
        Assert.That(result.ErrorMessage, Does.Contain("page 1").And.Contain("1x1"));
        Assert.That(engine.Calls, Is.Zero);
    }

    // ── 1.5 Stream reads honour the options token and input limit ────────────

    [Test]
    public void StreamRead_OptionsToken_Cancels()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        var options = new ExtractionOptions { CancellationToken = cts.Token };

        Assert.Throws<OperationCanceledException>(() => _extractor.GetText("a.txt", new MemoryStream(Utf8("hello")), options));
        Assert.That(() => _extractor.GetTextAsync("a.txt", new MemoryStream(Utf8("hello")), options).GetAwaiter().GetResult(),
            Throws.InstanceOf<OperationCanceledException>());
    }

    [Test]
    public void HtmlExtractFromStream_EnforcesMaxInputBytes()
    {
        var html = new NonSeekableStream(Utf8("<p>" + new string('a', 5000) + "</p>"));

        Assert.Throws<ExtractionLimitExceededException>(() =>
            new HtmlExtractor().ExtractFromStream(html, new ExtractionOptions { MaxInputBytes = 1000 }));
    }

    // ── 1.6 EPUB hrefs are resolved as URLs ──────────────────────────────────

    [TestCase("text/Chapter%20One.xhtml", "OEBPS/text/Chapter One.xhtml")]
    [TestCase("text/ch1.xhtml#start", "OEBPS/text/ch1.xhtml")]
    [TestCase("../shared/./ch1.xhtml?v=2", "shared/ch1.xhtml")]
    public void Epub_HrefVariants_AreFound(string href, string entryPath)
    {
        var epub = CreateEpub([(href, "Found chapter")], entryPaths: [entryPath]);

        var result = _extractor.GetText("b.epub", epub);

        Assert.That(result.Text, Does.Contain("Found chapter"));
        Assert.That(result.Warnings.Select(w => w.Code), Does.Not.Contain(ExtractionWarningCodes.ContainerEntryMissing));
    }

    // ── 1.7 Chunk offsets point at the chunk's own text ──────────────────────

    [TestCase("  First para.\n\n   Second para here.\n\n\tThird.")]
    [TestCase("   One sentence. Two sentence!   Three?  ")]
    public void ChunkOffsets_MatchOriginalText(string text)
    {
        var chunks = TextChunker.ChunkByParagraphs(text, 12)
            .Concat(TextChunker.ChunkBySentences(text, 12))
            .Concat(TextChunker.ChunkByTokens(text, 3, s => s.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length));

        foreach (var chunk in chunks)
            Assert.That(text[chunk.StartOffset..chunk.EndOffset], Is.EqualTo(chunk.Text));
    }

    [Test]
    public void ChunkByParagraphs_KeepsOriginalSeparators()
    {
        var chunks = TextChunker.ChunkByParagraphs("A.\n\nB.", 100);

        Assert.That(chunks.Single().Text, Is.EqualTo("A.\n\nB."));
    }

    // ── 2.1 A parse that overruns the timeout is not reported as success ─────

    [Test]
    public void UninterruptibleParser_PastTimeout_IsTimedOut()
    {
        var extractor = new TextExtractor([new SlowExtractor()]);

        var result = extractor.GetText("a.slow", Utf8("x"), new ExtractionOptions { Timeout = TimeSpan.FromMilliseconds(20) });

        Assert.That(result.ErrorKind, Is.EqualTo(ExtractionErrorKind.TimedOut));
    }

    // ── 2.2 One header-like line is not an email ─────────────────────────────

    [TestCase("notes.txt", "Date: tomorrow\nBring snacks.")]
    [TestCase("notes", "From: the desk of the editor\n\nDear readers")]
    public void HeaderLikeText_IsNotEmail(string name, string text)
    {
        var result = _extractor.GetText(name, Utf8(text));

        Assert.That(result.ExtractorName, Is.EqualTo(nameof(TxtExtractor)));
    }

    [Test]
    public void RealHeaders_WithoutExtension_AreEmail()
    {
        var result = _extractor.GetText("message", Utf8("From: a@example.com\r\nTo: b@example.com\r\nSubject: Hi\r\n\r\nBody text"));

        Assert.That(result.ExtractorName, Is.EqualTo(nameof(EmlExtractor)));
        Assert.That(result.Text, Does.Contain("Body text"));
    }

    // ── 2.3 Markdown structure ignores code fences; anchors are unique ───────

    [Test]
    public void MarkdownHeadingsInCodeFences_AreNotHeadings()
    {
        var md = "# Real\n\n```markdown\n# Not a heading\nSetext\n---\n```\n\n~~~\n## Also not\n~~~\n\n## Second";

        var headings = MarkdownStructureExtractor.BuildHeadingMap(md);
        var sections = MarkdownStructureExtractor.ExtractSections(md);

        Assert.That(headings.Select(h => h.Title), Is.EqualTo(new[] { "Real", "Second" }));
        Assert.That(sections.Single(s => s.Title == "Real").Content, Does.Contain("# Not a heading"));
    }

    [Test]
    public void MarkdownRepeatedTitles_GetUniqueAnchors()
    {
        var headings = MarkdownStructureExtractor.BuildHeadingMap("# Setup\n\n## Setup\n\n### Setup");

        Assert.That(headings.Select(h => h.Anchor), Is.EqualTo(new[] { "setup", "setup-1", "setup-2" }));
    }

    // ── 2.4 Direct stream APIs keep the package guard ────────────────────────

    [Test]
    public void DirectStreamApis_EnforcePackageEntryLimit()
    {
        var many = CreateZip(Enumerable.Range(0, 10_001).Select(i => ($"p{i}.xml", new byte[] { 1 })).ToArray());

        Assert.Throws<ExtractionLimitExceededException>(() => new DocxExtractor().ExtractText(new MemoryStream(many)));
        Assert.Throws<ExtractionLimitExceededException>(() => new PptxExtractor().ExtractText(new MemoryStream(many)));
        Assert.Throws<ExtractionLimitExceededException>(() => new OdtExtractor().ExtractText(new MemoryStream(many)));
    }

    // ── 2.5 Discovery, ODP slide selection, DOCX page breaks ─────────────────

    [Test]
    public void SupportedExtensions_IncludeImages()
    {
        Assert.That(_extractor.GetSupportedExtensions(), Does.Contain(".png").And.Contain(".jpg").And.Contain(".tiff"));
    }

    [Test]
    public void Odp_PageOptions_SelectSlides()
    {
        var slides = string.Concat(new[] { "One", "Two", "Three", "Four" }.Select(t =>
            $"<draw:page><draw:frame><draw:text-box><text:p>{t}</text:p></draw:text-box></draw:frame></draw:page>"));
        var odp = CreateOdt(slides, presentation: true);

        var result = _extractor.GetText("s.odp", odp, new ExtractionOptions { StartPage = 2, MaxPages = 2 });

        Assert.That(result.Text, Does.Contain("Two").And.Contain("Three"));
        Assert.That(result.Text, Does.Not.Contain("One").And.Not.Contain("Four"));
    }

    [Test]
    public void DocxPageBreakMidParagraph_SplitsTextAtTheBreak()
    {
        using var ms = new MemoryStream();
        using (var doc = WordprocessingDocument.Create(ms, DocumentFormat.OpenXml.WordprocessingDocumentType.Document))
        {
            doc.AddMainDocumentPart().Document = new W.Document(new W.Body(new W.Paragraph(
                new W.Run(new W.Text("end of page one")),
                new W.Run(new W.Break { Type = W.BreakValues.Page }, new W.Text("start of page two")))));
        }

        var result = _extractor.GetText("p.docx", ms.ToArray(), new ExtractionOptions { IncludePageMarkers = true });

        var nl = Environment.NewLine;
        Assert.That(result.Text, Is.EqualTo($"[Page 1]{nl}end of page one{nl}[Page 2]{nl}start of page two"));
    }

    // ── 2.6 Whitespace plain-text part falls back to HTML ────────────────────

    [Test]
    public void Eml_WhitespaceTextBody_UsesHtmlBody()
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("A", "a@example.com"));
        message.Subject = "S";
        message.Body = new BodyBuilder { TextBody = "  \r\n ", HtmlBody = "<p>Real content</p>" }.ToMessageBody();
        using var ms = new MemoryStream();
        message.WriteTo(ms);

        var result = _extractor.GetText("m.eml", ms.ToArray());

        Assert.That(result.Text, Does.Contain("Real content"));
    }

    // ── 2.7 OLE subtypes ─────────────────────────────────────────────────────

    [TestCase("letter.doc", "__substg1.0_", ".msg")]
    [TestCase("mail.msg", "WordDocument", ".doc")]
    public void OleMsgVersusOffice_IsMismatch(string name, string stream, string detected)
    {
        var result = _extractor.GetText(name, FakeOle(stream));

        Assert.That(result.Warnings.Any(w => w.Code == ExtractionWarningCodes.ContentTypeMismatch && w.Message.Contains(detected)), Is.True,
            string.Join("; ", result.Warnings.Select(w => w.Message)));
    }

    [Test]
    public void OleOfficeSiblings_AreNotMismatch()
    {
        // Office files embed each other as OLE objects, so a Word stream in a .xls is not evidence of disguise.
        var result = _extractor.GetText("book.xls", FakeOle("WordDocument"));

        Assert.That(result.Warnings.Any(w => w.Code == ExtractionWarningCodes.ContentTypeMismatch), Is.False);
    }

    // ── 3.1 CSV repeated headers ─────────────────────────────────────────────

    [Test]
    public void Csv_RepeatedHeaders_KeepEveryColumn()
    {
        var result = _extractor.GetText("a.csv", Utf8("Name,Name\nAlice,Bob\n"));

        Assert.That(result.Text, Does.Contain("Name: Alice").And.Contain("Name: Bob"));
    }

    // ── 3.2 RTF code pages ───────────────────────────────────────────────────

    [TestCase(@"{\rtf1\ansi\ansicpg1252 Price \'80 5\par}", "Price € 5")]
    [TestCase(@"{\rtf1\ansi\ansicpg1251 \'cf\'f0\'e8\'e2\'e5\'f2\par}", "Привет")]
    [TestCase(@"{\rtf1\ansi\ansicpg1252\deff0{\fonttbl{\f0\fnil\fcharset128 MS Gothic;}}\f0 \'82\'a0\par}", "あ")]
    [TestCase(@"{\rtf1\ansi\ansicpg1252{\fonttbl{\f0\froman\fcharset0 Times;}{\f1\fnil\fcharset161 Greek;}}\f0 A \f1\'e1\par}", "A α")]
    public void Rtf_HexEscapes_UseTheActiveCodePage(string rtf, string expected)
    {
        var result = _extractor.GetText("a.rtf", Encoding.ASCII.GetBytes(rtf));

        Assert.That(result.Text, Is.EqualTo(expected));
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static byte[] Utf8(string s) => Encoding.UTF8.GetBytes(s);

    private static byte[] CreatePdf(string text)
    {
        var builder = new PdfDocumentBuilder();
        var font = builder.AddStandard14Font(UglyToad.PdfPig.Fonts.Standard14Fonts.Standard14Font.Helvetica);
        builder.AddPage(PageSize.A4).AddText(text, 12, new PdfPoint(25, 700), font);
        return builder.Build();
    }

    private static byte[] CreateZip(params (string Name, byte[] Content)[] entries)
    {
        using var ms = new MemoryStream();
        using (var zip = new ZipArchive(ms, ZipArchiveMode.Create, leaveOpen: true))
        {
            foreach (var (name, content) in entries)
            {
                using var s = zip.CreateEntry(name, CompressionLevel.NoCompression).Open();
                s.Write(content);
            }
        }
        return ms.ToArray();
    }

    /// <summary>Rewrites every local and central header so each entry claims <paramref name="declared"/> uncompressed bytes.</summary>
    private static byte[] UnderDeclare(byte[] zip, int declared)
    {
        var data = (byte[])zip.Clone();
        for (int i = 0; i + 4 < data.Length; i++)
        {
            if (data[i] != 0x50 || data[i + 1] != 0x4B) continue;
            if (data[i + 2] == 0x03 && data[i + 3] == 0x04)
                BitConverter.GetBytes(declared).CopyTo(data, i + 22);
            else if (data[i + 2] == 0x01 && data[i + 3] == 0x02)
                BitConverter.GetBytes(declared).CopyTo(data, i + 24);
        }
        return data;
    }

    private static byte[] CreateDocxTable(bool markHeader)
    {
        using var ms = new MemoryStream();
        using (var doc = WordprocessingDocument.Create(ms, DocumentFormat.OpenXml.WordprocessingDocumentType.Document))
        {
            W.TableRow Row(string a, string b) => new(
                new W.TableCell(new W.Paragraph(new W.Run(new W.Text(a)))),
                new W.TableCell(new W.Paragraph(new W.Run(new W.Text(b)))));
            var first = Row("Alice", "30");
            if (markHeader)
                first.PrependChild(new W.TableRowProperties(new W.TableHeader()));
            doc.AddMainDocumentPart().Document = new W.Document(new W.Body(new W.Table(first, Row("Bob", "40"))));
        }
        return ms.ToArray();
    }

    private static byte[] CreateOdt(string bodyXml, bool presentation)
    {
        var kind = presentation ? "presentation" : "text";
        var content = $"""
            <?xml version="1.0" encoding="UTF-8"?>
            <office:document-content xmlns:office="urn:oasis:names:tc:opendocument:xmlns:office:1.0"
                xmlns:text="urn:oasis:names:tc:opendocument:xmlns:text:1.0"
                xmlns:table="urn:oasis:names:tc:opendocument:xmlns:table:1.0"
                xmlns:draw="urn:oasis:names:tc:opendocument:xmlns:drawing:1.0" office:version="1.2">
              <office:body><office:{kind}>{bodyXml}</office:{kind}></office:body>
            </office:document-content>
            """;
        return CreateZip(("mimetype", Utf8($"application/vnd.oasis.opendocument.{kind}")), ("content.xml", Utf8(content)));
    }

    private static byte[] CreateEpub((string Href, string Text)[] chapters, string[]? entryPaths = null)
    {
        var manifest = string.Concat(chapters.Select((c, i) => $"<item id=\"c{i}\" href=\"{c.Href}\" media-type=\"application/xhtml+xml\"/>"));
        var spine = string.Concat(chapters.Select((_, i) => $"<itemref idref=\"c{i}\"/>"));
        var entries = new List<(string, byte[])>
        {
            ("mimetype", Utf8("application/epub+zip")),
            ("META-INF/container.xml", Utf8("<?xml version=\"1.0\"?><container version=\"1.0\" xmlns=\"urn:oasis:names:tc:opendocument:xmlns:container\"><rootfiles><rootfile full-path=\"OEBPS/content.opf\" media-type=\"application/oebps-package+xml\"/></rootfiles></container>")),
            ("OEBPS/content.opf", Utf8($"<?xml version=\"1.0\"?><package xmlns=\"http://www.idpf.org/2007/opf\" version=\"2.0\"><manifest>{manifest}</manifest><spine>{spine}</spine></package>")),
        };
        for (int i = 0; i < chapters.Length; i++)
            entries.Add((entryPaths?[i] ?? $"OEBPS/{chapters[i].Href}",
                Utf8($"<?xml version=\"1.0\"?><html xmlns=\"http://www.w3.org/1999/xhtml\"><body><p>{chapters[i].Text}</p></body></html>")));
        return CreateZip(entries.ToArray());
    }

    /// <summary>An OLE2 signature followed by one UTF-16 stream name: enough for the sniffer, not a parseable file.</summary>
    private static byte[] FakeOle(string streamName)
        => [0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1, .. new byte[504], .. Encoding.Unicode.GetBytes(streamName), .. new byte[64]];

    private sealed class CountingOcr : IOcrEngine
    {
        public int Calls;
        public bool IsAvailable => true;
        public OcrResult Recognize(byte[] image, CancellationToken cancellationToken)
        {
            Interlocked.Increment(ref Calls);
            return new OcrResult("text");
        }
    }

    /// <summary>A parser that cannot be interrupted: it ignores the token and overruns any short timeout.</summary>
    private sealed class SlowExtractor : ITextExtractor
    {
        public bool CanHandle(string extension) => extension == ".slow";
        public string ExtractText(byte[] data)
        {
            Thread.Sleep(200);
            return "done";
        }
    }

    private sealed class NonSeekableStream(byte[] data) : MemoryStream(data)
    {
        public override bool CanSeek => false;
    }
}
