using System.IO.Compression;
using System.Text;
using DocumentFormat.OpenXml.Packaging;
using NUnit.Framework;
using Transformations.Text;
using W = DocumentFormat.OpenXml.Wordprocessing;

namespace Transformations.Tests;

/// <summary>
/// Covers the extraction safety limits: binary detection, input size, Office/OpenDocument
/// decompression limits, cooperative cancellation and timeouts.
/// </summary>
[TestFixture]
public class TextExtractorSafetyLimitTests
{
    private TextExtractor _extractor = null!;

    [SetUp]
    public void Setup() => _extractor = new TextExtractor();

    // ── Error kind compatibility ─────────────────────────────────────────────

    [TestCase(ExtractionErrorKind.None, 0)]
    [TestCase(ExtractionErrorKind.Empty, 1)]
    [TestCase(ExtractionErrorKind.Unsupported, 2)]
    [TestCase(ExtractionErrorKind.PasswordProtected, 3)]
    [TestCase(ExtractionErrorKind.Corrupted, 4)]
    [TestCase(ExtractionErrorKind.ExtractionFailed, 5)]
    [TestCase(ExtractionErrorKind.Binary, 6)]
    [TestCase(ExtractionErrorKind.TooLarge, 7)]
    [TestCase(ExtractionErrorKind.LimitExceeded, 8)]
    [TestCase(ExtractionErrorKind.TimedOut, 9)]
    [TestCase(ExtractionErrorKind.NeedsOcr, 10)]
    public void ExtractionErrorKind_ExistingValuesKeepTheirNumbers(ExtractionErrorKind kind, int expected)
        => Assert.That((int)kind, Is.EqualTo(expected));

    // ── Binary ───────────────────────────────────────────────────────────────

