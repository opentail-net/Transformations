using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.Json;
using DocumentFormat.OpenXml.Packaging;
using MimeKit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using UglyToad.PdfPig;

namespace Transformations.Text;

/// <summary>
/// Primary facade for text extraction. Routes files to format-specific extractors,
/// normalizes the result, and returns typed success/failure DTOs.
/// Register via <see cref="ServiceCollectionExtensions.AddTextExtractor"/> for DI,
/// or construct directly with <c>new TextExtractor()</c>.
/// </summary>
public class TextExtractor : IDocumentTextExtractor
{
    private readonly IReadOnlyList<ITextExtractor> _extractors;
    private readonly ILogger<TextExtractor> _logger;

    // ── Constructors ─────────────────────────────────────────────────────────

    /// <summary>Direct use: default extractors, no logging.</summary>
    public TextExtractor() : this(CreateDefaultExtractors(), NullLogger<TextExtractor>.Instance) { }

    /// <summary>DI constructor — logger is injected automatically.</summary>
    public TextExtractor(ILogger<TextExtractor> logger) : this(CreateDefaultExtractors(), logger) { }

    /// <summary>Custom extractor set, no logging.</summary>
    public TextExtractor(IEnumerable<ITextExtractor> extractors)
        : this(extractors, NullLogger<TextExtractor>.Instance) { }

    /// <summary>Full constructor: custom extractors + logger.</summary>
    public TextExtractor(IEnumerable<ITextExtractor> extractors, ILogger<TextExtractor> logger)
    {
        _extractors = extractors.ToList().AsReadOnly();
        _logger = logger;
    }

    public static IEnumerable<ITextExtractor> CreateDefaultExtractors()
    {
        var core = new List<ITextExtractor>
        {
            new PdfExtractor(),
            new DocxExtractor(),
            new PptxExtractor(),
            new HtmlExtractor(),
            new MarkdownExtractor(),
            new CsvExtractor(),
            new ExcelExtractor(),
            new MsgExtractor(),
            new JsonExtractor(),
            new XmlExtractor(),
            new YamlExtractor(),
            new LogExtractor(),
            new CodeExtractor(),
            new OdtExtractor(),
            new RtfExtractor(),
            new TxtExtractor()
        };
        // EmlExtractor, ZipExtractor, and EpubExtractor each take a snapshot of the core
        // pipeline so they can recursively extract nested documents without circular refs.
        // Insert before TxtExtractor (the catch-all) so Route finds them first.
        core.Insert(core.Count - 1, new EmlExtractor(core));
        core.Insert(core.Count - 1, new ZipExtractor(core));
        core.Insert(core.Count - 1, new EpubExtractor(core));
        return core;
    }

    // ── byte[] surface ──────────────────────────────────────────────────────

    public ExtractionResult GetText(string fileName, byte[] content)
        => GetTextCore(fileName, content, null);

    public ExtractionResult GetText(string fileName, byte[] content, ExtractionOptions? options)
        => GetTextCore(fileName, content, options);

    public ExtractionMetadataResult GetTextWithMetadata(string fileName, byte[] content)
        => GetTextWithMetadataCore(fileName, content, null);

    public ExtractionMetadataResult GetTextWithMetadata(string fileName, byte[] content, ExtractionOptions? options)
        => GetTextWithMetadataCore(fileName, content, options);

    // ── Stream surface ───────────────────────────────────────────────────────

    public ExtractionResult GetText(string fileName, Stream content)
        => GetTextCore(fileName, ReadStream(content), null);

    public ExtractionResult GetText(string fileName, Stream content, ExtractionOptions? options)
        => GetTextCore(fileName, ReadStream(content), options);

    public ExtractionMetadataResult GetTextWithMetadata(string fileName, Stream content)
        => GetTextWithMetadataCore(fileName, ReadStream(content), null);

    public ExtractionMetadataResult GetTextWithMetadata(string fileName, Stream content, ExtractionOptions? options)
        => GetTextWithMetadataCore(fileName, ReadStream(content), options);

    // ── Async surface ────────────────────────────────────────────────────────

