using MimeKit;
using System.Text;

namespace Transformations.Text;

/// <summary>
/// Extracts text and metadata from EML files, optionally parsing attachments.
/// The safety gate checks the message (headers and body) and each attachment on its own: a part that reaches
/// <see cref="SafetyPolicy.QuarantineAt"/> is replaced by a <c>withheld: reason</c> line and the rest is kept.
/// </summary>
public class EmlExtractor : ITextExtractor, IPartGatedContainer
{
    private static readonly HashSet<string> PlainTextExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".txt", ".text", ".log", ".out", ".md", ".markdown", ".csv", ".json", ".xml", ".yaml", ".yml",
        ".cs", ".py", ".js", ".ts", ".cpp", ".h", ".java", ".ps1", ".sh", ".go", ".rs", ".sql",
        ".rb", ".php", ".swift", ".kt", ".scala", ".fs", ".vb", ".lua", ".r", ".m", ".dart"
    };

    private readonly IReadOnlyList<ITextExtractor>? _innerExtractors;

    /// <summary>Basic constructor — extracts body text only.</summary>
    public EmlExtractor() { }

    /// <summary>
    /// Pipeline-aware constructor. When provided, the extractor will also extract text
    /// from document attachments (PDF, DOCX, etc.) by routing them through the supplied
    /// inner pipeline. Pass a snapshot that excludes EmlExtractor itself to prevent
    /// circular references.
    /// </summary>
    public EmlExtractor(IEnumerable<ITextExtractor> innerExtractors)
    {
        _innerExtractors = innerExtractors.ToList().AsReadOnly();
    }

    /// <inheritdoc />
    public bool CanHandle(string extension) =>
        extension.Equals(".eml", StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc />
    public string ExtractText(byte[] fileData) => ExtractText(fileData, null);

    /// <inheritdoc />
    public string ExtractText(byte[] fileData, ExtractionOptions? options)
    {
        using var stream = new MemoryStream(fileData);
        var message = MimeMessage.Load(stream);

        using var bodyScope = new SafetyScope(options, "message body");
        var messageText = TextNormalizer.Normalize(ExtractMessageText(message, bodyScope.Options));
        var sb = new StringBuilder();
        sb.Append(bodyScope.Complete(messageText, "message body", scanText: true) is { } withheld
            ? $"[Message body] withheld: {withheld}"
            : messageText);
        AppendHtmlAlternative(message, messageText, sb, options);

        if (_innerExtractors != null)
            AppendAttachments(message, sb, _innerExtractors, options);

        return sb.ToString().Trim();
    }

    private static string ExtractMessageText(MimeMessage message, ExtractionOptions options)
    {
        var sb = new StringBuilder();

        string formattedFrom = FormatSenders(message);
        if (!string.IsNullOrWhiteSpace(formattedFrom))
            sb.AppendLine($"From: {formattedFrom}");

        var rawDate = message.Headers["Date"];
        if (!string.IsNullOrWhiteSpace(rawDate))
            sb.AppendLine($"Date: {rawDate}");
        else if (message.Date != default)
            sb.AppendLine($"Date: {message.Date}");

        if (!string.IsNullOrWhiteSpace(message.Subject))
            sb.AppendLine($"Subject: {message.Subject}");

        if (sb.Length > 0)
            sb.AppendLine();

        // A plain-text part that is only whitespace is a placeholder; the HTML part then carries the message.
        string bodyText = string.Empty;
        if (!string.IsNullOrWhiteSpace(message.TextBody))
        {
            bodyText = message.TextBody;
        }
        else if (!string.IsNullOrEmpty(message.HtmlBody))
        {
            var htmlExtractor = new HtmlExtractor();
            bodyText = htmlExtractor.ExtractFromHtml(message.HtmlBody, options);
        }

        sb.Append(bodyText);
        return sb.ToString();
    }

    /// <summary>
    /// A multipart/alternative message whose plain-text body was used also has an HTML body, which a mail client may
    /// show instead, and the two can differ. The HTML body is extracted (with the HTML hidden-content checks) and gated
    /// as its own part, <c>message body (HTML)</c>, whether or not it differs. It is added to the text as an
    /// <c>[HTML alternative]</c> block only when it has wording the plain-text body lacks, so ordinary mail is not doubled.
    /// </summary>
    private static void AppendHtmlAlternative(MimeMessage message, string plainText, StringBuilder sb, ExtractionOptions? options)
    {
        if (string.IsNullOrWhiteSpace(message.TextBody) || string.IsNullOrWhiteSpace(message.HtmlBody)) return;

        using var scope = new SafetyScope(options, "message body (HTML)");
        var htmlText = TextNormalizer.Normalize(new HtmlExtractor().ExtractFromHtml(message.HtmlBody, scope.Options));
        var withheld = scope.Complete(htmlText, "message body (HTML)", scanText: true);
        if (withheld != null)
        {
            sb.AppendLine().AppendLine().Append($"[HTML alternative] withheld: {withheld}");
            return;
        }
        if (!AddsWording(htmlText, plainText)) return;
        Warn(options, ExtractionWarningCodes.AlternativesDiffer, "message body (HTML)",
            "The HTML and plain-text bodies of this message differ; both are in the text.");
        sb.AppendLine().AppendLine().AppendLine("[HTML alternative]").Append(htmlText);
    }

    /// <summary>True when <paramref name="candidate"/> has words (3+ letters) that <paramref name="reference"/> does not.</summary>
    private static bool AddsWording(string candidate, string reference)
    {
        static HashSet<string> Words(string text) => System.Text.RegularExpressions.Regex
            .Matches(text.ToLowerInvariant(), @"[\p{L}\p{N}]{3,}").Select(m => m.Value).ToHashSet(StringComparer.Ordinal);
        var known = Words(reference);
        return Words(candidate).Any(w => !known.Contains(w));
    }

    /// <summary>The usual extension for a media type (text/plain → .txt), so an unnamed part reaches the right extractor.</summary>
    private static string ExtensionFor(ContentType type)
        => MimeTypes.TryGetExtension(type.MimeType, out var ext) ? ext : $".{type.MediaSubtype.ToLowerInvariant()}";

    private static void AppendAttachments(
        MimeMessage message,
        StringBuilder sb,
        IReadOnlyList<ITextExtractor> innerExtractors,
        ExtractionOptions? options)
    {
        long maxBytes = options?.MaxDecompressedBytes ?? 512L * 1024 * 1024;
        int maxEntries = options?.MaxContainerEntries ?? 10_000;
        // With ExtractionOptions.ShareContainerBudget the counters belong to the whole extraction, so a container
        // nested in another one continues the same budget instead of starting a fresh one.
        var budget = options?.SharedBudget;
        long localBytes = 0;
        int localEntries = 0;
        ref long totalDecodedBytes = ref (budget != null ? ref budget.Used : ref localBytes);
        ref int processedEntries = ref (budget != null ? ref budget.Entries : ref localEntries);

        // Attachments, plus inline images (cid: pictures in HTML mail), which MimeKit does not count as attachments
        // but which can carry text a reader sees, such as an image-only phishing mail.
        var inlineImages = message.BodyParts.OfType<MimePart>()
            .Where(p => !p.IsAttachment && p.ContentType.MediaType.Equals("image", StringComparison.OrdinalIgnoreCase));
        int inlineIndex = 0, unnamedIndex = 0;
        foreach (var entity in message.Attachments.Concat(inlineImages))
        {
            ExtractionGuard.ThrowIfCancellationRequested(options);
            if (entity is not MimePart part || part.Content == null) continue;

            // A part without a file name is still content: it gets a generated name (its type as the extension) and is
            // extracted or stubbed like any other. Names from the message are gated and cleaned before use.
            var fileName = part.FileName ?? string.Empty;
            if (string.IsNullOrWhiteSpace(fileName))
                fileName = part.IsAttachment
                    ? $"unnamed attachment {++unnamedIndex}{ExtensionFor(part.ContentType)}"
                    : $"inline image {++inlineIndex}.{part.ContentType.MediaSubtype.ToLowerInvariant()}";
            else
                fileName = UntrustedPart.Name(fileName, keepPathSeparators: false, options, "attachment");

            if (processedEntries >= maxEntries)
            {
                Warn(options, ExtractionWarningCodes.ContainerEntryLimit, fileName, $"Stopped after {processedEntries} EML attachments.");
                break;
            }

            processedEntries++;

            var ext = Path.GetExtension(fileName);
            var extractor = ResolveProcessor(innerExtractors, ext);
            if (extractor is ImageExtractor && !ImageExtractor.OcrAvailable(options))
                extractor = null; // recorded as a stub that needs OCR
            if (extractor == null)
            {
                byte[]? stubBytes = null;
                try
                {
                    stubBytes = DecodeContentWithLimit(part, maxBytes, ref totalDecodedBytes);
                }
                catch (Exception ex) when (ex is not OperationCanceledException) { }
                sb.AppendLine();
                PartStub.Append(sb, $"[Attachment: {fileName}]", fileName, ext, stubBytes, stubBytes?.Length ?? 0,
                    "EML attachment", options);
                if (BudgetSpent(options, fileName, totalDecodedBytes, maxBytes)) break;
                continue;
            }

            try
            {
                byte[] attachmentBytes = DecodeContentWithLimit(part, maxBytes, ref totalDecodedBytes);
                ExtractPart(sb, fileName, ext, attachmentBytes, extractor, innerExtractors, options);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                if (BudgetSpent(options, fileName, totalDecodedBytes, maxBytes)) break;
                var code = ex is InvalidDataException or ExtractionLimitExceededException
                    ? ExtractionWarningCodes.ContainerByteLimit
                    : ExtractionWarningCodes.ContainerEntryFailed;
                Warn(options, code, fileName, ex.Message);
            }
        }
    }

    /// <summary>
    /// True once the message's decoding budget is used up, counting attachments that failed part-way. Later
    /// attachments are then not decoded at all, so over-limit attachments cannot each spend the budget again.
    /// </summary>
    private static bool BudgetSpent(ExtractionOptions? options, string fileName, long total, long maxBytes)
    {
        if (total < maxBytes) return false;
        Warn(options, ExtractionWarningCodes.ContainerByteLimit, fileName,
            $"Stopped: EML attachments decoded past {maxBytes} bytes (ExtractionOptions.MaxDecompressedBytes).");
        return true;
    }

    /// <summary>
    /// Extracts one attachment and appends it, gated on its own: a withheld attachment becomes a stub line. Shared with
    /// <see cref="MsgExtractor"/>; failures are warnings, so one bad attachment does not fail the message.
    /// </summary>
    internal static void AppendPart(StringBuilder sb, string fileName, byte[] bytes, IReadOnlyList<ITextExtractor> innerExtractors,
        ExtractionOptions? options, string containerKind)
    {
        fileName = UntrustedPart.Name(fileName, keepPathSeparators: false, options, "attachment");
        var ext = Path.GetExtension(fileName);
        var extractor = ResolveProcessor(innerExtractors, ext);
        if (extractor is ImageExtractor && !ImageExtractor.OcrAvailable(options))
            extractor = null; // recorded as a stub that needs OCR
        if (extractor == null)
        {
            sb.AppendLine();
            PartStub.Append(sb, $"[Attachment: {fileName}]", fileName, ext, bytes, bytes.Length, containerKind, options);
            return;
        }

        try
        {
            ExtractPart(sb, fileName, ext, bytes, extractor, innerExtractors, options);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            Warn(options, ex is ExtractionLimitExceededException ? ExtractionWarningCodes.ContainerByteLimit : ExtractionWarningCodes.ContainerEntryFailed,
                fileName, ex.Message);
        }
    }

    private static void ExtractPart(StringBuilder sb, string fileName, string ext, byte[] bytes, ITextExtractor extractor,
        IReadOnlyList<ITextExtractor> innerExtractors, ExtractionOptions? options)
    {
        // A disguised attachment (invoice.pdf that is really an Office file) is a classic malware carrier. Report it,
        // and parse it as what it is (never as a container it did not announce); if that is not possible, its content
        // was not inspected and the part says so.
        var detected = ContentTypeDetector.DetectMismatch(ext, bytes);
        if (detected != null)
            Warn(options, ExtractionWarningCodes.ContentTypeMismatch, fileName,
                $"Attachment extension '{ext}' does not match its content, which is '{detected}'.");
        extractor = UntrustedPart.PreferDetected(extractor, detected, innerExtractors, options)!;

        using var scope = new SafetyScope(options, fileName);
        var text = TextNormalizer.Normalize(extractor.ExtractText(bytes, scope.Options), preserveSpacing: extractor is CodeExtractor);
        UntrustedPart.ReportUnresolvedMismatch(scope.Options, fileName, detected, extractor);
        if (scope.Complete(text, fileName, scanText: extractor is not IPartGatedContainer) is { } withheld)
        {
            sb.AppendLine();
            sb.AppendLine($"[Attachment: {fileName}] withheld: {withheld}");
        }
        else if (!string.IsNullOrWhiteSpace(text))
        {
            sb.AppendLine();
            sb.AppendLine(extractor is ImageExtractor ? $"[Attachment: {fileName}] (OCR)" : $"[Attachment: {fileName}]");
            sb.AppendLine(text);
        }
    }

    private static ITextExtractor? ResolveProcessor(IReadOnlyList<ITextExtractor> innerExtractors, string extension)
        => innerExtractors.FirstOrDefault(e => e.CanHandle(extension) && e is not TxtExtractor)
            ?? (PlainTextExtensions.Contains(extension)
                ? innerExtractors.FirstOrDefault(e => e is TxtExtractor)
                : null);

    /// <summary>
    /// Decodes an attachment against the message-wide budget. Every byte the decoder produced is added to
    /// <paramref name="total"/>, including the write that crossed the limit, whether or not decoding succeeds.
    /// </summary>
    private static byte[] DecodeContentWithLimit(MimePart part, long maxBytes, ref long total)
    {
        if (part.Content == null)
            return [];

        using var stream = new LimitedMemoryStream(maxBytes - total);
        try
        {
            part.Content.DecodeTo(stream);
            return stream.ToArray();
        }
        finally
        {
            total += stream.Attempted;
        }
    }

    private static string FormatSenders(MimeMessage message)
    {
        if (message.From == null || message.From.Count == 0) return string.Empty;

        var mailboxes = message.From.Mailboxes.ToList();
        if (mailboxes.Count == 0) return string.Empty;

        var formatted = string.Join(", ", mailboxes.Select(m =>
        {
            var name = m.Name?.Trim();
            if (!string.IsNullOrEmpty(name) && name.Length >= 2 && name[0] == '"' && name[^1] == '"')
                name = name[1..^1];
            return string.IsNullOrWhiteSpace(name) ? m.Address : $"{name} <{m.Address}>";
        }));

        var quoteVariants = new[] { '“', '”', '"', '‘', '’', '«', '»' };
        foreach (var q in quoteVariants)
            formatted = formatted.Replace(q.ToString(), string.Empty);

        return formatted;
    }

    private static void Warn(ExtractionOptions? options, string code, string source, string message)
        => options?.WarningSink?.Invoke(new ExtractionWarning(code, source, message));

    private sealed class LimitedMemoryStream : MemoryStream
    {
        private readonly long _maxBytes;

        public LimitedMemoryStream(long maxBytes)
        {
            _maxBytes = maxBytes;
        }

        /// <summary>Bytes the decoder tried to write, including a write that was refused for crossing the limit.</summary>
        public long Attempted { get; private set; }

        public override void Write(byte[] buffer, int offset, int count)
        {
            Attempted += count;
            if (Length + count > _maxBytes)
                throw new InvalidDataException("EML attachment exceeded the configured decompression byte limit.");

            base.Write(buffer, offset, count);
        }

        // MemoryStream routes span writes on a derived type back through Write(byte[], int, int); doing the work only
        // there avoids the two overrides calling each other.
        public override void Write(ReadOnlySpan<byte> buffer) => Write(buffer.ToArray(), 0, buffer.Length);
    }
}
