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
/// facade for text extraction. Routes files to format-specific extractors,
/// normalizes the result, and returns typed success/failure DTOs.
/// Register via <see cref="ServiceCollectionExtensions.AddTextExtractor(Microsoft.Extensions.DependencyInjection.IServiceCollection)"/> for DI,
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

    /// <summary>Creates the default list of built-in format extractors.</summary>
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
            new JsonExtractor(),
            new XmlExtractor(),
            new YamlExtractor(),
            new LogExtractor(),
            new CodeExtractor(),
            new OdtExtractor(),
            new RtfExtractor(),
            new ImageExtractor(),
            new TxtExtractor()
        };
        // EmlExtractor, ZipExtractor, and EpubExtractor each take a snapshot of the core
        // pipeline so they can recursively extract nested documents without circular refs.
        // Insert before TxtExtractor (the catch-all) so Route finds them first.
        core.Insert(core.Count - 1, new EmlExtractor(core));
        core.Insert(core.Count - 1, new MsgExtractor(core));
        core.Insert(core.Count - 1, new ZipExtractor(core));
        core.Insert(core.Count - 1, new EpubExtractor(core));
        return core;
    }

    // ── byte[] surface ──────────────────────────────────────────────────────

    /// <inheritdoc />
    public ExtractionResult GetText(string fileName, byte[] content)
        => GetTextCore(fileName, content, null);

    /// <inheritdoc />
    public ExtractionResult GetText(string fileName, byte[] content, ExtractionOptions? options)
        => GetTextCore(fileName, content, options);

    /// <inheritdoc />
    public ExtractionMetadataResult GetTextWithMetadata(string fileName, byte[] content)
        => GetTextWithMetadataCore(fileName, content, null);

    /// <inheritdoc />
    public ExtractionMetadataResult GetTextWithMetadata(string fileName, byte[] content, ExtractionOptions? options)
        => GetTextWithMetadataCore(fileName, content, options);

    // ── Stream surface ───────────────────────────────────────────────────────

    /// <inheritdoc />
    public ExtractionResult GetText(string fileName, Stream content)
        => GetText(fileName, content, null);

    /// <inheritdoc />
    public ExtractionResult GetText(string fileName, Stream content, ExtractionOptions? options)
        => ReadStream(content, options) is { } bytes
            ? GetTextCore(fileName, bytes, options)
            : ExtractionResult.Failure(TooLargeMessage(options), ExtractionErrorKind.TooLarge);

    /// <inheritdoc />
    public ExtractionMetadataResult GetTextWithMetadata(string fileName, Stream content)
        => GetTextWithMetadata(fileName, content, null);

    /// <inheritdoc />
    public ExtractionMetadataResult GetTextWithMetadata(string fileName, Stream content, ExtractionOptions? options)
        => ReadStream(content, options) is { } bytes
            ? GetTextWithMetadataCore(fileName, bytes, options)
            : ExtractionMetadataResult.Failure(TooLargeMessage(options), ExtractionErrorKind.TooLarge);

    // ── Async surface ────────────────────────────────────────────────────────

    /// <inheritdoc />
    public Task<ExtractionResult> GetTextAsync(string fileName, Stream content, CancellationToken cancellationToken = default)
        => GetTextAsync(fileName, content, null, cancellationToken);

    /// <inheritdoc />
    public async Task<ExtractionResult> GetTextAsync(string fileName, Stream content, ExtractionOptions? options, CancellationToken cancellationToken = default)
        => await ReadStreamAsync(content, options, cancellationToken) is { } bytes
            ? GetTextCore(fileName, bytes, options, cancellationToken)
            : ExtractionResult.Failure(TooLargeMessage(options), ExtractionErrorKind.TooLarge);

    /// <inheritdoc />
    public Task<ExtractionMetadataResult> GetTextWithMetadataAsync(string fileName, Stream content, CancellationToken cancellationToken = default)
        => GetTextWithMetadataAsync(fileName, content, null, cancellationToken);

    /// <inheritdoc />
    public async Task<ExtractionMetadataResult> GetTextWithMetadataAsync(string fileName, Stream content, ExtractionOptions? options, CancellationToken cancellationToken = default)
        => await ReadStreamAsync(content, options, cancellationToken) is { } bytes
            ? GetTextWithMetadataCore(fileName, bytes, options, cancellationToken)
            : ExtractionMetadataResult.Failure(TooLargeMessage(options), ExtractionErrorKind.TooLarge);

    // ── Batch ────────────────────────────────────────────────────────────────

    /// <inheritdoc />
    public async IAsyncEnumerable<ExtractionResult> GetTextBatchAsync(
        IEnumerable<(string FileName, byte[] Content)> documents,
        int maxDegreeOfParallelism = 4,
        ExtractionOptions? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (maxDegreeOfParallelism <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxDegreeOfParallelism), "Must be > 0.");
        ArgumentNullException.ThrowIfNull(documents);
        ValidateOptions(options);

        // At most maxDegreeOfParallelism documents parse at once; up to twice that many are started
        // ahead so a slow document at the head of the queue doesn't idle the other workers.
        int readAhead = maxDegreeOfParallelism * 2;
        using var semaphore = new SemaphoreSlim(maxDegreeOfParallelism, maxDegreeOfParallelism);
        using var stop = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var pending = new Queue<Task<ExtractionResult>>();

        // Documents take a slot strictly in input order: each one queues for the semaphore only after the document before
        // it has a slot. If later documents could race ahead, they could hold every slot while the caller waits for the
        // first result, which then waits for documents nobody is consuming yet.
        Task slotQueue = Task.CompletedTask;
        Task<ExtractionResult> Start(string fileName, byte[] content)
        {
            var slot = TakeSlotAfter(slotQueue);
            slotQueue = slot;
            return Run(slot, fileName, content);
        }

        async Task TakeSlotAfter(Task previous)
        {
            try { await previous.ConfigureAwait(false); }
            catch (OperationCanceledException) { } // the wait below then throws for this document too
            await semaphore.WaitAsync(stop.Token).ConfigureAwait(false);
        }

        // Parsers are synchronous and can run for seconds, so each extraction gets its own thread (LongRunning) instead
        // of a thread-pool thread: on a small machine, blocking pool threads would starve the continuations that hand
        // results back, and a fast document could wait behind slow ones.
        async Task<ExtractionResult> Run(Task slot, string fileName, byte[] content)
        {
            await slot.ConfigureAwait(false);
            try
            {
                return await Task.Factory.StartNew(() => GetTextCore(fileName, content, options, stop.Token), stop.Token,
                    TaskCreationOptions.LongRunning, TaskScheduler.Default).ConfigureAwait(false);
            }
            finally { semaphore.Release(); }
        }

        using var source = documents.GetEnumerator();
        try
        {
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                while (pending.Count < readAhead && source.MoveNext())
                    pending.Enqueue(Start(source.Current.FileName, source.Current.Content));

                if (pending.Count == 0)
                    yield break;

                ExtractionResult next;
                try
                {
                    next = await pending.Dequeue().ConfigureAwait(false);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    // Surface caller cancellation as OperationCanceledException, as before, rather than
                    // the TaskCanceledException of whichever worker noticed it first.
                    cancellationToken.ThrowIfCancellationRequested();
                    throw;
                }
                yield return next;
            }
        }
        finally
        {
            // Reached when the batch completes, is cancelled, fails, or the caller stops enumerating.
            // Stop outstanding work and let it finish before the semaphore is disposed.
            if (pending.Count > 0)
            {
                stop.Cancel();
                try { await Task.WhenAll(pending).ConfigureAwait(false); }
                catch (OperationCanceledException) { }
            }
        }
    }

    // ── Model input ──────────────────────────────────────────────────────────

    /// <summary>
    /// Extracts a document for an AI model as structured, provenance-labelled segments instead of one string.
    /// <paramref name="options"/> defaults to <see cref="ExtractionOptions.ForAiIngestion"/>; page markers are always
    /// turned on so segments can carry their page, slide or chapter. Withheld content is never in a segment; only the
    /// names of withheld parts are listed. The result is untrusted data: see <see cref="ModelInput"/>.
    /// </summary>
    public ModelInput GetModelInput(string fileName, byte[] content, ExtractionOptions? options = null)
    {
        var segments = new List<ContentSegment>();
        var effective = (options ?? ExtractionOptions.ForAiIngestion()) with
            { IncludePageMarkers = true, SegmentSink = segments.Add, MarkerTag = ModelInput.NewMarkerTag() };
        var result = GetTextCore(fileName, content, effective);
        return ModelInput.From(fileName, result, segments, effective.MaxCharacters, effective.MarkerTag!);
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
        ".zip", ".odt", ".odp", ".rtf", ".epub", ".txt",
        ".png", ".jpg", ".jpeg", ".jpe", ".gif", ".bmp", ".dib", ".tif", ".tiff", ".webp"
    ];

    /// <inheritdoc />
    public IReadOnlyList<string> GetSupportedExtensions()
        => _probedExtensions
            .Where(ext => _extractors.Any(e => e.CanHandle(ext) && e is not TxtExtractor))
            .OrderBy(e => e, StringComparer.OrdinalIgnoreCase)
            .ToList()
            .AsReadOnly();

    /// <inheritdoc />
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

    private ExtractionResult GetTextCore(string fileName, byte[] content, ExtractionOptions? options,
        CancellationToken callerToken = default)
    {
        options ??= ExtractionOptions.Default;
        ValidateOptions(options);
        if (CheckBeforeParsing(fileName, content, options, out var processor) is { } rejected)
            return ExtractionResult.Failure(rejected.Message, rejected.Kind, processor?.GetType().Name);

        var warnings = new List<ExtractionWarning>();
        var safety = new SafetyCollector();
        var mismatch = ContentTypeDetector.DetectMismatch(Path.GetExtension(fileName), content);
        using var cancellation = new ExtractionCancellationScope(options, callerToken);
        var effectiveOptions = WithWarningCapture(options, warnings, safety, out var callerSinks) with { CancellationToken = cancellation.Token };

        _logger.LogDebug("Extracting {FileName} using {Extractor}", fileName, processor!.GetType().Name);
        var sw = Stopwatch.StartNew();
        try
        {
            var text = ExtractWithContentFallback(fileName, content, effectiveOptions, warnings, safety, callerSinks, ref processor, out var withheld);
            sw.Stop();
            ReportMismatch(fileName, mismatch, effectiveOptions);
            withheld ??= RejectIfRequired(fileName, text, options.Safety, safety, effectiveOptions);
            if (withheld != null)
            {
                _logger.LogWarning("Safety gate withheld {FileName}: {Reason}", fileName, withheld);
                return ExtractionResult.Failure(WithheldMessage(withheld), ExtractionErrorKind.Quarantined,
                    processor.GetType().Name, sw.Elapsed) with { Warnings = warnings, Findings = safety.Findings, Quarantine = safety.Quarantine, SafetyVerdict = SafetyVerdict.Quarantined };
            }
            text = Truncate(text, options);
            _logger.LogDebug("Extracted {CharCount} chars from {FileName} in {ElapsedMs}ms",
                text.Length, fileName, sw.ElapsedMilliseconds);
            return ExtractionResult.Success(text, processor.GetType().Name, sw.Elapsed, warnings)
                with { Findings = safety.Findings, Quarantine = safety.Quarantine, SafetyVerdict = safety.Verdict(options.Safety) };
        }
        catch (OperationCanceledException) when (cancellation.CallerCancelled)
        {
            throw;
        }
        catch (Exception ex)
        {
            sw.Stop();
            var (message, kind) = DescribeFailure(ex, cancellation, options, fileName, processor);
            ReportMismatch(fileName, mismatch, effectiveOptions);
            return ExtractionResult.Failure(message, kind, processor.GetType().Name, sw.Elapsed)
                with { Warnings = warnings, Findings = safety.Findings, SafetyVerdict = safety.Verdict(options.Safety) };
        }
    }

    private ExtractionMetadataResult GetTextWithMetadataCore(string fileName, byte[] content, ExtractionOptions? options,
        CancellationToken callerToken = default)
    {
        options ??= ExtractionOptions.Default;
        ValidateOptions(options);
        if (CheckBeforeParsing(fileName, content, options, out var processor) is { } rejected)
            return ExtractionMetadataResult.Failure(rejected.Message, rejected.Kind, processor?.GetType().Name);

        var warnings = new List<ExtractionWarning>();
        var safety = new SafetyCollector();
        var mismatch = ContentTypeDetector.DetectMismatch(Path.GetExtension(fileName), content);
        using var cancellation = new ExtractionCancellationScope(options, callerToken);
        var effectiveOptions = WithWarningCapture(options, warnings, safety, out var callerSinks) with { CancellationToken = cancellation.Token };

        var sw = Stopwatch.StartNew();
        try
        {
            var normalizedText = ExtractWithContentFallback(fileName, content, effectiveOptions, warnings, safety, callerSinks, ref processor!, out var withheld);
            sw.Stop();
            ReportMismatch(fileName, mismatch, effectiveOptions);
            withheld ??= RejectIfRequired(fileName, normalizedText, options.Safety, safety, effectiveOptions);
            if (withheld != null)
            {
                _logger.LogWarning("Safety gate withheld {FileName}: {Reason}", fileName, withheld);
                return ExtractionMetadataResult.Failure(WithheldMessage(withheld), ExtractionErrorKind.Quarantined,
                    processor!.GetType().Name, sw.Elapsed) with { Warnings = warnings, Findings = safety.Findings, Quarantine = safety.Quarantine, SafetyVerdict = SafetyVerdict.Quarantined };
            }
            normalizedText = Truncate(normalizedText, options);
            var metadata = BuildMetadata(fileName, content, normalizedText, processor);
            if (mismatch != null)
                metadata["file.detectedFormat"] = mismatch;
            metadata["extraction.extractor"] = processor!.GetType().Name;
            metadata["extraction.durationMs"] = sw.ElapsedMilliseconds.ToString();
            if (warnings.Count > 0)
            {
                metadata["extraction.warningCount"] = warnings.Count.ToString();
                metadata["extraction.warnings"] = string.Join(" | ", warnings.Select(w => $"{w.Code}:{w.Source}:{w.Message}"));
            }
            if (safety.Findings.Count > 0)
            {
                metadata["safety.findingCount"] = safety.Findings.Count.ToString();
                metadata["safety.maxSeverity"] = safety.Findings.Max(f => f.Severity).ToString();
            }
            if (safety.Quarantine != null)
                metadata["safety.withheldParts"] = safety.Quarantine.Parts.Count.ToString();
            return ExtractionMetadataResult.Success(normalizedText, metadata,
                processor.GetType().Name, sw.Elapsed, warnings) with { Findings = safety.Findings, Quarantine = safety.Quarantine, SafetyVerdict = safety.Verdict(options.Safety) };
        }
        catch (OperationCanceledException) when (cancellation.CallerCancelled)
        {
            throw;
        }
        catch (Exception ex)
        {
            sw.Stop();
            var (message, kind) = DescribeFailure(ex, cancellation, options, fileName, processor!);
            ReportMismatch(fileName, mismatch, effectiveOptions);
            return ExtractionMetadataResult.Failure(message, kind, processor!.GetType().Name, sw.Elapsed)
                with { Warnings = warnings, Findings = safety.Findings, SafetyVerdict = safety.Verdict(options.Safety) };
        }
    }

    /// <summary>
    /// Checks that run before any parser touches the bytes: empty input, input size,
    /// routing, and binary content that only the plain-text fallback would accept.
    /// </summary>
    private (string Message, ExtractionErrorKind Kind)? CheckBeforeParsing(
        string fileName, byte[] content, ExtractionOptions options, out ITextExtractor? processor)
    {
        processor = null;

        if (content == null || content.Length == 0)
            return ("File content is empty.", ExtractionErrorKind.Empty);

        if (options.MaxInputBytes is long maxInput && content.Length > maxInput)
            return ($"File is {content.Length} bytes; the limit is {maxInput} bytes (ExtractionOptions.MaxInputBytes).",
                ExtractionErrorKind.TooLarge);

        processor = Route(fileName, content);
        if (processor == null)
            return ($"Unsupported file type: {Path.GetExtension(fileName)}", ExtractionErrorKind.Unsupported);

        if (processor is TxtExtractor && ContentTypeDetector.IsImageExtension(ContentTypeDetector.SniffExtension(content)))
            return ($"File is an image ({ContentTypeDetector.SniffExtension(content)}); its text, if any, needs OCR: {fileName}",
                ExtractionErrorKind.NeedsOcr);

        if (processor is TxtExtractor && TxtExtractor.IsBinary(content))
            return ($"File appears to be binary and has no extractable text: {fileName}", ExtractionErrorKind.Binary);

        return null;
    }

    /// <summary>
    /// Runs <paramref name="processor"/>. If it was chosen by file extension and fails, and the content's
    /// magic bytes point to a different extractor (a DOCX named <c>.pdf</c>, say), retries once with that
    /// extractor. <paramref name="processor"/> is updated to the extractor that produced the result.
    /// A file disguised under another extension is a common malware-delivery trick, so the retry is
    /// deliberately narrow: it is skipped when <see cref="ExtractionOptions.RetryWithDetectedFormat"/> is off,
    /// when the first attempt hit a safety limit, and when the detected format is a container
    /// (ZIP, EML, EPUB) that would unpack further content. It shares the first attempt's timeout.
    /// <paramref name="withheld"/> is the safety gate's reason for withholding the whole document, or <c>null</c>.
    /// When a retry is possible, the caller's sinks are buffered during the first attempt: delivered if it succeeds
    /// (or fails for good), discarded if it is retried, so the sinks only ever see what the final result contains.
    /// </summary>
    private string ExtractWithContentFallback(string fileName, byte[] content, ExtractionOptions options,
        List<ExtractionWarning> warnings, SafetyCollector safety, CallerSinks callerSinks, ref ITextExtractor processor,
        out string? withheld)
    {
        callerSinks.Buffering = options.RetryWithDetectedFormat && SniffedAlternative(content, processor) != null;
        try
        {
            return ExtractGated(processor, content, options, fileName, out withheld);
        }
        catch (Exception ex) when (options.RetryWithDetectedFormat
                                   && ex is not (OperationCanceledException or ExtractionLimitExceededException)
                                   && SniffedAlternative(content, processor) is { } alternative)
        {
            _logger.LogWarning(ex, "{Extractor} failed on {FileName}; content looks like {Alternative}, retrying",
                processor.GetType().Name, fileName, alternative.GetType().Name);
            warnings.Clear(); // warnings and findings from the failed attempt describe the wrong format
            safety.Clear();
            callerSinks.Discard();
            processor = alternative;
            return ExtractGated(alternative, content, options, fileName, out withheld);
        }
        finally
        {
            callerSinks.Flush();
        }
    }

    /// <summary>
    /// Extracts and runs the safety gate over the whole text: normalised, and before
    /// <see cref="ExtractionOptions.MaxCharacters"/> so nothing escapes the scan by truncation.
    /// Containers that gate their own parts are not scanned again.
    /// </summary>
    private static string ExtractGated(ITextExtractor processor, byte[] content, ExtractionOptions options, string fileName,
        out string? withheld)
    {
        using var scope = new SafetyScope(options, fileName);
        var text = Extract(processor, content, scope.Options);
        withheld = scope.Complete(text, fileName, scanText: processor is not IPartGatedContainer);
        return text;
    }

    /// <summary>
    /// <see cref="SafetyPolicy.RejectAt"/>: when any finding (container parts included) reaches it, the whole document
    /// is withheld, not just the part. Returns the reason, or <c>null</c>.
    /// </summary>
    private static string? RejectIfRequired(string fileName, string text, SafetyPolicy policy, SafetyCollector safety,
        ExtractionOptions effectiveOptions)
    {
        var rejecting = safety.Findings.Where(f => policy.Rejects(f.Severity)).ToList();
        if (rejecting.Count == 0) return null;
        effectiveOptions.WithheldSink?.Invoke(new QuarantinedPart(fileName, text, safety.Findings));
        return "rejected: " + SafetyGate.Describe(rejecting);
    }

    private static string Truncate(string text, ExtractionOptions options)
        => options.MaxCharacters is int max && text.Length > max ? text[..max] : text;

    private static string WithheldMessage(string reason)
        => $"Withheld by the safety gate ({reason}). The text is in Quarantine.GetWithheldText(); do not pass it to an AI model unchecked.";

    private void ReportMismatch(string fileName, string? detected, ExtractionOptions options)
    {
        if (detected == null) return;
        _logger.LogWarning("File {FileName} has extension {Extension} but its content is {Detected}",
            fileName, Path.GetExtension(fileName), detected);
        options.WarningSink?.Invoke(new ExtractionWarning(ExtractionWarningCodes.ContentTypeMismatch, fileName,
            $"Extension '{Path.GetExtension(fileName)}' does not match the content, which is '{detected}'."));
    }

    private ITextExtractor? SniffedAlternative(byte[] content, ITextExtractor failed)
    {
        var sniffed = ContentTypeDetector.SniffExtension(content);
        if (sniffed == null) return null;
        var alternative = _extractors.FirstOrDefault(e => e.CanHandle(sniffed) && e is not TxtExtractor);
        if (alternative is null or ZipExtractor or EmlExtractor or MsgExtractor or EpubExtractor || ReferenceEquals(alternative, failed))
            return null;
        return alternative;
    }

    private static string Extract(ITextExtractor processor, byte[] content, ExtractionOptions options)
    {
        options.CancellationToken.ThrowIfCancellationRequested();
        var text = processor.ExtractText(content, options);
        // Some parsers (JSON, YAML, XML, Markdown, MSG libraries) cannot be interrupted. Checking again here means a
        // parse that ran past the timeout is reported as TimedOut, not as a normal success.
        options.CancellationToken.ThrowIfCancellationRequested();
        return TextNormalizer.Normalize(text, preserveSpacing: processor is CodeExtractor);
    }

    private (string Message, ExtractionErrorKind Kind) DescribeFailure(Exception ex, ExtractionCancellationScope cancellation,
        ExtractionOptions options, string fileName, ITextExtractor processor)
    {
        if (ex is OperationCanceledException && cancellation.TimedOut)
        {
            _logger.LogWarning("Extraction timed out for {FileName} ({Extractor}) after {Timeout}",
                fileName, processor.GetType().Name, options.Timeout);
            return ($"Extraction timed out after {options.Timeout} (ExtractionOptions.Timeout).", ExtractionErrorKind.TimedOut);
        }

        _logger.LogWarning(ex, "Extraction failed for {FileName} ({Extractor}): {Message}",
            fileName, processor.GetType().Name, ex.Message);
        return ($"Extraction failed: {ex.Message}", ClassifyException(ex));
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

    // ── Options validation ───────────────────────────────────────────────────

    /// <summary>Rejects option values that can only be a caller mistake, before any work starts.</summary>
    private static void ValidateOptions(ExtractionOptions? options)
    {
        if (options == null) return;

        static void Require(bool ok, string property, object? value, string rule)
        {
            if (!ok)
                throw new ArgumentOutOfRangeException(nameof(options), value,
                    $"ExtractionOptions.{property} {rule}.");
        }

        Require(options.MaxCharacters is null or >= 0, nameof(ExtractionOptions.MaxCharacters), options.MaxCharacters, "must be >= 0");
        Require(options.MaxPages is null or >= 1, nameof(ExtractionOptions.MaxPages), options.MaxPages, "must be >= 1");
        Require(options.StartPage is null or >= 1, nameof(ExtractionOptions.StartPage), options.StartPage, "must be >= 1");
        Require(options.EndPage is null or >= 1, nameof(ExtractionOptions.EndPage), options.EndPage, "must be >= 1");
        Require(options.EndPage is null || options.EndPage >= (options.StartPage ?? 1),
            nameof(ExtractionOptions.EndPage), options.EndPage, "must be >= StartPage");
        Require(options.MaxDecompressedBytes >= 0, nameof(ExtractionOptions.MaxDecompressedBytes), options.MaxDecompressedBytes, "must be >= 0");
        Require(options.MaxContainerEntries >= 0, nameof(ExtractionOptions.MaxContainerEntries), options.MaxContainerEntries, "must be >= 0");
        Require(options.MaxInputBytes is null or >= 0, nameof(ExtractionOptions.MaxInputBytes), options.MaxInputBytes, "must be >= 0");
        Require(options.MaxOcrImages >= 0, nameof(ExtractionOptions.MaxOcrImages), options.MaxOcrImages, "must be >= 0");
        Require(options.MaxImagePixels >= 0, nameof(ExtractionOptions.MaxImagePixels), options.MaxImagePixels, "must be >= 0");
        Require(options.Timeout is null || options.Timeout > TimeSpan.Zero || options.Timeout == System.Threading.Timeout.InfiniteTimeSpan,
            nameof(ExtractionOptions.Timeout), options.Timeout, "must be positive or Timeout.InfiniteTimeSpan");
    }

    // ── Exception classification ──────────────────────────────────────────────

    private static ExtractionErrorKind ClassifyException(Exception ex)
    {
        var kind = ClassifySingleException(ex);
        // Extractors may wrap the parser's exception (PdfExtractor does); the cause is more specific.
        if (kind == ExtractionErrorKind.ExtractionFailed && ex.InnerException is { } inner)
            kind = ClassifyException(inner);
        return kind;
    }

    private static ExtractionErrorKind ClassifySingleException(Exception ex)
    {
        if (ex is ExtractionLimitExceededException)
            return ExtractionErrorKind.LimitExceeded;
        if (ex is OcrUnavailableException)
            return ExtractionErrorKind.NeedsOcr;

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

    /// <summary>
    /// Buffers <paramref name="stream"/>, stopping as soon as it passes
    /// <see cref="ExtractionOptions.MaxInputBytes"/>. Returns <c>null</c> when the limit is exceeded.
    /// </summary>
    private static byte[]? ReadStream(Stream stream, ExtractionOptions? options)
    {
        ValidateOptions(options);
        long? limit = (options ?? ExtractionOptions.Default).MaxInputBytes;
        if (limit is long known && stream.CanSeek && stream.Length - stream.Position > known)
            return null;

        // A synchronous read cannot be interrupted, but the token is checked between reads so a stalled
        // or endless stream stops at the next chunk.
        var token = (options ?? ExtractionOptions.Default).CancellationToken;
        using var ms = new MemoryStream();
        var buffer = new byte[81920];
        int read;
        while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
        {
            token.ThrowIfCancellationRequested();
            if (limit is long max && ms.Length + read > max)
                return null;
            ms.Write(buffer, 0, read);
        }
        return ms.ToArray();
    }

    private static string TooLargeMessage(ExtractionOptions? options)
        => $"File exceeds the limit of {(options ?? ExtractionOptions.Default).MaxInputBytes} bytes (ExtractionOptions.MaxInputBytes).";

    /// <summary>Options whose sinks collect into this extraction's lists and forward to the caller's sinks.</summary>
    private static ExtractionOptions WithWarningCapture(ExtractionOptions? options, List<ExtractionWarning> warnings,
        SafetyCollector safety, out CallerSinks callerSinks)
    {
        options ??= ExtractionOptions.Default;
        var sinks = callerSinks = new CallerSinks(options.WarningSink, options.FindingSink);

        void Warn(ExtractionWarning warning)
        {
            warnings.Add(warning);
            sinks.Warning(warning);
        }

        return options with
        {
            SharedBudget = options.ShareContainerBudget ? new ContainerBudget() : null,
            WarningSink = Warn,
            FindingSink = finding =>
            {
                safety.Add(finding);
                Warn(new ExtractionWarning(ExtractionWarningCodes.SafetyFinding, finding.Source,
                    $"{finding.Code} ({finding.Severity}{(finding.Withheld ? ", withheld" : "")}): {finding.Reason}"));
                sinks.Finding(finding);
            },
            WithheldSink = part =>
            {
                safety.Add(part);
                Warn(new ExtractionWarning(ExtractionWarningCodes.ContentWithheld, part.Source,
                    $"Withheld by the safety gate: {SafetyGate.Describe(part.Findings.Any(f => f.Withheld) ? part.Findings.Where(f => f.Withheld) : part.Findings)}"));
            },
        };
    }

    /// <summary>
    /// The caller's <see cref="ExtractionOptions.WarningSink"/> and <see cref="ExtractionOptions.FindingSink"/>, called live
    /// except while <see cref="Buffering"/>: then notifications wait for <see cref="Flush"/> or are dropped by <see cref="Discard"/>.
    /// </summary>
    private sealed class CallerSinks(Action<ExtractionWarning>? warningSink, Action<SafetyFinding>? findingSink)
    {
        private readonly List<object> _pending = [];

        public bool Buffering { get; set; }

        public void Warning(ExtractionWarning warning)
        {
            if (Buffering) _pending.Add(warning);
            else warningSink?.Invoke(warning);
        }

        public void Finding(SafetyFinding finding)
        {
            if (Buffering) _pending.Add(finding);
            else findingSink?.Invoke(finding);
        }

        public void Flush()
        {
            Buffering = false;
            var pending = _pending.ToArray();
            _pending.Clear();
            foreach (var item in pending)
            {
                if (item is ExtractionWarning w) warningSink?.Invoke(w);
                else if (item is SafetyFinding f) findingSink?.Invoke(f);
            }
        }

        public void Discard()
        {
            _pending.Clear();
            Buffering = false;
        }
    }

    /// <summary>Findings and withheld parts collected over one extraction.</summary>
    private sealed class SafetyCollector
    {
        private readonly List<SafetyFinding> _findings = [];
        private readonly List<QuarantinedPart> _withheld = [];

        public IReadOnlyList<SafetyFinding> Findings => _findings.ToArray();
        public QuarantinedContent? Quarantine => _withheld.Count == 0 ? null : new QuarantinedContent(_withheld.ToArray());

        public void Add(SafetyFinding finding) => _findings.Add(finding);
        public void Add(QuarantinedPart part) => _withheld.Add(part);

        public void Clear()
        {
            _findings.Clear();
            _withheld.Clear();
        }

        /// <summary>The verdict for a result that was not withheld as a whole.</summary>
        public SafetyVerdict Verdict(SafetyPolicy policy)
        {
            if (_withheld.Count > 0) return SafetyVerdict.PartsWithheld;
            if (_findings.Any(f => policy.RequiresReview(f.Severity))) return SafetyVerdict.ReviewRequired;
            return _findings.Count > 0 ? SafetyVerdict.Flagged : SafetyVerdict.NoKnownSignals;
        }
    }

    private static async Task<byte[]?> ReadStreamAsync(Stream stream, ExtractionOptions? options, CancellationToken ct)
    {
        ValidateOptions(options);
        long? limit = (options ?? ExtractionOptions.Default).MaxInputBytes;
        if (limit is long known && stream.CanSeek && stream.Length - stream.Position > known)
            return null;

        // Both the method's token and ExtractionOptions.CancellationToken cancel the read itself.
        var optionsToken = (options ?? ExtractionOptions.Default).CancellationToken;
        using var linked = optionsToken.CanBeCanceled && ct.CanBeCanceled
            ? CancellationTokenSource.CreateLinkedTokenSource(optionsToken, ct)
            : null;
        var token = linked?.Token ?? (optionsToken.CanBeCanceled ? optionsToken : ct);

        using var ms = new MemoryStream();
        var buffer = new byte[81920];
        int read;
        while ((read = await stream.ReadAsync(buffer, token)) > 0)
        {
            if (limit is long max && ms.Length + read > max)
                return null;
            ms.Write(buffer, 0, read);
        }
        return ms.ToArray();
    }

    private static Dictionary<string, string> BuildMetadata(string fileName, byte[] content, string normalizedText,
        ITextExtractor processor)
    {
        var metadata = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        // Format-specific metadata follows the extractor that produced the text, so a PDF named
        // upload.txt still gets pdf.* keys. Custom extractors fall back to the file extension.
        var extension = processor switch
        {
            PdfExtractor => ".pdf",
            DocxExtractor => ".docx",
            PptxExtractor => ".pptx",
            ExcelExtractor when ExtractionGuard.IsZip(content) => ".xlsx",
            EmlExtractor => ".eml",
            MsgExtractor => ".msg",
            MarkdownExtractor => ".md",
            JsonExtractor => ".json",
            _ => Path.GetExtension(fileName),
        };
        // Only Markdown and JSON metadata need the source as text; decoding a large PDF or image as a string would
        // allocate for nothing.
        metadata["file.name"] = fileName;
        metadata["file.extension"] = Path.GetExtension(fileName);
        metadata["file.bytes"] = content.Length.ToString();
        metadata["text.length"] = normalizedText.Length.ToString();
        metadata["text.lineCount"] = CountLines(normalizedText).ToString();
        metadata["text.wordCount"] = CountWords(normalizedText).ToString();

        if (extension.Equals(".md", StringComparison.OrdinalIgnoreCase) ||
            extension.Equals(".markdown", StringComparison.OrdinalIgnoreCase))
        {
            var normalizedMarkdown = MarkdownStructureExtractor.NormalizeMarkdown(GetSourceText(content));
            var headings = MarkdownStructureExtractor.BuildHeadingMap(normalizedMarkdown);
            var sections = MarkdownStructureExtractor.ExtractSections(normalizedMarkdown);
            metadata["markdown.headingCount"] = headings.Count.ToString();
            metadata["markdown.sectionCount"] = sections.Count.ToString();
            metadata["markdown.headings"] = string.Join(" | ", headings.Select(h => $"H{h.Level}:{h.Title}"));
        }

        if (extension.Equals(".json", StringComparison.OrdinalIgnoreCase))
        {
            var json = JsonSchemaValidator.NormalizeJson(GetSourceText(content));
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
    ExtractionFailed,
    /// <summary>
    /// The file is binary (an image, executable, unknown archive, ...) and no extractor for its
    /// format is registered, so there is no text to extract. Previously reported as an empty success.
    /// </summary>
    Binary,
    /// <summary>The input is larger than <see cref="ExtractionOptions.MaxInputBytes"/>; it was not parsed.</summary>
    TooLarge,
    /// <summary>
    /// The document would exceed a safety limit while being parsed, for example an Office file that
    /// expands past <see cref="ExtractionOptions.MaxDecompressedBytes"/> or has more parts than
    /// <see cref="ExtractionOptions.MaxContainerEntries"/>.
    /// </summary>
    LimitExceeded,
    /// <summary>Parsing took longer than <see cref="ExtractionOptions.Timeout"/> and was stopped.</summary>
    TimedOut,
    /// <summary>The file is an image (recognised by content); any text in it can only be read with OCR.</summary>
    NeedsOcr,
    /// <summary>
    /// The safety gate withheld the document (see <see cref="ExtractionOptions.Safety"/>): <c>Text</c> is empty, the
    /// reasons are in <c>Findings</c>, and the text is reachable only through <c>Quarantine.GetWithheldText()</c>.
    /// </summary>
    Quarantined
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
    /// <summary>Default extraction options.</summary>
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

    /// <summary>
    /// Which table rows are header rows (HTML, DOCX, ODT). Default <see cref="TableHeaderMode.Auto"/>: only rows the
    /// format marks as headers, so an unmarked first row stays data. <see cref="TableHeaderMode.FirstRow"/> restores
    /// the pre-2.0.5 behaviour.
    /// </summary>
    public TableHeaderMode TableHeaders { get; init; } = TableHeaderMode.Auto;

    /// <summary>
    /// Decompression guard. ZIP, EPUB and EML attachments stop (with a warning) after this many total bytes;
    /// DOCX, XLSX/XLSM, PPTX, ODT and ODP files that would expand past it fail with
    /// <see cref="ExtractionErrorKind.LimitExceeded"/> before they are parsed. Default 512 MB.
    /// </summary>
    public long MaxDecompressedBytes { get; init; } = ExtractionGuard.DefaultMaxDecompressedBytes;

    /// <summary>
    /// Entry guard. ZIP, EPUB and EML stop (with a warning) after this many container entries;
    /// Office/OpenDocument files with more parts fail with <see cref="ExtractionErrorKind.LimitExceeded"/>. Default 10,000.
    /// </summary>
    public int MaxContainerEntries { get; init; } = ExtractionGuard.DefaultMaxContainerEntries;

    /// <summary>
    /// Inputs larger than this many bytes are rejected with <see cref="ExtractionErrorKind.TooLarge"/>
    /// before parsing. Stream input stops being read as soon as it passes the limit. Default 256 MB;
    /// <c>null</c> = no limit.
    /// </summary>
    public long? MaxInputBytes { get; init; } = 256L * 1024 * 1024;

    /// <summary>
    /// Stops parsing after this long and returns <see cref="ExtractionErrorKind.TimedOut"/>. The clock starts
    /// when parsing starts (after any stream has been read). <c>null</c> = no timeout.
    /// </summary>
    public TimeSpan? Timeout { get; init; }

    /// <summary>
    /// When the extractor chosen by file extension fails and the content's magic bytes identify a different
    /// format, retry once with that format's extractor and report
    /// <see cref="ExtractionWarningCodes.ContentTypeMismatch"/>. Never retries into ZIP, EML or EPUB,
    /// or after a safety limit was hit. Default <c>true</c>; set <c>false</c> to fail on the first attempt.
    /// </summary>
    public bool RetryWithDetectedFormat { get; init; } = true;

    /// <summary>
    /// OCR backend for images and scanned PDF pages (for example <c>TesseractOcrEngine</c> from
    /// <c>Transformations.Text.Ocr</c>, part of this package). <c>null</c> = no OCR: images get stub records / <see cref="ExtractionErrorKind.NeedsOcr"/>.
    /// </summary>
    public IOcrEngine? OcrEngine { get; init; }

    /// <summary>
    /// Images larger than this many pixels (read from the header, before decoding) are not OCR'd and fail with
    /// <see cref="ExtractionErrorKind.LimitExceeded"/>. Default 64 MP; an A4 page scanned at 600 dpi is about 35 MP.
    /// </summary>
    public long MaxImagePixels { get; init; } = 64_000_000;

    /// <summary>
    /// With an available <see cref="OcrEngine"/>, also read images embedded in documents: DOCX, PPTX, XLSX, ODT/ODP,
    /// PDF pages that have text, and HTML <c>data:</c> images. Images under 100×40 pixels (icons) are skipped.
    /// Default <c>true</c>.
    /// </summary>
    public bool OcrEmbeddedImages { get; init; } = true;

    /// <summary>
    /// Most embedded images OCR'd per document (OCR takes around a second per page-sized image). Images over the limit
    /// are reported with <see cref="ExtractionWarningCodes.NeedsOcr"/>. Default 20.
    /// </summary>
    public int MaxOcrImages { get; init; } = 20;

    /// <summary>
    /// Cancels extraction. The built-in extractors check it between pages, slides, rows, paragraphs and
    /// archive entries, so a cancelled parse actually stops rather than running on in the background.
    /// Cancellation throws <see cref="OperationCanceledException"/>. Custom extractors should check it too.
    /// </summary>
    public CancellationToken CancellationToken { get; init; }

    /// <summary>
    /// Optional callback invoked when extraction skips part of a container or recovers from a non-fatal issue. It is
    /// called as extraction goes, with the same warnings the result carries. When the first extractor may be retried
    /// with the content-detected format (<see cref="RetryWithDetectedFormat"/>), that attempt's warnings are held back
    /// until it succeeds, and dropped if it is retried, so the callback never reports a failed attempt.
    /// </summary>
    public Action<ExtractionWarning>? WarningSink { get; init; }

    /// <summary>
    /// Safety gate: detectors for content aimed at manipulating an AI reader, and when to withhold it.
    /// Default: on, quarantine at <see cref="SafetySeverity.High"/>. <see cref="SafetyPolicy.Off"/> disables it.
    /// </summary>
    public SafetyPolicy Safety { get; init; } = SafetyPolicy.Default;

    /// <summary>What to do with text the document hides from human readers. Default <see cref="HiddenTextMode.Mark"/>.</summary>
    public HiddenTextMode HiddenText { get; init; } = HiddenTextMode.Mark;

    /// <summary>
    /// Optional callback invoked for each <see cref="SafetyFinding"/>, including findings in container parts, with the
    /// same findings the result carries (held back for a first attempt that may be retried, like <see cref="WarningSink"/>).
    /// </summary>
    public Action<SafetyFinding>? FindingSink { get; init; }

    /// <summary>Receives parts the safety gate withheld, with their text. Set by the facade.</summary>
    internal Action<QuarantinedPart>? WithheldSink { get; init; }

    /// <summary>
    /// When <c>true</c>, ZIP, EML and EPUB content nested inside each other, and Office/OpenDocument packages inside
    /// them, share one <see cref="MaxDecompressedBytes"/> and <see cref="MaxContainerEntries"/> budget for the whole
    /// extraction instead of getting a fresh budget per container. Default <c>false</c> (limits apply per document).
    /// </summary>
    public bool ShareContainerBudget { get; init; }

    /// <summary>The budget shared by nested containers when <see cref="ShareContainerBudget"/> is on. Set by the facade.</summary>
    internal ContainerBudget? SharedBudget { get; init; }

    /// <summary>Path of the unit being extracted, from the top-level file down. Set by <see cref="SafetyScope"/>.</summary>
    internal string? PartPath { get; init; }

    /// <summary>Receives each extracted content segment with its provenance. Set by <see cref="TextExtractor.GetModelInput"/>.</summary>
    internal Action<ContentSegment>? SegmentSink { get; init; }

    /// <summary>Tag on the marker lines extractors write, so they can be told from marker-shaped document text. Set by the facade.</summary>
    internal string? MarkerTag { get; init; }

    /// <summary>
    /// Options for untrusted documents whose text goes to an AI model (RAG ingestion, agents):
    /// <see cref="SafetyPolicy.AiIngestion"/> (quarantine at High, review at Medium), hidden text dropped from
    /// <c>Text</c> (<see cref="HiddenTextMode.Drop"/>), finite limits (64 MB input, 256 MB decompressed and 1,000 entries
    /// shared across nested containers, 5,000,000 characters, 2,000 pages, 60 s) and the default image limit.
    /// Change any of them with <c>with</c>. The text is still untrusted: no finding does not mean safe.
    /// </summary>
    public static ExtractionOptions ForAiIngestion() => new()
    {
        Safety = SafetyPolicy.AiIngestion,
        HiddenText = HiddenTextMode.Drop,
        MaxInputBytes = 64L * 1024 * 1024,
        MaxDecompressedBytes = 256L * 1024 * 1024,
        MaxContainerEntries = 1_000,
        MaxCharacters = 5_000_000,
        MaxPages = 2_000,
        Timeout = TimeSpan.FromSeconds(60),
        ShareContainerBudget = true,
    };
}

/// <summary>
/// Describes a non-fatal extraction issue such as a skipped archive entry, unsupported
/// attachment, or decompression limit. Warnings indicate partial success.
/// </summary>
public sealed record ExtractionWarning(string Code, string Source, string Message);

/// <summary>
/// Stable warning codes emitted for recoverable, non-fatal extraction issues.
/// Callers can switch on these constants to decide whether to accept, retry, reject,
/// or manually review a partially extracted document.
/// </summary>
public static class ExtractionWarningCodes
{
    /// <summary>A container manifest, such as EPUB META-INF/container.xml, was missing or unreadable.</summary>
    public const string ContainerManifestMissing = "container.manifestMissing";

    /// <summary>A container spine, manifest, or ordered entry list was empty or unreadable.</summary>
    public const string ContainerSpineEmpty = "container.spineEmpty";

    /// <summary>A container entry was referenced by metadata but was not present in the container.</summary>
    public const string ContainerEntryMissing = "container.entryMissing";

    /// <summary>A container had more entries than <see cref="ExtractionOptions.MaxContainerEntries"/> allows.</summary>
    public const string ContainerEntryLimit = "container.entryLimit";

    /// <summary>An entry was skipped because it would exceed <see cref="ExtractionOptions.MaxDecompressedBytes"/>.</summary>
    public const string ContainerByteLimit = "container.byteLimit";

    /// <summary>An entry was skipped because no safe extractor is registered for its type.</summary>
    public const string ContainerUnsupportedEntry = "container.unsupportedEntry";

    /// <summary>An entry was selected for extraction but failed independently of the rest of the document.</summary>
    public const string ContainerEntryFailed = "container.entryFailed";

    /// <summary>
    /// The file's extension contradicts its content's binary signature (a DOCX named <c>invoice.pdf</c>, a PDF
    /// named <c>notes.txt</c>). Reported whether or not extraction succeeded; the detected extension is in
    /// <c>Message</c> and in the <c>file.detectedFormat</c> metadata key. Disguised files are a common way to
    /// deliver malware by email, so treat this as a reason to quarantine or scan, not just as a curiosity.
    /// </summary>
    public const string ContentTypeMismatch = "content.typeMismatch";

    /// <summary>
    /// A part has no text layer and its text, if any, can only be read with OCR: an image attachment or ZIP entry,
    /// or a scanned PDF page. A stub line marks its place in the text.
    /// </summary>
    public const string NeedsOcr = "content.needsOcr";

    /// <summary>
    /// The document holds content this extractor does not read: PPTX charts, SmartArt or embedded objects, XLSX cell
    /// comments or charts. Their text is not in the result and was not checked, so <see cref="ModelInput.IsPartial"/>
    /// is set. <c>Source</c> names the slide or sheet.
    /// </summary>
    public const string NotExtracted = "content.notExtracted";

    /// <summary>
    /// Text came from OCR. It is untrusted: an image can carry instructions aimed at an AI reader that no
    /// text-level check saw before OCR, so OCR text must go through the same safety checks as any other text.
    /// </summary>
    public const string OcrText = "content.ocr";

    /// <summary>The safety gate found something (a <see cref="SafetyFinding"/>); the message gives its code and severity.</summary>
    public const string SafetyFinding = "safety.finding";

    /// <summary>The safety gate withheld a part, which a stub line replaces in the text, or the whole document.</summary>
    public const string ContentWithheld = "safety.withheld";
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

    /// <summary>
    /// Non-fatal issues encountered during extraction, such as skipped archive entries. Failures after
    /// parsing started also carry warnings, notably <see cref="ExtractionWarningCodes.ContentTypeMismatch"/>.
    /// </summary>
    public IReadOnlyList<ExtractionWarning> Warnings { get; init; } = Array.Empty<ExtractionWarning>();

    /// <summary>Safety findings (see <see cref="ExtractionOptions.Safety"/>). Findings below the threshold leave <c>IsSuccess</c> unchanged.</summary>
    public IReadOnlyList<SafetyFinding> Findings { get; init; } = Array.Empty<SafetyFinding>();

    /// <summary>
    /// Text the safety gate withheld: the whole document (<see cref="ExtractionErrorKind.Quarantined"/>) or container
    /// parts replaced by stub lines. <c>null</c> when nothing was withheld.
    /// </summary>
    public QuarantinedContent? Quarantine { get; init; }

    /// <summary>Summary of the safety outcome. <see cref="SafetyVerdict.NoKnownSignals"/> does not mean the text is safe.</summary>
    public SafetyVerdict SafetyVerdict { get; init; }

    /// <summary>
    /// 0–100 heuristic risk over all <c>Findings</c> (prompt-injection and fraud signals): severity weight (Low 8,
    /// Medium 25, High 60) plus 10 for High confidence, capped at 100. For ranking and thresholds; not a probability.
    /// </summary>
    public int RiskScore => FraudSignals.RiskScore(Findings);

    /// <summary>Creates a successful extraction result.</summary>
    public static ExtractionResult Success(string text, string? extractorName = null, TimeSpan duration = default,
        IReadOnlyList<ExtractionWarning>? warnings = null)
        => new(true, text, null) { ExtractorName = extractorName, Duration = duration, Warnings = warnings ?? Array.Empty<ExtractionWarning>() };

    /// <summary>Creates a failed extraction result.</summary>
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

    /// <summary>Non-fatal issues encountered during extraction, such as skipped archive entries.</summary>
    public IReadOnlyList<ExtractionWarning> Warnings { get; init; } = Array.Empty<ExtractionWarning>();

    /// <summary>Safety findings (see <see cref="ExtractionOptions.Safety"/>). Findings below the threshold leave <c>IsSuccess</c> unchanged.</summary>
    public IReadOnlyList<SafetyFinding> Findings { get; init; } = Array.Empty<SafetyFinding>();

    /// <summary>
    /// Text the safety gate withheld: the whole document (<see cref="ExtractionErrorKind.Quarantined"/>) or container
    /// parts replaced by stub lines. <c>null</c> when nothing was withheld.
    /// </summary>
    public QuarantinedContent? Quarantine { get; init; }

    /// <summary>Summary of the safety outcome. <see cref="SafetyVerdict.NoKnownSignals"/> does not mean the text is safe.</summary>
    public SafetyVerdict SafetyVerdict { get; init; }

    /// <summary>
    /// 0–100 heuristic risk over all <c>Findings</c> (prompt-injection and fraud signals): severity weight (Low 8,
    /// Medium 25, High 60) plus 10 for High confidence, capped at 100. For ranking and thresholds; not a probability.
    /// </summary>
    public int RiskScore => FraudSignals.RiskScore(Findings);

    /// <summary>Creates a successful metadata extraction result.</summary>
    public static ExtractionMetadataResult Success(string text, Dictionary<string, string> metadata,
        string? extractorName = null, TimeSpan duration = default,
        IReadOnlyList<ExtractionWarning>? warnings = null)
        => new(true, text, metadata, null) { ExtractorName = extractorName, Duration = duration, Warnings = warnings ?? Array.Empty<ExtractionWarning>() };

    /// <summary>Creates a failed metadata extraction result.</summary>
    public static ExtractionMetadataResult Failure(string error,
        ExtractionErrorKind kind = ExtractionErrorKind.ExtractionFailed,
        string? extractorName = null,
        TimeSpan duration = default)
        => new(false, string.Empty, new Dictionary<string, string>(), error)
            { ExtractorName = extractorName, Duration = duration, ErrorKind = kind };
}