    [TestCase("image.bin")]
    [TestCase("program.exe")]
    [TestCase("notes.txt")]
    [TestCase("no-extension")]
    public void BinaryContent_ReturnsBinaryFailure(string fileName)
    {
        byte[] blob = [0x7F, 0x45, 0x4C, 0x46, 0x00, 0x10, 0x4A, 0x46, 0x49, 0x46];

        var result = _extractor.GetText(fileName, blob);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.ErrorKind, Is.EqualTo(ExtractionErrorKind.Binary));
        Assert.That(result.ExtractorName, Is.EqualTo(nameof(TxtExtractor)));
    }

    [Test]
    public void BinaryContent_WithMetadata_ReturnsBinaryFailure()
    {
        var result = _extractor.GetTextWithMetadata("blob.bin", [1, 0, 2, 0, 3]);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.ErrorKind, Is.EqualTo(ExtractionErrorKind.Binary));
    }

    [Test]
    public void Utf16TextWithBom_IsNotTreatedAsBinary()
    {
        var bytes = Encoding.Unicode.GetPreamble().Concat(Encoding.Unicode.GetBytes("hello")).ToArray();

        var result = _extractor.GetText("note.txt", bytes);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Text, Is.EqualTo("hello"));
    }

    [Test]
    public void BinaryEntryInsideZip_IsStillSkippedWithWarning()
    {
        var zip = CreateZip(("payload.bin", [1, 0, 2]), ("readme.txt", Encoding.UTF8.GetBytes("inside")));

        var result = _extractor.GetText("archive.zip", zip);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Text, Does.Contain("inside"));
        Assert.That(result.Warnings.Select(w => w.Code), Does.Contain(ExtractionWarningCodes.ContainerUnsupportedEntry));
    }

    // ── Input size ───────────────────────────────────────────────────────────

    [Test]
    public void ByteArrayOverMaxInputBytes_ReturnsTooLarge()
    {
        var bytes = Encoding.UTF8.GetBytes(new string('a', 101));

        var result = _extractor.GetText("big.txt", bytes, new ExtractionOptions { MaxInputBytes = 100 });

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.ErrorKind, Is.EqualTo(ExtractionErrorKind.TooLarge));
    }

    [Test]
    public void ByteArrayAtMaxInputBytes_IsExtracted()
    {
        var bytes = Encoding.UTF8.GetBytes(new string('a', 100));

        var result = _extractor.GetText("big.txt", bytes, new ExtractionOptions { MaxInputBytes = 100 });

        Assert.That(result.IsSuccess, Is.True);
    }

    [Test]
    public void MaxInputBytesNull_DisablesTheLimit()
    {
        var bytes = Encoding.UTF8.GetBytes(new string('a', 5000));

        var result = _extractor.GetText("big.txt", bytes, new ExtractionOptions { MaxInputBytes = null });

        Assert.That(result.IsSuccess, Is.True);
    }

    [Test]
    public void DefaultMaxInputBytes_Is256MB()
        => Assert.That(ExtractionOptions.Default.MaxInputBytes, Is.EqualTo(256L * 1024 * 1024));

    [Test]
    public void EndlessNonSeekableStream_StopsReadingAtTheLimit()
    {
        using var endless = new EndlessStream();

        var result = _extractor.GetText("big.txt", endless, new ExtractionOptions { MaxInputBytes = 1_000_000 });

        Assert.That(result.ErrorKind, Is.EqualTo(ExtractionErrorKind.TooLarge));
        Assert.That(endless.BytesRead, Is.LessThan(1_000_000 + 2 * 81920));
    }

    [Test]
    public void EndlessNonSeekableStream_Async_StopsReadingAtTheLimit()
    {
        using var endless = new EndlessStream();

        var result = _extractor.GetTextWithMetadataAsync("big.txt", endless,
            new ExtractionOptions { MaxInputBytes = 1_000_000 }).GetAwaiter().GetResult();

        Assert.That(result.ErrorKind, Is.EqualTo(ExtractionErrorKind.TooLarge));
        Assert.That(endless.BytesRead, Is.LessThan(1_000_000 + 2 * 81920));
    }

    [Test]
    public void SeekableStreamOverLimit_IsRejectedWithoutReading()
    {
        using var stream = new MemoryStream(new byte[500]);

        var result = _extractor.GetTextWithMetadata("big.txt", stream, new ExtractionOptions { MaxInputBytes = 100 });

        Assert.That(result.ErrorKind, Is.EqualTo(ExtractionErrorKind.TooLarge));
        Assert.That(stream.Position, Is.EqualTo(0));
    }

    // ── Office / OpenDocument expansion ──────────────────────────────────────

    [Test]
    public void DocxThatExpandsPastLimit_ReturnsLimitExceeded()
    {
        var docx = AddPart(CreateDocx("Hello"), "customXml/padding.bin", new byte[4 * 1024 * 1024]);
        Assert.That(docx.Length, Is.LessThan(64 * 1024), "zeros should compress to a few KB");

        var result = _extractor.GetText("report.docx", docx, new ExtractionOptions { MaxDecompressedBytes = 1024 * 1024 });

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.ErrorKind, Is.EqualTo(ExtractionErrorKind.LimitExceeded));
        Assert.That(result.ExtractorName, Is.EqualTo(nameof(DocxExtractor)));
    }

    [Test]
    public void DocxUnderLimit_IsExtracted()
    {
        var docx = AddPart(CreateDocx("Hello"), "customXml/padding.bin", new byte[4 * 1024 * 1024]);

        var result = _extractor.GetText("report.docx", docx);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Text, Is.EqualTo("Hello"));
    }

    [Test]
    public void DocxWithTooManyParts_ReturnsLimitExceeded()
    {
        var result = _extractor.GetText("report.docx", CreateDocx("Hello"), new ExtractionOptions { MaxContainerEntries = 1 });

        Assert.That(result.ErrorKind, Is.EqualTo(ExtractionErrorKind.LimitExceeded));
    }

    // The guard runs before the format parser, so a bare ZIP with a large entry is enough
    // to show each format is protected.
    [TestCase("book.xlsx", nameof(ExcelExtractor))]
    [TestCase("book.xlsm", nameof(ExcelExtractor))]
    [TestCase("deck.pptx", nameof(PptxExtractor))]
    [TestCase("doc.odt", nameof(OdtExtractor))]
    [TestCase("deck.odp", nameof(OdtExtractor))]
    public void OfficePackageThatExpandsPastLimit_ReturnsLimitExceeded(string fileName, string extractor)
    {
        var zip = CreateZip(("content.xml", new byte[2 * 1024 * 1024]));

        var result = _extractor.GetText(fileName, zip, new ExtractionOptions { MaxDecompressedBytes = 1024 * 1024 });

        Assert.That(result.ErrorKind, Is.EqualTo(ExtractionErrorKind.LimitExceeded));
        Assert.That(result.ExtractorName, Is.EqualTo(extractor));
    }

    [Test]
    public void DocxBombInsideZip_IsSkippedWithByteLimitWarning()
    {
        var bomb = AddPart(CreateDocx("Hidden"), "customXml/padding.bin", new byte[2 * 1024 * 1024]);
        var zip = CreateZip(("bomb.docx", bomb), ("readme.txt", Encoding.UTF8.GetBytes("visible")));

        var result = _extractor.GetText("archive.zip", zip, new ExtractionOptions { MaxDecompressedBytes = 1024 * 1024 });

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Text, Does.Contain("visible").And.Not.Contain("Hidden"));
        Assert.That(result.Warnings.Select(w => w.Code), Does.Contain(ExtractionWarningCodes.ContainerEntryFailed));
    }

    [Test]
    public void EpubWithOversizedPackageDocument_WarnsInsteadOfReadingItAll()
    {
        var epub = CreateZip(
            ("mimetype", Encoding.ASCII.GetBytes("application/epub+zip")),
            ("META-INF/container.xml", Encoding.UTF8.GetBytes(
                "<container><rootfiles><rootfile full-path=\"content.opf\"/></rootfiles></container>")),
            ("content.opf", new byte[2 * 1024 * 1024]));

        var result = _extractor.GetText("book.epub", epub, new ExtractionOptions { MaxDecompressedBytes = 1024 * 1024 });

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Warnings.Select(w => w.Code), Does.Contain(ExtractionWarningCodes.ContainerSpineEmpty));
    }

    // ── Cancellation and timeout ─────────────────────────────────────────────

    [Test]
    public void PreCancelledOptionsToken_Throws()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        Assert.Throws<OperationCanceledException>(() =>
            _extractor.GetText("report.docx", CreateDocx("Hello"), new ExtractionOptions { CancellationToken = cts.Token }));
    }

    [Test]
    public void PreCancelledAsyncToken_Throws()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("text"));

        Assert.That(() => _extractor.GetTextAsync("a.txt", stream, cts.Token).GetAwaiter().GetResult(),
            Throws.InstanceOf<OperationCanceledException>());
    }

    [Test]
    public void CancellingMidZip_StopsAtNextEntry_AndIsNotSwallowedAsWarning()
    {
        using var cts = new CancellationTokenSource();
        var zip = CreateZip(("first.unknownext", [1, 2, 3]), ("second.txt", Encoding.UTF8.GetBytes("never read")));
        var options = new ExtractionOptions
        {
            CancellationToken = cts.Token,
            WarningSink = _ => cts.Cancel(), // fires while the first entry is being skipped
        };

        Assert.Throws<OperationCanceledException>(() => _extractor.GetText("archive.zip", zip, options));
    }

    [Test]
    public void CancellingParser_StopsTheParser()
    {
        using var cts = new CancellationTokenSource();
        var slow = new SpinUntilCancelledExtractor();
        var extractor = new TextExtractor([slow, new TxtExtractor()]);
        cts.CancelAfter(TimeSpan.FromMilliseconds(50));

        Assert.Throws<OperationCanceledException>(() =>
            extractor.GetText("x.slow", [1], new ExtractionOptions { CancellationToken = cts.Token }));
        Assert.That(slow.StoppedByToken, Is.True);
    }

    [Test]
    public void Timeout_ReturnsTimedOutAndStopsTheParser()
    {
        var slow = new SpinUntilCancelledExtractor();
        var extractor = new TextExtractor([slow, new TxtExtractor()]);

        var result = extractor.GetText("x.slow", [1], new ExtractionOptions { Timeout = TimeSpan.FromMilliseconds(50) });

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.ErrorKind, Is.EqualTo(ExtractionErrorKind.TimedOut));
        Assert.That(slow.StoppedByToken, Is.True);
    }

    [Test]
    public void Timeout_WithMetadata_ReturnsTimedOut()
    {
        var extractor = new TextExtractor([new SpinUntilCancelledExtractor(), new TxtExtractor()]);

        var result = extractor.GetTextWithMetadata("x.slow", [1], new ExtractionOptions { Timeout = TimeSpan.FromMilliseconds(50) });

        Assert.That(result.ErrorKind, Is.EqualTo(ExtractionErrorKind.TimedOut));
    }

    [Test]
    public void Timeout_NotReached_Succeeds()
    {
        var result = _extractor.GetText("report.docx", CreateDocx("Hello"),
            new ExtractionOptions { Timeout = TimeSpan.FromMinutes(1) });

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Text, Is.EqualTo("Hello"));
    }

    [Test]
    public void BatchCancellation_ReachesRunningParsers()
    {
        using var cts = new CancellationTokenSource();
        var slow = new SpinUntilCancelledExtractor();
        var extractor = new TextExtractor([slow, new TxtExtractor()]);
        cts.CancelAfter(TimeSpan.FromMilliseconds(50));

        Assert.That(async () =>
        {
            await foreach (var _ in extractor.GetTextBatchAsync([("x.slow", new byte[] { 1 })], cancellationToken: cts.Token)) { }
        }, Throws.InstanceOf<OperationCanceledException>());
        Assert.That(slow.StoppedByToken, Is.True);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static byte[] CreateZip(params (string Name, byte[] Content)[] entries)
    {
        using var ms = new MemoryStream();
        using (var zip = new ZipArchive(ms, ZipArchiveMode.Create, leaveOpen: true))
        {
            foreach (var (name, content) in entries)
            {
                using var s = zip.CreateEntry(name, CompressionLevel.Optimal).Open();
                s.Write(content);
            }
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

    /// <summary>Adds a raw, unreferenced ZIP entry to an existing package (Office ignores unknown parts).</summary>
    private static byte[] AddPart(byte[] package, string name, byte[] content)
    {
        using var ms = new MemoryStream();
        ms.Write(package);
        using (var zip = new ZipArchive(ms, ZipArchiveMode.Update, leaveOpen: true))
        {
            using var s = zip.CreateEntry(name, CompressionLevel.Optimal).Open();
            s.Write(content);
        }
        return ms.ToArray();
    }

    private sealed class EndlessStream : Stream
    {
        public long BytesRead { get; private set; }
        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => BytesRead; set => throw new NotSupportedException(); }

        public override int Read(byte[] buffer, int offset, int count)
        {
            Array.Fill(buffer, (byte)'a', offset, count);
            BytesRead += count;
            return count;
        }

        public override void Flush() { }
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }

    /// <summary>Stands in for a parser stuck on a pathological file; only the token stops it.</summary>
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
