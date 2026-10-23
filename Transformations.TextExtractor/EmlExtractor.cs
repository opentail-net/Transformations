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

        using var bodyScope = new SafetyScope(options);
        var messageText = TextNormalizer.Normalize(ExtractMessageText(message, bodyScope.Options));
        var sb = new StringBuilder();
        sb.Append(bodyScope.Complete(messageText, "message body", scanText: true) is { } withheld
            ? $"[Message body] withheld: {withheld}"
            : messageText);

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

    private static void AppendAttachments(
        MimeMessage message,
        StringBuilder sb,
        IReadOnlyList<ITextExtractor> innerExtractors,
        ExtractionOptions? options)
    {
        long totalDecodedBytes = 0;
        long maxBytes = options?.MaxDecompressedBytes ?? 512L * 1024 * 1024;
        int maxEntries = options?.MaxContainerEntries ?? 10_000;
        int processedEntries = 0;

        foreach (var entity in message.Attachments)
        {
            ExtractionGuard.ThrowIfCancellationRequested(options);
            if (entity is not MimePart part || part.Content == null) continue;

            var fileName = part.FileName ?? string.Empty;
            if (string.IsNullOrWhiteSpace(fileName)) continue;

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
                    stubBytes = DecodeContentWithLimit(part, maxBytes - totalDecodedBytes, out long stubDecoded);
                    totalDecodedBytes += stubDecoded;
                }
                catch (Exception ex) when (ex is not OperationCanceledException) { }
                sb.AppendLine();
                PartStub.Append(sb, $"[Attachment: {fileName}]", fileName, ext, stubBytes, stubBytes?.Length ?? 0,
                    "EML attachment", options);
                continue;
            }

            try
            {
                byte[] attachmentBytes = DecodeContentWithLimit(part, maxBytes - totalDecodedBytes, out long decodedBytes);
                totalDecodedBytes += decodedBytes;

                // A disguised attachment (invoice.pdf that is really an Office file) is a classic
                // malware carrier. Report it; it is still parsed only as the format its name claims.
                if (ContentTypeDetector.DetectMismatch(ext, attachmentBytes) is { } detected)
                    Warn(options, ExtractionWarningCodes.ContentTypeMismatch, fileName,
                        $"Attachment extension '{ext}' does not match its content, which is '{detected}'.");

                using var scope = new SafetyScope(options);
                var attachText = TextNormalizer.Normalize(extractor.ExtractText(attachmentBytes, scope.Options));
                if (scope.Complete(attachText, fileName, scanText: extractor is not IPartGatedContainer) is { } withheld)
                {
                    sb.AppendLine();
                    sb.AppendLine($"[Attachment: {fileName}] withheld: {withheld}");
                }
                else if (!string.IsNullOrWhiteSpace(attachText))
                {
                    sb.AppendLine();
                    sb.AppendLine(extractor is ImageExtractor ? $"[Attachment: {fileName}] (OCR)" : $"[Attachment: {fileName}]");
                    sb.AppendLine(attachText);
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                var code = ex is InvalidDataException or ExtractionLimitExceededException
                    ? ExtractionWarningCodes.ContainerByteLimit
                    : ExtractionWarningCodes.ContainerEntryFailed;
                Warn(options, code, fileName, ex.Message);
            }
        }
    }

    private static ITextExtractor? ResolveProcessor(IReadOnlyList<ITextExtractor> innerExtractors, string extension)
        => innerExtractors.FirstOrDefault(e => e.CanHandle(extension) && e is not TxtExtractor)
            ?? (PlainTextExtensions.Contains(extension)
                ? innerExtractors.FirstOrDefault(e => e is TxtExtractor)
                : null);

    private static byte[] DecodeContentWithLimit(MimePart part, long remainingBytes, out long decodedBytes)
    {
        if (part.Content == null)
        {
            decodedBytes = 0;
            return [];
        }

        using var stream = new LimitedMemoryStream(remainingBytes);
        part.Content.DecodeTo(stream);
        decodedBytes = stream.Length;
        return stream.ToArray();
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

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (Length + count > _maxBytes)
                throw new InvalidDataException("EML attachment exceeded the configured decompression byte limit.");

            base.Write(buffer, offset, count);
        }

        public override void Write(ReadOnlySpan<byte> buffer)
        {
            if (Length + buffer.Length > _maxBytes)
                throw new InvalidDataException("EML attachment exceeded the configured decompression byte limit.");

            base.Write(buffer);
        }
    }
}
