using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using NUnit.Framework;
using System.IO.Compression;
using System.Text;
using Transformations.Text;
using S  = DocumentFormat.OpenXml.Spreadsheet;
using W  = DocumentFormat.OpenXml.Wordprocessing;
using P  = DocumentFormat.OpenXml.Presentation;
using DA = DocumentFormat.OpenXml.Drawing;

namespace Transformations.Tests;

[TestFixture]
public class Round4Tests
{
    private TextExtractor _extractor = null!;

    [SetUp]
    public void Setup() => _extractor = new TextExtractor();

    // ── 11. TableMode ─────────────────────────────────────────────────────────

    [Test]
    public void DocxExtractor_TableMode_Markdown_ProducesPipeTable()
    {
        var bytes = CreateDocxWithTable(["Name", "Age"], ["Alice", "30"], ["Bob", "25"]);
        var result = new DocxExtractor().ExtractText(bytes, new ExtractionOptions { TableMode = TableMode.Markdown });

        Assert.That(result, Does.Contain("| Name | Age |"));
        Assert.That(result, Does.Contain("| --- | --- |"));
        Assert.That(result, Does.Contain("| Alice | 30 |"));
    }

    [Test]
    public void DocxExtractor_TableMode_Csv_ProducesCsvRows()
    {
        var bytes = CreateDocxWithTable(["Name", "Age"], ["Alice", "30"]);
        var result = new DocxExtractor().ExtractText(bytes, new ExtractionOptions { TableMode = TableMode.Csv });

        Assert.That(result, Does.Contain("Name,Age"));
        Assert.That(result, Does.Contain("Alice,30"));
    }

    [Test]
    public void DocxExtractor_TableMode_Omit_SkipsTableContent()
    {
        var bytes = CreateDocxWithTable(["Name", "Age"], ["Alice", "30"]);
        var result = new DocxExtractor().ExtractText(bytes, new ExtractionOptions { TableMode = TableMode.Omit });

        Assert.That(result, Does.Not.Contain("Alice"));
        Assert.That(result, Does.Not.Contain("Name"));
    }

    [Test]
    public void HtmlExtractor_TableMode_Markdown_ProducesPipeTable()
    {
        const string html = """
            <html><body>
            <table>
            <tr><th>Product</th><th>Price</th></tr>
            <tr><td>Widget</td><td>9.99</td></tr>
            </table>
            </body></html>
            """;
        var result = new HtmlExtractor().ExtractText(
            Encoding.UTF8.GetBytes(html),
            new ExtractionOptions { TableMode = TableMode.Markdown });

        Assert.That(result, Does.Contain("| Product | Price |"));
        Assert.That(result, Does.Contain("| Widget | 9.99 |"));
    }

    [Test]
    public void HtmlExtractor_TableMode_Csv_ProducesCsvRows()
    {
        const string html = """
            <html><body>
            <table>
            <tr><th>Item</th><th>Qty</th></tr>
            <tr><td>Apple</td><td>5</td></tr>
            </table>
            </body></html>
            """;
        var result = new HtmlExtractor().ExtractText(
            Encoding.UTF8.GetBytes(html),
            new ExtractionOptions { TableMode = TableMode.Csv });

        Assert.That(result, Does.Contain("Item,Qty"));
        Assert.That(result, Does.Contain("Apple,5"));
    }

    [Test]
    public void HtmlExtractor_TableMode_Omit_SkipsTable()
    {
        const string html = """
            <html><body>
            <p>Introduction</p>
            <table><tr><td>Secret</td></tr></table>
            <p>Conclusion</p>
            </body></html>
            """;
        var result = new HtmlExtractor().ExtractText(
            Encoding.UTF8.GetBytes(html),
            new ExtractionOptions { TableMode = TableMode.Omit });

        Assert.That(result, Does.Contain("Introduction"));
        Assert.That(result, Does.Not.Contain("Secret"));
        Assert.That(result, Does.Contain("Conclusion"));
    }

