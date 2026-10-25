using System.Linq;
using static Transformations.TestDocuments.SafetyTestDocs;

namespace Transformations.TestDocuments;

/// <summary>
/// Ordinary and malformed documents with known expected text. Each marker is a unique SMOKE-* token placed where the
/// extractor should find it, so a check is "the marker is there", not "the call did not throw".
/// </summary>
public static class OrdinaryCorpus
{
    public static IEnumerable<CorpusSample> All()
    {
        // ── Office and PDF ──
        yield return new("pdf two pages", "pdf-two-pages.pdf",
            () => Pdf(("SMOKE-PDF-001 first page", null, null), ("SMOKE-PDF-002 second page", null, null)),
            CorpusLabel.Ordinary, MustContain: ["SMOKE-PDF-001", "SMOKE-PDF-002"]);

        yield return new("docx paragraphs", "docx-paragraphs.docx",
            () => Docx(null, Para("SMOKE-DOCX-001 heading"), Para("SMOKE-DOCX-002 body text")),
            CorpusLabel.Ordinary, MustContain: ["SMOKE-DOCX-001", "SMOKE-DOCX-002"]);

        yield return new("pptx two slides", "pptx-two-slides.pptx",
            () => Pptx((true, "SMOKE-PPTX-001 agenda"), (true, "SMOKE-PPTX-002 results")),
            CorpusLabel.Ordinary, MustContain: ["SMOKE-PPTX-001", "SMOKE-PPTX-002"]);

        yield return new("xlsx sheet", "xlsx-sheet.xlsx",
            () => Xlsx(("Sales", false, ["Region", "SMOKE-XLSX-001"])),
            CorpusLabel.Ordinary, MustContain: ["SMOKE-XLSX-001"]);

        // ── Messages and containers ──
        yield return new("eml with attachment", "eml-attachment.eml",
            () => Eml("SMOKE-EML-001 body", null, ("notes.txt", Utf8("SMOKE-EML-002 attachment"))),
            CorpusLabel.Ordinary, MustContain: ["SMOKE-EML-001", "SMOKE-EML-002"]);

        yield return new("zip of documents", "zip-documents.zip",
            () => Zip(("a.txt", Utf8("SMOKE-ZIP-001 first entry")), ("b.md", Utf8("# Heading\nSMOKE-ZIP-002 second entry"))),
            CorpusLabel.Ordinary, MustContain: ["SMOKE-ZIP-001", "SMOKE-ZIP-002"]);

        yield return new("epub two chapters", "epub-two-chapters.epub",
            () => Epub(("ch1.xhtml", "<h1>Chapter one</h1><p>SMOKE-EPUB-001 text</p>"),
                       ("ch2.xhtml", "<h1>Chapter two</h1><p>SMOKE-EPUB-002 text</p>")),
            CorpusLabel.Ordinary, MustContain: ["SMOKE-EPUB-001", "SMOKE-EPUB-002"]);

        yield return new("odt paragraph", "odt-paragraph.odt",
            () => Odt("SMOKE-ODT-001 paragraph"),
            CorpusLabel.Ordinary, MustContain: ["SMOKE-ODT-001"]);

        yield return new("rtf cp1252 euro", "rtf-cp1252.rtf",
            () => Utf8("""{\rtf1\ansi\ansicpg1252 SMOKE-RTF-001 Price \'80 5\par}"""),
            CorpusLabel.Ordinary, MustContain: ["SMOKE-RTF-001", "€"]);

        // ── Web and markup ──
        yield return new("html visible and hidden", "html-visible-hidden.html",
            () => Utf8("""
                <h1>SMOKE-HTML-001</h1>
                <p>Visible <a href="https://example.test">link SMOKE-HTML-002</a></p>
                <table><tr><td>SMOKE-HTML-003</td></tr></table>
                <div style="display:none">SMOKE-HTML-HIDDEN</div>
                """),
            CorpusLabel.Ordinary, MustContain: ["SMOKE-HTML-001", "SMOKE-HTML-002", "SMOKE-HTML-003"],
            MustNotContain: ["SMOKE-HTML-HIDDEN"]);

        yield return new("markdown with code and unicode", "markdown-code-unicode.md",
            () => Utf8("# SMOKE-MD-001\n\n```\nprint('x')\n```\n\nÜnïcödé 日本語 SMOKE-MD-002\n"),
            CorpusLabel.Ordinary, MustContain: ["SMOKE-MD-001", "SMOKE-MD-002", "日本語"]);

        // ── Plain text, code and logs ──
        yield return new("txt crlf with bom", "txt-crlf-bom.txt",
            () => [0xEF, 0xBB, 0xBF, .. Utf8("SMOKE-TXT-001 first line\r\nsecond line SMOKE-TXT-002\r\n")],
            CorpusLabel.Ordinary, MustContain: ["SMOKE-TXT-001", "SMOKE-TXT-002"]);

        yield return new("source code", "source-code.cs",
            () => Utf8("// SMOKE-CODE-001\nclass Sample { }\n"),
            CorpusLabel.Ordinary, MustContain: ["SMOKE-CODE-001"]);

        yield return new("log file", "app.log",
            () => Utf8("2026-10-25 INFO SMOKE-LOG-001 started\n"),
            CorpusLabel.Ordinary, MustContain: ["SMOKE-LOG-001"]);

        // ── Structured data ──
        yield return new("csv repeated header and quoting", "csv-repeated-header.csv",
            () => Utf8("Name,Name,Note\r\nAlice,Bob,\"SMOKE-CSV-001, quoted\"\r\n\"multi\nline\",x,SMOKE-CSV-002\r\n"),
            CorpusLabel.Ordinary, MustContain: ["SMOKE-CSV-001", "SMOKE-CSV-002", "Bob"]);

        yield return new("json nested", "json-nested.json",
            () => Utf8("""{"order":{"id":1,"items":[{"sku":"SMOKE-JSON-001"}]},"name":"Zoë"}"""),
            CorpusLabel.Ordinary, MustContain: ["SMOKE-JSON-001", "Zoë"]);

        yield return new("yaml multiline", "yaml-multiline.yaml",
            () => Utf8("order:\n  id: 1\n  note: |\n    SMOKE-YAML-001\n    second line\nname: Zoë\n"),
            CorpusLabel.Ordinary, MustContain: ["SMOKE-YAML-001", "Zoë"]);

        yield return new("xml nested", "xml-nested.xml",
            () => Utf8("<root><item id=\"1\">SMOKE-XML-001</item><note>Zoë</note></root>"),
            CorpusLabel.Ordinary, MustContain: ["SMOKE-XML-001", "Zoë"]);

        // ── Malformed and misnamed: must fail or warn, never come back silently empty ──
        yield return new("truncated docx", "truncated.docx",
            () => Docx(null, Para("SMOKE-TRUNC-001")).Take(200).ToArray(),
            CorpusLabel.Malformed, "A DOCX cut off after 200 bytes.");

        yield return new("random bytes named pdf", "random.pdf",
            () => RandomBytes(4096, seed: 42),
            CorpusLabel.Malformed, "Random bytes with a PDF extension.");

        yield return new("plain text named pdf", "misnamed.pdf",
            () => Utf8("SMOKE-MISNAMED-001 plain text in a file called .pdf"),
            CorpusLabel.Malformed, "Text content under a PDF extension; sniffing or a typed failure is expected.");
    }

