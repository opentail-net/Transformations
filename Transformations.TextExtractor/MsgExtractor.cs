using MsgReader.Outlook;
using System.Text;

namespace Transformations.Text;

/// <summary>
/// Extracts email body and metadata (From, Date, Subject) from Outlook MSG files and, when built with an inner
/// pipeline, its attachments (documents, images through OCR, inline pictures). Like <see cref="EmlExtractor"/>, the
/// message body and each attachment go through the safety gate on their own: a part that reaches
/// <see cref="SafetyPolicy.QuarantineAt"/> is replaced by a <c>withheld: reason</c> line and the rest is kept.
/// Attached Outlook messages are reported, not opened.
/// </summary>
public class MsgExtractor : ITextExtractor, IPartGatedContainer
{
    private readonly IReadOnlyList<ITextExtractor>? _innerExtractors;

    /// <summary>Basic constructor — extracts the message only, not its attachments.</summary>
    public MsgExtractor() { }

    /// <summary>
    /// Pipeline-aware constructor: attachments are routed through <paramref name="innerExtractors"/>, which must not
    /// contain this instance.
    /// </summary>
    public MsgExtractor(IEnumerable<ITextExtractor> innerExtractors)
    {
        _innerExtractors = innerExtractors.ToList().AsReadOnly();
    }

    /// <inheritdoc />
    public bool CanHandle(string extension) =>
        extension.Equals(".msg", StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc />
    public string ExtractText(byte[] fileData) => ExtractText(fileData, null);

    /// <inheritdoc />
    /// <remarks>MSGReader parses in calls that cannot be interrupted; cancellation is checked between them.</remarks>
    public string ExtractText(byte[] fileData, ExtractionOptions? options)
    {
        ExtractionGuard.ThrowIfCancellationRequested(options);
        using var stream = new MemoryStream(fileData);
        using var msg = new Storage.Message(stream);

        using var bodyScope = new SafetyScope(options, "message body");
        var messageText = TextNormalizer.Normalize(ExtractMessageText(msg, bodyScope.Options));
        var sb = new StringBuilder();
        sb.Append(bodyScope.Complete(messageText, "message body", scanText: true) is { } withheld
            ? $"[Message body] withheld: {withheld}"
            : messageText);

        if (_innerExtractors != null)
            AppendAttachments(msg, sb, _innerExtractors, options);

        return sb.ToString().Trim();
    }

    private static string ExtractMessageText(Storage.Message msg, ExtractionOptions options)
    {
        var sb = new StringBuilder();

        var sender = msg.Sender;
        if (sender != null)
        {
            var from = string.IsNullOrWhiteSpace(sender.DisplayName)
                ? sender.Email
                : $"{sender.DisplayName} <{sender.Email}>";
            if (!string.IsNullOrWhiteSpace(from))
                sb.AppendLine($"From: {from}");
        }

        if (msg.SentOn.HasValue)
            sb.AppendLine($"Date: {msg.SentOn.Value:R}");

        if (!string.IsNullOrWhiteSpace(msg.Subject))
            sb.AppendLine($"Subject: {msg.Subject}");

        if (sb.Length > 0)
            sb.AppendLine();

        ExtractionGuard.ThrowIfCancellationRequested(options);
        var body = msg.BodyText;
        if (string.IsNullOrWhiteSpace(body) && !string.IsNullOrWhiteSpace(msg.BodyHtml))
        {
            ExtractionGuard.ThrowIfCancellationRequested(options);
            body = new HtmlExtractor().ExtractFromHtml(msg.BodyHtml, options);
        }

        if (!string.IsNullOrWhiteSpace(body))
            sb.Append(body);

        return sb.ToString();
    }

    private static void AppendAttachments(Storage.Message msg, StringBuilder sb, IReadOnlyList<ITextExtractor> innerExtractors,
        ExtractionOptions? options)
    {
        long maxBytes = options?.MaxDecompressedBytes ?? ExtractionGuard.DefaultMaxDecompressedBytes;
        int maxEntries = options?.MaxContainerEntries ?? ExtractionGuard.DefaultMaxContainerEntries;
        var budget = options?.SharedBudget;
        long localBytes = 0;
        int localEntries = 0;
        ref long totalBytes = ref (budget != null ? ref budget.Used : ref localBytes);
        ref int processedEntries = ref (budget != null ? ref budget.Entries : ref localEntries);

        int index = 0;
        foreach (var item in msg.Attachments)
        {
            ExtractionGuard.ThrowIfCancellationRequested(options);
            index++;
            if (processedEntries >= maxEntries)
            {
                Warn(options, ExtractionWarningCodes.ContainerEntryLimit, $"attachment {index}", $"Stopped after {processedEntries} MSG attachments.");
                break;
            }
            processedEntries++;

            if (item is Storage.Message embedded)
            {
                // The subject is message content: gated and cleaned before it becomes a label.
                var name = string.IsNullOrWhiteSpace(embedded.Subject)
                    ? $"attachment {index}.msg"
                    : UntrustedPart.Name($"{embedded.Subject}.msg", keepPathSeparators: false, options, "attached message");
                sb.AppendLine();
                sb.AppendLine($"[Attachment: {name}] not extracted: attached Outlook message");
                Warn(options, ExtractionWarningCodes.ContainerUnsupportedEntry, name, "Attached Outlook messages are not opened.");
                continue;
            }
            if (item is not Storage.Attachment attachment || attachment.Data is not { } data) continue;

            var fileName = string.IsNullOrWhiteSpace(attachment.FileName) ? $"attachment {index}" : attachment.FileName;
            // MSGReader has already loaded the bytes; they still count against the message's budget.
            totalBytes += data.Length;
            if (totalBytes > maxBytes)
            {
                Warn(options, ExtractionWarningCodes.ContainerByteLimit, fileName,
                    $"Stopped: MSG attachments exceed {maxBytes} bytes (ExtractionOptions.MaxDecompressedBytes).");
                break;
            }

            EmlExtractor.AppendPart(sb, fileName, data, innerExtractors, options, "MSG attachment");
        }
    }

    private static void Warn(ExtractionOptions? options, string code, string source, string message)
        => options?.WarningSink?.Invoke(new ExtractionWarning(code, source, message));
}
