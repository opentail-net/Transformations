using MimeKit;
using System.Text;

namespace Transformations.Text;

public class EmlExtractor : ITextExtractor
{
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

    public bool CanHandle(string extension) =>
        extension.Equals(".eml", StringComparison.OrdinalIgnoreCase);

    public string ExtractText(byte[] fileData)
    {
        using var stream = new MemoryStream(fileData);
        var message = MimeMessage.Load(stream);
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

        string bodyText = string.Empty;
        if (!string.IsNullOrEmpty(message.TextBody))
        {
            bodyText = message.TextBody;
        }
        else if (!string.IsNullOrEmpty(message.HtmlBody))
        {
            var htmlExtractor = new HtmlExtractor();
            bodyText = htmlExtractor.ExtractText(Encoding.UTF8.GetBytes(message.HtmlBody));
        }

        sb.Append(bodyText);

        if (_innerExtractors != null)
            AppendAttachments(message, sb, _innerExtractors);

        return sb.ToString().Trim();
    }

    private static void AppendAttachments(
        MimeMessage message,
        StringBuilder sb,
        IReadOnlyList<ITextExtractor> innerExtractors)
    {
        foreach (var entity in message.Attachments)
        {
            if (entity is not MimePart part || part.Content == null) continue;

            var fileName = part.FileName ?? string.Empty;
            if (string.IsNullOrWhiteSpace(fileName)) continue;

            var ext = Path.GetExtension(fileName);
            var extractor = innerExtractors.FirstOrDefault(e => e.CanHandle(ext) && e is not TxtExtractor);
            if (extractor == null) continue;

            try
            {
                using var ms = new MemoryStream();
                part.Content.DecodeTo(ms);
                var attachText = extractor.ExtractText(ms.ToArray());
                if (!string.IsNullOrWhiteSpace(attachText))
                {
                    sb.AppendLine();
                    sb.AppendLine($"[Attachment: {fileName}]");
                    sb.AppendLine(attachText);
                }
            }
            catch
            {
                // Skip attachments that cannot be extracted
            }
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
}