    public async Task<ExtractionResult> GetTextAsync(string fileName, Stream content, CancellationToken cancellationToken = default)
        => GetTextCore(fileName, await ReadStreamAsync(content, cancellationToken), null);

    public async Task<ExtractionResult> GetTextAsync(string fileName, Stream content, ExtractionOptions? options, CancellationToken cancellationToken = default)
        => GetTextCore(fileName, await ReadStreamAsync(content, cancellationToken), options);

    public async Task<ExtractionMetadataResult> GetTextWithMetadataAsync(string fileName, Stream content, CancellationToken cancellationToken = default)
        => GetTextWithMetadataCore(fileName, await ReadStreamAsync(content, cancellationToken), null);

    public async Task<ExtractionMetadataResult> GetTextWithMetadataAsync(string fileName, Stream content, ExtractionOptions? options, CancellationToken cancellationToken = default)
        => GetTextWithMetadataCore(fileName, await ReadStreamAsync(content, cancellationToken), options);

    // ── Batch ────────────────────────────────────────────────────────────────

    public async IAsyncEnumerable<ExtractionResult> GetTextBatchAsync(
        IEnumerable<(string FileName, byte[] Content)> documents,
        int maxDegreeOfParallelism = 4,
        ExtractionOptions? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (maxDegreeOfParallelism <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxDegreeOfParallelism), "Must be > 0.");

        using var semaphore = new SemaphoreSlim(maxDegreeOfParallelism, maxDegreeOfParallelism);
        var docs = documents.ToList();
        var tasks = new Task<ExtractionResult>[docs.Count];

        for (int i = 0; i < docs.Count; i++)
        {
            var (fileName, content) = docs[i];
            cancellationToken.ThrowIfCancellationRequested();
            await semaphore.WaitAsync(cancellationToken);

            tasks[i] = Task.Run(() =>
            {
                try { return GetTextCore(fileName, content, options); }
                finally { semaphore.Release(); }
            }, cancellationToken);
        }

