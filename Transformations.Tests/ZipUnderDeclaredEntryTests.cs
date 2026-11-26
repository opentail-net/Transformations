namespace Transformations.Tests
{
    using System;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Text;

    using NUnit.Framework;
    using Transformations.Text;

    /// <summary>
    /// An archive entry whose header declares fewer bytes than its data. The runtimes differ in what they hand back for such an
    /// entry: net8.0 to net10.0 stop at the declared size, and net11.0 refuses the first read. The invariant that holds on
    /// every runtime is that bytes past the declared size never reach the extracted text, and that the extractors do not
    /// throw for a chapter. These tests check that invariant on the real extractors, with an honest control for each.
    /// </summary>
    [TestFixture]
    public class ZipUnderDeclaredEntryTests
    {
        // Sits after the first 100 bytes of the entry, where the lying header stops.
        private const string TailMarker = "ZZ-TAIL-MARKER-NEVER-EXTRACT";
        private const string DocxPart = "word/document.xml";
        private const string EpubChapter = "OEBPS/ch1.xhtml";
        private const uint DeclaredBytes = 100;

        [Test]
        public void Docx_HonestSizes_ExtractsTheTailMarker()
        {
            string text = new DocxExtractor().ExtractText(BuildDocx(declaredLie: false), null);

            Assert.That(text, Does.Contain(TailMarker));
        }

        [Test]
        public void Docx_EntryDeclaringFewerBytesThanItsData_NeverExtractsTheTail()
        {
            string? text = TryExtract(() => new DocxExtractor().ExtractText(BuildDocx(declaredLie: true), null));

            // The truncated part is malformed XML: net8.0 fails in OpenXml (XmlException), net11.0 fails at the archive read
            // (InvalidDataException). Either way no text comes back, so the tail cannot appear.
            Assert.That(text ?? string.Empty, Does.Not.Contain(TailMarker));
        }

        [Test]
        public void Epub_HonestSizes_ExtractsTheTailMarker()
        {
            string text = new EpubExtractor().ExtractText(BuildEpub(declaredLie: false), null);

            Assert.That(text, Does.Contain(TailMarker));
        }

        [Test]
        public void Epub_ChapterDeclaringFewerBytesThanItsData_NeverExtractsTheTail()
        {
            // A failed chapter is reported as a warning and skipped, so the extractor returns text for the rest of the book.
            string text = new EpubExtractor().ExtractText(BuildEpub(declaredLie: true), null);

            Assert.That(text, Does.Not.Contain(TailMarker));
        }

        private static string? TryExtract(Func<string> extract)
        {
            try
            {
                return extract();
            }
            catch (Exception ex) when (ex is InvalidDataException or System.Xml.XmlException)
            {
                return null;
            }
        }

        private static byte[] BuildDocx(bool declaredLie)
        {
            byte[] document = Encoding.UTF8.GetBytes(
                "<w:document xmlns:w=\"http://schemas.openxmlformats.org/wordprocessingml/2006/main\"><w:body><w:p><w:r><w:t>"
                + BigBody() + "</w:t></w:r></w:p></w:body></w:document>");
            byte[] zip = BuildZip(
                ("[Content_Types].xml", Encoding.UTF8.GetBytes(ContentTypesXml), false),
                ("_rels/.rels", Encoding.UTF8.GetBytes(RootRelsXml), false),
                (DocxPart, document, false));
            return declaredLie ? DeclareSize(zip, DocxPart, DeclaredBytes) : zip;
        }

        private static byte[] BuildEpub(bool declaredLie)
        {
            byte[] mimetype = Encoding.ASCII.GetBytes("application/epub+zip");
            byte[] container = Encoding.UTF8.GetBytes(
                "<?xml version=\"1.0\"?><container xmlns=\"urn:oasis:names:tc:opendocument:xmlns:container\" version=\"1.0\">"
                + "<rootfiles><rootfile full-path=\"OEBPS/content.opf\" media-type=\"application/oebps-package+xml\"/></rootfiles></container>");
            byte[] opf = Encoding.UTF8.GetBytes(
                "<?xml version=\"1.0\"?><package xmlns=\"http://www.idpf.org/2007/opf\" version=\"3.0\"><manifest>"
                + "<item id=\"c1\" href=\"ch1.xhtml\" media-type=\"application/xhtml+xml\"/></manifest><spine><itemref idref=\"c1\"/></spine></package>");
            byte[] chapter = Encoding.UTF8.GetBytes(
                "<html xmlns=\"http://www.w3.org/1999/xhtml\"><body><p>" + BigBody() + "</p></body></html>");
            byte[] zip = BuildZip(
                ("mimetype", mimetype, true),
                ("META-INF/container.xml", container, false),
                ("OEBPS/content.opf", opf, false),
                (EpubChapter, chapter, false));
            return declaredLie ? DeclareSize(zip, EpubChapter, DeclaredBytes) : zip;
        }

        private static string BigBody()
            => string.Concat(Enumerable.Repeat("The quick brown fox jumps over the lazy dog. ", 50000)) + TailMarker;

        private static byte[] BuildZip(params (string Name, byte[] Data, bool Store)[] entries)
        {
            using var ms = new MemoryStream();
            using (var zip = new ZipArchive(ms, ZipArchiveMode.Create, leaveOpen: true))
            {
                foreach (var (name, data, store) in entries)
                {
                    var entry = zip.CreateEntry(name, store ? CompressionLevel.NoCompression : CompressionLevel.Optimal);
                    using var stream = entry.Open();
                    stream.Write(data, 0, data.Length);
                }
            }

            return ms.ToArray();
        }

        // Rewrites the declared uncompressed size in the local and central headers of one entry. The data is unchanged.
        private static byte[] DeclareSize(byte[] zip, string name, uint declared)
        {
            for (int i = 0; i + 50 <= zip.Length; i++)
            {
                if (zip[i] != 0x50 || zip[i + 1] != 0x4b)
                {
                    continue;
                }

                if (zip[i + 2] == 0x03 && zip[i + 3] == 0x04)
                {
                    int length = BitConverter.ToUInt16(zip, i + 26);
                    if (Encoding.UTF8.GetString(zip, i + 30, length) == name)
                    {
                        BitConverter.TryWriteBytes(new Span<byte>(zip, i + 22, 4), declared);
                    }
                }
                else if (zip[i + 2] == 0x01 && zip[i + 3] == 0x02)
                {
                    int length = BitConverter.ToUInt16(zip, i + 28);
                    if (Encoding.UTF8.GetString(zip, i + 46, length) == name)
                    {
                        BitConverter.TryWriteBytes(new Span<byte>(zip, i + 24, 4), declared);
                    }
                }
            }

            return zip;
        }

        private const string ContentTypesXml =
            "<?xml version=\"1.0\" encoding=\"UTF-8\"?><Types xmlns=\"http://schemas.openxmlformats.org/package/2006/content-types\">"
            + "<Default Extension=\"rels\" ContentType=\"application/vnd.openxmlformats-package.relationships+xml\"/>"
            + "<Default Extension=\"xml\" ContentType=\"application/xml\"/>"
            + "<Override PartName=\"/word/document.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.wordprocessingml.document.main+xml\"/>"
            + "</Types>";

        private const string RootRelsXml =
            "<?xml version=\"1.0\" encoding=\"UTF-8\"?><Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\">"
            + "<Relationship Id=\"rId1\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument\" Target=\"word/document.xml\"/>"
            + "</Relationships>";
    }
}
