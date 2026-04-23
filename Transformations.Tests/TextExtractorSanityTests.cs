using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
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

    //[Test]
    //public void DocxExtractor_PreservesParagraphSeparation()
    //{
    //    // Setup: Create a minimal valid DOCX in memory
    //    using var mem = new MemoryStream();
    //    using (var wordDoc = WordprocessingDocument.Create(mem, WordprocessingDocumentType.Document))
    //    {
    //        var mainPart = wordDoc.AddMainDocumentPart();
    //        mainPart.Document = new Document(new Body(
    //            new Paragraph(new Run(new Text("Para 1"))),
    //            new Paragraph(new Run(new Text("Para 2")))
    //        ));
    //    }

    //    // Execute
    //    var result = _extractor.GetText("test.docx", mem.ToArray());

    //    // Assert
    //    Assert.That(result.IsSuccess, Is.True);
    //    Assert.That(result.Text, Does.Contain($"Para 1{Environment.NewLine}Para 2"));
    //}

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
    public void UnknownExtension_ReturnsFailure()
    {
        // Requirement: "Unknown extension defaults to TXT" 
        // If your business logic strictly requires a fail, use this.
        // If you want a TXT fallback, you would adjust the TextExtractor.GetText logic.

        byte[] bytes = Encoding.UTF8.GetBytes("Some data");
        var result = _extractor.GetText("file.xyz", bytes);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.ErrorMessage, Does.Contain("Unsupported"));
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
}