    [Test]
    public void ExcelExtractor_TableMode_Markdown_ProducesPipeTable()
    {
        var bytes = CreateXlsxWithData("Prices", ["Item", "Cost"], ["Widget", "9.99"]);
        var result = new ExcelExtractor().ExtractText(bytes, new ExtractionOptions { TableMode = TableMode.Markdown });

        Assert.That(result, Does.Contain("## Prices"));
        Assert.That(result, Does.Contain("| Item | Cost |"));
        Assert.That(result, Does.Contain("| Widget | 9.99 |"));
    }

    [Test]
    public void ExcelExtractor_TableMode_Omit_YieldsEmpty()
    {
        var bytes = CreateXlsxWithData("Sheet1", ["A", "B"], ["1", "2"]);
        var result = new ExcelExtractor().ExtractText(bytes, new ExtractionOptions { TableMode = TableMode.Omit });

        Assert.That(result.Trim(), Is.Empty);
    }

    [Test]
    public void OdtExtractor_TableMode_Markdown_ProducesPipeTable()
    {
        var bytes = CreateOdtWithTable(["X", "Y"], ["10", "20"]);
        var result = new OdtExtractor().ExtractText(bytes, new ExtractionOptions { TableMode = TableMode.Markdown });

        Assert.That(result, Does.Contain("| X | Y |"));
        Assert.That(result, Does.Contain("| --- | --- |"));
        Assert.That(result, Does.Contain("| 10 | 20 |"));
    }

    [Test]
    public void OdtExtractor_TableMode_Csv_ProducesCsvRows()
    {
        var bytes = CreateOdtWithTable(["X", "Y"], ["10", "20"]);
        var result = new OdtExtractor().ExtractText(bytes, new ExtractionOptions { TableMode = TableMode.Csv });

        Assert.That(result, Does.Contain("X,Y"));
        Assert.That(result, Does.Contain("10,20"));
    }

    // ── 12. ExtractionOptions: MaxPages / StartPage / EndPage ─────────────────

    [Test]
    public void PptxExtractor_MaxPages_LimitsSlides()
    {
        var bytes = CreateMultiSlidePptx("Alpha", "Beta", "Gamma");
        var result = new PptxExtractor().ExtractText(bytes, new ExtractionOptions { MaxPages = 2 });

        Assert.That(result, Does.Contain("Alpha"));
        Assert.That(result, Does.Contain("Beta"));
        Assert.That(result, Does.Not.Contain("Gamma"));
    }

    [Test]
    public void PptxExtractor_StartPage_SkipsEarlierSlides()
    {
        var bytes = CreateMultiSlidePptx("Alpha", "Beta", "Gamma");
        var result = new PptxExtractor().ExtractText(bytes, new ExtractionOptions { StartPage = 2 });

        Assert.That(result, Does.Not.Contain("Alpha"));
        Assert.That(result, Does.Contain("Beta"));
        Assert.That(result, Does.Contain("Gamma"));
    }

    [Test]
    public void PptxExtractor_EndPage_StopsAtPage()
    {
        var bytes = CreateMultiSlidePptx("Alpha", "Beta", "Gamma");
        var result = new PptxExtractor().ExtractText(bytes, new ExtractionOptions { EndPage = 2 });

        Assert.That(result, Does.Contain("Alpha"));
        Assert.That(result, Does.Contain("Beta"));
        Assert.That(result, Does.Not.Contain("Gamma"));
    }

    [Test]
    public void PptxExtractor_StartAndEndPage_ExtractsWindow()
    {
        var bytes = CreateMultiSlidePptx("Alpha", "Beta", "Gamma", "Delta");
        var result = new PptxExtractor().ExtractText(bytes, new ExtractionOptions { StartPage = 2, EndPage = 3 });

        Assert.That(result, Does.Not.Contain("Alpha"));
        Assert.That(result, Does.Contain("Beta"));
        Assert.That(result, Does.Contain("Gamma"));
        Assert.That(result, Does.Not.Contain("Delta"));
    }

    // ── 13. DetectFormat ──────────────────────────────────────────────────────

    [Test]
    public void DetectFormat_Pdf_ReturnsPdfExtension()
        => Assert.That(TextExtractor.DetectFormat(Encoding.ASCII.GetBytes("%PDF-1.4\n1 0 obj")),
            Is.EqualTo(".pdf"));