        foreach (var task in tasks)
            yield return await task;
    }

    // ── Discovery ────────────────────────────────────────────────────────────

    private static readonly string[] _probedExtensions =
    [
        ".pdf", ".docx", ".pptx", ".xlsx", ".xls", ".xlsm",
        ".html", ".htm", ".md", ".markdown", ".csv", ".eml", ".msg",
        ".json", ".xml", ".config", ".yaml", ".yml", ".log", ".out",
        ".cs", ".py", ".js", ".ts", ".cpp", ".h", ".java", ".ps1", ".sh",
        ".go", ".rs", ".sql", ".rb", ".php", ".swift", ".kt", ".scala",
        ".fs", ".vb", ".lua", ".r", ".m", ".dart",
        ".zip", ".odt", ".odp", ".rtf", ".epub", ".txt"
    ];

    public IReadOnlyList<string> GetSupportedExtensions()
        => _probedExtensions
            .Where(ext => _extractors.Any(e => e.CanHandle(ext) && e is not TxtExtractor))
            .OrderBy(e => e, StringComparer.OrdinalIgnoreCase)
            .ToList()
            .AsReadOnly();

    public bool IsSupported(string fileName)
    {
        var ext = Path.GetExtension(fileName);
        return _extractors.Any(e => e.CanHandle(ext));
    }

    /// <summary>
    /// Inspects the file's magic bytes and returns the most likely extension (e.g. ".pdf",
    /// ".docx"), or <c>null</c> when the format cannot be identified from content alone.
    /// </summary>
    public static string? DetectFormat(byte[] content) => ContentTypeDetector.SniffExtension(content);

    // ── Core implementations ─────────────────────────────────────────────────

    private ExtractionResult GetTextCore(string fileName, byte[] content, ExtractionOptions? options)
    {
        if (content == null || content.Length == 0)
            return ExtractionResult.Failure("File content is empty.", ExtractionErrorKind.Empty);

        var processor = Route(fileName, content);
        if (processor == null)
            return ExtractionResult.Failure($"Unsupported file type: {Path.GetExtension(fileName)}", ExtractionErrorKind.Unsupported);

        _logger.LogDebug("Extracting {FileName} using {Extractor}", fileName, processor.GetType().Name);
        var sw = Stopwatch.StartNew();
        try
        {
            var text = TextNormalizer.Normalize(processor.ExtractText(content, options));
            if (options?.MaxCharacters is int max && text.Length > max)
                text = text[..max];
            sw.Stop();
            _logger.LogDebug("Extracted {CharCount} chars from {FileName} in {ElapsedMs}ms",
                text.Length, fileName, sw.ElapsedMilliseconds);
            return ExtractionResult.Success(text, processor.GetType().Name, sw.Elapsed);
        }
        catch (Exception ex)
        {
            sw.Stop();
            _logger.LogWarning(ex, "Extraction failed for {FileName} ({Extractor}): {Message}",
                fileName, processor.GetType().Name, ex.Message);
            return ExtractionResult.Failure($"Extraction failed: {ex.Message}",
                ClassifyException(ex), processor.GetType().Name, sw.Elapsed);
        }
    }

    private ExtractionMetadataResult GetTextWithMetadataCore(string fileName, byte[] content, ExtractionOptions? options)
    {
        if (content == null || content.Length == 0)
            return ExtractionMetadataResult.Failure("File content is empty.", ExtractionErrorKind.Empty);

        var processor = Route(fileName, content);
        if (processor == null)
            return ExtractionMetadataResult.Failure($"Unsupported file type: {Path.GetExtension(fileName)}", ExtractionErrorKind.Unsupported);

        var sw = Stopwatch.StartNew();
        try
        {
            var normalizedText = TextNormalizer.Normalize(processor.ExtractText(content, options));
            if (options?.MaxCharacters is int max && normalizedText.Length > max)
                normalizedText = normalizedText[..max];
            sw.Stop();
            var metadata = BuildMetadata(fileName, content, normalizedText);
            metadata["extraction.extractor"] = processor.GetType().Name;
            metadata["extraction.durationMs"] = sw.ElapsedMilliseconds.ToString();
            return ExtractionMetadataResult.Success(normalizedText, metadata,
                processor.GetType().Name, sw.Elapsed);
        }
        catch (Exception ex)
        {
            sw.Stop();
            _logger.LogWarning(ex, "Extraction failed for {FileName} ({Extractor}): {Message}",
                fileName, processor.GetType().Name, ex.Message);
            return ExtractionMetadataResult.Failure($"Extraction failed: {ex.Message}",
                ClassifyException(ex), processor.GetType().Name, sw.Elapsed);
        }
    }

    // ── Routing ──────────────────────────────────────────────────────────────

    private ITextExtractor? Route(string fileName, byte[] content)
    {
        var extension = Path.GetExtension(fileName);
        var byExtension = _extractors.FirstOrDefault(e => e.CanHandle(extension));

        // If we found a specific (non-catch-all) extractor, use it
        if (byExtension is { } matched && matched is not TxtExtractor)
            return matched;

        // Fall back to magic-byte sniffing for better specificity
        var sniffed = ContentTypeDetector.SniffExtension(content);
        if (sniffed != null)
        {
            var byContent = _extractors.FirstOrDefault(e => e.CanHandle(sniffed) && e is not TxtExtractor);
            if (byContent != null)
            {
                _logger.LogDebug("Content sniffing detected {Sniffed} for {FileName}", sniffed, fileName);
                return byContent;
            }
        }

        return byExtension; // TxtExtractor catch-all, or null if no extractors registered
    }

    // ── Exception classification ──────────────────────────────────────────────

    private static ExtractionErrorKind ClassifyException(Exception ex)
    {
        var msg = ex.Message ?? string.Empty;
        var typeName = ex.GetType().Name;

        if (msg.Contains("password", StringComparison.OrdinalIgnoreCase) ||
            msg.Contains("encrypted", StringComparison.OrdinalIgnoreCase) ||
            msg.Contains("protected", StringComparison.OrdinalIgnoreCase) ||
            typeName.Contains("Encrypt", StringComparison.OrdinalIgnoreCase) ||
            typeName.Contains("Password", StringComparison.OrdinalIgnoreCase) ||
            typeName.Contains("Decrypt", StringComparison.OrdinalIgnoreCase))
            return ExtractionErrorKind.PasswordProtected;

        if (ex is InvalidDataException or System.IO.IOException or FormatException)
            return ExtractionErrorKind.Corrupted;

        return ExtractionErrorKind.ExtractionFailed;
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static byte[] ReadStream(Stream stream)
    {
        using var ms = new MemoryStream();
        stream.CopyTo(ms);
        return ms.ToArray();
    }

    private static async Task<byte[]> ReadStreamAsync(Stream stream, CancellationToken ct)
    {
        using var ms = new MemoryStream();
        await stream.CopyToAsync(ms, ct);
        return ms.ToArray();
    }

    private static Dictionary<string, string> BuildMetadata(string fileName, byte[] content, string normalizedText)
    {
        var metadata = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        var extension = Path.GetExtension(fileName);
        string sourceText = GetSourceText(content);
        metadata["file.name"] = fileName;
        metadata["file.extension"] = extension;
        metadata["file.bytes"] = content.Length.ToString();
        metadata["text.length"] = normalizedText.Length.ToString();
        metadata["text.lineCount"] = CountLines(normalizedText).ToString();
        metadata["text.wordCount"] = CountWords(normalizedText).ToString();

        if (extension.Equals(".md", StringComparison.OrdinalIgnoreCase) ||
            extension.Equals(".markdown", StringComparison.OrdinalIgnoreCase))
        {
            var normalizedMarkdown = MarkdownStructureExtractor.NormalizeMarkdown(sourceText);
            var headings = MarkdownStructureExtractor.BuildHeadingMap(normalizedMarkdown);
            var sections = MarkdownStructureExtractor.ExtractSections(normalizedMarkdown);
            metadata["markdown.headingCount"] = headings.Count.ToString();
            metadata["markdown.sectionCount"] = sections.Count.ToString();
            metadata["markdown.headings"] = string.Join(" | ", headings.Select(h => $"H{h.Level}:{h.Title}"));
        }

        if (extension.Equals(".json", StringComparison.OrdinalIgnoreCase))
        {
            var json = JsonSchemaValidator.NormalizeJson(sourceText);
            try
            {
                using var document = JsonDocument.Parse(json);
                var root = document.RootElement;
                metadata["json.rootKind"] = root.ValueKind.ToString();

                if (root.ValueKind == JsonValueKind.Object)
                {
                    metadata["json.topLevelPropertyCount"] = root.EnumerateObject().Count().ToString();
                    metadata["json.topLevelProperties"] = string.Join(",", root.EnumerateObject().Select(p => p.Name));
                }
                else if (root.ValueKind == JsonValueKind.Array)
                {
                    metadata["json.topLevelArrayLength"] = root.GetArrayLength().ToString();
                }
            }
            catch
            {
                metadata["json.parse"] = "failed";
            }
        }

        if (extension.Equals(".eml", StringComparison.OrdinalIgnoreCase))
            TryAddEmailMetadata(content, metadata);

        if (extension.Equals(".msg", StringComparison.OrdinalIgnoreCase))
            TryAddMsgMetadata(content, metadata);

        if (extension.Equals(".pdf", StringComparison.OrdinalIgnoreCase))
            TryAddPdfMetadata(content, metadata);

        if (extension.Equals(".docx", StringComparison.OrdinalIgnoreCase))
            TryAddDocxMetadata(content, metadata);

        if (extension.Equals(".xlsx", StringComparison.OrdinalIgnoreCase) ||
            extension.Equals(".xlsm", StringComparison.OrdinalIgnoreCase))
            TryAddXlsxMetadata(content, metadata);

        if (extension.Equals(".pptx", StringComparison.OrdinalIgnoreCase))
            TryAddPptxMetadata(content, metadata);

        return metadata;
    }

    private static string GetSourceText(byte[] content)
    {
        using var stream = new MemoryStream(content);
        using var reader = new StreamReader(stream, System.Text.Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
        return reader.ReadToEnd();
    }

    private static void TryAddEmailMetadata(byte[] content, Dictionary<string, string> metadata)
    {
        try
        {
            using var stream = new MemoryStream(content);
            var message = MimeMessage.Load(stream);

            metadata["email.subject"] = message.Subject ?? string.Empty;
            metadata["email.fromCount"] = message.From?.Count.ToString() ?? "0";
            metadata["email.toCount"] = message.To?.Count.ToString() ?? "0";

            if (message.Date != default)
                metadata["email.date"] = message.Date.ToString("O");

            var attachments = message.Attachments?.ToList() ?? new List<MimeEntity>();
            metadata["email.attachment.count"] = attachments.Count.ToString();

            if (attachments.Count > 0)
            {
                var attachmentNames = attachments
                    .Select(a => (a as MimePart)?.FileName)
                    .Where(n => !string.IsNullOrWhiteSpace(n))
                    .Select(n => n!)
                    .ToList();

                metadata["email.attachment.names"] = string.Join(",", attachmentNames);

                int specialCount = 0;
                var specialNames = new List<string>();

                foreach (var attachment in attachments)
                {
                    var part = attachment as MimePart;
                    string attachFileName = part?.FileName ?? string.Empty;
                    string mediaType = part?.ContentType?.MimeType ?? attachment.ContentType?.MimeType ?? string.Empty;
                    bool isSpecial = attachFileName.Contains("special", StringComparison.OrdinalIgnoreCase) ||
                                     mediaType.Contains("special", StringComparison.OrdinalIgnoreCase);

                    if (isSpecial)
                    {
                        specialCount++;
                        if (!string.IsNullOrWhiteSpace(attachFileName))
                            specialNames.Add(attachFileName);
                    }
                }

                metadata["email.attachment.special.count"] = specialCount.ToString();
                metadata["email.attachment.special.names"] = string.Join(",", specialNames);
            }
            else
            {
                metadata["email.attachment.special.count"] = "0";
                metadata["email.attachment.special.names"] = string.Empty;
            }
        }
        catch
        {
            metadata["email.metadata"] = "unavailable";
        }
    }

    private static void TryAddMsgMetadata(byte[] content, Dictionary<string, string> metadata)
    {
        try
        {
            using var stream = new MemoryStream(content);
            using var msg = new MsgReader.Outlook.Storage.Message(stream);

            if (!string.IsNullOrWhiteSpace(msg.Subject))
                metadata["msg.subject"] = msg.Subject;

            if (msg.Sender != null)
            {
                var from = string.IsNullOrWhiteSpace(msg.Sender.DisplayName)
                    ? msg.Sender.Email
                    : $"{msg.Sender.DisplayName} <{msg.Sender.Email}>";
                if (!string.IsNullOrWhiteSpace(from))
                    metadata["msg.from"] = from;
            }

            if (msg.SentOn.HasValue)
                metadata["msg.date"] = msg.SentOn.Value.ToString("O");
        }
        catch
        {
            metadata["msg.metadata"] = "unavailable";
        }
    }

    private static void TryAddPdfMetadata(byte[] content, Dictionary<string, string> metadata)
    {
        try
        {
            using var stream = new MemoryStream(content);
            using var doc = PdfDocument.Open(stream);
            var info = doc.Information;

            if (!string.IsNullOrWhiteSpace(info.Title))    metadata["pdf.title"]    = info.Title;
            if (!string.IsNullOrWhiteSpace(info.Author))   metadata["pdf.author"]   = info.Author;
            if (!string.IsNullOrWhiteSpace(info.Subject))  metadata["pdf.subject"]  = info.Subject;
            if (!string.IsNullOrWhiteSpace(info.Keywords)) metadata["pdf.keywords"] = info.Keywords;
            if (!string.IsNullOrWhiteSpace(info.Creator))  metadata["pdf.creator"]  = info.Creator;
            metadata["pdf.pageCount"] = doc.NumberOfPages.ToString();
        }
        catch
        {
            metadata["pdf.metadata"] = "unavailable";
        }
    }

    private static void TryAddDocxMetadata(byte[] content, Dictionary<string, string> metadata)
    {
        try
        {
            using var stream = new MemoryStream(content);
            using var doc = WordprocessingDocument.Open(stream, false);
            var props = doc.PackageProperties;

            if (!string.IsNullOrWhiteSpace(props.Title))    metadata["docx.title"]    = props.Title;
            if (!string.IsNullOrWhiteSpace(props.Creator))  metadata["docx.author"]   = props.Creator;
            if (!string.IsNullOrWhiteSpace(props.Subject))  metadata["docx.subject"]  = props.Subject;
            if (props.Created.HasValue)  metadata["docx.created"]  = props.Created.Value.ToString("O");
            if (props.Modified.HasValue) metadata["docx.modified"] = props.Modified.Value.ToString("O");
            if (!string.IsNullOrWhiteSpace(props.Revision)) metadata["docx.revision"] = props.Revision;
        }
        catch
        {
            metadata["docx.metadata"] = "unavailable";
        }
    }

    private static void TryAddXlsxMetadata(byte[] content, Dictionary<string, string> metadata)
    {
        try
        {
            using var stream = new MemoryStream(content);
            using var doc = SpreadsheetDocument.Open(stream, false);
            var props = doc.PackageProperties;

            if (!string.IsNullOrWhiteSpace(props.Title))    metadata["xlsx.title"]    = props.Title;
            if (!string.IsNullOrWhiteSpace(props.Creator))  metadata["xlsx.author"]   = props.Creator;
            if (!string.IsNullOrWhiteSpace(props.Subject))  metadata["xlsx.subject"]  = props.Subject;
            if (props.Created.HasValue)  metadata["xlsx.created"]  = props.Created.Value.ToString("O");
            if (props.Modified.HasValue) metadata["xlsx.modified"] = props.Modified.Value.ToString("O");
        }
        catch
        {
            metadata["xlsx.metadata"] = "unavailable";
        }
    }

    private static void TryAddPptxMetadata(byte[] content, Dictionary<string, string> metadata)
    {
        try
        {
            using var stream = new MemoryStream(content);
            using var doc = PresentationDocument.Open(stream, false);
            var props = doc.PackageProperties;

            if (!string.IsNullOrWhiteSpace(props.Title))    metadata["pptx.title"]    = props.Title;
            if (!string.IsNullOrWhiteSpace(props.Creator))  metadata["pptx.author"]   = props.Creator;
            if (!string.IsNullOrWhiteSpace(props.Subject))  metadata["pptx.subject"]  = props.Subject;
            if (props.Created.HasValue)  metadata["pptx.created"]  = props.Created.Value.ToString("O");
            if (props.Modified.HasValue) metadata["pptx.modified"] = props.Modified.Value.ToString("O");

            var slideCount = doc.PresentationPart?.Presentation?.SlideIdList?.ChildElements.Count;
            if (slideCount.HasValue)
                metadata["pptx.slideCount"] = slideCount.Value.ToString();
        }
        catch
        {
            metadata["pptx.metadata"] = "unavailable";
        }
    }

    private static int CountLines(string text)
    {
        if (string.IsNullOrEmpty(text)) return 0;
        return text.Split(new[] { Environment.NewLine }, StringSplitOptions.None).Length;
    }

    private static int CountWords(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return 0;
        return text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Length;
    }
}

// ── Value types ──────────────────────────────────────────────────────────────

/// <summary>
/// Categorizes the reason an extraction operation failed, enabling programmatic routing
/// without string parsing.
/// </summary>
public enum ExtractionErrorKind
{
    /// <summary>No error — extraction succeeded.</summary>
    None = 0,
    /// <summary>The input had zero bytes.</summary>
    Empty,
    /// <summary>No extractor is registered for this file type.</summary>
    Unsupported,
    /// <summary>The file is password-protected or encrypted.</summary>
    PasswordProtected,
    /// <summary>The file is corrupt or malformed.</summary>
    Corrupted,
    /// <summary>Extraction failed for another reason (see <c>ErrorMessage</c>).</summary>
    ExtractionFailed
}

/// <summary>Controls how tables are rendered in extracted text.</summary>
public enum TableMode
{
    /// <summary>Header: Value per cell — compact and LLM-readable. Default.</summary>
    KeyValue,
    /// <summary>Markdown pipe table — preserves structure for LLM consumption.</summary>
    Markdown,
    /// <summary>Comma-separated values, one row per line.</summary>
    Csv,
    /// <summary>Omit tables from the output entirely.</summary>
    Omit
}

/// <summary>
/// Controls extraction behaviour. All properties are optional; pass <c>null</c> for defaults.
/// </summary>
public sealed record ExtractionOptions
{
    public static readonly ExtractionOptions Default = new();

    /// <summary>Truncate output to this many characters after normalization. <c>null</c> = no limit.</summary>
    public int? MaxCharacters { get; init; }

    /// <summary>Stop after extracting this many pages or slides. <c>null</c> = no limit.</summary>
    public int? MaxPages { get; init; }

    /// <summary>1-based first page/slide to include. <c>null</c> = start from page 1.</summary>
    public int? StartPage { get; init; }

    /// <summary>1-based last page/slide to include (inclusive). <c>null</c> = no limit.</summary>
    public int? EndPage { get; init; }

    /// <summary>When <c>true</c>, insert <c>[Page N]</c> / <c>[Slide N]</c> markers in the output.</summary>
    public bool IncludePageMarkers { get; init; }

    /// <summary>How tables are rendered. Defaults to <see cref="TableMode.KeyValue"/>.</summary>
    public TableMode TableMode { get; init; } = TableMode.KeyValue;

    /// <summary>ZIP/EPUB decompression guard: stop after this many total bytes. Default 512 MB.</summary>
    public long MaxDecompressedBytes { get; init; } = 512L * 1024 * 1024;
}

/// <summary>
/// Immutable result DTO for a text extraction operation.
/// </summary>
public record ExtractionResult(bool IsSuccess, string Text, string? ErrorMessage)
{
    /// <summary>Name of the extractor that processed the file, if applicable.</summary>
    public string? ExtractorName { get; init; }

    /// <summary>Time taken to extract the text.</summary>
    public TimeSpan Duration { get; init; }

    /// <summary>Structured failure category; <see cref="ExtractionErrorKind.None"/> on success.</summary>
    public ExtractionErrorKind ErrorKind { get; init; } = ExtractionErrorKind.None;

    public static ExtractionResult Success(string text, string? extractorName = null, TimeSpan duration = default)
        => new(true, text, null) { ExtractorName = extractorName, Duration = duration };

    public static ExtractionResult Failure(string error,
        ExtractionErrorKind kind = ExtractionErrorKind.ExtractionFailed,
        string? extractorName = null,
        TimeSpan duration = default)
        => new(false, string.Empty, error) { ExtractorName = extractorName, Duration = duration, ErrorKind = kind };
}

/// <summary>
/// Immutable result DTO for a text extraction operation with associated metadata.
/// </summary>
public record ExtractionMetadataResult(bool IsSuccess, string Text, Dictionary<string, string> Metadata, string? ErrorMessage)
{
    /// <summary>Name of the extractor that processed the file, if applicable.</summary>
    public string? ExtractorName { get; init; }

    /// <summary>Time taken to extract the text.</summary>
    public TimeSpan Duration { get; init; }

    /// <summary>Structured failure category; <see cref="ExtractionErrorKind.None"/> on success.</summary>
    public ExtractionErrorKind ErrorKind { get; init; } = ExtractionErrorKind.None;

    public static ExtractionMetadataResult Success(string text, Dictionary<string, string> metadata,
        string? extractorName = null, TimeSpan duration = default)
        => new(true, text, metadata, null) { ExtractorName = extractorName, Duration = duration };

    public static ExtractionMetadataResult Failure(string error,
        ExtractionErrorKind kind = ExtractionErrorKind.ExtractionFailed,
        string? extractorName = null,
        TimeSpan duration = default)
        => new(false, string.Empty, new Dictionary<string, string>(), error)
            { ExtractorName = extractorName, Duration = duration, ErrorKind = kind };
}
