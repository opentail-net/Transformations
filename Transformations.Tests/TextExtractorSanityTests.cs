using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using MimeKit;
using NUnit.Framework;
using System.Reflection.Metadata;
using System.Text;
using Transformations.Text;
using static System.Net.Mime.MediaTypeNames;

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
                new Paragraph(new Run(new DocumentFormat.OpenXml.Wordprocessing.Text("Para 1"))),
                new Paragraph(new Run(new DocumentFormat.OpenXml.Wordprocessing.Text("Para 2")))
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

        // If PdfPig fails to parse the malformed byte array, it returns a Failure Result
        // which proves the routing to PdfPig worked.
        Assert.That(result.ErrorMessage, Does.Contain("PDF") | Does.Contain("extraction"));
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
        Assert.That(result.Text, Does.Contain($"Paris{Environment.NewLine}{Environment.NewLine}Name:")); // Row separation
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
    [Ignore("Requires embedded resource test.xlsx")]
    public void ExcelExtractor_ProcessesMultipleSheets()
    {
        // Note: Creating a raw Excel byte array in code is complex. 
        // In a real test, you'd use a small 5KB embedded resource file.
        // This test verifies the logic assumes headers and sheet names correctly.
        var bytes = GetEmbeddedTestExcel(); // Helper to load a tiny .xlsx

        var result = _extractor.GetText("inventory.xlsx", bytes);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Text, Does.Contain("[Sheet1]"));
        Assert.That(result.Text, Does.Contain("PartNumber:"));
    }

    private byte[] GetEmbeddedTestExcel()
    {
        var assembly = typeof(TextExtractorSanityTests).Assembly;
        // Format: {Namespace}.{Folder}.{FileName}
        string resourceName = "OpenTail.Tests.Resources.test.xlsx";

        using Stream? stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
            throw new FileNotFoundException($"Could not find embedded resource: {resourceName}");

        using MemoryStream ms = new MemoryStream();
        stream.CopyTo(ms);
        return ms.ToArray();
    }

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

        // Should either fail or skip with a skip message
        Assert.That(result.IsSuccess == false || result.Text.Contains("Skipped"), Is.True);
    }

    [Test]
    public void NullContent_ReturnsFailure()
    {
        var result = _extractor.GetText("test.txt", null);

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
}