    [Test]
    public void DetectFormat_Rtf_ReturnsRtfExtension()
        => Assert.That(TextExtractor.DetectFormat(Encoding.ASCII.GetBytes(@"{\rtf1\ansi Hello\par}")),
            Is.EqualTo(".rtf"));

    [Test]
    public void DetectFormat_Html_ReturnsHtmlExtension()
        => Assert.That(TextExtractor.DetectFormat(Encoding.UTF8.GetBytes("<!DOCTYPE html><html><body>Hi</body></html>")),
            Is.EqualTo(".html"));

    [Test]
    public void DetectFormat_Eml_ReturnsEmlExtension()
        => Assert.That(TextExtractor.DetectFormat(Encoding.ASCII.GetBytes("MIME-Version: 1.0\r\nFrom: a@b.com\r\n\r\nBody")),
            Is.EqualTo(".eml"));

    [Test]
    public void DetectFormat_Docx_ReturnsDocxExtension()
        => Assert.That(TextExtractor.DetectFormat(CreateDocxWithTable(["H"], ["V"])),
            Is.EqualTo(".docx"));

    [Test]
    public void DetectFormat_Epub_ReturnsEpubExtension()
        => Assert.That(TextExtractor.DetectFormat(CreateEpub("Test chapter")),
            Is.EqualTo(".epub"));

    [Test]
    public void DetectFormat_Unknown_ReturnsNull()
        => Assert.That(TextExtractor.DetectFormat(new byte[] { 0xDE, 0xAD, 0xBE, 0xEF, 0x00, 0x01 }),
            Is.Null);

    // ── 14. TextExtractorBuilder ──────────────────────────────────────────────

    [Test]
    public void TextExtractorBuilder_Add_InsertsBeforeTxtExtractor()
    {
        var list = TextExtractor.CreateDefaultExtractors().ToList();
        var builder = new TextExtractorBuilder(list);
        builder.Add(new StubExtractor());
        var built = builder.Build().ToList();

        int stubIdx = built.FindIndex(e => e is StubExtractor);
        int txtIdx  = built.FindIndex(e => e is TxtExtractor);
        Assert.That(stubIdx, Is.GreaterThanOrEqualTo(0));
        Assert.That(stubIdx, Is.LessThan(txtIdx));
    }

    [Test]
    public void TextExtractorBuilder_AddGeneric_Works()
    {
        var list = TextExtractor.CreateDefaultExtractors().ToList();
        var builder = new TextExtractorBuilder(list);
        builder.Add<StubExtractor>();
        Assert.That(builder.Build(), Has.One.InstanceOf<StubExtractor>());
    }

    [Test]
    public void TextExtractorBuilder_Disable_RemovesExtractor()
    {
        var list = TextExtractor.CreateDefaultExtractors().ToList();
        var builder = new TextExtractorBuilder(list);
        builder.Disable<PdfExtractor>();
        Assert.That(builder.Build(), Has.None.InstanceOf<PdfExtractor>());
    }

    [Test]
    public void TextExtractorBuilder_Replace_SwapsExtractor()
    {
        var list = TextExtractor.CreateDefaultExtractors().ToList();
        var builder = new TextExtractorBuilder(list);
        builder.Replace<PdfExtractor>(new StubExtractor());
        var built = builder.Build().ToList();
        Assert.That(built, Has.None.InstanceOf<PdfExtractor>());
        Assert.That(built, Has.One.InstanceOf<StubExtractor>());
    }

    // ── 15. RtfExtractor ─────────────────────────────────────────────────────

    [Test]
    public void RtfExtractor_BasicText_ExtractsContent()
    {
        var rtf = @"{\rtf1\ansi\deff0{\fonttbl{\f0 Arial;}}\pard Hello World\par}";
        var result = new RtfExtractor().ExtractText(Encoding.ASCII.GetBytes(rtf));
        Assert.That(result, Does.Contain("Hello World"));
    }

    [Test]
    public void RtfExtractor_CanHandle_DotRtf()
    {
        var ext = new RtfExtractor();
        Assert.That(ext.CanHandle(".rtf"), Is.True);
        Assert.That(ext.CanHandle(".RTF"), Is.True);
        Assert.That(ext.CanHandle(".doc"), Is.False);
    }

