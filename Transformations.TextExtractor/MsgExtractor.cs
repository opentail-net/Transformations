using MsgReader.Outlook;
using System.Text;

namespace Transformations.Text;

/// <summary>
/// Extracts email body and metadata (From, Date, Subject) from Outlook MSG files.
/// </summary>
public class MsgExtractor : ITextExtractor
{
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
            var htmlExtractor = new HtmlExtractor();
            body = htmlExtractor.ExtractFromHtml(msg.BodyHtml, options);
        }

        if (!string.IsNullOrWhiteSpace(body))
            sb.Append(body);

        return sb.ToString().Trim();
    }
}