    /// <summary>A minimal EPUB 2 package: a stored mimetype, a container, an OPF with a spine, and the chapters.</summary>
    private static byte[] Epub(params (string Name, string Body)[] chapters)
    {
        var manifest = string.Concat(chapters.Select((c, i) => $"<item id=\"c{i}\" href=\"{c.Name}\" media-type=\"application/xhtml+xml\"/>"));
        var spine = string.Concat(chapters.Select((_, i) => $"<itemref idref=\"c{i}\"/>"));
        var entries = new List<(string Name, byte[] Content)>
        {
            ("mimetype", Utf8("application/epub+zip")),
            ("META-INF/container.xml", Utf8("<?xml version=\"1.0\"?><container version=\"1.0\" xmlns=\"urn:oasis:names:tc:opendocument:xmlns:container\"><rootfiles><rootfile full-path=\"OEBPS/content.opf\" media-type=\"application/oebps-package+xml\"/></rootfiles></container>")),
            ("OEBPS/content.opf", Utf8($"<?xml version=\"1.0\"?><package version=\"2.0\" xmlns=\"http://www.idpf.org/2007/opf\"><metadata xmlns:dc=\"http://purl.org/dc/elements/1.1/\"><dc:title>Smoke</dc:title></metadata><manifest>{manifest}</manifest><spine>{spine}</spine></package>")),
        };
        foreach (var chapter in chapters)
            entries.Add(($"OEBPS/{chapter.Name}", Utf8($"<?xml version=\"1.0\"?><html xmlns=\"http://www.w3.org/1999/xhtml\"><body>{chapter.Body}</body></html>")));
        return Zip(entries.ToArray());
    }

    /// <summary>A minimal OpenDocument text file: a stored mimetype and a content.xml with one paragraph.</summary>
    private static byte[] Odt(string paragraph) => Zip(
        ("mimetype", Utf8("application/vnd.oasis.opendocument.text")),
        ("content.xml", Utf8($"<?xml version=\"1.0\" encoding=\"UTF-8\"?><office:document-content xmlns:office=\"urn:oasis:names:tc:opendocument:xmlns:office:1.0\" xmlns:text=\"urn:oasis:names:tc:opendocument:xmlns:text:1.0\"><office:body><office:text><text:p>{paragraph}</text:p></office:text></office:body></office:document-content>")));

    private static byte[] RandomBytes(int length, int seed)
    {
        var bytes = new byte[length];
        new Random(seed).NextBytes(bytes);
        return bytes;
    }
}