    [Test]
    public void RtfExtractor_SkipsMetadataGroups()
    {
        var rtf = @"{\rtf1\ansi{\info{\title SecretTitle}}Hello\par}";
        var result = new RtfExtractor().ExtractText(Encoding.ASCII.GetBytes(rtf));
        Assert.That(result, Does.Contain("Hello"));
        Assert.That(result, Does.Not.Contain("SecretTitle"));
    }

    [Test]
    public void RtfExtractor_FacadeRoutes_DotRtf()
    {
        var rtf = @"{\rtf1\ansi\pard RtfContent\par}";
        var result = _extractor.GetText("doc.rtf", Encoding.ASCII.GetBytes(rtf));
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Text, Does.Contain("RtfContent"));
    }

    // ── 16. EpubExtractor ─────────────────────────────────────────────────────

    [Test]
    public void EpubExtractor_SingleChapter_ExtractsText()
    {
        var bytes = CreateEpub("Hello EPUB World");
        var result = new EpubExtractor().ExtractText(bytes);
        Assert.That(result, Does.Contain("Hello EPUB World"));
    }

    [Test]
    public void EpubExtractor_MultiChapter_MaxPages_Limits()
    {
        var bytes = CreateEpub("Chapter One", "Chapter Two", "Chapter Three");
        var result = new EpubExtractor().ExtractText(bytes, new ExtractionOptions { MaxPages = 2 });

        Assert.That(result, Does.Contain("Chapter One"));
        Assert.That(result, Does.Contain("Chapter Two"));
        Assert.That(result, Does.Not.Contain("Chapter Three"));
    }

    [Test]
    public void EpubExtractor_IncludePageMarkers_AddsChapterLabels()
    {
        var bytes = CreateEpub("First chapter text", "Second chapter text");
        var result = new EpubExtractor().ExtractText(bytes, new ExtractionOptions { IncludePageMarkers = true });

        Assert.That(result, Does.Contain("[Chapter 1]"));
        Assert.That(result, Does.Contain("[Chapter 2]"));
    }

    [Test]
    public void EpubExtractor_CanHandle_DotEpub()
    {
        var ext = new EpubExtractor();
        Assert.That(ext.CanHandle(".epub"), Is.True);
        Assert.That(ext.CanHandle(".EPUB"), Is.True);
        Assert.That(ext.CanHandle(".zip"),  Is.False);
    }

    [Test]
    public void EpubExtractor_FacadeRoutes_DotEpub()
    {
        var bytes = CreateEpub("FacadeRoutingTest");
        var result = _extractor.GetText("book.epub", bytes);
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Text, Does.Contain("FacadeRoutingTest"));
    }

    [Test]
    public void EpubExtractor_MissingChapter_AddsWarningAndKeepsReadableChapters()
    {
        var bytes = CreateEpubWithMissingChapter();
        var result = _extractor.GetText("book.epub", bytes);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Text, Does.Contain("Present chapter"));
        Assert.That(result.Warnings, Has.Count.EqualTo(1));
        Assert.That(result.Warnings[0].Code, Is.EqualTo(ExtractionWarningCodes.ContainerEntryMissing));
        Assert.That(result.Warnings[0].Source, Is.EqualTo("OEBPS/missing.xhtml"));
    }

    [Test]
    public void EpubExtractor_MissingManifest_AddsWarning()
    {
        var bytes = CreateEpubWithoutManifest();
        var result = _extractor.GetText("book.epub", bytes);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Text, Is.Empty);
        Assert.That(result.Warnings, Has.Count.EqualTo(1));
        Assert.That(result.Warnings[0].Code, Is.EqualTo(ExtractionWarningCodes.ContainerManifestMissing));
    }

    // ── 17. TextChunk provenance ──────────────────────────────────────────────

    [Test]
    public void ChunkBySections_HeadingPath_PopulatesHierarchy()
    {
        var sections = new[]
        {
            new MarkdownSection(1, "Chapter", "chapter", "Intro text"),
            new MarkdownSection(2, "Section", "section", "Section text"),
            new MarkdownSection(3, "Subsection", "subsection", "Sub text")
        };

        var chunks = TextChunker.ChunkBySections(sections);

        Assert.That(chunks[0].HeadingPath, Is.EqualTo(new[] { "Chapter" }));
        Assert.That(chunks[1].HeadingPath, Is.EqualTo(new[] { "Chapter", "Section" }));
        Assert.That(chunks[2].HeadingPath, Is.EqualTo(new[] { "Chapter", "Section", "Subsection" }));
    }

    [Test]
    public void ChunkBySections_HeadingPath_ResetsSiblingBranch()
    {
        var sections = new[]
        {
            new MarkdownSection(1, "A", "a", "TextA"),
            new MarkdownSection(2, "A1", "a1", "TextA1"),
            new MarkdownSection(1, "B", "b", "TextB")
        };

        var chunks = TextChunker.ChunkBySections(sections);

        // Third chunk is at the top level — no ancestors from the A branch
        Assert.That(chunks[2].HeadingPath, Is.EqualTo(new[] { "B" }));
    }

    [Test]
    public void TextChunk_PageNumber_IsNullableInt()
    {
        var chunk = new TextChunk(0, "text", 0, 4);
        Assert.That(chunk.PageNumber, Is.Null);

        var withPage = chunk with { PageNumber = 3 };
        Assert.That(withPage.PageNumber, Is.EqualTo(3));
    }

    // ── 18. GetSupportedExtensions includes new formats ───────────────────────

    [Test]
    public void GetSupportedExtensions_IncludesRtfAndEpub()
    {
        var extensions = _extractor.GetSupportedExtensions().ToList();
        Assert.That(extensions, Does.Contain(".rtf"));
        Assert.That(extensions, Does.Contain(".epub"));
    }

    // ── helpers ───────────────────────────────────────────────────────────────

    private sealed class StubExtractor : ITextExtractor
    {
        public bool CanHandle(string extension) => extension == ".stub";
        public string ExtractText(byte[] data) => "stub";
    }

    private static byte[] CreateDocxWithTable(string[] headers, params string[][] dataRows)
    {
        using var ms = new MemoryStream();
        using (var doc = WordprocessingDocument.Create(ms, WordprocessingDocumentType.Document))
        {
            var mainPart = doc.AddMainDocumentPart();
            var table = new W.Table();

            var headerRow = new W.TableRow();
            foreach (var h in headers)
                headerRow.AppendChild(new W.TableCell(new W.Paragraph(new W.Run(new W.Text(h)))));
            table.AppendChild(headerRow);

            foreach (var row in dataRows)
            {
                var tr = new W.TableRow();
                foreach (var v in row)
                    tr.AppendChild(new W.TableCell(new W.Paragraph(new W.Run(new W.Text(v)))));
                table.AppendChild(tr);
            }

            mainPart.Document = new W.Document(new W.Body(table));
        }
        return ms.ToArray();
    }

    private static byte[] CreateXlsxWithData(string sheetName, string[] headers, params string[][] dataRows)
    {
        using var ms = new MemoryStream();
        using (var spreadsheet = SpreadsheetDocument.Create(ms, SpreadsheetDocumentType.Workbook))
        {
            var wbPart = spreadsheet.AddWorkbookPart();
            wbPart.Workbook = new S.Workbook();

            var wsPart = wbPart.AddNewPart<WorksheetPart>();
            var sheetData = new S.SheetData();

            sheetData.AppendChild(MakeXlsxRow(headers));
            foreach (var row in dataRows)
                sheetData.AppendChild(MakeXlsxRow(row));

            wsPart.Worksheet = new S.Worksheet(sheetData);
            var sheets = wbPart.Workbook.AppendChild(new S.Sheets());
            sheets.AppendChild(new S.Sheet
            {
                Id = wbPart.GetIdOfPart(wsPart),
                SheetId = 1,
                Name = sheetName
            });
        }
        return ms.ToArray();
    }

    private static S.Row MakeXlsxRow(string[] values)
    {
        var row = new S.Row();
        foreach (var v in values)
            row.AppendChild(new S.Cell
            {
                DataType = S.CellValues.String,
                CellValue = new S.CellValue(v)
            });
        return row;
    }

    private static byte[] CreateOdtWithTable(string[] headers, params string[][] dataRows)
    {
        using var ms = new MemoryStream();
        using (var zip = new ZipArchive(ms, ZipArchiveMode.Create, leaveOpen: true))
        {
            var mimeEntry = zip.CreateEntry("mimetype", CompressionLevel.NoCompression);
            using (var w = new StreamWriter(mimeEntry.Open()))
                w.Write("application/vnd.oasis.opendocument.text");

            var xml = new StringBuilder();
            xml.Append("""
                <?xml version="1.0" encoding="UTF-8"?>
                <office:document-content
                    xmlns:office="urn:oasis:names:tc:opendocument:xmlns:office:1.0"
                    xmlns:text="urn:oasis:names:tc:opendocument:xmlns:text:1.0"
                    xmlns:table="urn:oasis:names:tc:opendocument:xmlns:table:1.0"
                    office:version="1.2">
                  <office:body><office:text><table:table table:name="T1">
                """);
            AppendOdtTableRow(xml, headers);
            foreach (var row in dataRows)
                AppendOdtTableRow(xml, row);
            xml.Append("</table:table></office:text></office:body></office:document-content>");

            var contentEntry = zip.CreateEntry("content.xml");
            using (var w = new StreamWriter(contentEntry.Open(), Encoding.UTF8))
                w.Write(xml.ToString());

            var manifestEntry = zip.CreateEntry("META-INF/manifest.xml");
            using (var w = new StreamWriter(manifestEntry.Open(), Encoding.UTF8))
                w.Write("""
                    <?xml version="1.0" encoding="UTF-8"?>
                    <manifest:manifest xmlns:manifest="urn:oasis:names:tc:opendocument:xmlns:manifest:1.0">
                      <manifest:file-entry manifest:full-path="/" manifest:media-type="application/vnd.oasis.opendocument.text"/>
                      <manifest:file-entry manifest:full-path="content.xml" manifest:media-type="text/xml"/>
                    </manifest:manifest>
                    """);
        }
        return ms.ToArray();
    }

    private static void AppendOdtTableRow(StringBuilder sb, string[] cells)
    {
        sb.Append("<table:table-row>");
        foreach (var c in cells)
            sb.Append($"<table:table-cell><text:p>{System.Security.SecurityElement.Escape(c)}</text:p></table:table-cell>");
        sb.Append("</table:table-row>");
    }

    private static byte[] CreateEpub(params string[] chapterContents)
    {
        using var ms = new MemoryStream();
        using (var zip = new ZipArchive(ms, ZipArchiveMode.Create, leaveOpen: true))
        {
            var mimeEntry = zip.CreateEntry("mimetype", CompressionLevel.NoCompression);
            using (var w = new StreamWriter(mimeEntry.Open()))
                w.Write("application/epub+zip");

            var containerEntry = zip.CreateEntry("META-INF/container.xml");
            using (var w = new StreamWriter(containerEntry.Open(), Encoding.UTF8))
                w.Write("""
                    <?xml version="1.0"?>
                    <container version="1.0" xmlns="urn:oasis:names:tc:opendocument:xmlns:container">
                      <rootfiles>
                        <rootfile full-path="OEBPS/content.opf" media-type="application/oebps-package+xml"/>
                      </rootfiles>
                    </container>
                    """);

            var manifest = new StringBuilder();
            var spine    = new StringBuilder();
            for (int i = 0; i < chapterContents.Length; i++)
            {
                manifest.AppendLine($"""  <item id="ch{i + 1}" href="chapter{i + 1}.xhtml" media-type="application/xhtml+xml"/>""");
                spine.AppendLine($"""  <itemref idref="ch{i + 1}"/>""");
            }

            var opfEntry = zip.CreateEntry("OEBPS/content.opf");
            using (var w = new StreamWriter(opfEntry.Open(), Encoding.UTF8))
                w.Write($"""
                    <?xml version="1.0"?>
                    <package xmlns="http://www.idpf.org/2007/opf" version="2.0">
                      <metadata xmlns:dc="http://purl.org/dc/elements/1.1/"><dc:title>Test</dc:title></metadata>
                      <manifest>
                    {manifest.ToString().TrimEnd()}
                      </manifest>
                      <spine>
                    {spine.ToString().TrimEnd()}
                      </spine>
                    </package>
                    """);

            for (int i = 0; i < chapterContents.Length; i++)
            {
                var chEntry = zip.CreateEntry($"OEBPS/chapter{i + 1}.xhtml");
                using (var w = new StreamWriter(chEntry.Open(), Encoding.UTF8))
                    w.Write($"""
                        <?xml version="1.0"?>
                        <html xmlns="http://www.w3.org/1999/xhtml">
                        <body><p>{chapterContents[i]}</p></body>
                        </html>
                        """);
            }
        }
        return ms.ToArray();
    }

    private static byte[] CreateEpubWithMissingChapter()
    {
        using var ms = new MemoryStream();
        using (var zip = new ZipArchive(ms, ZipArchiveMode.Create, leaveOpen: true))
        {
            var mimeEntry = zip.CreateEntry("mimetype", CompressionLevel.NoCompression);
            using (var w = new StreamWriter(mimeEntry.Open()))
                w.Write("application/epub+zip");

            var containerEntry = zip.CreateEntry("META-INF/container.xml");
            using (var w = new StreamWriter(containerEntry.Open(), Encoding.UTF8))
                w.Write("""
                    <?xml version="1.0"?>
                    <container version="1.0" xmlns="urn:oasis:names:tc:opendocument:xmlns:container">
                      <rootfiles>
                        <rootfile full-path="OEBPS/content.opf" media-type="application/oebps-package+xml"/>
                      </rootfiles>
                    </container>
                    """);

            var opfEntry = zip.CreateEntry("OEBPS/content.opf");
            using (var w = new StreamWriter(opfEntry.Open(), Encoding.UTF8))
                w.Write("""
                    <?xml version="1.0"?>
                    <package xmlns="http://www.idpf.org/2007/opf" version="2.0">
                      <metadata xmlns:dc="http://purl.org/dc/elements/1.1/"><dc:title>Test</dc:title></metadata>
                      <manifest>
                        <item id="ch1" href="chapter1.xhtml" media-type="application/xhtml+xml"/>
                        <item id="ch2" href="missing.xhtml" media-type="application/xhtml+xml"/>
                      </manifest>
                      <spine>
                        <itemref idref="ch1"/>
                        <itemref idref="ch2"/>
                      </spine>
                    </package>
                    """);

            var chapter = zip.CreateEntry("OEBPS/chapter1.xhtml");
            using (var w = new StreamWriter(chapter.Open(), Encoding.UTF8))
                w.Write("""
                    <?xml version="1.0"?>
                    <html xmlns="http://www.w3.org/1999/xhtml">
                    <body><p>Present chapter</p></body>
                    </html>
                    """);
        }

        return ms.ToArray();
    }

    private static byte[] CreateEpubWithoutManifest()
    {
        using var ms = new MemoryStream();
        using (var zip = new ZipArchive(ms, ZipArchiveMode.Create, leaveOpen: true))
        {
            var mimeEntry = zip.CreateEntry("mimetype", CompressionLevel.NoCompression);
            using var w = new StreamWriter(mimeEntry.Open());
            w.Write("application/epub+zip");
        }

        return ms.ToArray();
    }

    private static byte[] CreateMultiSlidePptx(params string[] titles)
    {
        using var ms = new MemoryStream();
        using (var pres = PresentationDocument.Create(ms, PresentationDocumentType.Presentation))
        {
            var presPart = pres.AddPresentationPart();

            var masterPart = presPart.AddNewPart<SlideMasterPart>();
            var masterTree = new P.ShapeTree();
            masterTree.AppendChild(new P.NonVisualGroupShapeProperties(
                new P.NonVisualDrawingProperties { Id = 1U, Name = "" },
                new P.NonVisualGroupShapeDrawingProperties(),
                new P.ApplicationNonVisualDrawingProperties()));
            masterTree.AppendChild(new P.GroupShapeProperties(new DA.TransformGroup()));
            masterPart.SlideMaster = new P.SlideMaster(
                new P.CommonSlideData(masterTree),
                new P.ColorMap
                {
                    Background1 = DA.ColorSchemeIndexValues.Light1,
                    Text1       = DA.ColorSchemeIndexValues.Dark1,
                    Background2 = DA.ColorSchemeIndexValues.Light2,
                    Text2       = DA.ColorSchemeIndexValues.Dark2,
                    Accent1     = DA.ColorSchemeIndexValues.Accent1,
                    Accent2     = DA.ColorSchemeIndexValues.Accent2,
                    Accent3     = DA.ColorSchemeIndexValues.Accent3,
                    Accent4     = DA.ColorSchemeIndexValues.Accent4,
                    Accent5     = DA.ColorSchemeIndexValues.Accent5,
                    Accent6     = DA.ColorSchemeIndexValues.Accent6,
                    Hyperlink        = DA.ColorSchemeIndexValues.Hyperlink,
                    FollowedHyperlink = DA.ColorSchemeIndexValues.FollowedHyperlink
                },
                new P.SlideLayoutIdList());

            var layoutPart = masterPart.AddNewPart<SlideLayoutPart>();
            var layoutTree = new P.ShapeTree();
            layoutTree.AppendChild(new P.NonVisualGroupShapeProperties(
                new P.NonVisualDrawingProperties { Id = 1U, Name = "" },
                new P.NonVisualGroupShapeDrawingProperties(),
                new P.ApplicationNonVisualDrawingProperties()));
            layoutTree.AppendChild(new P.GroupShapeProperties(new DA.TransformGroup()));
            layoutPart.SlideLayout = new P.SlideLayout(
                new P.CommonSlideData(layoutTree),
                new P.ColorMapOverride(new DA.MasterColorMapping()));

            string masterRelId = presPart.GetIdOfPart(masterPart);
            masterPart.SlideMaster.SlideLayoutIdList!.AppendChild(
                new P.SlideLayoutId { Id = 2049U, RelationshipId = masterPart.GetIdOfPart(layoutPart) });

            var slideIdList = new P.SlideIdList();
            uint slideIdVal = 256U;

            foreach (var title in titles)
            {
                var slidePart = presPart.AddNewPart<SlidePart>();
                string slideRelId = presPart.GetIdOfPart(slidePart);

                var shapeTree = new P.ShapeTree();
                shapeTree.AppendChild(new P.NonVisualGroupShapeProperties(
                    new P.NonVisualDrawingProperties { Id = 1U, Name = "" },
                    new P.NonVisualGroupShapeDrawingProperties(),
                    new P.ApplicationNonVisualDrawingProperties()));
                shapeTree.AppendChild(new P.GroupShapeProperties(new DA.TransformGroup()));
                shapeTree.AppendChild(BuildTitleShape(2U, title));

                slidePart.Slide = new P.Slide(
                    new P.CommonSlideData(shapeTree),
                    new P.ColorMapOverride(new DA.MasterColorMapping()));
                slidePart.AddPart(layoutPart);

                slideIdList.AppendChild(new P.SlideId { Id = slideIdVal++, RelationshipId = slideRelId });
            }

            presPart.Presentation = new P.Presentation(
                new P.SlideSize { Cx = 9144000, Cy = 5143500 },
                new P.NotesSize { Cx = 6858000, Cy = 9144000 },
                new P.SlideMasterIdList(new P.SlideMasterId { Id = 2147483648U, RelationshipId = masterRelId }),
                slideIdList);
        }
        return ms.ToArray();
    }

    private static P.Shape BuildTitleShape(uint id, string text)
    {
        return new P.Shape(
            new P.NonVisualShapeProperties(
                new P.NonVisualDrawingProperties { Id = id, Name = "Title" },
                new P.NonVisualShapeDrawingProperties(new DA.ShapeLocks { NoGrouping = true }),
                new P.ApplicationNonVisualDrawingProperties(
                    new P.PlaceholderShape { Type = P.PlaceholderValues.Title })),
            new P.ShapeProperties(),
            new P.TextBody(
                new DA.BodyProperties(),
                new DA.ListStyle(),
                new DA.Paragraph(new DA.Run(new DA.Text(text)))));
    }
}
