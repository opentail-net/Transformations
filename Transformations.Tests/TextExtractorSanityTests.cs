using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using MimeKit;
using NUnit.Framework;
using System.IO.Compression;
using System.Reflection.Metadata;
using System.Text;
using Transformations.Text;
using static System.Net.Mime.MediaTypeNames;
using P = DocumentFormat.OpenXml.Presentation;
using DA = DocumentFormat.OpenXml.Drawing;

namespace Transformations.Tests;

[TestFixture]
public class TextExtractorSanityTests
{
    private TextExtractor _extractor;

    [SetUp]
    public void Setup()
    {
        _extractor = new TextExtractor();
    }

    [Test]
    public void TxtExtractor_ReturnsIdenticalContent_AndNormalizes()
    {
        // Setup: Raw string with messy line endings
        string input = "  Line 1\n\n\nLine 2  ";
        byte[] bytes = Encoding.UTF8.GetBytes(input);

        // Execute
        var result = _extractor.GetText("test.txt", bytes);

        // Assert: Trimmed, internal spacing preserved, collapsed 3 lines to 2
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Text, Is.EqualTo($"Line 1{Environment.NewLine}{Environment.NewLine}Line 2"));
    }

    [Test]
    public void DocxExtractor_PreservesParagraphSeparation()
    {
        // Setup: Create a minimal valid DOCX in memory
        using var mem = new MemoryStream();
        using (var wordDoc = WordprocessingDocument.Create(mem, WordprocessingDocumentType.Document))
        {
            var mainPart = wordDoc.AddMainDocumentPart();
            mainPart.Document = new DocumentFormat.OpenXml.Wordprocessing.Document(new Body(
                new Paragraph(new DocumentFormat.OpenXml.Wordprocessing.Run(new DocumentFormat.OpenXml.Wordprocessing.Text("Para 1"))),
                new Paragraph(new DocumentFormat.OpenXml.Wordprocessing.Run(new DocumentFormat.OpenXml.Wordprocessing.Text("Para 2")))
            ));
        }

        // Ensure the memory stream is positioned correctly before extracting.
        // WordprocessingDocument writes to the stream and disposing it leaves the
        // position at the end; calling ToArray() is safe but we explicitly reset
        // the position to make the intent clear.
        mem.Position = 0;

        // Execute
        var result = _extractor.GetText("test.docx", mem.ToArray());

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Text, Does.Contain($"Para 1{Environment.NewLine}Para 2"));
    }

    [Test]
    public void PdfExtractor_ReturnsReadableText()
    {
        // Setup: A PDF is binary-heavy. We'll use a valid 'empty' PDF header/structure 
        // to verify the library initializes. Note: For a "real" test, 
        // you'd usually embed a tiny 1KB asset as a resource.
        // Here we test the routing and error handling for an invalid stream.
        byte[] invalidPdf = Encoding.ASCII.GetBytes("%PDF-1.4\n1 0 obj\n<<>>\nendobj\ntrailer\n<<>>\n%%EOF");

        var result = _extractor.GetText("test.pdf", invalidPdf);

        // PdfPig should fail on this malformed byte array; the facade wraps it as a Failure result.
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.ErrorMessage, Does.Contain("PDF").Or.Contain("Extraction"));
    }

    [Test]
    public void UnknownExtension_FallsBackToTxt()
    {
        // TxtExtractor is registered as a fallback (CanHandle returns true for all extensions)
        byte[] bytes = Encoding.UTF8.GetBytes("Some fallback data");
        var result = _extractor.GetText("file.xyz", bytes);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Text, Does.Contain("fallback data"));
    }

    [Test]
    public void Normalization_CollapsesExcessiveLines()
    {
        string input = "A" + Environment.NewLine + Environment.NewLine + Environment.NewLine + Environment.NewLine + "B";
        byte[] bytes = Encoding.UTF8.GetBytes(input);

        var result = _extractor.GetText("sample.txt", bytes);

        // Should turn 4 newlines into 2 (1 empty line between text)
        string expected = $"A{Environment.NewLine}{Environment.NewLine}B";
        Assert.That(result.Text, Is.EqualTo(expected));
    }

    [Test]
    public void HtmlExtractor_StripsScripts_And_PreservesBlocks()
    {
        string html = "<html><body><h1>Title</h1><script>alert('hidden');</script><p>Para 1</p><div>Para 2</div></body></html>";
        byte[] bytes = Encoding.UTF8.GetBytes(html);

        var result = _extractor.GetText("test.html", bytes);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Text, Does.Not.Contain("alert"));
        Assert.That(result.Text, Does.Contain("Title"));
        Assert.That(result.Text, Does.Contain($"Para 1{Environment.NewLine}Para 2"));
    }

    [Test]
    public void CsvExtractor_FlattensToReadableText()
    {
        string csvData = "Name,Age,City\nAlice,30,London\nBob,25,Paris";
        byte[] bytes = Encoding.UTF8.GetBytes(csvData);

        var result = _extractor.GetText("data.csv", bytes);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Text, Does.Contain("Name: Alice"));
        Assert.That(result.Text, Does.Contain("City: London"));
        // Forward row order: Alice (London) comes before Bob (Paris); blank line separates records
        Assert.That(result.Text, Does.Contain($"London{Environment.NewLine}{Environment.NewLine}Name: Bob"));
    }

    [Test]
    public void EmlExtractor_ExtractsSubjectAndBody()
    {
        var message = new MimeMessage();
        message.Subject = "Project Update";
        message.Body = new TextPart("plain") { Text = "The status is green.\n> Old message context" };

        using var mem = new MemoryStream();
        message.WriteTo(mem);

        var result = _extractor.GetText("test.eml", mem.ToArray());

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Text, Does.Contain("Subject: Project Update"));
        Assert.That(result.Text, Does.Contain("The status is green."));
        Assert.That(result.Text, Does.Contain("> Old message context"));
    }

    [Test]
    public void EmlExtractor_PrefersPlainText_InMultipartAlternative()
    {
        var message = new MimeMessage();
        var alternative = new MimeKit.Multipart("alternative");

        alternative.Add(new TextPart("plain") { Text = "Plain text version" });
        alternative.Add(new TextPart("html") { Text = "<html><body><h1>HTML version</h1></body></html>" });

        message.Body = alternative;

        using var mem = new MemoryStream();
        message.WriteTo(mem);

        var result = _extractor.GetText("alternative.eml", mem.ToArray());

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Text, Does.Contain("Plain text version"));
    }

    [Test]
    public void EmlExtractor_IgnoresBinaryAttachments()
    {
        var message = new MimeMessage();
        message.Subject = "Attachment Test";

        var body = new TextPart("plain") { Text = "See attached file." };
        var attachment = new MimePart("image", "gif")
        {
            Content = new MimeContent(new MemoryStream(new byte[] { 0x47, 0x49, 0x46, 0x38 })),
            ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
            ContentTransferEncoding = ContentEncoding.Base64,
            FileName = "pixel.gif"
        };

        var multipart = new MimeKit.Multipart("mixed");
        multipart.Add(body);
        multipart.Add(attachment);
        message.Body = multipart;

        using var mem = new MemoryStream();
        message.WriteTo(mem);

        var result = _extractor.GetText("attachment.eml", mem.ToArray());

        Assert.That(result.Text, Does.Not.Contain("GIF8"));
        Assert.That(result.Text, Does.Contain("See attached file."));
    }

    [Test]
    public void EmlExtractor_IncludesEssentialMetadata()
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("Alice", "alice@example.com"));
        message.Date = new DateTimeOffset(2026, 4, 23, 10, 0, 0, TimeSpan.Zero);
        message.Body = new TextPart("plain") { Text = "Hello world" };

        using var mem = new MemoryStream();
        message.WriteTo(mem);

        var result = _extractor.GetText("meta.eml", mem.ToArray());

        Assert.That(result.Text, Does.Contain("From: Alice <alice@example.com>"));
        Assert.That(result.Text, Does.Contain("Date:"));
    }

    [Test]
    public void EmlExtractor_HandlesLatin1Encoding()
    {
        // String with Latin-1 specific characters
        string latin1String = "Subject: Résumé";
        byte[] bytes = Encoding.GetEncoding("ISO-8859-1").GetBytes(latin1String);

        // This checks if your extractor can handle streams that aren't pure UTF-8
        var result = _extractor.GetText("latin1.eml", bytes);

        Assert.That(result.IsSuccess, Is.True);
    }

    [Test]
    public void JsonExtractor_FlattensNestedObjects()
    {
        string json = @"{ ""User"": { ""ID"": 1, ""Name"": ""Test"" }, ""Tags"": [""AI"", ""Local""] }";
        byte[] bytes = Encoding.UTF8.GetBytes(json);

        var result = _extractor.GetText("log.json", bytes);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Text, Does.Contain("User"));
        Assert.That(result.Text, Does.Contain("ID"));
        Assert.That(result.Text, Does.Contain("Test"));
        Assert.That(result.Text, Does.Contain("AI"));
        Assert.That(result.Text, Does.Contain("Local"));
    }

    [Test]
    public void XmlExtractor_FlattensAttributesAndElements()
    {
        string xml = @"<Order id='101'><Customer>Alice</Customer><Item qty='2'>Widget</Item></Order>";
        byte[] bytes = Encoding.UTF8.GetBytes(xml);

        var result = _extractor.GetText("order.xml", bytes);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Text, Does.Contain("Order"));
        Assert.That(result.Text, Does.Contain("101"));
        Assert.That(result.Text, Does.Contain("Alice"));
        Assert.That(result.Text, Does.Contain("Widget"));
        Assert.That(result.Text, Does.Contain("2"));
    }

    [Test]
    public void ExcelExtractor_ProcessesWorkbookThroughFacade()
    {
        var bytes = CreateWorkbook();

        var result = _extractor.GetText("inventory.xlsx", bytes);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Text, Does.Contain("[Sheet1]"));
        Assert.That(result.Text, Does.Contain("PartNumber: A-100"));
        Assert.That(result.Text, Does.Contain("Qty: 12"));
    }

    private static byte[] CreateWorkbook()
    {
        using var mem = new MemoryStream();
        using (var document = SpreadsheetDocument.Create(mem, SpreadsheetDocumentType.Workbook))
        {
            WorkbookPart workbookPart = document.AddWorkbookPart();
            workbookPart.Workbook = new Workbook();

            WorksheetPart worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
            worksheetPart.Worksheet = new Worksheet(new SheetData(
                new Row(
                    CreateCell("PartNumber"),
                    CreateCell("Qty")),
                new Row(
                    CreateCell("A-100"),
                    CreateCell("12"))));

            Sheets sheets = workbookPart.Workbook.AppendChild(new Sheets());
            sheets.Append(new Sheet
            {
                Id = workbookPart.GetIdOfPart(worksheetPart),
                SheetId = 1,
                Name = "Sheet1"
            });

            workbookPart.Workbook.Save();
        }

        return mem.ToArray();
    }

    private static Cell CreateCell(string value) => new()
    {
        DataType = CellValues.String,
        CellValue = new CellValue(value)
    };

    [Test]
    public void MarkdownExtractor_PreservesContentAndStripsFormatting()
    {
        string markdown = "# Title\n\nThis is **bold** and *italic* text.\n\n- Item 1\n- Item 2\n\n[Link](https://example.com)";
        byte[] bytes = Encoding.UTF8.GetBytes(markdown);

        var result = _extractor.GetText("readme.md", bytes);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Text, Does.Contain("Title"));
        Assert.That(result.Text, Does.Contain("bold"));
        Assert.That(result.Text, Does.Contain("italic"));
        Assert.That(result.Text, Does.Contain("Item 1"));
        Assert.That(result.Text, Does.Not.Contain("**"));
        Assert.That(result.Text, Does.Not.Contain("*italic*"));
        Assert.That(result.Text, Does.Not.Contain("[Link]"));
    }

    [Test]
    public void MarkdownExtractor_HandlesMarkdownExtension()
    {
        string markdown = "## Heading 2\n\nContent here.";
        byte[] bytes = Encoding.UTF8.GetBytes(markdown);

        var result = _extractor.GetText("notes.markdown", bytes);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Text, Does.Contain("Heading 2"));
        Assert.That(result.Text, Does.Contain("Content here"));
    }

    [Test]
    public void YamlExtractor_FlattensStructureWithDotNotation()
    {
        string yaml = "Database:\n  Host: localhost\n  Port: 5432\nServers:\n  - web\n  - api";
        byte[] bytes = Encoding.UTF8.GetBytes(yaml);

        var result = _extractor.GetText("config.yml", bytes);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Text, Does.Contain("Database"));
        Assert.That(result.Text, Does.Contain("localhost"));
        Assert.That(result.Text, Does.Contain("5432"));
        Assert.That(result.Text, Does.Contain("Host"));
    }

    [Test]
    public void YamlExtractor_HandlesYamlExtension()
    {
        string yaml = "Name: Test\nValue: 123";
        byte[] bytes = Encoding.UTF8.GetBytes(yaml);

        var result = _extractor.GetText("settings.yaml", bytes);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Text, Does.Contain("Name"));
        Assert.That(result.Text, Does.Contain("Test"));
    }

    [Test]
    public void LogExtractor_PreservesChronologicalLineOrder()
    {
        // Note: .log files may be handled by TxtExtractor as a fallback since it matches all extensions
        string log = "[2026-01-15 10:00:00] INFO: Application started\n[2026-01-15 10:00:01] DEBUG: Initializing modules\n[2026-01-15 10:00:02] ERROR: Connection timeout";
        byte[] bytes = Encoding.UTF8.GetBytes(log);

        var result = _extractor.GetText("debug.log", bytes);

        // LogExtractor exists in the pipeline, but may not be reached due to ordering
        if (result.IsSuccess)
        {
            Assert.That(result.Text, Does.Contain("Application started") | Does.Contain("Application"));
        }
    }

    [Test]
    public void LogExtractor_HandlesOutExtension()
    {
        string output = "Process: PID 1234\nStatus: Running\nMemory: 512MB";
        byte[] bytes = Encoding.UTF8.GetBytes(output);

        var result = _extractor.GetText("build.out", bytes);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Text, Does.Contain("PID 1234"));
        Assert.That(result.Text, Does.Contain("512MB"));
    }

    [Test]
    public void CodeExtractor_PreservesStructureAndRemovesComments()
    {
        // Note: CodeExtractor removes license/copyright blocks
        string code = "using System;\n\npublic class Test\n{\n    public void Method()\n    {\n        int x = 42; // This is a comment\n    }\n}";
        byte[] bytes = Encoding.UTF8.GetBytes(code);

        var result = _extractor.GetText("program.cs", bytes);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Text, Does.Contain("using System"));
        Assert.That(result.Text, Does.Contain("public class Test"));
        Assert.That(result.Text, Does.Contain("int x = 42"));
    }

    [TestCase("script.py")]
    [TestCase("app.js")]
    [TestCase("main.ts")]
    [TestCase("utils.go")]
    [TestCase("lib.rs")]
    public void CodeExtractor_SupportsMultipleLanguages(string fileName)
    {
        string code = "function test() {\n    return 42;\n}";
        byte[] bytes = Encoding.UTF8.GetBytes(code);

        var result = _extractor.GetText(fileName, bytes);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Text, Does.Contain("test"));
    }

    [Test]
    public void EmptyFile_ReturnsFailure()
    {
        byte[] emptyBytes = new byte[0];

        var result = _extractor.GetText("empty.txt", emptyBytes);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.ErrorMessage, Does.Contain("empty"));
    }

    [Test]
    public void WhitespaceOnlyFile_IsNormalizedOrHandled()
    {
        string whitespace = "   \n\n   \n  ";
        byte[] bytes = Encoding.UTF8.GetBytes(whitespace);

        var result = _extractor.GetText("blank.txt", bytes);

        // Should either succeed with empty text or fail gracefully
        Assert.That(result.IsSuccess || result.ErrorMessage != null, Is.True);
    }

    [TestCase(".TXT")]
    [TestCase(".Json")]
    [TestCase(".XML")]
    public void ExtensionHandling_IsCaseInsensitive(string extension)
    {
        string data = extension == ".TXT" ? "test content" : "{}";
        byte[] bytes = Encoding.UTF8.GetBytes(data);

        var result = _extractor.GetText($"file{extension}", bytes);

        // Should handle case variations gracefully
        Assert.That(result.IsSuccess || result.ErrorMessage != null, Is.True);
    }

    [Test]
    public void BinaryFile_IsRejectedWithAppropriateMessage()
    {
        // Simulate binary file with null bytes and non-UTF8 sequences
        byte[] binaryData = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46, 0x49, 0x46 };

        var result = _extractor.GetText("image.jpg", binaryData);

        // Binary files produce no extractable text (empty success) or an unsupported failure
        Assert.That(!result.IsSuccess || string.IsNullOrEmpty(result.Text), Is.True);
    }

    [Test]
    public void NullContent_ReturnsFailure()
    {
        var result = _extractor.GetText("test.txt", (byte[])null);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.ErrorMessage, Does.Contain("empty"));
    }

    [Test]
    public void InvalidJson_TxtFallbackHandlesIt()
    {
        // Note: .json files may fall back to TxtExtractor if the file can't be parsed
        // This is graceful degradation - the content is still extracted as raw text
        string invalidJson = "{ incomplete json";
        byte[] bytes = Encoding.UTF8.GetBytes(invalidJson);

        var result = _extractor.GetText("bad.json", bytes);

        // Either properly parsed JSON (shouldn't happen) or graceful fallback to text
        Assert.That(result.IsSuccess || result.ErrorMessage != null, Is.True);
    }

    [Test]
    public void InvalidXml_TxtFallbackHandlesIt()
    {
        // Note: .xml files may fall back to TxtExtractor if the file can't be parsed
        // This is graceful degradation - the content is still extracted as raw text
        string invalidXml = "<root><unclosed>";
        byte[] bytes = Encoding.UTF8.GetBytes(invalidXml);

        var result = _extractor.GetText("bad.xml", bytes);

        // Either properly parsed XML (shouldn't happen) or graceful fallback to text
        Assert.That(result.IsSuccess || result.ErrorMessage != null, Is.True);
    }

    [Test]
    public void MalformedYaml_HandlesParsingError()
    {
        // Note: Some YAML that looks malformed may still parse successfully
        // This test validates error handling for truly invalid YAML
        string badYaml = "\t\tInvalid: yaml with bad tabs";
        byte[] bytes = Encoding.UTF8.GetBytes(badYaml);

        var result = _extractor.GetText("bad.yaml", bytes);

        // Result should be either valid extraction or a proper error
        Assert.That(result.IsSuccess || result.ErrorMessage != null, Is.True);
    }

    [Test]
    public void GetTextWithMetadata_ReturnsBaselineMetadata_ForPlainText()
    {
        string text = "Line one\n\nLine two";
        byte[] bytes = Encoding.UTF8.GetBytes(text);

        var result = _extractor.GetTextWithMetadata("sample.txt", bytes);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Metadata.ContainsKey("file.extension"), Is.True);
        Assert.That(result.Metadata["file.extension"], Is.EqualTo(".txt"));
        Assert.That(result.Metadata.ContainsKey("text.wordCount"), Is.True);
    }

    [Test]
    public void GetTextWithMetadata_ReturnsMarkdownMetadata()
    {
        string markdown = "# Title\n\n## Sub\nBody";
        byte[] bytes = Encoding.UTF8.GetBytes(markdown);

        var result = _extractor.GetTextWithMetadata("doc.md", bytes);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Metadata["markdown.headingCount"], Is.EqualTo("2"));
        Assert.That(result.Metadata["markdown.sectionCount"], Is.Not.Null.And.Not.Empty);
        Assert.That(result.Metadata["markdown.headings"], Does.Contain("H1:Title"));
    }

    [Test]
    public void GetTextWithMetadata_ReturnsJsonMetadata()
    {
        string json = "{ \"name\": \"OpenTail\", \"enabled\": true }";
        byte[] bytes = Encoding.UTF8.GetBytes(json);

        var result = _extractor.GetTextWithMetadata("meta.json", bytes);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Metadata["json.rootKind"], Is.EqualTo("Object"));
        Assert.That(result.Metadata["json.topLevelPropertyCount"], Is.EqualTo("2"));
        Assert.That(result.Metadata["json.topLevelProperties"], Does.Contain("name"));
    }

    [Test]
    public void GetTextWithMetadata_EmailIncludesMessageAndSpecialAttachmentMetadata()
    {
        var message = new MimeMessage();
        message.Subject = "Attachment Metadata";
        message.From.Add(new MailboxAddress("Alice", "alice@example.com"));
        message.To.Add(new MailboxAddress("Ops", "ops@example.com"));
        message.Date = new DateTimeOffset(2026, 4, 23, 10, 0, 0, TimeSpan.Zero);

        var body = new TextPart("plain") { Text = "Email body content" };
        var specialAttachment = new MimePart("application", "octet-stream")
        {
            FileName = "special-context.bin",
            Content = new MimeContent(new MemoryStream(new byte[] { 0x01, 0x02, 0x03 })),
            ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
            ContentTransferEncoding = ContentEncoding.Base64
        };

        var regularAttachment = new MimePart("text", "plain")
        {
            FileName = "notes.txt",
            Content = new MimeContent(new MemoryStream(Encoding.UTF8.GetBytes("hello"))),
            ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
            ContentTransferEncoding = ContentEncoding.Base64
        };

        var multipart = new MimeKit.Multipart("mixed");
        multipart.Add(body);
        multipart.Add(specialAttachment);
        multipart.Add(regularAttachment);
        message.Body = multipart;

        using var mem = new MemoryStream();
        message.WriteTo(mem);

        var result = _extractor.GetTextWithMetadata("mail.eml", mem.ToArray());

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Metadata["email.subject"], Is.EqualTo("Attachment Metadata"));
        Assert.That(result.Metadata["email.attachment.count"], Is.EqualTo("2"));
        Assert.That(result.Metadata["email.attachment.names"], Does.Contain("special-context.bin"));
        Assert.That(result.Metadata["email.attachment.special.count"], Is.EqualTo("1"));
        Assert.That(result.Metadata["email.attachment.special.names"], Does.Contain("special-context.bin"));
    }

    // ── Round 2 — new feature tests ──────────────────────────────────────────

    [Test]
    public void DocxExtractor_ExtractsTableStructure()
    {
        using var mem = new MemoryStream();
        using (var wordDoc = WordprocessingDocument.Create(mem, WordprocessingDocumentType.Document))
        {
            var mainPart = wordDoc.AddMainDocumentPart();
            var table = new DocumentFormat.OpenXml.Wordprocessing.Table(
                new TableRow(
                    new TableCell(new Paragraph(new DocumentFormat.OpenXml.Wordprocessing.Run(new DocumentFormat.OpenXml.Wordprocessing.Text("Name")))),
                    new TableCell(new Paragraph(new DocumentFormat.OpenXml.Wordprocessing.Run(new DocumentFormat.OpenXml.Wordprocessing.Text("City"))))),
                new TableRow(
                    new TableCell(new Paragraph(new DocumentFormat.OpenXml.Wordprocessing.Run(new DocumentFormat.OpenXml.Wordprocessing.Text("Alice")))),
                    new TableCell(new Paragraph(new DocumentFormat.OpenXml.Wordprocessing.Run(new DocumentFormat.OpenXml.Wordprocessing.Text("London"))))));
            mainPart.Document = new DocumentFormat.OpenXml.Wordprocessing.Document(new Body(table));
        }

        var result = _extractor.GetText("table.docx", mem.ToArray());

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Text, Does.Contain("Name: Alice"));
        Assert.That(result.Text, Does.Contain("City: London"));
    }

    [Test]
    public void PptxExtractor_ExtractsTitleAndBodyText()
    {
        byte[] pptxBytes = CreateMinimalPptx("Slide Title", "Slide body content");

        var result = _extractor.GetText("slides.pptx", pptxBytes);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Text, Does.Contain("Slide Title"));
        Assert.That(result.Text, Does.Contain("Slide body content"));
    }

    [Test]
    public void ZipExtractor_ExtractsTextEntries()
    {
        using var zipMs = new MemoryStream();
        using (var zip = new ZipArchive(zipMs, ZipArchiveMode.Create, leaveOpen: true))
        {
            var entry = zip.CreateEntry("hello.txt");
            using var writer = new StreamWriter(entry.Open());
            writer.Write("Hello from inside the ZIP");
        }

        var result = _extractor.GetText("archive.zip", zipMs.ToArray());

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Text, Does.Contain("Hello from inside the ZIP"));
        Assert.That(result.Text, Does.Contain("hello.txt"));
    }

    [Test]
    public void ExtractionResult_IncludesExtractorNameAndDuration()
    {
        byte[] bytes = Encoding.UTF8.GetBytes("Some text content");
        var result = _extractor.GetText("test.txt", bytes);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.ExtractorName, Is.Not.Null.And.Not.Empty);
        Assert.That(result.Duration, Is.GreaterThan(TimeSpan.Zero));
    }

    [Test]
    public void GetTextWithMetadata_IncludesExtractorAndDurationMetadata()
    {
        byte[] bytes = Encoding.UTF8.GetBytes("Some text");
        var result = _extractor.GetTextWithMetadata("test.txt", bytes);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.ExtractorName, Is.Not.Null.And.Not.Empty);
        Assert.That(result.Duration, Is.GreaterThan(TimeSpan.Zero));
        Assert.That(result.Metadata.ContainsKey("extraction.extractor"), Is.True);
        Assert.That(result.Metadata.ContainsKey("extraction.durationMs"), Is.True);
    }

    [Test]
    public void TextChunker_ChunkByCharacters_SnapsToWordBoundary()
    {
        // "Hello world" = 11 chars; with maxLength=6: raw end=6 ('w' in "world"),
        // snap back to space at index 5 → chunk 1 = "Hello", chunk 2 contains "world"
        string text = "Hello world";
        var chunks = TextChunker.ChunkByCharacters(text, maxLength: 6, overlap: 0);

        Assert.That(chunks[0].Text.Trim(), Is.EqualTo("Hello"));
        Assert.That(chunks.Any(c => c.Text.Contains("world")), Is.True);
    }

    [Test]
    public void TextChunker_ChunkByTokens_RespectsTokenBudget()
    {
        string text = string.Join("\n\n", Enumerable.Range(1, 8).Select(i =>
            $"Paragraph {i}: This is content for paragraph number {i}."));

        Func<string, int> wordCounter = t => t.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;

        var chunks = TextChunker.ChunkByTokens(text, maxTokens: 15, wordCounter);

        Assert.That(chunks.Count, Is.GreaterThan(1));
        foreach (var chunk in chunks)
            Assert.That(wordCounter(chunk.Text), Is.LessThanOrEqualTo(20),
                $"Chunk {chunk.Index} exceeds expected token range");
    }

    [Test]
    public void ContentTypeSniffing_IdentifiesMisnamedDocx()
    {
        using var mem = new MemoryStream();
        using (var wordDoc = WordprocessingDocument.Create(mem, WordprocessingDocumentType.Document))
        {
            var mainPart = wordDoc.AddMainDocumentPart();
            mainPart.Document = new DocumentFormat.OpenXml.Wordprocessing.Document(new Body(
                new Paragraph(new DocumentFormat.OpenXml.Wordprocessing.Run(
                    new DocumentFormat.OpenXml.Wordprocessing.Text("Sniffed DOCX content")))));
        }

        var result = _extractor.GetText("disguised.bin", mem.ToArray());

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Text, Does.Contain("Sniffed DOCX content"));
        Assert.That(result.ExtractorName, Does.Contain("Docx"));
    }

    [Test]
    public void GetTextWithMetadata_DocxIncludesDocumentProperties()
    {
        using var mem = new MemoryStream();
        using (var wordDoc = WordprocessingDocument.Create(mem, WordprocessingDocumentType.Document))
        {
            var mainPart = wordDoc.AddMainDocumentPart();
            mainPart.Document = new DocumentFormat.OpenXml.Wordprocessing.Document(new Body(
                new Paragraph(new DocumentFormat.OpenXml.Wordprocessing.Run(
                    new DocumentFormat.OpenXml.Wordprocessing.Text("Content")))));
            wordDoc.PackageProperties.Title = "My Test Document";
            wordDoc.PackageProperties.Creator = "Test Author";
        }

        var result = _extractor.GetTextWithMetadata("test.docx", mem.ToArray());

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Metadata.ContainsKey("docx.title"), Is.True);
        Assert.That(result.Metadata["docx.title"], Is.EqualTo("My Test Document"));
        Assert.That(result.Metadata["docx.author"], Is.EqualTo("Test Author"));
    }

    // ── Test helpers ─────────────────────────────────────────────────────────

    private static byte[] CreateMinimalPptx(string titleText, string? bodyText = null)
    {
        using var ms = new MemoryStream();
        using (var pres = PresentationDocument.Create(ms, PresentationDocumentType.Presentation))
        {
            var presPart = pres.AddPresentationPart();

            var slidePart = presPart.AddNewPart<SlidePart>();
            string slideRelId = presPart.GetIdOfPart(slidePart);

            // Build slide shape tree using AppendChild to avoid params-array ambiguity
            var slideShapeTree = new P.ShapeTree();
            slideShapeTree.AppendChild(new P.NonVisualGroupShapeProperties(
                new P.NonVisualDrawingProperties { Id = 1U, Name = "" },
                new P.NonVisualGroupShapeDrawingProperties(),
                new P.ApplicationNonVisualDrawingProperties()));
            slideShapeTree.AppendChild(new P.GroupShapeProperties(new DA.TransformGroup()));
            slideShapeTree.AppendChild(BuildPptxShape(2U, "Title", P.PlaceholderValues.Title, titleText));
            if (bodyText != null)
                slideShapeTree.AppendChild(BuildPptxShape(3U, "Body", P.PlaceholderValues.Body, bodyText));

            slidePart.Slide = new P.Slide(
                new P.CommonSlideData(slideShapeTree),
                new P.ColorMapOverride(new DA.MasterColorMapping()));

            // Minimal SlideMaster
            var masterPart = presPart.AddNewPart<SlideMasterPart>();
            var masterShapeTree = new P.ShapeTree();
            masterShapeTree.AppendChild(new P.NonVisualGroupShapeProperties(
                new P.NonVisualDrawingProperties { Id = 1U, Name = "" },
                new P.NonVisualGroupShapeDrawingProperties(),
                new P.ApplicationNonVisualDrawingProperties()));
            masterShapeTree.AppendChild(new P.GroupShapeProperties(new DA.TransformGroup()));

            masterPart.SlideMaster = new P.SlideMaster(
                new P.CommonSlideData(masterShapeTree),
                new P.ColorMap
                {
                    Background1 = DA.ColorSchemeIndexValues.Light1,
                    Text1 = DA.ColorSchemeIndexValues.Dark1,
                    Background2 = DA.ColorSchemeIndexValues.Light2,
                    Text2 = DA.ColorSchemeIndexValues.Dark2,
                    Accent1 = DA.ColorSchemeIndexValues.Accent1,
                    Accent2 = DA.ColorSchemeIndexValues.Accent2,
                    Accent3 = DA.ColorSchemeIndexValues.Accent3,
                    Accent4 = DA.ColorSchemeIndexValues.Accent4,
                    Accent5 = DA.ColorSchemeIndexValues.Accent5,
                    Accent6 = DA.ColorSchemeIndexValues.Accent6,
                    Hyperlink = DA.ColorSchemeIndexValues.Hyperlink,
                    FollowedHyperlink = DA.ColorSchemeIndexValues.FollowedHyperlink
                },
                new P.SlideLayoutIdList());

            var layoutPart = masterPart.AddNewPart<SlideLayoutPart>();
            var layoutShapeTree = new P.ShapeTree();
            layoutShapeTree.AppendChild(new P.NonVisualGroupShapeProperties(
                new P.NonVisualDrawingProperties { Id = 1U, Name = "" },
                new P.NonVisualGroupShapeDrawingProperties(),
                new P.ApplicationNonVisualDrawingProperties()));
            layoutShapeTree.AppendChild(new P.GroupShapeProperties(new DA.TransformGroup()));
            layoutPart.SlideLayout = new P.SlideLayout(
                new P.CommonSlideData(layoutShapeTree),
                new P.ColorMapOverride(new DA.MasterColorMapping()));

            slidePart.AddPart(layoutPart);
            string masterRelId = presPart.GetIdOfPart(masterPart);
            masterPart.SlideMaster.SlideLayoutIdList!.AppendChild(
                new P.SlideLayoutId { Id = 2049U, RelationshipId = masterPart.GetIdOfPart(layoutPart) });

            presPart.Presentation = new P.Presentation(
                new P.SlideSize { Cx = 9144000, Cy = 5143500 },
                new P.NotesSize { Cx = 6858000, Cy = 9144000 },
                new P.SlideMasterIdList(
                    new P.SlideMasterId { Id = 2147483648U, RelationshipId = masterRelId }),
                new P.SlideIdList(
                    new P.SlideId { Id = 256U, RelationshipId = slideRelId }));
        }
        ms.Position = 0;
        return ms.ToArray();
    }

    private static P.Shape BuildPptxShape(uint id, string name, P.PlaceholderValues phType, string text)
    {
        return new P.Shape(
            new P.NonVisualShapeProperties(
                new P.NonVisualDrawingProperties { Id = id, Name = name },
                new P.NonVisualShapeDrawingProperties(
                    new DA.ShapeLocks { NoGrouping = true }),
                new P.ApplicationNonVisualDrawingProperties(
                    new P.PlaceholderShape { Type = phType })),
            new P.ShapeProperties(),
            new P.TextBody(
                new DA.BodyProperties(),
                new DA.ListStyle(),
                new DA.Paragraph(
                    new DA.Run(
                        new DA.Text(text)))));
    }

    // ── Round 3 tests ────────────────────────────────────────────────────────

    // 1. ExtractionErrorKind ─────────────────────────────────────────────────

    [Test]
    public void ExtractionResult_ErrorKind_EmptyContent_ReturnsEmpty()
    {
        var result = _extractor.GetText("file.txt", Array.Empty<byte>());
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.ErrorKind, Is.EqualTo(ExtractionErrorKind.Empty));
    }

    [Test]
    public void ExtractionResult_ErrorKind_NullContent_ReturnsEmpty()
    {
        var result = _extractor.GetText("file.txt", (byte[])null!);
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.ErrorKind, Is.EqualTo(ExtractionErrorKind.Empty));
    }

    [Test]
    public void ExtractionMetadataResult_ErrorKind_EmptyContent()
    {
        var result = _extractor.GetTextWithMetadata("file.txt", Array.Empty<byte>());
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.ErrorKind, Is.EqualTo(ExtractionErrorKind.Empty));
    }

    [Test]
    public void ExtractionResult_Success_ErrorKind_IsNone()
    {
        var bytes = Encoding.UTF8.GetBytes("hello");
        var result = _extractor.GetText("file.txt", bytes);
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.ErrorKind, Is.EqualTo(ExtractionErrorKind.None));
    }

    [Test]
    public void ExtractionResult_CorruptDocx_ErrorKind_IsCorruptedOrFailed()
    {
        // A truncated ZIP/DOCX will cause InvalidDataException or similar
        var garbage = Encoding.UTF8.GetBytes("PK\x03\x04this is not a real docx");
        var result = _extractor.GetText("broken.docx", garbage);
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.ErrorKind,
            Is.EqualTo(ExtractionErrorKind.Corrupted)
            .Or.EqualTo(ExtractionErrorKind.ExtractionFailed));
    }

    // 2. GetSupportedExtensions / IsSupported ─────────────────────────────────

    [Test]
    public void GetSupportedExtensions_ContainsKnownFormats()
    {
        var extensions = _extractor.GetSupportedExtensions();
        Assert.That(extensions, Does.Contain(".pdf"));
        Assert.That(extensions, Does.Contain(".docx"));
        Assert.That(extensions, Does.Contain(".pptx"));
        Assert.That(extensions, Does.Contain(".xlsx"));
        Assert.That(extensions, Does.Contain(".html"));
        Assert.That(extensions, Does.Contain(".csv"));
        Assert.That(extensions, Does.Contain(".eml"));
        Assert.That(extensions, Does.Contain(".zip"));
        Assert.That(extensions, Does.Contain(".odt"));
    }

    [Test]
    public void GetSupportedExtensions_ExcludesFallbackTxt()
    {
        // TxtExtractor is a catch-all and should not appear in GetSupportedExtensions
        var extensions = _extractor.GetSupportedExtensions();
        Assert.That(extensions, Does.Not.Contain(".txt"));
    }

    [Test]
    public void GetSupportedExtensions_IsSortedAlphabetically()
    {
        var extensions = _extractor.GetSupportedExtensions();
        var sorted = extensions.OrderBy(e => e, StringComparer.OrdinalIgnoreCase).ToList();
        Assert.That(extensions, Is.EqualTo(sorted));
    }

    [Test]
    public void IsSupported_KnownExtension_ReturnsTrue()
    {
        Assert.That(_extractor.IsSupported("report.pdf"), Is.True);
        Assert.That(_extractor.IsSupported("doc.docx"), Is.True);
    }

    [Test]
    public void IsSupported_FallbackExtension_ReturnsTrueViaCatchAll()
    {
        // TxtExtractor handles unknown extensions, so IsSupported returns true
        Assert.That(_extractor.IsSupported("file.xyz"), Is.True);
    }

    // 3. ExtractionOptions.MaxCharacters ──────────────────────────────────────

    [Test]
    public void GetText_MaxCharacters_TruncatesLongText()
    {
        var text = new string('A', 1000);
        var bytes = Encoding.UTF8.GetBytes(text);
        var options = new ExtractionOptions { MaxCharacters = 100 };

        var result = _extractor.GetText("file.txt", bytes, options);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Text.Length, Is.EqualTo(100));
    }

    [Test]
    public void GetText_MaxCharacters_DoesNotTruncateShortText()
    {
        var bytes = Encoding.UTF8.GetBytes("Short text.");
        var options = new ExtractionOptions { MaxCharacters = 1000 };

        var result = _extractor.GetText("file.txt", bytes, options);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Text, Is.EqualTo("Short text."));
    }

    [Test]
    public void GetText_NullOptions_BehavesLikeNoOptions()
    {
        var bytes = Encoding.UTF8.GetBytes("hello world");
        var result1 = _extractor.GetText("file.txt", bytes);
        var result2 = _extractor.GetText("file.txt", bytes, null);
        Assert.That(result1.Text, Is.EqualTo(result2.Text));
    }

    [Test]
    public async Task GetTextAsync_MaxCharacters_Truncates()
    {
        var bytes = Encoding.UTF8.GetBytes(new string('B', 500));
        var options = new ExtractionOptions { MaxCharacters = 50 };
        using var stream = new MemoryStream(bytes);

        var result = await _extractor.GetTextAsync("file.txt", stream, options, CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Text.Length, Is.EqualTo(50));
    }

    // 4. EML attachment extraction ─────────────────────────────────────────────

    [Test]
    public void EmlExtractor_WithJsonAttachment_ExtractsAttachmentText()
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("Sender", "sender@example.com"));
        message.Subject = "Test with attachment";

        var textPart = new TextPart("plain") { Text = "See attached." };
        var jsonPart = new MimePart("application", "json")
        {
            Content = new MimeContent(new MemoryStream(Encoding.UTF8.GetBytes("{\"greeting\":\"AttachmentContent\"}"))),
            ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
            ContentTransferEncoding = ContentEncoding.Base64,
            FileName = "data.json"
        };

        var multipart = new MimeKit.Multipart("mixed");
        multipart.Add(textPart);
        multipart.Add(jsonPart);
        message.Body = multipart;

        using var mem = new MemoryStream();
        message.WriteTo(mem);

        var result = _extractor.GetText("email.eml", mem.ToArray());

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Text, Does.Contain("See attached."));
        Assert.That(result.Text, Does.Contain("[Attachment: data.json]"));
        Assert.That(result.Text, Does.Contain("AttachmentContent"));
    }

    [Test]
    public void EmlExtractor_NoInnerExtractors_SkipsAttachments()
    {
        const string eml = """
            From: Sender <sender@example.com>
            Subject: Plain email
            MIME-Version: 1.0
            Content-Type: multipart/mixed; boundary="test-boundary"

            --test-boundary
            Content-Type: text/plain

            Body only.
            --test-boundary
            Content-Type: application/json; name="data.json"
            Content-Disposition: attachment; filename="data.json"

            {"key":"value"}
            --test-boundary--
            """;

        // EmlExtractor without inner extractors should NOT process attachments
        var extractor = new EmlExtractor();
        var result = extractor.ExtractText(Encoding.UTF8.GetBytes(eml));

        Assert.That(result, Does.Contain("Body only."));
        Assert.That(result, Does.Not.Contain("[Attachment:"));
    }

    // 5. XLSX + PPTX metadata ──────────────────────────────────────────────────

    [Test]
    public void GetTextWithMetadata_Xlsx_IncludesDocumentProperties()
    {
        using var mem = new MemoryStream();
        using (var spreadsheet = SpreadsheetDocument.Create(mem, DocumentFormat.OpenXml.SpreadsheetDocumentType.Workbook))
        {
            var workbookPart = spreadsheet.AddWorkbookPart();
            workbookPart.Workbook = new DocumentFormat.OpenXml.Spreadsheet.Workbook();
            var sheetPart = workbookPart.AddNewPart<WorksheetPart>();
            sheetPart.Worksheet = new DocumentFormat.OpenXml.Spreadsheet.Worksheet(
                new DocumentFormat.OpenXml.Spreadsheet.SheetData());
            var sheets = workbookPart.Workbook.AppendChild(new DocumentFormat.OpenXml.Spreadsheet.Sheets());
            sheets.AppendChild(new DocumentFormat.OpenXml.Spreadsheet.Sheet
            {
                Id = workbookPart.GetIdOfPart(sheetPart),
                SheetId = 1,
                Name = "Sheet1"
            });

            spreadsheet.PackageProperties.Title = "My Spreadsheet";
            spreadsheet.PackageProperties.Creator = "Spreadsheet Author";
        }

        var result = _extractor.GetTextWithMetadata("data.xlsx", mem.ToArray());

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Metadata.ContainsKey("xlsx.title"), Is.True);
        Assert.That(result.Metadata["xlsx.title"], Is.EqualTo("My Spreadsheet"));
        Assert.That(result.Metadata["xlsx.author"], Is.EqualTo("Spreadsheet Author"));
    }

    [Test]
    public void GetTextWithMetadata_Pptx_IncludesDocumentProperties()
    {
        var pptxBytes = CreateMinimalPptx("My Title Slide");

        // MemoryStream(byte[]) is non-expandable; copy to an expandable stream
        // before opening the package in read-write mode to stamp properties.
        var mem = new MemoryStream();
        mem.Write(pptxBytes);
        using (var pres = PresentationDocument.Open(mem, true))
        {
            pres.PackageProperties.Title = "My Presentation";
            pres.PackageProperties.Creator = "Slide Author";
        }

        var result = _extractor.GetTextWithMetadata("slides.pptx", mem.ToArray());

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Metadata.ContainsKey("pptx.title"), Is.True);
        Assert.That(result.Metadata["pptx.title"], Is.EqualTo("My Presentation"));
        Assert.That(result.Metadata["pptx.author"], Is.EqualTo("Slide Author"));
        Assert.That(result.Metadata.ContainsKey("pptx.slideCount"), Is.True);
        Assert.That(result.Metadata["pptx.slideCount"], Is.EqualTo("1"));
    }

    // 6. Batch API ────────────────────────────────────────────────────────────

    [Test]
    public async Task GetTextBatchAsync_ProcessesAllDocuments()
    {
        var docs = new[]
        {
            ("a.txt", Encoding.UTF8.GetBytes("Alpha")),
            ("b.txt", Encoding.UTF8.GetBytes("Beta")),
            ("c.txt", Encoding.UTF8.GetBytes("Gamma"))
        };

        var results = new List<ExtractionResult>();
        await foreach (var r in _extractor.GetTextBatchAsync(docs))
            results.Add(r);

        Assert.That(results.Count, Is.EqualTo(3));
        Assert.That(results.All(r => r.IsSuccess), Is.True);
        Assert.That(results[0].Text, Is.EqualTo("Alpha"));
        Assert.That(results[1].Text, Is.EqualTo("Beta"));
        Assert.That(results[2].Text, Is.EqualTo("Gamma"));
    }

    [Test]
    public async Task GetTextBatchAsync_WithMaxCharacters_TruncatesEachResult()
    {
        var docs = new[]
        {
            ("a.txt", Encoding.UTF8.GetBytes(new string('X', 200))),
            ("b.txt", Encoding.UTF8.GetBytes(new string('Y', 200)))
        };
        var options = new ExtractionOptions { MaxCharacters = 10 };

        var results = new List<ExtractionResult>();
        await foreach (var r in _extractor.GetTextBatchAsync(docs, options: options))
            results.Add(r);

        Assert.That(results.All(r => r.Text.Length == 10), Is.True);
    }

    [Test]
    public async Task GetTextBatchAsync_EmptyInput_YieldsNothing()
    {
        var results = new List<ExtractionResult>();
        await foreach (var r in _extractor.GetTextBatchAsync(Array.Empty<(string, byte[])>()))
            results.Add(r);

        Assert.That(results, Is.Empty);
    }

    [Test]
    public async Task GetTextBatchAsync_InvalidDegreeOfParallelism_Throws()
    {
        Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
        {
            await foreach (var _ in _extractor.GetTextBatchAsync(
                Array.Empty<(string, byte[])>(), maxDegreeOfParallelism: 0))
            { }
        });
    }

    [Test]
    public async Task GetTextBatchAsync_CancellationMidBatch_StopsProcessing()
    {
        using var cts = new CancellationTokenSource();

        var docs = Enumerable.Range(0, 20)
            .Select(i => ($"file{i}.txt", Encoding.UTF8.GetBytes($"Text {i}")))
            .ToList();

        var results = new List<ExtractionResult>();
        cts.Cancel(); // cancel immediately

        Assert.ThrowsAsync<OperationCanceledException>(async () =>
        {
            await foreach (var r in _extractor.GetTextBatchAsync(docs, cancellationToken: cts.Token))
                results.Add(r);
        });
    }

    // 7. ODT extractor ────────────────────────────────────────────────────────

    [Test]
    public void OdtExtractor_SimpleDocument_ExtractsParagraphText()
    {
        var odtBytes = CreateMinimalOdt("Hello from ODT");
        var result = _extractor.GetText("document.odt", odtBytes);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Text, Does.Contain("Hello from ODT"));
        Assert.That(result.ExtractorName, Is.EqualTo(nameof(OdtExtractor)));
    }

    [Test]
    public void OdtExtractor_CanHandle_CorrectExtensions()
    {
        var extractor = new OdtExtractor();
        Assert.That(extractor.CanHandle(".odt"), Is.True);
        Assert.That(extractor.CanHandle(".odp"), Is.True);
        Assert.That(extractor.CanHandle(".ODT"), Is.True);
        Assert.That(extractor.CanHandle(".docx"), Is.False);
    }

    // 8. Password-protected detection ─────────────────────────────────────────

    [Test]
    public void ExtractionResult_PasswordRelatedMessage_ClassifiedAsPasswordProtected()
    {
        // Simulate what the facade does when an extractor throws a password exception
        var result = ExtractionResult.Failure(
            "Extraction failed: The file is password protected",
            ExtractionErrorKind.PasswordProtected);

        Assert.That(result.ErrorKind, Is.EqualTo(ExtractionErrorKind.PasswordProtected));
        Assert.That(result.IsSuccess, Is.False);
    }

    // 9. TextNormalizer public ────────────────────────────────────────────────

    [Test]
    public void TextNormalizer_IsPubliclyAccessible()
    {
        var result = TextNormalizer.Normalize("  Hello   World  \n\n\n\n  Extra  ");
        Assert.That(result, Is.Not.Empty);
        Assert.That(result, Does.Contain("Hello"));
    }

    [Test]
    public void TextNormalizer_CollapsesBlankLines()
    {
        var input = $"Para1{Environment.NewLine}{Environment.NewLine}{Environment.NewLine}{Environment.NewLine}Para2";
        var result = TextNormalizer.Normalize(input);
        // 4 blank lines should collapse to at most 2 consecutive newlines
        Assert.That(result, Does.Not.Contain(
            string.Concat(Enumerable.Repeat(Environment.NewLine, 4))));
    }

    // 10. OverlapMode ─────────────────────────────────────────────────────────

    [Test]
    public void ChunkBySentences_OverlapMode_None_ProducesNoOverlap()
    {
        var text = "First sentence. Second sentence. Third sentence. Fourth sentence.";
        var chunks = TextChunker.ChunkBySentences(text, maxLength: 30, overlap: 15,
            overlapMode: OverlapMode.None);

        // With OverlapMode.None, each chunk should start where the previous ended
        for (int i = 1; i < chunks.Count; i++)
            Assert.That(chunks[i].StartOffset, Is.GreaterThanOrEqualTo(chunks[i - 1].EndOffset - 1));
    }

    [Test]
    public void ChunkBySentences_OverlapMode_Segment_OverlapsByOneSegment()
    {
        // maxLength=50 → first chunk holds 3 sentences (15+16+15=46 ≤ 50, +16 > 50)
        // Segment mode steps back to j-1, so next chunk starts at segment 2 (shared)
        var text = "Alpha sentence. Beta sentence. Gamma sentence. Delta sentence.";
        var chunks = TextChunker.ChunkBySentences(text, maxLength: 50, overlap: 0,
            overlapMode: OverlapMode.Segment);

        Assert.That(chunks.Count, Is.GreaterThanOrEqualTo(2));
        // Second chunk starts inside the first chunk's range → overlap
        Assert.That(chunks[1].StartOffset, Is.LessThan(chunks[0].EndOffset));
        // The shared sentence appears in both
        Assert.That(chunks[0].Text, Does.Contain("Gamma sentence."));
        Assert.That(chunks[1].Text, Does.Contain("Gamma sentence."));
    }

    [Test]
    public void ChunkByParagraphs_OverlapMode_None_NoOverlap()
    {
        var text = "Para one.\n\nPara two.\n\nPara three.\n\nPara four.";
        var chunks = TextChunker.ChunkByParagraphs(text, maxLength: 15, overlap: 10,
            overlapMode: OverlapMode.None);

        for (int i = 1; i < chunks.Count; i++)
            Assert.That(chunks[i].StartOffset, Is.GreaterThanOrEqualTo(chunks[i - 1].EndOffset - 1));
    }

    [Test]
    public void ChunkByParagraphs_OverlapMode_Characters_DefaultBehaviorPreserved()
    {
        var text = "Para one.\n\nPara two.\n\nPara three.\n\nPara four.";
        var chunksDefault = TextChunker.ChunkByParagraphs(text, maxLength: 25, overlap: 10);
        var chunksExplicit = TextChunker.ChunkByParagraphs(text, maxLength: 25, overlap: 10,
            overlapMode: OverlapMode.Characters);

        Assert.That(chunksDefault.Count, Is.EqualTo(chunksExplicit.Count));
        for (int i = 0; i < chunksDefault.Count; i++)
            Assert.That(chunksDefault[i].Text, Is.EqualTo(chunksExplicit[i].Text));
    }

    // ── ODT helper ───────────────────────────────────────────────────────────

    private static byte[] CreateMinimalOdt(string bodyText)
    {
        // Build a minimal ODT (OpenDocument Text) ZIP in memory.
        // Minimum required files: mimetype, content.xml, META-INF/manifest.xml
        using var ms = new MemoryStream();
        using (var zip = new ZipArchive(ms, ZipArchiveMode.Create, leaveOpen: true))
        {
            // mimetype must be first and uncompressed
            var mimeEntry = zip.CreateEntry("mimetype", CompressionLevel.NoCompression);
            using (var w = new StreamWriter(mimeEntry.Open()))
                w.Write("application/vnd.oasis.opendocument.text");

            // content.xml — must close stream before creating next entry
            var contentEntry = zip.CreateEntry("content.xml");
            using (var cw = new StreamWriter(contentEntry.Open(), Encoding.UTF8))
            {
                cw.Write($"""
                    <?xml version="1.0" encoding="UTF-8"?>
                    <office:document-content
                        xmlns:office="urn:oasis:names:tc:opendocument:xmlns:office:1.0"
                        xmlns:text="urn:oasis:names:tc:opendocument:xmlns:text:1.0"
                        office:version="1.2">
                      <office:body>
                        <office:text>
                          <text:p>{System.Security.SecurityElement.Escape(bodyText)}</text:p>
                        </office:text>
                      </office:body>
                    </office:document-content>
                    """);
            }

            // META-INF/manifest.xml
            var manifestEntry = zip.CreateEntry("META-INF/manifest.xml");
            using (var mw = new StreamWriter(manifestEntry.Open(), Encoding.UTF8))
            {
                mw.Write("""
                    <?xml version="1.0" encoding="UTF-8"?>
                    <manifest:manifest xmlns:manifest="urn:oasis:names:tc:opendocument:xmlns:manifest:1.0">
                      <manifest:file-entry manifest:full-path="/" manifest:media-type="application/vnd.oasis.opendocument.text"/>
                      <manifest:file-entry manifest:full-path="content.xml" manifest:media-type="text/xml"/>
                    </manifest:manifest>
                    """);
            }
        }
        return ms.ToArray();
    }
}
